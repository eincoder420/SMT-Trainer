using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager;

[vClassHeader("OpenClose Inventory Trigger", false, "icon_v2", false, "")]
public class vOpenCloseInventoryTrigger : vMonoBehaviour
{
	public bool getComponentsInParent = true;

	public vInventory inventory;

	public vItemManager itemManager;

	public UnityEvent onOpen;

	public UnityEvent onClose;

	protected virtual IEnumerator Start()
	{
		inventory = (getComponentsInParent ? GetComponentInParent<vInventory>() : GetComponent<vInventory>());
		if (!inventory)
		{
			yield return new WaitForEndOfFrame();
			itemManager = (getComponentsInParent ? GetComponentInParent<vItemManager>() : GetComponent<vItemManager>());
			if ((bool)itemManager)
			{
				inventory = itemManager.inventory;
			}
		}
		if ((bool)inventory)
		{
			inventory.onOpenCloseInventory.AddListener(OpenCloseInventory);
		}
	}

	public void OpenCloseInventory(bool value)
	{
		if (value)
		{
			onOpen.Invoke();
		}
		else
		{
			onClose.Invoke();
		}
	}
}
