using System;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Serializable]
    public enum PickupType
    {
        power,
        score, bomb, life
    }

    [SerializeField] private PickupType type;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UIController.Instance.GotPickup(type);
    }
}
