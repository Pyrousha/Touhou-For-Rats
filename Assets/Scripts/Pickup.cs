using UnityEngine;

public class Pickup : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Player.Instance.PickupNewBullet();
        Destroy(gameObject);
    }
}
