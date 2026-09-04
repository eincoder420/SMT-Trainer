using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Invector.vItemManager;

[vClassHeader("Inventory", true, "icon_v2", false, "")]
public class vInventory : vMonoBehaviour
{
	public delegate List<vItem> GetItemsDelegate();

	public delegate void AddItemDelegate(ItemReference itemReference, bool immediate = true, UnityAction<vItem> onFinish = null);

	public delegate bool LockInventoryInputEvent();

	public delegate int GetAllAmountDelegate(int id);

	public delegate void OnUpdateInventoryDelegate();

	public GetItemsDelegate GetItemsHandler;

	public GetItemsDelegate GetItemsAllHandler;

	public AddItemDelegate AddItemsHandler;

	public GetAllAmountDelegate GetAllAmount;

	public LockInventoryInputEvent IsLockedEvent;

	[vEditorToolbar("Settings", false, "", false, false)]
	[vHelpBox("True: Play Item animation when the timeScale is 0 \n False: Ignore Item animation if timeScale equals 0", vHelpBoxAttribute.MessageType.None)]
	public bool playItemAnimation = true;

	[Range(0f, 1f)]
	public float timeScaleWhileIsOpen;

	[Tooltip("Check true to not destroy this object when changing scenes")]
	public bool dontDestroyOnLoad = true;

	public List<ChangeEquipmentControl> changeEquipmentControllers;

	[vEditorToolbar("Input Mapping", false, "", false, false)]
	public GenericInput openInventory = new GenericInput("I", "Start", "Start");

	public GenericInput removeEquipment = new GenericInput("Mouse1", "X", "X");

	[Header("This fields will override the EventSystem Input")]
	public GenericInput horizontal = new GenericInput("Horizontal", "D-Pad Horizontal", "Horizontal");

	public GenericInput vertical = new GenericInput("Vertical", "D-Pad Vertical", "Vertical");

	public GenericInput submit = new GenericInput("Return", "A", "A");

	public GenericInput cancel = new GenericInput("Backspace", "B", "B");

	[vEditorToolbar("Events", false, "", false, false)]
	public OnOpenCloseInventory onOpenCloseInventory;

	public OnHandleItemEvent onUseItem;

	public OnChangeItemAmount onDestroyItem;

	public OnChangeItemAmount onDropItem;

	public OnChangeEquipmentEvent onEquipItem;

	public OnChangeEquipmentEvent onUnequipItem;

	[HideInInspector]
	public bool isOpen;

	[HideInInspector]
	public bool canEquip;

	[HideInInspector]
	public bool lockInventoryInput;

	public vEquipArea[] equipAreas;

	private float originalTimeScale = 1f;

	private bool updatedTimeScale;

	private vEquipArea currentEquipArea;

	private StandaloneInputModule inputModule;

	public List<vItem> items
	{
		get
		{
			if (GetItemsHandler != null)
			{
				return GetItemsHandler();
			}
			return new List<vItem>();
		}
	}

	public List<vItem> allItems
	{
		get
		{
			if (GetItemsAllHandler != null)
			{
				return GetItemsAllHandler();
			}
			return new List<vItem>();
		}
	}

	public event OnUpdateInventoryDelegate OnUpdateInventory;

	private void Start()
	{
		canEquip = true;
		inputModule = Object.FindObjectOfType<StandaloneInputModule>();
		if (inputModule == null)
		{
			inputModule = new GameObject("EventSystem").AddComponent<StandaloneInputModule>();
		}
		equipAreas = GetComponentsInChildren<vEquipArea>(includeInactive: true);
		vEquipArea[] array = equipAreas;
		foreach (vEquipArea obj in array)
		{
			obj.Init();
			obj.onEquipItem.AddListener(OnEquipItem);
			obj.onUnequipItem.AddListener(OnUnequipItem);
			obj.onSelectEquipArea.AddListener(SetCurrentSelectedArea);
		}
		for (int j = 0; j < changeEquipmentControllers.Count; j++)
		{
			if (changeEquipmentControllers[j] != null && (bool)changeEquipmentControllers[j].equipArea && (bool)changeEquipmentControllers[j].display)
			{
				changeEquipmentControllers[j].equipArea.onSetLockToEquip.AddListener(changeEquipmentControllers[j].display.SetLockToEquip);
			}
		}
	}

