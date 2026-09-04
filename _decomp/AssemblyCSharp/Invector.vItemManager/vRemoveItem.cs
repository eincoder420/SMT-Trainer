using UnityEngine;

namespace Invector.vItemManager;

[vClassHeader("Remove Item", true, "icon_v2", false, "", openClose = false)]
public class vRemoveItem : vMonoBehaviour
{
	public vRemoveCurrentItem.Type type = vRemoveCurrentItem.Type.DestroyItem;

	public bool getItemByName;

	[vHideInInspector("getItemByName", false)]
	public string itemName;

	[vHideInInspector("getItemByName", true)]
	public int itemID;

	public void RemoveItem(Collider target)
	{
		vItemManager component = target.GetComponent<vItemManager>();
		RemoveItem(component);
	}

	public void RemoveItem(GameObject target)
	{
		vItemManager component = target.GetComponent<vItemManager>();
		RemoveItem(component);
	}

	public void RemoveItem(vItemManager itemManager)
	{
		if (!itemManager)
		{
			return;
		}
		vItem item = GetItem(itemManager);
		if (item != null)
		{
			if (type == vRemoveCurrentItem.Type.UnequipItem)
			{
				itemManager.UnequipItem(item);
			}
			else if (type == vRemoveCurrentItem.Type.DestroyItem)
			{
				itemManager.DestroyItem(item, 1);
			}
			else
			{
				itemManager.DropItem(item, 1);
			}
		}
	}

	private vItem GetItem(vItemManager itemManager)
	{
		if (getItemByName)
		{
			if (itemManager.ContainItem(itemName))
			{
				return itemManager.GetItem(itemName);
			}
		}
		else if (itemManager.ContainItem(itemID))
		{
			return itemManager.GetItem(itemID);
		}
		return null;
	}
}
