using Invector.vCharacterController.AI;
using UnityEngine;

public class Waypont_Control : MonoBehaviour
{
	public vControlAI[] Characters;

	private void Awake()
	{
		for (int i = 0; i < Characters.Length; i++)
		{
			Characters[i].waypointArea = GetComponent<vWaypointArea>();
		}
	}
}
