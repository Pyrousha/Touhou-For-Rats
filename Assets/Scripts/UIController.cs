using System.Collections.Generic;
using UnityEngine;

public class UIController : Singleton<UIController>
{
    [SerializeField] private Transform livesParent;
    [SerializeField] private Transform bombsParent;
    [SerializeField] private int numLives;
    [SerializeField] private int numBombs;
    private List<Transform> livesIcons;
    private List<Transform> bombsIcons;

    private int startingBombs;

    [SerializeField] private float deathBombTime;
    private bool dying = false;
    private float dieTime;


    private void Start()
    {
        livesIcons = Utils.GetChildrenFromParent(livesParent);
        SetLivesVisuals();

        bombsIcons = Utils.GetChildrenFromParent(bombsParent);
        SetBombsVisuals();

        startingBombs = numBombs;
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
        SetLivesVisuals();
    }

    public void LoseLife()
    {
        if (dying)
            return;

        dying = true;
        dieTime = Time.time + deathBombTime;

        if (numLives > 0)
            AudioManager.Instance.Play(AudioType.SQUEAK);
        else
            AudioManager.Instance.Play(AudioType.DEATH);
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

    private void Update()
    {
        if (InputHandler.Instance.Cancel_Bomb.Down && numBombs > 0)
        {
            dying = false;
            UseBomb();
        }
        else if (dying && Time.time >= dieTime)
        {
            //die
            numLives--;

            Player.Instance.transform.position = Lantern.Instance.transform.position + new Vector3(0, -1, 0);

            SetLivesVisuals();
            dying = false;
            numBombs = Mathf.Max(startingBombs, numBombs);
            SetBombsVisuals();

            if (numLives >= 0)
                Bomb.Instance.Boom();
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
}
