using System;
using System.Collections.Generic;
using Invector.vItemManager;
using UnityEngine;

public class vItemDisplay3D : MonoBehaviour
{
	[Serializable]
	public class vDisplay
	{
		public int itemId;

		public GameObject itemModel;
	}

	public GameObject currentItemModel;

	public List<vDisplay> displays;

	public void Display(vItemSlot slot)
	{
		if ((bool)slot)
		{
			Display(slot.item);
		}
	}

	public void Display(int id)
	{
		vDisplay vDisplay = displays.Find((vDisplay d) => d.itemId.Equals(id));
		if (vDisplay != null)
		{
			if ((bool)currentItemModel)
			{
				currentItemModel.SetActive(value: false);
			}
			vDisplay.itemModel.SetActive(value: true);
			currentItemModel = vDisplay.itemModel;
		}
	}

	public void Display(vItem item)
	{
		if ((bool)item)
		{
			Display(item.id);
		}
	}
}
