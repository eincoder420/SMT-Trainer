using System;
using System.Runtime.CompilerServices;
using Invector.IK;
using Invector.PlayerController;
using Invector.vCamera;
using Invector.vShooter;
using UnityEngine;

namespace Invector.vCharacterController;

[vClassHeader("SHOOTER/MELEE INPUT", true, "icon_v2", false, "", iconName = "inputIcon")]
public class vShooterMeleeInput : vMeleeCombatInput, vIShooterIKController, vILockCamera
{
	[vEditorToolbar("Inputs", false, "", false, false)]
	[Header("Shooter Inputs")]
	public GenericInput aimInput = new GenericInput("Mouse1", keyboardAxis: false, "LT", joystickAxis: true, "LT", mobileAxis: false);

	public GenericInput shotInput = new GenericInput("Mouse0", keyboardAxis: false, "RT", joystickAxis: true, "RT", mobileAxis: false);

	public GenericInput reloadInput = new GenericInput("R", "LB", "LB");

	public GenericInput switchCameraSideInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");

	public GenericInput scopeViewInput = new GenericInput("Z", "RB", "RB");

	[HideInInspector]
	public vShooterManager shooterManager;

	internal bool isAimingByInput;

	internal bool isReloading;

	internal bool defaultStrafeWalk;

	internal Transform leftHand;

	internal Transform rightHand;

	internal Transform rightLowerArm;

	internal Transform leftLowerArm;

	internal Transform rightUpperArm;

	internal Transform leftUpperArm;

	internal bool aimConditions;

	internal bool ignoreIK;

	protected int onlyArmsLayer;

	protected int shotLayer;

	protected int shootCountA;

	protected bool allowAttack;

	protected bool isUsingScopeView;

	protected bool isCameraRightSwitched;

	protected float onlyArmsLayerWeight;

	protected float supportIKWeight;

	protected float weaponIKWeight;

	protected float armAlignmentWeight;

	protected float aimWeight;

	protected float lastAimDistance;

	protected Quaternion handRotation;

	protected Quaternion upperArmRotation;

	protected vHeadTrack headTrack;

	protected bool lastRotateWithCamera;

	protected vControlAimCanvas _controlAimCanvas;

	protected GameObject aimAngleReference;

	protected Vector3 ikRotationOffset;

	protected Vector3 ikPositionOffset;

	protected bool checkCanAimInit;

	protected GameObject lastShooterWeapon;

	protected Quaternion upperArmRotationAlignment;

	protected Quaternion handRotationAlignment;

	protected bool _ignoreIKFromAnimator;

	protected bool _walkingByDefaultWasChanged;

	protected float _aimTimming;

	protected float checkCanAimOffsetStartX;

	protected float checkCanAimOffsetStartY;

	protected float checkCanAimOffsetEndX;

	protected float checkCanAimOffsetEndY;

	protected float checkCanAimHeight;

	protected float scopeDirectionWeight;

	protected RaycastHit checkCanAimHit;

	protected Vector3 aimHitPoint;

	protected Vector3 upperArmPosition;

	protected Vector3 muzzlePosition;

	protected Vector3 muzzleForward;

	protected IKAdjust _currentIKAdjust;

	internal bool lockShooterInput;

	public virtual vIKSolver LeftIK { get; set; }

	public virtual vIKSolver RightIK { get; set; }

	public virtual vWeaponIKAdjustList WeaponIKAdjustList
	{
		get
		{
			if ((bool)shooterManager)
			{
				return shooterManager.weaponIKAdjustList;
			}
			return null;
		}
		set
		{
			if ((bool)shooterManager)
			{
				shooterManager.weaponIKAdjustList = value;
			}
		}
	}

	public virtual vWeaponIKAdjust CurrentWeaponIK
	{
		get
		{
			if ((bool)shooterManager)
			{
				return shooterManager.CurrentWeaponIK;
			}
			return null;
		}
	}

	public virtual IKAdjust CurrentIKAdjust
	{
		get
		{
			if (CurrentWeaponIK == null)
			{
				return null;
			}
			if (CurrentIKAdjustStateWithTag != IKWeaponTag + TargetIKAdjustState || _currentIKAdjust == null)
			{
				CurrentIKAdjustStateWithTag = IKWeaponTag + TargetIKAdjustState;
				CurrentIKAdjustState = TargetIKAdjustState;
				_currentIKAdjust = CurrentWeaponIK.GetIKAdjust(CurrentIKAdjustState, CurrentActiveWeapon.isLeftWeapon);
			}
			return _currentIKAdjust;
		}
	}

	public virtual bool EditingIKGlobalOffset { get; set; }

	public virtual string DefaultIKAdjustState
	{
		get
		{
			if (!CurrentWeaponIK)
			{
				return string.Empty;
			}
			return CurrentWeaponIK.GetDefaultStateName(this);
		}
	}

	protected virtual string TargetIKAdjustState
	{
		get
		{
			if (IsUsingCustomIKAdjust)
			{
				return CustomIKAdjustState;
			}
			return DefaultIKAdjustState;
		}
	}

	protected virtual string IKWeaponTag
	{
		get
		{
			if (!CurrentActiveWeapon)
			{
				return "";
			}
			return CurrentActiveWeapon.weaponCategory + "@";
		}
	}

	public virtual string CurrentIKAdjustStateWithTag { get; set; }

	public virtual string CurrentIKAdjustState { get; protected set; }

	public virtual bool IsUsingCustomIKAdjust => !string.IsNullOrEmpty(CustomIKAdjustState);

	public string CustomIKAdjustState { get; protected set; }

	public virtual bool IsIgnoreIK
	{
		get
		{
			if (!ignoreIK)
			{
				return _ignoreIKFromAnimator;
			}
			return true;
		}
	}

	public virtual bool IsSupportHandIKEnabled { get; protected set; }

	public virtual Vector3 AimPosition { get; protected set; }

	public virtual bool LockAiming
	{
		get
		{
			if ((bool)shooterManager)
			{
				return shooterManager.alwaysAiming;
			}
			return false;
		}
		set
		{
			shooterManager.alwaysAiming = value;
		}
	}

	public virtual bool LockHipFireAiming { get; set; }

	public virtual bool IsCrouching
	{
		get
		{
			return cc.isCrouching;
		}
		set
		{
			cc.isCrouching = value;
		}
	}

	public virtual bool IsLeftWeapon
	{
		get
		{
			if ((bool)shooterManager)
			{
				return shooterManager.IsLeftWeapon;
			}
			return false;
		}
	}

	public virtual bool LockCamera
	{
		get
		{
			if ((bool)tpCamera)
			{
				return tpCamera.LockCamera;
			}
			return false;
		}
		set
		{
			if ((bool)tpCamera)
			{
				tpCamera.LockCamera = value;
			}
		}
	}

	public virtual bool IsAiming
	{
		get
		{
			if (!cc.isRolling)
			{
				if (!isAimingByInput)
				{
					return isAimingByHipFire;
				}
				return true;
			}
			return false;
		}
	}

