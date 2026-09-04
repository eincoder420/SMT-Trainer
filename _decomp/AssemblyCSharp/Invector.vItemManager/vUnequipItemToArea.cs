using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager;

public class vUnequipItemToArea : MonoBehaviour
{
	[HideInInspector]
	public List<vEquipArea> equipAreas;

	protected vEquipArea equipArea;

	protected vInventory inventory;

	protected vEquipSlot currentSlot
	{
		get
		{
			if (!equipArea)
			{
				return null;
			}
			if (!equipArea.currentSelectedSlot)
			{
				return equipArea.lastSelectedSlot;
			}
			return equipArea.currentSelectedSlot;
		}
	}

	private void Start()
	{
		equipAreas = GetComponentsInChildren<vEquipArea>().vToList();
		foreach (vEquipArea equipArea in equipAreas)
		{
			equipArea.onSelectEquipArea.AddListener(OnSelectArea);
		}
		inventory = GetComponentInParent<vInventory>();
	}

	public void OnSelectArea(vEquipArea area)
	{
		equipArea = area;
	}

	public void UnequipItem()
	{
		if ((bool)equipArea && currentSlot != null && currentSlot.item != null)
		{
			equipArea.RemoveItemOfEquipSlot(currentSlot);
		}
	}
}
