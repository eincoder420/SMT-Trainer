using Invector.vCharacterController.vActions;
using Invector.vShooter;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vMelee;

[vClassHeader("Collect Shooter Melee Control", "This component is used when you're character doesn't have a ItemManager to manage items, this will allow you to pickup 1 weapon at the time.")]
public class vCollectShooterMeleeControl : vCollectMeleeControl
{
	protected vShooterManager shooterManager;

	[vEditorToolbar("Shooter Events", false, "", false, false)]
	public UnityEvent onEquipShooterWeapon;

	[vEditorToolbar("Shooter Events", false, "", false, false)]
	public UnityEvent onUnequipShooterWeapon;

	internal bool wasUsingShooterWeapon;

	public virtual bool isUsingShooterWeapon
	{
		get
		{
			if (!shooterManager)
			{
				return false;
			}
			if ((bool)shooterManager.CurrentWeapon)
			{
				return shooterManager.IsCurrentWeaponActive();
			}
			return false;
		}
	}

	protected override void Start()
	{
		base.Start();
		shooterManager = GetComponent<vShooterManager>();
	}

	public override void HandleCollectableInput(vCollectableStandalone collectableStandAlone)
	{
		if ((bool)shooterManager && collectableStandAlone != null && collectableStandAlone.weapon != null)
		{
			EquipShooterWeapon(collectableStandAlone);
		}
		base.HandleCollectableInput(collectableStandAlone);
	}

	protected virtual void EquipShooterWeapon(vCollectableStandalone collectable)
	{
		vShooterWeapon component = collectable.weapon.GetComponent<vShooterWeapon>();
		if (!component)
		{
			return;
		}
		Transform transform = null;
		if (component.isLeftWeapon)
		{
			transform = GetEquipPoint(leftHandler, collectable.targetEquipPoint);
			if ((bool)transform)
			{
				collectable.weapon.transform.SetParent(transform);
				collectable.weapon.transform.localPosition = Vector3.zero;
				collectable.weapon.transform.localEulerAngles = Vector3.zero;
				if ((bool)leftWeapon && leftWeapon.gameObject != collectable.gameObject)
				{
					RemoveLeftWeapon();
				}
				shooterManager.SetLeftWeapon(component.gameObject);
				collectable.OnEquip.Invoke();
				leftWeapon = collectable;
				UpdateLeftDisplay(collectable);
				if ((bool)rightWeapon)
				{
					RemoveRightWeapon();
				}
			}
			return;
		}
		transform = GetEquipPoint(rightHandler, collectable.targetEquipPoint);
		if ((bool)transform)
		{
			collectable.weapon.transform.SetParent(transform);
			collectable.weapon.transform.localPosition = Vector3.zero;
			collectable.weapon.transform.localEulerAngles = Vector3.zero;
			if ((bool)rightWeapon && rightWeapon.gameObject != collectable.gameObject)
			{
				RemoveRightWeapon();
			}
			shooterManager.SetRightWeapon(component.gameObject);
			collectable.OnEquip.Invoke();
			rightWeapon = collectable;
			UpdateRightDisplay(collectable);
			if ((bool)leftWeapon)
			{
				RemoveLeftWeapon();
			}
		}
	}

	public override void RemoveRightWeapon()
	{
		base.RemoveRightWeapon();
		if ((bool)shooterManager)
		{
			shooterManager.rWeapon = null;
		}
	}

	public override void RemoveLeftWeapon()
	{
		base.RemoveLeftWeapon();
		if ((bool)shooterManager)
		{
			shooterManager.lWeapon = null;
		}
	}

	protected override void CheckIsEquipedWifhWeapon()
	{
		if (!wasUsingShooterWeapon && isUsingShooterWeapon)
		{
			onUnequipMeleeWeapon.Invoke();
			wasUsingMeleeWeapon = false;
			onEquipShooterWeapon.Invoke();
			wasUsingShooterWeapon = true;
		}
		else if (wasUsingShooterWeapon && !isUsingShooterWeapon)
		{
			onUnequipShooterWeapon.Invoke();
			wasUsingShooterWeapon = false;
		}
		if (!wasUsingShooterWeapon)
		{
			base.CheckIsEquipedWifhWeapon();
		}
	}
}
