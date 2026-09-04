using UnityEngine;

namespace Invector.Utils;

[vClassHeader("Set Transform", true, "icon_v2", false, "", openClose = false)]
public class vSetTransform : vMonoBehaviour
{
	public Transform targetPosition;

	public Transform SelfTransform
	{
		get
		{
			if (!targetPosition)
			{
				return base.transform;
			}
			return targetPosition;
		}
	}

	public void SetPosition(Transform _target)
	{
		_target.position = SelfTransform.position;
	}

	public void SetRotation(Transform _target)
	{
		_target.rotation = SelfTransform.rotation;
	}

	public void SetPositionAndRotation(Transform _target)
	{
		SetPosition(_target);
		SetRotation(_target);
	}

	public void SetPosition(Collider _target)
	{
		_target.transform.position = SelfTransform.position;
	}

	public void SetRotation(Collider _target)
	{
		_target.transform.rotation = SelfTransform.rotation;
	}

	public void SetPositionAndRotation(Collider _target)
	{
		SetPosition(_target);
		SetRotation(_target);
	}
}
