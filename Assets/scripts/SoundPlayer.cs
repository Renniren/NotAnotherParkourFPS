using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class SoundPlayer : MonoBehaviorExtended
{
    public AudioClip Sound;
    public bool BypassLevelActiveTime;
    public bool PlayOnEnable = true;
    public bool RespectsVolume = true;
    public bool TwoDee;
    public float Volume = 1;
    public float MinDistance = 25, MaxDistance = 90;

    public void Play()
    {
        PlaySound(Sound, transform.position, TwoDee, Volume, MaxDistance, MinDistance, RespectsVolume);
    }

    private void OnEnable()
    {
        if (Game.LevelTime > 0.75f)
        {
            Play();
        }
        else if (Game.LevelTime < 0.75f)
        {
            if (BypassLevelActiveTime)
            {
                Play();
            }
        }
    }
}
