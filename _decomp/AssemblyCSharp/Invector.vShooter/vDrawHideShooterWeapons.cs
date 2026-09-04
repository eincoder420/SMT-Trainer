using Invector.vCharacterController;
using UnityEngine;

namespace Invector.vShooter;

[vClassHeader("Draw/Hide Shooter Melee Weapons", "This component works with vItemManager, vWeaponHolderManager and vShooterMeleeInput", useHelpBox = true)]
public class vDrawHideShooterWeapons : vDrawHideMeleeWeapons
{
	[vEditorToolbar("Shooter", false, "", false, false)]
	[Header("Draw Immediate Conditions")]
	public bool shoot;

	public bool aim = true;

	public bool hipFire = true;

	public virtual vShooterMeleeInput shooter { get; set; }

	protected override void Start()
	{
		base.Start();
		shooter = GetComponent<vShooterMeleeInput>();
	}

	protected override bool CanHideWeapons()
	{
		if (!shooter || !shooter.shooterManager || !shooter.shooterManager.CurrentWeapon || (!forceHide && (shooter.IsAiming || shooter.isReloading)))
		{
			if (base.CanHideWeapons())
			{
				if (!forceHide)
				{
					if (!shooter.IsAiming)
					{
						return !shooter.isReloading;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	protected override bool CanDrawWeapons()
	{
		if (forceHide || !shooter || !shooter.shooterManager || !shooter.shooterManager.CurrentWeapon || shooter.shooterManager.CurrentWeapon.gameObject.activeInHierarchy)
		{
			return base.CanDrawWeapons();
		}
		return true;
	}

	protected override GameObject RightWeaponObject(bool checkIsActve = false)
	{
		if ((bool)shooter && (bool)shooter.shooterManager && (bool)shooter.shooterManager.rWeapon && (!checkIsActve || shooter.shooterManager.rWeapon.gameObject.activeInHierarchy))
		{
			if (shooter.shooterManager.rWeapon.inHolder)
			{
				return null;
			}
			return shooter.shooterManager.rWeapon.gameObject;
		}
		return base.RightWeaponObject(checkIsActve);
	}

	protected override GameObject LeftWeaponObject(bool checkIsActve = false)
	{
		if ((bool)shooter && (bool)shooter.shooterManager && (bool)shooter.shooterManager.lWeapon && (!checkIsActve || shooter.shooterManager.lWeapon.gameObject.activeInHierarchy))
		{
			if (shooter.shooterManager.lWeapon.inHolder)
			{
				return null;
			}
			return shooter.shooterManager.lWeapon.gameObject;
		}
		return base.LeftWeaponObject(checkIsActve);
	}

	protected override void DrawRightWeapon(bool immediate = false)
	{
		base.DrawRightWeapon(immediate);
	}

	protected override bool DrawWeaponsImmediateConditions()
	{
		if ((bool)shooter && (bool)shooter.shooterManager && (bool)shooter.shooterManager.CurrentWeapon)
		{
			return DrawShooterWeaponImmediateConditions();
		}
		return base.DrawWeaponsImmediateConditions();
	}

	protected virtual bool DrawShooterWeaponImmediateConditions()
	{
		if (!shooter || !shooter.shooterManager || shooter.cc.customAction || !shooter.shooterManager.CurrentWeapon || shooter.lockInput)
		{
			return false;
		}
		if (shooter.CurrentActiveWeapon == null && ((shooter.aimInput.GetButtonDown() && aim) || (shooter.shooterManager.hipfireShot && shooter.shotInput.GetButtonDown() && hipFire) || (shooter.shotInput.GetButtonDown() && shoot)))
		{
			return true;
		}
		return false;
	}

	protected override void HandleInput()
	{
		base.HandleInput();
	}
}
