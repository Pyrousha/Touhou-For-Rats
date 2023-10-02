using UnityEngine;

public class ScrollingObj : MonoBehaviour
{
    [SerializeField] private float speed;

    private float startingYPos;

    private void Start()
    {
        startingYPos = transform.position.y;
    }

    private void Update()
    {
        float newY = transform.position.y - speed * Time.deltaTime;
        if (newY < startingYPos - 2)
        {
            newY++;
        }
        else if (newY > startingYPos + 2)
        { newY--; }

        Vector3 pos = transform.position;
        pos.y = newY;

        transform.position = pos;
    }
}
