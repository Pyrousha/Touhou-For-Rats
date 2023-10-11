using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource source;
    [SerializeField] private List<AudioClip> clips;

    public void Play(AudioType type)
    {
        source.PlayOneShot(clips[(int)type]);
    }
}

public enum AudioType
{
    SQUEAK, HIT, DEATH, ENEMY_DEATH, PLAYER_HIT, //0-4

    PLAYER_SHOOT, KICK_START, KICK_READY, LANTERN_HIT, BOMB, //5-9

    LIFEUP, GRAZE, POWERUP, PICKUP, BOSS_DEAD, //10-14
    BUTTON_CLICK //15-19
}
