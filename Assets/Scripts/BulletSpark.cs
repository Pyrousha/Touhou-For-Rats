using UnityEngine;

public class BulletSpark : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void RestartAnim()
    {
        anim.SetTrigger("Restart");
        transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
    }

    public void OnAnimEnd()
    {
        ObjectPool.Instance.AddToPool(this);
    }
}
