using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI;

[vClassHeader("SpawnPoint", true, "icon_v2", false, "", helpBoxText = "This component needs a Collider set as IsTrigger to check if this SpawnPoint area is in use", useHelpBox = true, openClose = false)]
public class vSpawnPoint : vMonoBehaviour
{
	public List<Collider> colliders = new List<Collider>();

	public bool isValid => colliders.Count == 0;

	private void OnTriggerStay(Collider other)
	{
		if (colliders.Contains(other))
		{
			vIHealthController componentInParent = other.gameObject.GetComponentInParent<vIHealthController>();
			if (componentInParent != null && componentInParent.isDead)
			{
				colliders.Remove(other);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.gameObject.CompareTag("Untagged") && !colliders.Contains(other))
		{
			colliders.Add(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (colliders.Contains(other))
		{
			colliders.Remove(other);
		}
	}
}
