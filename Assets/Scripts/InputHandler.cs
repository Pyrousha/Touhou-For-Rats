using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : Singleton<InputHandler>
{
    private enum ButtonIndices
    {
        Interact_Confirm,
        Cancel,
        Menu,
        Focus,
        SkipText
    }

    public Vector2 MoveXZ
    {
        get;
        private set;
    }
    public ButtonState Interact_Shoot => buttons[(int)ButtonIndices.Interact_Confirm];
    public ButtonState Cancel_Bomb => buttons[(int)ButtonIndices.Cancel];
    public ButtonState Menu => buttons[(int)ButtonIndices.Menu];
    public ButtonState Focus => buttons[(int)ButtonIndices.Focus];
    public ButtonState SkipText => buttons[(int)ButtonIndices.SkipText];

    //TODO: Refactor dialogue code to use Interactable/Navigation instead of this
    public AnalogToDigitalButtonState Up;
    public AnalogToDigitalButtonState Down;
    public AnalogToDigitalButtonState Left;
    public AnalogToDigitalButtonState Right;

    private int buttonCount = -1; //Size of ButtonIndices enum
    [SerializeField] private short bufferFrames = 5;
    [SerializeField] private bool bufferEnabled = false;
    private short IDSRC = 0;
    private ButtonState[] buttons;
    private Queue<Dictionary<short, short>> inputBuffer = new Queue<Dictionary<short, short>>();
    private Dictionary<short, short> currentFrame;

    public void Start()
    {
        buttonCount = System.Enum.GetValues(typeof(ButtonIndices)).Length;

        buttons = new ButtonState[buttonCount];
        for (int i = 0; i < buttonCount; i++)
            buttons[i].Init(ref IDSRC, this);
    }

    private void Update()
    {
        Up.SetIsHolding(MoveXZ.y);
        Down.SetIsHolding(-MoveXZ.y);
        Left.SetIsHolding(-MoveXZ.x);
        Right.SetIsHolding(MoveXZ.x);
    }

    private void LateUpdate()
    {
        Up.Reset();
        Down.Reset();
        Left.Reset();
        Right.Reset();
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < buttonCount; i++)
            buttons[i].Reset();

        if (bufferEnabled)
        {
            UpdateBuffer();
        }
    }

    //Input functions
    public void CTX_MoveXZ(InputAction.CallbackContext _ctx)
    {
        MoveXZ = _ctx.ReadValue<Vector2>();
    }

    public void CTX_Interact_Or_Confirm(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.Interact_Confirm].Set(_ctx);
    }
    public void CTX_Cancel(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.Cancel].Set(_ctx);
    }
    public void CTX_Menu(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.Menu].Set(_ctx);
    }
    public void CTX_Focus(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.Focus].Set(_ctx);
    }
    public void CTX_SkipText(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.SkipText].Set(_ctx);
    }


    //Buffer functions
    public void FlushBuffer()
    {
        inputBuffer.Clear();
    }

    public void UpdateBuffer()
    {
        if (inputBuffer.Count >= bufferFrames)
            inputBuffer.Dequeue();
        currentFrame = new Dictionary<short, short>();
        inputBuffer.Enqueue(currentFrame);
    }

    public void PrintBuffer()
    {
        string bufferData = $"InputBuffer: count-{inputBuffer.Count}";
        foreach (var frame in inputBuffer)
            if (frame.Count > 0)
                bufferData += $"\n{frame.Count}";
        Debug.Log(bufferData);
    }

    public struct AnalogToDigitalButtonState
    {
        private const float cutoff = 0.75f;

        private bool firstFrame;
        public bool Holding { get; private set; }

        public readonly bool Down => (Holding && firstFrame);
        public readonly bool Up => (!Holding && firstFrame);

        public void SetIsHolding(float _value)
        {
            bool _holding = (_value >= cutoff);

            if (Holding == _holding)
            {
                //Same state
                firstFrame = false;
                return;
            }

            //New state
            Holding = _holding;
            firstFrame = true;
        }

        public void Reset()
        {
            firstFrame = false;
        }
    }

    public struct ButtonState
    {
        private short id;
        private static short STATE_PRESSED = 0,
                                STATE_RELEASED = 1;
        private InputHandler handler;
        private bool firstFrame;
        public bool Holding
        {
            get;
            private set;
        }
        public readonly bool Down
        {
            get
            {
                if (handler.bufferEnabled && handler.inputBuffer != null)
                {
                    foreach (var frame in handler.inputBuffer)
                    {
                        if (frame.ContainsKey(id) && frame[id] == STATE_PRESSED)
                        {
                            return frame.Remove(id);
                        }
                    }
                    return false;
                }
                return Holding && firstFrame;
            }
        }

        public readonly bool Up
        {
            get
            {
                if (handler.bufferEnabled && handler.inputBuffer != null)
                {
                    foreach (var frame in handler.inputBuffer)
                    {
                        if (frame.ContainsKey(id) && frame[id] == STATE_RELEASED)
                        {
                            return frame.Remove(id);
                        }
                    }
                    return false;
                }
                return !Holding && firstFrame;
            }
        }

        public void Set(InputAction.CallbackContext ctx)
        {
            Holding = !ctx.canceled;
            firstFrame = true;

            if (handler.bufferEnabled && handler.currentFrame != null)
            {
                handler.currentFrame.TryAdd(id, Holding ? STATE_PRESSED : STATE_RELEASED);
            }
        }

        public void Reset()
        {
            firstFrame = false;
        }

        public void Init(ref short IDSRC, InputHandler handler)
        {
            id = IDSRC++;
            this.handler = handler;
        }
    }
}