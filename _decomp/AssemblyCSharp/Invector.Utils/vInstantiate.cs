using UnityEngine;

namespace Invector.Utils;

[vClassHeader("v Instantiate", true, "icon_v2", false, "", openClose = false)]
public class vInstantiate : vMonoBehaviour
{
	public GameObject prefab;

	public bool instantiateOnStart;

	public bool setThisAsParent;

	protected virtual void Start()
	{
		if (instantiateOnStart)
		{
			InstantiateObject();
		}
	}

	public virtual void InstantiateObject()
	{
		if ((bool)prefab)
		{
			GameObject gameObject = Object.Instantiate(prefab, base.transform.position, base.transform.rotation);
			gameObject.SetActive(value: true);
			if (setThisAsParent)
			{
				gameObject.transform.parent = base.transform;
			}
		}
	}
}
