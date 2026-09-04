using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController;

[vClassHeader("THROW COLLECTABLE", false, "icon_v2", false, "")]
public class vThrowCollectable : vMonoBehaviour
{
	public int amount = 1;

	public bool destroyAfter = true;

	private vThrowManager throwManager;

	public UnityEvent onCollectObject;

	public UnityEvent onReachMaxObjects;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player") && other.GetComponentInChildren<vThrowManager>() != null)
		{
			throwManager = other.GetComponentInChildren<vThrowManager>();
		}
	}

	public void UpdateThrowObj(Rigidbody throwObj)
	{
		if (throwManager.currentThrowObject < throwManager.maxThrowObjects)
		{
			throwManager.SetAmount(amount);
			throwManager.objectToThrow = throwObj;
			onCollectObject.Invoke();
			if (destroyAfter)
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			onReachMaxObjects.Invoke();
		}
	}
}