	private void LateUpdate()
	{
		if (!IsLocked())
		{
			OpenCloseInventoryInput();
			if (isOpen)
			{
				UpdateEventSystemInput();
			}
			if (!isOpen)
			{
				ChangeEquipmentInput();
			}
			else
			{
				RemoveEquipmentInput();
			}
		}
	}

	public void SaveItemsExample()
	{
		GetComponentInParent<vItemManager>().SaveInventory();
	}

	public void LoadItemsExample()
	{
		GetComponentInParent<vItemManager>().LoadInventory();
	}

	public void OnReloadGame()
	{
		StartCoroutine(ReloadEquipment());
	}

	private IEnumerator ReloadEquipment()
	{
		yield return new WaitForEndOfFrame();
		inputModule = Object.FindObjectOfType<StandaloneInputModule>();
		isOpen = true;
		for (int i = 0; i < equipAreas.Length; i++)
		{
			vEquipArea vEquipArea2 = equipAreas[i];
			for (int j = 0; j < vEquipArea2.equipSlots.Count; j++)
			{
				vEquipSlot vEquipSlot2 = vEquipArea2.equipSlots[j];
				if (vEquipArea2.currentEquippedItem == null)
				{
					OnUnequipItem(vEquipArea2, vEquipSlot2.item);
					vEquipArea2.UnequipItem(vEquipSlot2);
				}
				else
				{
					vEquipArea2.UnequipItem(vEquipSlot2);
				}
			}
		}
		isOpen = false;
	}

	public virtual bool IsLocked()
	{
		if (IsLockedEvent == null || !IsLockedEvent())
		{
			return lockInventoryInput;
		}
		return true;
	}

	public virtual void UpdateInventory()
	{
		this.OnUpdateInventory();
	}

	public virtual void OpenCloseInventoryInput()
	{
		if (openInventory.GetButtonDown() && canEquip)
		{
			if (!isOpen)
			{
				OpenInventory();
			}
			else
			{
				CloseInventory();
			}
		}
	}

	public virtual void OpenInventory()
	{
		if (!isOpen)
		{
			isOpen = true;
			if (!updatedTimeScale)
			{
				updatedTimeScale = true;
				originalTimeScale = Time.timeScale;
				Time.timeScale = timeScaleWhileIsOpen;
			}
			onOpenCloseInventory.Invoke(arg0: true);
		}
	}

	public virtual void CloseInventory()
	{
		if (isOpen)
		{
			isOpen = false;
			if (updatedTimeScale)
			{
				Time.timeScale = originalTimeScale;
				updatedTimeScale = false;
			}
			onOpenCloseInventory.Invoke(arg0: false);
		}
	}

	protected virtual void RemoveEquipmentInput()
	{
		if (currentEquipArea != null && removeEquipment.GetButtonDown())
		{
			currentEquipArea.UnequipCurrentItem();
		}
	}

	protected virtual void SetCurrentSelectedArea(vEquipArea equipArea)
	{
		currentEquipArea = equipArea;
	}

	protected virtual void ChangeEquipmentInput()
	{
		if (changeEquipmentControllers.Count <= 0 || !canEquip)
		{
			return;
		}
		foreach (ChangeEquipmentControl changeEquipmentController in changeEquipmentControllers)
		{
			UseItemInput(changeEquipmentController);
			if (!(changeEquipmentController.equipArea != null))
			{
				continue;
			}
			if (vInput.instance.inputDevice == InputDevice.MouseKeyboard || vInput.instance.inputDevice == InputDevice.Mobile)
			{
				if (changeEquipmentController.previousItemInput.GetButtonDown())
				{
					changeEquipmentController.equipArea.PreviousEquipSlot();
				}
				if (changeEquipmentController.nextItemInput.GetButtonDown())
				{
					changeEquipmentController.equipArea.NextEquipSlot();
				}
			}
			else if (vInput.instance.inputDevice == InputDevice.Joystick)
			{
				if (changeEquipmentController.previousItemInput.GetAxisButtonDown(-1f))
				{
					changeEquipmentController.equipArea.PreviousEquipSlot();
				}
				if (changeEquipmentController.nextItemInput.GetAxisButtonDown(1f))
				{
					changeEquipmentController.equipArea.NextEquipSlot();
				}
			}
		}
	}

