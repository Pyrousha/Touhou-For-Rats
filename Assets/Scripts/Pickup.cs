using System;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private List<Sprite> sprites;

    private static float startingSpeed = 0.05f;
    private static float accelSpeed = -0.0075f;
    private static float maxSpeed = -0.075f;

    private float currSpeed;

    [Serializable]
    public enum PickupType
    {
        power,
        score, bomb, life
    }

    void FixedUpdate()
    {
        currSpeed = MathF.Max(maxSpeed, currSpeed + accelSpeed);

        Vector3 pos = transform.position;

        pos.y += currSpeed;
        transform.position = pos;

        if (transform.position.y < -20)
            ObjectPool.Instance.AddToPool(this);
    }

    public void RollType()
    {
        this.type = UIController.Instance.RollPickupType();

        sprite.sprite = sprites[(int)type];
        currSpeed = startingSpeed;
    }

    [SerializeField] private PickupType type;

    private void OnTriggerEnter2D(Collider2D other)
    {
        UIController.Instance.GotPickup(type);
        ObjectPool.Instance.AddToPool(this);
    }
}
