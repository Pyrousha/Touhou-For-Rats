using UnityEngine;
using UnityEngine.UI;

public class MainMenu : Singleton<MainMenu>
{
    private bool isAnimOver = false;

    [SerializeField] private Selectable playButton;

    private void Update()
    {
        if (InputHandler.Instance.Interact_Shoot.Down && !isAnimOver)
        {
            GetComponent<Animator>().SetTrigger("Skip");
            OnAnimEnded();
        }
    }

    public void OnAnimEnded()
    {
        isAnimOver = true;
        playButton.Select();
    }
}
