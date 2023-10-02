using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFXManager : Singleton<ScreenFXManager>
{
    [SerializeField] private Animator animator;
    public void ScreenFlash() {
        animator.SetTrigger("ScreenFlash");
    }
}
