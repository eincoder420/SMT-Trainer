using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Invector.vItemManager;

[vClassHeader("Check If Item Is Equipped", true, "icon_v2", false, "", openClose = false)]
public class vCheckItemIsEquipped : vMonoBehaviour
{
	[Serializable]
	public class CheckItemIDEvent
	{
		public string name;

		public List<int> _itemsID;

		public UnityEvent onIsItemEquipped;

		public UnityEvent onIsItemUnequipped;

		internal bool isEquipped;
	}

	[Serializable]
	public class CheckItemTypeEvent
	{
		public string name;

		public List<vItemType> itemTypes;

		public UnityEvent onIsItemEquipped;

		public UnityEvent onIsItemUnequipped;

		internal bool isEquipped;
	}

	public vItemManager itemManager;

	public bool getInParent = true;

	[FormerlySerializedAs("itemChecks")]
	public List<CheckItemIDEvent> itemIDEvents;

	public List<CheckItemTypeEvent> itemTypeEvents;

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
			itemManager.onEquipItem.AddListener(CheckIsEquipped);
			itemManager.onUnequipItem.AddListener(CheckIsEquipped);
		}
	}

	private void CheckIsEquipped(vEquipArea arg0, vItem arg1)
	{
		for (int i = 0; i < itemIDEvents.Count; i++)
		{
			CheckItemIDEvent check = itemIDEvents[i];
			CheckItemID(check);
		}
		for (int j = 0; j < itemTypeEvents.Count; j++)
		{
			CheckItemTypeEvent check2 = itemTypeEvents[j];
			CheckItemType(check2);
		}
	}

	private void CheckItemID(CheckItemIDEvent check)
	{
		bool flag = check._itemsID.Exists((int t) => itemManager.ItemIsEquipped(t));
		if (flag != check.isEquipped)
		{
			check.isEquipped = flag;
			if (check.isEquipped)
			{
				check.onIsItemEquipped.Invoke();
			}
			else
			{
				check.onIsItemUnequipped.Invoke();
			}
		}
	}

	private void CheckItemType(CheckItemTypeEvent check)
	{
		bool flag = check.itemTypes.Exists((vItemType t) => itemManager.ItemTypeIsEquipped(t));
		if (flag != check.isEquipped)
		{
			check.isEquipped = flag;
			if (check.isEquipped)
			{
				check.onIsItemEquipped.Invoke();
			}
			else
			{
				check.onIsItemUnequipped.Invoke();
			}
		}
	}
}
