using System;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    [Serializable]
    public enum ButtonType
    {
        Play,
        Options,
        Quit
    }

    [SerializeField] private ButtonType buttonType;

    public void OnClick()
    {
        switch (buttonType)
        {
            case ButtonType.Play:
                SceneTransitioner.Instance.PlayGame();
                break;
            case ButtonType.Options:
                break;
            case ButtonType.Quit:
                Application.Quit();
                break;
        }
    }
}