	public virtual bool isAimingByHipFire
	{
		get
		{
			if ((!shooterManager.hipfireShot && _aimTimming > 0f) || (isReloading && !shooterManager.keepAimingWhenReload) || base.isEquipping)
			{
				_aimTimming = 0f;
				return false;
			}
			if (shooterManager.hipfireShot)
			{
				if (!(_aimTimming > 0f) && (!shotInput.GetButton() || !(shooterManager.CurrentWeapon != null)))
				{
					if (!isAimingByInput)
					{
						return shootCountA > 0;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public virtual vControlAimCanvas controlAimCanvas
	{
		get
		{
			if (!_controlAimCanvas)
			{
				_controlAimCanvas = UnityEngine.Object.FindObjectOfType<vControlAimCanvas>();
				if ((bool)_controlAimCanvas)
				{
					_controlAimCanvas.Init(cc);
				}
			}
			return _controlAimCanvas;
		}
	}

	public override bool lockInventory
	{
		get
		{
			if (!base.lockInventory && !isReloading && !cc.customAction)
			{
				return cc.isRolling;
			}
			return true;
		}
	}

	public virtual vShooterWeapon CurrentActiveWeapon
	{
		get
		{
			if (!shooterManager.CurrentWeapon || !shooterManager.IsCurrentWeaponActive())
			{
				return null;
			}
			return shooterManager.CurrentWeapon;
		}
	}

	public virtual int shooterMoveSetID
	{
		get
		{
			int moveSetID = shooterManager.GetMoveSetID();
			if (moveSetID == 0 || overrideWeaponMoveSetID)
			{
				moveSetID = defaultMoveSetID;
			}
			return moveSetID;
		}
	}

	protected virtual Vector3 targetArmAlignmentPosition
	{
		get
		{
			if (!isUsingScopeView || !controlAimCanvas.scopeBackgroundCamera)
			{
				if (!shooterManager.alignArmToHitPoint)
				{
					return base.cameraMain.transform.position + base.cameraMain.transform.forward * 100f;
				}
				return AimPosition;
			}
			return base.cameraMain.transform.position + base.cameraMain.transform.forward * lastAimDistance;
		}
	}

	protected virtual Vector3 targetArmAligmentDirection => (((bool)controlAimCanvas && controlAimCanvas.isScopeCameraActive && (bool)controlAimCanvas.scopeBackgroundCamera) ? controlAimCanvas.scopeBackgroundCamera.transform : base.cameraMain.transform).forward;

	public event IKUpdateEvent onStartUpdateIK;

	public event IKUpdateEvent onFinishUpdateIK;

	public virtual void SetCustomIKAdjustState(string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			CustomIKAdjustState = value;
		}
	}

	public virtual void ResetCustomIKAdjustState()
	{
		if (!string.IsNullOrEmpty(CustomIKAdjustState))
		{
			CustomIKAdjustState = string.Empty;
		}
	}

	public virtual void UpdateWeaponIK()
	{
		if ((bool)shooterManager)
		{
			shooterManager.UpdateWeaponIK();
			if (!(CurrentWeaponIK == null))
			{
				_currentIKAdjust = CurrentWeaponIK.GetIKAdjust(CurrentIKAdjustState, CurrentActiveWeapon.isLeftWeapon);
			}
		}
	}

	protected override void Start()
	{
		shooterManager = GetComponent<vShooterManager>();
		base.Start();
		checkCanAimHeight = cc._capsuleCollider.height;
		leftHand = base.animator.GetBoneTransform(HumanBodyBones.LeftHand);
		rightHand = base.animator.GetBoneTransform(HumanBodyBones.RightHand);
		leftLowerArm = base.animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
		rightLowerArm = base.animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
		leftUpperArm = base.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
		rightUpperArm = base.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
		onlyArmsLayer = base.animator.GetLayerIndex("OnlyArms");
		shotLayer = base.animator.GetLayerIndex("Shot");
		aimAngleReference = new GameObject("aimAngleReference");
		aimAngleReference.tag = "Ignore Ragdoll";
		aimAngleReference.transform.rotation = base.transform.rotation;
		Transform boneTransform = base.animator.GetBoneTransform(HumanBodyBones.Head);
		aimAngleReference.transform.SetParent(boneTransform);
		aimAngleReference.transform.localPosition = Vector3.zero;
		defaultStrafeWalk = cc.strafeSpeed.walkByDefault;
		headTrack = GetComponent<vHeadTrack>();
		lastRotateWithCamera = cc.strafeSpeed.rotateWithCamera;
		if ((bool)headTrack)
		{
			headTrack.onInitUpdate.AddListener(UpdateAimAngleReference);
		}
		if (!controlAimCanvas)
		{
			Debug.LogWarning("Missing the AimCanvas, drag and drop the prefab to this scene in order to Aim", base.gameObject);
		}
	}

	protected override void LateUpdate()
	{
		if (updateIK || base.animator.updateMode != AnimatorUpdateMode.AnimatePhysics)
		{
			base.LateUpdate();
			UpdateAimBehaviour();
		}
	}

	protected virtual void Reset()
	{
		weakAttackInput = new GenericInput("Mouse2", "RB", "RB");
		strafeInput.useInput = false;
	}

	public virtual void SetLockShooterInput(bool value)
	{
		lockShooterInput = value;
		if (value)
		{
			base.isBlocking = false;
			isAimingByInput = false;
			_aimTimming = 0f;
			if ((bool)controlAimCanvas)
			{
				controlAimCanvas.SetActiveAim(value: false);
				controlAimCanvas.SetActiveScopeCamera(value: false);
			}
		}
	}

	public override void SetLockAllInput(bool value)
	{
		base.SetLockAllInput(value);
		SetLockShooterInput(value);
	}

	public virtual void SetAlwaysAim(bool value)
	{
		shooterManager.alwaysAiming = value;
	}

	protected override void InputHandle()
	{
		if (cc == null || cc.isDead)
		{
			AimInput();
			return;
		}
		if (!cc.ragdolled && !lockInput)
		{
			MoveInput();
			SprintInput();
			CrouchInput();
			StrafeInput();
			JumpInput();
			RollInput();
		}
		if (MeleeAttackConditions() && !IsAiming && !isReloading && !lockMeleeInput && !CurrentActiveWeapon)
		{
			if (shooterManager.canUseMeleeWeakAttack_H || shooterManager.CurrentWeapon == null)
			{
				MeleeWeakAttackInput();
			}
			if (shooterManager.canUseMeleeStrongAttack_H || shooterManager.CurrentWeapon == null)
			{
				MeleeStrongAttackInput();
			}
			if (shooterManager.canUseMeleeBlock_H || shooterManager.CurrentWeapon == null)
			{
				BlockingInput();
			}
			else
			{
				base.isBlocking = false;
			}
		}
		if (lockShooterInput)
		{
			isAimingByInput = false;
			_aimTimming = 0f;
			if (controlAimCanvas != null)
			{
				if (controlAimCanvas.isAimActive)
				{
					controlAimCanvas.SetActiveAim(value: false);
				}
				if (controlAimCanvas.isScopeCameraActive)
				{
					controlAimCanvas.SetActiveScopeCamera(value: false);
				}
			}
			return;
		}
		if ((bool)shooterManager.CurrentWeapon)
		{
			if (MeleeAttackConditions() && (!IsAiming || shooterManager.canUseMeleeAiming))
			{
				if (shooterManager.canUseMeleeWeakAttack_E)
				{
					MeleeWeakAttackInput();
				}
				if (shooterManager.canUseMeleeStrongAttack_E)
				{
					MeleeStrongAttackInput();
				}
				if (shooterManager.canUseMeleeBlock_E)
				{
					BlockingInput();
				}
				else
				{
					base.isBlocking = false;
				}
			}
			else
			{
				base.isBlocking = false;
			}
			if (shooterManager == null || CurrentActiveWeapon == null || base.isEquipping)
			{
				if (IsAiming)
				{
					isAimingByInput = false;
					_aimTimming = 0f;
					if (!cc.lockInStrafe && cc.isStrafing)
					{
						cc.Strafe();
					}
					if (controlAimCanvas != null)
					{
						controlAimCanvas.SetActiveAim(value: false);
						controlAimCanvas.SetActiveScopeCamera(value: false);
					}
					if ((bool)shooterManager && (bool)shooterManager.CurrentWeapon && shooterManager.CurrentWeapon.chargeWeapon && shooterManager.CurrentWeapon.powerCharge != 0f)
					{
						CurrentActiveWeapon.powerCharge = 0f;
					}
					shootCountA = 0;
				}
			}
			else
			{
				AimInput();
				ShotInput();
				ReloadInput();
				SwitchCameraSideInput();
				ScopeViewInput();
			}
			return;
		}
		isAimingByInput = false;
		_aimTimming = 0f;
		if (controlAimCanvas != null)
		{
			if (controlAimCanvas.isAimActive)
			{
				controlAimCanvas.SetActiveAim(value: false);
			}
			if (controlAimCanvas.isScopeCameraActive)
			{
				controlAimCanvas.SetActiveScopeCamera(value: false);
			}
		}
	}

	public override void TriggerStrongAttack()
	{
		shooterManager.CancelReload();
		base.TriggerStrongAttack();
	}

	public virtual void AimInput()
	{
		cc.strafeSpeed.rotateWithCamera = IsAiming || lastRotateWithCamera;
		if (_walkingByDefaultWasChanged && !IsAiming)
		{
			_walkingByDefaultWasChanged = false;
			SetWalkByDefault(defaultStrafeWalk);
		}
		if (!shooterManager || base.isAttacking || (isReloading && (isUsingScopeView || !shooterManager.keepAimingWhenReload)))
		{
			if (!isReloading || (isReloading && !shooterManager.keepAimingWhenReload))
			{
				isAimingByInput = false;
				SetWalkByDefault(defaultStrafeWalk);
				_walkingByDefaultWasChanged = false;
				if (cc.isStrafing)
				{
					cc.Strafe();
				}
			}
			if ((bool)controlAimCanvas)
			{
				controlAimCanvas.SetActiveAim(value: false);
				if (isUsingScopeView)
				{
					DisableScopeView();
				}
			}
			return;
		}
		if (LockHipFireAiming)
		{
			_aimTimming = 1f;
		}
		if (shooterManager.onlyWalkWhenAiming && (!isReloading || shooterManager.keepAimingWhenReload))
		{
			SetWalkByDefault(isAimingByInput || defaultStrafeWalk);
			_walkingByDefaultWasChanged = isAimingByInput || defaultStrafeWalk;
		}
		if (cc.locomotionType == vThirdPersonMotor.LocomotionType.OnlyFree)
		{
			Debug.LogWarning("Shooter behaviour needs to be OnlyStrafe or Free with Strafe. \n Please change the Locomotion Type.");
			return;
		}
		if (shooterManager.hipfireShot && !LockHipFireAiming)
		{
			if (_aimTimming > 0f && !shooterManager.isShooting && !shooterManager.isShootingEmptyClip && CanRotateAimArm())
			{
				_aimTimming -= Time.deltaTime;
			}
			if (sprintInput.GetButtonDown() && _aimTimming > 0f)
			{
				_aimTimming = 0f;
			}
		}
		if (!shooterManager || !CurrentActiveWeapon)
		{
			if ((bool)controlAimCanvas)
			{
				controlAimCanvas.SetActiveAim(value: false);
				controlAimCanvas.SetActiveScopeCamera(value: false);
			}
			isAimingByInput = false;
			if (cc.isStrafing)
			{
				cc.Strafe();
			}
			return;
		}
		if (!cc.isRolling)
		{
			isAimingByInput = ((!isReloading || shooterManager.keepAimingWhenReload) && (aimInput.GetButton() || (shooterManager.alwaysAiming && (bool)CurrentActiveWeapon)) && !cc.ragdolled && !cc.customAction) || (cc.customAction && cc.isJumping);
		}
		if (aimInput.GetButtonUp() && !shotInput.GetButton())
		{
			_aimTimming = 0f;
		}
		if ((bool)headTrack)
		{
			headTrack.alwaysFollowCamera = isAimingByInput;
		}
		if (cc.locomotionType == vThirdPersonMotor.LocomotionType.FreeWithStrafe && !cc.lockInStrafe)
		{
			if (IsAiming && !cc.isStrafing)
			{
				cc.Strafe();
			}
			else if (!IsAiming && cc.isStrafing)
			{
				cc.Strafe();
			}
		}
		if (IsAiming && shooterManager.onlyWalkWhenAiming && cc.isSprinting)
		{
			cc.isSprinting = false;
		}
		if ((bool)controlAimCanvas)
		{
			if (IsAiming && !controlAimCanvas.isAimActive)
			{
				controlAimCanvas.SetActiveAim(value: true);
			}
			if (!IsAiming && controlAimCanvas.isAimActive)
			{
				controlAimCanvas.SetActiveAim(value: false);
			}
		}
		if ((bool)shooterManager.rWeapon)
		{
			shooterManager.rWeapon.SetActiveAim(IsAiming && aimConditions);
			shooterManager.rWeapon.SetActiveScope(IsAiming && isUsingScopeView);
		}
		else if ((bool)shooterManager.lWeapon)
		{
			shooterManager.lWeapon.SetActiveAim(IsAiming && aimConditions);
			shooterManager.lWeapon.SetActiveScope(IsAiming && isUsingScopeView);
		}
	}

	public virtual void ShotInput()
	{
		if (!shooterManager || CurrentActiveWeapon == null || cc.isDead || isReloading || base.isAttacking || base.isEquipping)
		{
			if ((bool)shooterManager && shooterManager.CurrentWeapon.chargeWeapon && shooterManager.CurrentWeapon.powerCharge != 0f)
			{
				CurrentActiveWeapon.powerCharge = 0f;
			}
			shootCountA = 0;
		}
		else if (((IsAiming && !shooterManager.hipfireShot) || shooterManager.hipfireShot) && !shooterManager.isShooting && aimConditions)
		{
			if ((bool)CurrentActiveWeapon || ((bool)shooterManager.CurrentWeapon && shooterManager.hipfireShot))
			{
				HandleShotCount(shooterManager.CurrentWeapon, shotInput.GetButton());
			}
		}
		else if (!IsAiming)
		{
			if (shooterManager.CurrentWeapon.chargeWeapon && shooterManager.CurrentWeapon.powerCharge != 0f)
			{
				CurrentActiveWeapon.powerCharge = 0f;
			}
			shootCountA = 0;
		}
	}

	public virtual void HandleShotCount(vShooterWeapon weapon, bool weaponInput = true)
	{
		if (weapon.chargeWeapon)
		{
			if (shooterManager.WeaponHasLoadedAmmo() && weapon.powerCharge < 1f && weaponInput)
			{
				if (shooterManager.hipfireShot)
				{
					_aimTimming = shooterManager.HipfireAimTime + CurrentActiveWeapon.shootFrequency;
				}
				weapon.powerCharge += Time.deltaTime * weapon.chargeSpeed;
			}
			else if ((weapon.powerCharge >= 1f && weapon.autoShotOnFinishCharge && weaponInput) || (!weaponInput && IsAiming && weapon.powerCharge > 0f))
			{
				if (shooterManager.hipfireShot)
				{
					_aimTimming = shooterManager.HipfireAimTime + CurrentActiveWeapon.shootFrequency;
				}
				shootCountA = 1;
			}
			else if (!shooterManager.WeaponHasLoadedAmmo() && shooterManager.WeaponHasUnloadedAmmo() && !isReloading && CurrentActiveWeapon.autoReload)
			{
				shooterManager.ReloadWeapon();
			}
			base.animator.SetFloat(vAnimatorParameters.PowerCharger, weapon.powerCharge);
		}
		else if (weapon.automaticWeapon && weaponInput)
		{
			if (shooterManager.hipfireShot && !isAimingByInput)
			{
				_aimTimming = shooterManager.HipfireAimTime;
			}
			shootCountA = 1;
		}
		else if (weaponInput)
		{
			if (shooterManager.hipfireShot && !isAimingByInput)
			{
				_aimTimming = shooterManager.HipfireAimTime;
			}
			if (!allowAttack)
			{
				shootCountA = 1;
				allowAttack = true;
			}
		}
		else
		{
			allowAttack = false;
		}
	}

	public virtual void DoShots()
	{
		if (CanDoShots())
		{
			base.animator.SetFloat(vAnimatorParameters.Shot_ID, shooterManager.GetShotID());
			shooterManager.Shoot(AimPosition, !isAimingByInput);
			if (CurrentActiveWeapon.chargeWeapon)
			{
				CurrentActiveWeapon.powerCharge = 0f;
			}
			shootCountA--;
		}
	}

	public virtual void ReloadInput()
	{
		if ((bool)shooterManager && !(CurrentActiveWeapon == null) && reloadInput.GetButtonDown() && !isReloading && !cc.customAction && !cc.ragdolled && !shooterManager.isShooting)
		{
			shootCountA = 0;
			_aimTimming = 0f;
			shooterManager.ReloadWeapon();
		}
	}

	public virtual void SwitchCameraSideInput()
	{
		if (!(tpCamera == null) && switchCameraSideInput.GetButtonDown())
		{
			SwitchCameraSide();
		}
	}

	public virtual void SwitchCameraSide()
	{
		if (!(tpCamera == null))
		{
			isCameraRightSwitched = !isCameraRightSwitched;
			tpCamera.SwitchRight(isCameraRightSwitched);
		}
	}

	public virtual void CancelAiming()
	{
		isAimingByInput = false;
		_aimTimming = 0f;
		if ((bool)controlAimCanvas)
		{
			controlAimCanvas.SetActiveAim(value: false);
			controlAimCanvas.SetActiveScopeCamera(value: false);
		}
	}

	public virtual void ScopeViewInput()
	{
		if (!shooterManager || CurrentActiveWeapon == null)
		{
			return;
		}
		if (isAimingByInput && aimConditions && (scopeViewInput.GetButtonDown() || CurrentActiveWeapon.onlyUseScopeUIView))
		{
			if ((bool)controlAimCanvas && (bool)CurrentActiveWeapon.scopeTarget)
			{
				if (!isUsingScopeView && CurrentActiveWeapon.onlyUseScopeUIView)
				{
					EnableScopeView();
				}
				else if (isUsingScopeView && !CurrentActiveWeapon.onlyUseScopeUIView)
				{
					DisableScopeView();
				}
				else if (!isUsingScopeView)
				{
					EnableScopeView();
				}
			}
		}
		else if (isUsingScopeView && (((bool)controlAimCanvas && !isAimingByInput) || ((bool)controlAimCanvas && !aimConditions) || cc.isRolling))
		{
			DisableScopeView();
		}
	}

	public virtual void EnableScopeView()
	{
		if (isAimingByInput && (bool)controlAimCanvas.scopeBackgroundCamera && !isReloading && !base.isEquipping)
		{
			isUsingScopeView = true;
			controlAimCanvas.SetActiveScopeCamera(value: true, CurrentActiveWeapon.useUI);
		}
	}

	public virtual void DisableScopeView()
	{
		if ((bool)controlAimCanvas.scopeBackgroundCamera)
		{
			isUsingScopeView = false;
			controlAimCanvas.SetActiveScopeCamera(value: false);
		}
	}

	protected override void UpdateMeleeAnimations()
	{
		if ((bool)base.animator)
		{
			base.isEquipping = cc.IsAnimatorTag("IsEquipping");
			_ignoreIKFromAnimator = cc.IsAnimatorTag("IgnoreIK");
			if (cc.customAction)
			{
				ResetMeleeAnimations();
				ResetShooterAnimations();
				UpdateCameraStates();
				CancelAiming();
			}
			else if ((shooterManager == null || !CurrentActiveWeapon) && (bool)meleeManager)
			{
				base.UpdateMeleeAnimations();
				onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, 0f, 6f * vTime.deltaTime);
				base.animator.SetLayerWeight(onlyArmsLayer, onlyArmsLayerWeight);
				base.animator.SetBool(vAnimatorParameters.IsAiming, value: false);
				isReloading = false;
			}
			else if ((bool)shooterManager && (bool)CurrentActiveWeapon)
			{
				UpdateShooterAnimations();
			}
			else
			{
				ResetMoveSet();
				ResetMeleeAnimations();
				ResetShooterAnimations();
			}
		}
	}

	public virtual void ResetMoveSet()
	{
		cc.animator.SetFloat(vAnimatorParameters.MoveSet_ID, defaultMoveSetID, 0.2f, Time.deltaTime);
	}

	public virtual void ResetShooterAnimations()
	{
		if (!(shooterManager == null) && (bool)base.animator)
		{
			base.animator.SetFloat(vAnimatorParameters.UpperBody_ID, 0f, 0.2f, vTime.deltaTime);
			base.animator.SetBool(vAnimatorParameters.CanAim, value: false);
			base.animator.SetBool(vAnimatorParameters.IsAiming, value: false);
			onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, 0f, 6f * vTime.deltaTime);
			base.animator.SetLayerWeight(onlyArmsLayer, onlyArmsLayerWeight);
		}
	}

	protected virtual void UpdateShooterAnimations()
	{
		if (!(shooterManager == null))
		{
			onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, ((bool)CurrentActiveWeapon || base.isEquipping) ? 1f : 0f, shooterManager.onlyArmsSpeed * vTime.deltaTime);
			base.animator.SetLayerWeight(onlyArmsLayer, onlyArmsLayerWeight);
			if ((bool)CurrentActiveWeapon && IsAiming)
			{
				base.animator.SetLayerWeight(shotLayer, isUsingScopeView ? CurrentActiveWeapon.scopeShootAnimationWeight : 1f);
			}
			if (((bool)CurrentActiveWeapon && !shooterManager.useDefaultMovesetWhenNotAiming) || IsAiming || isReloading)
			{
				base.animator.SetFloat(vAnimatorParameters.MoveSet_ID, shooterMoveSetID, 0.1f, vTime.deltaTime);
			}
			else if ((!CurrentActiveWeapon && !shooterManager.useDefaultMovesetWhenNotAiming) || shooterManager.useDefaultMovesetWhenNotAiming)
			{
				base.animator.SetFloat(vAnimatorParameters.MoveSet_ID, defaultMoveSetID, 0.1f, vTime.deltaTime);
			}
			base.animator.SetBool(vAnimatorParameters.IsBlocking, base.isBlocking);
			base.animator.SetFloat(vAnimatorParameters.UpperBody_ID, shooterManager.GetUpperBodyID());
			base.animator.SetBool(vAnimatorParameters.CanAim, aimConditions);
			base.animator.SetBool(vAnimatorParameters.IsAiming, IsAiming);
			isReloading = cc.IsAnimatorTag("IsReloading") || shooterManager.isReloadingWeapon;
			new vAnimatorParameter(base.animator, "IsReloading");
		}
	}

	public override void UpdateCameraStates()
	{
		if (ignoreTpCamera)
		{
			return;
		}
		if (tpCamera == null)
		{
			tpCamera = UnityEngine.Object.FindObjectOfType<vThirdPersonCamera>();
			if (tpCamera == null)
			{
				return;
			}
			if ((bool)tpCamera)
			{
				tpCamera.SetMainTarget(base.transform);
				tpCamera.Init();
			}
		}
		if (changeCameraState)
		{
			tpCamera.ChangeState(customCameraState, customlookAtPoint, hasSmooth: true);
		}
		else if (cc.isCrouching && !isAimingByInput)
		{
			tpCamera.ChangeState("Crouch", hasSmooth: true);
		}
		else if (cc.isStrafing && !isAimingByInput)
		{
			tpCamera.ChangeState("Strafing", hasSmooth: true);
		}
		else if (isAimingByInput && (bool)CurrentActiveWeapon)
		{
			if (isUsingScopeView)
			{
				if (string.IsNullOrEmpty(CurrentActiveWeapon.customScopeCameraState))
				{
					tpCamera.ChangeState(cc.isCrouching ? "CrouchingAiming" : "Aiming", hasSmooth: true);
				}
				else
				{
					tpCamera.ChangeState(CurrentActiveWeapon.customScopeCameraState, hasSmooth: true);
				}
			}
			else if (string.IsNullOrEmpty(CurrentActiveWeapon.customAimCameraState))
			{
				tpCamera.ChangeState(cc.isCrouching ? "CrouchingAiming" : "Aiming", hasSmooth: true);
			}
			else
			{
				tpCamera.ChangeState(CurrentActiveWeapon.customAimCameraState, hasSmooth: true);
			}
		}
		else
		{
			tpCamera.ChangeState("Default", hasSmooth: true);
		}
	}

	protected virtual void UpdateAimPosition()
	{
		if (!shooterManager || CurrentActiveWeapon == null)
		{
			return;
		}
		Transform transform = ((!isUsingScopeView || !controlAimCanvas || !controlAimCanvas.scopeBackgroundCamera) ? base.cameraMain.transform : (CurrentActiveWeapon.zoomScopeCamera ? CurrentActiveWeapon.zoomScopeCamera.transform : controlAimCanvas.scopeBackgroundCamera.transform));
		Vector3 position = transform.position;
		if (!controlAimCanvas || !controlAimCanvas.isScopeCameraActive || !controlAimCanvas.scopeBackgroundCamera)
		{
			position = transform.position;
		}
		Vector3 origin = position;
		origin += (((bool)controlAimCanvas && controlAimCanvas.isScopeCameraActive && (bool)controlAimCanvas.scopeBackgroundCamera) ? transform.forward : Vector3.zero);
		AimPosition = transform.position + transform.forward * 100f;
		if (!isUsingScopeView)
		{
			lastAimDistance = 100f;
		}
		if (shooterManager.raycastAimTarget && CurrentActiveWeapon.raycastAimTarget)
		{
			Ray ray = new Ray(origin, transform.forward);
			if (Physics.Raycast(ray, out var hitInfo, base.cameraMain.farClipPlane, shooterManager.damageLayer))
			{
				bool flag = false;
				if (hitInfo.collider.transform.IsChildOf(base.transform))
				{
					GameObject gameObject = hitInfo.collider.gameObject;
					RaycastHit[] array = Physics.RaycastAll(ray, base.cameraMain.farClipPlane, shooterManager.damageLayer);
					float num = base.cameraMain.farClipPlane;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].distance < num && array[i].collider.gameObject != gameObject.gameObject && !array[i].collider.transform.IsChildOf(base.transform))
						{
							flag = true;
							num = array[i].distance;
							hitInfo = array[i];
						}
					}
				}
				else
				{
					flag = true;
				}
				if ((bool)hitInfo.collider && flag)
				{
					if (!isUsingScopeView)
					{
						lastAimDistance = Vector3.Distance(transform.position, hitInfo.point);
					}
					AimPosition = hitInfo.point;
				}
			}
			if (shooterManager.showCheckAimGizmos)
			{
				Debug.DrawLine(ray.origin, AimPosition);
			}
		}
		if (isAimingByInput)
		{
			shooterManager.CameraSway();
		}
	}

	protected virtual void OnDrawGizmos()
	{
		if ((bool)shooterManager && shooterManager.showCheckAimGizmos && (bool)CurrentActiveWeapon && shooterManager.useCheckAim)
		{
			Vector3 start = Vector3.zero;
			Vector3 end = Vector3.zero;
			UpdateCheckAimPoints(ref start, ref end);
			Color color = Gizmos.color;
			color = (aimConditions ? Color.green : Color.red);
			color.a = 1f;
			Gizmos.color = color;
			Gizmos.DrawLine(start, end);
			Gizmos.DrawSphere(start, 0.02f);
			Gizmos.DrawSphere(end, shooterManager.checkAimRadius);
		}
	}

	protected virtual void UpdateAimBehaviour()
	{
		if (!cc.isDead)
		{
			UpdateAimPosition();
			UpdateHeadTrack();
			OnStartUpdateIK();
			if ((bool)shooterManager && (bool)CurrentActiveWeapon)
			{
				UpdateIKAdjust(shooterManager.IsLeftWeapon);
				RotateAimArm(shooterManager.IsLeftWeapon);
				RotateAimHand(shooterManager.IsLeftWeapon);
				UpdateArmsIK(shooterManager.IsLeftWeapon);
				UpdateCheckAimHelpers(shooterManager.IsLeftWeapon);
			}
			OnFinishUpdateIK();
			CheckAimConditions();
			UpdateAimHud();
			DoShots();
		}
	}

	protected virtual void UpdateAimAngleReference()
	{
		aimAngleReference.transform.rotation = base.transform.rotation;
	}

	protected virtual void UpdateCheckAimPoints(ref Vector3 start, ref Vector3 end)
	{
		if ((bool)CurrentActiveWeapon)
		{
			float checkAimOffsetSmooth = shooterManager.checkAimOffsetSmooth;
			checkCanAimOffsetStartX = Mathf.Lerp(checkCanAimOffsetStartX, IsCrouching ? shooterManager.checkAimCrouchedOffsetStartX : shooterManager.checkAimStandingOffsetStartX, checkAimOffsetSmooth * Time.deltaTime);
			checkCanAimOffsetStartY = Mathf.Lerp(checkCanAimOffsetStartY, IsCrouching ? shooterManager.checkAimCrouchedOffsetStartY : shooterManager.checkAimStandingOffsetStartY, checkAimOffsetSmooth * Time.deltaTime);
			checkCanAimOffsetEndX = Mathf.Lerp(checkCanAimOffsetEndX, IsCrouching ? shooterManager.checkAimCrouchedOffsetEndX : shooterManager.checkAimStandingOffsetEndX, checkAimOffsetSmooth * Time.deltaTime);
			checkCanAimOffsetEndY = Mathf.Lerp(checkCanAimOffsetEndY, IsCrouching ? shooterManager.checkAimCrouchedOffsetEndY : shooterManager.checkAimStandingOffsetEndY, checkAimOffsetSmooth * Time.deltaTime);
			Vector3 vector = aimAngleReference.transform.TransformPoint(upperArmPosition);
			Vector3 vector2 = aimAngleReference.transform.TransformPoint(muzzlePosition);
			Vector3 vector3 = aimAngleReference.transform.InverseTransformDirection(muzzleForward);
			Vector3 vector4 = vector + base.cameraMain.transform.right * (checkCanAimOffsetStartX * (float)((tpCamera.switchRight > 0f) ? 1 : (-1))) + base.cameraMain.transform.up * checkCanAimOffsetStartY;
			Vector3 vector5 = vector2 + base.cameraMain.transform.right * (checkCanAimOffsetEndX * (float)((tpCamera.switchRight > 0f) ? 1 : (-1))) + base.cameraMain.transform.up * checkCanAimOffsetEndY + vector3 * shooterManager.checkAimOffsetZ;
			start = vector4;
			end = vector5;
		}
	}

	protected virtual void OnFinishUpdateIK()
	{
		this.onFinishUpdateIK?.Invoke();
	}

	protected virtual void OnStartUpdateIK()
	{
		this.onStartUpdateIK?.Invoke();
	}

	protected virtual void UpdateIKAdjust(bool isUsingLeftHand)
	{
		if (LeftIK == null || !LeftIK.isValidBones)
		{
			LeftIK = new vIKSolver(base.animator, AvatarIKGoal.LeftHand);
			LeftIK.UpdateIK();
		}
		if (RightIK == null || !RightIK.isValidBones)
		{
			RightIK = new vIKSolver(base.animator, AvatarIKGoal.RightHand);
			RightIK.UpdateIK();
		}
		if (WeaponIKAdjustList == null)
		{
			return;
		}
		CurrentActiveWeapon.handIKTargetOffset.localPosition = (isUsingLeftHand ? WeaponIKAdjustList.ikTargetPositionOffsetL : WeaponIKAdjustList.ikTargetPositionOffsetR);
		CurrentActiveWeapon.handIKTargetOffset.localEulerAngles = (isUsingLeftHand ? WeaponIKAdjustList.ikTargetRotationOffsetL : WeaponIKAdjustList.ikTargetRotationOffsetR);
		if (!CurrentWeaponIK || IsIgnoreIK)
		{
			LeftIK.UpdateIK();
			RightIK.UpdateIK();
			RightIK.SetIKWeight(0f);
			LeftIK.SetIKWeight(0f);
			weaponIKWeight = 0f;
			return;
		}
		bool flag = !cc.customAction && !isReloading && !base.isEquipping && CurrentWeaponIK != null && CurrentIKAdjust != null;
		weaponIKWeight = Mathf.Lerp(weaponIKWeight, flag ? 1 : 0, 25f * vTime.deltaTime);
		if (!(weaponIKWeight <= 0f))
		{
			if (isUsingLeftHand)
			{
				ApplyOffsets(LeftIK, RightIK, flag);
			}
			else
			{
				ApplyOffsets(RightIK, LeftIK, flag);
			}
		}
	}

	protected virtual void ApplyOffsets(vIKSolver weaponHand, vIKSolver supportHand, bool isValidIK = true)
	{
		if (weaponHand.isValidBones && supportHand.isValidBones)
		{
			weaponHand.SetIKWeight(weaponIKWeight);
			ApplyOffsetToTargetBone(isValidIK ? CurrentIKAdjust.weaponHandOffset : null, weaponHand.endBoneOffset, isValidIK);
			ApplyOffsetToTargetBone(isValidIK ? CurrentIKAdjust.weaponHintOffset : null, weaponHand.middleBoneOffset, isValidIK);
			weaponHand.AnimationToIK();
			ApplyOffsetToTargetBone(isValidIK ? CurrentIKAdjust.supportHandOffset : null, supportHand.endBoneOffset, !EditingIKGlobalOffset && isValidIK);
			ApplyOffsetToTargetBone(isValidIK ? CurrentIKAdjust.supportHintOffset : null, supportHand.middleBoneOffset, !EditingIKGlobalOffset && isValidIK);
		}
	}

	protected virtual void ApplyOffsetToTargetBone(IKOffsetTransform iKOffset, Transform target, bool isValidIK)
	{
		try
		{
			target.localPosition = Vector3.Lerp(target.localPosition, isValidIK ? iKOffset.position : Vector3.zero, shooterManager.ikAdjustSmooth * vTime.deltaTime);
			target.localRotation = Quaternion.Lerp(target.localRotation, isValidIK ? Quaternion.Euler(iKOffset.eulerAngles) : Quaternion.Euler(Vector3.zero), shooterManager.ikAdjustSmooth * vTime.deltaTime);
		}
		catch
		{
			Debug.LogWarning("Can't Get IK Adjust");
		}
	}

	protected virtual void UpdateArmsIK(bool isUsingLeftHand = false)
	{
		if (LeftIK == null || !LeftIK.isValidBones)
		{
			LeftIK = new vIKSolver(base.animator, AvatarIKGoal.LeftHand);
		}
		if (RightIK == null || !RightIK.isValidBones)
		{
			RightIK = new vIKSolver(base.animator, AvatarIKGoal.RightHand);
		}
		vIKSolver vIKSolver = null;
		vIKSolver = ((!isUsingLeftHand) ? LeftIK : RightIK);
		bool flag = (isUsingLeftHand ? shooterManager.useLeftIK : shooterManager.useRightIK);
		if (!shooterManager || !CurrentActiveWeapon || !flag || IsIgnoreIK || base.isEquipping || (cc.IsAnimatorTag("Shot Fire") && CurrentActiveWeapon.disableIkOnShot))
		{
			if (supportIKWeight > 0f)
			{
				supportIKWeight = 0f;
				vIKSolver.SetIKWeight(0f);
			}
			return;
		}
		bool flag2 = false;
		double num = Math.Round(cc.inputMagnitude, 1);
		if (!IsAiming && !base.isAttacking)
		{
			vShooterWeapon.IKLocomotionOptions iKLocomotionOptions = (cc.isStrafing ? CurrentActiveWeapon.strafeIKOptions : CurrentActiveWeapon.freeIKOptions);
			if (iKLocomotionOptions.use)
			{
				if (num <= 0.10000000149011612)
				{
					flag2 = iKLocomotionOptions.useOnIdle;
				}
				else if (num <= 0.5)
				{
					flag2 = iKLocomotionOptions.useOnWalk;
				}
				else if (num <= 1.0)
				{
					flag2 = iKLocomotionOptions.useOnRun;
				}
				else if (num <= 1.5)
				{
					flag2 = iKLocomotionOptions.useOnSprint;
				}
			}
			else
			{
				flag2 = false;
			}
		}
		else if (IsAiming && !base.isAttacking)
		{
			flag2 = (shooterManager.isShooting ? (!CurrentActiveWeapon.disableIkOnShot) : CurrentActiveWeapon.useIKOnAiming);
		}
		else if (base.isAttacking)
		{
			flag2 = CurrentActiveWeapon.useIkAttacking;
		}
		IsSupportHandIKEnabled = flag2;
		if (vIKSolver == null)
		{
			return;
		}
		if ((bool)shooterManager.weaponIKAdjustList)
		{
			if (isUsingLeftHand)
			{
				ikRotationOffset = shooterManager.weaponIKAdjustList.ikTargetRotationOffsetR;
				ikPositionOffset = shooterManager.weaponIKAdjustList.ikTargetPositionOffsetR;
			}
			else
			{
				ikRotationOffset = shooterManager.weaponIKAdjustList.ikTargetRotationOffsetL;
				ikPositionOffset = shooterManager.weaponIKAdjustList.ikTargetPositionOffsetL;
			}
		}
		if ((bool)CurrentActiveWeapon && (bool)CurrentActiveWeapon.handIKTargetOffset && !isReloading && !cc.customAction && (cc.isGrounded || IsAiming) && flag2)
		{
			supportIKWeight = Mathf.Lerp(supportIKWeight, 1f, shooterManager.armIKSmoothIn * vTime.deltaTime);
		}
		else
		{
			supportIKWeight = Mathf.Lerp(supportIKWeight, 0f, shooterManager.armIKSmoothOut * vTime.deltaTime);
		}
		if (supportIKWeight <= 0f)
		{
			return;
		}
		vIKSolver.SetIKWeight(shooterManager.armIKCurve.Evaluate(supportIKWeight));
		if ((bool)shooterManager && (bool)CurrentActiveWeapon && (bool)CurrentActiveWeapon.handIKTargetOffset)
		{
			vIKSolver.SetIKPosition(CurrentActiveWeapon.handIKTargetOffset.position);
			vIKSolver.SetIKRotation(CurrentActiveWeapon.handIKTargetOffset.rotation);
			if ((bool)shooterManager.CurrentWeaponIK)
			{
				vIKSolver.AnimationToIK();
			}
		}
	}

	protected virtual bool CanRotateAimArm()
	{
		if (cc.IsAnimatorTag("Upperbody Pose") && IsAimAlignWithForward())
		{
			return cc.animatorStateInfos.GetCurrentNormalizedTime(cc.upperBodyLayer) > 0.5f;
		}
		return false;
	}

	protected virtual bool CanDoShots()
	{
		if (armAlignmentWeight >= 0.9f && cc.IsAnimatorTag("Upperbody Pose") && shootCountA > 0)
		{
			return !isReloading;
		}
		return false;
	}

	protected virtual void RotateAimArm(bool isUsingLeftHand = false)
	{
		if (!shooterManager)
		{
			return;
		}
		armAlignmentWeight = ((IsAiming && aimConditions && CanRotateAimArm()) ? Mathf.Lerp(armAlignmentWeight, Mathf.Clamp(cc.upperBodyInfo.normalizedTime, 0f, 1f), shooterManager.smoothArmWeight * (0.001f + Time.deltaTime)) : 0f);
		if ((bool)CurrentActiveWeapon && armAlignmentWeight > 0.01f && CurrentActiveWeapon.alignRightUpperArmToAim)
		{
			Transform transform = (isUsingLeftHand ? leftUpperArm : rightUpperArm);
			Vector3 position = targetArmAlignmentPosition;
			Vector3 position2 = base.transform.InverseTransformPoint(position);
			base.transform.InverseTransformPoint(transform.position);
			position = base.transform.TransformPoint(position2);
			Vector3 direction = position - CurrentActiveWeapon.aimReference.position;
			Vector3 forward = CurrentActiveWeapon.aimReference.forward;
			Quaternion quaternion = Quaternion.FromToRotation(transform.InverseTransformDirection(forward), transform.InverseTransformDirection(direction));
			if (!float.IsNaN(quaternion.x) && !float.IsNaN(quaternion.y) && !float.IsNaN(quaternion.z))
			{
				upperArmRotationAlignment = ((!shooterManager.isShooting) ? quaternion : ((armAlignmentWeight > 0.98f) ? upperArmRotation : Quaternion.identity));
			}
			float num = Vector3.Angle((AimPosition - aimAngleReference.transform.position).normalized, aimAngleReference.transform.forward);
			if ((!(num > shooterManager.maxAimAngle) && !(num < 0f - shooterManager.maxAimAngle)) || ((bool)controlAimCanvas && controlAimCanvas.isScopeCameraActive))
			{
				upperArmRotation = Quaternion.Lerp(upperArmRotation, upperArmRotationAlignment, shooterManager.smoothArmIKRotation * (0.001f + Time.deltaTime));
			}
			if (!float.IsNaN(upperArmRotation.x) && !float.IsNaN(upperArmRotation.y) && !float.IsNaN(upperArmRotation.z))
			{
				float num2 = (CurrentActiveWeapon.alignRightHandToAim ? Mathf.Clamp(armAlignmentWeight, 0f, 0.5f) : armAlignmentWeight);
				transform.localRotation *= Quaternion.Euler(upperArmRotation.eulerAngles.NormalizeAngle() * num2);
			}
		}
		else
		{
			upperArmRotation = Quaternion.Euler(0f, 0f, 0f);
		}
	}

	protected virtual void RotateAimHand(bool isUsingLeftHand = false)
	{
		if (!shooterManager)
		{
			return;
		}
		if ((bool)CurrentActiveWeapon && armAlignmentWeight > 0.01f && CurrentActiveWeapon.alignRightHandToAim)
		{
			Transform transform = (isUsingLeftHand ? leftHand : rightHand);
			Vector3 position = targetArmAlignmentPosition;
			Vector3 position2 = base.transform.InverseTransformPoint(position);
			base.transform.InverseTransformPoint(transform.position);
			position = base.transform.TransformPoint(position2);
			Vector3 direction = position - CurrentActiveWeapon.aimReference.position;
			Vector3 forward = CurrentActiveWeapon.aimReference.forward;
			Quaternion quaternion = Quaternion.FromToRotation(transform.InverseTransformDirection(forward), transform.InverseTransformDirection(direction));
			if (!float.IsNaN(quaternion.x) && !float.IsNaN(quaternion.y) && !float.IsNaN(quaternion.z))
			{
				handRotationAlignment = ((!shooterManager.isShooting) ? quaternion : ((armAlignmentWeight > 0.98f) ? handRotation : Quaternion.identity));
			}
			float num = Vector3.Angle((AimPosition - aimAngleReference.transform.position).normalized, aimAngleReference.transform.forward);
			if ((!(num > shooterManager.maxAimAngle) && !(num < 0f - shooterManager.maxAimAngle)) || ((bool)controlAimCanvas && controlAimCanvas.isScopeCameraActive))
			{
				handRotation = Quaternion.Lerp(handRotation, handRotationAlignment, shooterManager.smoothArmIKRotation * (0.001f + Time.deltaTime));
			}
			if (!float.IsNaN(handRotation.x) && !float.IsNaN(handRotation.y) && !float.IsNaN(handRotation.z))
			{
				float num2 = armAlignmentWeight;
				transform.localRotation *= Quaternion.Euler(handRotation.eulerAngles.NormalizeAngle() * num2);
			}
			CurrentActiveWeapon.SetScopeLookTarget(position);
		}
		else
		{
			handRotation = Quaternion.Euler(0f, 0f, 0f);
		}
	}

	protected void UpdateCheckAimHelpers(bool isUsingLeftHand)
	{
		if (aimConditions && IsAiming && armAlignmentWeight >= 1f)
		{
			Transform transform = (isUsingLeftHand ? leftUpperArm : rightUpperArm);
			upperArmPosition = aimAngleReference.transform.InverseTransformPoint(transform.position);
			muzzlePosition = aimAngleReference.transform.InverseTransformPoint(CurrentActiveWeapon.muzzle.position);
			muzzleForward = aimAngleReference.transform.InverseTransformDirection(CurrentActiveWeapon.muzzle.forward);
		}
	}

	protected virtual void CheckAimConditions()
	{
		if (!shooterManager)
		{
			return;
		}
		_ = tpCamera.switchRight;
		_ = 0f;
		if (CurrentActiveWeapon == null)
		{
			aimConditions = false;
		}
		else if (!shooterManager.useCheckAim || !IsAiming)
		{
			aimConditions = true;
		}
		else if (!base.animator.IsInTransition(0))
		{
			Vector3 start = Vector3.zero;
			Vector3 end = Vector3.zero;
			UpdateCheckAimPoints(ref start, ref end);
			if (Vector3.Distance(start, AimPosition) < Vector3.Distance(start, end))
			{
				aimConditions = false;
			}
			if (Physics.SphereCast(new Ray(start, (end - start).normalized), shooterManager.checkAimRadius, out checkCanAimHit, (end - start).magnitude, shooterManager.blockAimLayer))
			{
				aimConditions = false;
			}
			else
			{
				aimConditions = true;
			}
		}
	}

	protected virtual bool IsAimAlignWithForward()
	{
		if (!shooterManager)
		{
			return false;
		}
		Vector3 forward = aimAngleReference.transform.forward;
		forward.y = 0f;
		Vector3 directionB = targetArmAligmentDirection;
		directionB.y = 0f;
		return Mathf.Abs(forward.AngleFormOtherDirection(directionB).y) < shooterManager.maxAimAngle;
	}

	protected virtual void UpdateHeadTrack()
	{
		if ((bool)headTrack)
		{
			headTrack.ignoreSmooth = IsAiming || isUsingScopeView;
			UpdateHeadTrackLookPoint();
		}
		if (!shooterManager || !headTrack)
		{
			if ((bool)headTrack)
			{
				headTrack.offsetSpine = Vector2.Lerp(headTrack.offsetSpine, Vector2.zero, headTrack.Smooth);
				headTrack.offsetHead = Vector2.Lerp(headTrack.offsetHead, Vector2.zero, headTrack.Smooth);
			}
		}
		else if (!CurrentActiveWeapon || !headTrack || !CurrentWeaponIK || CurrentIKAdjust == null)
		{
			if ((bool)headTrack)
			{
				headTrack.offsetSpine = Vector2.Lerp(headTrack.offsetSpine, Vector2.zero, headTrack.Smooth);
				headTrack.offsetHead = Vector2.Lerp(headTrack.offsetHead, Vector2.zero, headTrack.Smooth);
			}
		}
		else if (IsAiming)
		{
			IKAdjust currentIKAdjust = CurrentIKAdjust;
			Vector2 spine = currentIKAdjust.spineOffset.spine;
			Vector2 head = currentIKAdjust.spineOffset.head;
			headTrack.offsetSpine = Vector2.Lerp(headTrack.offsetSpine, spine, headTrack.Smooth);
			headTrack.offsetHead = Vector2.Lerp(headTrack.offsetHead, head, headTrack.Smooth);
		}
		else
		{
			IKAdjust currentIKAdjust2 = CurrentIKAdjust;
			Vector2 spine2 = currentIKAdjust2.spineOffset.spine;
			Vector2 head2 = currentIKAdjust2.spineOffset.head;
			headTrack.offsetSpine = Vector2.Lerp(headTrack.offsetSpine, spine2, headTrack.Smooth);
			headTrack.offsetHead = Vector2.Lerp(headTrack.offsetHead, head2, headTrack.Smooth);
		}
	}

	protected virtual void UpdateHeadTrackLookPoint()
	{
		if (IsAiming && !isUsingScopeView)
		{
			headTrack.SetTemporaryLookPoint(base.cameraMain.transform.position + base.cameraMain.transform.forward * 10f, 0.1f);
		}
	}

	protected virtual void UpdateAimHud()
	{
		if (!shooterManager || !controlAimCanvas || CurrentActiveWeapon == null)
		{
			return;
		}
		controlAimCanvas.SetAimCanvasID(CurrentActiveWeapon.scopeID);
		if ((bool)controlAimCanvas.scopeBackgroundCamera && controlAimCanvas.scopeBackgroundCamera.gameObject.activeSelf)
		{
			controlAimCanvas.SetAimToCenter();
		}
		else if (IsAiming)
		{
			if (Physics.Linecast(CurrentActiveWeapon.muzzle.position, AimPosition, out var hitInfo, shooterManager.blockAimLayer))
			{
				Debug.DrawLine(CurrentActiveWeapon.muzzle.position, hitInfo.point);
				controlAimCanvas.SetWordPosition(hitInfo.point, aimConditions);
			}
			else
			{
				Debug.DrawLine(CurrentActiveWeapon.muzzle.position, AimPosition);
				controlAimCanvas.SetWordPosition(AimPosition, aimConditions);
			}
		}
		else
		{
			controlAimCanvas.SetAimToCenter();
		}
		if (!controlAimCanvas.scopeBackgroundCamera || !CurrentActiveWeapon.scopeTarget)
		{
			return;
		}
		if (isUsingScopeView)
		{
			if (Physics.Raycast(CurrentActiveWeapon.scopeTarget.position, CurrentActiveWeapon.scopeTarget.forward, out var hitInfo2, 100f, shooterManager.blockAimLayer) || Physics.Raycast(CurrentActiveWeapon.scopeTarget.position, CurrentActiveWeapon.scopeTarget.forward, out hitInfo2, 100f, shooterManager.damageLayer))
			{
				AimPosition = hitInfo2.point;
			}
			else
			{
				AimPosition = CurrentActiveWeapon.scopeTarget.position + CurrentActiveWeapon.scopeTarget.forward * 100f;
			}
			float t = (shooterManager.isShooting ? (1f - CurrentActiveWeapon.scopeShootAnimationWeight) : 1f);
			Vector3 lookDirection = Vector3.Lerp(controlAimCanvas.scopeBackgroundCamera.transform.forward, CurrentActiveWeapon.scopeTarget.forward, shooterManager.isShooting ? (scopeDirectionWeight = 0f) : (scopeDirectionWeight = Mathf.Lerp(scopeDirectionWeight, 1.001f, 10f / CurrentActiveWeapon.shootFrequency * Time.deltaTime)));
			Vector3 position = Vector3.Lerp(controlAimCanvas.scopeBackgroundCamera.transform.position, CurrentActiveWeapon.scopeTarget.position, t);
			controlAimCanvas.UpdateScopeCamera(position, lookDirection, CurrentActiveWeapon.backGroundScopeZoom);
		}
		else
		{
			scopeDirectionWeight = 1f;
		}
	}

	[SpecialName]
	GameObject vIShooterIKController.get_gameObject()
	{
		return base.gameObject;
	}
}
