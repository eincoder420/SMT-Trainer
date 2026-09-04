using UnityEngine;

namespace Invector.Utils;

public class vDestroyChildrens : MonoBehaviour
{
	public virtual void DestroyChildrens()
	{
		DestroyChildrens(base.transform);
	}

	public virtual void DestroyChildrensOfOther(Transform target)
	{
		DestroyChildrens(target);
	}

	protected virtual void DestroyChildrens(Transform target)
	{
		for (int num = target.childCount - 1; num >= 0; num--)
		{
			Object.Destroy(target.GetChild(num).gameObject);
		}
	}
}
