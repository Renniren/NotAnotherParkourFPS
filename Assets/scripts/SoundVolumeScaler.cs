using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class SoundVolumeScaler : MonoBehaviour
{
	public AudioSource src;
	public float NormalVolume = 1;

	private void Start()
	{
		src.volume = NormalVolume * Game.Volume;
	}

	void Update()
	{
		src.volume = NormalVolume * Game.Volume;
	}
}
