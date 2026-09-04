using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Invector.vItemManager;

[vClassHeader("Item Slot", true, "icon_v2", false, "", openClose = false)]
public class vItemSlot : vMonoBehaviour, IPointerClickHandler, IEventSystemHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerEnterHandler, IPointerExitHandler
{
	[Serializable]
	public class AttributeDisplay
	{
		public vItemAttributes name;

		[Tooltip("Special Tags\n(NAME) = Display name of the Attribute\n(VALUE) = Display the value of the Attribute\n ***Keep Empty to use default attribute display***")]
		public string displayFormat = "(VALUE)";

		public Text text;

		public InputField.OnChangeEvent onChangeDisplay;
	}

	[vEditorToolbar("Default", false, "", false, false)]
	public vItem item;

	public bool isValid = true;

	[HideInInspector]
	public bool isChecked;

	[vEditorToolbar("Optional", false, "", false, false)]
	public Image icon;

	public Image blockIcon;

	public Image checkIcon;

	public Text displayNameText;

	public Text displayTypeText;

	public Text displayAmountText;

	public Text displayDescriptionText;

	public Text displayAttributesText;

	[vHelpBox("You can ignore display Attributes using this property", vHelpBoxAttribute.MessageType.None)]
	public List<vItemAttributes> ignoreAttributes;

	[vEditorToolbar("Events", false, "", false, false)]
	public InputField.OnChangeEvent onChangeName;

	public InputField.OnChangeEvent onChangeType;

	public InputField.OnChangeEvent onChangeAmount;

	public InputField.OnChangeEvent onChangeDescription;

	public InputField.OnChangeEvent onChangeAttributes;

	public List<AttributeDisplay> customAttributeDisplay;

	[vEditorToolbar("Events", false, "", false, false)]
	public ItemSlotEvent onSubmitSlotCallBack;

	[vEditorToolbar("Events", false, "", false, false)]
	public ItemSlotEvent onSelectSlotCallBack;

	[vEditorToolbar("Events", false, "", false, false)]
	public ItemSlotEvent onDeselectSlotCallBack;

	public OnHandleItemEvent onAddItem;

	public OnHandleItemEvent onRemoveItem;

	public UnityEvent onEnable;

	public UnityEvent onDisable;

	public UnityEvent onClick;

	protected Selectable selectable;

	protected Color color = Color.white;

	private void OnEnable()
	{
		onEnable.Invoke();
		UpdateDisplays(item);
	}

	private void OnDisable()
	{
		onDisable.Invoke();
	}

	protected virtual void Start()
	{
		vInventory componentInParent = GetComponentInParent<vInventory>();
		if ((bool)componentInParent)
		{
			componentInParent.OnUpdateInventory += UpdateDisplays;
		}
		selectable = GetComponent<Selectable>();
		SetValid(isValid);
	}

	public virtual void UpdateDisplays()
	{
		UpdateDisplays(item);
	}

	private void OnDestroy()
	{
		vInventory componentInParent = GetComponentInParent<vInventory>();
		if ((bool)componentInParent)
		{
			componentInParent.OnUpdateInventory -= UpdateDisplays;
		}
	}

	public virtual void CheckItem(bool value)
	{
		isChecked = value;
		if ((bool)checkIcon)
		{
			checkIcon.gameObject.SetActive(isChecked);
		}
	}

	public virtual void SetValid(bool value)
	{
		isValid = value;
		if ((bool)selectable)
		{
			selectable.interactable = value;
		}
		if (!(blockIcon == null))
		{
			blockIcon.color = (value ? Color.clear : Color.white);
			blockIcon.SetAllDirty();
			isValid = value;
		}
	}

	public virtual void AddItem(vItem item)
	{
		if (item != null)
		{
			onAddItem.Invoke(item);
			this.item = item;
			UpdateDisplays(item);
		}
		else
		{
			RemoveItem();
		}
	}

	private void UpdateDisplays(vItem item)
	{
		ChangeDisplayIcon(item);
		ChangeDisplayName(item);
		ChangeDisplayType(item);
		ChangeDisplayAmount(item);
		ChangeDisplayDescription(item);
		ChangeDisplayAttributes(item);
		CheckItem(item != null && item.isInEquipArea);
	}

