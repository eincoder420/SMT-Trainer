using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager;

[vClassHeader("ItemManager", true, "icon_v2", false, "")]
public class vItemManager : vMonoBehaviour, IActionReceiver, IActionController
{
	public delegate void CanUseItemDelegate(vItem item, ref List<bool> validationList);

	public struct CollectedItemInfo
	{
		public vItem item;

		public int amount;
	}

	[vHelpBox("Place a Inventory Prefab inside your Character, it will be auto-assigned when you enter Playmode.\nYou can find the prefab at the Inventory/Prefabs folder", vHelpBoxAttribute.MessageType.Info)]
	public vInventory inventory;

	[vHelpBox("You can find the default ItemListData at the Inventory/ItemListData folder, or create a new list at the Invector Menu", vHelpBoxAttribute.MessageType.Info)]
	public vItemListData itemListData;

	public List<ItemReference> startItems = new List<ItemReference>();

	public List<EquipPoint> equipPoints;

	public List<ApplyAttributeEvent> applyAttributeEvents;

	public bool debugMode;

	public OnHandleItemEvent onStartItemUsage;

	public OnHandleItemEvent onUseItem;

	public OnHandleItemEvent onUseItemFail;

	public OnHandleItemEvent onAddItem;

	public OnHandleItemEvent onChangeItemAmount;

	public OnHandleItemIDEvent onAddItemID;

	public OnHandleItemIDEvent onRemoveItemID;

	public OnCollectItemEvent onCollectItem;

	public OnChangeItemAmount onDestroyItem;

	public OnChangeItemAmount onDropItem;

	public OnOpenCloseInventory onOpenCloseInventory;

	public OnChangeEquipmentEvent onEquipItem;

	public OnChangeEquipmentEvent onUnequipItem;

	public OnChangeEquipmentEvent onFinishEquipItem;

	public OnChangeEquipmentEvent onFinishUnequipItem;

	public OnSelectEquipArea onSetLockedToEquip;

	public UnityEvent onSaveItems;

	public UnityEvent onLoadItems;

	public Dictionary<vItem, vEquipment> equipments = new Dictionary<vItem, vEquipment>();

	public Dictionary<GameObject, vEquipment> equipmentsObject = new Dictionary<GameObject, vEquipment>();

	protected GameObject equipmentContainer;

	public List<vItem> items;

	internal bool inEquip;

	internal bool usingItem;

	private float equipTimer;

	private Animator animator;

	protected bool inCollectItemRoutine;

	protected List<ItemReference> itemsToCollect = new List<ItemReference>();

	public vIAnimatorStateInfoController animatorStateInfos;

	[HideInInspector]
	public List<vItemType> itemsFilter = new List<vItemType> { vItemType.Consumable };

	internal bool temporarilyIgnoreItemAnimation;

	internal EquipPoint defaultLeftArmEquipPoint;

	internal EquipPoint defaultRightArmEquipPoint;

	private float unequipTimer;

	internal bool playItemAnimation
	{
		get
		{
			if (inventory != null && ((inventory.isOpen && inventory.playItemAnimation) || !inventory.isOpen))
			{
				return !temporarilyIgnoreItemAnimation;
			}
			return false;
		}
	}

	public event CanUseItemDelegate canUseItemDelegate;

	private IEnumerator Start()
	{
		if (!inventory)
		{
			inventory = base.transform.GetComponentInChildren<vInventory>();
		}
		if (!inventory && debugMode)
		{
			Debug.LogWarning("Missing Inventory prefab - You need to Drag and drop a Inventory Prefab inside the Character");
		}
		if ((bool)inventory)
		{
			equipmentContainer = new GameObject("Equipment Container");
			equipmentContainer.transform.parent = base.transform;
			equipmentContainer.transform.localPosition = Vector3.zero;
			equipmentContainer.transform.localEulerAngles = Vector3.zero;
			inventory.GetItemsHandler = GetItems;
			inventory.GetItemsAllHandler = GetAllItems;
			inventory.AddItemsHandler = AddItem;
			inventory.GetAllAmount = GetAllAmount;
			inventory.onEquipItem.AddListener(EquipItem);
			inventory.onUnequipItem.AddListener(UnequipItem);
			inventory.onDropItem.AddListener(DropItem);
			inventory.onDestroyItem.AddListener(DestroyItem);
			inventory.onUseItem.AddListener(UseItem);
			inventory.onOpenCloseInventory.AddListener(OnOpenCloseInventory);
			vMeleeCombatInput melee = GetComponent<vMeleeCombatInput>();
			if ((bool)melee)
			{
				inventory.IsLockedEvent = () => melee.lockInventory;
			}
		}
		defaultLeftArmEquipPoint = equipPoints.Find((EquipPoint e) => e.equipPointName.Equals("LeftArm"));
		defaultRightArmEquipPoint = equipPoints.Find((EquipPoint e) => e.equipPointName.Equals("RightArm"));
		animator = GetComponent<Animator>();
		animatorStateInfos = GetComponent<vIAnimatorStateInfoController>();
		yield return new WaitForEndOfFrame();
		RegisterDefaultEquipPointListeners();
		items = new List<vItem>();
		if ((bool)itemListData)
		{
			for (int i = 0; i < startItems.Count; i++)
			{
				AddItem(startItems[i], ignoreItemAnimation: true);
			}
		}
	}

