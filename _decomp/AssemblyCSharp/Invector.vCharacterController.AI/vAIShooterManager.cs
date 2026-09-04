using System;
using System.Collections;
using System.Collections.Generic;
using Invector.vShooter;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI;

[vClassHeader("AI SHOOTER MANAGER", "Make sure to set the Damage Layers to 'Default' and 'BodyPart', or any other layer you need to inflict damage.")]
public class vAIShooterManager : vMonoBehaviour
{
	[Serializable]
	public class OnReloadWeapon : UnityEvent<vShooterWeapon>
	{
	}

	[vEditorToolbar("Aim", false, "", false, false)]
	[Tooltip("min distance to aim")]
	public float minDistanceToAim = 1f;

	public float checkAimRadius = 0.1f;

	[Tooltip("smooth of the right hand when correcting the aim")]
	public float smoothHandRotation = 30f;

	[Tooltip("Limit the maxAngle for the right hand to correct the aim")]
	public float maxHandAngle = 60f;

	[Tooltip("Check this to syinc the weapon aim to the camera aim")]
	public bool raycastAimTarget = true;

	[Tooltip("Layer to aim")]
	public LayerMask damageLayer = 1;

	[Tooltip("Tags to the Aim ignore - tag this gameObject to avoid shot on yourself")]
	public List<string> ignoreTags;

	[vEditorToolbar("IK Adjust", false, "", false, false)]
	[Tooltip("Check this to use IK on the left hand")]
	public bool useLeftIK = true;

	[vEditorToolbar("IK Adjust", false, "", false, false)]
	[Tooltip("Check this to use IK on the left hand")]
	public bool useRightIK = true;

	public vWeaponIKAdjustList weaponIKAdjustList;

	[vEditorToolbar("Weapons", false, "", false, false)]
	public vShooterWeapon rWeapon;

	[vEditorToolbar("Weapons", false, "", false, false)]
	public vShooterWeapon lWeapon;

	[HideInInspector]
	public OnReloadWeapon onReloadWeapon;

	private Animator animator;

	private int totalAmmo;

	private int secundaryTotalAmmo;

	protected vWeaponIKAdjust currentWeaponIKAdjust;

	public virtual vWeaponIKAdjust CurrentWeaponIK => currentWeaponIKAdjust;

	public bool isShooting
	{
		get
		{
			if ((bool)CurrentWeapon)
			{
				return !CurrentWeapon.CanDoShot;
			}
			return false;
		}
	}

	public virtual bool weaponHasAmmo
	{
		get
		{
			if (!CurrentWeapon)
			{
				return false;
			}
			return CurrentWeapon.ammoCount > 0;
		}
	}

	public virtual vShooterWeapon CurrentWeapon
	{
		get
		{
			if (!rWeapon || !rWeapon.gameObject.activeSelf)
			{
				if (!lWeapon || !lWeapon.gameObject.activeSelf)
				{
					return null;
				}
				return lWeapon;
			}
			return rWeapon;
		}
	}

	public bool IsLeftWeapon
	{
		get
		{
			if (!(rWeapon == null))
			{
				return false;
			}
			return lWeapon;
		}
	}

	private void Start()
	{
		animator = GetComponent<Animator>();
		if ((bool)animator)
		{
			Transform boneTransform = animator.GetBoneTransform(HumanBodyBones.RightHand);
			Transform boneTransform2 = animator.GetBoneTransform(HumanBodyBones.LeftHand);
			vShooterWeapon componentInChildren = boneTransform.GetComponentInChildren<vShooterWeapon>(includeInactive: true);
			vShooterWeapon componentInChildren2 = boneTransform2.GetComponentInChildren<vShooterWeapon>(includeInactive: true);
			if (componentInChildren != null)
			{
				SetRightWeapon(componentInChildren.gameObject);
			}
			if (componentInChildren2 != null)
			{
				SetLeftWeapon(componentInChildren2.gameObject);
			}
		}
		if (!ignoreTags.Contains(base.gameObject.tag))
		{
			ignoreTags.Add(base.gameObject.tag);
		}
	}

	public void SetDamageLayer(LayerMask mask)
	{
		damageLayer = mask;
		if ((bool)CurrentWeapon)
		{
			CurrentWeapon.hitLayer = mask;
		}
	}

	public void SetLeftWeapon(GameObject weapon)
	{
		if (!(weapon != null))
		{
			return;
		}
		vShooterWeapon componentInChildren = weapon.GetComponentInChildren<vShooterWeapon>(includeInactive: true);
		lWeapon = componentInChildren;
		if ((bool)lWeapon)
		{
			lWeapon.ignoreTags = ignoreTags;
			lWeapon.hitLayer = damageLayer;
			lWeapon.root = base.transform;
			lWeapon.isSecundaryWeapon = false;
			lWeapon.onDestroy.AddListener(OnDestroyWeapon);
			if (lWeapon.dontUseReload)
			{
				ReloadWeaponAuto(lWeapon);
			}
			UpdateWeaponIK();
		}
	}

