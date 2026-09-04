using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Invector.vItemManager;

[vClassHeader("Item Window", true, "icon_v2", false, "", openClose = false)]
public class vItemWindow : vMonoBehaviour
{
	[vReadOnly(true)]
	public vItem currentItem;

	public vItemSlot slotPrefab;

	public RectTransform contentWindow;

	public List<vItemSlot> slots;

	public List<vItemType> supportedItems;

	public bool updateSlotCount = true;

	public Text displayNameText;

	public Text displayTypeText;

	public Text displayAmountText;

	public Text displayDescriptionText;

	public Text displayAttributesText;

	[vHelpBox("You can ignore display Attributes using this property", vHelpBoxAttribute.MessageType.None)]
	public List<vItemAttributes> ignoreAttributes;

	[vEditorToolbar("Text Events", false, "", false, false)]
	public InputField.OnChangeEvent onChangeName;

	public InputField.OnChangeEvent onChangeType;

	public InputField.OnChangeEvent onChangeAmount;

	public InputField.OnChangeEvent onChangeDescription;

	public InputField.OnChangeEvent onChangeAttributes;

	[vEditorToolbar("Events", false, "", false, false)]
	public OnCompleteSlotList onCompleteSlotListCallBack;

	public OnHandleSlot onSubmitSlot;

	public OnHandleSlot onSelectSlot;

	public UnityEvent onCancelSlot;

	[Tooltip("Called when item window has slots on enable")]
	public UnityEvent onAddSlots;

	[Tooltip("Called when item window dont have slots on enable")]
	public UnityEvent onClearSlots;

	public vItemSlot currentSelectedSlot;

	private UnityAction<vItemSlot> onSubmitSlotCallback;

	private UnityAction<vItemSlot> onSelectCallback;

	private readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

	public void ReloadItems(List<vItem> items)
	{
		int num = (slots.Contains(currentSelectedSlot) ? slots.IndexOf(currentSelectedSlot) : 0);
		for (int i = 0; i < slots.Count; i++)
		{
			if (i < 0 || i >= slots.Count)
			{
				continue;
			}
			if (slots[i] != null && (slots[i].item == null || !items.Contains(slots[i].item)))
			{
				Object.Destroy(slots[i].gameObject);
				slots.Remove(slots[i]);
				if (i == num)
				{
					currentSelectedSlot = ((i - 1 >= 0) ? slots[i - 1] : ((slots.Count - 1 > 0) ? slots[0] : null));
					if (currentSelectedSlot != null)
					{
						CreateFullItemDescription(currentSelectedSlot);
					}
				}
				i--;
			}
			else if (slots[i] == null)
			{
				slots.RemoveAt(i);
				i--;
			}
		}
		if (currentSelectedSlot == null || currentSelectedSlot.item == null || slots.Count == 0)
		{
			CreateFullItemDescription(null);
			if (slots.Count == 0)
			{
				onClearSlots.Invoke();
			}
		}
		else
		{
			CreateFullItemDescription(currentSelectedSlot);
		}
	}

	public virtual void CreateEquipmentWindow(List<vItem> items, UnityAction<vItemSlot> onPickUpItemCallBack = null, UnityAction<vItemSlot> onSelectSlotCallBack = null, bool destroyAdictionSlots = true)
	{
		StartCoroutine(CreateEquipmentWindowRoutine(items, onPickUpItemCallBack, onSelectSlotCallBack, destroyAdictionSlots));
	}

	public virtual void CreateEquipmentWindow(List<vItem> items, List<vItemType> type, vItem currentItem = null, UnityAction<vItemSlot> onPickUpItemCallback = null, UnityAction<vItemSlot> onSelectSlotCallBack = null)
	{
		this.currentItem = currentItem;
		List<vItem> items2 = items.FindAll((vItem item) => type.Contains(item.type));
		StartCoroutine(CreateEquipmentWindowRoutine(items2, onPickUpItemCallback));
	}