	protected virtual void ChangeDisplayType(vItem item)
	{
		if ((bool)item)
		{
			onChangeType.Invoke(item.ItemTypeText());
			if ((bool)displayTypeText)
			{
				displayTypeText.text = item.ItemTypeText();
			}
		}
		else
		{
			onChangeType.Invoke("");
			if ((bool)displayTypeText)
			{
				displayTypeText.text = "";
			}
		}
	}

	protected virtual void ChangeDisplayAttributes(vItem item)
	{
		if ((bool)item)
		{
			if ((bool)displayAttributesText)
			{
				displayAttributesText.text = item.GetItemAttributesText(ignoreAttributes);
			}
			onChangeAttributes.Invoke(item.GetItemAttributesText(ignoreAttributes));
			int i;
			for (i = 0; i < item.attributes.Count; i++)
			{
				AttributeDisplay attributeDisplay = customAttributeDisplay.Find((AttributeDisplay att) => att.name.Equals(item.attributes[i].name));
				if (attributeDisplay != null)
				{
					string displayText = item.attributes[i].GetDisplayText();
					if ((bool)attributeDisplay.text)
					{
						attributeDisplay.text.text = displayText;
					}
					attributeDisplay.onChangeDisplay.Invoke(displayText);
				}
			}
			return;
		}
		if ((bool)displayAttributesText)
		{
			displayAttributesText.text = "";
		}
		onChangeAttributes.Invoke("");
		for (int j = 0; j < customAttributeDisplay.Count; j++)
		{
			if ((bool)customAttributeDisplay[j].text)
			{
				customAttributeDisplay[j].text.text = "";
			}
			customAttributeDisplay[j].onChangeDisplay.Invoke("");
		}
	}

	protected virtual void ChangeDisplayIcon(vItem item)
	{
		if ((bool)icon && (bool)item)
		{
			icon.sprite = item.icon;
			color.a = 1f;
			icon.color = color;
		}
	}

	protected virtual void ChangeDisplayDescription(vItem item)
	{
		if ((bool)item)
		{
			onChangeDescription.Invoke(item.description);
			if ((bool)displayDescriptionText)
			{
				displayDescriptionText.text = item.description;
			}
		}
		else
		{
			onChangeDescription.Invoke("");
			if ((bool)displayDescriptionText)
			{
				displayDescriptionText.text = "";
			}
		}
	}

	protected virtual void ChangeDisplayAmount(vItem item)
	{
		string text = "";
		if (item != null && base.gameObject.activeSelf)
		{
			text = ((!item.stackable) ? "" : ("x" + item.amount));
		}
		else if (item == null)
		{
			text = "";
		}
		if ((bool)displayAmountText)
		{
			displayAmountText.text = text;
		}
		onChangeAmount.Invoke(text);
	}

	protected virtual void ChangeDisplayName(vItem item)
	{
		if ((bool)item)
		{
			onChangeName.Invoke(item.name);
			if ((bool)displayNameText)
			{
				displayNameText.text = item.name;
			}
		}
		else
		{
			onChangeName.Invoke("");
			if ((bool)displayNameText)
			{
				displayNameText.text = "";
			}
		}
	}

	public virtual void RemoveItem()
	{
		onRemoveItem.Invoke(item);
		item = null;
		if ((bool)icon)
		{
			color.a = 0f;
			icon.color = color;
			icon.sprite = null;
			icon.SetAllDirty();
		}
		UpdateDisplays(null);
	}

	public virtual bool isOcupad()
	{
		return item != null;
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		if (onSelectSlotCallBack != null)
		{
			onSelectSlotCallBack(this);
		}
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		if (onDeselectSlotCallBack != null)
		{
			onDeselectSlotCallBack(this);
		}
	}

	public virtual void OnSubmit(BaseEventData eventData)
	{
		if (isValid)
		{
			onClick.Invoke();
			if (onSubmitSlotCallBack != null)
			{
				onSubmitSlotCallBack(this);
			}
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		EventSystem.current.SetSelectedGameObject(base.gameObject);
		if (onSelectSlotCallBack != null)
		{
			onSelectSlotCallBack(this);
		}
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		if (onDeselectSlotCallBack != null)
		{
			onDeselectSlotCallBack(this);
		}
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && isValid)
		{
			onClick.Invoke();
			if (onSubmitSlotCallBack != null)
			{
				onSubmitSlotCallBack(this);
			}
		}
	}
}
