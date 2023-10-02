using UnityEngine;

public class DialogueActivator : MonoBehaviour, IInteractable
{
    [Header("Special Case stuff (can leave blank)")]
    [SerializeField] private bool playOnStart;

    [Header("Objects + Options")]
    [SerializeField] private DialogueObject dialogueObject;
    [SerializeField] private bool playWithoutInput;
    [SerializeField] private PlayOptions playOption;
    [field: SerializeField] public int Priority { get; set; }

    public Transform Transform { get; set; }

    public enum PlayOptions
    {
        playOnce,
        playAgainWithInput
    }

    private bool played = false;
    private bool playOnce = false;

    private void Awake()
    {
        Transform = transform;
    }

    private void Start()
    {
        if (playOnStart)
            TryInteract();
    }

    public void UpdateDialogueObject(DialogueObject dialogueObject)
    {
        this.dialogueObject = dialogueObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out HeroDialogueInteract player))
        {
            player.AddInteractable(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out HeroDialogueInteract player))
        {
            player.RemoveInteractable(this);
        }
    }

    public void TryInteract()
    {
        if (played && playOnce)
            return;

        DialogueUI.Instance.ShowDialogue(dialogueObject, gameObject);
    }

    public void TryInteract(HeroDialogueInteract player)
    {
        if (played && playOnce)
            return;

        if (playWithoutInput || InputHandler.Instance.Interact_Shoot.Down)
        {
            played = true;
            DialogueUI.Instance.ShowDialogue(dialogueObject, gameObject);

            switch (playOption)
            {
                case PlayOptions.playOnce:
                    {
                        playOnce = true;
                        break;
                    }

                case PlayOptions.playAgainWithInput:
                    {
                        playWithoutInput = false;
                        break;
                    }
            }
        }
    }

    public void SetPlayWithoutInput(bool playWithNoInput)
    {
        playWithoutInput = playWithNoInput;
    }
}
