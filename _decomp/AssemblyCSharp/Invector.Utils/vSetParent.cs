using UnityEngine;

namespace Invector.Utils;

public class vSetParent : MonoBehaviour
{
	public void RemoveParent()
	{
		base.transform.parent = null;
	}

	public void RemoveParent(Transform target)
	{
		target.parent = null;
	}

	public void SetParent(Transform parent)
	{
		base.transform.parent = parent;
	}
}
