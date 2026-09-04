using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Invector.vItemManager;

[vClassHeader("Equip Area", true, "icon_v2", false, "", openClose = false)]
public class vEquipArea : vMonoBehaviour
{
	public delegate void OnPickUpItem(vEquipArea area, vItemSlot slot);

	public OnPickUpItem onPickUpItemCallBack;

	public vInventory inventory;

	public vItemWindow itemPicker;

	[Tooltip("Set current equiped slot when submit an slot of this area")]
	public bool setEquipSlotWhenSubmit;

	[Tooltip("Skip empty slots when switching between slots")]
	public bool skipEmptySlots;

	public List<vEquipSlot> equipSlots;

	public string equipPointName;

	public Text displayNameText;

	public Text displayTypeText;

	public Text displayAmountText;

	public Text displayDescriptionText;

	public Text displayAttributesText;

	[vHelpBox("You can ignore display Attributes using this property", vHelpBoxAttribute.MessageType.None)]
	public List<vItemAttributes> ignoreAttributes;

	public UnityEvent onInitPickUpItem;

	public UnityEvent onFinishPickUpItem;

	public InputField.OnChangeEvent onChangeName;

	public InputField.OnChangeEvent onChangeType;

	public InputField.OnChangeEvent onChangeAmount;

	public InputField.OnChangeEvent onChangeDescription;

	public InputField.OnChangeEvent onChangeAttributes;

	public OnChangeEquipmentEvent onEquipItem;

	public OnChangeEquipmentEvent onUnequipItem;

	public OnSelectEquipArea onSelectEquipArea;

	public Toggle.ToggleEvent onSetLockToEquip;

	[HideInInspector]
	public vEquipSlot currentSelectedSlot;

	public vEquipSlot lastSelectedSlot;

	[HideInInspector]
	public int indexOfEquippedItem;

	public vItem lastEquipedItem;

	protected bool _isLockedToEquip;

	public bool ignoreEquipEvents;

	internal bool isInit;

	public bool isLockedToEquip
	{
		get
		{
			return _isLockedToEquip;
		}
		set
		{
			if (_isLockedToEquip != value)
			{
				onSetLockToEquip.Invoke(value);
			}
			_isLockedToEquip = value;
		}
	}

	public vEquipSlot currentEquippedSlot => equipSlots[indexOfEquippedItem];

	public vItem currentEquippedItem
	{
		get
		{
			List<vEquipSlot> validSlots = ValidSlots;
			if (validSlots.Count > 0 && indexOfEquippedItem >= 0 && indexOfEquippedItem < validSlots.Count)
			{
				return validSlots[indexOfEquippedItem].item;
			}
			return null;
		}
	}

	public List<vEquipSlot> ValidSlots => equipSlots.FindAll((vEquipSlot slot) => slot.isValid && (!skipEmptySlots || slot.item != null));

	public void Init()
	{
		if (!isInit)
		{
			Start();
		}
	}

	private void Start()
	{
		if (isInit)
		{
			return;
		}
		isInit = true;
		inventory = GetComponentInParent<vInventory>();
		if (equipSlots.Count == 0)
		{
			vEquipSlot[] componentsInChildren = GetComponentsInChildren<vEquipSlot>(includeInactive: true);
			equipSlots = componentsInChildren.vToList();
		}
		foreach (vEquipSlot equipSlot in equipSlots)
		{
			equipSlot.onSubmitSlotCallBack = OnSubmitSlot;
			equipSlot.onSelectSlotCallBack = OnSelectSlot;
			equipSlot.onDeselectSlotCallBack = OnDeselect;
			onSetLockToEquip.AddListener(equipSlot.SetLockToEquip);
			if ((bool)equipSlot.displayAmountText)
			{
				equipSlot.displayAmountText.text = "";
			}
			equipSlot.onChangeAmount.Invoke("");
		}
	}

	public bool ContainsItem(vItem item)
	{
		return ValidSlots.Find((vEquipSlot slot) => slot.item == item) != null;
	}

	public void OnSubmitSlot(vItemSlot slot)
	{
		lastSelectedSlot = currentSelectedSlot;
		if (itemPicker != null)
		{
			currentSelectedSlot = slot as vEquipSlot;
			if (setEquipSlotWhenSubmit)
			{
				SetEquipSlot(equipSlots.IndexOf(currentSelectedSlot));
			}
			itemPicker.gameObject.SetActive(value: true);
			itemPicker.onCancelSlot.RemoveAllListeners();
			itemPicker.onCancelSlot.AddListener(CancelCurrentSlot);
			itemPicker.CreateEquipmentWindow(inventory.items, currentSelectedSlot.itemType, slot.item, OnPickItem);
			onInitPickUpItem.Invoke();
		}
	}

	public void CancelCurrentSlot()
	{
		if (currentSelectedSlot == null)
		{
			currentSelectedSlot = lastSelectedSlot;
		}
		if (currentSelectedSlot != null)
		{
			currentSelectedSlot.OnCancel();
		}
		onFinishPickUpItem.Invoke();
	}