	public void SetRightWeapon(GameObject weapon)
	{
		if (!(weapon != null))
		{
			return;
		}
		vShooterWeapon componentInChildren = weapon.GetComponentInChildren<vShooterWeapon>(includeInactive: true);
		rWeapon = componentInChildren;
		if ((bool)rWeapon)
		{
			rWeapon.ignoreTags = ignoreTags;
			rWeapon.hitLayer = damageLayer;
			rWeapon.root = base.transform;
			rWeapon.isSecundaryWeapon = false;
			rWeapon.onDestroy.AddListener(OnDestroyWeapon);
			if (rWeapon.dontUseReload)
			{
				ReloadWeaponAuto(rWeapon);
			}
			UpdateWeaponIK();
		}
	}

	public virtual void SetIKAdjustList(vWeaponIKAdjustList weaponIKAdjustList)
	{
		this.weaponIKAdjustList = weaponIKAdjustList;
		UpdateWeaponIK();
	}

	public virtual void UpdateWeaponIK()
	{
		if ((bool)weaponIKAdjustList && (bool)CurrentWeapon)
		{
			currentWeaponIKAdjust = weaponIKAdjustList.GetWeaponIK(CurrentWeapon.weaponCategory);
		}
	}

	public void OnDestroyWeapon(GameObject otherGameObject)
	{
	}

	public int GetMoveSetID()
	{
		int result = 0;
		if ((bool)rWeapon && rWeapon.gameObject.activeSelf)
		{
			result = (int)rWeapon.moveSetID;
		}
		else if ((bool)lWeapon && lWeapon.gameObject.activeSelf)
		{
			result = (int)lWeapon.moveSetID;
		}
		return result;
	}

	public int GetUpperBodyID()
	{
		int result = 0;
		if ((bool)rWeapon && rWeapon.gameObject.activeSelf)
		{
			result = (int)rWeapon.upperBodyID;
		}
		else if ((bool)lWeapon && lWeapon.gameObject.activeSelf)
		{
			result = (int)lWeapon.upperBodyID;
		}
		return result;
	}

	public int GetShotID()
	{
		int result = 0;
		if ((bool)rWeapon && rWeapon.gameObject.activeSelf)
		{
			result = (int)rWeapon.shotID;
		}
		else if ((bool)lWeapon && lWeapon.gameObject.activeSelf)
		{
			result = (int)lWeapon.shotID;
		}
		return result;
	}

	public int GetAttackID()
	{
		int result = 0;
		if ((bool)rWeapon && rWeapon.gameObject.activeSelf)
		{
			result = (int)rWeapon.shotID;
		}
		else if ((bool)lWeapon && lWeapon.gameObject.activeSelf)
		{
			result = (int)lWeapon.shotID;
		}
		return result;
	}

	public int GetEquipID()
	{
		int result = 0;
		if ((bool)rWeapon && rWeapon.gameObject.activeSelf)
		{
			result = rWeapon.equipID;
		}
		else if ((bool)lWeapon && lWeapon.gameObject.activeSelf)
		{
			result = lWeapon.equipID;
		}
		return result;
	}

	public int GetReloadID()
	{
		int result = 0;
		if ((bool)rWeapon && rWeapon.gameObject.activeSelf)
		{
			result = rWeapon.reloadID;
		}
		else if ((bool)lWeapon && lWeapon.gameObject.activeSelf)
		{
			result = lWeapon.reloadID;
		}
		return result;
	}

	public void ReloadWeapon()
	{
		vShooterWeapon currentWeapon = CurrentWeapon;
		if ((bool)currentWeapon && currentWeapon.gameObject.activeSelf && currentWeapon.ammoCount < currentWeapon.clipSize && !currentWeapon.dontUseReload)
		{
			onReloadWeapon.Invoke(currentWeapon);
			int value = currentWeapon.clipSize - currentWeapon.ammoCount;
			currentWeapon.AddAmmo(value);
			if ((bool)animator)
			{
				animator.SetInteger("ReloadID", GetReloadID());
				animator.SetTrigger("Reload");
			}
			currentWeapon.ReloadEffect();
		}
	}

	protected void ReloadWeaponAuto(vShooterWeapon weapon)
	{
		if ((bool)weapon && weapon.gameObject.activeSelf && weapon.ammoCount < weapon.clipSize)
		{
			int value = weapon.clipSize - weapon.ammoCount;
			weapon.AddAmmo(value);
		}
	}

	public virtual void Shoot(Vector3 aimPosition)
	{
		vShooterWeapon currentWeapon = CurrentWeapon;
		if ((bool)currentWeapon && currentWeapon.gameObject.activeSelf)
		{
			vShooterWeapon vShooterWeapon = currentWeapon;
			if (vShooterWeapon.dontUseReload)
			{
				ReloadWeaponAuto(vShooterWeapon);
			}
			bool applyRecoil = false;
			vShooterWeapon.Shoot(aimPosition, base.transform, delegate(bool sucessful)
			{
				applyRecoil = sucessful;
			});
			if (applyRecoil)
			{
				StartCoroutine(Recoil());
			}
			if (vShooterWeapon.dontUseReload)
			{
				ReloadWeaponAuto(vShooterWeapon);
			}
		}
	}

	private IEnumerator Recoil()
	{
		yield return new WaitForSeconds(0.02f);
		if ((bool)animator)
		{
			animator.SetTrigger("Shoot");
		}
	}
}
