using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager;

[Serializable]
public class vAmmo
{
	public delegate void OnDestroyItem(vItem item);

	public string ammoName;

	[Tooltip("Ammo ID - if is using ItemManager, make sure your AmmoManager and ItemListData use the same ID")]
	public int ammoID;

	[Tooltip("Don't need to setup if you're using a Inventory System")]
	[SerializeField]
	private int _count;

	public List<vItem> ammoItems;

	public OnDestroyItem onDestroyAmmoItem = delegate
	{
	};

	public virtual int count
	{
		get
		{
			int num = 0;
			if (ammoItems != null && ammoItems.Count > 0)
			{
				for (int i = 0; i < ammoItems.Count; i++)
				{
					if ((bool)ammoItems[i])
					{
						num += ammoItems[i].amount;
					}
				}
			}
			return _count + num;
		}
	}

	public vAmmo()
	{
		ammoItems = new List<vItem>();
	}

	public vAmmo(string ammoName, int ammoID, int amount = 0)
	{
		this.ammoName = ammoName;
		this.ammoID = ammoID;
		_count = amount;
		ammoItems = new List<vItem>();
	}

	public vAmmo(int ammoID, int amount = 0)
	{
		this.ammoID = ammoID;
		_count = amount;
		ammoItems = new List<vItem>();
	}

	public vAmmo(vAmmo ammo)
	{
		ammoName = ammo.ammoName;
		ammoID = ammo.ammoID;
		ammoItems = ammo.ammoItems;
		_count = ammo.count;
		ammoItems = new List<vItem>();
	}

	public virtual void Use()
	{
		vItem vItem2 = ammoItems.Find((vItem a) => a.amount > 0);
		if ((bool)vItem2)
		{
			vItem2.amount--;
			if (vItem2.amount == 0)
			{
				ammoItems.Remove(vItem2);
				onDestroyAmmoItem(vItem2);
			}
		}
		else if (_count > 0)
		{
			_count--;
		}
	}

	public virtual void Use(int amout)
	{
		for (int i = 0; i < amout; i++)
		{
			Use();
		}
	}

	public void AddAmmo(int amount)
	{
		_count += amount;
	}
}