	public virtual void RegisterDefaultEquipPointListeners()
	{
		IWeaponEquipmentListener[] components = GetComponents<IWeaponEquipmentListener>();
		if (components.Length == 0)
		{
			return;
		}
		for (int i = 0; i < components.Length; i++)
		{
			if (defaultLeftArmEquipPoint != null)
			{
				defaultLeftArmEquipPoint.onInstantiateEquiment.AddListener(components[i].SetLeftWeapon);
			}
			if (defaultRightArmEquipPoint != null)
			{
				defaultRightArmEquipPoint.onInstantiateEquiment.AddListener(components[i].SetRightWeapon);
			}
		}
	}

	public virtual void LockInventoryInput(bool value)
	{
		if ((bool)inventory)
		{
			inventory.lockInventoryInput = value;
		}
	}

	protected virtual void OnOpenCloseInventory(bool value)
	{
		onOpenCloseInventory.Invoke(value);
	}

	public void SaveItemsExample()
	{
		this.SaveInventory();
	}

	public void LoadItemsExample()
	{
		this.LoadInventory();
	}

	public virtual bool IsAnimatorTag(string tag)
	{
		if (animator == null)
		{
			return false;
		}
		if (animatorStateInfos.isValid() && animatorStateInfos.animatorStateInfos.HasTag(tag))
		{
			return true;
		}
		return false;
	}

	protected virtual void CheckIsLockedToEquip()
	{
		vEquipArea vEquipArea2 = Array.Find(inventory.equipAreas, (vEquipArea a) => a.equipPointName.Equals("RightArm"));
		vEquipArea vEquipArea3 = Array.Find(inventory.equipAreas, (vEquipArea a) => a.equipPointName.Equals("LeftArm"));
		if (vEquipArea2 == null || vEquipArea3 == null)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		flag = (bool)vEquipArea3.currentEquippedItem && (bool)vEquipArea2.currentEquippedItem && vEquipArea2.currentEquippedItem.twoHandWeapon;
		if (flag && (bool)vEquipArea3.currentEquippedItem && equipments.ContainsKey(vEquipArea3.currentEquippedItem))
		{
			vEquipment vEquipment2 = equipments[vEquipArea3.currentEquippedItem];
			if (vEquipment2.gameObject.activeSelf)
			{
				vEquipment2.gameObject.SetActive(value: false);
			}
		}
		if (!flag)
		{
			flag2 = (bool)vEquipArea2.currentEquippedItem && (bool)vEquipArea3.currentEquippedItem && vEquipArea3.currentEquippedItem.twoHandWeapon;
			if (flag2 && (bool)vEquipArea2.currentEquippedItem && equipments.ContainsKey(vEquipArea2.currentEquippedItem))
			{
				vEquipment vEquipment3 = equipments[vEquipArea2.currentEquippedItem];
				if (vEquipment3.gameObject.activeSelf)
				{
					vEquipment3.gameObject.SetActive(value: false);
				}
			}
		}
		if (flag != vEquipArea3.isLockedToEquip)
		{
			vEquipArea3.isLockedToEquip = flag;
			onSetLockedToEquip.Invoke(vEquipArea3);
		}
		if (flag2 != vEquipArea2.isLockedToEquip)
		{
			vEquipArea2.isLockedToEquip = flag2;
			onSetLockedToEquip.Invoke(vEquipArea2);
		}
	}

	[Obsolete("This method will be removed in the future.\n use CheckIsLockedToEquip Method")]
	protected virtual void CheckTwoHandItem(EquipPoint equipPoint, vItem item)
	{
		if (item == null)
		{
			return;
		}
		EquipPoint equipPoint2 = equipPoints.Find((EquipPoint ePoint) => ePoint.area != null && ePoint.equipPointName.Equals("LeftArm") && ePoint.area.currentEquippedItem != null);
		if (equipPoint.equipPointName.Equals("LeftArm"))
		{
			equipPoint2 = equipPoints.Find((EquipPoint ePoint) => ePoint.area != null && ePoint.equipPointName.Equals("RightArm") && ePoint.area.currentEquippedItem != null);
		}
		else if (!equipPoint.equipPointName.Equals("RightArm"))
		{
			return;
		}
		if (equipPoint2 != null && (item.twoHandWeapon || equipPoint2.area.currentEquippedItem.twoHandWeapon))
		{
			equipPoint2.area.RemoveCurrentItem();
		}
	}

	public virtual bool ContainItem(int id)
	{
		return items.Exists((vItem i) => i.id == id);
	}

	public virtual bool ContainItem(string itemName)
	{
		return items.Exists((vItem i) => i.name == itemName);
	}

	public virtual bool ContainItem(int id, int amount)
	{
		return GetAllAmount(id) >= amount;
	}

	public virtual bool ContainItem(string itemName, int amount)
	{
		vItem vItem2 = items.Find((vItem i) => i.name == itemName && i.amount >= amount);
		if (!(vItem2 != null))
		{
			return false;
		}
		return GetAllAmount(vItem2.id) >= amount;
	}

	public virtual bool EquipAreaHasSomeItem(int indexOfArea)
	{
		return inventory.equipAreas[indexOfArea].equipSlots.Exists((vEquipSlot slot) => slot.item != null);
	}

