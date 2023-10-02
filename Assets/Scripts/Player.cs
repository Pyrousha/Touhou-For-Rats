using UnityEngine;
using static ObjectPool;

public class Player : Singleton<Player>
{
    [SerializeField] private GameObject hitboxObj;
    [SerializeField] private SpriteRenderer sprRend;
    [SerializeField] private Animator KickParent;
    [SerializeField] private Transform chargeParent;
    [SerializeField] private Animator chargeAnim;
    private Animator spriteAnim;
    private bool hitboxShowing = true;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float focusSpeed;

    [SerializeField] private int numBullets;
    [SerializeField] private float bulletCircleOffsetFromPlayer;
    [SerializeField] private float bulletXOffsetFromPlayer;
    [SerializeField] private float reloadTime;
    private float nextShootTime;
    [SerializeField] private float bulletSpread_min;
    [SerializeField] private float bulletSpread_max;
    [SerializeField] private float bulletSpread_accel;
    [SerializeField] private float bulletSpeed;
    private float currBulletSpread;
    [field: SerializeField] public float KickDamage { get; private set; }
    [field: SerializeField] public float BombDamage { get; private set; }


    [SerializeField] private float chargeDuration;
    [SerializeField] private float chargeAnimDuration = 0.5f;
    [SerializeField] private float shootCooldownAfterCharge;
    private bool isCharging;
    private float chargeStartTime;

    private float nextChargeTime;
    private float nextChargeAnimTime;
    private bool startedChargeAnim;
    private bool chargeFXReady;
    private float kickDuration = 0.49f;

    private Rigidbody2D rb;
    private Vector2 facingDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteAnim = sprRend.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        bool focusing = InputHandler.Instance.Focus.Holding || InputHandler.Instance.Interact_Shoot.Holding;

        if (focusing)
        {
            //focusing, converge spread
            currBulletSpread = Mathf.Max(currBulletSpread - bulletSpread_accel * Time.deltaTime, bulletSpread_min);

            rb.velocity = InputHandler.Instance.MoveXZ * focusSpeed;
        }
        else
        {
            //Not focusing, widen spread
            currBulletSpread = Mathf.Min(currBulletSpread + bulletSpread_accel * Time.deltaTime, bulletSpread_max);

            rb.velocity = InputHandler.Instance.MoveXZ * moveSpeed;
        }

        if (rb.velocity.x < 0)
        {
            sprRend.flipX = true;
            chargeParent.localScale = new Vector3(-1, 1, 1);
        }
        else if (rb.velocity.x > 0)
        {
            sprRend.flipX = false;
            chargeParent.localScale = new Vector3(1, 1, 1);
        }


        if (rb.velocity.sqrMagnitude > 0)
            facingDir = rb.velocity.normalized;

        if (focusing != hitboxShowing)
        {
            hitboxShowing = !hitboxShowing;
            hitboxObj.SetActive(hitboxShowing);
        }

        if (DialogueUI.Instance.isOpen)
            return;

        chargeAnim.SetBool("HoldingCharge", InputHandler.Instance.Interact_Shoot.Holding);

        if (InputHandler.Instance.Interact_Shoot.Holding)
        {
            if (!isCharging && Time.time >= nextChargeTime)
            {
                isCharging = true;
                startedChargeAnim = false;
                chargeFXReady = true;

                nextChargeAnimTime = Time.time + chargeDuration - chargeAnimDuration;
                chargeStartTime = Time.time;
                spriteAnim.SetTrigger("StartCharge");
                AudioManager.Instance.Play(AudioType.KICK_START);
            }
            else if (!startedChargeAnim && Time.time >= nextChargeAnimTime)
            {
                startedChargeAnim = true;
                chargeAnim.SetTrigger("StartCharge");
            } else if (chargeFXReady && Time.time >= chargeStartTime + chargeDuration){
                chargeFXReady = false;
                AudioManager.Instance.Play(AudioType.KICK_READY);
            }
        }
        else
        {
            if (InputHandler.Instance.Interact_Shoot.Up && isCharging)
            {
                isCharging = false;

                if (Time.time >= chargeStartTime + chargeDuration)
                {
                    //Finished Charge
                    spriteAnim.SetTrigger("Kick");
                    nextShootTime = Time.time + shootCooldownAfterCharge;

                    KickParent.transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * Mathf.Atan2(facingDir.y, facingDir.x));
                    KickParent.SetTrigger("Kick");

                    nextChargeTime = Time.time + kickDuration;
                }
                else
                {
                    //Canceled charge
                    spriteAnim.SetTrigger("CancelCharge");
                }
            }
            else
            {
                //Rapid Fire
                if (Time.time >= nextShootTime)
                {
                    AudioManager.Instance.Play(AudioType.PLAYER_SHOOT);
                    nextShootTime = Time.time + reloadTime;

                    //Spawn bullets
                    if (numBullets % 2 == 1)
                    {
                        //Odd, 1 in front and then rest at sides
                        float maxSpread = (numBullets - 1) * currBulletSpread / 2;

                        for (float deg = -maxSpread; deg <= maxSpread; deg += currBulletSpread)
                        {
                            Bullet bullet = ObjectPool.Instance.GetBulletOfType(BulletType.player);
                            bullet.transform.position = transform.position;

                            bullet.SetDirection(deg + 90, bulletSpeed);
                            bullet.MoreForwardBy(bulletCircleOffsetFromPlayer);

                            bullet.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        ////even, 2 in front and then rest at sides
                        //for (int i = -1; i <= 1; i += 2)
                        //{
                        //    //2 in front
                        //    Bullet bullet = ObjectPool.Instance.GetBulletOfType(BulletType.player);
                        //    bullet.transform.position = transform.position + new Vector3(bulletXOffsetFromPlayer * i, 0, 0);

                        //    bullet.SetDirection(90, bulletSpeed);
                        //    bullet.MoreForwardBy(bulletCircleOffsetFromPlayer);

                        //    bullet.gameObject.SetActive(true);
                        //}
                        //int maxSpread = (numBullets - 2) * bulletSpread / 2;

                        float maxSpread = (numBullets - 1) * currBulletSpread / 2;
                        for (float deg = maxSpread; deg >= -maxSpread; deg -= currBulletSpread)
                        {
                            //rest at sides
                            Bullet bullet = ObjectPool.Instance.GetBulletOfType(BulletType.player);
                            bullet.transform.position = transform.position;

                            bullet.SetDirection(90 + deg, bulletSpeed);
                            bullet.MoreForwardBy(bulletCircleOffsetFromPlayer);

                            bullet.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    public void PickupNewBullet()
    {
        numBullets++;
    }
}
