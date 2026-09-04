using UnityEngine;

public class Decor_Control : MonoBehaviour
{
	private void OnBecameInvisible()
	{
		GetComponentInParent<Collider>().enabled = false;
		GetComponentInParent<Rigidbody>().isKinematic = true;
		GetComponentInParent<Rigidbody>().detectCollisions = false;
	}
}
