using UnityEngine;

public class Moving_Car : MonoBehaviour
{
	private Roxanne_Control player;

	private AudioSource audio;

	public AudioClip Beep;

	public float MaxDistance;

	public Transform Raycaster;

	private void Start()
	{
		player = Object.FindObjectOfType<Roxanne_Control>();
		audio = GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (Physics.Raycast(new Ray(Raycaster.transform.position, Raycaster.transform.forward), out var hitInfo, MaxDistance) && hitInfo.collider.CompareTag("Player") && !audio.isPlaying)
		{
			audio.PlayOneShot(Beep);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if ((bool)collision.collider.GetComponent<Roxanne_Control>())
		{
			collision.collider.GetComponent<Roxanne_Control>().GetComponentInChildren<Hit_Trigger>().Hitten_By_Car();
		}
	}
}
