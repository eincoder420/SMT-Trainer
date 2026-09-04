using UnityEngine;

public class vTeleport : MonoBehaviour
{
	public Transform targetPoint;

	public bool includeRoot;

	public bool rotateToTargetForward = true;

	public void Teleport(Collider collider)
	{
		Vector3 position = base.transform.InverseTransformPoint(includeRoot ? collider.transform.root.position : collider.transform.position);
		Vector3 direction = base.transform.InverseTransformDirection(includeRoot ? collider.transform.root.forward : collider.transform.forward);
		position.Set(0f, position.y, 0f);
		if (includeRoot)
		{
			collider.transform.root.position = targetPoint.TransformPoint(position);
			if (rotateToTargetForward)
			{
				collider.transform.root.rotation = targetPoint.rotation;
			}
			else
			{
				collider.transform.root.forward = targetPoint.TransformDirection(direction);
			}
		}
		else
		{
			collider.transform.position = targetPoint.TransformPoint(position);
			if (rotateToTargetForward)
			{
				collider.transform.rotation = targetPoint.rotation;
			}
			else
			{
				collider.transform.forward = targetPoint.TransformDirection(direction);
			}
		}
	}
}
