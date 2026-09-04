using UnityEngine;

namespace Invector;

public class vPickupItem : MonoBehaviour
{
	private AudioSource _audioSource;

	public AudioClip _audioClip;

	public GameObject _particle;

	private void Start()
	{
		_audioSource = GetComponent<AudioSource>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && !_audioSource.isPlaying)
		{
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			_audioSource.PlayOneShot(_audioClip);
			Object.Destroy(base.gameObject, _audioClip.length);
		}
	}
}
