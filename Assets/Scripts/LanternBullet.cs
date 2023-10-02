using UnityEngine;

public class LanternBullet : Bullet
{
    private float currXAccel;
    [SerializeField] private float yAccel;
    [SerializeField] private float xAccel;
    [SerializeField] private float moveWaitTime;
    private float nextMoveTime;
    private bool canMove;

    public void SetDirection(float _degrees, float _speed, float _randSpread)
    {
        float randSpeed = Random.Range(-_randSpread, _randSpread);

        currXAccel = (-randSpeed / _randSpread) * 0.5f * xAccel + xAccel;

        float rads = Mathf.Deg2Rad * _degrees;

        velocity = new Vector3(Mathf.Cos(rads), Mathf.Sin(rads)) * (_speed + randSpeed);

        canMove = false;
        nextMoveTime = Time.time + moveWaitTime;
    }

    private new void Update()
    {
        Vector3 newVelocity = velocity;
        newVelocity.y += yAccel * Time.deltaTime;

        if (canMove)
        {
            float toEnemyX = ObjectPool.Instance.GetClosestEnemy(velocity + transform.position).x;
            float inputX = Mathf.Clamp(toEnemyX, -currXAccel, currXAccel);
            newVelocity.x += inputX;
        }
        else
        {
            if (Time.time >= nextMoveTime)
            {
                canMove = true;
            }
        }

        velocity = newVelocity;

        transform.position += velocity * Time.deltaTime;
    }
}
