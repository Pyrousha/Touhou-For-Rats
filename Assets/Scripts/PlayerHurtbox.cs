using UnityEngine;

public class PlayerHurtbox : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.LanternLayer))
        {
            UIController.Instance.LoseLife();
        }
    }
}
