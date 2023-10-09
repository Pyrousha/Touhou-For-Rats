using UnityEngine;

public class MainMenu : Singleton<MainMenu>
{
    public bool IsAnimOver { get; private set; } = false;

    public void OnAnimEnded()
    {
        IsAnimOver = true;
    }

    public void SkipIntro()
    {
        GetComponent<Animator>().SetTrigger("Skip");
        IsAnimOver = true;
    }
}
