using UnityEngine;

public class Bomb : Singleton<Bomb>
{
    [SerializeField] private Animator anim;

    private bool isBooming;

    public void BombEnd()
    {
        isBooming = false;
    }

    public bool Boom()
    {
        if (isBooming)
            return false;

        isBooming = true;
        transform.position = Player.Instance.transform.position;
        ScreenFXManager.Instance.ScreenFlash();
        anim.SetTrigger("Boom");
        AudioManager.Instance.Play(AudioType.BOMB);
        return true;
    }
}
