using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager;

[vClassHeader("Contains Item Trigger", "Simple trigger to check if the Player has a specific Item, you can also use Events to trigger something if you have the item.", openClose = false)]
public class vContainsItemTrigger : vMonoBehaviour
{
	public bool getItemByName;

	[vHideInInspector("getItemByName", false)]
	public string itemName;

	[vHideInInspector("getItemByName", true)]
	public int itemID;

	public bool useTriggerStay;

	public int desiredAmount = 1;

	[Header("OnTriggerEnter/Stay")]
	public UnityEvent onContains;

	public UnityEvent onNotContains;

	[Header("OnTriggerExit")]
	public UnityEvent onExit;

	public vItemManager itemManager;

	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			vItemManager component = other.GetComponent<vItemManager>();
			if ((bool)component)
			{
				CheckItem(component);
			}
		}
	}

	public void OnTriggerStay(Collider other)
	{
		if (useTriggerStay && other.gameObject.CompareTag("Player"))
		{
			itemManager = other.GetComponent<vItemManager>();
			if ((bool)itemManager)
			{
				CheckItem(itemManager);
			}
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			onExit.Invoke();
		}
	}

	public void RemoveDesiredItem()
	{
		if (!itemManager)
		{
			return;
		}
		if (getItemByName)
		{
			if (ContainsItem(itemManager))
			{
				itemManager.DestroyItem(itemManager.GetItem(itemName), (desiredAmount <= 1) ? 1 : desiredAmount);
			}
		}
		else if (ContainsItem(itemManager))
		{
			itemManager.DestroyItem(itemManager.GetItem(itemID), (desiredAmount <= 1) ? 1 : desiredAmount);
		}
	}

	protected virtual void CheckItem(vItemManager itemManager)
	{
		if (!(itemManager == null))
		{
			if (ContainsItem(itemManager))
			{
				onContains.Invoke();
			}
			else
			{
				onNotContains.Invoke();
			}
		}
	}

	protected bool ContainsItem(vItemManager itemManager)
	{
		if (desiredAmount <= 1)
		{
			if (!getItemByName)
			{
				return itemManager.ContainItem(itemID);
			}
			return itemManager.ContainItem(itemName);
		}
		if (!getItemByName)
		{
			return itemManager.ContainItem(itemID, desiredAmount);
		}
		return itemManager.ContainItem(itemName, desiredAmount);
	}
}
