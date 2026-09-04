using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

[vClassHeader("Destroy GameObject", true, "icon_v2", false, "", openClose = false)]
public class vDestroyGameObject : vMonoBehaviour
{
	public float delay;

	public UnityEvent onDestroy;

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(delay);
		onDestroy.Invoke();
		Object.Destroy(base.gameObject);
	}
}
