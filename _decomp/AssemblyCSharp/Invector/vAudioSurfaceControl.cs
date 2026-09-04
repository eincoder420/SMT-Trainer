using UnityEngine;
using UnityEngine.Audio;

namespace Invector;

[RequireComponent(typeof(AudioSource))]
public class vAudioSurfaceControl : MonoBehaviour
{
	private AudioSource source;

	private bool isWorking;

	public AudioMixerGroup outputAudioMixerGroup
	{
		set
		{
			if (!source)
			{
				source = GetComponent<AudioSource>();
			}
			source.outputAudioMixerGroup = value;
		}
	}

	public void PlayOneShot(AudioClip clip, float volume)
	{
		if (!source)
		{
			source = GetComponent<AudioSource>();
		}
		source.volume = volume;
		source.PlayOneShot(clip, volume);
		isWorking = true;
	}

	private void Update()
	{
		if (isWorking && !source.isPlaying)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
