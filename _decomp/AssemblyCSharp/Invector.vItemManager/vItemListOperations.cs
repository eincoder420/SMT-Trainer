using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager;

public static class vItemListOperations
{
	public class EquipedItemInfo
	{
		public vItem item;

		public int indexOfItem;

		public vEquipArea area;

		public int indexOfArea;

		public EquipedItemInfo()
		{
		}

		public EquipedItemInfo(vItem item, vEquipArea area)
		{
			this.item = item;
			this.area = area;
		}
	}

	public static List<vItem> GetSameItems(this List<vItem> itemList, int id)
	{
		return itemList.FindAll((vItem i) => i.id.Equals(id));
	}

	public static List<vItem> GetSameItems(this List<vItem> itemList, string name)
	{
		return itemList.FindAll((vItem i) => i.name.Equals(name));
	}

	public static List<vItem> GetSameItems(this List<vItem> itemList, params int[] ids)
	{
		return itemList.FindAll((vItem i) => Array.Exists(ids, (int id) => i.id.Equals(id)));
	}

	public static List<vItem> GetSameItems(this List<vItem> itemList, params string[] names)
	{
		return itemList.FindAll((vItem i) => Array.Exists(names, (string name) => i.name.Equals(name)));
	}

	public static bool HasItem(this List<vItem> itemList, int id)
	{
		return itemList.Exists((vItem i) => i.id.Equals(id));
	}

	public static bool HasItem(this List<vItem> itemList, string name)
	{
		return itemList.Exists((vItem i) => i.name.Equals(name));
	}

