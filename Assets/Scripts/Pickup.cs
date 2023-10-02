using System;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private List<Sprite> sprites;

    [Serializable]
    public enum PickupType
    {
        power,
        score, bomb, life
    }

    void FixedUpdate() {
        Vector3 pos = transform.position;

        pos.y -= 0.05f;
        transform.position = pos;

        if (transform.position.y < -20)
            Destroy(gameObject);
    }

    public void RollType() {
        this.type = UIController.Instance.RollPickupType();

        sprite.sprite = sprites[(int)type];
    }

    [SerializeField] private PickupType type;

    private void OnTriggerEnter2D(Collider2D other) {
        UIController.Instance.GotPickup(type);
        Destroy(gameObject);
    }
}
