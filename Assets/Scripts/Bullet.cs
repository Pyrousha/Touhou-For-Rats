using UnityEngine;
using static ObjectPool;

public class Bullet : MonoBehaviour
{
    public static readonly float RandSpread = 2.5f;

    [SerializeField] private bool isPlayerBullet;
    [field: SerializeField] public float Damage { get; private set; }

    protected Vector3 velocity;

    public BulletType BulletType { get; private set; }
    public void SetType(BulletType _type)
    {
        BulletType = _type;
    }


    public void SetDirection(float _degrees, float _speed)
    {
        transform.localEulerAngles = new Vector3(0, 0, _degrees - 90);

        float rads = Mathf.Deg2Rad * _degrees;

        velocity = new Vector3(Mathf.Cos(rads), Mathf.Sin(rads)) * _speed;
    }

    public void MoreForwardBy(float _offset)
    {
        transform.position += velocity.normalized * _offset;
    }

    protected void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPlayerBullet && LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.EnemyLayer))
        {
            BulletSpark bulletSpark = ObjectPool.Instance.GetFromPool<BulletSpark>();
            bulletSpark.RestartAnim();
            bulletSpark.transform.position = Utils.RoundToNearest(collision.ClosestPoint(transform.position), 1f / 16f);

            bulletSpark.gameObject.SetActive(true);
        }

        ObjectPool.Instance.AddToBulletPool(this);
    }
}
