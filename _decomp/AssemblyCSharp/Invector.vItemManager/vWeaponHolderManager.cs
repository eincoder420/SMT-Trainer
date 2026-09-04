using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager;

[vClassHeader("Weapon Holder Manager", "Create a new empty object inside a bone and add the vWeaponHolder script")]
public class vWeaponHolderManager : vMonoBehaviour
{
	public vWeaponHolder[] holders = new vWeaponHolder[0];

	public bool debugMode;

	internal bool inEquip;

	internal bool inUnequip;

	internal vItemManager itemManager;

	internal vThirdPersonController cc;

	public Dictionary<string, List<vWeaponHolder>> holderAreas = new Dictionary<string, List<vWeaponHolder>>();

	protected float equipTime;

	protected virtual bool IsEquipping
	{
		get
		{
			if ((bool)cc)
			{
				return cc.IsAnimatorTag("IsEquipping");
			}
			return false;
		}
	}

	private void OnDrawGizmosSelected()
	{
		holders = GetComponentsInChildren<vWeaponHolder>(includeInactive: true);
	}

	private void Start()
	{
		itemManager = GetComponent<vItemManager>();
		cc = GetComponent<vThirdPersonController>();
		if (!itemManager)
		{
			return;
		}
		itemManager.onFinishEquipItem.AddListener(EquipWeapon);
		itemManager.onFinishUnequipItem.AddListener(UnequipWeapon);
		itemManager.onSetLockedToEquip.AddListener(CheckAreaLockedToEquip);
		holders = GetComponentsInChildren<vWeaponHolder>(includeInactive: true);
		if (holders == null)
		{
			return;
		}
		vWeaponHolder[] array = holders;
		foreach (vWeaponHolder vWeaponHolder in array)
		{
			if (!holderAreas.ContainsKey(vWeaponHolder.equipPointName))
			{
				holderAreas.Add(vWeaponHolder.equipPointName, new List<vWeaponHolder>());
				holderAreas[vWeaponHolder.equipPointName].Add(vWeaponHolder);
			}
			else
			{
				holderAreas[vWeaponHolder.equipPointName].Add(vWeaponHolder);
			}
			vWeaponHolder.SetActiveHolder(active: false);
			vWeaponHolder.SetActiveWeapon(active: false);
		}
	}

	public void CheckAreaLockedToEquip(vEquipArea equipArea)
	{
		if (equipArea.isLockedToEquip)
		{
			UnequipWeapon(equipArea, equipArea.currentEquippedItem);
		}
	}

	public void EquipWeapon(vEquipArea equipArea, vItem item)
	{
		if (item == null)
		{
			return;
		}
		List<vEquipSlot> slotsInArea = equipArea.ValidSlots;
		if (slotsInArea == null || slotsInArea.Count <= 0 || !holderAreas.ContainsKey(equipArea.equipPointName))
		{
			return;
		}
		int i;
		for (i = 0; i < slotsInArea.Count; i++)
		{
			if (!(slotsInArea[i].item != null))
			{
				continue;
			}
			vWeaponHolder vWeaponHolder = holderAreas[equipArea.equipPointName].Find((vWeaponHolder h) => (bool)slotsInArea[i].item && slotsInArea[i].item.id == h.itemID && ((equipArea.currentEquippedItem != null && equipArea.currentEquippedItem != item && equipArea.currentEquippedItem != slotsInArea[i].item && equipArea.currentEquippedItem.id != item.id) || equipArea.currentEquippedItem == null || equipArea.isLockedToEquip));
			if ((bool)vWeaponHolder)
			{
				vWeaponHolder.SetActiveHolder(active: true);
				vWeaponHolder.SetActiveWeapon(active: true);
				if (debugMode)
				{
					Debug.Log("Hold: " + slotsInArea[i].item);
				}
			}
		}
		if (equipArea.isLockedToEquip || !(equipArea.currentEquippedItem != null) || !(equipArea.currentEquippedItem == item))
		{
			return;
		}
		vWeaponHolder vWeaponHolder2 = holderAreas[equipArea.equipPointName].Find((vWeaponHolder h) => h.itemID == equipArea.currentEquippedItem.id);
		if ((bool)vWeaponHolder2)
		{
			if (equipArea.currentEquippedItem != item || itemManager.playItemAnimation)
			{
				_ = !string.IsNullOrEmpty(equipArea.currentEquippedItem.EnableAnim);
			}
			else
				_ = 0;
			if (debugMode)
			{
				Debug.Log("UnHold: " + item.name);
			}
			vWeaponHolder2.SetActiveHolder(active: true);
			vWeaponHolder2.SetActiveWeapon(active: false);
		}
	}

