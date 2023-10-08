using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHpController : Singleton<BossHpController>
{
    [SerializeField] private Sprite brokenHeart;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Slider bossSlider;

    private Animator anim;

    private int currLives;

    private List<Image> heartImages;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetHp(float _hpPercent)
    {
        bossSlider.value = Mathf.Max(0, _hpPercent);
    }

    public void LoseLife()
    {
        currLives--;
        heartImages[currLives].sprite = brokenHeart;

        if (currLives == 0)
            anim.SetTrigger("FadeOut");
    }

    public void StartFight(int _numLives)
    {
        currLives = _numLives;

        heartImages = new List<Image>();
        for (int i = 0; i < currLives; i++)
        {
            heartImages.Add(Instantiate(heartPrefab, transform).GetComponent<Image>());
        }
        bossSlider.transform.SetAsLastSibling();

        bossSlider.value = 1;

        anim.SetTrigger("FadeIn");
    }
}
