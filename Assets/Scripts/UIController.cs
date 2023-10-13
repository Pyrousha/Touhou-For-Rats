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

    public int Score { get; private set; }
    private static int hiScore;

    private int dropCounter;
    private int scoreThreshold = 20000;

    private bool stageFinished;

    public int Continues { get; private set; }
    public int Power { get; private set; }
    public int Deaths { get; private set; }
    public int BombsUsed { get; private set; }

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
        Power++;

        if (Power % 10 == 0)
            Player.Instance.PickupNewBullet();

        powerText.text = Power.ToString();
    }

    public void OnUnpause()
    {
        paused = false;
        pauseParent.SetActive(false);

        Time.timeScale = 1;
    }

    private void Update()
    {
        if (Player.Instance == null || Player.Instance.StageOver || DialogueUI.Instance.isOpen)
            return;

        if (InputHandler.Instance.Menu.Down)
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

        if (paused)
            return;

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
            Deaths++;

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
            {
                numBombs--;
                BombsUsed++;
            }

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
        if (Score > scoreThreshold)
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
        Score += _scoreToGain;
        scoreText.text = Score.ToString();

        if (Score > hiScore)
        {
            hiScore = Score;
            hiScoreText.text = hiScore.ToString();
        }
    }

    public void OnStartNewGame()
    {
        Time.timeScale = 1;

        stageFinished = false;
        stageEndUI.SetActive(false);

        Score = 0;
        dropCounter = 0;
        Power = 0;

        Continues = 0;
        Deaths = 0;
        BombsUsed = 0;

        numLives = startingLives;
        numBombs = startingBombs;

        SetLivesVisuals();
        SetBombsVisuals();
        powerText.text = Power.ToString();

        scoreText.text = Score.ToString();
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
        stageEndScore.text = Score.ToString();
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
        stageEndScore.text = Score.ToString();

        stageFinished = true;

        PlayerPrefs.SetInt(HISCORE_KEY, hiScore);
        PlayerPrefs.Save();
    }

    public void OnContinue()
    {
        if (stageFinished)
        {
            //This code is called when the player presses the "next stage" button after finishing a level
            stageFinished = false;
            stageEndUI.SetActive(false);

            Time.timeScale = 1;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }


        Time.timeScale = 1;

        stageFinished = false;
        stageEndUI.SetActive(false);

        Continues++;
        Score = 0;
        dropCounter = 0;

        numLives = startingLives;
        numBombs = startingBombs;

        SetLivesVisuals();
        SetBombsVisuals();

        scoreText.text = Score.ToString();

        PlayerPrefs.SetInt(HISCORE_KEY, hiScore);
        PlayerPrefs.Save();

        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnRestart()
    {
        OnUnpause();

        SceneTransitioner.Instance.PlayGame();
    }

    public void OnGameQuit()
    {
        stageFinished = false;
        stageEndUI.SetActive(false);

        OnUnpause();

        SceneManager.LoadScene(1);
    }
}
