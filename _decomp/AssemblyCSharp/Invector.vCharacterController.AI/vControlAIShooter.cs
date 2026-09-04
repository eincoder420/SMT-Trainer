using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Invector.IK;
using Invector.vShooter;
using UnityEngine;

namespace Invector.vCharacterController.AI;

[vClassHeader(" AI SHOOTER CONTROLLER", true, "icon_v2", false, "", iconName = "AI-icon")]
public class vControlAIShooter : vControlAICombat, vIControlAIShooter, vIControlAICombat, vIControlAI, vIHealthController, vIDamageReceiver, vIShooterIKController
{
	[vEditorToolbar("Shooter Settings", false, "", false, false, order = 10)]
	[Header("Shooter Settings")]
	public float minTimeShooting = 2f;

	public float maxTimeShooting = 5f;

	public float minShotWaiting = 3f;

	public float maxShotWaiting = 6f;

	public float aimTargetHeight = 0.35f;

	public bool doReloadWhileWaiting = true;

	public float aimSmoothDamp = 10f;

	public float smoothArmAlignmentWeight = 4f;

	public float aimTurnAngle = 60f;

	public float maxAngleToShot = 60f;

	protected float _timeShotting;

	protected float _waitingToShot;

	protected float _upperBodyID;

	protected float _shotID;

	protected Quaternion handRotationAlignment;

	protected Quaternion upperArmRotationAlignment;

	protected float armAlignmentWeight;

	protected IKAdjust _currentIKAdjust;

	private Transform leftUpperArm;

	private Transform rightUpperArm;

	private Transform leftHand;

	private Transform rightHand;

	private GameObject aimAngleReference;

	private Quaternion upperArmRotation;

	private Quaternion handRotation;

	private readonly float rightRotationWeight;

	private float _onlyArmsLayerWeight;

	private float handIKWeight;

	private float weaponIKWeight;

	private float aimTime;

	private float delayEnableAimAfterRagdolled;

	private int onlyArmsLayer;

	private int _moveSetID;

	private int _attackID;

	private bool aimEnable;

	[vEditorToolbar("Debug", false, "", false, true, order = 100)]
	[SerializeField]
	[vReadOnly(false)]
	protected bool _canAiming;

	[SerializeField]
	[vReadOnly(false)]
	protected bool _canShot;

	[SerializeField]
	[vReadOnly(false)]
	protected bool _waitingReload;

	[SerializeField]
	[vReadOnly(false)]
	protected int shots;

	public bool debugAim;

	public bool lockAimDebug;

	[SerializeField]
	[vHideInInspector("lockAimDebug", false)]
	private Transform aimDebugTarget;

	[SerializeField]
	[vHideInInspector("lockAimDebug", false)]
	private bool debugShoots;

	private Vector3 aimVelocity;

	private Vector3 aimTarget;

	private Vector3 _lastaValidAimLocal;

	protected bool forceCanShot;

	public Vector3 _debugAimPosition;

	public bool IsReloading { get; protected set; }

	public bool IsEquipping { get; protected set; }

	public bool IsInShotAngle { get; protected set; }

	public vAIShooterManager shooterManager { get; set; }

	public vIKSolver LeftIK { get; set; }

	public vIKSolver RightIK { get; set; }

	public vWeaponIKAdjustList WeaponIKAdjustList
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

	public vWeaponIKAdjust CurrentWeaponIK
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

	public virtual bool EditingIKGlobalOffset { get; set; }

	public virtual bool IsUsingCustomIKAdjust => !string.IsNullOrEmpty(CustomIKAdjustState);

	public string CustomIKAdjustState { get; protected set; }

	public virtual bool IsIgnoreIK => IsAnimatorTag("IgnoreIK");

	public virtual bool IsSupportHandIKEnabled { get; protected set; }

	public bool LockAiming
	{
		get
		{
			return lockAimDebug;
		}
		set
		{
			lockAimDebug = value;
		}
	}

	public virtual bool LockHipFireAiming { get; set; }

	public bool IsCrouching
	{
		get
		{
			return isCrouching;
		}
		set
		{
			isCrouching = value;
		}
	}

