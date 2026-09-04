using System;
using UnityEngine;

namespace Invector;

[RequireComponent(typeof(AudioSource))]
public class vPlayRandomClip : MonoBehaviour
{
	public AudioClip[] clips;

	public AudioSource audioSource;

	public bool playOnStart = true;

	private void Start()
	{
		if (!audioSource)
		{
			audioSource = GetComponent<AudioSource>();
		}
		UnityEngine.Random.InitState(UnityEngine.Random.Range(0, DateTime.Now.Millisecond));
		if (playOnStart)
		{
			Play();
		}
	}

	public void Play()
	{
		if ((bool)audioSource)
		{
			int num = 0;
			num = UnityEngine.Random.Range(0, clips.Length - 1);
			if (clips.Length != 0)
			{
				audioSource.PlayOneShot(clips[num]);
			}
		}
	}
}