	public virtual bool ItemIsInSomeEquipArea(int id)
	{
		if (!inventory || inventory.equipAreas.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < inventory.equipAreas.Length; i++)
		{
			if (inventory.equipAreas[i].equipSlots.Exists((vEquipSlot slot) => slot.item.id.Equals(id)))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool ItemIsInSomeEquipArea(string itemName)
	{
		if (!inventory || inventory.equipAreas.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < inventory.equipAreas.Length; i++)
		{
			if (inventory.equipAreas[i].equipSlots.Exists((vEquipSlot slot) => slot.item.name.Equals(itemName)))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool ItemIsInSpecificEquipArea(int id, int indexOfArea)
	{
		if (!inventory || inventory.equipAreas.Length == 0 || indexOfArea > inventory.equipAreas.Length - 1)
		{
			return false;
		}
		if (inventory.equipAreas[indexOfArea].equipSlots.Exists((vEquipSlot slot) => slot.item.id.Equals(id)))
		{
			return true;
		}
		return false;
	}

	public virtual bool ItemIsInSpecificEquipArea(string itemName, int indexOfArea)
	{
		if (!inventory || inventory.equipAreas.Length == 0 || indexOfArea > inventory.equipAreas.Length - 1)
		{
			return false;
		}
		if (inventory.equipAreas[indexOfArea].equipSlots.Exists((vEquipSlot slot) => slot.item.name.Equals(itemName)))
		{
			return true;
		}
		return false;
	}

	public virtual bool EquipPointHasSomeItem(string equipPointName)
	{
		return equipPoints.Exists((EquipPoint ep) => ep.equipPointName.Equals(equipPointName) && ep.equipmentReference != null && ep.equipmentReference.item != null);
	}

	public virtual bool ItemIsInSomeEquipPont(int id)
	{
		return equipPoints.Exists((EquipPoint ep) => ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.id.Equals(id));
	}

	public virtual bool ItemIsInSomeEquipPont(string itemName)
	{
		return equipPoints.Exists((EquipPoint ep) => ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.name.Equals(itemName));
	}

	public virtual bool ItemIsInSpecificEquipPoint(int id, string equipPointName)
	{
		return equipPoints.Exists((EquipPoint ep) => ep.equipPointName.Equals(equipPointName) && ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.id.Equals(id));
	}

	public virtual bool ItemIsInSpecificEquipPoint(string itemName, string equipPointName)
	{
		return equipPoints.Exists((EquipPoint ep) => ep.equipPointName.Equals(equipPointName) && ep.equipmentReference != null && ep.equipmentReference.item != null && ep.equipmentReference.item.name.Equals(itemName));
	}

	public virtual int GetAllAmount(int id)
	{
		List<vItem> list = GetItems(id);
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			num += list[i].amount;
		}
		return num;
	}

	public virtual List<vItem> GetItems()
	{
		return items;
	}

	public virtual List<vItem> GetAllItems()
	{
		if (!itemListData)
		{
			return null;
		}
		return itemListData.items;
	}

	public virtual vItem GetItem(int id)
	{
		return items.Find((vItem i) => i.id == id);
	}

	public virtual vItem GetItem(string itemName)
	{
		return items.Find((vItem i) => i.name == itemName);
	}

	public virtual vItem GetItemInEquipPoint(string equipPointName)
	{
		EquipPoint equipPoint = equipPoints.Find((EquipPoint ep) => ep.equipPointName.Equals(equipPointName));
		if (equipPoint != null && equipPoint.equipmentReference != null && (bool)equipPoint.equipmentReference.item)
		{
			return equipPoint.equipmentReference.item;
		}
		return null;
	}

	public virtual List<vItem> GetItems(int id)
	{
		return items.FindAll((vItem i) => i.id == id);
	}

	public virtual List<vItem> GetItems(string itemName)
	{
		return items.FindAll((vItem i) => i.name == itemName);
	}

	public virtual List<vItem> GetItemsInEquipArea(int indexOfArea)
	{
		List<vItem> list = new List<vItem>();
		if (!inventory || inventory.equipAreas.Length == 0 || indexOfArea > inventory.equipAreas.Length - 1)
		{
			return list;
		}
		List<vEquipSlot> validSlots = inventory.equipAreas[indexOfArea].ValidSlots;
		for (int i = 0; i < validSlots.Count; i++)
		{
			if (validSlots[i].item != null)
			{
				list.Add(validSlots[i].item);
			}
		}
		return list;
	}

	public virtual List<vItem> GetAllItemInAllEquipAreas()
	{
		List<vItem> list = new List<vItem>();
		if (!inventory || inventory.equipAreas.Length == 0)
		{
			return list;
		}
		for (int i = 0; i < inventory.equipAreas.Length; i++)
		{
			List<vEquipSlot> validSlots = inventory.equipAreas[i].ValidSlots;
			for (int j = 0; j < validSlots.Count; j++)
			{
				if (validSlots[j].item != null)
				{
					list.Add(validSlots[j].item);
				}
			}
		}
		return list;
	}

	protected vEquipment EquipEquipment(vItem item, bool startActive = true)
	{
		if (equipments.ContainsKey(item))
		{
			if (!startActive)
			{
				if (debugMode)
				{
					Debug.Log($"<color=green>Disable Equipment {equipments[item].gameObject} </color>");
				}
				equipments[item].gameObject.SetActive(value: false);
			}
			else
			{
				if (debugMode)
				{
					Debug.Log($"<color=green>Enable Equipment {equipments[item].gameObject} </color>");
				}
				equipments[item].gameObject.SetActive(value: true);
			}
			return equipments[item];
		}
		if ((bool)item.originalObject)
		{
			vEquipment component = item.originalObject.GetComponent<vEquipment>();
			if (component != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(item.originalObject);
				if (!startActive)
				{
					if (debugMode)
					{
						Debug.Log($"<color=green>Instantiate and disable Equipment {gameObject.gameObject} </color>");
					}
					gameObject.gameObject.SetActive(value: false);
				}
				else if (debugMode)
				{
					Debug.Log($"<color=green>Instantiate and enable Equipment {gameObject.gameObject} </color>");
				}
				gameObject.transform.SetParent(equipmentContainer.transform);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localEulerAngles = Vector3.zero;
				component = gameObject.GetComponent<vEquipment>();
				equipments.Add(item, component);
				equipmentsObject.Add(gameObject, component);
				return component;
			}
		}
		return null;
	}

	protected vEquipment EquipEquipment(vItem item, Vector3 position, Quaternion rotation, Transform parent = null)
	{
		vEquipment vEquipment2 = EquipEquipment(item);
		if ((bool)vEquipment2)
		{
			if ((bool)parent)
			{
				vEquipment2.transform.parent = parent;
			}
			vEquipment2.transform.position = position;
			vEquipment2.transform.rotation = rotation;
			vEquipment2.OnEquip(item);
			return vEquipment2;
		}
		return null;
	}

	protected void UnequipEquipment(vItem item)
	{
		if (equipments.ContainsKey(item))
		{
			if (debugMode)
			{
				Debug.Log($"<color=red>Disable Equipment {equipments[item].gameObject} </color>");
			}
			equipments[item].gameObject.SetActive(value: false);
			equipments[item].gameObject.transform.SetParent(equipmentContainer.transform);
			equipments[item].gameObject.transform.localPosition = Vector3.zero;
			equipments[item].gameObject.transform.localEulerAngles = Vector3.zero;
			equipments[item].equipPoint = null;
			equipments[item].OnUnequip(item);
		}
	}

	public virtual void AddItem(ItemReference itemReference, bool ignoreItemAnimation = false, UnityAction<vItem> onFinish = null)
	{
		if (itemReference != null && itemListData != null && itemListData.items.Count > 0)
		{
			vItem item = itemListData.items.Find((vItem t) => t.id.Equals(itemReference.id));
			if ((bool)item)
			{
				List<vItem> list = items.FindAll((vItem i) => i.stackable && i.id == item.id && i.amount < i.maxStack);
				if (list.Count == 0)
				{
					vItem vItem2 = UnityEngine.Object.Instantiate(item);
					vItem2.name = vItem2.name.Replace("(Clone)", string.Empty);
					if (itemReference.attributes != null && vItem2.attributes != null && item.attributes.Count == itemReference.attributes.Count)
					{
						for (int j = 0; j < vItem2.attributes.Count; j++)
						{
							itemReference.attributes[j].CopyTo(vItem2.attributes[j]);
						}
					}
					vItem2.amount = 0;
					for (int k = 0; k < item.maxStack; k++)
					{
						if (vItem2.amount >= vItem2.maxStack)
						{
							break;
						}
						if (itemReference.amount <= 0)
						{
							break;
						}
						vItem2.amount++;
						itemReference.amount--;
					}
					items.Add(vItem2);
					onAddItem.Invoke(vItem2);
					onAddItemID.Invoke(vItem2.id);
					if (itemReference.addToEquipArea)
					{
						itemReference.addToEquipArea = false;
						AutoEquipItem(vItem2, itemReference.indexArea, itemReference.autoEquip, ignoreItemAnimation);
					}
					if (itemReference.amount > 0)
					{
						AddItem(itemReference, ignoreItemAnimation: false, onFinish);
					}
					else
					{
						onFinish?.Invoke(vItem2);
					}
				}
				else
				{
					int index = items.IndexOf(list[0]);
					bool flag = false;
					for (int l = 0; l < items[index].maxStack; l++)
					{
						if (items[index].amount >= items[index].maxStack)
						{
							break;
						}
						if (itemReference.amount <= 0)
						{
							break;
						}
						items[index].amount++;
						itemReference.amount--;
						flag = true;
					}
					if (flag)
					{
						onChangeItemAmount.Invoke(items[index]);
					}
					if (itemReference.amount > 0)
					{
						AddItem(itemReference, ignoreItemAnimation: false, onFinish);
					}
					else if (flag)
					{
						onFinish?.Invoke(items[index]);
					}
				}
			}
		}
		inventory.UpdateInventory();
	}

	public virtual void AutoEquipItem(vItem item, int indexArea, bool autoEquip = false, bool ignoreItemAnimation = true)
	{
		if (!inventory)
		{
			return;
		}
		if (inventory.equipAreas != null && inventory.equipAreas.Length != 0 && indexArea < inventory.equipAreas.Length)
		{
			vEquipSlot vEquipSlot2 = inventory.equipAreas[indexArea].equipSlots.Find((vEquipSlot slot) => slot.isValid && slot.item == null && slot.itemType.Contains(item.type));
			if (vEquipSlot2 == null && autoEquip && (bool)inventory.equipAreas[indexArea].currentEquippedSlot && inventory.equipAreas[indexArea].currentEquippedSlot.item == null)
			{
				vEquipSlot2 = inventory.equipAreas[indexArea].currentEquippedSlot;
			}
			if ((bool)vEquipSlot2 && !inventory.equipAreas[indexArea].equipSlots.Exists((vEquipSlot slot) => slot.item == item))
			{
				int indexOfSlot = inventory.equipAreas[indexArea].equipSlots.IndexOf(vEquipSlot2);
				if (vEquipSlot2.item != item)
				{
					EquipItemToEquipSlot(indexArea, indexOfSlot, item, autoEquip, ignoreItemAnimation);
				}
			}
		}
		else if (debugMode)
		{
			Debug.LogWarning("Fail to auto equip " + item.name + " on equipArea " + indexArea);
		}
	}

	protected virtual void EquipItem(vEquipArea equipArea, vItem item)
	{
		CheckIsLockedToEquip();
		if (!item)
		{
			return;
		}
		item.isEquiped = true;
		onEquipItem.Invoke(equipArea, item);
		if (debugMode)
		{
			Debug.Log($"<color=green>Start Equip {item} </color>");
		}
		inventory.UpdateInventory();
		if (item != equipArea.currentEquippedItem)
		{
			if (debugMode)
			{
				Debug.Log($"<color=green>Not Current Equip {item} </color>{equipArea.indexOfEquippedItem}", equipArea.currentEquippedItem);
			}
			EquipEquipment(item, startActive: false);
			onFinishEquipItem?.Invoke(equipArea, item);
			if (debugMode)
			{
				Debug.Log($"<color=green>Finish Equip {item} </color>");
			}
			return;
		}
		EquipPoint equipPoint = equipPoints.Find((EquipPoint ep) => ep.equipPointName == equipArea.equipPointName);
		if (equipPoint != null && item != null && equipPoint.equipmentReference.item != item && (bool)item.originalObject && item.originalObject.GetComponentInChildren<vEquipment>() != null)
		{
			equipPoint.area = equipArea;
			StartCoroutine(EquipItemRoutine(equipPoint, item, delegate
			{
				onFinishEquipItem?.Invoke(equipArea, item);
			}));
		}
	}

	private IEnumerator EquipItemRoutine(EquipPoint equipPoint, vItem item, UnityAction onFinish)
	{
		LockInventoryInput(value: true);
		while (inEquip || IsAnimatorTag("IsEquipping"))
		{
			yield return new WaitForEndOfFrame();
		}
		if (!equipPoint.area.isLockedToEquip && playItemAnimation)
		{
			if (debugMode)
			{
				Debug.Log($"<color=green>Play Equip Animation {item} </color>");
			}
			equipTimer = item.enableDelayTime;
			animator.SetBool("FlipEquip", equipPoint.equipPointName.Contains("Left"));
			animator.CrossFade(item.EnableAnim, 0.25f);
		}
		if (!inEquip)
		{
			inEquip = true;
			inventory.canEquip = false;
			if (equipPoint != null)
			{
				if ((bool)item.originalObject)
				{
					if (equipPoint.equipmentReference != null && equipPoint.equipmentReference.equipedObject != null && (bool)equipPoint.equipmentReference.item)
					{
						UnequipEquipment(equipPoint.equipmentReference.item);
						equipPoint.equipmentReference.item = null;
					}
					if (!equipPoint.area.isLockedToEquip && playItemAnimation && !string.IsNullOrEmpty(item.EnableAnim))
					{
						if (debugMode && equipTimer > 0f)
						{
							Debug.Log($"<color=green>In Equip delay {item} </color>");
						}
						while (equipTimer > 0f && !(item == null))
						{
							yield return null;
							equipTimer -= vTime.deltaTime;
						}
					}
					inEquip = false;
					Transform transform = equipPoint.handler.customHandlers.Find((Transform p) => p.name == item.customHandler);
					Transform transform2 = ((transform != null) ? transform : equipPoint.handler.defaultHandler);
					vEquipment vEquipment2 = EquipEquipment(item, transform2.position, transform2.rotation, transform2);
					if (equipPoint.area.isLockedToEquip)
					{
						vEquipment2.gameObject.SetActive(value: false);
					}
					vEquipment2.equipPoint = equipPoint;
					equipPoint.equipmentReference.item = item;
					equipPoint.equipmentReference.equipedObject = vEquipment2.gameObject;
					equipPoint.onInstantiateEquiment.Invoke(vEquipment2.gameObject);
				}
				else if (equipPoint.equipmentReference != null && equipPoint.equipmentReference.equipedObject != null && (bool)equipPoint.equipmentReference.item)
				{
					UnequipEquipment(equipPoint.equipmentReference.item);
					equipments[equipPoint.equipmentReference.item].equipPoint = null;
					equipPoint.equipmentReference.item = null;
				}
			}
		}
		LockInventoryInput(value: false);
		onFinish?.Invoke();
		if (debugMode)
		{
			Debug.Log($"<color=green>Finish Equip {item} </color>");
		}
		inEquip = false;
		inventory.canEquip = true;
	}

	public virtual void EquipItemToEquipSlot(int indexOfArea, int indexOfSlot, vItem item, bool autoEquip = false, bool ignoreItemAnimation = false)
	{
		if (!inventory)
		{
			return;
		}
		if (ignoreItemAnimation)
		{
			temporarilyIgnoreItemAnimation = ignoreItemAnimation;
		}
		if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
		{
			vEquipArea vEquipArea2 = inventory.equipAreas[indexOfArea];
			if (vEquipArea2 != null)
			{
				vEquipArea2.AddItemToEquipSlot(indexOfSlot, item, autoEquip);
			}
		}
		if (ignoreItemAnimation)
		{
			temporarilyIgnoreItemAnimation = false;
		}
	}

	public virtual void EquipItemToCurrentEquipSlot(vItem item, int indexOfArea, bool ignoreItemAnimation = true)
	{
		if ((bool)inventory || items.Count != 0)
		{
			if (ignoreItemAnimation)
			{
				temporarilyIgnoreItemAnimation = ignoreItemAnimation;
			}
			if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
			{
				inventory.equipAreas[indexOfArea].AddCurrentItem(item);
			}
			if (ignoreItemAnimation)
			{
				temporarilyIgnoreItemAnimation = false;
			}
		}
	}

	protected virtual void UnequipItem(vEquipArea equipArea, vItem item)
	{
		if (!item)
		{
			return;
		}
		item.isEquiped = false;
		onUnequipItem.Invoke(equipArea, item);
		if (debugMode)
		{
			Debug.Log($"<color=red>Start Unequip {item}</color>");
		}
		EquipPoint equipPoint = equipPoints.Find((EquipPoint ep) => ep.equipPointName == equipArea.equipPointName && ep.equipmentReference.item != null && ep.equipmentReference.item == item);
		if (equipPoint != null && item != null)
		{
			equipPoint.onInstantiateEquiment.Invoke(null);
			unequipTimer = item.disableDelayTime;
			if ((bool)item.originalObject && item.originalObject.GetComponentInChildren<vEquipment>() != null)
			{
				if (!inventory.isOpen && playItemAnimation && !inEquip && equipPoint.equipmentReference.equipedObject.activeInHierarchy)
				{
					if (debugMode)
					{
						Debug.Log($"<color=red>Play Unequip Animation {item}</color>");
					}
					animator.SetBool("FlipEquip", equipArea.equipPointName.Contains("Left"));
					animator.CrossFade(item.DisableAnim, 0.25f);
				}
				StartCoroutine(UnequipItemRoutine(equipPoint, item, delegate
				{
					onFinishUnequipItem?.Invoke(equipArea, item);
				}));
			}
		}
		else if (item != null)
		{
			if (debugMode)
			{
				Debug.Log($"<color=red>Finish Unequip {item}</color>");
			}
			onFinishUnequipItem.Invoke(equipArea, item);
		}
		inventory.UpdateInventory();
		CheckIsLockedToEquip();
	}

	public virtual void UnequipItem(vItem item, bool ignoreItemAnimation = true)
	{
		vEquipArea vEquipArea2 = Array.Find(inventory.equipAreas, (vEquipArea e) => e.ValidSlots.Exists((vEquipSlot s) => s.item != null && s.item.id.Equals(item.id)));
		if (vEquipArea2 != null)
		{
			if (ignoreItemAnimation)
			{
				temporarilyIgnoreItemAnimation = ignoreItemAnimation;
			}
			UnequipItem(vEquipArea2, item);
		}
		inventory.UpdateInventory();
		if (ignoreItemAnimation)
		{
			temporarilyIgnoreItemAnimation = false;
		}
	}

	public virtual void UnequipItemOfEquipSlot(int indexOfArea, int indexOfSlot, bool ignoreItemAnimation = true)
	{
		if (!inventory)
		{
			return;
		}
		if (ignoreItemAnimation)
		{
			temporarilyIgnoreItemAnimation = ignoreItemAnimation;
		}
		if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
		{
			vEquipArea vEquipArea2 = inventory.equipAreas[indexOfArea];
			if (vEquipArea2 != null)
			{
				vEquipArea2.RemoveItemOfEquipSlot(indexOfSlot);
			}
		}
		if (ignoreItemAnimation)
		{
			temporarilyIgnoreItemAnimation = false;
		}
	}

	public virtual void UnequipCurrentEquipedItem(int indexOfArea, bool ignoreItemAnimation = true)
	{
		if ((bool)inventory || items.Count != 0)
		{
			if (ignoreItemAnimation)
			{
				temporarilyIgnoreItemAnimation = ignoreItemAnimation;
			}
			if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
			{
				inventory.equipAreas[indexOfArea].RemoveCurrentItem();
			}
			if (ignoreItemAnimation)
			{
				temporarilyIgnoreItemAnimation = false;
			}
		}
	}

	private IEnumerator UnequipItemRoutine(EquipPoint equipPoint, vItem item, UnityAction onFinish)
	{
		LockInventoryInput(value: true);
		if (!inEquip)
		{
			inEquip = true;
			inventory.canEquip = false;
			if (equipPoint != null && equipPoint.equipmentReference != null && equipPoint.equipmentReference.equipedObject != null)
			{
				if (!inventory.isOpen && playItemAnimation)
				{
					if (debugMode && unequipTimer > 0f)
					{
						Debug.Log($"<color=red>In Unequip delay {item} </color>");
					}
					while (unequipTimer > 0f && !string.IsNullOrEmpty(item.DisableAnim))
					{
						unequipTimer -= vTime.deltaTime;
						yield return null;
					}
				}
				if (equipPoint != null && equipPoint.equipmentReference != null && (bool)equipPoint.equipmentReference.equipedObject)
				{
					UnequipEquipment(item);
					equipPoint.equipmentReference.item = null;
				}
			}
			inEquip = false;
			inventory.canEquip = true;
		}
		if (debugMode)
		{
			Debug.Log($"<color=red>Finish Unequip {item}</color>");
		}
		onFinish?.Invoke();
		LockInventoryInput(value: false);
	}

	public bool CanUseItem(vItem item)
	{
		if (this.canUseItemDelegate != null)
		{
			List<bool> validationList = new List<bool>();
			this.canUseItemDelegate(item, ref validationList);
			return !validationList.Contains(item: false);
		}
		return item.canBeUsed;
	}

	protected virtual void UseItem(vItem item)
	{
		if ((bool)item)
		{
			if (CanUseItem(item))
			{
				StartCoroutine(UseItemRoutine(item));
			}
			else
			{
				onUseItemFail.Invoke(item);
			}
		}
	}

	protected IEnumerator UseItemRoutine(vItem item)
	{
		usingItem = true;
		LockInventoryInput(value: true);
		onStartItemUsage.Invoke(item);
		bool canUse = CanUseItem(item);
		if (canUse)
		{
			float time = item.enableDelayTime;
			if (!inventory.isOpen && playItemAnimation && !string.IsNullOrEmpty(item.EnableAnim))
			{
				animator.SetBool("FlipAnimation", value: false);
				animator.CrossFade(item.EnableAnim, 0.25f);
				while (usingItem && time > 0f && canUse)
				{
					canUse = CanUseItem(item);
					time -= vTime.deltaTime;
					yield return null;
				}
			}
			if (usingItem && canUse)
			{
				if (item.destroyAfterUse)
				{
					item.amount--;
				}
				onUseItem.Invoke(item);
				if (item.attributes != null && item.attributes.Count > 0 && applyAttributeEvents.Count > 0)
				{
					foreach (ApplyAttributeEvent attributeEvent in applyAttributeEvents)
					{
						foreach (vItemAttribute item2 in item.attributes.FindAll((vItemAttribute a) => a.name.Equals(attributeEvent.attribute)))
						{
							attributeEvent.onApplyAttribute.Invoke(item2.value);
						}
					}
				}
				if (item.destroyAfterUse && item.amount <= 0 && items.Contains(item))
				{
					DestroyItem(item);
				}
				else
				{
					onRemoveItemID.Invoke(item.id);
				}
				usingItem = false;
				inventory.CheckEquipmentChanges();
			}
			else
			{
				onUseItemFail.Invoke(item);
			}
		}
		else
		{
			onUseItemFail.Invoke(item);
		}
		LockInventoryInput(value: false);
		inventory.UpdateInventory();
	}

	public virtual void DestroyItem(vItem item)
	{
		DestroyItem(item, item.amount);
	}

	public virtual void DestroyItem(vItem item, int amount)
	{
		item.amount -= amount;
		onDestroyItem.Invoke(item, amount);
		if (item.amount <= 0)
		{
			vEquipArea vEquipArea2 = Array.Find(inventory.equipAreas, (vEquipArea e) => e.ValidSlots.Exists((vEquipSlot s) => s.item != null && s.item.id.Equals(item.id)));
			if (vEquipArea2 != null)
			{
				temporarilyIgnoreItemAnimation = true;
				vEquipArea2.UnequipItem(item);
				temporarilyIgnoreItemAnimation = false;
			}
			int id = item.id;
			if (items.Contains(item))
			{
				items.Remove(item);
			}
			if (equipments.ContainsKey(item))
			{
				vEquipment vEquipment2 = equipments[item];
				if (vEquipment2 != null)
				{
					UnityEngine.Object.Destroy(vEquipment2.gameObject);
				}
				equipments.Remove(item);
			}
			UnityEngine.Object.Destroy(item);
			onRemoveItemID.Invoke(id);
		}
		inventory.UpdateInventory();
	}

	public virtual void DestroyAllItems()
	{
		for (int num = items.Count - 1; num >= 0; num--)
		{
			DestroyItem(items[num]);
		}
	}

	public virtual void DestroyCurrentEquipedItem(int indexOfArea, bool ignoreItemAnimation = true)
	{
		if (!inventory && items.Count == 0)
		{
			return;
		}
		if (ignoreItemAnimation)
		{
			temporarilyIgnoreItemAnimation = ignoreItemAnimation;
		}
		if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
		{
			vItem currentEquippedItem = inventory.equipAreas[indexOfArea].currentEquippedItem;
			if ((bool)currentEquippedItem)
			{
				DestroyItem(currentEquippedItem, currentEquippedItem.amount);
			}
		}
		if (ignoreItemAnimation)
		{
			temporarilyIgnoreItemAnimation = false;
		}
	}

	public virtual void DropItem(vItem item)
	{
		DropItem(item, item.amount);
	}

	public virtual void DropItem(vItem item, int amount)
	{
		item.amount -= amount;
		if (item.dropObject != null)
		{
			vItemCollection component = UnityEngine.Object.Instantiate(item.dropObject, base.transform.position, base.transform.rotation).GetComponent<vItemCollection>();
			if (component != null)
			{
				component.items.Clear();
				ItemReference itemReference = new ItemReference(item.id);
				itemReference.amount = amount;
				itemReference.attributes = new List<vItemAttribute>(item.attributes);
				component.items.Add(itemReference);
			}
		}
		onDropItem.Invoke(item, amount);
		if (item.amount <= 0 && items.Contains(item))
		{
			vEquipArea vEquipArea2 = Array.Find(inventory.equipAreas, (vEquipArea e) => e.ValidSlots.Exists((vEquipSlot s) => s.item != null && s.item.id.Equals(item.id)));
			if (vEquipArea2 != null)
			{
				vEquipArea2.UnequipItem(item);
			}
			items.Remove(item);
			DestroyItem(item);
		}
		inventory.UpdateInventory();
	}

	public virtual void DropAllItens()
	{
		List<ItemReference> list = new List<ItemReference>();
		List<GameObject> list2 = new List<GameObject>();
		for (int num = items.Count - 1; num >= 0; num--)
		{
			vItem item = items[num];
			ItemReference itemReference = list.Find((ItemReference _item) => _item.id == item.id);
			if (itemReference == null)
			{
				itemReference = new ItemReference(item.id);
				list.Add(itemReference);
				list2.Add(item.dropObject);
			}
			itemReference.amount += item.amount;
			DestroyItem(item);
		}
		for (int i = 0; i < list2.Count; i++)
		{
			GameObject gameObject = list2[i];
			ItemReference item2 = list[i];
			if ((bool)gameObject)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, base.transform.position, base.transform.rotation);
				vItemCollection vItemCollection2 = gameObject2.GetComponent<vItemCollection>();
				if (!vItemCollection2)
				{
					vItemCollection2 = gameObject2.AddComponent<vItemCollection>();
				}
				if (vItemCollection2 != null)
				{
					vItemCollection2.items.Clear();
					vItemCollection2.items.Add(item2);
				}
			}
		}
	}

	public virtual void DropCurrentEquippedItem(int indexOfArea, bool ignoreItemAnimation = true)
	{
		if (!inventory && items.Count == 0)
		{
			return;
		}
		if (ignoreItemAnimation)
		{
			temporarilyIgnoreItemAnimation = ignoreItemAnimation;
		}
		if (inventory.equipAreas != null && indexOfArea < inventory.equipAreas.Length)
		{
			vItem currentEquippedItem = inventory.equipAreas[indexOfArea].currentEquippedItem;
			if ((bool)currentEquippedItem)
			{
				DropItem(currentEquippedItem, currentEquippedItem.amount);
			}
		}
		if (ignoreItemAnimation)
		{
			temporarilyIgnoreItemAnimation = false;
		}
	}

	public virtual void CollectItem(ItemReference itemRef, float onCollectDelay = 0f, float textDelay = 0f, bool ignoreItemAnimation = true)
	{
		if (!inCollectItemRoutine)
		{
			itemsToCollect.Add(itemRef);
			StartCoroutine(CollectItemsRoutine(onCollectDelay, textDelay, ignoreItemAnimation));
		}
		else
		{
			itemsToCollect.Add(itemRef);
		}
	}

	public virtual void CollectItems(List<ItemReference> collection, float onCollectDelay = 0f, float textDelay = 0f, bool ignoreItemAnimation = true)
	{
		foreach (ItemReference item in collection)
		{
			CollectItem(item, onCollectDelay, textDelay, ignoreItemAnimation);
		}
	}

	protected virtual IEnumerator CollectItemsRoutine(float onCollectDelay = 0f, float textDelay = 0f, bool ignoreItemAnimation = true)
	{
		while (inCollectItemRoutine)
		{
			yield return null;
		}
		inCollectItemRoutine = true;
		yield return new WaitForSeconds(onCollectDelay);
		List<CollectedItemInfo> collectedItems = new List<CollectedItemInfo>();
		for (int j = 0; j < inventory.equipAreas.Length; j++)
		{
			inventory.equipAreas[j].ignoreEquipEvents = true;
		}
		for (int k = 0; k < itemsToCollect.Count; k++)
		{
			ItemReference itemReference = itemsToCollect[k];
			CollectedItemInfo collectedItemInfo = default(CollectedItemInfo);
			collectedItemInfo.amount = itemsToCollect[k].amount;
			UnityAction<vItem> equipAction = null;
			if (itemReference.addToEquipArea)
			{
				bool autoEquip = itemsToCollect[k].autoEquip;
				int indexOfArea = itemsToCollect[k].indexArea;
				itemsToCollect[k].addToEquipArea = false;
				itemsToCollect[k].autoEquip = false;
				equipAction = delegate(vItem _item)
				{
					AutoEquipItem(_item, indexOfArea, autoEquip);
				};
			}
			UnityAction<vItem> onFinish = delegate(vItem _item)
			{
				collectedItemInfo.item = _item;
				collectedItems.Add(collectedItemInfo);
				equipAction?.Invoke(_item);
			};
			AddItem(itemsToCollect[k].Clone(), ignoreItemAnimation, onFinish);
		}
		inCollectItemRoutine = false;
		itemsToCollect.Clear();
		StartCoroutine(DisplayCollectedItems(textDelay, collectedItems.ToArray()));
		new List<CollectedItemInfo>();
		if (ignoreItemAnimation)
		{
			temporarilyIgnoreItemAnimation = true;
		}
		for (int i = 0; i < inventory.equipAreas.Length; i++)
		{
			vEquipArea area = inventory.equipAreas[i];
			if (area.isLockedToEquip)
			{
				temporarilyIgnoreItemAnimation = true;
			}
			else
			{
				while (!ignoreItemAnimation && (inEquip || IsAnimatorTag("IsEquipping")))
				{
					yield return null;
				}
			}
			area.ignoreEquipEvents = false;
			area.EquipCurrentSlot();
			if (area.isLockedToEquip)
			{
				temporarilyIgnoreItemAnimation = false;
			}
		}
		if (ignoreItemAnimation)
		{
			temporarilyIgnoreItemAnimation = false;
		}
	}

	protected virtual IEnumerator DisplayCollectedItems(float delay, params CollectedItemInfo[] _items)
	{
		for (int i = 0; i < _items.Length; i++)
		{
			onCollectItem.Invoke(_items[i]);
			if ((bool)vItemCollectionDisplay.Instance)
			{
				string message = $"Acquired: {_items[i].amount} {_items[i].item.name}";
				vItemCollectionDisplay.Instance.FadeText(message, 4f, 0.25f);
			}
			yield return new WaitForSeconds(delay);
		}
	}

	public virtual void OnReceiveAction(vTriggerGenericAction action)
	{
		vItemCollection componentInChildren = action.GetComponentInChildren<vItemCollection>();
		if (componentInChildren != null && componentInChildren.items.Count > 0)
		{
			List<ItemReference> collection = componentInChildren.items.vCopy();
			CollectItems(collection, componentInChildren.onCollectDelay, componentInChildren.textDelay, componentInChildren.ignoreItemAnimation);
		}
	}

	[SpecialName]
	bool IActionController.get_enabled()
	{
		return base.enabled;
	}

	[SpecialName]
	void IActionController.set_enabled(bool value)
	{
		base.enabled = value;
	}

	[SpecialName]
	GameObject IActionController.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	Transform IActionController.get_transform()
	{
		return base.transform;
	}

	[SpecialName]
	string IActionController.get_name()
	{
		return base.name;
	}

	Type IActionController.GetType()
	{
		return GetType();
	}
}