	public bool IsLeftWeapon
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

	public bool IsAiming
	{
		get
		{
			int num;
			if (!isAiming)
			{
				num = (lockAimDebug ? 1 : 0);
				if (num == 0)
				{
					goto IL_0023;
				}
			}
			else
			{
				num = 1;
			}
			if (!base.isStrafing)
			{
				base.isStrafing = true;
			}
			goto IL_0023;
			IL_0023:
			return (byte)num != 0;
		}
	}

	public Vector3 AimPosition { get; protected set; }

	protected virtual int MoveSetID
	{
		get
		{
			return _moveSetID;
		}
		set
		{
			if (value != _moveSetID || base.animator.GetFloat("MoveSet_ID") != (float)value)
			{
				_moveSetID = value;
				base.animator.SetFloat("MoveSet_ID", _moveSetID, 0.25f, Time.deltaTime);
			}
		}
	}

	protected virtual int AttackID
	{
		get
		{
			return _attackID;
		}
		set
		{
			if (value != _attackID)
			{
				_attackID = value;
				base.animator.SetInteger("AttackID", _attackID);
			}
		}
	}

	public virtual Vector3 defaultValidAimLocal => Vector3.forward * 10f + Vector3.up * (_capsuleCollider.height * 0.5f + aimTargetHeight);

	protected virtual float UpperBodyID
	{
		get
		{
			return _upperBodyID;
		}
		set
		{
			if (_upperBodyID != value || base.animator.GetFloat("UpperBody_ID") != value)
			{
				_upperBodyID = value;
				base.animator.SetFloat("UpperBody_ID", _upperBodyID);
			}
		}
	}

	protected virtual float ShotID
	{
		get
		{
			return _shotID;
		}
		set
		{
			if (_shotID != value || base.animator.GetFloat("Shot_ID") != value)
			{
				_shotID = value;
				base.animator.SetFloat("Shot_ID", _shotID);
			}
		}
	}

	protected virtual Vector3 DebugAimPosition
	{
		get
		{
			if ((bool)aimDebugTarget)
			{
				return aimDebugTarget.position;
			}
			return base.transform.position + base.transform.forward * (2f + _debugAimPosition.z) + base.transform.right * _debugAimPosition.x + base.transform.up * (1.5f + _debugAimPosition.y);
		}
	}

	public virtual vShooterWeapon CurrentActiveWeapon
	{
		get
		{
			if (!shooterManager.CurrentWeapon || !shooterManager.CurrentWeapon.gameObject.activeInHierarchy)
			{
				return null;
			}
			return shooterManager.CurrentWeapon;
		}
	}

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

	public void UpdateWeaponIK()
	{
		if ((bool)shooterManager)
		{
			shooterManager.UpdateWeaponIK();
			if ((bool)CurrentWeaponIK)
			{
				_currentIKAdjust = CurrentWeaponIK.GetIKAdjust(CurrentIKAdjustState, CurrentActiveWeapon.isLeftWeapon);
			}
		}
	}

	public override void CreateSecondaryComponents()
	{
		base.CreateSecondaryComponents();
		if (GetComponent<vAIShooterManager>() == null)
		{
			base.gameObject.AddComponent<vAIShooterManager>();
		}
		if (GetComponent<vAIHeadtrack>() == null)
		{
			base.gameObject.AddComponent<vAIHeadtrack>();
		}
	}

	public virtual void CheckCanShot()
	{
		if (isAiming && _waitingToShot < Time.time && (base.isStrafing || debugShoots || input.magnitude < 0.1f))
		{
			_timeShotting = UnityEngine.Random.Range(minTimeShooting, maxTimeShooting) + Time.time;
		}
		_canShot = _timeShotting > Time.time;
		if (_canShot)
		{
			_waitingToShot = Time.time + UnityEngine.Random.Range(minShotWaiting, maxShotWaiting);
		}
	}

