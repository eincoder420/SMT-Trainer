using UnityEngine;

public class Play_Audio : MonoBehaviour
{
	public AudioSource audio;

	public void Play_Sound(AudioClip clip)
	{
		audio.PlayOneShot(clip);
	}
}
