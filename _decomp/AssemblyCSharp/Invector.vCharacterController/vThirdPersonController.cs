using System.Collections;
using UnityEngine;

namespace Invector.vCharacterController;

[vClassHeader("THIRD PERSON CONTROLLER", true, "icon_v2", false, "", iconName = "controllerIcon")]
public class vThirdPersonController : vThirdPersonAnimator
{
	public virtual void MoveToPosition(Transform targetPosition)
	{
		MoveToPosition(targetPosition.position);
	}

	public virtual void MoveToPosition(Vector3 targetPosition)
	{
		Vector3 vector = targetPosition - base.transform.position;
		vector.y = 0f;
		if (vector.magnitude < 0.1f)
		{
			input = Vector3.zero;
			moveDirection = Vector3.zero;
		}
		else
		{
			input = base.transform.InverseTransformDirection(vector.normalized);
			moveDirection = vector.normalized;
		}
	}

	public virtual void ControlAnimatorRootMotion()
	{
		if (!base.enabled)
		{
			return;
		}
		if (isRolling)
		{
			RollBehavior();
			return;
		}
		if (customAction || lockAnimMovement)
		{
			StopCharacterWithLerp();
			base.transform.position = base.animator.rootPosition;
			base.transform.rotation = base.animator.rootRotation;
		}
		if (useRootMotion)
		{
			MoveCharacter(moveDirection);
		}
	}

	public virtual void ControlLocomotionType()
	{
		if (lockAnimMovement || lockMovement || customAction)
		{
			return;
		}
		if (!lockSetMoveSpeed)
		{
			if ((locomotionType.Equals(LocomotionType.FreeWithStrafe) && !base.isStrafing) || locomotionType.Equals(LocomotionType.OnlyFree))
			{
				SetControllerMoveSpeed(freeSpeed);
				SetAnimatorMoveSpeed(freeSpeed);
			}
			else if (locomotionType.Equals(LocomotionType.OnlyStrafe) || (locomotionType.Equals(LocomotionType.FreeWithStrafe) && base.isStrafing))
			{
				base.isStrafing = true;
				SetControllerMoveSpeed(strafeSpeed);
				SetAnimatorMoveSpeed(strafeSpeed);
			}
		}
		if (!useRootMotion)
		{
			MoveCharacter(moveDirection);
		}
	}

	public virtual void ControlRotationType()
	{
		if (!lockAnimRotation && !lockRotation && !customAction && !isRolling && (input != Vector3.zero || (base.isStrafing ? strafeSpeed.rotateWithCamera : freeSpeed.rotateWithCamera)))
		{
			if (lockAnimMovement)
			{
				inputSmooth = Vector3.Lerp(inputSmooth, input, (base.isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
			}
			Vector3 direction = ((((base.isStrafing && base.isGrounded && (!base.isSprinting || !sprintOnlyFree)) || (freeSpeed.rotateWithCamera && input == Vector3.zero)) && (bool)rotateTarget) ? rotateTarget.forward : moveDirection);
			RotateToDirection(direction);
		}
	}

	public virtual void ControlKeepDirection()
	{
		if (!keepDirection)
		{
			oldInput = input;
		}
		else if ((input.magnitude < 0.01f || Vector3.Distance(oldInput, input) > 0.9f) && keepDirection)
		{
			keepDirection = false;
		}
	}

	public virtual void UpdateMoveDirection(Transform referenceTransform = null)
	{
		if (isRolling && !rollControl)
		{
			moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, (base.isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
		}
		else if ((bool)referenceTransform && !rotateByWorld)
		{
			Vector3 right = referenceTransform.right;
			right.y = 0f;
			Vector3 vector = Quaternion.AngleAxis(-90f, Vector3.up) * right;
			moveDirection = inputSmooth.x * right + inputSmooth.z * vector;
		}
		else
		{
			moveDirection = new Vector3(inputSmooth.x, 0f, inputSmooth.z);
		}
	}

	public virtual void Sprint(bool value)
	{
		bool flag = (!isCrouching || (!base.inCrouchArea && CanExitCrouch())) && currentStamina > 0f && base.hasMovementInput && (!base.isStrafing || (!((double)horizontalSpeed >= 0.5) && !((double)horizontalSpeed <= -0.5) && !(verticalSpeed <= 0.1f)) || sprintOnlyFree);
		if (value && flag)
		{
			if (currentStamina > (finishStaminaOnSprint ? sprintStamina : 0f) && base.hasMovementInput)
			{
				finishStaminaOnSprint = false;
				if (base.isGrounded && useContinuousSprint)
				{
					isCrouching = false;
					base.isSprinting = !base.isSprinting;
					if (base.isSprinting)
					{
						OnStartSprinting.Invoke();
						base.alwaysWalkByDefault = false;
					}
					else
					{
						OnFinishSprinting.Invoke();
					}
				}
				else if (!base.isSprinting)
				{
					OnStartSprinting.Invoke();
					base.alwaysWalkByDefault = false;
					base.isSprinting = true;
				}
			}
			else if (!useContinuousSprint && base.isSprinting)
			{
				if (currentStamina <= 0f)
				{
					finishStaminaOnSprint = true;
					OnFinishSprintingByStamina.Invoke();
				}
				base.isSprinting = false;
				OnFinishSprinting.Invoke();
			}
		}
		else if (base.isSprinting && (!useContinuousSprint || !flag))
		{
			if (currentStamina <= 0f)
			{
				finishStaminaOnSprint = true;
				OnFinishSprintingByStamina.Invoke();
			}
			base.isSprinting = false;
			OnFinishSprinting.Invoke();
		}
	}

	public virtual void Crouch()
	{
		if (base.isGrounded && !customAction)
		{
			AutoCrouch();
			if (isCrouching && CanExitCrouch())
			{
				isCrouching = false;
				return;
			}
			isCrouching = true;
			base.isSprinting = false;
		}
	}

	public virtual void Strafe()
	{
		base.isStrafing = !base.isStrafing;
	}

	public virtual void Jump(bool consumeStamina = false)
	{
		jumpCounter = jumpTimer;
		OnJump.Invoke();
		if (input.sqrMagnitude < 0.1f)
		{
			StartCoroutine(DelayToJump());
			base.animator.CrossFadeInFixedTime("Jump", 0.1f);
		}
		else
		{
			isJumping = true;
			base.animator.CrossFadeInFixedTime("JumpMove", 0.2f);
		}
		if (consumeStamina)
		{
			ReduceStamina(jumpStamina, accumulative: false);
			currentStaminaRecoveryDelay = 1f;
		}
	}

	protected IEnumerator DelayToJump()
	{
		yield return new WaitForSeconds(jumpStandingDelay);
		isJumping = true;
	}

	public virtual void Roll()
	{
		OnRoll.Invoke();
		isRolling = true;
		base.animator.CrossFadeInFixedTime("Roll", rollTransition, base.baseLayer);
		ReduceStamina(rollStamina, accumulative: false);
		currentStaminaRecoveryDelay = 2f;
	}

	protected override void OnTriggerStay(Collider other)
	{
		try
		{
			CheckForAutoCrouch(other);
		}
		catch (UnityException ex)
		{
			Debug.LogWarning(ex.Message);
		}
		base.OnTriggerStay(other);
	}

	protected override void OnTriggerExit(Collider other)
	{
		AutoCrouchExit(other);
		base.OnTriggerExit(other);
	}
}
