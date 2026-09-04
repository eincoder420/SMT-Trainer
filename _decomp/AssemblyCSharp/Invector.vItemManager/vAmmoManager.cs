using System.Collections.Generic;
using Invector.vShooter;
using UnityEngine;

namespace Invector.vItemManager;

[vClassHeader("Ammo Manager", true, "icon_v2", false, "", iconName = "ammoIcon")]
public class vAmmoManager : vMonoBehaviour
{
	public delegate void OnUpdateTotalAmmo();

	public vAmmoListData ammoListData;

	[HideInInspector]
	public vItemManager itemManager;

	public List<vAmmo> ammos = new List<vAmmo>();

	public OnUpdateTotalAmmo updateTotalAmmo = delegate
	{
	};

	private void Start()
	{
		itemManager = GetComponent<vItemManager>();
		if ((bool)itemManager)
		{
			itemManager.onAddItem.AddListener(AddAmmo);
			itemManager.onDropItem.AddListener(DropAmmo);
			itemManager.onDestroyItem.AddListener(LeaveAmmo);
			itemManager.onChangeItemAmount.AddListener(ChangeItemAmount);
			itemManager.onLoadItems.AddListener(ReloadAllAmmoItems);
		}
		if ((bool)ammoListData)
		{
			ammos.Clear();
			for (int i = 0; i < ammoListData.ammos.Count; i++)
			{
				vAmmo vAmmo2 = new vAmmo(ammoListData.ammos[i]);
				vAmmo2.onDestroyAmmoItem = OnDestroyAmmoItem;
				ammos.Add(vAmmo2);
			}
		}
	}

	public vAmmo GetAmmo(int id)
	{
		return ammos.Find((vAmmo a) => a.ammoID == id);
	}

	public void AddAmmo(int id, int amount)
	{
		ammos.Find((vAmmo a) => a.ammoID == id)?.AddAmmo(amount);
		UpdateTotalAmmo();
	}

	public void AddAmmo(string ammoName, int id, int amount)
	{
		vAmmo vAmmo2 = ammos.Find((vAmmo a) => a.ammoID == id);
		if (vAmmo2 == null)
		{
			vAmmo2 = new vAmmo(ammoName, id, amount);
			ammos.Add(vAmmo2);
			vAmmo2.onDestroyAmmoItem = OnDestroyAmmoItem;
		}
		else
		{
			vAmmo2?.AddAmmo(amount);
		}
		UpdateTotalAmmo();
	}

	public void AddAmmo(vItem item)
	{
		if (item.type == vItemType.Ammo)
		{
			vAmmo vAmmo2 = ammos.Find((vAmmo a) => a.ammoID == item.id);
			if (vAmmo2 == null)
			{
				vAmmo2 = new vAmmo(item.name, item.id);
				ammos.Add(vAmmo2);
				vAmmo2.onDestroyAmmoItem = OnDestroyAmmoItem;
			}
			vAmmo2.ammoItems.Add(item);
		}
		else
		{
			vItemAttribute itemAttribute = item.GetItemAttribute(vItemAttributes.AmmoCount);
			if (itemAttribute != null)
			{
				vShooterWeapon component = item.originalObject.GetComponent<vShooterWeapon>();
				if (component != null && itemAttribute != null && itemAttribute.value > component.clipSize)
				{
					int num = itemAttribute.value - component.clipSize;
					itemAttribute.value -= num;
					ItemReference itemReference = new ItemReference(component.ammoID);
					itemReference.amount = num;
					itemManager.CollectItem(itemReference);
				}
			}
		}
		UpdateTotalAmmo();
	}

	protected void ChangeItemAmount(vItem item)
	{
		if (item.type == vItemType.Ammo)
		{
			vAmmo vAmmo2 = ammos.Find((vAmmo a) => a.ammoID == item.id);
			if (vAmmo2 == null)
			{
				vAmmo2 = new vAmmo(item.name, item.id, item.amount);
				ammos.Add(vAmmo2);
				vAmmo2.onDestroyAmmoItem = OnDestroyAmmoItem;
			}
		}
		UpdateTotalAmmo();
	}

	public void LeaveAmmo(vItem item, int amount)
	{
		if (item.type == vItemType.Ammo)
		{
			vAmmo vAmmo2 = ammos.Find((vAmmo a) => a.ammoID == item.id);
			if (vAmmo2 != null && item.amount - amount <= 0 && vAmmo2.ammoItems.Contains(item))
			{
				vAmmo2.ammoItems.Remove(item);
			}
		}
		UpdateTotalAmmo();
	}

	public void DropAmmo(vItem item, int amount)
	{
		if (item.type == vItemType.Ammo)
		{
			vAmmo vAmmo2 = ammos.Find((vAmmo a) => a.ammoID == item.id);
			if (vAmmo2 != null && item.amount - amount <= 0 && vAmmo2.ammoItems.Contains(item))
			{
				vAmmo2.ammoItems.Remove(item);
			}
		}
		UpdateTotalAmmo();
	}

	public void UpdateTotalAmmo()
	{
		updateTotalAmmo();
	}

	public void ReloadAllAmmoItems()
	{
		List<vItem> list = itemManager.items.FindAll((vItem item) => item.type == vItemType.Ammo);
		ammos.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			AddAmmo(list[i]);
		}
	}

	private void OnDestroyAmmoItem(vItem item)
	{
		if ((bool)itemManager)
		{
			itemManager.DestroyItem(item, item.amount);
		}
	}
}
