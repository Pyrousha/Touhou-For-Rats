using UnityEngine;

public class LanternPlayerTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.PlayerHurtboxLayer))
        {
            Lantern.Instance.OnPlayerEnterLantern();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.PlayerHurtboxLayer))
        {
            Lantern.Instance.OnPlayerExitLantern();
        }
    }
}
