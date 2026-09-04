using System;
using Invector.vEventSystems;
using UnityEngine;

namespace Invector.vCharacterController;

public class vThirdPersonAnimator : vThirdPersonMotor
{
	[HideInInspector]
	public Transform matchTarget;

	private float randomIdleCount;

	public const float walkSpeed = 0.5f;

	public const float runningSpeed = 1f;

	public const float sprintSpeed = 1.5f;

	private bool triggerDieBehaviour;

	public Vector3 lastCharacterPosition { get; protected set; }

	protected override void Start()
	{
		base.Start();
		RegisterAnimatorStateInfos();
	}

	protected virtual void RegisterAnimatorStateInfos()
	{
		base.animatorStateInfos = new vAnimatorStateInfos(GetComponent<Animator>());
		base.animatorStateInfos.RegisterListener();
	}

	protected virtual void OnEnable()
	{
		if (base.animatorStateInfos.animator != null)
		{
			base.animatorStateInfos.RegisterListener();
		}
	}

	protected virtual void OnDisable()
	{
		base.animatorStateInfos.RemoveListener();
	}

	public virtual void UpdateAnimator()
	{
		if (!(base.animator == null) && base.animator.enabled)
		{
			AnimatorLayerControl();
			ActionsControl();
			TriggerRandomIdle();
			UpdateAnimatorParameters();
			DeadAnimation();
		}
	}

	public virtual void AnimatorLayerControl()
	{
		baseLayerInfo = base.animator.GetCurrentAnimatorStateInfo(base.baseLayer);
		underBodyInfo = base.animator.GetCurrentAnimatorStateInfo(base.underBodyLayer);
		rightArmInfo = base.animator.GetCurrentAnimatorStateInfo(base.rightArmLayer);
		leftArmInfo = base.animator.GetCurrentAnimatorStateInfo(base.leftArmLayer);
		upperBodyInfo = base.animator.GetCurrentAnimatorStateInfo(base.upperBodyLayer);
		fullBodyInfo = base.animator.GetCurrentAnimatorStateInfo(base.fullbodyLayer);
	}

	public virtual void ActionsControl()
	{
		isRolling = IsAnimatorTag("IsRolling");
		isTurningOnSpot = IsAnimatorTag("TurnOnSpot");
		lockAnimMovement = IsAnimatorTag("LockMovement");
		lockAnimRotation = IsAnimatorTag("LockRotation");
		customAction = IsAnimatorTag("CustomAction");
		isInAirborne = IsAnimatorTag("Airborne");
	}

	public virtual void UpdateAnimatorParameters()
	{
		if (!disableAnimations)
		{
			base.animator.SetBool(vAnimatorParameters.IsStrafing, base.isStrafing);
			base.animator.SetBool(vAnimatorParameters.IsSprinting, base.isSprinting);
			base.animator.SetBool(vAnimatorParameters.IsSliding, base.isSliding && !isRolling);
			base.animator.SetBool(vAnimatorParameters.IsCrouching, isCrouching);
			base.animator.SetBool(vAnimatorParameters.IsGrounded, base.isGrounded);
			base.animator.SetBool(vAnimatorParameters.IsDead, base.isDead);
			base.animator.SetFloat(vAnimatorParameters.GroundDistance, groundDistance);
			base.animator.SetFloat(vAnimatorParameters.GroundAngle, GroundAngleFromDirection());
			if (!base.isGrounded)
			{
				base.animator.SetFloat(vAnimatorParameters.VerticalVelocity, verticalVelocity);
			}
			if (base.isStrafing)
			{
				base.animator.SetFloat(vAnimatorParameters.InputHorizontal, horizontalSpeed, strafeSpeed.animationSmooth, Time.fixedDeltaTime);
				base.animator.SetFloat(vAnimatorParameters.InputVertical, verticalSpeed, strafeSpeed.animationSmooth, Time.fixedDeltaTime);
			}
			else
			{
				base.animator.SetFloat(vAnimatorParameters.InputVertical, verticalSpeed, freeSpeed.animationSmooth, Time.fixedDeltaTime);
				base.animator.SetFloat(vAnimatorParameters.InputHorizontal, 0f, freeSpeed.animationSmooth, Time.fixedDeltaTime);
			}
			base.animator.SetFloat(vAnimatorParameters.InputMagnitude, Mathf.LerpUnclamped(inputMagnitude, 0f, stopMoveWeight), base.isStrafing ? strafeSpeed.animationSmooth : freeSpeed.animationSmooth, Time.fixedDeltaTime);
			if (useLeanMovementAnim && inputMagnitude >= 0.1f)
			{
				base.animator.SetFloat(vAnimatorParameters.RotationMagnitude, rotationMagnitude, leanSmooth, Time.fixedDeltaTime);
			}
			else if (useTurnOnSpotAnim && inputMagnitude < 0.1f)
			{
				base.animator.SetFloat(vAnimatorParameters.RotationMagnitude, (float)Math.Round(rotationMagnitude, 2), (rotationMagnitude == 0f) ? 0.1f : 0.01f, Time.fixedDeltaTime);
			}
		}
	}

