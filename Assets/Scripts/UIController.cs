using System.Collections.Generic;
using UnityEngine;

public class UIController : Singleton<UIController>
{
    [SerializeField] private Transform livesParent;
    [SerializeField] private int numLives;
    private List<Transform> livesIcons;

    private void Start()
    {
        livesIcons = Utils.GetChildrenFromParent(livesParent);
        SetLivesVisuals();
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
        numLives--;
        if (numLives >= 0)
            AudioManager.Instance.Play(AudioType.SQUEAK);
        else
            AudioManager.Instance.Play(AudioType.DEATH);
        SetLivesVisuals();
    }
}
