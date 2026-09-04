using UnityEngine;
using UnityEngine.EventSystems;

namespace Invector.vItemManager;

public class vItemWindowDisplay : MonoBehaviour
{
	public vInventory inventory;

	public vItemWindow itemWindow;

	public vItemOptionWindow optionWindow;

	[HideInInspector]
	public vItemSlot currentSelectedSlot;

	[HideInInspector]
	public int amount;

	public virtual void OnEnable()
	{
		if (inventory == null)
		{
			inventory = GetComponentInParent<vInventory>();
		}
		if ((bool)inventory && (bool)itemWindow)
		{
			inventory.onDestroyItem.RemoveListener(OnDestroyItem);
			inventory.onDestroyItem.AddListener(OnDestroyItem);
			itemWindow.CreateEquipmentWindow(inventory.items, OnSubmit, OnSelectSlot);
			inventory.OnUpdateInventory -= CheckItemExits;
			inventory.OnUpdateInventory += CheckItemExits;
		}
	}

	public void OnDisable()
	{
		if ((bool)inventory)
		{
			inventory.OnUpdateInventory -= CheckItemExits;
		}
	}

	public virtual void OnDestroyItem(vItem item, int amount)
	{
		vItemSlot vItemSlot2 = itemWindow.slots.Find((vItemSlot slot) => slot.item.Equals(item));
		if (vItemSlot2 != null && (vItemSlot2.item == null || vItemSlot2.item.amount == 0))
		{
			itemWindow.slots.Remove(vItemSlot2);
			Object.Destroy(vItemSlot2.gameObject);
		}
	}

	public virtual void OnSubmit(vItemSlot slot)
	{
		currentSelectedSlot = slot;
		if ((bool)slot.item)
		{
			slot.GetComponent<RectTransform>();
			if (optionWindow.CanOpenOptions(slot.item))
			{
				optionWindow.gameObject.SetActive(value: true);
				optionWindow.EnableOptions(slot);
			}
		}
	}

	public virtual void OnSelectSlot(vItemSlot slot)
	{
		currentSelectedSlot = slot;
	}

	public virtual void DropItem()
	{
		if (amount <= 0)
		{
			return;
		}
		inventory.OnDropItem(currentSelectedSlot.item, amount);
		if (currentSelectedSlot != null && (currentSelectedSlot.item == null || currentSelectedSlot.item.amount <= 0))
		{
			if (itemWindow.slots.Contains(currentSelectedSlot))
			{
				itemWindow.slots.Remove(currentSelectedSlot);
			}
			Object.Destroy(currentSelectedSlot.gameObject);
			if (itemWindow.slots.Count > 0)
			{
				SetSelectable(itemWindow.slots[0].gameObject);
			}
		}
	}

	public virtual void LeaveItem()
	{
		if (amount <= 0)
		{
			return;
		}
		inventory.OnDestroyItem(currentSelectedSlot.item, amount);
		if (currentSelectedSlot != null && (currentSelectedSlot.item == null || currentSelectedSlot.item.amount <= 0))
		{
			if (itemWindow.slots.Contains(currentSelectedSlot))
			{
				itemWindow.slots.Remove(currentSelectedSlot);
			}
			Object.Destroy(currentSelectedSlot.gameObject);
			if (itemWindow.slots.Count > 0)
			{
				SetSelectable(itemWindow.slots[0].gameObject);
			}
		}
	}

	public virtual void UseItem()
	{
		inventory.OnUseItem(currentSelectedSlot.item);
	}

	private void CheckItemExits()
	{
		itemWindow.ReloadItems(inventory.items);
	}

	public virtual void SetOldSelectable()
	{
		try
		{
			if (currentSelectedSlot != null)
			{
				SetSelectable(currentSelectedSlot.gameObject);
			}
			else if (itemWindow.slots.Count > 0 && itemWindow.slots[0] != null)
			{
				SetSelectable(itemWindow.slots[0].gameObject);
			}
		}
		catch
		{
		}
	}

	public virtual void SetSelectable(GameObject target)
	{
		try
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, eventData, ExecuteEvents.pointerExitHandler);
			EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
			ExecuteEvents.Execute(target, eventData, ExecuteEvents.selectHandler);
		}
		catch
		{
		}
	}
}
