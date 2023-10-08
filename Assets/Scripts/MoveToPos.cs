using BeauRoutine;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MoveToPos : MonoBehaviour
{
    [SerializeField] private float timeToWaitToMove;

    [SerializeField] private Transform targ;
    [SerializeField] private float speed;

    [SerializeField] private UnityEvent eventWhenArrive;

    private bool moving = false;

    public void StartCountdownRoutine()
    {
        Routine.Start(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        yield return timeToWaitToMove;
        StartMoving();
    }

    public void StartMoving()
    {
        moving = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!moving)
            return;

        Vector3 toTarg = targ.position - transform.position;

        if (toTarg.magnitude <= speed * Time.deltaTime)
        {
            transform.position = targ.position;
            moving = false;
            eventWhenArrive.Invoke();
        }
        else
        {
            transform.position += toTarg * speed * Time.deltaTime;
        }
    }
}
