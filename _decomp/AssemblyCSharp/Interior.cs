using UnityEngine;

public class Interior : MonoBehaviour
{
	public int id;

	public AudioClip Music;

	public Transform Place;

	public Transform Start_Place;

	public Transform[] Deactivate_If_Inside;

	public Jerk_Place[] jerk_places;

	public Dance_Place[] dance_places;

	public Jerk_Place Secret_Place;
}
