using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager;

[vClassHeader("Check Item in Inventory", true, "icon_v2", false, "", openClose = false)]
public class vCheckItemInInventory : vMonoBehaviour
{
	[Serializable]
	public class CheckItemIDEvent
	{
		public string name;

		public List<int> _itemsID;

		public UnityEvent onContainItem;

		public UnityEvent onNotContainItem;

		public bool Check(vItemManager itemManager)
		{
			bool result = true;
			for (int i = 0; i < _itemsID.Count; i++)
			{
				if (!itemManager.ContainItem(_itemsID[i]))
				{
					result = false;
					break;
				}
			}
			return result;
		}
	}

	protected vItemManager itemManager;

	public bool getInParent = true;

	public List<CheckItemIDEvent> itemIDEvents;

	private void Awake()
	{
		if (!itemManager)
		{
			if (getInParent)
			{
				itemManager = GetComponentInParent<vItemManager>();
			}
			else
			{
				itemManager = GetComponent<vItemManager>();
			}
			if ((bool)itemManager)
			{
				itemManager.onAddItemID.AddListener(CheckItemExists);
				itemManager.onRemoveItemID.AddListener(CheckItemExists);
			}
		}
	}

	public void CheckOnTrigger(Collider collider)
	{
		if (!(collider != null))
		{
			return;
		}
		itemManager = collider.gameObject.GetComponent<vItemManager>();
		if ((bool)itemManager)
		{
			for (int i = 0; i < itemIDEvents.Count; i++)
			{
				CheckItemIDEvent check = itemIDEvents[i];
				CheckItemID(check);
			}
		}
	}

	private void CheckItemExists(int arg1)
	{
		for (int i = 0; i < itemIDEvents.Count; i++)
		{
			CheckItemIDEvent check = itemIDEvents[i];
			CheckItemID(check);
		}
	}

	private void CheckItemID(CheckItemIDEvent check)
	{
		if (check.Check(itemManager))
		{
			check.onContainItem.Invoke();
		}
		else
		{
			check.onNotContainItem.Invoke();
		}
	}
}
