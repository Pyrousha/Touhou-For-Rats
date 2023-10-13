using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI continuesLabel;
    [SerializeField] private TextMeshProUGUI continuesText;
    [SerializeField] private TextMeshProUGUI deathsText;
    [SerializeField] private TextMeshProUGUI bombsText;
    [SerializeField] private GameObject noccTip;

    [SerializeField] private Color noContinueColor;

    private void Start()
    {
        scoreText.text = UIController.Instance.Score.ToString();
        continuesText.text = UIController.Instance.Continues.ToString();
        deathsText.text = UIController.Instance.Deaths.ToString();
        bombsText.text = UIController.Instance.BombsUsed.ToString();

        if (UIController.Instance.Continues == 0)
        {
            continuesLabel.color = noContinueColor;
            continuesText.color = noContinueColor;
        }
        else
            noccTip.SetActive(true);
    }

    public void BackToMenu()
    {
        UIController.Instance.OnGameQuit();
    }
}
