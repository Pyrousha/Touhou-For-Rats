using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : Singleton<UIController>
{
    [SerializeField] private Transform livesParent;
    [SerializeField] private Transform bombsParent;
    [SerializeField] private TextMeshProUGUI continueText;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private RectTransform buttonsParent;
    [SerializeField] private GameObject pauseParent;
    [SerializeField] private Selectable resumeButton;
    [Space(5)]
    [SerializeField] private int numLives;
    [SerializeField] private int numBombs;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI hiScoreText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI powerText;

    [Space(10)]
    [SerializeField] private GameObject stageEndUI;
    [SerializeField] private TextMeshProUGUI stageEndTitle;
    [SerializeField] private TextMeshProUGUI stageEndScore;
    [SerializeField] private TextMeshProUGUI stageEndHiScore;

    private List<Transform> livesIcons;
    private List<Transform> bombsIcons;

    private int startingLives;
    private int startingBombs;

    [SerializeField] private float deathBombTime;
    private bool dying = false;
    private float dieTime;

    private int score;
    private static int hiScore;

    private int dropCounter;
    private int scoreThreshold = 20000;

    private bool stageFinished;

    private int power;
    public int Power => power;

    private bool paused = false;

    public const string HISCORE_KEY = "HiScore";

    private void Start()
    {
        livesIcons = Utils.GetChildrenFromParent(livesParent);
        SetLivesVisuals();

        bombsIcons = Utils.GetChildrenFromParent(bombsParent);
        SetBombsVisuals();

        startingLives = numLives;
        startingBombs = numBombs;

        hiScore = PlayerPrefs.GetInt(HISCORE_KEY, 0);
        hiScoreText.text = hiScore.ToString();
    }

    private void SetLivesVisuals()
    {
        for (int i = 0; i < livesIcons.Count; i++)
        {
            livesIcons[i].gameObject.SetActive(numLives > i);
        }
    }

    public void GainLife()
    {
        numLives = Mathf.Min(numLives + 1, livesIcons.Count);
        AudioManager.Instance.Play(AudioType.LIFEUP);
        SetLivesVisuals();
    }

    public void LoseLife()
    {
        if (dying || Player.Instance.StageOver)
            return;

        dying = true;
        dieTime = Time.time + deathBombTime;

        AudioManager.Instance.Play(AudioType.SQUEAK);
        SetLivesVisuals();
    }


    private void SetBombsVisuals()
    {
        for (int i = 0; i < bombsIcons.Count; i++)
        {
            bombsIcons[i].gameObject.SetActive(numBombs > i);
        }
    }

    public void GainBomb()
    {
        numBombs = Mathf.Min(numBombs + 1, bombsIcons.Count);
        SetBombsVisuals();
    }

    public void GainPower()
    {
        power++;

        if (power % 10 == 0)
            Player.Instance.PickupNewBullet();

        powerText.text = power.ToString();
    }

    public void OnUnpause()
    {
        paused = false;
        pauseParent.SetActive(paused);

        if (paused)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    private void Update()
    {
        if (Player.Instance == null || Player.Instance.StageOver)
            return;

        if (InputHandler.Instance.Menu.Down && !DialogueUI.Instance.isOpen)
        {
            paused = !paused;
            pauseParent.SetActive(paused);

            if (paused)
            {
                Time.timeScale = 0;
                resumeButton.Select();
            }
            else
                Time.timeScale = 1;

            return;
        }

        if (InputHandler.Instance.Cancel_Bomb.Down && numBombs > 0)
        {
            dying = false;
            Lantern.Instance.OnBomb();
            PlayerHurtbox.Instance.GiveIFrames();
            UseBomb();
        }
        else if (dying && Time.time >= dieTime)
        {
            //die
            numLives--;

            PlayerHurtbox.Instance.GiveIFrames();

            if (numLives >= 0)
                Player.Instance.transform.position = Lantern.Instance.transform.position + new Vector3(0, -1, 0);

            Lantern.Instance.OnBomb();

            SetLivesVisuals();
            dying = false;
            numBombs = Mathf.Max(startingBombs, numBombs);
            SetBombsVisuals();

            Bomb.Instance.Boom();

            if (numLives < 0)
            {
                AudioManager.Instance.Play(AudioType.DEATH);
                OnGameOver();
            }
        }
    }

    public void UseBomb()
    {
        if (numBombs > 0)
        {
            if (Bomb.Instance.Boom())
                numBombs--;

            SetBombsVisuals();
        }
    }

    public void GotPickup(Pickup.PickupType type)
    {
        AudioManager.Instance.Play(AudioType.PICKUP);

        switch (type)
        {
            case Pickup.PickupType.power:
                GainPower();
                break;
            case Pickup.PickupType.score:
                GainScore(500);
                break;
            case Pickup.PickupType.bomb:
                GainBomb();
                break;
            case Pickup.PickupType.life:
                GainLife();
                break;
            default:
                break;
        }
    }

    public Pickup.PickupType RollPickupType()
    {
        if (score > scoreThreshold)
        {
            scoreThreshold += 20000;
            return Pickup.PickupType.life;
        }


        Pickup.PickupType type = Pickup.PickupType.score;

        dropCounter++;

        if (dropCounter % 20 == 0)
            type = Pickup.PickupType.bomb;
        else if (dropCounter % 2 == 0)
            type = Pickup.PickupType.power;

        return type;
    }

    public void GainScore(int _scoreToGain)
    {
        score += _scoreToGain;
        scoreText.text = score.ToString();

        if (score > hiScore)
        {
            hiScore = score;
            hiScoreText.text = hiScore.ToString();
        }
    }

    public void OnStartNewGame()
    {
        Time.timeScale = 1;

        stageFinished = false;
        stageEndUI.SetActive(false);

        score = 0;
        dropCounter = 0;
        power = 0;

        numLives = startingLives;
        numBombs = startingBombs;

        SetLivesVisuals();
        SetBombsVisuals();
        powerText.text = power.ToString();

        scoreText.text = score.ToString();
    }

    public void OnGameOver()
    {
        Player.Instance.OnDie();

        Time.timeScale = 0;

        stageEndUI.SetActive(true);
        restartButton.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsParent);
        buttonsParent.GetComponent<LinkSelectables>().Link();

        continueText.text = "Continue";
        continueText.transform.parent.GetComponent<Button>().Select();

        stageEndTitle.text = "You Died";
        stageEndHiScore.text = hiScore.ToString();
        stageEndScore.text = score.ToString();
    }

    public void OnStageCleared()
    {
        Time.timeScale = 0;

        stageEndUI.SetActive(true);
        restartButton.SetActive(false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonsParent);
        buttonsParent.GetComponent<LinkSelectables>().Link();

        continueText.text = "Next Stage";
        continueText.transform.parent.GetComponent<Button>().Select();

        stageEndTitle.text = "Stage Clear!";
        stageEndHiScore.text = hiScore.ToString();
        stageEndScore.text = score.ToString();

        stageFinished = true;

        PlayerPrefs.SetInt(HISCORE_KEY, hiScore);
        PlayerPrefs.Save();
    }

    public void OnContinue()
    {
        if (stageFinished)
        {
            stageFinished = false;
            stageEndUI.SetActive(false);

            Time.timeScale = 1;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }


        Time.timeScale = 1;

        stageFinished = false;
        stageEndUI.SetActive(false);

        score = 0;
        dropCounter = 0;

        numLives = startingLives;
        numBombs = startingBombs;

        SetLivesVisuals();
        SetBombsVisuals();

        scoreText.text = score.ToString();

        PlayerPrefs.SetInt(HISCORE_KEY, hiScore);
        PlayerPrefs.Save();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnRestart()
    {
        SceneTransitioner.Instance.PlayGame();
    }

    public void OnGameQuit()
    {
        stageFinished = false;
        stageEndUI.SetActive(false);

        Time.timeScale = 1;

        SceneManager.LoadScene(1);
    }
}
