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

    public void RollType() {

    }

    [SerializeField] private PickupType type;

    private void OnTriggerEnter2D(Collision2D collision)
    {
        UIController.Instance.GotPickup(type);
    }
}
