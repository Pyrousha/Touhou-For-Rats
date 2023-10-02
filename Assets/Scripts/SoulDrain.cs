using System.Collections.Generic;
using UnityEngine;

public class SoulDrain : Singleton<SoulDrain>
{
    private ParticleSystem particle;

    private List<Transform> children;
    private bool isOnRight;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        children = Utils.GetChildrenFromParent(transform);
    }

    private void Update()
    {
        if (transform.position.x < Lantern.Instance.transform.position.x)
        {
            if (isOnRight)
            {
                //Was on right, now on left
                isOnRight = false;
                foreach (Transform child in children)
                {
                    child.transform.localScale = new Vector3(-1, 1, 1);
                }
            }
        }
        else if (transform.position.x > Lantern.Instance.transform.position.x)
        {
            if (!isOnRight)
            {
                //Was on left, now on right
                isOnRight = true;
                foreach (Transform child in children)
                {
                    child.transform.localScale = Vector3.one;
                }
            }
        }
    }

    public void StartParticle()
    {
        particle.Play();
    }

    public void StopParticle()
    {
        particle.Stop();
    }
}