	protected virtual IEnumerator CreateEquipmentWindowRoutine(List<vItem> items, UnityAction<vItemSlot> onPickUpItemCallBack = null, UnityAction<vItemSlot> onSelectSlotCallBack = null, bool destroyAdictionSlots = true)
	{
		List<vItem> _items = ((supportedItems.Count == 0) ? items : items.FindAll((vItem i) => supportedItems.Contains(i.type)));
		if (_items.Count == 0)
		{
			CreateFullItemDescription(null);
			onClearSlots.Invoke();
			if (slots.Count > 0 && destroyAdictionSlots && updateSlotCount)
			{
				for (int l = 0; l < slots.Count; l++)
				{
					yield return null;
					Object.Destroy(slots[l].gameObject);
				}
				slots.Clear();
			}
		}
		else
		{
			if (slots.Count > _items.Count && destroyAdictionSlots && updateSlotCount)
			{
				int l = slots.Count - _items.Count;
				for (int k = 0; k < l; k++)
				{
					yield return null;
					Object.Destroy(slots[0].gameObject);
					slots.RemoveAt(0);
				}
			}
			bool selecItem = false;
			onSubmitSlotCallback = onPickUpItemCallBack;
			onSelectCallback = onSelectSlotCallBack;
			if (slots == null)
			{
				slots = new List<vItemSlot>();
			}
			_ = items.Count;
			for (int l = 0; l < _items.Count; l++)
			{
				vItemSlot slot;
				if (l < slots.Count)
				{
					slot = slots[l];
				}
				else
				{
					slot = Object.Instantiate(slotPrefab);
					slots.Add(slot);
					RectTransform component = slot.GetComponent<RectTransform>();
					component.SetParent(contentWindow);
					component.localPosition = Vector3.zero;
					component.localScale = Vector3.one;
					yield return null;
				}
				slot.AddItem(_items[l]);
				slot.CheckItem(_items[l].isInEquipArea);
				slot.onSubmitSlotCallBack = OnSubmit;
				slot.onSelectSlotCallBack = OnSelect;
				if (currentItem != null && currentItem == _items[l])
				{
					selecItem = true;
					currentSelectedSlot = slot;
					SetSelectable(slot.gameObject);
				}
				slot.UpdateDisplays();
			}
			if (slots.Count > 0 && !selecItem)
			{
				currentSelectedSlot = slots[0];
				StartCoroutine(SetSelectableHandle(slots[0].gameObject));
			}
		}
		if (slots.Count > 0)
		{
			onAddSlots.Invoke();
			CreateFullItemDescription(currentSelectedSlot);
		}
		onCompleteSlotListCallBack.Invoke(slots);
	}

	public virtual IEnumerator SetSelectableHandle(GameObject target)
	{
		if (base.enabled)
		{
			yield return WaitForEndOfFrame;
			SetSelectable(target);
		}
	}

	public virtual void SetSelectable(GameObject target)
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current);
		ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, eventData, ExecuteEvents.pointerExitHandler);
		EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
		ExecuteEvents.Execute(target, eventData, ExecuteEvents.selectHandler);
	}

	public virtual void OnSubmit(vItemSlot slot)
	{
		currentSelectedSlot = slot;
		onSubmitSlotCallback?.Invoke(slot);
		onSubmitSlot.Invoke(slot);
	}

	public virtual void OnSelect(vItemSlot slot)
	{
		currentSelectedSlot = slot;
		CreateFullItemDescription(slot);
		onSelectCallback?.Invoke(slot);
		onSelectSlot.Invoke(slot);
	}

	protected virtual void CreateFullItemDescription(vItemSlot slot)
	{
		string text = (((bool)slot && (bool)slot.item) ? slot.item.name : "");
		string text2 = (((bool)slot && (bool)slot.item) ? slot.item.ItemTypeText() : "");
		string text3 = (((bool)slot && (bool)slot.item) ? slot.item.amount.ToString() : "");
		string text4 = (((bool)slot && (bool)slot.item) ? slot.item.description : "");
		string text5 = (((bool)slot && (bool)slot.item) ? slot.item.GetItemAttributesText(ignoreAttributes) : "");
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

	public virtual void OnCancel()
	{
		onCancelSlot.Invoke();
	}
}