	public virtual void SetAnimatorMoveSpeed(vMovementSpeed speed)
	{
		Vector3 vector = base.transform.InverseTransformDirection(moveDirection);
		verticalSpeed = vector.z;
		horizontalSpeed = vector.x;
		Vector2 vector2 = new Vector2(verticalSpeed, horizontalSpeed);
		if (speed.walkByDefault || base.alwaysWalkByDefault)
		{
			inputMagnitude = Mathf.Clamp(vector2.magnitude, 0f, base.isSprinting ? 1f : 0.5f);
			return;
		}
		float magnitude = vector2.magnitude;
		sprintWeight = Mathf.Lerp(sprintWeight, base.isSprinting ? 1f : 0f, (base.isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.fixedDeltaTime);
		inputMagnitude = Mathf.Clamp(Mathf.Lerp(magnitude, magnitude + 0.5f, sprintWeight), 0f, base.isSprinting ? 1.5f : 1f);
	}

	public virtual void ResetInputAnimatorParameters()
	{
		base.animator.SetBool(vAnimatorParameters.IsSprinting, value: false);
		base.animator.SetBool(vAnimatorParameters.IsSliding, value: false);
		base.animator.SetBool(vAnimatorParameters.IsCrouching, value: false);
		base.animator.SetBool(vAnimatorParameters.IsGrounded, value: true);
		base.animator.SetFloat(vAnimatorParameters.GroundDistance, 0f);
		base.animator.SetFloat("InputHorizontal", 0f);
		base.animator.SetFloat("InputVertical", 0f);
		base.animator.SetFloat("InputMagnitude", 0f);
	}

	protected virtual void TriggerRandomIdle()
	{
		if (input != Vector3.zero || customAction || !(randomIdleTime > 0f))
		{
			return;
		}
		if (input.sqrMagnitude == 0f && !isCrouching && _capsuleCollider.enabled && base.isGrounded)
		{
			randomIdleCount += Time.fixedDeltaTime;
			if (randomIdleCount > 6f)
			{
				randomIdleCount = 0f;
				base.animator.SetTrigger(vAnimatorParameters.IdleRandomTrigger);
				base.animator.SetInteger(vAnimatorParameters.IdleRandom, UnityEngine.Random.Range(1, 4));
			}
		}
		else
		{
			randomIdleCount = 0f;
			base.animator.SetInteger(vAnimatorParameters.IdleRandom, 0);
		}
	}

	protected virtual void DeadAnimation()
	{
		if (!base.isDead)
		{
			return;
		}
		if (!triggerDieBehaviour)
		{
			triggerDieBehaviour = true;
			DeathBehaviour();
		}
		if (deathBy == DeathBy.Animation)
		{
			int layerIndex = 0;
			vAnimatorStateInfos.vStateInfo stateInfoUsingTag = base.animatorStateInfos.GetStateInfoUsingTag("Dead");
			if (stateInfoUsingTag != null && !base.animator.IsInTransition(layerIndex) && stateInfoUsingTag.normalizedTime >= 0.99f && groundDistance <= 0.15f)
			{
				RemoveComponents();
			}
		}
		else if (deathBy == DeathBy.AnimationWithRagdoll)
		{
			int layerIndex2 = 0;
			vAnimatorStateInfos.vStateInfo stateInfoUsingTag2 = base.animatorStateInfos.GetStateInfoUsingTag("Dead");
			if (stateInfoUsingTag2 != null && !base.animator.IsInTransition(layerIndex2) && stateInfoUsingTag2.normalizedTime >= 0.8f)
			{
				base.onActiveRagdoll.Invoke(null);
			}
		}
		else if (deathBy == DeathBy.Ragdoll)
		{
			base.onActiveRagdoll.Invoke(null);
		}
	}

	public virtual void SetActionState(int value)
	{
		base.animator.SetInteger(vAnimatorParameters.ActionState, value);
	}

	public virtual bool IsAnimatorTag(string tag)
	{
		if (base.animator == null)
		{
			return false;
		}
		if (base.animatorStateInfos != null && base.animatorStateInfos.HasTag(tag))
		{
			return true;
		}
		if (baseLayerInfo.IsTag(tag))
		{
			return true;
		}
		if (underBodyInfo.IsTag(tag))
		{
			return true;
		}
		if (rightArmInfo.IsTag(tag))
		{
			return true;
		}
		if (leftArmInfo.IsTag(tag))
		{
			return true;
		}
		if (upperBodyInfo.IsTag(tag))
		{
			return true;
		}
		if (fullBodyInfo.IsTag(tag))
		{
			return true;
		}
		return false;
	}

	public virtual void MatchTarget(Vector3 matchPosition, Quaternion matchRotation, AvatarTarget target, MatchTargetWeightMask weightMask, float normalisedStartTime, float normalisedEndTime)
	{
		if (!base.animator.isMatchingTarget && !base.animator.IsInTransition(0) && !(Mathf.Repeat(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f) > normalisedEndTime))
		{
			base.animator.MatchTarget(matchPosition, matchRotation, target, weightMask, normalisedStartTime, normalisedEndTime);
		}
	}
}
