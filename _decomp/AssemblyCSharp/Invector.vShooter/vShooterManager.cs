using System;
using System.Collections;
using System.Collections.Generic;
using Invector.vCamera;
using Invector.vCharacterController;
using Invector.vItemManager;
using Invector.vMelee;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vShooter;

[vClassHeader("SHOOTER MANAGER", true, "icon_v2", false, "", iconName = "shooterIcon")]
public class vShooterManager : vMonoBehaviour, IWeaponEquipmentListener
{
	[Serializable]
	public class OnReloadWeapon : UnityEvent<vShooterWeapon>
	{
	}

	public delegate void AmmoHandle(int ammoID, ref int ammo);

	[vEditorToolbar("Melee Overrides", false, "", false, false)]
	[vHelpBox("Behaviour when Shooter Weapon is Disabled (equipped but disabled)", vHelpBoxAttribute.MessageType.None)]
	public bool canUseMeleeBlock_H = true;

	public bool canUseMeleeWeakAttack_H = true;

	public bool canUseMeleeStrongAttack_H = true;

	[vHelpBox("Behaviour when Shooter Weapon is Enabled (equipped and enabled)", vHelpBoxAttribute.MessageType.None)]
	public bool canUseMeleeBlock_E;

	public bool canUseMeleeWeakAttack_E = true;

	public bool canUseMeleeStrongAttack_E;

	public bool canUseMeleeAiming;

	[vEditorToolbar("Damage Layers", false, "", false, false)]
	[Tooltip("Layer to aim and apply damage")]
	public LayerMask damageLayer = 1;

	[Tooltip("Tags to ignore (auto add this gameObject tag to avoid damage your self)")]
	public vTagMask ignoreTags = new vTagMask("Player");

	[Tooltip("Layer to block aim")]
	public LayerMask blockAimLayer = 1;

	[vEditorToolbar("Cancel Reload", false, "", false, false)]
	[vHelpBox("You can call the <b>CancelReload</b> method using events to interupt the reload routine and animation, for example, when doing an Custom Action or receiving a specific hitReaction ID", vHelpBoxAttribute.MessageType.None)]
	[Tooltip("It will always automatically use the CancelReload")]
	public bool useCancelReload = true;

	[Tooltip("This is a list of HitReaction ID that will be ignored by the CancelReload routine")]
	public List<int> ignoreReacionIDList = new List<int> { -1 };

	[vEditorToolbar("Aim", false, "", false, false)]
	[vSeparator("Float Values", "")]
	public bool useCheckAim = true;

	public float checkAimRadius = 0.1f;

	public float checkAimOffsetZ;

	public float checkAimOffsetSmooth = 2f;

	[vSeparator("Standing", "")]
	public float checkAimStandingOffsetStartY;

	public float checkAimStandingOffsetStartX = 0.2f;

	public float checkAimStandingOffsetEndY;

	public float checkAimStandingOffsetEndX;

	[vSeparator("Crouching", "")]
	public float checkAimCrouchedOffsetStartY;

	public float checkAimCrouchedOffsetStartX = 0.2f;

	public float checkAimCrouchedOffsetEndY;

	public float checkAimCrouchedOffsetEndX;

	[vSeparator("Shooter Settings", "")]
	[Tooltip("The Aim stays active when reload, including animator parameter and camera state")]
	public bool keepAimingWhenReload;

	[Tooltip("Check true to make the character always aim and walk on strafe mode")]
	public bool alwaysAiming;

	public bool onlyWalkWhenAiming = true;

	public bool useDefaultMovesetWhenNotAiming = true;

	[vEditorToolbar("IK Adjust", false, "", false, false)]
	public float armIKSmoothIn = 10f;

	[vEditorToolbar("IK Adjust", false, "", false, false)]
	public float armIKSmoothOut = 25f;

	public AnimationCurve armIKCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	[Tooltip("Control the speed of the Animator Layer OnlyArms Weight")]
	public float onlyArmsSpeed = 25f;

