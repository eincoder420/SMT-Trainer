using System.Collections.Generic;
using UnityEngine;

namespace Invector;

public class vDestroyOnTrigger : MonoBehaviour
{
	public List<string> targsToDestroy;

	public float destroyDelayTime;

	private void OnTriggerEnter(Collider other)
	{
		if (targsToDestroy.Contains(other.gameObject.tag))
		{
			Object.Destroy(other.gameObject, destroyDelayTime);
		}
	}
}
