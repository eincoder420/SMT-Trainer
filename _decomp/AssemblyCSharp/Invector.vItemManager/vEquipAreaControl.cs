using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager;

public class vEquipAreaControl : MonoBehaviour
{
	[HideInInspector]
	public List<vEquipArea> equipAreas;

	private void Start()
	{
		equipAreas = GetComponentsInChildren<vEquipArea>().vToList();
		foreach (vEquipArea equipArea in equipAreas)
		{
			equipArea.onPickUpItemCallBack = OnPickUpItemCallBack;
		}
		vInventory componentInParent = GetComponentInParent<vInventory>();
		if ((bool)componentInParent)
		{
			componentInParent.onOpenCloseInventory.AddListener(OnOpen);
		}
	}

	public virtual void OnOpen(bool value)
	{
	}

	public virtual void OnPickUpItemCallBack(vEquipArea area, vItemSlot slot)
	{
		for (int i = 0; i < equipAreas.Count; i++)
		{
			List<vEquipSlot> list = equipAreas[i].equipSlots.FindAll((vEquipSlot slotInArea) => slotInArea != slot && slotInArea.item != null && slotInArea.item == slot.item);
			for (int j = 0; j < list.Count; j++)
			{
				equipAreas[i].UnequipItem(list[j]);
			}
		}
		CheckTwoHandItem(area, slot);
	}

	private void CheckTwoHandItem(vEquipArea area, vItemSlot slot)
	{
		if (slot.item == null)
		{
			return;
		}
		vEquipArea vEquipArea2 = equipAreas.Find((vEquipArea _area) => _area != null && area.equipPointName.Equals("LeftArm") && _area.currentEquippedItem != null);
		if (area.equipPointName.Equals("LeftArm"))
		{
			vEquipArea2 = equipAreas.Find((vEquipArea _area) => _area != null && area.equipPointName.Equals("RightArm") && _area.currentEquippedItem != null);
		}
		else if (!area.equipPointName.Equals("RightArm"))
		{
			return;
		}
		if (vEquipArea2 != null && (slot.item.twoHandWeapon || vEquipArea2.currentEquippedItem.twoHandWeapon))
		{
			vEquipArea2.onUnequipItem.Invoke(vEquipArea2, slot.item);
			vEquipArea2.UnequipItem(slot as vEquipSlot);
		}
	}
}
