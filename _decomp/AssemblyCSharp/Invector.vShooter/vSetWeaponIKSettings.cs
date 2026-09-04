using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vShooter;

[vClassHeader("Set Weapon IK Settings", true, "icon_v2", false, "", openClose = false)]
public class vSetWeaponIKSettings : vMonoBehaviour
{
	[Serializable]
	public class IKSettings
	{
		public string name;

		[Tooltip("IK will help the right hand to align where you actually is aiming")]
		public bool alignRightHandToAim = true;

		[Tooltip("IK will help the right hand to align where you actually is aiming")]
		public bool alignRightUpperArmToAim = true;

		public bool raycastAimTarget = true;

		public vShooterWeapon.IKLocomotionOptions strafeIKOptions;

		public vShooterWeapon.IKLocomotionOptions freeIKOptions;

		[Tooltip("Left IK while attacking")]
		public bool useIkAttacking;

		[Tooltip("Left IK while Shot")]
		public bool disableIkOnShot;

		[Tooltip("Left IK while Aming")]
		public bool useIKOnAiming = true;
	}

	public List<IKSettings> settings;

	[vHelpBox("It's recommended to attach this component in a Handler", vHelpBoxAttribute.MessageType.None)]
	[Tooltip("Auto get shooter weapon when set settings")]
	public bool getWeaponOnSet = true;

	[vHideInInspector("getWeaponOnSet", false, invertValue = true)]
	public vShooterWeapon weapon;

	public bool setOnStart;

	[vHideInInspector("setOnStart", false)]
	public int indexOfSetting;

	public IKSettings defaultIKSettings;

	private bool defaultIsCreated;

	private void Start()
	{
		if (setOnStart)
		{
			SetSettings(indexOfSetting);
		}
	}

	private void CopyDefaultIK()
	{
		if ((bool)weapon && !defaultIsCreated)
		{
			defaultIKSettings.freeIKOptions = weapon.freeIKOptions.Copy();
			defaultIKSettings.strafeIKOptions = weapon.freeIKOptions.Copy();
			defaultIKSettings.useIkAttacking = weapon.useIkAttacking;
			defaultIKSettings.useIKOnAiming = weapon.useIKOnAiming;
			defaultIKSettings.alignRightHandToAim = weapon.alignRightHandToAim;
			defaultIKSettings.alignRightUpperArmToAim = weapon.alignRightUpperArmToAim;
			defaultIKSettings.raycastAimTarget = weapon.raycastAimTarget;
			defaultIsCreated = true;
		}
	}

	public void ResetSettings()
	{
		if (defaultIsCreated && (bool)weapon)
		{
			ApplySettings(defaultIKSettings);
		}
	}

	public void SetSettings(int index)
	{
		if (getWeaponOnSet)
		{
			vShooterWeapon componentInChildren = GetComponentInChildren<vShooterWeapon>();
			if (weapon != componentInChildren)
			{
				defaultIsCreated = false;
			}
		}
		if ((bool)weapon)
		{
			CopyDefaultIK();
			if (settings.Count > 0 && index >= 0 && index < settings.Count)
			{
				IKSettings iKSettings = settings[index];
				ApplySettings(iKSettings);
			}
		}
	}

	public void SetSettings(string name)
	{
		if (getWeaponOnSet)
		{
			vShooterWeapon componentInChildren = GetComponentInChildren<vShooterWeapon>();
			if (weapon != componentInChildren)
			{
				defaultIsCreated = false;
			}
		}
		if (!weapon)
		{
			return;
		}
		CopyDefaultIK();
		if (settings.Count > 0)
		{
			IKSettings iKSettings = settings.Find((IKSettings s) => s.name.Equals(name));
			ApplySettings(iKSettings);
		}
	}

	private void ApplySettings(IKSettings settings)
	{
		if (settings != null)
		{
			weapon.alignRightHandToAim = settings.alignRightHandToAim;
			weapon.alignRightUpperArmToAim = settings.alignRightUpperArmToAim;
			weapon.raycastAimTarget = settings.raycastAimTarget;
			weapon.useIkAttacking = settings.useIkAttacking;
			weapon.useIKOnAiming = settings.useIKOnAiming;
			weapon.freeIKOptions = settings.freeIKOptions;
			weapon.strafeIKOptions = settings.strafeIKOptions;
		}
	}
}
