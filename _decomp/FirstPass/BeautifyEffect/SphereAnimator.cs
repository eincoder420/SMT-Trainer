using UnityEngine;

namespace BeautifyEffect;

public class SphereAnimator : MonoBehaviour
{
	private Rigidbody rb;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (base.transform.position.z < 1f)
		{
			rb.AddForce(Vector3.forward * 10f);
		}
		else if (base.transform.position.z > 8f)
		{
			rb.AddForce(-Vector3.forward * 10f);
		}
	}
}