	protected override void Start()
	{
		base.Start();
		_lastaValidAimLocal = defaultValidAimLocal;
		_waitingReload = false;
		InitShooter();
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		if (lockAimDebug)
		{
			Gizmos.DrawSphere(DebugAimPosition, 0.1f);
		}
		if (debugAim && (bool)currentTarget.transform)
		{
			Gizmos.DrawSphere(AimPosition, 0.1f);
			if ((bool)currentTarget.collider)
			{
				Gizmos.DrawWireCube(currentTarget.collider.bounds.center, currentTarget.collider.bounds.size);
			}
			else
			{
				Gizmos.DrawWireCube(currentTarget.transform.position, Vector3.one * 0.5f);
			}
		}
	}

	public virtual void SetShooterHitLayer(LayerMask mask)
	{
		if ((bool)shooterManager)
		{
			shooterManager.SetDamageLayer(mask);
		}
	}

	public override void Attack(bool strongAttack = false, int attackID = -1, bool forceCanAttack = false)
	{
		if (base.ragdolled)
		{
			return;
		}
		if ((bool)shooterManager && attackID != -1)
		{
			AttackID = attackID;
		}
		else
		{
			AttackID = shooterManager.GetAttackID();
		}
		if ((bool)currentTarget.transform || (debugShoots && lockAimDebug) || forceCanAttack)
		{
			forceCanShot = forceCanAttack;
			if ((_canShot || forceCanShot) && shots == 0)
			{
				shots++;
			}
		}
	}

	public override void InitAttackTime()
	{
		base.InitAttackTime();
		_waitingToShot = Time.time + UnityEngine.Random.Range(minShotWaiting, maxShotWaiting);
		_waitingReload = false;
	}

	public override void ResetAttackTime()
	{
		base.ResetAttackTime();
		_waitingToShot = Time.time + UnityEngine.Random.Range(minShotWaiting, maxShotWaiting);
	}

	protected virtual void InitShooter()
	{
		if ((bool)_headtrack)
		{
			_headtrack.onPreUpdateSpineIK.AddListener(HandleAim);
			_headtrack.onPosUpdateSpineIK.AddListener(IKBehaviour);
		}
		shooterManager = GetComponent<vAIShooterManager>();
		leftHand = base.animator.GetBoneTransform(HumanBodyBones.LeftHand);
		rightHand = base.animator.GetBoneTransform(HumanBodyBones.RightHand);
		leftUpperArm = base.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
		rightUpperArm = base.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
		onlyArmsLayer = base.animator.GetLayerIndex("OnlyArms");
		aimAngleReference = new GameObject("aimAngleReference");
		aimAngleReference.transform.rotation = base.transform.rotation;
		Transform boneTransform = base.animator.GetBoneTransform(HumanBodyBones.Head);
		aimAngleReference.transform.SetParent(boneTransform);
		aimAngleReference.transform.localPosition = Vector3.zero;
		AimPosition = DebugAimPosition;
	}

	protected virtual void HandleAim()
	{
		if (base.ragdolled)
		{
			aimTime = 0f;
			isAiming = false;
			delayEnableAimAfterRagdolled = 2f;
		}
		else if (delayEnableAimAfterRagdolled <= 0f)
		{
			ControlAimTime();
			if (isAiming)
			{
				_headtrack.LookAtPoint(AimPositionClamped(), 1f, 0f);
			}
		}
		else
		{
			aimTime = 0f;
			isAiming = false;
			delayEnableAimAfterRagdolled -= Time.deltaTime;
		}
	}

	protected virtual void IKBehaviour()
	{
		if (lockAimDebug)
		{
			if (!IsStrafingAnim)
			{
				base.isStrafing = true;
				IsStrafingAnim = true;
			}
			AimTo(DebugAimPosition, 0.5f);
		}
		UpdateAimBehaviour();
		if (lockAimDebug && debugShoots)
		{
			Attack();
		}
	}

	protected override void UpdateAnimator()
	{
		base.UpdateAnimator();
		UpdateCombatAnimator();
	}

	protected override void UpdateCombatAnimator()
	{
		base.UpdateCombatAnimator();
		UpdateShooterAnimator();
	}

