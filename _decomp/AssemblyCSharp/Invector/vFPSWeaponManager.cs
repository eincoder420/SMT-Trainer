using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector;

[vClassHeader("FPS Weapon Manager", "Simple weapon manager for the FPS Controller")]
public class vFPSWeaponManager : vMonoBehaviour
{
	[Serializable]
	public class vFPSWeapon
	{
		public string weaponName;

		public bool canUse;

		public vFPSAttackControl attackControl;

		public float attackFrequency = 1f;

		public int animsetID;

		public KeyCode selectKey;
	}

	public bool startWithWeapon;

	public string startWeaponName;

	public List<vFPSWeapon> weapons;

	public Animator animator;

	private bool inAttack;

	private vFPSWeapon currentWeapon;

	public void EquipWeapon(string weaponName)
	{
		vFPSWeapon vFPSWeapon = weapons.Find((vFPSWeapon w) => w.weaponName.Equals(weaponName));
		if (vFPSWeapon != null)
		{
			vFPSWeapon.canUse = true;
			if (currentWeapon != null)
			{
				currentWeapon.attackControl.SetActiveWeapon(value: false);
			}
			if ((bool)vFPSWeapon.attackControl)
			{
				vFPSWeapon.attackControl.SetActiveWeapon(value: true);
			}
			currentWeapon = vFPSWeapon;
		}
	}

	private void EquipWeapon(vFPSWeapon weapon)
	{
		if (weapon != null)
		{
			if (currentWeapon != null)
			{
				currentWeapon.attackControl.SetActiveWeapon(value: false);
			}
			if ((bool)weapon.attackControl)
			{
				weapon.attackControl.SetActiveWeapon(value: true);
			}
			currentWeapon = weapon;
		}
	}

	public void Update()
	{
		if ((bool)animator && currentWeapon != null)
		{
			animator.SetFloat("AnimSetID", currentWeapon.animsetID, 0.2f, Time.deltaTime);
		}
		for (int i = 0; i < weapons.Count; i++)
		{
			if (weapons[i].canUse && weapons[i] != currentWeapon && Input.GetKeyDown(weapons[i].selectKey))
			{
				EquipWeapon(weapons[i]);
			}
		}
	}

	public void Attack()
	{
		if (!inAttack && currentWeapon != null && !inAttack)
		{
			inAttack = true;
			animator.SetTrigger("Attack");
			currentWeapon.attackControl.Attack();
			Invoke("ResetAttack", currentWeapon.attackFrequency);
		}
	}

	private void ResetAttack()
	{
		inAttack = false;
	}

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		for (int i = 0; i < weapons.Count; i++)
		{
			if ((bool)weapons[i].attackControl)
			{
				weapons[i].attackControl.SetActiveWeapon(value: false);
			}
		}
		if (startWithWeapon)
		{
			EquipWeapon(startWeaponName);
		}
	}
}