	public void UnequipItem(vEquipSlot slot)
	{
		if ((bool)slot)
		{
			vItem item = slot.item;
			if (ValidSlots[indexOfEquippedItem].item == item)
			{
				lastEquipedItem = item;
			}
			slot.RemoveItem();
			onUnequipItem.Invoke(this, item);
		}
	}

	public void UnequipItem(vItem item)
	{
		vEquipSlot vEquipSlot2 = ValidSlots.Find((vEquipSlot _slot) => _slot.item == item);
		if ((bool)vEquipSlot2)
		{
			if (ValidSlots[indexOfEquippedItem].item == item)
			{
				lastEquipedItem = item;
			}
			vEquipSlot2.RemoveItem();
			onUnequipItem.Invoke(this, item);
		}
	}

	public void UnequipCurrentItem()
	{
		if ((bool)currentSelectedSlot && (bool)currentSelectedSlot.item)
		{
			vItem item = currentSelectedSlot.item;
			if (ValidSlots[indexOfEquippedItem].item == item)
			{
				lastEquipedItem = item;
			}
			currentSelectedSlot.RemoveItem();
			onUnequipItem.Invoke(this, item);
		}
	}

	public void OnSelectSlot(vItemSlot slot)
	{
		if (equipSlots.Contains(slot as vEquipSlot))
		{
			currentSelectedSlot = slot as vEquipSlot;
		}
		else
		{
			currentSelectedSlot = null;
		}
		onSelectEquipArea.Invoke(this);
		CreateFullItemDescription(slot);
	}

	public void OnDeselect(vItemSlot slot)
	{
		if (equipSlots.Contains(slot as vEquipSlot))
		{
			currentSelectedSlot = null;
		}
	}

	protected virtual void CreateFullItemDescription(vItemSlot slot)
	{
		string text = (slot.item ? slot.item.name : "");
		string text2 = (slot.item ? slot.item.ItemTypeText() : "");
		string text3 = (slot.item ? slot.item.amount.ToString() : "");
		string text4 = (slot.item ? slot.item.description : "");
		string text5 = (slot.item ? slot.item.GetItemAttributesText(ignoreAttributes) : "");
		if ((bool)displayNameText)
		{
			displayNameText.text = text;
		}
		onChangeName.Invoke(text);
		if ((bool)displayTypeText)
		{
			displayTypeText.text = text2;
		}
		onChangeType.Invoke(text2);
		if ((bool)displayAmountText)
		{
			displayAmountText.text = text3;
		}
		onChangeAmount.Invoke(text3);
		if ((bool)displayDescriptionText)
		{
			displayDescriptionText.text = text4;
		}
		onChangeDescription.Invoke(text4);
		if ((bool)displayAttributesText)
		{
			displayAttributesText.text = text5;
		}
		onChangeAttributes.Invoke(text5);
	}

	public void OnPickItem(vItemSlot slot)
	{
		if (!currentSelectedSlot)
		{
			currentSelectedSlot = lastSelectedSlot;
		}
		if (!currentSelectedSlot)
		{
			return;
		}
		if (currentSelectedSlot.item != null && slot.item != currentSelectedSlot.item)
		{
			currentSelectedSlot.item.isInEquipArea = false;
			vItem item = currentSelectedSlot.item;
			if (item == slot.item)
			{
				lastEquipedItem = item;
			}
			currentSelectedSlot.RemoveItem();
			onUnequipItem.Invoke(this, item);
		}
		if (slot.item != currentSelectedSlot.item)
		{
			if (onPickUpItemCallBack != null)
			{
				onPickUpItemCallBack(this, slot);
			}
			currentSelectedSlot.AddItem(slot.item);
			if (!ignoreEquipEvents)
			{
				onEquipItem.Invoke(this, currentSelectedSlot.item);
			}
		}
		currentSelectedSlot.OnCancel();
		currentSelectedSlot = null;
		lastSelectedSlot = null;
		itemPicker.gameObject.SetActive(value: false);
		onFinishPickUpItem.Invoke();
	}

	public void NextEquipSlot()
	{
		if (equipSlots != null && equipSlots.Count != 0)
		{
			lastEquipedItem = currentEquippedItem;
			List<vEquipSlot> validSlots = ValidSlots;
			if (indexOfEquippedItem + 1 < validSlots.Count)
			{
				indexOfEquippedItem++;
			}
			else
			{
				indexOfEquippedItem = 0;
			}
			if (currentEquippedItem != null && !ignoreEquipEvents)
			{
				onEquipItem.Invoke(this, currentEquippedItem);
			}
			onUnequipItem.Invoke(this, lastEquipedItem);
		}
	}

