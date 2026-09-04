using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Invector.vItemManager;

public static class vSaveLoadInventory
{
	[Serializable]
	private class InventoryData
	{
		public List<ItemReference> itemReferences = new List<ItemReference>();

		public List<EquipAreaData> equipAreas = new List<EquipAreaData>();

		public List<vItem> GetItems(vItemListData itemListData)
		{
			List<vItem> list = new List<vItem>();
			int i;
			for (i = 0; i < itemReferences.Count; i++)
			{
				vItem original = itemListData.items.Find((vItem a) => a.id.Equals(itemReferences[i].id));
				original = UnityEngine.Object.Instantiate(original);
				original.amount = itemReferences[i].amount;
				original.attributes = itemReferences[i].attributes;
				original.name = original.name.Replace("(Clone)", string.Empty);
				list.Add(original);
			}
			return list;
		}
	}

	[Serializable]
	private class EquipAreaData
	{
		public List<SlotData> slotsData = new List<SlotData>();

		public int indexOfSelectedSlot;
	}

	[Serializable]
	private class SlotData
	{
		public bool hasItem;

		public int indexOfItem;
	}

	[Serializable]
	private class ItemReference
	{
		[SerializeField]
		public int amount;

		[SerializeField]
		public int id;

		[SerializeField]
		public List<vItemAttribute> attributes;

		public ItemReference(vItem item)
		{
			amount = item.amount;
			id = item.id;
			attributes = item.attributes;
		}
	}

	public static string InventoryDataFile = Application.dataPath + Path.DirectorySeparatorChar + "InventoryData.json";

	public static string InventoryToJsonText(vItemManager itemManager)
	{
		if (!itemManager.inventory)
		{
			return string.Empty;
		}
		InventoryData inventoryData = new InventoryData();
		vEquipArea[] equipAreas = itemManager.inventory.equipAreas;
		for (int i = 0; i < equipAreas.Length; i++)
		{
			EquipAreaData equipAreaData = new EquipAreaData();
			equipAreaData.indexOfSelectedSlot = equipAreas[i].indexOfEquippedItem;
			for (int j = 0; j < equipAreas[i].equipSlots.Count; j++)
			{
				SlotData slotData = new SlotData();
				slotData.hasItem = equipAreas[i].equipSlots[j].item != null;
				if (slotData.hasItem)
				{
					slotData.indexOfItem = itemManager.items.IndexOf(equipAreas[i].equipSlots[j].item);
				}
				equipAreaData.slotsData.Add(slotData);
			}
			inventoryData.equipAreas.Add(equipAreaData);
		}
		for (int k = 0; k < itemManager.items.Count; k++)
		{
			inventoryData.itemReferences.Add(new ItemReference(itemManager.items[k]));
		}
		return JsonUtility.ToJson(inventoryData, prettyPrint: true);
	}

	public static string LoadInventoryJasonText()
	{
		if (File.Exists(InventoryDataFile))
		{
			return File.ReadAllText(InventoryDataFile);
		}
		return string.Empty;
	}

	public static void SaveInventory(this vItemManager itemManager)
	{
		string text = InventoryToJsonText(itemManager);
		if (!string.IsNullOrEmpty(text))
		{
			File.WriteAllText(InventoryDataFile, text);
			itemManager.onSaveItems.Invoke();
		}
	}

	public static void LoadInventory(this vItemManager itemManager)
	{
		string text = LoadInventoryJasonText();
		if (!string.IsNullOrEmpty(text))
		{
			InventoryData inventoryData = new InventoryData();
			JsonUtility.FromJsonOverwrite(text, inventoryData);
			itemManager.items = inventoryData.GetItems(itemManager.itemListData);
			vEquipArea[] equipAreas = itemManager.inventory.equipAreas;
			for (int i = 0; i < equipAreas.Length; i++)
			{
				if (i >= inventoryData.equipAreas.Count)
				{
					continue;
				}
				vEquipArea vEquipArea2 = equipAreas[i];
				EquipAreaData equipAreaData = inventoryData.equipAreas[i];
				vEquipArea2.indexOfEquippedItem = equipAreaData.indexOfSelectedSlot;
				for (int j = 0; j < equipAreas[i].equipSlots.Count; j++)
				{
					if (j < equipAreaData.slotsData.Count)
					{
						SlotData slotData = equipAreaData.slotsData[j];
						_ = equipAreas[i].equipSlots[j];
						itemManager.temporarilyIgnoreItemAnimation = true;
						if (slotData.hasItem)
						{
							vEquipArea2.AddItemToEquipSlot(j, itemManager.items[slotData.indexOfItem]);
						}
						else
						{
							vEquipArea2.RemoveItemOfEquipSlot(j);
						}
					}
				}
			}
		}
		itemManager.inventory.UpdateInventory();
		itemManager.temporarilyIgnoreItemAnimation = false;
		itemManager.onLoadItems.Invoke();
	}
}
