using UnityEngine;

namespace Invector.Utils;

public class vUpdateUIPosition : MonoBehaviour
{
	public Transform referenceLocalParent;

	public bool updateLocalX;

	public bool updateLocalY;

	public bool updateLocalZ;

	public bool limitOnBox;

	[vHideInInspector("limitOnBox", false)]
	public BoxCollider box;

	public void UpdatePosition(GameObject target)
	{
		SetLocalPosition(target.transform.position);
	}

	public void UpdatePosition(Collider target)
	{
		SetLocalPosition(target.transform.position);
	}

	public void UpdatePosition(Transform target)
	{
		SetLocalPosition(target.position);
	}

	private void SetLocalPosition(Vector3 position)
	{
		if (limitOnBox && (bool)box)
		{
			position = box.ClosestPointOnBounds(position);
		}
		Vector3 vector = referenceLocalParent.InverseTransformPoint(position);
		Vector3 localPosition = base.transform.localPosition;
		if (updateLocalX)
		{
			localPosition.x = vector.x;
		}
		if (updateLocalY)
		{
			localPosition.y = vector.y;
		}
		if (updateLocalZ)
		{
			localPosition.z = vector.z;
		}
		base.transform.localPosition = localPosition;
	}
}