	public void UnequipWeapon(vEquipArea equipArea, vItem item)
	{
		if (holders.Length == 0 || item == null || !(itemManager.inventory != null) || !holderAreas.ContainsKey(equipArea.equipPointName))
		{
			return;
		}
		vWeaponHolder vWeaponHolder = holderAreas[equipArea.equipPointName].Find((vWeaponHolder h) => item.id == h.itemID);
		if ((bool)vWeaponHolder)
		{
			bool flag = equipArea.ValidSlots.Find((vEquipSlot slot) => slot.item == item) != null;
			if (debugMode)
			{
				Debug.Log(flag ? ("Hold: " + item.name) : ("Hide :" + item.name + " Holder"));
			}
			if (flag)
			{
				vWeaponHolder.SetActiveHolder(flag);
				vWeaponHolder.SetActiveWeapon(flag);
			}
			else
			{
				vWeaponHolder.SetActiveHolder(active: false);
				vWeaponHolder.SetActiveWeapon(active: false);
			}
		}
	}

	internal vWeaponHolder GetHolder(GameObject equipment, int id)
	{
		EquipPoint equipPoint = itemManager.equipPoints.Find((EquipPoint e) => e.equipmentReference != null && (bool)e.equipmentReference.item && e.equipmentReference.item.id == id && e.equipmentReference.equipedObject == equipment);
		if (equipPoint != null && holderAreas.ContainsKey(equipPoint.equipPointName))
		{
			return holderAreas[equipPoint.equipPointName].Find((vWeaponHolder h) => id == h.itemID);
		}
		if (debugMode)
		{
			Debug.LogWarning(ToString() + " fail to find a holder with equipPointName " + equipPoint.equipPointName);
		}
		return null;
	}

	internal IEnumerator UnequipRoutine(float equipDelay, bool immediate = false, UnityAction onStart = null, UnityAction onFinish = null, string itemName = "")
	{
		if (debugMode)
		{
			Debug.Log("Start Unequip: " + itemName);
		}
		if (!immediate && !inEquip)
		{
			inUnequip = true;
		}
		while (!IsEquipping && !immediate && !inEquip)
		{
			yield return null;
		}
		onStart?.Invoke();
		if (!inEquip && !immediate)
		{
			float equipTime = equipDelay;
			while (!immediate && !inEquip && equipTime > 0f)
			{
				equipTime -= vTime.deltaTime;
				yield return null;
			}
		}
		inUnequip = false;
		onFinish?.Invoke();
		if (debugMode)
		{
			Debug.Log("Finish Unequip: " + itemName);
		}
	}

	internal IEnumerator EquipRoutine(float equipDelay, bool immediate = false, UnityAction onStart = null, UnityAction onFinish = null, string itemName = "")
	{
		if (debugMode)
		{
			Debug.Log("Start Equip: " + itemName);
		}
		if (!immediate)
		{
			inEquip = true;
		}
		while (!IsEquipping && !immediate)
		{
			yield return null;
		}
		onStart?.Invoke();
		if (!inUnequip && !immediate)
		{
			float equipTime = equipDelay;
			while (!immediate && !inUnequip && equipTime > 0f)
			{
				equipTime -= vTime.deltaTime;
				yield return null;
			}
		}
		inEquip = false;
		onFinish?.Invoke();
		if (debugMode)
		{
			Debug.Log("Finish Equip: " + itemName);
		}
	}
}
