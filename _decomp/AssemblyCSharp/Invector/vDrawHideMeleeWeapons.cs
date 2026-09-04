using Invector.vCharacterController;
using Invector.vItemManager;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

[vClassHeader("Draw/Hide Melee Weapons", "This component works with vItemManager, vWeaponHolderManager and vMeleeCombatInput", useHelpBox = true)]
public class vDrawHideMeleeWeapons : vMonoBehaviour
{
	[vEditorToolbar("Default", false, "", false, false)]
	public bool hideWeaponsAutomatically = true;

	[vHideInInspector("hideWeaponsAutomatically", false)]
	public float hideWeaponsTimer = 5f;

	[vHelpBox("Set Lock input to Inventory when Lock method is called", vHelpBoxAttribute.MessageType.None)]
	public bool lockInventoryInputOnLock;

	[vReadOnly(true)]
	public bool isLocked;

	public GenericInput hideAndDrawWeaponsInput = new GenericInput("H", "LB", "LB");

	[vEditorToolbar("Melee", false, "", false, false)]
	[Header("Draw Immediate Conditions")]
	public bool meleeWeakAttack = true;

	public bool meleeStrongAttack = true;

	public bool meleeBlock = true;

	[vEditorToolbar("Debug", false, "", false, false)]
	[vReadOnly(false)]
	public bool weaponsHided;

	[vReadOnly(false)]
	public bool previouslyWeaponsHided;

	protected float currentTimer;

	protected bool forceHide;

	public virtual vMeleeCombatInput melee { get; set; }

	public virtual vWeaponHolderManager holderManager { get; set; }

	protected virtual bool IsEquipping
	{
		get
		{
			if (melee != null && (bool)melee.cc)
			{
				return melee.cc.IsAnimatorTag("IsEquipping");
			}
			return false;
		}
	}

	protected virtual void Start()
	{
		holderManager = GetComponent<vWeaponHolderManager>();
		melee = GetComponent<vMeleeCombatInput>();
		if ((bool)holderManager && (bool)melee)
		{
			melee.onUpdate -= ControlWeapons;
			melee.onUpdate += ControlWeapons;
			if (melee == null)
			{
				Debug.LogWarning("You're missing a vMeleeCombatInput, please add one", base.gameObject);
			}
		}
	}

	protected virtual void ControlWeapons()
	{
		if (!isLocked && !(melee.cc == null) && !melee.cc.customAction)
		{
			HandleInput();
			DrawWeaponsImmediateHandle();
			HideWeaponsAutomatically();
		}
	}

	protected virtual GameObject RightWeaponObject(bool checkIsActve = false)
	{
		if (!melee || !melee.meleeManager || !melee.meleeManager.rightWeapon || (checkIsActve && !melee.meleeManager.rightWeapon.gameObject.activeInHierarchy))
		{
			return null;
		}
		return melee.meleeManager.rightWeapon.gameObject;
	}

	protected virtual GameObject LeftWeaponObject(bool checkIsActve = false)
	{
		if (!melee || !melee.meleeManager || !melee.meleeManager.leftWeapon || (checkIsActve && !melee.meleeManager.leftWeapon.gameObject.activeInHierarchy))
		{
			return null;
		}
		return melee.meleeManager.leftWeapon.gameObject;
	}

	public virtual void ReturnToLastState(bool immediate = false)
	{
		if (previouslyWeaponsHided)
		{
			HideWeapons(immediate);
		}
		else
		{
			DrawWeapons(immediate);
		}
	}

	public virtual void LockDrawHideInput(bool value)
	{
		isLocked = value;
		if (lockInventoryInputOnLock && (bool)holderManager.itemManager)
		{
			holderManager.itemManager.LockInventoryInput(value);
		}
	}

	public virtual void HideWeapons(bool immediate = false)
	{
		previouslyWeaponsHided = weaponsHided;
		if (CanHideRightWeapon())
		{
			weaponsHided = true;
			HideRightWeapon(immediate);
		}
		else if (CanHideLeftWeapon())
		{
			weaponsHided = true;
			HideLeftWeapon(immediate);
		}
	}

	public virtual void ForceHideWeapons(bool immediate = false)
	{
		forceHide = true;
		HideWeapons(immediate);
		Invoke("ResetForceHide", 1f);
	}

	protected virtual void ResetForceHide()
	{
		forceHide = false;
	}

