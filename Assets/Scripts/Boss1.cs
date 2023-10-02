using System.Collections.Generic;
using UnityEngine;
using static ObjectPool;

public class Boss1 : MonoBehaviour
{
    [SerializeField] protected float accelSpeed;
    [SerializeField] protected float maxSpeed;
    [SerializeField] protected float frictionSpeed;
    private float dmgTaken;
    protected Rigidbody2D rb;

    private Vector3 target;
    private bool isMoving;
    private float nextMoveTime;

    public List<BulletPattern> patterns = new List<BulletPattern>();


    [System.Serializable]
    public struct SpellCard
    {
        [field: SerializeField] public List<PosAndAttack> PosAndAttacks { get; private set; }
        [field: SerializeField] public float Hp { get; private set; }
    }

    [System.Serializable]
    public struct PosAndAttack
    {
        [field: SerializeField] public Transform Pos { get; private set; }
        [field: SerializeField] public float TimeToWaitBeforeMovingNext { get; private set; }
        [field: SerializeField] public int BulletPatternIndex { get; private set; }
    }

    [System.Serializable]
    public struct BulletPattern
    {
        [field: SerializeField] public BulletType bulletType { get; private set; }
        [field: SerializeField] public int numBullets { get; private set; }
        [field: SerializeField] public int numWaves { get; private set; }
        [field: SerializeField] public float bulletSpread { get; private set; }
        [field: SerializeField] public float bulletSpeed { get; private set; }
        [field: SerializeField] public float bulletSpeedRand { get; private set; }
        [field: SerializeField] public float spawnInterval { get; private set; }
        [field: SerializeField] public bool aimed { get; private set; }
    }


    [SerializeField] public List<SpellCard> SpellCards;
    private int cardIndex = -1;
    private List<PosAndAttack> currPoses;
    private SpellCard currSpellCard;
    private BulletPattern? currBulletPattern;

    private bool firing;

    private int currPosIndex = -1;
    private float nextShootTime;

