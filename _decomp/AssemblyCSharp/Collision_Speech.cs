using UnityEngine;

public class Collision_Speech : MonoBehaviour
{
	private Roxanne_Control player;

	public Speech speech;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.tag == "Player")
		{
			if (!player)
			{
				player = Object.FindObjectOfType<Roxanne_Control>();
			}
			player.Speak(speech);
		}
	}
}
