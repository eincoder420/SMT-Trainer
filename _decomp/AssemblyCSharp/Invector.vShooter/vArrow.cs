using UnityEngine;

namespace Invector.vShooter;

public class vArrow : MonoBehaviour
{
	public vProjectileControl projectileControl;

	public Transform detachObject;

	[Tooltip("Use to raycast other side of the penetration")]
	public bool raycastBackSide = true;

	public bool alignToNormal = true;

	[HideInInspector]
	public float penetration;

	public bool debugPenetration;

	private void Start()
	{
		if (!projectileControl)
		{
			projectileControl = GetComponent<vProjectileControl>();
		}
	}

	public void OnDestroyProjectile(RaycastHit hit)
	{
		Transform transform = hit.transform.Find("ArrowParent");
		if (!transform)
		{
			transform = new GameObject("ArrowParent").transform;
			transform.position = hit.transform.position;
			transform.parent = hit.transform;
		}
		detachObject.parent = transform.transform;
		if (alignToNormal)
		{
			detachObject.rotation = Quaternion.LookRotation(-hit.normal);
		}
		detachObject.position = hit.point + base.transform.forward * penetration;
		if (debugPenetration)
		{
			Debug.DrawLine(hit.point, hit.point + base.transform.forward * penetration, Color.red, 10f);
		}
		if ((bool)projectileControl && raycastBackSide && penetration > 0f)
		{
			Vector3 start = hit.point + base.transform.forward * penetration;
			Vector3 end = hit.point + base.transform.forward * 0.001f;
			if (Physics.Linecast(start, end, out hit, projectileControl.hitLayer))
			{
				projectileControl.onCastCollider.Invoke(hit);
			}
		}
	}
}
