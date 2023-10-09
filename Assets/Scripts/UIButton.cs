using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIButton : Button, ISelectHandler, IDeselectHandler
{
    [Space(25)]
    [Header("UIButton Stuff")]

    protected List<Graphic> graphics = new List<Graphic>();
    protected Button button;
    [SerializeField] protected ColorBlock normalColors;

    [SerializeField] protected ColorBlock disabledColors;

    public bool IsFullyInteractable { get; private set; }

    public Selectable C_Selectable { get; private set; }

    protected override void Awake()
    {
        graphics = new List<Graphic>(GetComponentsInChildren<Graphic>());
        button = GetComponent<Button>();

        C_Selectable = GetComponent<Selectable>();
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        var targetColor =
            state == SelectionState.Disabled ? colors.disabledColor :
            state == SelectionState.Highlighted ? colors.highlightedColor :
            state == SelectionState.Normal ? colors.normalColor :
            state == SelectionState.Pressed ? colors.pressedColor :
            state == SelectionState.Selected ? colors.selectedColor : Color.white;

        foreach (Graphic graphic in graphics)
        {
            graphic.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
        }
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        if (!button.interactable)
        {
            ColorBlock cb = button.colors;
            cb.disabledColor = normalColors.disabledColor;
            button.colors = cb;
        }

        base.OnDeselect(eventData);
    }

    public void SetButtonInteractable(bool _newInteractable, bool _useDisabledColorsEvenIfEnabled = false)
    {
        button.interactable = _newInteractable;

        if (_newInteractable == false || _useDisabledColorsEvenIfEnabled)
        {
            button.colors = disabledColors;

            IsFullyInteractable = false;
        }
        else
        {
            button.colors = normalColors;

            IsFullyInteractable = true;
        }
    }
}