	public virtual void CheckEquipmentChanges()
	{
		for (int i = 0; i < equipAreas.Length; i++)
		{
			vEquipArea equipArea = equipAreas[i];
			for (int j = 0; j < equipArea.equipSlots.Count; j++)
			{
				vEquipSlot vEquipSlot2 = equipArea.equipSlots[j];
				if (vEquipSlot2.item != null && !items.Contains(vEquipSlot2.item))
				{
					equipArea.UnequipItem(vEquipSlot2);
					ChangeEquipmentControl changeEquipmentControl = changeEquipmentControllers.Find((ChangeEquipmentControl e) => e.equipArea.Equals(equipArea));
					if (changeEquipmentControl != null && (bool)changeEquipmentControl.display)
					{
						changeEquipmentControl.display.RemoveItem();
					}
				}
			}
		}
	}

	protected virtual void UpdateEventSystemInput()
	{
		if ((bool)inputModule)
		{
			inputModule.horizontalAxis = horizontal.buttonName;
			inputModule.verticalAxis = vertical.buttonName;
			inputModule.submitButton = submit.buttonName;
			inputModule.cancelButton = cancel.buttonName;
		}
		else
		{
			inputModule = Object.FindObjectOfType<StandaloneInputModule>();
		}
	}

	protected virtual void UseItemInput(ChangeEquipmentControl changeEquip)
	{
		if (changeEquip.display != null && changeEquip.display.item != null && changeEquip.display.item.type == vItemType.Consumable && changeEquip.useItemInput.GetButtonDown() && changeEquip.display.item.amount > 0)
		{
			OnUseItem(changeEquip.display.item);
		}
	}

	internal virtual void OnUseItem(vItem item)
	{
		onUseItem.Invoke(item);
	}

	internal virtual void OnDestroyItem(vItem item, int amount)
	{
		onDestroyItem.Invoke(item, amount);
		CheckEquipmentChanges();
	}

	internal virtual void OnDropItem(vItem item, int amount)
	{
		onDropItem.Invoke(item, amount);
		CheckEquipmentChanges();
	}

	public virtual void OnEquipItem(vEquipArea equipArea, vItem item)
	{
		onEquipItem.Invoke(equipArea, item);
		ChangeEquipmentDisplay(equipArea, item, removeItem: false);
	}

	public virtual void OnUnequipItem(vEquipArea equipArea, vItem item)
	{
		onUnequipItem.Invoke(equipArea, item);
		ChangeEquipmentDisplay(equipArea, item);
	}

	protected virtual void ChangeEquipmentDisplay(vEquipArea equipArea, vItem item, bool removeItem = true)
	{
		if (changeEquipmentControllers.Count <= 0)
		{
			return;
		}
		ChangeEquipmentControl changeEquipmentControl = changeEquipmentControllers.Find((ChangeEquipmentControl changeEquip) => changeEquip.equipArea != null && changeEquip.equipArea == equipArea && changeEquip.display != null);
		if (changeEquipmentControl != null)
		{
			if (removeItem && changeEquipmentControl.display.item == item)
			{
				changeEquipmentControl.display.RemoveItem();
				changeEquipmentControl.display.ItemIdentifier(changeEquipmentControl.equipArea.indexOfEquippedItem + 1, showIdentifier: true);
			}
			else if (equipArea.currentEquippedItem == item)
			{
				changeEquipmentControl.display.AddItem(item);
				changeEquipmentControl.display.ItemIdentifier(changeEquipmentControl.equipArea.indexOfEquippedItem + 1, showIdentifier: true);
			}
		}
	}
}