	protected virtual void UpdateShooterAnimator()
	{
		if ((bool)shooterManager.CurrentWeapon)
		{
			IsReloading = IsAnimatorTag("IsReloading");
			IsEquipping = IsAnimatorTag("IsEquipping");
			bool flag = IsAiming && !IsReloading;
			if (flag && !aimEnable)
			{
				if (armAlignmentWeight > 0.5f && IsAnimatorTag("Upperbody Pose") && !base.animator.IsInTransition(upperBodyLayer) && base.animatorStateInfos.GetCurrentNormalizedTime(upperBodyLayer) > 0.5f)
				{
					shooterManager.CurrentWeapon.onEnableAim.Invoke();
					aimEnable = true;
				}
			}
			else if (!flag && aimEnable)
			{
				shooterManager.CurrentWeapon.onDisableAim.Invoke();
				aimEnable = false;
			}
			base.animator.SetBool("CanAim", flag && _canAiming);
			ShotID = shooterManager.GetShotID();
			UpperBodyID = shooterManager.GetUpperBodyID();
			MoveSetID = shooterManager.GetMoveSetID();
			base.animator.SetBool("IsAiming", flag);
		}
		else
		{
			IsReloading = false;
			base.animator.SetBool("IsAiming", value: false);
			base.animator.SetBool("CanAim", value: false);
			if (aimEnable)
			{
				shooterManager.CurrentWeapon.onDisableAim.Invoke();
				aimEnable = false;
			}
		}
		_onlyArmsLayerWeight = Mathf.Lerp(_onlyArmsLayerWeight, (isAiming || base.isRolling) ? 0f : (((bool)shooterManager && (bool)shooterManager.CurrentWeapon) ? 1f : 0f), 6f * Time.deltaTime);
		base.animator.SetLayerWeight(onlyArmsLayer, _onlyArmsLayerWeight);
	}

	protected virtual void UpdateAimBehaviour()
	{
		if (!base.isDead)
		{
			UpdateHeadTrack();
			CheckCanAiming();
			CheckCanShot();
			HandleShots();
			UpdateValidAim();
			ValidateShotAngle();
		}
	}