	[Tooltip("smooth of the right hand when correcting the aim")]
	public float smoothArmIKRotation = 30f;

	[Tooltip("smooth of the right arm when correcting the aim")]
	public float smoothArmWeight = 24f;

	[Tooltip("Limit the maxAngle for the right hand to correct the aim")]
	public float maxAimAngle = 60f;

	[Tooltip("Check this to syinc the weapon aim to the camera aim")]
	public bool raycastAimTarget = true;

	[Tooltip("rotate arm ik to aim hit point, if false the arms will rotate to  camera forward distance 100")]
	public bool alignArmToHitPoint = true;

	[Tooltip("Move camera angle when shot using recoil properties of weapon")]
	public bool applyRecoilToCamera = true;

	[Tooltip("Check this to use IK on the left hand")]
	public bool useLeftIK = true;

	[Tooltip("Check this to use IK on the left hand")]
	public bool useRightIK = true;

	[vSeparator("--- Start PlayMode to edit the IK Adjust ---", "")]
	public vWeaponIKAdjustList weaponIKAdjustList;

	public float ikAdjustSmooth = 20f;

	[vEditorToolbar("Ammo", false, "", false, false)]
	[SerializeField]
	protected bool allAmmoInfinity;

	[Tooltip("Use the vAmmoDisplay to shot ammo count")]
	public bool useAmmoDisplay = true;

	[Tooltip("ID to find ammoDisplay for leftWeapon")]
	public int leftWeaponAmmoDisplayID = -1;

	[Tooltip("ID to find ammoDisplay for rightWeapon")]
	public int rightWeaponAmmoDisplayID = 1;

	[vEditorToolbar("LockOn", false, "", false, false)]
	[vSeparator("LockOn (need the shooter lockon component)", "")]
	[Tooltip("Allow the use of the LockOn or not")]
	public bool useLockOn;

	[Tooltip("Allow the use of the LockOn only with a Melee Weapon")]
	public bool useLockOnMeleeOnly = true;

	[vEditorToolbar("HipFire", false, "", false, false)]
	[vSeparator("HipFire Options", "")]
	[Tooltip("If enable, remember to change your weak attack input to other input - this allows shot without aim")]
	public bool hipfireShot;

	[Tooltip("Precision of the weapon when shooting using hipfire (without aiming)")]
	public float hipfireDispersion = 0.5f;

	[Tooltip("Time to keep aiming after shot")]
	[SerializeField]
	public float hipfireAimTime = 2f;

	[vEditorToolbar("Camera Sway", false, "", false, false)]
	[vSeparator("Camera Sway Settings", "")]
	[Tooltip("Camera Sway movement while aiming")]
	public float cameraMaxSwayAmount = 2f;

	[Tooltip("Camera Sway Speed while aiming")]
	public float cameraSwaySpeed = 0.5f;

	[vEditorToolbar("Weapons", false, "", false, false)]
	public vShooterWeapon rWeapon;

	[vEditorToolbar("Weapons", false, "", false, false)]
	public vShooterWeapon lWeapon;

	public int reloadAnimatorLayer = 4;

	[HideInInspector]
	public vAmmoManager ammoManager;

	public AmmoHandle ammoHandle;

	public OnReloadWeapon onStartReloadWeapon;

	public OnReloadWeapon onFinishReloadWeapon;

	[HideInInspector]
	public vAmmoDisplay ammoDisplayR;

	[HideInInspector]
	public vAmmoDisplay ammoDisplayL;

	[HideInInspector]
	public vThirdPersonCamera tpCamera;

	[HideInInspector]
	public bool showCheckAimGizmos;

	internal bool isReloadingWeapon;

	protected Animator animator;

	protected bool usingThirdPersonController;

	protected float hipfirePrecisionAngle;

	protected float hipfirePrecision;

	protected bool cancelReload;

	protected bool isReloading;

