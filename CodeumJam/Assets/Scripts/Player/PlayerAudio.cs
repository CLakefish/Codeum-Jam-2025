using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerAudioClipType
{
    Walk,
    Roll,
    Jump,
    Land,
    Bounce,
    Explode
}

public class PlayerAudio : Player.PlayerComponent
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource looping;
    [SerializeField] private AudioClip walk, roll, jump, land, bounce, explode;

    private void Awake()
    {
        looping.loop = true;

        looping.volume     = 1.25f;
        audioSource.volume = 1.25f;
    }

    public void PlaySound(PlayerAudioClipType type, float pitch = 1)
    {
        switch (type)
        { 
            case PlayerAudioClipType.Walk:
                if (looping.isPlaying && looping.clip == walk) return;
                looping.clip = walk;
                looping.pitch = pitch;
                looping.Play();
                break;

            case PlayerAudioClipType.Roll:
                if (looping.isPlaying && looping.clip == roll) return;
                looping.clip = roll;
                looping.pitch = pitch;
                looping.Play();
                break;

            case PlayerAudioClipType.Jump:
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(jump);
                break;

            case PlayerAudioClipType.Land:
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(land);
                break;

            case PlayerAudioClipType.Bounce:
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(bounce);
                break;

            case PlayerAudioClipType.Explode:
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(explode);
                break;
        }
    }

    public void StopLoop()
    {
        looping.Stop();
        looping.clip = null;
    }

    public void StopAudio()
    {
        StopLoop();
        audioSource.Stop();
        audioSource.clip = null;
    }
}
