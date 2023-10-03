using UnityEngine;

public class EnemyBullet : Bullet
{
    public new void OnTriggerEnter2D(Collider2D collision)
    {
        //Hit Something other than graze, delete
        if (!LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.GrazeLayer))
            ObjectPool.Instance.AddToBulletPool(this);
    }
}