	protected float reloadStartTime;

	protected vWeaponIKAdjust currentWeaponIKAdjust;

	internal readonly int IsShoot = Animator.StringToHash("Shoot");

	internal readonly int Reload = Animator.StringToHash("Reload");

	internal readonly int ReloadID = Animator.StringToHash("ReloadID");

	protected int extraAmmo;

	public OnEquipWeaponEvent onEquipWeapon;

	public virtual int ExtraAmmo => extraAmmo;

	public bool AllAmmoInfinity
	{
		get
		{
			return allAmmoInfinity;
		}
		set
		{
			allAmmoInfinity = value;
		}
	}

	public virtual float HipfireAimTime => hipfireAimTime + (CurrentWeapon ? CurrentWeapon.shootFrequency : 0f);

	public virtual bool isShooting
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

	public virtual bool isShootingEmptyClip
	{
		get
		{
			if ((bool)CurrentWeapon)
			{
				return !CurrentWeapon.CanDoEmptyClip;
			}
			return false;
		}
	}

	public virtual vShooterWeapon CurrentWeapon
	{
		get
		{
			vShooterWeapon vShooterWeapon2 = (rWeapon ? rWeapon : (lWeapon ? lWeapon : null));
			if (!(vShooterWeapon2 != null) || vShooterWeapon2.inHolder)
			{
				return null;
			}
			return vShooterWeapon2;
		}
	}

	public virtual vWeaponIKAdjust CurrentWeaponIK => currentWeaponIKAdjust;

	public virtual bool IsLeftWeapon
	{
		get
		{
			if (!(rWeapon == null))
			{
				return rWeapon.isLeftWeapon;
			}
			return lWeapon;
		}
	}

