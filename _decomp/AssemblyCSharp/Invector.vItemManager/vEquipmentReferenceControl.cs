using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager;

public class vEquipmentReferenceControl : MonoBehaviour
{
	[Serializable]
	public class vEquipmentReference
	{
		public string name;

		public int id;

		public vEquipment equipment;
	}

	public List<vEquipmentReference> equipmentReferences;

	private void Awake()
	{
		vItemManager componentInParent = GetComponentInParent<vItemManager>();
		componentInParent.onEquipItem.AddListener(OnEquip);
		componentInParent.onUnequipItem.AddListener(OnUniquip);
	}

	protected virtual void OnEquip(vEquipArea equipArea, vItem equipment)
	{
		if ((bool)equipment)
		{
			SetActiveEquipment(equipment, active: true);
		}
	}

	protected virtual void OnUniquip(vEquipArea equipArea, vItem equipment)
	{
		if ((bool)equipment)
		{
			SetActiveEquipment(equipment, active: false);
		}
	}

	public virtual void SetActiveEquipment(vItem item, bool active)
	{
		List<vEquipmentReference> list = equipmentReferences.FindAll((vEquipmentReference e) => e.id.Equals(item.id));
		for (int i = 0; i < list.Count; i++)
		{
			if ((bool)list[i].equipment)
			{
				list[i].equipment.gameObject.SetActive(active);
				if (active)
				{
					list[i].equipment.OnEquip(item);
				}
				else
				{
					list[i].equipment.OnUnequip(item);
				}
			}
		}
	}
}
