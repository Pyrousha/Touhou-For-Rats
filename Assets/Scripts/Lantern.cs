using UnityEngine;
using UnityEngine.UI;
using static ObjectPool;

public class Lantern : Singleton<Lantern>
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private Slider lightSlider;

    [SerializeField] private float darkTimeTillDie;
    [SerializeField] private float timeToFullHeal;

    [SerializeField] private float kickSpeed;
    [SerializeField] private float frictionSpeed;
    [Space(10)]
    [SerializeField] private int numBullets;
    [SerializeField] private float bulletSpread;
    [SerializeField] private float bulletSpeed;

    float lightHp = 1;

    private bool isInDark;
    private bool isMoving;

    private void Update()
    {
        if (isInDark)
        {
            lightHp -= Time.deltaTime / darkTimeTillDie;

            if (lightHp <= 0)
            {
                UIController.Instance.LoseLife();
            }
        }
        else
            lightHp += Time.deltaTime / timeToFullHeal;

        lightHp = Mathf.Clamp(lightHp, 0, 1);

        lightSlider.gameObject.SetActive(lightHp != 1);
        lightSlider.value = lightHp;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.KickLayer))
        {
            //Move in direction of kick
            float angle = (collision.transform.parent.eulerAngles.z) * Mathf.Deg2Rad;
            anim.transform.eulerAngles = new Vector3(0, 0, collision.transform.parent.eulerAngles.z);
            anim.transform.position = transform.position;

            rb.velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * kickSpeed;
            isMoving = true;
            anim.SetTrigger("Hit");
            SpawnBullets();

            AudioManager.Instance.Play(AudioType.LANTERN_HIT);
        }
    }

    private void SpawnBullets()
    {
        Vector3 dir = rb.velocity.normalized;
        float dirAngle = Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x) + Random.Range(-Bullet.RandSpread, Bullet.RandSpread);

        //Spawn bullets
        float maxSpread = (numBullets - 1) * bulletSpread / 2;

        for (float deg = dirAngle - maxSpread; deg <= dirAngle + maxSpread; deg += bulletSpread)
        {
            LanternBullet bullet = (LanternBullet)ObjectPool.Instance.GetBulletOfType(BulletType.lanternFire);
            bullet.transform.position = transform.position;

            bullet.SetDirection(deg, bulletSpeed, 5);

            bullet.gameObject.SetActive(true);
        }
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            float speedMagnitude = rb.velocity.magnitude - frictionSpeed;
            rb.velocity = rb.velocity.normalized * Mathf.Max(0, speedMagnitude);

            if (speedMagnitude == 0)
                isMoving = false;
        }
    }

    public void OnPlayerEnterLantern()
    {
        isInDark = false;
        SoulDrain.Instance.StopParticle();
    }

    public void OnPlayerExitLantern()
    {
        isInDark = true;
        SoulDrain.Instance.StartParticle();
    }

    public void OnBomb()
    {
        lightHp = 1;
        lightSlider.gameObject.SetActive(false);
        lightSlider.value = lightHp;
    }
}