	public static bool HasItems(this List<vItem> itemList, params int[] ids)
	{
		bool result = true;
		for (int i = 0; i < ids.Length; i++)
		{
			if (!itemList.HasItem(ids[i]))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public static bool HasItems(this List<vItem> itemList, params string[] names)
	{
		bool result = true;
		for (int i = 0; i < names.Length; i++)
		{
			if (!itemList.HasItem(names[i]))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public static int GetItemCount(this List<vItem> itemList, int id)
	{
		int count = 0;
		itemList.GetSameItems(id).ForEach(delegate(vItem item)
		{
			count += item.amount;
		});
		return count;
	}

	public static int GetItemCount(this List<vItem> itemList, string name)
	{
		int count = 0;
		itemList.GetSameItems(name).ForEach(delegate(vItem item)
		{
			count += item.amount;
		});
		return count;
	}

	public static void DestroySameItems(this List<vItem> itemList, int id, int amount, Action<vItem, int> onChangeItemAmount = null)
	{
		List<vItem> sameItems = itemList.GetSameItems(id);
		for (int i = 0; i < sameItems.Count; i++)
		{
			vItem vItem2 = sameItems[i];
			if (vItem2.amount > amount)
			{
				onChangeItemAmount?.Invoke(vItem2, amount);
				vItem2.amount -= amount;
				break;
			}
			onChangeItemAmount?.Invoke(vItem2, vItem2.amount);
			amount -= vItem2.amount;
			vItem2.amount = 0;
			itemList.Remove(vItem2);
			UnityEngine.Object.Destroy(vItem2);
		}
	}

	public static void DestroySameItems(this List<vItem> itemList, string name, int amount, Action<vItem, int> onChangeItemAmount = null)
	{
		List<vItem> sameItems = itemList.GetSameItems(name);
		for (int i = 0; i < sameItems.Count; i++)
		{
			vItem vItem2 = sameItems[i];
			if (vItem2.amount > amount)
			{
				onChangeItemAmount?.Invoke(vItem2, amount);
				vItem2.amount -= amount;
				break;
			}
			onChangeItemAmount?.Invoke(vItem2, vItem2.amount);
			amount -= vItem2.amount;
			vItem2.amount = 0;
			itemList.Remove(vItem2);
			UnityEngine.Object.Destroy(vItem2);
		}
	}

	public static void DestroySameItems(this List<vItem> itemList, int id, Action<vItem, int> onChangeItemAmount = null)
	{
		List<vItem> sameItems = itemList.GetSameItems(id);
		itemList.RemoveAll((vItem i) => i.id.Equals(id));
		for (int j = 0; j < sameItems.Count; j++)
		{
			onChangeItemAmount?.Invoke(sameItems[j], sameItems[j].amount);
			UnityEngine.Object.Destroy(sameItems[j]);
		}
	}

	public static void DestroySameItems(this List<vItem> itemList, string name, Action<vItem, int> onChangeItemAmount = null)
	{
		List<vItem> sameItems = itemList.GetSameItems(name);
		itemList.RemoveAll((vItem i) => i.name.Equals(name));
		for (int j = 0; j < sameItems.Count; j++)
		{
			onChangeItemAmount?.Invoke(sameItems[j], sameItems[j].amount);
			UnityEngine.Object.Destroy(sameItems[j]);
		}
	}

	public static void DestroySameItems(this List<vItem> itemList, Action<vItem, int> onChangeItemAmount = null, params int[] ids)
	{
		List<vItem> sameItems = itemList.GetSameItems(ids);
		itemList.RemoveAll((vItem i) => Array.Exists(ids, (int id) => i.id.Equals(id)));
		for (int j = 0; j < sameItems.Count; j++)
		{
			onChangeItemAmount?.Invoke(sameItems[j], sameItems[j].amount);
			UnityEngine.Object.Destroy(sameItems[j]);
		}
	}

	public static void DestroySameItems(this List<vItem> itemList, Action<vItem, int> onChangeItemAmount = null, params string[] names)
	{
		List<vItem> sameItems = itemList.GetSameItems(names);
		itemList.RemoveAll((vItem i) => Array.Exists(names, (string name) => i.name.Equals(name)));
		for (int j = 0; j < sameItems.Count; j++)
		{
			onChangeItemAmount?.Invoke(sameItems[j], sameItems[j].amount);
			UnityEngine.Object.Destroy(sameItems[j]);
		}
	}

	public static bool ItemIsEquipped(this vItemManager itemManager, int id)
	{
		if ((bool)itemManager.inventory)
		{
			return Array.Find(itemManager.inventory.equipAreas, (vEquipArea equipArea) => (bool)equipArea.currentEquippedItem && equipArea.currentEquippedItem.id.Equals(id));
		}
		return false;
	}

	public static bool ItemTypeIsEquipped(this vItemManager itemManager, vItemType type)
	{
		if ((bool)itemManager.inventory)
		{
			return Array.Find(itemManager.inventory.equipAreas, (vEquipArea equipArea) => (bool)equipArea.currentEquippedItem && equipArea.currentEquippedItem.type.Equals(type));
		}
		return false;
	}

	public static bool ItemIsEquipped(this vItemManager itemManager, int id, out EquipedItemInfo equipedItemInfo)
	{
		equipedItemInfo = null;
		if ((bool)itemManager.inventory)
		{
			vEquipArea vEquipArea2 = Array.Find(itemManager.inventory.equipAreas, (vEquipArea equipArea) => (bool)equipArea.currentEquippedItem && equipArea.currentEquippedItem.id.Equals(id));
			if ((bool)vEquipArea2)
			{
				equipedItemInfo = new EquipedItemInfo(vEquipArea2.currentEquippedItem, vEquipArea2);
				equipedItemInfo.indexOfArea = Array.IndexOf(itemManager.inventory.equipAreas, vEquipArea2);
				equipedItemInfo.indexOfItem = itemManager.items.IndexOf(vEquipArea2.currentEquippedItem);
			}
			return vEquipArea2 != null;
		}
		return false;
	}

	public static bool ItemTypeIsEquipped(this vItemManager itemManager, vItemType type, out EquipedItemInfo equipedItemInfo)
	{
		equipedItemInfo = null;
		if ((bool)itemManager.inventory)
		{
			vEquipArea vEquipArea2 = Array.Find(itemManager.inventory.equipAreas, (vEquipArea equipArea) => (bool)equipArea.currentEquippedItem && equipArea.currentEquippedItem.type.Equals(type));
			if ((bool)vEquipArea2)
			{
				equipedItemInfo = new EquipedItemInfo(vEquipArea2.currentEquippedItem, vEquipArea2);
				equipedItemInfo.indexOfArea = Array.IndexOf(itemManager.inventory.equipAreas, vEquipArea2);
				equipedItemInfo.indexOfItem = itemManager.items.IndexOf(vEquipArea2.currentEquippedItem);
			}
			return vEquipArea2 != null;
		}
		return false;
	}

	public static vItem GetEquippedItem(this vItemManager itemManager, int id)
	{
		if ((bool)itemManager.inventory)
		{
			vEquipArea vEquipArea2 = Array.Find(itemManager.inventory.equipAreas, (vEquipArea equipArea) => (bool)equipArea.currentEquippedItem && equipArea.currentEquippedItem.id.Equals(id));
			if (!vEquipArea2)
			{
				return null;
			}
			return vEquipArea2.currentEquippedItem;
		}
		return null;
	}
}
