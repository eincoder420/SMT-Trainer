using UnityEngine;

namespace Invector;

public class vLookAtCamera : MonoBehaviour
{
	[Tooltip("Align position to stay always on top of parent")]
	public bool alignUp;

	[Tooltip("Height of alignment on top of parent \n!!(Check alignUp to work)!!")]
	public float height = 1f;

	[Tooltip("Detach of the parent on start \n!!(if alignUp not is checked, the object not follow the parent)!!")]
	public bool detachOnStart;

	[Tooltip("use smoth to look at camera")]
	public bool useSmothRotation = true;

	protected Transform parent;

	public bool justY;

	internal Camera cameraMain;

	private void Start()
	{
		if (detachOnStart)
		{
			parent = base.transform.parent;
			base.transform.SetParent(null);
		}
		cameraMain = Camera.main;
	}

	private void FixedUpdate()
	{
		if (alignUp && (bool)parent)
		{
			base.transform.position = parent.position + Vector3.up * height;
		}
		if ((bool)cameraMain)
		{
			Vector3 forward = cameraMain.transform.position - base.transform.position;
			forward.y = 0f;
			Quaternion b = Quaternion.LookRotation(forward);
			if (useSmothRotation)
			{
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * 4f);
				base.transform.eulerAngles = new Vector3(justY ? 0f : base.transform.eulerAngles.x, base.transform.eulerAngles.y, 0f);
			}
			else
			{
				base.transform.eulerAngles = new Vector3(justY ? 0f : b.eulerAngles.x, b.eulerAngles.y, 0f);
			}
		}
	}
}
