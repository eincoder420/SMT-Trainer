using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vMelee;

[vClassHeader("Collect Melee Control", "This component is used when you're character doesn't have a ItemManager to manage items, this will allow you to pickup 1 weapon at the time.")]
public class vCollectMeleeControl : vMonoBehaviour
{
	[HideInInspector]
	public vMeleeManager meleeManager;

	[Header("Handlers")]
	public vHandler rightHandler = new vHandler();

	public vHandler leftHandler = new vHandler();

	[Header("Unequip Inputs")]
	public GenericInput unequipRightInput;

	public GenericInput unequipLeftInput;

	[HideInInspector]
	public vCollectableStandalone leftWeapon;

	[HideInInspector]
	public vCollectableStandalone rightWeapon;

	public vControlDisplayWeaponStandalone controlDisplayPrefab;

	protected vControlDisplayWeaponStandalone currentDisplay;

	[vEditorToolbar("Melee Events", false, "", false, false)]
	public UnityEvent onEquipMeleeWeapon;

	[vEditorToolbar("Melee Events", false, "", false, false)]
	public UnityEvent onUnequipMeleeWeapon;

	[vEditorToolbar("Melee Events", false, "", false, false)]
	public UnityEvent onEquipRightWeapon;

	[vEditorToolbar("Melee Events", false, "", false, false)]
	public UnityEvent onEquipLeftWeapon;

	[vEditorToolbar("Melee Events", false, "", false, false)]
	public UnityEvent onUnEquipRightWeapon;

	[vEditorToolbar("Melee Events", false, "", false, false)]
	public UnityEvent onUnEquipLeftWeapon;

	internal bool wasUsingMeleeWeapon;

	public virtual bool isUsingTwoHandWeapon
	{
		get
		{
			if (!(rightWeapon != null) || !rightWeapon.twoHandWeapon)
			{
				if (leftWeapon != null)
				{
					return leftWeapon.twoHandWeapon;
				}
				return false;
			}
			return true;
		}
	}

	public virtual bool isUsingMeleeWeapon
	{
		get
		{
			if (!meleeManager)
			{
				return false;
			}
			if (!meleeManager.leftWeapon || !meleeManager.leftWeapon.gameObject.activeInHierarchy)
			{
				if ((bool)meleeManager.rightWeapon)
				{
					return meleeManager.rightWeapon.gameObject.activeInHierarchy;
				}
				return false;
			}
			return true;
		}
	}

	protected virtual void Start()
	{
		meleeManager = GetComponent<vMeleeManager>();
		if ((bool)controlDisplayPrefab)
		{
			currentDisplay = Object.Instantiate(controlDisplayPrefab);
		}
	}

	protected virtual void Update()
	{
		UnequipWeaponHandle();
		CheckIsEquipedWifhWeapon();
	}

	public virtual void HandleCollectableInput(vCollectableStandalone collectableStandAlone)
	{
		if ((bool)meleeManager && collectableStandAlone != null && collectableStandAlone.weapon != null)
		{
			EquipMeleeWeapon(collectableStandAlone);
		}
	}