	public virtual void DrawWeapons(bool immediate = false)
	{
		if (CanDrawRightWeapon())
		{
			previouslyWeaponsHided = weaponsHided;
			weaponsHided = false;
			DrawRightWeapon(immediate);
		}
		else if (CanDrawLeftWeapon())
		{
			previouslyWeaponsHided = weaponsHided;
			weaponsHided = false;
			DrawLeftWeapon(immediate);
		}
	}

	protected virtual void HideWeaponsAutomatically()
	{
		if (hideWeaponsAutomatically)
		{
			if (HideTimerConditions())
			{
				currentTimer += Time.deltaTime;
			}
			else
			{
				currentTimer = 0f;
			}
			if (currentTimer >= hideWeaponsTimer && !IsEquipping)
			{
				currentTimer = 0f;
				HideWeapons();
			}
		}
		else if (currentTimer > 0f)
		{
			currentTimer = 0f;
		}
	}

	protected virtual bool HideTimerConditions()
	{
		if (CanHideWeapons())
		{
			if (!CanHideRightWeapon())
			{
				return CanHideLeftWeapon();
			}
			return true;
		}
		return false;
	}

	protected virtual bool CanHideWeapons()
	{
		if ((bool)melee && (bool)melee.meleeManager)
		{
			if (!forceHide)
			{
				if (!melee.isAttacking && !melee.isBlocking)
				{
					if (!melee.meleeManager.rightWeapon)
					{
						return melee.meleeManager.leftWeapon;
					}
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	protected virtual bool CanDrawWeapons()
	{
		if ((bool)melee)
		{
			return melee.meleeManager;
		}
		return false;
	}

	protected virtual bool CanHideRightWeapon()
	{
		if (CanHideWeapons() && (bool)RightWeaponObject())
		{
			return RightWeaponObject().activeInHierarchy;
		}
		return false;
	}

	protected virtual bool CanHideLeftWeapon()
	{
		if (CanHideWeapons() && (bool)LeftWeaponObject())
		{
			return LeftWeaponObject().activeInHierarchy;
		}
		return false;
	}

	protected virtual bool CanDrawRightWeapon()
	{
		if (CanDrawWeapons() && (bool)RightWeaponObject())
		{
			return !RightWeaponObject().activeInHierarchy;
		}
		return false;
	}

	protected virtual bool CanDrawLeftWeapon()
	{
		if (CanDrawWeapons() && (bool)LeftWeaponObject())
		{
			return !LeftWeaponObject().activeInHierarchy;
		}
		return false;
	}

	protected virtual void HandleInput()
	{
		if (hideAndDrawWeaponsInput.GetButtonDown() && !IsEquipping)
		{
			if (CanHideRightWeapon() || CanHideLeftWeapon())
			{
				HideWeapons();
			}
			else if (CanDrawRightWeapon() || CanDrawLeftWeapon())
			{
				DrawWeapons();
			}
		}
	}

	protected virtual void DrawWeaponsImmediateHandle()
	{
		if (DrawWeaponsImmediateConditions())
		{
			DrawWeapons(immediate: true);
		}
	}

	protected virtual bool DrawWeaponsImmediateConditions()
	{
		if (!melee || melee.cc.customAction || !melee.meleeManager || (melee.meleeManager.CurrentAttackWeapon == null && melee.meleeManager.CurrentDefenseWeapon == null))
		{
			return false;
		}
		if ((!melee.weakAttackInput.GetButton() || !meleeWeakAttack) && (!melee.strongAttackInput.GetButton() || !meleeStrongAttack))
		{
			if (melee.blockInput.GetButton())
			{
				return meleeBlock;
			}
			return false;
		}
		return true;
	}

	protected virtual void HideRightWeapon(bool immediate = false)
	{
		GameObject weapon = RightWeaponObject(checkIsActve: true);
		if (!weapon)
		{
			return;
		}
		vEquipment component = weapon.GetComponent<vEquipment>();
		if (component == null || component.equipPoint == null || component.equipPoint.area == null)
		{
			return;
		}
		vWeaponHolder holder = holderManager.GetHolder(weapon.gameObject, component.referenceItem.id);
		HideWeaponsHandle(melee, component, null, delegate
		{
			if ((bool)holder)
			{
				holder.SetActiveWeapon(active: true);
			}
			if ((bool)weapon)
			{
				weapon.gameObject.SetActive(value: false);
			}
			if (CanHideLeftWeapon())
			{
				HideLeftWeapon(immediate);
			}
		}, immediate);
	}

	protected virtual void HideLeftWeapon(bool immediate = false)
	{
		GameObject weapon = LeftWeaponObject(checkIsActve: true);
		if (!weapon)
		{
			return;
		}
		vEquipment component = weapon.GetComponent<vEquipment>();
		if (component == null || component.equipPoint == null || component.equipPoint.area == null)
		{
			return;
		}
		vWeaponHolder holder = holderManager.GetHolder(weapon.gameObject, component.referenceItem.id);
		HideWeaponsHandle(melee, component, null, delegate
		{
			if ((bool)holder)
			{
				holder.SetActiveWeapon(active: true);
			}
			if ((bool)weapon)
			{
				weapon.gameObject.SetActive(value: false);
			}
		}, immediate);
	}

	protected virtual void DrawRightWeapon(bool immediate = false)
	{
		GameObject weapon = RightWeaponObject();
		if (!weapon)
		{
			return;
		}
		vEquipment component = weapon.GetComponent<vEquipment>();
		if (component == null || component.equipPoint == null || component.equipPoint.area == null || component.equipPoint.area.isLockedToEquip)
		{
			return;
		}
		vWeaponHolder holder = holderManager.GetHolder(weapon.gameObject, component.referenceItem.id);
		DrawWeaponsHandle(melee, component, null, delegate
		{
			if ((bool)holder)
			{
				holder.SetActiveWeapon(active: false);
			}
			if ((bool)weapon && (bool)weapon.gameObject)
			{
				weapon.gameObject.SetActive(value: true);
			}
			if (CanDrawLeftWeapon())
			{
				DrawLeftWeapon(immediate);
			}
		}, immediate);
	}

	protected virtual void DrawLeftWeapon(bool immediate = false)
	{
		GameObject weapon = LeftWeaponObject();
		if (!weapon)
		{
			return;
		}
		vEquipment component = weapon.GetComponent<vEquipment>();
		if (component == null || component.equipPoint == null || component.equipPoint.area == null || component.equipPoint.area.isLockedToEquip)
		{
			return;
		}
		vWeaponHolder holder = holderManager.GetHolder(weapon.gameObject, component.referenceItem.id);
		DrawWeaponsHandle(melee, component, null, delegate
		{
			if ((bool)holder)
			{
				holder.SetActiveWeapon(active: false);
			}
			if ((bool)weapon && (bool)weapon.gameObject)
			{
				weapon.gameObject.SetActive(value: true);
			}
		}, immediate);
	}

	protected virtual void DrawWeaponsHandle(vThirdPersonInput tpInput, vEquipment equipment, UnityAction onStart, UnityAction onFinish, bool immediate = false)
	{
		if (holderManager.inEquip)
		{
			return;
		}
		if (!immediate)
		{
			if (!string.IsNullOrEmpty(equipment.referenceItem.EnableAnim) && equipment != null && equipment.equipPoint != null)
			{
				tpInput.animator.SetBool("FlipEquip", equipment.equipPoint.equipPointName.Contains("Left"));
				tpInput.animator.CrossFade(equipment.referenceItem.EnableAnim, 0.25f);
			}
			else
			{
				immediate = true;
			}
		}
		StartCoroutine(holderManager.EquipRoutine(equipment.referenceItem.enableDelayTime, immediate, onStart, onFinish));
	}

	protected virtual void HideWeaponsHandle(vThirdPersonInput tpInput, vEquipment equipment, UnityAction onStart, UnityAction onFinish, bool immediate = false)
	{
		if (holderManager.inUnequip)
		{
			return;
		}
		if (!immediate)
		{
			if (!string.IsNullOrEmpty(equipment.referenceItem.DisableAnim) && equipment != null && equipment.equipPoint != null)
			{
				tpInput.animator.SetBool("FlipEquip", equipment.equipPoint.equipPointName.Contains("Left"));
				tpInput.animator.CrossFade(equipment.referenceItem.DisableAnim, 0.25f);
			}
			else
			{
				immediate = true;
			}
		}
		StartCoroutine(holderManager.UnequipRoutine(equipment.referenceItem.disableDelayTime, immediate, onStart, onFinish));
	}
}
