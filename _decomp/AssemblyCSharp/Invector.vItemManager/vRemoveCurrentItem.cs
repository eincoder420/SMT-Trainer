using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager;

[vClassHeader("Remove Current Item", false, "icon_v2", false, "")]
public class vRemoveCurrentItem : vMonoBehaviour
{
	public enum Type
	{
		UnequipItem,
		DestroyItem,
		DropItem
	}

	public Type type;

	[Tooltip("Immediately equip the item ignoring the Equip animation")]
	public bool immediate = true;

	[Tooltip("Equip Area of your Inventory Prefab")]
	public int equipArea;

	public UnityEvent OnTriggerEnterEvent;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.gameObject.CompareTag("Player"))
		{
			return;
		}
		vItemManager component = other.gameObject.GetComponent<vItemManager>();
		if ((bool)component)
		{
			if (type == Type.UnequipItem)
			{
				component.UnequipCurrentEquipedItem(equipArea, immediate);
			}
			else if (type == Type.DestroyItem)
			{
				component.DestroyCurrentEquipedItem(equipArea, immediate);
			}
			else
			{
				component.DropCurrentEquippedItem(equipArea, immediate);
			}
		}
		OnTriggerEnterEvent.Invoke();
	}
}