	public void PreviousEquipSlot()
	{
		if (equipSlots != null && equipSlots.Count != 0)
		{
			lastEquipedItem = currentEquippedItem;
			List<vEquipSlot> validSlots = ValidSlots;
			if (indexOfEquippedItem - 1 >= 0)
			{
				indexOfEquippedItem--;
			}
			else
			{
				indexOfEquippedItem = validSlots.Count - 1;
			}
			if (currentEquippedItem != null && !ignoreEquipEvents)
			{
				onEquipItem.Invoke(this, currentEquippedItem);
			}
			onUnequipItem.Invoke(this, lastEquipedItem);
		}
	}

	public void SetEquipSlot(int indexOfSlot)
	{
		if (equipSlots != null && equipSlots.Count != 0 && indexOfSlot < equipSlots.Count && indexOfSlot >= 0)
		{
			lastEquipedItem = currentEquippedItem;
			indexOfEquippedItem = indexOfSlot;
			if (currentEquippedItem != null && !ignoreEquipEvents)
			{
				onEquipItem.Invoke(this, currentEquippedItem);
			}
			if (currentEquippedItem != lastEquipedItem)
			{
				onUnequipItem.Invoke(this, lastEquipedItem);
			}
		}
	}

	public void EquipCurrentSlot()
	{
		if ((bool)currentEquippedSlot && (!(currentEquippedSlot.item != null) || !currentEquippedSlot.item.isEquiped))
		{
			if ((bool)currentEquippedItem)
			{
				onEquipItem.Invoke(this, currentEquippedItem);
			}
			else if ((bool)lastEquipedItem)
			{
				onUnequipItem.Invoke(this, lastEquipedItem);
			}
		}
	}

	public void AddItemToEquipSlot(vItemSlot slot, vItem item, bool autoEquip = false)
	{
		if (slot is vEquipSlot && equipSlots.Contains(slot as vEquipSlot))
		{
			AddItemToEquipSlot(equipSlots.IndexOf(slot as vEquipSlot), item, autoEquip);
		}
	}

	public void AddItemToEquipSlot(int indexOfSlot, vItem item, bool autoEquip = false)
	{
		if (indexOfSlot >= equipSlots.Count || !(item != null))
		{
			return;
		}
		vEquipSlot slot = equipSlots[indexOfSlot];
		if (!(slot != null) || !slot.isValid || !slot.itemType.Contains(item.type))
		{
			return;
		}
		vEquipSlot vEquipSlot2 = equipSlots.Find((vEquipSlot s) => s.item == item && s != slot);
		if (vEquipSlot2 != null)
		{
			RemoveItemOfEquipSlot(equipSlots.IndexOf(vEquipSlot2));
		}
		if (slot.item != null && slot.item != item)
		{
			if (currentEquippedItem == slot.item)
			{
				lastEquipedItem = slot.item;
			}
			slot.item.isInEquipArea = false;
			onUnequipItem.Invoke(this, slot.item);
		}
		item.isInEquipArea = true;
		slot.AddItem(item);
		if (autoEquip)
		{
			SetEquipSlot(indexOfSlot);
		}
		else if (!ignoreEquipEvents)
		{
			onEquipItem.Invoke(this, item);
		}
	}

	public void RemoveItemOfEquipSlot(vItemSlot slot)
	{
		if (slot is vEquipSlot && equipSlots.Contains(slot as vEquipSlot))
		{
			RemoveItemOfEquipSlot(equipSlots.IndexOf(slot as vEquipSlot));
		}
	}

	public void RemoveItemOfEquipSlot(int indexOfSlot)
	{
		if (indexOfSlot >= equipSlots.Count)
		{
			return;
		}
		vEquipSlot vEquipSlot2 = equipSlots[indexOfSlot];
		if (vEquipSlot2 != null && vEquipSlot2.item != null)
		{
			vItem item = vEquipSlot2.item;
			item.isInEquipArea = false;
			if (currentEquippedItem == item)
			{
				lastEquipedItem = currentEquippedItem;
			}
			vEquipSlot2.RemoveItem();
			onUnequipItem.Invoke(this, item);
		}
	}

	public void AddCurrentItem(vItem item)
	{
		if (indexOfEquippedItem >= equipSlots.Count)
		{
			return;
		}
		vEquipSlot vEquipSlot2 = equipSlots[indexOfEquippedItem];
		if (vEquipSlot2.item != null && item != vEquipSlot2.item)
		{
			if (currentEquippedItem == vEquipSlot2.item)
			{
				lastEquipedItem = vEquipSlot2.item;
			}
			vEquipSlot2.item.isInEquipArea = false;
			onUnequipItem.Invoke(this, currentSelectedSlot.item);
		}
		vEquipSlot2.AddItem(item);
		if (!ignoreEquipEvents)
		{
			onEquipItem.Invoke(this, item);
		}
	}

	public void RemoveCurrentItem()
	{
		if ((bool)currentEquippedItem)
		{
			lastEquipedItem = currentEquippedItem;
			ValidSlots[indexOfEquippedItem].RemoveItem();
			onUnequipItem.Invoke(this, lastEquipedItem);
		}
	}
}
