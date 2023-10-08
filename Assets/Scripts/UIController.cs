using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : Singleton<UIController>
{
    [SerializeField] private Transform livesParent;
    [SerializeField] private Transform bombsParent;
    [SerializeField] private int numLives;
    [SerializeField] private int numBombs;

    [SerializeField] private TextMeshProUGUI hiScoreText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI powerText;

    [SerializeField] private GameObject stageEndUI;
    [SerializeField] private TextMeshProUGUI stageEndTitle;
    [SerializeField] private TextMeshProUGUI stageEndScore;
    [SerializeField] private TextMeshProUGUI stageEndHiScore;

    private List<Transform> livesIcons;
    private List<Transform> bombsIcons;

    private int startingBombs;

    [SerializeField] private float deathBombTime;
    private bool dying = false;
    private float dieTime;

    private int score;
    private static int hiScore;

    private int dropCounter;
    private int scoreThreshold = 20000;

    private int power;
    private void Start()
    {
        livesIcons = Utils.GetChildrenFromParent(livesParent);
        SetLivesVisuals();

        bombsIcons = Utils.GetChildrenFromParent(bombsParent);
        SetBombsVisuals();

        startingBombs = numBombs;

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

    private void Update()
    {
        if (Player.Instance.StageOver)
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

    public void OnGameOver()
    {
        Player.Instance.OnDie();

        Time.timeScale = 0;

        stageEndUI.SetActive(true);
        stageEndTitle.text = "You Died";
        stageEndHiScore.text = hiScore.ToString();
        stageEndScore.text = score.ToString();
    }

    public void OnGameReset()
    {
        Time.timeScale = 1;

        stageEndUI.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Player.Instance.OnReset();
    }

    public void OnStageCleared()
    {
        Time.timeScale = 0;

        stageEndUI.SetActive(true);
        stageEndTitle.text = "Stage Clear!";
        stageEndHiScore.text = hiScore.ToString();
        stageEndScore.text = score.ToString();
    }

    public void OnGameQuit()
    {
        //Application.Quit();
    }
}
