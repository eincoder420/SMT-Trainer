using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vShooter;

[vClassHeader("PowerCharge Projectile", true, "icon_v2", false, "", openClose = false)]
public class vPowerChargeProjectileControl : vMonoBehaviour
{
	[Serializable]
	public class vProjectilePerPower
	{
		public float min;

		public float max;

		public GameObject projectile;

		[Tooltip("Called when power is between min and max value")]
		public UnityEvent OnValidatePower;
	}

	public List<vProjectilePerPower> projectiles;

	private vShooterWeapon weapon;

	private vProjectilePerPower lastProjectilePerPower;

	private void Start()
	{
		weapon = GetComponent<vShooterWeapon>();
		if ((bool)weapon)
		{
			weapon.onPowerChargerChanged.AddListener(OnChangerPower);
		}
	}

	public void OnChangerPower(float value)
	{
		if (!(value <= 0f) && (bool)weapon)
		{
			vProjectilePerPower vProjectilePerPower = projectiles.Find((vProjectilePerPower projectile) => value >= projectile.min && value <= projectile.max);
			if (vProjectilePerPower != null && (bool)vProjectilePerPower.projectile && lastProjectilePerPower != vProjectilePerPower)
			{
				lastProjectilePerPower = vProjectilePerPower;
				weapon.projectile = vProjectilePerPower.projectile;
				vProjectilePerPower.OnValidatePower.Invoke();
			}
		}
	}
}
