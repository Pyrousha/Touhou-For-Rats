using UnityEngine.SceneManagement;

public class SceneTransitioner : Singleton<SceneTransitioner>
{
    public bool InGameScene { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene(1);
    }

    public void PlayGame()
    {
        InGameScene = true;
        UIController.Instance.OnStartNewGame();
        SceneManager.LoadScene(2);
    }
}
