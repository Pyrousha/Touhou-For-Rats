using UnityEngine;

public class GrazeHurtbox : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        //TODO: Hit Graze
        if (LayerManager.IsInLayer(collision.gameObject.layer, LayerManager.Instance.Grazable))
        {
            Player.Instance.ShowGrazeEffect();
            AudioManager.Instance.Play(AudioType.GRAZE);
            UIController.Instance.GainScore(250);
        }
    }
}
