using UnityEngine;

namespace Invector.vShooter;

[RequireComponent(typeof(LineRenderer))]
public class vLaserSight : MonoBehaviour
{
	public LayerMask layerMask;

	public GameObject aimSprite;

	public float aimSpriteOffset;

	public float maxDistance;

	private Ray ray;

	private RaycastHit hit;

	private LineRenderer line;

	private void Start()
	{
		line = GetComponent<LineRenderer>();
		ray = default(Ray);
	}

	private void LateUpdate()
	{
		ray.origin = base.transform.position;
		ray.direction = base.transform.forward.normalized;
		Vector3 zero = Vector3.zero;
		if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
		{
			zero.z = base.transform.InverseTransformPoint(hit.point).z - aimSpriteOffset;
			line.SetPosition(1, zero);
			aimSprite.transform.rotation = Quaternion.LookRotation(hit.normal);
		}
		else
		{
			zero.z = Vector3.Distance(base.transform.position, ray.GetPoint(maxDistance - aimSpriteOffset));
			line.SetPosition(1, zero);
			aimSprite.transform.localEulerAngles = Vector3.zero;
		}
		aimSprite.transform.localPosition = zero;
	}
}
