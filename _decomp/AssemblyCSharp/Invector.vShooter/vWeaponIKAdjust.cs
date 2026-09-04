using System.Collections.Generic;
using Invector.IK;
using UnityEngine;
using UnityEngine.Serialization;

namespace Invector.vShooter;

[CreateAssetMenu(menuName = "Invector/Shooter/New Weapon IK Adjust")]
public class vWeaponIKAdjust : ScriptableObject
{
	public const string StandingState = "Standing";

	public const string StandingAimingState = "StandingAiming";

	public const string CrouchingState = "Crouching";

	public const string CrouchingAimingState = "CrouchingAiming";

	public static string[] defaultNames = new string[4] { "Standing", "StandingAiming", "Crouching", "CrouchingAiming" };

	public List<string> weaponCategories = new List<string> { "HandGun", "Pistol" };

	public List<IKAdjust> ikAdjustsLeft = new List<IKAdjust>();

	public List<IKAdjust> ikAdjustsRight = new List<IKAdjust>();

	[vSeparator("<color=yellow><size=15>The fields below will be removed in the future.</size></color>\n<color=green>The old states settings will be automatically added to IKAdjustsLeft and IKAdjustsRight</color>\n<color=white><size=10> If for some reason the default States is not present in the lists, Right Click in this Inspector Header and click in Add Default States</size></color>", "")]
	[FormerlySerializedAs("standing")]
	public IKAdjust standingRight = new IKAdjust("StandingRight");

	[FormerlySerializedAs("standingAiming")]
	public IKAdjust standingAimingRight = new IKAdjust("StandingAimingRight");

	public IKAdjust standingLeft = new IKAdjust("StandingLeft");

	public IKAdjust standingAimingLeft = new IKAdjust("StandingAimingLeft");

	[FormerlySerializedAs("crouching")]
	public IKAdjust crouchingRight = new IKAdjust("CrouchingRight");

	[FormerlySerializedAs("crouchingAiming")]
	public IKAdjust crouchingAimingRight = new IKAdjust("CrouchingAimingRight");

	public IKAdjust crouchingLeft = new IKAdjust("CrouchingLeft");

	public IKAdjust crouchingAimingLeft = new IKAdjust("CrouchingAimingLeft");

	public void Awake()
	{
		AddDefaultStates();
	}

	public bool HasAllDefaultStates()
	{
		int i;
		for (i = 0; i < defaultNames.Length; i++)
		{
			if (!ikAdjustsLeft.Exists((IKAdjust a) => a.name.Equals(defaultNames[i])))
			{
				return false;
			}
			if (!ikAdjustsRight.Exists((IKAdjust a) => a.name.Equals(defaultNames[i])))
			{
				return false;
			}
		}
		return true;
	}

	[ContextMenu("Add Default States")]
	public virtual void AddDefaultStates()
	{
		ApplyCorretlyName();
		AddIKAdjust(standingRight.Copy());
		AddIKAdjust(standingAimingRight.Copy());
		AddIKAdjust(crouchingRight.Copy());
		AddIKAdjust(crouchingAimingRight.Copy());
		AddIKAdjust(standingLeft.Copy(), isLeftWeapon: true);
		AddIKAdjust(standingAimingLeft.Copy(), isLeftWeapon: true);
		AddIKAdjust(crouchingLeft.Copy(), isLeftWeapon: true);
		AddIKAdjust(crouchingAimingLeft.Copy(), isLeftWeapon: true);
	}

	public virtual void AddIKAdjust(string name, bool isLeftWeapon = false)
	{
		List<IKAdjust> list = (isLeftWeapon ? ikAdjustsLeft : ikAdjustsRight);
		if (!list.Exists((IKAdjust a) => a.name.Equals(name)))
		{
			list.Add(new IKAdjust(name));
		}
	}

