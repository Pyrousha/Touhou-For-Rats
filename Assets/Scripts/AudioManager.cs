using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource source;
    [SerializeField] private List<AudioClip> clips;

    public void Play(AudioType type) {
        source.PlayOneShot(clips[(int)type]);
    }
}

public enum AudioType {
    SQUEAK, HIT, DEATH, ENEMY_DEATH, PLAYER_HIT, PLAYER_SHOOT, KICK_START, KICK_READY, LANTERN_HIT, BOMB, PICKUP
}