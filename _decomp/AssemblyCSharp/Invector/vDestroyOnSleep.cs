using System.Collections;
using UnityEngine;

namespace Invector;

public class vDestroyOnSleep : MonoBehaviour
{
	private IEnumerator Start()
	{
		Rigidbody rigdb = GetComponent<Rigidbody>();
		Collider collider = GetComponent<Collider>();
		yield return base.transform.parent.gameObject.activeSelf;
		while (!rigdb.IsSleeping())
		{
			yield return new WaitForSeconds(2f);
		}
		Object.Destroy(rigdb);
		if ((bool)collider)
		{
			Object.Destroy(collider);
		}
	}
}
