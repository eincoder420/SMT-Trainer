using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Invector.vItemManager;

public class vEquipSlot : vItemSlot
{
	[vEditorToolbar("Default", false, "", false, false)]
	[vHelpBox("Select what ItemType this EquipSlot will equip", vHelpBoxAttribute.MessageType.Warning)]
	public List<vItemType> itemType;

	public bool clickToOpen = true;

	public bool autoDeselect = true;

	public UnityEvent onCancel;

	public UnityEvent onSetLockToEquip;

	public UnityEvent onUnlockToEquip;

	public void SetLockToEquip(bool value)
	{
		if (value)
		{
			onSetLockToEquip.Invoke();
		}
		else
		{
			onUnlockToEquip.Invoke();
		}
	}

	public override void AddItem(vItem item)
	{
		if ((bool)item)
		{
			item.isInEquipArea = true;
		}
		base.AddItem(item);
	}

	public override void CheckItem(bool value)
	{
		if ((bool)checkIcon && checkIcon.gameObject.activeSelf)
		{
			checkIcon.gameObject.SetActive(value: false);
		}
	}

	public override void RemoveItem()
	{
		if (item != null)
		{
			item.isInEquipArea = false;
		}
		base.RemoveItem();
	}

	public virtual void OnCancel()
	{
		onCancel.Invoke();
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		if (autoDeselect)
		{
			base.OnDeselect(eventData);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (autoDeselect)
		{
			base.OnPointerExit(eventData);
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (clickToOpen)
		{
			base.OnPointerClick(eventData);
		}
	}
}
