using System.Collections.Generic;
using UnityEngine;
using static ObjectPool;
using static Path;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float accelSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float frictionSpeed;
    [Space(10)]
    [SerializeField] private float startingHp;
    private float currHp;
    private Rigidbody2D rb;

    private List<PathPoint> path;
    private Vector3 pathOffset = Vector3.zero;
    private bool offsetLastPos;
    private int pathPosIndex;
    private Vector3 target;
    private bool isMoving;
    private float nextMoveTime;

    private float moveSpeedOverride;
    private float bulletSpeedOverride;

    [SerializeField] private BulletType bulletType = BulletType.enemy_circle;
    [SerializeField] private int numBullets;
    [SerializeField] private float reloadTime;
    private float nextShootTime;
    [SerializeField] private float bulletSpread;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private GameObject pickupPrefab;


    private EnemyType indexID;
    public void SetIndex(EnemyType _index)
    {
        indexID = _index;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currHp = startingHp;
    }

    private void Start()
    {
        nextShootTime = Time.time + reloadTime;
    }

    public void SetPath(Path _newPath, Vector3 _offset)
    {
        currHp = startingHp;

        path = _newPath.points;
        transform.position = _newPath.transform.position + _offset;
        pathOffset = _offset;

        offsetLastPos = _newPath.OffsetLastPos;
        moveSpeedOverride = maxSpeed + _newPath.MoveSpeedAdditiveOverride;
        bulletSpeedOverride = bulletSpeed + _newPath.BulletSpeedAdditiveOverride;

        pathPosIndex = -1;

        nextShootTime = Time.time + reloadTime / 2f;
        isMoving = true;
        GetNextPathPoint();
    }

    private void GetNextPathPoint()
    {
        if (pathPosIndex >= 0 && path[pathPosIndex].timeToWait > 0)
        {
            isMoving = false;
            nextMoveTime = Time.time + path[pathPosIndex].timeToWait;
        }

        pathPosIndex++;
        if (pathPosIndex < path.Count)
        {
            if (pathPosIndex == path.Count - 1 && !offsetLastPos) //don't change location of last point
                target = path[pathPosIndex].point.position;
            else
                target = path[pathPosIndex].point.position + pathOffset;
        }
        else
            ReturnToPool();
    }

    private void Update()
    {
        //Rapid Fire
        if (Time.time >= nextShootTime)
        {
            nextShootTime = Time.time + reloadTime;

            Vector3 toPlayer = (Player.Instance.transform.position - transform.position).normalized;
            float dirTowardsPlayer = Mathf.Rad2Deg * Mathf.Atan2(toPlayer.y, toPlayer.x);
            float randAngle = Random.Range(-Bullet.RandSpread, Bullet.RandSpread);
            dirTowardsPlayer += randAngle;

            //Spawn bullets
            float maxSpread = (numBullets - 1) * bulletSpread / 2;

            for (float deg = dirTowardsPlayer - maxSpread; deg <= dirTowardsPlayer + maxSpread; deg += bulletSpread)
            {
                Bullet bullet = ObjectPool.Instance.GetBulletOfType(bulletType);
                bullet.transform.position = transform.position;

                if (bulletSpeedOverride > 0)
                    bullet.SetDirection(deg, bulletSpeedOverride);
                else
                    bullet.SetDirection(deg, bulletSpeed);

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
            else
            {
                currInput.Normalize();
            }
        }

        float maxSpeedToUse;
        if (moveSpeedOverride > 0)
            maxSpeedToUse = moveSpeedOverride;
        else
            maxSpeedToUse = maxSpeed;

        //Apply ground fricion
        Vector3 velocity_local_friction = rb.velocity.normalized * Mathf.Max(0, rb.velocity.magnitude - frictionSpeed);

        Vector3 updatedVelocity = velocity_local_friction;

        if (currInput.magnitude > 0.05f) //Pressing something, try to accelerate
        {
            Vector3 velocity_friction_and_input = velocity_local_friction + currInput * accelSpeed;

            if (velocity_local_friction.magnitude <= maxSpeedToUse)
            {
                //under max speed, accelerate towards max speed
                updatedVelocity = velocity_friction_and_input.normalized * Mathf.Min(velocity_friction_and_input.magnitude, maxSpeedToUse);
            }
            else //Over max speed
            {
                if (velocity_friction_and_input.magnitude <= maxSpeedToUse) //Input below max speed
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
        if (LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.PlayerBulletLayer))
        {
            //Hit by bullet, take damage
            OnHit(collision.GetComponent<Bullet>().Damage);
            if (currHp <= 0)
                OnDeath();
        }
        else if (LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.KickLayer))
        {
            //Hit by bullet, take damage
            OnHit(Player.Instance.KickDamage);
            if (currHp <= 0)
                OnDeath();
        }
        else if (LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.BombLayer))
        {
            //Hit by bomb, take damage
            currHp -= Player.Instance.BombDamage;
            if (currHp <= 0)
                OnDeath();
        }
    }

    private void OnDeath()
    {
        AudioManager.Instance.Play(AudioType.ENEMY_DEATH);
        UIController.Instance.GainScore(100);

        // TODO replace w/ pools
        Pickup pickup = ObjectPool.Instance.GetFromPool<Pickup>();
        pickup.RollType();
        pickup.transform.position = transform.position;

        pickup.gameObject.SetActive(true);

        ReturnToPool();
    }

    private void OnHit(float damage)
    {
        currHp -= damage;
        UIController.Instance.GainScore(10);
        AudioManager.Instance.Play(AudioType.PLAYER_HIT);
    }

    private void ReturnToPool()
    {
        ObjectPool.Instance.AddToEnemyPool(this, indexID);
    }
}
