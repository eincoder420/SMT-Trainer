using UnityEngine;

namespace Invector.vCharacterController;

public static class vAnimatorParameters
{
	public static int InputHorizontal = Animator.StringToHash("InputHorizontal");

	public static int InputVertical = Animator.StringToHash("InputVertical");

	public static int InputMagnitude = Animator.StringToHash("InputMagnitude");

	public static int RotationMagnitude = Animator.StringToHash("RotationMagnitude");

	public static int TurnOnSpotDirection = Animator.StringToHash("TurnOnSpotDirection");

	public static int ActionState = Animator.StringToHash("ActionState");

	public static int ResetState = Animator.StringToHash("ResetState");

	public static int IsDead = Animator.StringToHash("isDead");

	public static int IsGrounded = Animator.StringToHash("IsGrounded");

	public static int IsCrouching = Animator.StringToHash("IsCrouching");

	public static int IsStrafing = Animator.StringToHash("IsStrafing");

	public static int IsSprinting = Animator.StringToHash("IsSprinting");

	public static int IsSliding = Animator.StringToHash("IsSliding");

	public static int GroundDistance = Animator.StringToHash("GroundDistance");

	public static int GroundAngle = Animator.StringToHash("GroundAngle");

	public static int VerticalVelocity = Animator.StringToHash("VerticalVelocity");

	public static int IdleRandom = Animator.StringToHash("IdleRandom");

	public static int IdleRandomTrigger = Animator.StringToHash("IdleRandomTrigger");

	public static int AttackID = Animator.StringToHash("AttackID");

	public static int DefenseID = Animator.StringToHash("DefenseID");

	public static int IsBlocking = Animator.StringToHash("IsBlocking");

	public static int MoveSet_ID = Animator.StringToHash("MoveSet_ID");

	public static int RecoilID = Animator.StringToHash("RecoilID");

	public static int TriggerRecoil = Animator.StringToHash("TriggerRecoil");

	public static int WeakAttack = Animator.StringToHash("WeakAttack");

	public static int StrongAttack = Animator.StringToHash("StrongAttack");

	public static int UpperBody_ID = Animator.StringToHash("UpperBody_ID");

	public static int CanAim = Animator.StringToHash("CanAim");

	public static int IsAiming = Animator.StringToHash("IsAiming");

	public static int IsHipFire = Animator.StringToHash("IsHipFire");

	public static int Shot_ID = Animator.StringToHash("Shot_ID");

	public static int PowerCharger = Animator.StringToHash("PowerCharger");
}