	public virtual void AddIKAdjust(IKAdjust adjust, bool isLeftWeapon = false)
	{
		if (adjust != null)
		{
			List<IKAdjust> list = (isLeftWeapon ? ikAdjustsLeft : ikAdjustsRight);
			if (!list.Exists((IKAdjust a) => a.name.Equals(adjust.name)))
			{
				list.Add(adjust);
			}
		}
	}

	public virtual IKAdjust CreateIKAdjust(string name, bool isLeftWeapon = false)
	{
		List<IKAdjust> list = (isLeftWeapon ? ikAdjustsLeft : ikAdjustsRight);
		if (!list.Exists((IKAdjust a) => a.name.Equals(name)))
		{
			IKAdjust iKAdjust = new IKAdjust(name);
			list.Add(iKAdjust);
			return iKAdjust;
		}
		return GetIKAdjust(name, isLeftWeapon);
	}

	public virtual string GetDefaultStateName(vIShooterIKController controller)
	{
		bool isAiming = controller.IsAiming;
		bool isCrouching = controller.IsCrouching;
		if (!isAiming)
		{
			if (!isCrouching)
			{
				return "Standing";
			}
			return "Crouching";
		}
		if (!isCrouching)
		{
			return "StandingAiming";
		}
		return "CrouchingAiming";
	}

	public virtual IKAdjust GetIKAdjust(bool isAming, bool isCrouching, bool isLeftWeapon)
	{
		if (isAming)
		{
			if (isCrouching)
			{
				if (!isLeftWeapon)
				{
					return crouchingAimingRight;
				}
				return crouchingAimingLeft;
			}
			if (!isLeftWeapon)
			{
				return standingAimingRight;
			}
			return standingAimingLeft;
		}
		if (isCrouching)
		{
			if (!isLeftWeapon)
			{
				return crouchingRight;
			}
			return crouchingLeft;
		}
		if (!isLeftWeapon)
		{
			return standingRight;
		}
		return standingLeft;
	}

	[ContextMenu("Reset Standing")]
	public virtual void ResetStanding()
	{
		standingLeft = new IKAdjust("StandingLeft");
		standingRight = new IKAdjust("StandingRight");
	}

	[ContextMenu("Reset Standing Aiming")]
	public virtual void ResetStandingAiming()
	{
		standingAimingLeft = new IKAdjust("StandingAimingLeft");
		standingAimingRight = new IKAdjust("StandingAimingRight");
	}

	[ContextMenu("Reset Crouching")]
	public virtual void ResetCrouching()
	{
		crouchingLeft = new IKAdjust("CrouchingLeft");
		crouchingRight = new IKAdjust("CrouchingRight");
	}

	[ContextMenu("Reset Crouching Aiming")]
	public virtual void ResetCrouchingAiming()
	{
		crouchingAimingLeft = new IKAdjust("CrouchingAimingLeft");
		crouchingAimingRight = new IKAdjust("CrouchingAimingRight");
	}

	[ContextMenu("Reset Default Adjust Names")]
	public virtual void ApplyCorretlyName()
	{
		standingRight.name = "Standing";
		standingAimingRight.name = "StandingAiming";
		standingLeft.name = "Standing";
		standingAimingLeft.name = "StandingAiming";
		crouchingRight.name = "Crouching";
		crouchingAimingRight.name = "CrouchingAiming";
		crouchingLeft.name = "Crouching";
		crouchingAimingLeft.name = "CrouchingAiming";
	}

	[ContextMenu("Reset ALL")]
	public virtual void Reset()
	{
		ResetStanding();
		ResetStandingAiming();
		ResetCrouching();
		ResetCrouchingAiming();
		ApplyCorretlyName();
	}

	public virtual IKAdjust GetIKAdjust(string name, bool isLeftWeapon)
	{
		return (isLeftWeapon ? ikAdjustsLeft : ikAdjustsRight).Find((IKAdjust ik) => ik.name.Equals(name));
	}
}
