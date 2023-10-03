using UnityEngine;

public class PlayerHurtbox : Singleton<PlayerHurtbox>
{
    private bool invincible = false;
    private float nextDamagableTime;

    public void GiveIFrames()
    {
        invincible = true;
        nextDamagableTime = Time.time + 0.25f;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.LanternLayer))
        {
            if (!invincible || (invincible && Time.time >= nextDamagableTime))
            {
                invincible = false;
                UIController.Instance.LoseLife();
            }
        }
    }
}