    private bool dead = false;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        StartNextSpellcard();
    }

    public void StartNextSpellcard()
    {
        cardIndex++;
        if (cardIndex < SpellCards.Count)
        {
            //valid next card
            currSpellCard = SpellCards[cardIndex];
            dmgTaken = 0;

            SetPath(currSpellCard.PosAndAttacks);
        }
        else
            OnDeath();
    }

    private void SetPath(List<PosAndAttack> posAndAttacks)
    {
        currPoses = posAndAttacks;
        currPosIndex = -1;

        isMoving = true;
        GetNextPathPoint();
    }

    private void GetNextPathPoint()
    {
        int startPos = currPosIndex;

        currPosIndex = (currPosIndex + 1) % currSpellCard.PosAndAttacks.Count;

        if (startPos > 0 && currPoses[startPos].TimeToWaitBeforeMovingNext > 0)
        {
            isMoving = false;
            nextMoveTime = Time.time + currPoses[startPos].TimeToWaitBeforeMovingNext;
        }

        if (currPoses[currPosIndex].BulletPatternIndex >= 0)
            currBulletPattern = patterns[currPoses[currPosIndex].BulletPatternIndex];
        else
            currBulletPattern = null;
        firing = (currBulletPattern != null);
        bulletsSpawned = 0;

        target = currPoses[currPosIndex].Pos.position;
    }

    private int bulletsSpawned = 0;

    private void Update()
    {
        if (dead) return;

        //Rapid Fire
        while (firing && Time.time >= nextShootTime)
        {
            nextShootTime = Time.time + ((BulletPattern)currBulletPattern).spawnInterval;

            bulletsSpawned++;
            if (bulletsSpawned >= ((BulletPattern)currBulletPattern).numWaves)
                firing = false;


            float dirTowardsPlayer;


            if (((BulletPattern)currBulletPattern).aimed)
            {
                Vector3 dir = (Player.Instance.transform.position - transform.position).normalized;
                dirTowardsPlayer = Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x) + Random.Range(-Bullet.RandSpread, Bullet.RandSpread);
            }
            else
            {
                dirTowardsPlayer = Random.Range(0, 360f);
            }



            //Spawn bullets
            float maxSpread = (((BulletPattern)currBulletPattern).numBullets - 1) * ((BulletPattern)currBulletPattern).bulletSpread / 2;

            for (float deg = dirTowardsPlayer - maxSpread; deg <= dirTowardsPlayer + maxSpread; deg += ((BulletPattern)currBulletPattern).bulletSpread)
            {
                Bullet bullet = ObjectPool.Instance.GetBulletOfType(((BulletPattern)currBulletPattern).bulletType);
                bullet.transform.position = transform.position;

                bullet.SetDirection(deg, ((BulletPattern)currBulletPattern).bulletSpeed + Random.Range(-((BulletPattern)currBulletPattern).bulletSpeedRand, ((BulletPattern)currBulletPattern).bulletSpeedRand));

                bullet.gameObject.SetActive(true);
            }
        }

        if (!isMoving && Time.time >= nextMoveTime)
        {
            isMoving = true;
        }
    }

    private void FixedUpdate()
    {
        //XZ Friction + acceleration
        Vector3 currInput;
        if (!isMoving)
            currInput = Vector2.zero;
        else
        {
            currInput = (target - transform.position);
            if (currInput.magnitude <= 1)
            {
                //reached target
                GetNextPathPoint();
            }

            currInput.Normalize();
        }

        //Apply ground fricion
        Vector3 velocity_local_friction = rb.velocity.normalized * Mathf.Max(0, rb.velocity.magnitude - frictionSpeed);

        Vector3 updatedVelocity = velocity_local_friction;

        if (currInput.magnitude > 0.05f) //Pressing something, try to accelerate
        {
            Vector3 velocity_friction_and_input = velocity_local_friction + currInput * accelSpeed;

            if (velocity_local_friction.magnitude <= maxSpeed)
            {
                //under max speed, accelerate towards max speed
                updatedVelocity = velocity_friction_and_input.normalized * Mathf.Min(velocity_friction_and_input.magnitude, maxSpeed);
            }
            else //Over max speed
            {
                if (velocity_friction_and_input.magnitude <= maxSpeed) //Input below max speed
                    updatedVelocity = velocity_friction_and_input;
                else
                {
                    //Can't use input directly, since would be over max speed

                    //Would accelerate more, so don't user player input
                    if (velocity_friction_and_input.magnitude > velocity_local_friction.magnitude)
                        updatedVelocity = velocity_local_friction;
                    else
                        //Would accelerate less, user player input (since input moves velocity more to 0,0 than just friciton)
                        updatedVelocity = velocity_friction_and_input;
                }
            }
        }

        //Apply velocity
        rb.velocity = updatedVelocity;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!enabled) return;

        if (LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.PlayerBulletLayer))
        {
            //Hit by bullet, take damage
            OnHit(collision.GetComponent<Bullet>().Damage);
        }
        else if (LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.KickLayer))
        {
            //Hit by bullet, take damage
            OnHit(Player.Instance.KickDamage);
        }
        else if (LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.BombLayer))
        {
            //Hit by bomb, take damage
            OnHit(Player.Instance.BombDamage);
        }
    }

    private void OnDeath()
    {
        if (dead)
            return;

        GetComponent<DialogueActivator>().TryInteract();

        AudioManager.Instance.Play(AudioType.ENEMY_DEATH);
        UIController.Instance.GainScore(50000);

        rb.velocity = Vector2.zero;
        dead = true;
    }

    private void OnHit(float damage)
    {
        dmgTaken += damage;

        if (dmgTaken > currSpellCard.Hp)
        {
            StartNextSpellcard();
        }

        UIController.Instance.GainScore(10);
        AudioManager.Instance.Play(AudioType.PLAYER_HIT);
    }
}