	protected virtual void HandleShots()
	{
		this.onStartUpdateIK?.Invoke();
		if (!IsIgnoreIK)
		{
			if ((bool)shooterManager && (bool)shooterManager.rWeapon && shooterManager.rWeapon.gameObject.activeSelf)
			{
				UpdateIKAdjust(isUsingLeftHand: false);
				RotateAimArm();
				RotateAimHand();
				if (!shooterManager.lWeapon || !shooterManager.lWeapon.gameObject.activeSelf)
				{
					UpdateSupportHandIK();
				}
			}
			if ((bool)shooterManager && (bool)shooterManager.lWeapon && shooterManager.lWeapon.gameObject.activeSelf)
			{
				UpdateIKAdjust(isUsingLeftHand: true);
				RotateAimArm(isUsingLeftHand: true);
				RotateAimHand(isUsingLeftHand: true);
				if (!shooterManager.rWeapon || !shooterManager.rWeapon.gameObject.activeSelf)
				{
					UpdateSupportHandIK(isUsingLeftHand: true);
				}
			}
			if (shots > 0)
			{
				Shot();
			}
		}
		this.onFinishUpdateIK?.Invoke();
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
		bool flag = !customAction && !IsReloading && !IsEquipping && CurrentWeaponIK != null && CurrentIKAdjust != null;
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

	protected virtual void ApplyOffsetToTargetBone(IKOffsetTransform iKOffset, Transform target, bool isValid)
	{
		target.localPosition = Vector3.Lerp(target.localPosition, isValid ? iKOffset.position : Vector3.zero, 10f * Time.deltaTime);
		target.localRotation = Quaternion.Lerp(target.localRotation, isValid ? Quaternion.Euler(iKOffset.eulerAngles) : Quaternion.Euler(Vector3.zero), 10f * Time.deltaTime);
	}

	protected virtual void UpdateValidAim()
	{
		if (isAiming && _canAiming)
		{
			AimPosition = Vector3.SmoothDamp(AimPosition, aimTarget, ref aimVelocity, aimSmoothDamp * Time.deltaTime);
			_lastaValidAimLocal = base.transform.InverseTransformPoint(AimPosition);
			return;
		}
		if (!isAiming)
		{
			_lastaValidAimLocal = defaultValidAimLocal;
		}
		AimPosition = base.transform.TransformPoint(_lastaValidAimLocal);
	}

	protected virtual Vector3 AimPositionClamped()
	{
		Vector3 position = defaultValidAimLocal;
		if (_canAiming)
		{
			position = base.transform.InverseTransformPoint(AimPosition);
			if (position.z < 0.5f)
			{
				position.z = 0.5f;
			}
		}
		return base.transform.TransformPoint(position);
	}

	protected virtual void UpdateHeadTrack()
	{
		if (!shooterManager || !_headtrack)
		{
			if ((bool)_headtrack)
			{
				_headtrack.offsetSpine = Vector2.Lerp(_headtrack.offsetSpine, Vector2.zero, _headtrack.smooth * Time.deltaTime);
				_headtrack.offsetHead = Vector2.Lerp(_headtrack.offsetHead, Vector2.zero, _headtrack.smooth * Time.deltaTime);
			}
		}
		else if (!CurrentActiveWeapon || !_headtrack || !shooterManager.CurrentWeaponIK)
		{
			if ((bool)_headtrack)
			{
				_headtrack.offsetSpine = Vector2.Lerp(_headtrack.offsetSpine, Vector2.zero, _headtrack.smooth * Time.deltaTime);
				_headtrack.offsetHead = Vector2.Lerp(_headtrack.offsetHead, Vector2.zero, _headtrack.smooth * Time.deltaTime);
			}
		}
		else if (isAiming)
		{
			IKAdjust obj = ((!isCrouching) ? (CurrentActiveWeapon.isLeftWeapon ? shooterManager.CurrentWeaponIK.standingAimingLeft : shooterManager.CurrentWeaponIK.standingAimingRight) : (CurrentActiveWeapon.isLeftWeapon ? shooterManager.CurrentWeaponIK.crouchingAimingLeft : shooterManager.CurrentWeaponIK.crouchingAimingRight));
			Vector2 spine = obj.spineOffset.spine;
			Vector2 head = obj.spineOffset.head;
			_headtrack.offsetSpine = Vector2.Lerp(_headtrack.offsetSpine, spine, _headtrack.smooth * Time.deltaTime);
			_headtrack.offsetHead = Vector2.Lerp(_headtrack.offsetHead, head, _headtrack.smooth * Time.deltaTime);
		}
		else
		{
			IKAdjust obj2 = ((!isCrouching) ? (CurrentActiveWeapon.isLeftWeapon ? shooterManager.CurrentWeaponIK.standingLeft : shooterManager.CurrentWeaponIK.standingRight) : (CurrentActiveWeapon.isLeftWeapon ? shooterManager.CurrentWeaponIK.crouchingLeft : shooterManager.CurrentWeaponIK.crouchingRight));
			Vector2 spine2 = obj2.spineOffset.spine;
			Vector2 head2 = obj2.spineOffset.head;
			_headtrack.offsetSpine = Vector2.Lerp(_headtrack.offsetSpine, spine2, _headtrack.smooth * Time.deltaTime);
			_headtrack.offsetHead = Vector2.Lerp(_headtrack.offsetHead, head2, _headtrack.smooth * Time.deltaTime);
		}
	}

	protected virtual void ValidateShotAngle()
	{
		vShooterWeapon currentActiveWeapon = CurrentActiveWeapon;
		if (debugAim && (bool)currentActiveWeapon)
		{
			Debug.DrawRay(currentActiveWeapon.aimReference.position, currentActiveWeapon.aimReference.forward * 100f, IsInShotAngle ? Color.green : Color.red);
		}
		if ((bool)shooterManager && isAiming && _canAiming && !IsReloading && !customAction && !isJumping && (bool)currentActiveWeapon)
		{
			float num = Vector3.Angle(currentActiveWeapon.aimReference.forward, (aimTarget - currentActiveWeapon.aimReference.position).normalized);
			IsInShotAngle = num <= maxAngleToShot;
		}
		else
		{
			IsInShotAngle = false;
		}
	}

	protected virtual void ControlAimTime()
	{
		if (aimTime > 0f)
		{
			aimTime -= Time.deltaTime;
		}
		else if (isAiming)
		{
			isAiming = false;
		}
	}

	protected virtual void UpdateSupportHandIK(bool isUsingLeftHand = false)
	{
		if (base.ragdolled)
		{
			return;
		}
		vShooterWeapon vShooterWeapon = (isUsingLeftHand ? shooterManager.lWeapon : shooterManager.rWeapon);
		if (LeftIK == null || !LeftIK.isValidBones)
		{
			LeftIK = new vIKSolver(base.animator, AvatarIKGoal.LeftHand);
		}
		if (RightIK == null || !RightIK.isValidBones)
		{
			RightIK = new vIKSolver(base.animator, AvatarIKGoal.RightHand);
		}
		vIKSolver vIKSolver = null;
		if (isUsingLeftHand)
		{
			vIKSolver = RightIK;
		}
		else
		{
			vIKSolver = LeftIK;
		}
		bool flag = (shooterManager.rWeapon ? shooterManager.useLeftIK : shooterManager.useRightIK);
		if (!shooterManager || !vShooterWeapon || !vShooterWeapon.gameObject.activeInHierarchy || !flag)
		{
			return;
		}
		if (IsAnimatorTag("Shot") && vShooterWeapon.disableIkOnShot)
		{
			handIKWeight = 0f;
			return;
		}
		bool flag2 = false;
		double num = Math.Round(base.animator.GetFloat("InputMagnitude"), 1);
		if (!IsAiming && !isAttacking)
		{
			vShooterWeapon.IKLocomotionOptions iKLocomotionOptions = (base.isStrafing ? CurrentActiveWeapon.strafeIKOptions : CurrentActiveWeapon.freeIKOptions);
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
		else if (IsAiming && !isAttacking)
		{
			flag2 = (shooterManager.isShooting ? (!CurrentActiveWeapon.disableIkOnShot) : CurrentActiveWeapon.useIKOnAiming);
		}
		else if (isAttacking)
		{
			flag2 = CurrentActiveWeapon.useIkAttacking;
		}
		IsSupportHandIKEnabled = flag2;
		vIKSolver = ((!isUsingLeftHand) ? LeftIK : RightIK);
		if (vIKSolver == null)
		{
			return;
		}
		_ = Vector3.zero;
		_ = Vector3.zero;
		if ((bool)shooterManager.weaponIKAdjustList)
		{
			if (isUsingLeftHand)
			{
				_ = shooterManager.weaponIKAdjustList.ikTargetRotationOffsetR;
				_ = shooterManager.weaponIKAdjustList.ikTargetPositionOffsetR;
			}
			else
			{
				_ = shooterManager.weaponIKAdjustList.ikTargetRotationOffsetL;
				_ = shooterManager.weaponIKAdjustList.ikTargetPositionOffsetL;
			}
		}
		if ((bool)vShooterWeapon && (bool)vShooterWeapon.handIKTarget && Time.timeScale > 0f && !IsReloading && !actions && !customAction && !IsEquipping && (base.isGrounded || isAiming) && !lockMovement && flag2)
		{
			handIKWeight = Mathf.Lerp(handIKWeight, 1f, 10f * Time.deltaTime);
		}
		else
		{
			handIKWeight = Mathf.Lerp(handIKWeight, 0f, 10f * Time.deltaTime);
		}
		if (handIKWeight <= 0f)
		{
			return;
		}
		vIKSolver.SetIKWeight(handIKWeight);
		if ((bool)shooterManager && (bool)vShooterWeapon && (bool)vShooterWeapon.handIKTarget)
		{
			vIKSolver.SetIKPosition(vShooterWeapon.handIKTargetOffset.position);
			vIKSolver.SetIKRotation(vShooterWeapon.handIKTargetOffset.rotation);
			if ((bool)shooterManager.CurrentWeaponIK)
			{
				vIKSolver.AnimationToIK();
			}
		}
	}

	protected virtual bool CanRotateAimArm()
	{
		return IsAnimatorTag("Upperbody Pose");
	}

	protected virtual void RotateAimArm(bool isUsingLeftHand = false)
	{
		if (!shooterManager)
		{
			return;
		}
		armAlignmentWeight = ((isAiming && !IsReloading && CanRotateAimArm() && _canAiming) ? Mathf.Lerp(armAlignmentWeight, 1f, smoothArmAlignmentWeight * Time.deltaTime) : 0f);
		if ((bool)CurrentActiveWeapon && armAlignmentWeight > 0.1f && CurrentActiveWeapon.alignRightUpperArmToAim)
		{
			Vector3 vector = AimPositionClamped();
			Vector3 vector2 = vector - CurrentActiveWeapon.aimReference.position;
			Vector3 forward = CurrentActiveWeapon.aimReference.forward;
			Transform transform = (isUsingLeftHand ? leftUpperArm : rightUpperArm);
			Quaternion quaternion = Quaternion.FromToRotation(transform.InverseTransformDirection(forward), transform.InverseTransformDirection(vector2));
			if (!float.IsNaN(quaternion.x) && !float.IsNaN(quaternion.y) && !float.IsNaN(quaternion.z))
			{
				upperArmRotationAlignment = (shooterManager.isShooting ? upperArmRotation : quaternion);
			}
			float num = Vector3.Angle(vector - aimAngleReference.transform.position, aimAngleReference.transform.forward);
			if (!(num > shooterManager.maxHandAngle) && !(num < 0f - shooterManager.maxHandAngle))
			{
				upperArmRotation = Quaternion.Lerp(upperArmRotation, upperArmRotationAlignment, shooterManager.smoothHandRotation * Time.deltaTime);
			}
			else
			{
				upperArmRotation = Quaternion.Euler(0f, 0f, 0f);
			}
			if (!float.IsNaN(upperArmRotation.x) && !float.IsNaN(upperArmRotation.y) && !float.IsNaN(upperArmRotation.z))
			{
				float num2 = (CurrentActiveWeapon.alignRightHandToAim ? (armAlignmentWeight * 0.5f) : armAlignmentWeight);
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
		if ((bool)CurrentActiveWeapon && armAlignmentWeight > 0.1f && CurrentActiveWeapon.alignRightHandToAim)
		{
			Vector3 vector = AimPositionClamped();
			Vector3 vector2 = vector - CurrentActiveWeapon.aimReference.position;
			Vector3 forward = CurrentActiveWeapon.aimReference.forward;
			Transform transform = (isUsingLeftHand ? leftHand : rightHand);
			Quaternion quaternion = Quaternion.FromToRotation(transform.InverseTransformDirection(forward), transform.InverseTransformDirection(vector2));
			if (!float.IsNaN(quaternion.x) && !float.IsNaN(quaternion.y) && !float.IsNaN(quaternion.z))
			{
				handRotationAlignment = (shooterManager.isShooting ? handRotation : quaternion);
			}
			float num = Vector3.Angle(vector - aimAngleReference.transform.position, aimAngleReference.transform.forward);
			if (!(num > shooterManager.maxHandAngle) && !(num < 0f - shooterManager.maxHandAngle))
			{
				handRotation = Quaternion.Lerp(handRotation, handRotationAlignment, shooterManager.smoothHandRotation * Time.deltaTime);
			}
			else
			{
				handRotation = Quaternion.Euler(0f, 0f, 0f);
			}
			if (!float.IsNaN(handRotation.x) && !float.IsNaN(handRotation.y) && !float.IsNaN(handRotation.z))
			{
				transform.localRotation *= Quaternion.Euler(handRotation.eulerAngles.NormalizeAngle() * armAlignmentWeight);
			}
			CurrentActiveWeapon.SetScopeLookTarget(vector);
		}
		else
		{
			handRotation = Quaternion.Euler(0f, 0f, 0f);
		}
	}

	protected virtual void CheckCanAiming()
	{
		if (base.ragdolled || (!base.isStrafing && !lockAimDebug) || customAction || IsReloading)
		{
			_canAiming = false;
			return;
		}
		Vector3 vector = aimTarget;
		vector.y = base.transform.position.y;
		bool num = Vector3.Angle(base.transform.forward, vector - base.transform.position) > aimTurnAngle;
		_canAiming = true;
		if (num && isAiming)
		{
			RotateTo(aimTarget - base.transform.position);
		}
	}

	protected virtual void Shot()
	{
		if (base.isDead || !shooterManager || !shooterManager.CurrentWeapon || customAction)
		{
			return;
		}
		if ((_canShot || forceCanShot) && !IsReloading && !_waitingReload && _canAiming && IsInShotAngle && isAiming)
		{
			forceCanShot = false;
			if (shooterManager.weaponHasAmmo)
			{
				if (shots > 0)
				{
					shooterManager.Shoot(CurrentActiveWeapon.muzzle.position + CurrentActiveWeapon.muzzle.forward * 100f);
					shots--;
				}
			}
			else if (!IsReloading && !_waitingReload)
			{
				StartCoroutine(Reload());
			}
		}
		if (!_canShot && !IsReloading && !_waitingReload && doReloadWhileWaiting && shooterManager.CurrentWeapon.ammoCount < shooterManager.CurrentWeapon.clipSize)
		{
			StartCoroutine(Reload());
		}
	}

	protected virtual IEnumerator Reload()
	{
		_waitingReload = true;
		yield return new WaitForSeconds(0.5f);
		shooterManager.ReloadWeapon();
		float minTimeToStartReload = 2f;
		while (!IsReloading)
		{
			minTimeToStartReload -= Time.deltaTime;
			if (minTimeToStartReload <= 0f)
			{
				break;
			}
			yield return null;
		}
		while (IsReloading)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		_waitingReload = false;
	}

	protected override void TryBlockAttack(vDamage damage)
	{
		if (shooterManager.CurrentWeapon != null)
		{
			isBlocking = false;
		}
		else
		{
			base.TryBlockAttack(damage);
		}
	}

	public override void Blocking()
	{
		if (shooterManager.CurrentWeapon != null)
		{
			isBlocking = false;
		}
		else
		{
			base.Blocking();
		}
	}

	public override void AimTo(Vector3 point, float timeToCancelAim = 1f, object sender = null)
	{
		aimTime = timeToCancelAim;
		isAiming = true;
		aimTarget = point;
	}

	public override void AimToTarget(float stayLookTime = 1f, object sender = null)
	{
		aimTime = stayLookTime;
		isAiming = true;
		if ((bool)currentTarget.transform && (bool)currentTarget.collider)
		{
			aimTarget = _lastTargetPosition + Vector3.up * (currentTarget.collider.bounds.size.y * 0.5f + aimTargetHeight);
		}
		else
		{
			aimTarget = _lastTargetPosition + Vector3.up * aimTargetHeight;
		}
		if (!base.isStrafing && input.magnitude > 0.1f)
		{
			base.isStrafing = true;
		}
	}

	public override void StrafeMoveTo(Vector3 newDestination, Vector3 targetDirection, vAIMovementSpeed speed = vAIMovementSpeed.Walking)
	{
		if (isAiming)
		{
			if (useNavMeshAgent && (bool)navMeshAgent && navMeshAgent.isOnNavMesh && navMeshAgent.isStopped)
			{
				navMeshAgent.isStopped = false;
			}
			SetStrafeLocomotion();
			SetSpeed(speed);
			destination = newDestination;
			if (input.magnitude > 0.1f)
			{
				temporaryDirection = targetDirection;
				temporaryDirectionTime = 1f;
			}
		}
		else
		{
			base.StrafeMoveTo(newDestination, targetDirection, speed);
		}
	}

	[SpecialName]
	Transform vIDamageReceiver.get_transform()
	{
		return base.transform;
	}

	[SpecialName]
	GameObject vIDamageReceiver.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	GameObject vIShooterIKController.get_gameObject()
	{
		return base.gameObject;
	}
}