	public virtual void Start()
	{
		animator = GetComponent<Animator>();
		if (applyRecoilToCamera)
		{
			tpCamera = UnityEngine.Object.FindObjectOfType<vThirdPersonCamera>();
		}
		ammoManager = GetComponent<vAmmoManager>();
		if (ammoManager != null)
		{
			ammoManager.updateTotalAmmo = AmmoManagerWasUpdated;
		}
		vThirdPersonController component = GetComponent<vThirdPersonController>();
		usingThirdPersonController = component;
		if (usingThirdPersonController && useCancelReload)
		{
			component.onReceiveDamage.AddListener(CancelReload);
		}
		if (useAmmoDisplay)
		{
			GetAmmoDisplays();
		}
		if ((bool)animator)
		{
			Transform boneTransform = animator.GetBoneTransform(HumanBodyBones.RightHand);
			Transform boneTransform2 = animator.GetBoneTransform(HumanBodyBones.LeftHand);
			vShooterWeapon componentInChildren = boneTransform.GetComponentInChildren<vShooterWeapon>();
			vShooterWeapon componentInChildren2 = boneTransform2.GetComponentInChildren<vShooterWeapon>();
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
		if (useAmmoDisplay)
		{
			if ((bool)ammoDisplayR)
			{
				ammoDisplayR.UpdateDisplay("");
			}
			if ((bool)ammoDisplayL)
			{
				ammoDisplayL.UpdateDisplay("");
			}
		}
		UpdateTotalAmmo();
	}

	public virtual void SetLeftWeapon(GameObject weapon)
	{
		if (weapon != null)
		{
			vShooterWeapon component = weapon.GetComponent<vShooterWeapon>();
			SetLeftWeapon(component);
		}
		else
		{
			lWeapon = null;
		}
	}

	protected virtual void SetLeftWeapon(vShooterWeapon weapon)
	{
		lWeapon = weapon;
		if (!lWeapon)
		{
			return;
		}
		lWeapon.inHolder = false;
		lWeapon.ignoreTags = ignoreTags;
		lWeapon.hitLayer = damageLayer;
		lWeapon.root = base.transform;
		lWeapon.onDisable.RemoveListener(HideLeftAmmoDisplay);
		lWeapon.onDisable.AddListener(HideLeftAmmoDisplay);
		lWeapon.onDestroy.RemoveListener(OnDestroyWeapon);
		lWeapon.onDestroy.AddListener(OnDestroyWeapon);
		CollectExtraAmmo(weapon);
		if (lWeapon.dontUseReload)
		{
			LoadAllAmmo(lWeapon);
		}
		if (usingThirdPersonController)
		{
			if (useAmmoDisplay && !ammoDisplayL)
			{
				GetAmmoDisplays();
			}
			if (useAmmoDisplay && (bool)ammoDisplayL)
			{
				ammoDisplayL.Show();
			}
			UpdateLeftAmmo();
		}
		UpdateWeaponIK();
		onEquipWeapon.Invoke(weapon.gameObject, arg1: true);
	}

	public virtual void SetRightWeapon(GameObject weapon)
	{
		if (weapon != null)
		{
			vShooterWeapon component = weapon.GetComponent<vShooterWeapon>();
			SetRightWeapon(component);
			onEquipWeapon.Invoke(weapon.gameObject, arg1: true);
		}
		else
		{
			rWeapon = null;
		}
	}

	protected virtual void SetRightWeapon(vShooterWeapon weapon)
	{
		rWeapon = weapon;
		if (!rWeapon)
		{
			return;
		}
		rWeapon.inHolder = false;
		rWeapon.ignoreTags = ignoreTags;
		rWeapon.hitLayer = damageLayer;
		rWeapon.root = base.transform;
		rWeapon.onDisable.RemoveListener(HideRightAmmoDisplay);
		rWeapon.onDisable.AddListener(HideRightAmmoDisplay);
		rWeapon.onDestroy.RemoveListener(OnDestroyWeapon);
		rWeapon.onDestroy.AddListener(OnDestroyWeapon);
		if (rWeapon.dontUseReload)
		{
			LoadAllAmmo(rWeapon);
		}
		CollectExtraAmmo(weapon);
		if (usingThirdPersonController)
		{
			if (useAmmoDisplay && !ammoDisplayR)
			{
				GetAmmoDisplays();
			}
			if (useAmmoDisplay && (bool)ammoDisplayR)
			{
				ammoDisplayR.Show();
			}
			UpdateRightAmmo();
		}
		UpdateWeaponIK();
		onEquipWeapon.Invoke(weapon.gameObject, arg1: false);
	}

	protected virtual void CollectExtraAmmo(vShooterWeapon weapon)
	{
		if (weapon.ammoCount > weapon.clipSize)
		{
			int num = weapon.ammo - weapon.clipSize;
			weapon.ammo -= num;
			if ((bool)ammoManager)
			{
				ammoManager.AddAmmo(weapon.ammoID, num);
			}
		}
	}

	protected virtual void HideLeftAmmoDisplay()
	{
		HideAmmoDisplay(ammoDisplayL);
	}

	protected virtual void HideRightAmmoDisplay()
	{
		HideAmmoDisplay(ammoDisplayR);
	}

	protected virtual void HideAmmoDisplay(vAmmoDisplay ammoDisplay)
	{
		if (useAmmoDisplay && (bool)ammoDisplay)
		{
			ammoDisplay.UpdateDisplay("");
			ammoDisplay.Hide();
		}
	}

	public virtual void OnDestroyWeapon(GameObject otherGameObject)
	{
		if (usingThirdPersonController)
		{
			vAmmoDisplay ammoDisplay = ((rWeapon != null && otherGameObject == rWeapon.gameObject) ? ammoDisplayR : ((lWeapon != null && otherGameObject == lWeapon.gameObject) ? ammoDisplayL : null));
			HideAmmoDisplay(ammoDisplay);
		}
	}

	protected virtual void GetAmmoDisplays()
	{
		vAmmoDisplay[] array = UnityEngine.Object.FindObjectsOfType<vAmmoDisplay>();
		if (array.Length == 0)
		{
			return;
		}
		if (!ammoDisplayL)
		{
			ammoDisplayL = array.vToList().Find((vAmmoDisplay d) => d.displayID == leftWeaponAmmoDisplayID);
		}
		if (!ammoDisplayR)
		{
			ammoDisplayR = array.vToList().Find((vAmmoDisplay d) => d.displayID == rightWeaponAmmoDisplayID);
		}
	}

	public virtual int GetMoveSetID()
	{
		int result = 0;
		if ((bool)rWeapon && rWeapon.gameObject.activeInHierarchy)
		{
			result = (int)rWeapon.moveSetID;
		}
		else if ((bool)lWeapon && lWeapon.gameObject.activeInHierarchy)
		{
			result = (int)lWeapon.moveSetID;
		}
		return result;
	}

	public virtual int GetUpperBodyID()
	{
		int result = 0;
		if ((bool)rWeapon && rWeapon.gameObject.activeInHierarchy)
		{
			result = (int)rWeapon.upperBodyID;
		}
		else if ((bool)lWeapon && lWeapon.gameObject.activeInHierarchy)
		{
			result = (int)lWeapon.upperBodyID;
		}
		return result;
	}

	public virtual int GetShotID()
	{
		int result = 0;
		if ((bool)rWeapon && rWeapon.gameObject.activeInHierarchy)
		{
			result = (int)rWeapon.shotID;
		}
		else if ((bool)lWeapon && lWeapon.gameObject.activeInHierarchy)
		{
			result = (int)lWeapon.shotID;
		}
		return result;
	}

	public virtual int GetEquipID()
	{
		int result = 0;
		if ((bool)rWeapon && rWeapon.gameObject.activeInHierarchy)
		{
			result = rWeapon.equipID;
		}
		else if ((bool)lWeapon && lWeapon.gameObject.activeInHierarchy)
		{
			result = lWeapon.equipID;
		}
		return result;
	}

	public virtual int GetReloadID()
	{
		int result = 0;
		if ((bool)rWeapon && rWeapon.gameObject.activeInHierarchy)
		{
			result = rWeapon.reloadID;
		}
		else if ((bool)lWeapon && lWeapon.gameObject.activeInHierarchy)
		{
			result = lWeapon.reloadID;
		}
		return result;
	}

	public virtual bool WeaponHasLoadedAmmo()
	{
		return (((bool)CurrentWeapon && CurrentWeapon.ammoCount != 0) ? 1 : 0) > (false ? 1 : 0);
	}

	public virtual bool WeaponHasUnloadedAmmo()
	{
		return extraAmmo > 0;
	}

	public virtual void ReloadWeapon()
	{
		vShooterWeapon vShooterWeapon2 = (rWeapon ? rWeapon : lWeapon);
		if (!vShooterWeapon2 || !vShooterWeapon2.gameObject.activeInHierarchy || isReloading)
		{
			return;
		}
		UpdateTotalAmmo();
		if (vShooterWeapon2.ammoCount < vShooterWeapon2.clipSize && (vShooterWeapon2.isInfinityAmmo || AllAmmoInfinity || WeaponHasUnloadedAmmo()) && !vShooterWeapon2.dontUseReload)
		{
			onStartReloadWeapon.Invoke(vShooterWeapon2);
			if ((bool)animator)
			{
				animator.SetInteger(ReloadID, GetReloadID());
				animator.SetTrigger(Reload);
			}
			if ((bool)CurrentWeapon && CurrentWeapon.gameObject.activeInHierarchy)
			{
				StartCoroutine(AddAmmoToWeapon(CurrentWeapon, CurrentWeapon.reloadTime));
			}
		}
	}

	protected virtual IEnumerator AddAmmoToWeapon(vShooterWeapon weapon, float delayTime)
	{
		isReloading = true;
		isReloadingWeapon = true;
		reloadStartTime = Time.time;
		if (weapon.ammoCount < weapon.clipSize && (weapon.isInfinityAmmo || AllAmmoInfinity || WeaponHasUnloadedAmmo()) && !weapon.dontUseReload && !cancelReload)
		{
			weapon.ReloadEffect();
			yield return new WaitForSeconds(delayTime);
			if (!cancelReload)
			{
				int num = (weapon.reloadOneByOne ? 1 : (weapon.clipSize - weapon.ammoCount));
				if (weapon.isInfinityAmmo || AllAmmoInfinity)
				{
					weapon.AddAmmo(num);
				}
				else
				{
					if (WeaponAmmo(weapon).count < num)
					{
						num = WeaponAmmo(weapon).count;
					}
					weapon.AddAmmo(num);
					WeaponAmmo(weapon).Use(num);
				}
				if (weapon.reloadOneByOne && weapon.ammoCount < weapon.clipSize && WeaponHasUnloadedAmmo())
				{
					if (WeaponAmmo(weapon).count == 0)
					{
						weapon.FinishReloadEffect();
						isReloadingWeapon = false;
						onFinishReloadWeapon.Invoke(weapon);
					}
					else
					{
						isReloadingWeapon = true;
						if (!cancelReload)
						{
							animator.SetInteger(ReloadID, weapon.reloadID);
							animator.SetTrigger(Reload);
							StartCoroutine(AddAmmoToWeapon(weapon, delayTime));
						}
					}
				}
				else
				{
					weapon.FinishReloadEffect();
					isReloadingWeapon = false;
					onFinishReloadWeapon.Invoke(weapon);
				}
			}
			UpdateTotalAmmo();
		}
		isReloading = false;
	}

	public virtual void CancelReload()
	{
		if (isReloading)
		{
			StartCoroutine(CancelReloadRoutine());
		}
	}

	public virtual void CancelReload(vDamage damage)
	{
		if (!ignoreReacionIDList.Contains(damage.reaction_id) && isReloading)
		{
			StartCoroutine(CancelReloadRoutine());
		}
	}

	protected virtual IEnumerator CancelReloadRoutine()
	{
		if (!(CurrentWeapon != null))
		{
			yield break;
		}
		animator.SetTrigger("CancelReload");
		animator.ResetTrigger("Reload");
		cancelReload = true;
		StopCoroutine("AddAmmoToWeapon");
		if ((bool)CurrentWeapon)
		{
			CurrentWeapon.CancelReload();
		}
		yield return new WaitForSeconds(CurrentWeapon.reloadTime + 0.1f);
		cancelReload = false;
		if (isReloadingWeapon)
		{
			isReloadingWeapon = false;
			if ((bool)CurrentWeapon)
			{
				onFinishReloadWeapon.Invoke(CurrentWeapon);
			}
		}
		animator.ResetTrigger("CancelReload");
		UpdateTotalAmmo();
	}

	public virtual void LoadAllAmmo(vShooterWeapon weapon)
	{
		if (!weapon)
		{
			return;
		}
		UpdateTotalAmmo();
		if (weapon.ammoCount >= weapon.clipSize || (!weapon.isInfinityAmmo && !AllAmmoInfinity && !WeaponHasUnloadedAmmo()))
		{
			return;
		}
		int num = weapon.clipSize - weapon.ammoCount;
		if (weapon.isInfinityAmmo || AllAmmoInfinity)
		{
			weapon.AddAmmo(num);
		}
		else
		{
			if (WeaponAmmo(weapon).count < num)
			{
				num = WeaponAmmo(weapon).count;
			}
			weapon.AddAmmo(num);
			WeaponAmmo(weapon).Use(num);
		}
		weapon.onReload.Invoke();
	}

	public virtual vAmmo WeaponAmmo(vShooterWeapon weapon)
	{
		if (!weapon)
		{
			return null;
		}
		vAmmo result = new vAmmo();
		if ((bool)ammoManager && ammoManager.ammos != null && ammoManager.ammos.Count > 0)
		{
			result = ammoManager.GetAmmo(weapon.ammoID);
		}
		return result;
	}

	public virtual void SetIKAdjustList(vWeaponIKAdjustList weaponIKAdjustList)
	{
		this.weaponIKAdjustList = weaponIKAdjustList;
		if ((bool)CurrentWeapon)
		{
			currentWeaponIKAdjust = weaponIKAdjustList.GetWeaponIK(CurrentWeapon.weaponCategory);
		}
	}

	public virtual void UpdateWeaponIK()
	{
		if ((bool)weaponIKAdjustList && (bool)CurrentWeapon)
		{
			currentWeaponIKAdjust = weaponIKAdjustList.GetWeaponIK(CurrentWeapon.weaponCategory);
		}
	}

	public virtual void AmmoManagerWasUpdated()
	{
		bool flag = true;
		if ((bool)CurrentWeapon && CurrentWeapon.dontUseReload)
		{
			LoadAllAmmo(CurrentWeapon);
			flag = false;
		}
		if (flag)
		{
			UpdateTotalAmmo();
		}
	}

	public virtual void UpdateTotalAmmo()
	{
		UpdateLeftAmmo();
		UpdateRightAmmo();
	}

	public virtual void UpdateLeftAmmo()
	{
		if ((bool)lWeapon)
		{
			UpdateTotalAmmo(lWeapon, ref extraAmmo, -1);
		}
	}

	public virtual bool IsCurrentWeaponActive()
	{
		if ((bool)CurrentWeapon && CurrentWeapon.gameObject.activeInHierarchy)
		{
			return !CurrentWeapon.inHolder;
		}
		return false;
	}

	public virtual void UpdateRightAmmo()
	{
		if ((bool)rWeapon)
		{
			UpdateTotalAmmo(rWeapon, ref extraAmmo, 1);
		}
	}

	protected virtual void UpdateTotalAmmo(vShooterWeapon weapon, ref int targetTotalAmmo, int displayId)
	{
		if (!weapon)
		{
			return;
		}
		int num = 0;
		if (weapon.isInfinityAmmo || AllAmmoInfinity)
		{
			num = 9999;
		}
		else
		{
			vAmmo vAmmo = WeaponAmmo(weapon);
			if (vAmmo != null)
			{
				num += vAmmo.count;
			}
		}
		targetTotalAmmo = num;
		UpdateAmmoDisplay(displayId);
	}

	protected virtual void UpdateAmmoDisplay(int displayId)
	{
		if (useAmmoDisplay)
		{
			vShooterWeapon vShooterWeapon2 = ((displayId == 1) ? rWeapon : lWeapon);
			if (!ammoDisplayR || !ammoDisplayL)
			{
				GetAmmoDisplays();
			}
			vAmmoDisplay vAmmoDisplay = ((displayId == 1) ? ammoDisplayR : ammoDisplayL);
			if (useAmmoDisplay && (bool)vAmmoDisplay)
			{
				string text = ((!vShooterWeapon2.dontUseReload) ? vShooterWeapon2.ammoCount.ToString("00") : ((vShooterWeapon2.isInfinityAmmo || AllAmmoInfinity) ? "∞" : (vShooterWeapon2.ammoCount + extraAmmo).ToString("00")));
				string text2 = ((vShooterWeapon2.dontUseReload && (vShooterWeapon2.isInfinityAmmo || AllAmmoInfinity)) ? "" : ((!vShooterWeapon2.dontUseReload && (vShooterWeapon2.isInfinityAmmo || AllAmmoInfinity)) ? "∞" : ((vShooterWeapon2.dontUseReload && (!vShooterWeapon2.isInfinityAmmo || !AllAmmoInfinity)) ? "" : extraAmmo.ToString("00"))));
				vAmmoDisplay.UpdateDisplay(text, text2, vShooterWeapon2.ammoID);
			}
		}
	}

	public virtual void Shoot(Vector3 aimPosition, bool applyHipfirePrecision = false)
	{
		if (isShooting)
		{
			return;
		}
		vShooterWeapon vShooterWeapon2 = (rWeapon ? rWeapon : lWeapon);
		if ((bool)vShooterWeapon2 && vShooterWeapon2.gameObject.activeInHierarchy)
		{
			if (vShooterWeapon2.dontUseReload)
			{
				LoadAllAmmo(vShooterWeapon2);
			}
			else if (vShooterWeapon2.autoReload && vShooterWeapon2.ammoCount <= 0 && WeaponHasUnloadedAmmo())
			{
				ReloadWeapon();
				return;
			}
			Vector3 aimPosition2 = (applyHipfirePrecision ? (aimPosition + HipFirePrecision(aimPosition)) : aimPosition);
			bool applyRecoil = false;
			vShooterWeapon2.Shoot(aimPosition2, base.transform, delegate(bool sucessful)
			{
				applyRecoil = sucessful;
			});
			if (applyRecoil)
			{
				float horizontal = UnityEngine.Random.Range(vShooterWeapon2.recoilLeft, vShooterWeapon2.recoilRight);
				float up = UnityEngine.Random.Range(0f, vShooterWeapon2.recoilUp);
				StartCoroutine(Recoil(horizontal, up));
			}
			UpdateAmmoDisplay(rWeapon ? 1 : (-1));
			if (vShooterWeapon2.dontUseReload)
			{
				LoadAllAmmo(vShooterWeapon2);
			}
			if (extraAmmo <= 0)
			{
				vShooterWeapon2.onFinishAmmo.Invoke();
			}
		}
	}

	protected virtual IEnumerator Recoil(float horizontal, float up)
	{
		yield return new WaitForSeconds(0.02f);
		if ((bool)animator)
		{
			animator.SetTrigger(IsShoot);
		}
		if (tpCamera != null && applyRecoilToCamera)
		{
			tpCamera.RotateCamera(horizontal, up);
		}
	}

	protected virtual Vector3 HipFirePrecision(Vector3 _aimPosition)
	{
		vShooterWeapon vShooterWeapon2 = (rWeapon ? rWeapon : lWeapon);
		if (!vShooterWeapon2)
		{
			return Vector3.zero;
		}
		hipfirePrecisionAngle = UnityEngine.Random.Range(-1000, 1000);
		hipfirePrecision = UnityEngine.Random.Range(0f - hipfireDispersion, hipfireDispersion);
		return (Quaternion.AngleAxis(hipfirePrecisionAngle, _aimPosition - vShooterWeapon2.muzzle.position) * Vector3.up).normalized * hipfirePrecision;
	}

	public virtual void CameraSway()
	{
		vShooterWeapon vShooterWeapon2 = (rWeapon ? rWeapon : lWeapon);
		if (!vShooterWeapon2)
		{
			return;
		}
		float num = Mathf.PerlinNoise(0f, Time.time * cameraSwaySpeed) - 0.5f;
		float num2 = Mathf.PerlinNoise(0f, Time.time * cameraSwaySpeed + 100f) - 0.5f;
		float num3 = cameraMaxSwayAmount * (1f - vShooterWeapon2.cameraStability);
		if (num3 != 0f)
		{
			num *= num3;
			num2 *= num3;
			float num4 = Mathf.PerlinNoise(0f, Time.time * cameraSwaySpeed) - 0.5f;
			float num5 = Mathf.PerlinNoise(0f, Time.time * cameraSwaySpeed + 100f) - 0.5f;
			num4 *= 0f - num3 * 0.25f;
			num5 *= num3 * 0.25f;
			if (tpCamera != null)
			{
				tpCamera.offsetMouse.x = num + num4;
				tpCamera.offsetMouse.y = num2 + num5;
			}
		}
	}
}