	protected virtual void EquipMeleeWeapon(vCollectableStandalone collectable)
	{
		vMeleeWeapon component = collectable.weapon.GetComponent<vMeleeWeapon>();
		if (!component)
		{
			return;
		}
		if (component.meleeType != 0)
		{
			Transform equipPoint = GetEquipPoint(rightHandler, collectable.targetEquipPoint);
			if (!equipPoint)
			{
				return;
			}
			collectable.weapon.transform.SetParent(equipPoint);
			collectable.weapon.transform.localPosition = Vector3.zero;
			collectable.weapon.transform.localEulerAngles = Vector3.zero;
			if ((bool)rightWeapon && rightWeapon.gameObject != collectable.gameObject)
			{
				RemoveRightWeapon();
			}
			if (collectable.twoHandWeapon || ((bool)leftWeapon && leftWeapon.twoHandWeapon))
			{
				RemoveLeftWeapon();
			}
			meleeManager.SetRightWeapon(component.gameObject);
			collectable.OnEquip.Invoke();
			rightWeapon = collectable;
			onEquipRightWeapon.Invoke();
			UpdateRightDisplay(collectable);
		}
		if (component.meleeType == vMeleeType.OnlyAttack || component.meleeType == vMeleeType.AttackAndDefense)
		{
			return;
		}
		Transform equipPoint2 = GetEquipPoint(leftHandler, collectable.targetEquipPoint);
		if ((bool)equipPoint2)
		{
			collectable.weapon.transform.SetParent(equipPoint2);
			collectable.weapon.transform.localPosition = Vector3.zero;
			collectable.weapon.transform.localEulerAngles = Vector3.zero;
			if ((bool)leftWeapon && leftWeapon.gameObject != collectable.gameObject)
			{
				RemoveLeftWeapon();
			}
			if (collectable.twoHandWeapon || ((bool)rightWeapon && rightWeapon.twoHandWeapon))
			{
				RemoveRightWeapon();
			}
			onEquipLeftWeapon.Invoke();
			meleeManager.SetLeftWeapon(component.gameObject);
			collectable.OnEquip.Invoke();
			leftWeapon = collectable;
			UpdateLeftDisplay(collectable);
		}
	}

	protected virtual Transform GetEquipPoint(vHandler point, string name)
	{
		Transform result = point.defaultHandler;
		Transform transform = point.customHandlers.Find((Transform _p) => _p.name.Equals(name));
		if ((bool)transform)
		{
			result = transform;
		}
		return result;
	}

	protected virtual void UnequipWeaponHandle()
	{
		if ((bool)rightWeapon && unequipRightInput.GetButtonDown())
		{
			RemoveRightWeapon();
		}
		if ((bool)leftWeapon && unequipLeftInput.GetButtonDown())
		{
			RemoveLeftWeapon();
		}
	}

	public virtual void RemoveLeftWeapon()
	{
		if ((bool)leftWeapon)
		{
			leftWeapon.weapon.transform.parent = null;
			leftWeapon.OnDrop.Invoke();
			onUnEquipLeftWeapon.Invoke();
		}
		if ((bool)meleeManager)
		{
			meleeManager.leftWeapon = null;
		}
		UpdateLeftDisplay();
	}

	public virtual void RemoveRightWeapon()
	{
		if ((bool)rightWeapon)
		{
			rightWeapon.weapon.transform.parent = null;
			rightWeapon.OnDrop.Invoke();
			onUnEquipRightWeapon.Invoke();
		}
		if ((bool)meleeManager)
		{
			meleeManager.rightWeapon = null;
		}
		UpdateRightDisplay();
	}

	protected virtual void CheckIsEquipedWifhWeapon()
	{
		if (wasUsingMeleeWeapon && !isUsingMeleeWeapon)
		{
			onUnequipMeleeWeapon.Invoke();
			wasUsingMeleeWeapon = false;
		}
		else if (!wasUsingMeleeWeapon && isUsingMeleeWeapon)
		{
			onEquipMeleeWeapon.Invoke();
			wasUsingMeleeWeapon = true;
		}
	}

	protected virtual void UpdateLeftDisplay(vCollectableStandalone collectable = null)
	{
		if ((bool)currentDisplay)
		{
			if ((bool)collectable)
			{
				currentDisplay.SetLeftWeaponIcon(collectable.weaponIcon);
				currentDisplay.SetLeftWeaponText(collectable.weaponText);
			}
			else
			{
				currentDisplay.RemoveLeftWeaponIcon();
				currentDisplay.RemoveLeftWeaponText();
			}
		}
	}

	protected virtual void UpdateRightDisplay(vCollectableStandalone collectable = null)
	{
		if ((bool)currentDisplay)
		{
			if ((bool)collectable)
			{
				currentDisplay.SetRightWeaponIcon(collectable.weaponIcon);
				currentDisplay.SetRightWeaponText(collectable.weaponText);
			}
			else
			{
				currentDisplay.RemoveRightWeaponIcon();
				currentDisplay.RemoveRightWeaponText();
			}
		}
	}
}
