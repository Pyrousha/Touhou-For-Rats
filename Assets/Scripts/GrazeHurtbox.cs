using UnityEngine;

public class GrazeHurtbox : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        //TODO: Hit Graze
        Debug.Log("Graze!");
        UIController.Instance.GainScore(25);
    }
}
