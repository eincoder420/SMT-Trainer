using UnityEngine;

namespace Invector.Utils;

public class vTargetLookAt : MonoBehaviour
{
	public Transform target;

	public float smooth;

	public float offsetHeight;

	public bool limitDistance;

	public float minDistanceToLook;

	private void Update()
	{
		if ((bool)target)
		{
			Vector3 vector = target.position + Vector3.up * offsetHeight - base.transform.position;
			Quaternion b = Quaternion.LookRotation(vector.normalized, Vector3.up);
			if (!limitDistance || vector.magnitude > minDistanceToLook)
			{
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, smooth * Time.deltaTime);
			}
		}
	}
}
