using System;
using System.Collections;
using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController;

public class vThirdPersonMotor : vCharacter, vIAnimatorStateInfoController
{
	public enum LocomotionType
	{
		FreeWithStrafe,
		OnlyStrafe,
		OnlyFree
	}

	public enum CustomFixedTimeStep
	{
		Default,
		FPS30,
		FPS60,
		FPS75,
		FPS90,
		FPS120,
		FPS144
	}

	public enum GroundCheckMethod
	{
		Low,
		High
	}

	public enum StopMoveCheckMethod
	{
		RayCast,
		SphereCast,
		CapsuleCast
	}

	[Serializable]
	public class vMovementSpeed
	{
		[vHelpBox("Higher means faster/responsive movement, lower means smooth movement", vHelpBoxAttribute.MessageType.None)]
		[Range(1f, 20f)]
		public float movementSmooth = 6f;

		[vHelpBox("Lower means faster transitions between animations, higher means slower", vHelpBoxAttribute.MessageType.None)]
		[Range(0f, 1f)]
		public float animationSmooth = 0.2f;

		[Tooltip("Rotation speed of the character")]
		public float rotationSpeed = 20f;

		[Tooltip("Character will limit the movement to walk instead of running")]
		public bool walkByDefault;

		[Tooltip("Rotate with the Camera forward when standing idle")]
		public bool rotateWithCamera;

		[Tooltip("Speed to Walk using rigidbody or extra speed if you're using RootMotion")]
		public float walkSpeed = 2f;

		[Tooltip("Speed to Run using rigidbody or extra speed if you're using RootMotion")]
		public float runningSpeed = 4f;

		[Tooltip("Speed to Sprint using rigidbody or extra speed if you're using RootMotion")]
		public float sprintSpeed = 6f;

		[Tooltip("Speed to Crouch using rigidbody or extra speed if you're using RootMotion")]
		public float crouchSpeed = 2f;
	}

	[vEditorToolbar("Stamina", false, "", false, false, order = 2)]
	public float maxStamina = 200f;

	public float staminaRecovery = 1.2f;

	internal float currentStamina;

	internal float currentStaminaRecoveryDelay;

	public float sprintStamina = 30f;

	public float jumpStamina = 30f;

	public float rollStamina = 25f;

	public bool Energized;

	[vEditorToolbar("Events", false, "", false, false, order = 7)]
	public UnityEvent OnRoll;

	public UnityEvent OnJump;

	public UnityEvent OnStartSprinting;

	public UnityEvent OnFinishSprinting;

	public UnityEvent OnFinishSprintingByStamina;

	public UnityEvent OnStaminaEnd;

	[vEditorToolbar("Crouch", false, "", false, false, order = 3)]
	[Range(1f, 2.5f)]
	public float crouchHeightReduction = 1.5f;

	[Tooltip("What objects can make the character auto crouch")]
	public LayerMask autoCrouchLayer = 1;

	[Tooltip("[SPHERECAST] ADJUST IN PLAY MODE - White Spherecast put just above the head, this will make the character Auto-Crouch if something hit the sphere.")]
	public float crouchHeadDetect = 0.95f;

	[vEditorToolbar("Locomotion", false, "", false, false, order = 0)]
	[vSeparator("Movement Settings", "")]
	[Tooltip("Multiply the current speed of the controller rigidbody velocity")]
	public float speedMultiplier = 1f;

	[Tooltip("Use this to rotate the character using the World axis, or false to use the camera axis - CHECK for Isometric Camera")]
	public bool rotateByWorld;

	[vHelpBox("FreeLocomotion: Rotate on any direction regardless of the camera \nStrafeLocomotion: Move always facing foward (extra directional animations)", vHelpBoxAttribute.MessageType.None)]
	public LocomotionType locomotionType;

	public vMovementSpeed freeSpeed;

	public vMovementSpeed strafeSpeed;

	[vSeparator("Extra Animation Settings", "")]
	[Tooltip("Use it for debug purposes")]
	public bool disableAnimations;

	[Tooltip("Turn off if you have 'in place' animations and use this values above to move the character, or use with root motion as extra speed")]
	[vHelpBox("When 'Use RootMotion' is checked, make sure to reset all speeds to zero to use the original root motion velocity.", vHelpBoxAttribute.MessageType.None)]
	public bool useRootMotion;

	[Tooltip("While in Free Locomotion the character will lean to left/right when steering")]
	public bool useLeanMovementAnim = true;

	[Tooltip("Smooth value for the Lean Movement animation")]
	[Range(0.01f, 0.1f)]
	public float leanSmooth = 0.05f;

	[Tooltip("Check this to use the TurnOnSpot animations while the character is stading still and rotating in place")]
	public bool useTurnOnSpotAnim = true;

	[Tooltip("Put your Random Idle animations at the AnimatorController and select a value to randomize, 0 is disable.")]
	public float randomIdleTime;

	internal bool ignoreAnimatorMovement;

	[vSeparator("Extra Movement Settings", "")]
	[Tooltip("Check This to use sprint on press button to your Character run until the stamina finish or movement stops\nIf uncheck your Character will sprint as long as the SprintInput is pressed or the stamina finishes")]
	public bool useContinuousSprint = true;

	[Tooltip("Check this to sprint always in free movement")]
	public bool sprintOnlyFree = true;

	[vHelpBox("Set the FixedTimeStep to match the FPS of your Game, \nEx: If your game aims to run at 30fps, select FPS30 to match the FixedUpdate Physics", vHelpBoxAttribute.MessageType.None)]
	public CustomFixedTimeStep customFixedTimeStep = CustomFixedTimeStep.FPS60;

	[vEditorToolbar("Jump / Airborne", false, "", false, false, order = 3)]
	[vHelpBox("Jump only works via Rigidbody Physics, if you want Jump that use only RootMotion make sure to use the AnimatorTag 'CustomAction' ", vHelpBoxAttribute.MessageType.None)]
	[vSeparator("Jump", "")]
	[Tooltip("Use the currently Rigidbody Velocity to influence on the Jump Distance")]
	public bool jumpWithRigidbodyForce;

	[Tooltip("Rotate or not while airborne")]
	public bool jumpAndRotate = true;

	[Tooltip("How much time the character will be jumping")]
	public float jumpTimer = 0.3f;

	[Tooltip("Delay to match the animation anticipation")]
	public float jumpStandingDelay = 0.25f;

	internal float jumpCounter;

	[Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
	public float jumpHeight = 4f;

	[vSeparator("Falling", "")]
	[Tooltip("Speed that the character will move while airborne")]
	public float airSpeed = 5f;

	[Tooltip("Smoothness of the direction while airborne")]
	public float airSmooth = 6f;

	[Tooltip("Apply extra gravity when the character is not grounded")]
	public float extraGravity = -10f;

	[Tooltip("Limit of the vertival velocity when Falling")]
	public float limitFallVelocity = -15f;

	[Tooltip("Turn the Ragdoll On when falling at high speed (check VerticalVelocity) - leave the value with 0 if you don't want this feature")]
	public float ragdollVelocity = -15f;

	[vSeparator("Fall Damage", "")]
	public float fallMinHeight = 6f;

	public float fallMinVerticalVelocity = -10f;

	public float fallDamage = 10f;

	[vEditorToolbar("Roll", false, "", false, false, order = 4)]
	public bool useRollRootMotion = true;

	[Tooltip("Animation Transition from current animation to Roll")]
	public float rollTransition = 0.25f;

	[Tooltip("Can control the Roll Direction")]
	public bool rollControl = true;

	[Tooltip("Speed of the Roll Movement")]
	public float rollSpeed;

	[Tooltip("Speed of the Roll Rotation")]
	public float rollRotationSpeed = 20f;

	[vHideInInspector("Roll use gravity inflence", false)]
	public bool rollUseGravity = true;

	[vHideInInspector("rollUseGravity", false)]
	[Tooltip("Normalized Time of the roll animation to enable gravity influence")]
	public float rollUseGravityTime = 0.2f;

	[Tooltip("Use the normalized time of the animation to know when you can roll again")]
	[Range(0f, 1f)]
	public float timeToRollAgain = 0.75f;

	[Tooltip("Ignore all damage while is rolling, include Damage that ignore defence")]
	public bool noDamageWhileRolling = true;

	[Tooltip("Ignore damage that needs to activate ragdoll")]
	public bool noActiveRagdollWhileRolling = true;

	[vEditorToolbar("Grounded", false, "", false, false, order = 3)]
	[vSeparator("Ground", "")]
	[Tooltip("Layers that the character can walk on")]
	public LayerMask groundLayer = 1;

	[Tooltip("Ground Check Method To check ground Distance and ground angle\n*Simple: Use just a single Raycast\n*Normal: Use Raycast and SphereCast\n*Complex: Use SphereCastAll")]
	public GroundCheckMethod groundCheckMethod = GroundCheckMethod.High;

	[Tooltip("The length of the Ray cast to detect ground ")]
	public float groundDetectionDistance = 10f;

	[Tooltip("Snaps the capsule collider to the ground surface, recommend when using complex terrains or inclined ramps")]
	public bool useSnapGround = true;

	[Range(0f, 1f)]
	public float snapPower = 0.5f;

	[Tooltip("Distance to became not grounded")]
	[Range(0f, 10f)]
	public float groundMinDistance = 0.1f;

	[Range(0f, 10f)]
	public float groundMaxDistance = 0.5f;

	[Tooltip("Max angle to walk")]
	[vSeparator("StopMove", "")]
	public LayerMask stopMoveLayer;

	[vHelpBox("Character will stop moving, ex: walls - set the layer to nothing to not use", vHelpBoxAttribute.MessageType.None)]
	public float stopMoveRayDistance = 1f;

	public float stopMoveMaxHeight = 1.6f;

	public StopMoveCheckMethod stopMoveCheckMethod;

	[vSeparator("Slope Limit", "")]
	public bool useSlopeLimit = true;

	[Range(30f, 80f)]
	public float slopeLimit = 75f;

	public float stopSlopeMargin = 20f;

	public float slopeSidewaysSmooth = 2f;

	public float slopeMinDistance;

	public float slopeMaxDistance = 1.5f;

	public float slopeLimitHeight = 0.2f;

	protected float _slopeSidewaysSmooth;

	[HideInInspector]
	public bool steepSlopeAhead;

	[vSeparator("Slide On Slopes", "")]
	public bool useSlide = true;

	[Tooltip("Velocity to slide down when on a slope limit ramp")]
	[Range(0f, 30f)]
	public float slideDownVelocity = 10f;

	[Tooltip("Smooth to slide down the controller")]
	public float slideDownSmooth = 2f;

	[Tooltip("Velocity to slide sideways when on a slope limit ramp")]
	[Range(0f, 1f)]
	public float slideSidewaysVelocity = 0.5f;

	[Range(0f, 1f)]
	[Tooltip("Delay to start sliding once the character is standing on a slope")]
	public float slidingEnterTime = 0.2f;

	internal float _slidingEnterTime;

	[Range(0f, 1f)]
	[Tooltip("Delay to rotate once the character started sliding")]
	public float rotateSlopeEnterTime = 0.1f;

	[Tooltip("Smooth to rotate the controller")]
	public float rotateDownSlopeSmooth = 8f;

	internal float _rotateSlopeEnterTime;

	[vSeparator("Step Offset", "")]
	public bool useStepOffset = true;

	[Tooltip("Offset max height to walk on steps - YELLOW Raycast in front of the legs")]
	[Range(0f, 1f)]
	public float stepOffsetMaxHeight = 0.5f;

	[Tooltip("Offset min height to walk on steps. Make sure to keep slight above the floor - YELLOW Raycast in front of the legs")]
	[Range(0f, 1f)]
	public float stepOffsetMinHeight;

	[Tooltip("Offset distance to walk on steps - YELLOW Raycast in front of the legs")]
	[Range(0f, 1f)]
	public float stepOffsetDistance = 0.1f;

	internal float stopMoveWeight;

	internal float sprintWeight;

	internal float groundDistance;

	public RaycastHit groundHit;

	[vEditorToolbar("Debug", false, "", false, false, order = 9)]
	[Header("--- Debug Info ---")]
	public bool debugWindow;

	public vAnimatorStateInfos _animatorStateInfos;

	internal bool isRolling;

	internal bool isJumping;

	internal bool isInAirborne;

	internal bool isTurningOnSpot;

	internal bool customAction;

	internal Rigidbody _rigidbody;

	internal PhysicMaterial frictionPhysics;

	internal PhysicMaterial maxFrictionPhysics;

	internal PhysicMaterial slippyPhysics;

	internal CapsuleCollider _capsuleCollider;

	internal float defaultSpeedMultiplier = 1f;

	internal float inputMagnitude;

	internal float rotationMagnitude;

	internal float verticalSpeed;

	internal float horizontalSpeed;

	internal bool invertVerticalSpeed;

	internal bool invertHorizontalSpeed;

	internal float moveSpeed;

	internal float verticalVelocity;

	internal float colliderRadius;

	internal float colliderHeight;

	internal float jumpMultiplier = 1f;

	internal float timeToResetJumpMultiplier;

	internal float heightReached;

	internal bool lockMovement;

	internal bool lockRotation;

	internal bool lockSetMoveSpeed;

	internal bool _isStrafing;

	internal bool lockInStrafe;

	internal bool forceRootMotion;

	internal bool keepDirection;

	internal bool finishStaminaOnSprint;

	[HideInInspector]
	public bool applyingStepOffset;

	protected internal bool lockAnimMovement;

	protected internal bool lockAnimRotation;

	protected Vector3 lastCharacterAngle;

	internal Transform rotateTarget;

	internal Vector3 input;

	internal Vector3 oldInput;

	internal Vector3 colliderCenter;

	[HideInInspector]
	public Vector3 inputSmooth;

	[HideInInspector]
	public Vector3 moveDirection;

	public RaycastHit stepOffsetHit;

	public RaycastHit slopeHitInfo;

	internal AnimatorStateInfo baseLayerInfo;

	internal AnimatorStateInfo underBodyInfo;

	internal AnimatorStateInfo rightArmInfo;

	internal AnimatorStateInfo leftArmInfo;

	internal AnimatorStateInfo fullBodyInfo;

	internal AnimatorStateInfo upperBodyInfo;

	internal bool blockApplyFallDamage;

	public vAnimatorStateInfos animatorStateInfos
	{
		get
		{
			return _animatorStateInfos;
		}
		protected set
		{
			_animatorStateInfos = value;
		}
	}

	public bool isStrafing
	{
		get
		{
			if (!sprintOnlyFree || !isSprinting)
			{
				return _isStrafing;
			}
			return false;
		}
		set
		{
			_isStrafing = value;
		}
	}

	public bool isGrounded { get; set; }

	public bool disableCheckGround { get; set; }

	public bool inCrouchArea { get; protected set; }

	public bool isSprinting { get; set; }

	public bool isSliding { get; protected set; }

	public bool autoCrouch { get; protected set; }

	public PhysicMaterial currentMaterialPhysics { get; protected set; }

	public int baseLayer => base.animator.GetLayerIndex("Base Layer");

	public int underBodyLayer => base.animator.GetLayerIndex("UnderBody");

	public int rightArmLayer => base.animator.GetLayerIndex("RightArm");

	public int leftArmLayer => base.animator.GetLayerIndex("LeftArm");

	public int upperBodyLayer => base.animator.GetLayerIndex("UpperBody");

	public int fullbodyLayer => base.animator.GetLayerIndex("FullBody");

	public float colliderRadiusDefault { get; protected set; }

	public float colliderHeightDefault { get; protected set; }

	public Vector3 colliderCenterDefault { get; protected set; }

	protected virtual bool _canApplyFallDamage
	{
		get
		{
			if (!blockApplyFallDamage && jumpMultiplier <= 1f)
			{
				return !customAction;
			}
			return false;
		}
	}

	public bool alwaysWalkByDefault { get; set; }

	public bool hasMovementInput
	{
		get
		{
			if (!(inputSmooth.sqrMagnitude + input.sqrMagnitude > 0.1f))
			{
				return (input - inputSmooth).sqrMagnitude > 0.1f;
			}
			return true;
		}
	}

	protected virtual bool jumpFwdCondition
	{
		get
		{
			Vector3 vector = base.transform.position + _capsuleCollider.center + Vector3.up * (0f - _capsuleCollider.height) * 0.5f;
			Vector3 point = vector + Vector3.up * _capsuleCollider.height;
			return Physics.CapsuleCastAll(vector, point, _capsuleCollider.radius * 0.5f, base.transform.forward, 0.6f, groundLayer).Length == 0;
		}
	}

	internal bool canRollAgain
	{
		get
		{
			if (isRolling)
			{
				return animatorStateInfos.GetCurrentNormalizedTime(0) >= timeToRollAgain;
			}
			return false;
		}
	}

	protected void RemoveComponents()
	{
		if (removeComponentsAfterDie)
		{
			if (_capsuleCollider != null)
			{
				UnityEngine.Object.Destroy(_capsuleCollider);
			}
			if (_rigidbody != null)
			{
				UnityEngine.Object.Destroy(_rigidbody);
			}
			if (base.animator != null)
			{
				UnityEngine.Object.Destroy(base.animator);
			}
			MonoBehaviour[] components = GetComponents<MonoBehaviour>();
			for (int i = 0; i < components.Length; i++)
			{
				UnityEngine.Object.Destroy(components[i]);
			}
		}
	}

	private void Awake()
	{
		SetCustomFixedTimeStep();
	}

	protected override void Start()
	{
		base.Start();
		heightReached = base.transform.position.y;
	}

	public override void Init()
	{
		base.Init();
		base.animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
		frictionPhysics = new PhysicMaterial();
		frictionPhysics.name = "frictionPhysics";
		frictionPhysics.staticFriction = 0.25f;
		frictionPhysics.dynamicFriction = 0.25f;
		frictionPhysics.frictionCombine = PhysicMaterialCombine.Multiply;
		maxFrictionPhysics = new PhysicMaterial();
		maxFrictionPhysics.name = "maxFrictionPhysics";
		maxFrictionPhysics.staticFriction = 1f;
		maxFrictionPhysics.dynamicFriction = 1f;
		maxFrictionPhysics.frictionCombine = PhysicMaterialCombine.Maximum;
		slippyPhysics = new PhysicMaterial();
		slippyPhysics.name = "slippyPhysics";
		slippyPhysics.staticFriction = 0f;
		slippyPhysics.dynamicFriction = 0f;
		slippyPhysics.frictionCombine = PhysicMaterialCombine.Minimum;
		_rigidbody = GetComponent<Rigidbody>();
		_capsuleCollider = GetComponent<CapsuleCollider>();
		Vector3 vector = (colliderCenterDefault = _capsuleCollider.center);
		colliderCenter = vector;
		float num = (colliderRadiusDefault = _capsuleCollider.radius);
		colliderRadius = num;
		num = (colliderHeightDefault = _capsuleCollider.height);
		colliderHeight = num;
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Physics.IgnoreCollision(_capsuleCollider, componentsInChildren[i]);
		}
		if (fillHealthOnStart)
		{
			base.currentHealth = maxHealth;
		}
		currentHealthRecoveryDelay = healthRecoveryDelay;
		currentStamina = maxStamina;
		ResetJumpMultiplier();
		isGrounded = true;
		ResetControllerSpeedMultiplier();
	}

	public virtual void SetCustomFixedTimeStep()
	{
		switch (customFixedTimeStep)
		{
		case CustomFixedTimeStep.FPS30:
			Time.fixedDeltaTime = 0.03333334f;
			break;
		case CustomFixedTimeStep.FPS60:
			Time.fixedDeltaTime = 0.01666667f;
			break;
		case CustomFixedTimeStep.FPS75:
			Time.fixedDeltaTime = 0.01333333f;
			break;
		case CustomFixedTimeStep.FPS90:
			Time.fixedDeltaTime = 0.01111111f;
			break;
		case CustomFixedTimeStep.FPS120:
			Time.fixedDeltaTime = 1f / 120f;
			break;
		case CustomFixedTimeStep.FPS144:
			Time.fixedDeltaTime = 0.006944444f;
			break;
		case CustomFixedTimeStep.Default:
			break;
		}
	}

	public virtual void UpdateMotor()
	{
		CheckHealth();
		CheckStamina();
		CheckGround();
		SlideMovementBehavior();
		CheckRagdoll();
		ControlCapsuleHeight();
		ControlJumpBehaviour();
		AirControl();
		StaminaRecovery();
		CalculateRotationMagnitude();
	}

	public override void TakeDamage(vDamage damage)
	{
		if (base.currentHealth <= 0f || IgnoreDamageRolling())
		{
			if (damage.activeRagdoll && !IgnoreDamageActiveRagdollRolling())
			{
				base.onActiveRagdoll.Invoke(damage);
			}
			return;
		}
		if (damage.activeRagdoll && IgnoreDamageActiveRagdollRolling())
		{
			damage.activeRagdoll = false;
		}
		base.TakeDamage(damage);
	}

	protected virtual bool IgnoreDamageRolling()
	{
		if (noDamageWhileRolling)
		{
			return isRolling;
		}
		return false;
	}

	protected virtual bool IgnoreDamageActiveRagdollRolling()
	{
		if (noActiveRagdollWhileRolling)
		{
			return isRolling;
		}
		return false;
	}

	protected override void TriggerDamageReaction(vDamage damage)
	{
		if (!customAction)
		{
			base.TriggerDamageReaction(damage);
		}
		else if (damage.activeRagdoll)
		{
			base.onActiveRagdoll.Invoke(damage);
		}
	}

	public virtual void ReduceStamina(float value, bool accumulative)
	{
		if (!customAction)
		{
			if (accumulative)
			{
				currentStamina -= value * Time.fixedDeltaTime;
			}
			else
			{
				currentStamina -= value;
			}
			if (currentStamina < 0f)
			{
				currentStamina = 0f;
				OnStaminaEnd.Invoke();
			}
		}
	}

	public virtual void ChangeStamina(int value)
	{
		currentStamina += value;
		currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
	}

	public virtual void ChangeMaxStamina(int value)
	{
		maxStamina += value;
		if (maxStamina < 0f)
		{
			maxStamina = 0f;
		}
	}

	public virtual void DeathBehaviour()
	{
		lockAnimMovement = true;
		base.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		if (deathBy == DeathBy.Animation || deathBy == DeathBy.AnimationWithRagdoll)
		{
			base.animator.SetBool("isDead", value: true);
		}
	}

	private void CheckHealth()
	{
		if (base.isDead && base.currentHealth > 0f)
		{
			base.isDead = false;
		}
	}

	private void CheckStamina()
	{
		if (isSprinting && !Energized)
		{
			currentStaminaRecoveryDelay = 0.25f;
			ReduceStamina(sprintStamina, accumulative: true);
		}
	}

	public void StaminaRecovery()
	{
		if (currentStaminaRecoveryDelay > 0f)
		{
			currentStaminaRecoveryDelay -= Time.fixedDeltaTime;
			return;
		}
		if (currentStamina > maxStamina)
		{
			currentStamina = maxStamina;
		}
		if (currentStamina < maxStamina)
		{
			currentStamina += staminaRecovery;
		}
	}

	protected virtual void CalculateRotationMagnitude()
	{
		Vector3 eulerAngle = base.transform.eulerAngles - lastCharacterAngle;
		if ((double)eulerAngle.sqrMagnitude < 0.01)
		{
			lastCharacterAngle = base.transform.eulerAngles;
			rotationMagnitude = 0f;
		}
		else
		{
			float num = eulerAngle.NormalizeAngle().y / (isStrafing ? strafeSpeed.rotationSpeed : freeSpeed.rotationSpeed);
			rotationMagnitude = (float)Math.Round(num, 2);
			lastCharacterAngle = base.transform.eulerAngles;
		}
	}

	public virtual void SetControllerSpeedMultiplier(float speed)
	{
		speedMultiplier = speed;
	}

	public virtual void ResetControllerSpeedMultiplier()
	{
		speedMultiplier = defaultSpeedMultiplier;
	}

	public virtual void SetControllerMoveSpeed(vMovementSpeed speed)
	{
		if (isCrouching)
		{
			moveSpeed = Mathf.Lerp(moveSpeed, speed.crouchSpeed, speed.movementSmooth * Time.fixedDeltaTime);
		}
		else if (speed.walkByDefault || alwaysWalkByDefault)
		{
			moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? speed.runningSpeed : speed.walkSpeed, speed.movementSmooth * Time.fixedDeltaTime);
		}
		else
		{
			moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? speed.sprintSpeed : speed.runningSpeed, speed.movementSmooth * Time.fixedDeltaTime);
		}
	}

	public virtual void MoveCharacter(Vector3 _direction)
	{
		inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * (useRootMotion ? vTime.deltaTime : vTime.fixedDeltaTime));
		if (!isSliding && !base.ragdolled && isGrounded && !isJumping)
		{
			_direction.y = 0f;
			_direction = _direction.normalized * Mathf.Clamp(_direction.magnitude, 0f, 1f);
			Vector3 targetVelocity = ((useRootMotion ? base.animator.rootPosition : _rigidbody.position) + _direction * (moveSpeed * speedMultiplier) * (useRootMotion ? vTime.deltaTime : vTime.fixedDeltaTime) - base.transform.position) / (useRootMotion ? vTime.deltaTime : vTime.fixedDeltaTime);
			bool useVerticalVelocity = true;
			SnapToGround(ref targetVelocity, ref useVerticalVelocity);
			steepSlopeAhead = CheckForSlope(ref targetVelocity);
			if (!steepSlopeAhead)
			{
				CalculateStepOffset(_direction.normalized, ref targetVelocity, ref useVerticalVelocity);
			}
			CheckStopMove(ref targetVelocity);
			if (useVerticalVelocity)
			{
				targetVelocity.y = _rigidbody.velocity.y;
			}
			_rigidbody.velocity = targetVelocity;
		}
	}

	protected virtual void CheckStopMove(ref Vector3 targetVelocity)
	{
		Vector3 start = base.transform.position + base.transform.up * colliderRadiusDefault;
		Vector3 normalized = moveDirection.normalized;
		normalized = Vector3.ProjectOnPlane(normalized, groundHit.normal);
		_ = colliderRadiusDefault;
		float num = 0f;
		float num2 = (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth);
		bool flag = isGrounded && !isJumping && !isInAirborne && !applyingStepOffset && !customAction;
		RaycastHit hit;
		if (steepSlopeAhead)
		{
			num = 1f * _slopeSidewaysSmooth;
		}
		else if (flag && CheckStopMove(normalized, out hit))
		{
			if (Vector3.Angle(normalized, -hit.normal) < slopeLimit)
			{
				float num3 = hit.distance - colliderRadiusDefault;
				num = 1f - num3;
			}
			else
			{
				num = -0.01f;
			}
			if (debugWindow)
			{
				Debug.DrawLine(start, hit.point, Color.cyan);
			}
		}
		else
		{
			num = -0.01f;
		}
		stopMoveWeight = Mathf.Lerp(stopMoveWeight, num, num2 * Time.deltaTime);
		stopMoveWeight = Mathf.Clamp(stopMoveWeight, 0f, 1f);
		targetVelocity = Vector3.LerpUnclamped(targetVelocity, Vector3.zero, stopMoveWeight);
	}

	protected virtual bool CheckStopMove(Vector3 direction, out RaycastHit hit)
	{
		Vector3 vector = base.transform.position + base.transform.up * colliderRadiusDefault;
		float maxDistance = colliderRadiusDefault + stopMoveRayDistance;
		StopMoveCheckMethod stopMoveCheckMethod = this.stopMoveCheckMethod;
		if ((uint)(stopMoveCheckMethod - 1) <= 1u)
		{
			Vector3 point = vector + base.transform.up * slopeLimitHeight;
			Vector3 point2 = vector + base.transform.up * (stopMoveMaxHeight - _capsuleCollider.radius);
			return Physics.CapsuleCast(point, point2, _capsuleCollider.radius, direction, out hit, maxDistance, stopMoveLayer);
		}
		return Physics.Raycast(vector, direction, out hit, maxDistance, stopMoveLayer);
	}

	protected virtual void SnapToGround(ref Vector3 targetVelocity, ref bool useVerticalVelocity)
	{
		if (useSnapGround && (disableCheckGround || isRolling) && !(groundDistance < groundMinDistance * 0.2f) && !applyingStepOffset && isGrounded && groundHit.collider != null && GroundAngle() <= slopeLimit && !disableCheckGround && !isSliding && !isJumping && !customAction && input.magnitude > 0.1f && !isInAirborne)
		{
			float num = Mathf.Max(0f, groundDistance - groundMinDistance);
			Vector3 vector = base.transform.up * ((0f - num) * snapPower / Time.fixedDeltaTime);
			targetVelocity = (targetVelocity + vector).normalized * targetVelocity.magnitude;
			useVerticalVelocity = false;
		}
	}

	private void CalculateStepOffset(Vector3 moveDir, ref Vector3 targetVelocity, ref bool useVerticalVelocity)
	{
		if (useStepOffset && isGrounded && !disableCheckGround && !isSliding && !isJumping && !customAction && !isInAirborne)
		{
			Vector3 vector = Vector3.Lerp(base.transform.forward, moveDir.normalized, inputSmooth.magnitude);
			float num = _capsuleCollider.radius + stepOffsetDistance;
			float num2 = stepOffsetMaxHeight + 0.01f + _capsuleCollider.radius * 0.5f;
			Vector3 vector2 = base.transform.position + base.transform.up * (stepOffsetMinHeight + 0.05f);
			Vector3 end = vector2 + vector.normalized * num;
			if (Physics.Linecast(vector2, end, out stepOffsetHit, groundLayer))
			{
				if (debugWindow)
				{
					Debug.DrawLine(vector2, stepOffsetHit.point);
				}
				num = stepOffsetHit.distance + 0.1f;
			}
			if (Physics.SphereCast(new Ray(base.transform.position + base.transform.up * num2 + vector.normalized * num, Vector3.down), _capsuleCollider.radius * 0.5f, out stepOffsetHit, stepOffsetMaxHeight - stepOffsetMinHeight, groundLayer) && stepOffsetHit.point.y > base.transform.position.y)
			{
				vector = stepOffsetHit.point - base.transform.position;
				vector.Normalize();
				targetVelocity = Vector3.Project(targetVelocity, vector);
				applyingStepOffset = true;
				useVerticalVelocity = false;
				return;
			}
		}
		applyingStepOffset = false;
	}

	public virtual void StopCharacterWithLerp()
	{
		sprintWeight = 0f;
		horizontalSpeed = 0f;
		verticalSpeed = 0f;
		moveDirection = Vector3.zero;
		input = Vector3.Lerp(input, Vector3.zero, 2f * Time.fixedDeltaTime);
		inputSmooth = Vector3.Lerp(inputSmooth, Vector3.zero, 2f * Time.fixedDeltaTime);
		_rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, Vector3.zero, 4f * Time.fixedDeltaTime);
		inputMagnitude = Mathf.Lerp(inputMagnitude, 0f, 2f * Time.fixedDeltaTime);
		moveSpeed = Mathf.Lerp(moveSpeed, 0f, 2f * Time.fixedDeltaTime);
		base.animator.SetFloat(vAnimatorParameters.InputMagnitude, 0f, 0.2f, Time.fixedDeltaTime);
		base.animator.SetFloat(vAnimatorParameters.InputVertical, 0f, 0.2f, Time.fixedDeltaTime);
		base.animator.SetFloat(vAnimatorParameters.InputHorizontal, 0f, 0.2f, Time.fixedDeltaTime);
		base.animator.SetFloat(vAnimatorParameters.RotationMagnitude, 0f, 0.2f, Time.fixedDeltaTime);
	}

	public virtual void StopCharacter()
	{
		sprintWeight = 0f;
		horizontalSpeed = 0f;
		verticalSpeed = 0f;
		moveDirection = Vector3.zero;
		input = Vector3.zero;
		inputSmooth = Vector3.zero;
		_rigidbody.velocity = Vector3.zero;
		inputMagnitude = 0f;
		moveSpeed = 0f;
		base.animator.SetFloat(vAnimatorParameters.InputMagnitude, 0f);
		base.animator.SetFloat(vAnimatorParameters.InputVertical, 0f);
		base.animator.SetFloat(vAnimatorParameters.InputHorizontal, 0f);
		base.animator.SetFloat(vAnimatorParameters.RotationMagnitude, 0f);
	}

	public virtual void RotateToPosition(Vector3 position)
	{
		RotateToDirection((position - base.transform.position).normalized);
	}

	public virtual void RotateToDirection(Vector3 direction)
	{
		RotateToDirection(direction, isStrafing ? strafeSpeed.rotationSpeed : freeSpeed.rotationSpeed);
	}

	public virtual void RotateToDirection(Vector3 direction, float rotationSpeed)
	{
		if (!lockAnimRotation && !customAction && (jumpAndRotate || isGrounded) && !base.ragdolled && !isSliding)
		{
			direction.y = 0f;
			if (direction.normalized.magnitude == 0f)
			{
				direction = base.transform.forward;
			}
			Vector3 euler = base.transform.rotation.eulerAngles.NormalizeAngle();
			Vector3 vector = Quaternion.LookRotation(direction.normalized).eulerAngles.NormalizeAngle();
			euler.y = Mathf.LerpAngle(euler.y, vector.y, rotationSpeed * Time.fixedDeltaTime);
			Quaternion rotation = Quaternion.Euler(euler);
			base.transform.rotation = rotation;
		}
	}

	protected virtual void ControlJumpBehaviour()
	{
		if (isJumping)
		{
			jumpCounter -= Time.fixedDeltaTime;
			if (jumpCounter <= 0f)
			{
				jumpCounter = 0f;
				isJumping = false;
			}
			Vector3 velocity = _rigidbody.velocity;
			velocity.y = jumpHeight * jumpMultiplier;
			_rigidbody.velocity = velocity;
		}
	}

	public virtual void SetJumpMultiplier(float jumpMultiplier, float timeToReset = 1f)
	{
		this.jumpMultiplier = jumpMultiplier;
		if (timeToResetJumpMultiplier <= 0f)
		{
			timeToResetJumpMultiplier = timeToReset;
			StartCoroutine(ResetJumpMultiplierRoutine());
		}
		else
		{
			timeToResetJumpMultiplier = timeToReset;
		}
	}

	public virtual void ResetJumpMultiplier()
	{
		StopCoroutine("ResetJumpMultiplierRoutine");
		timeToResetJumpMultiplier = 0f;
		jumpMultiplier = 1f;
	}

	protected IEnumerator ResetJumpMultiplierRoutine()
	{
		while (timeToResetJumpMultiplier > 0f && jumpMultiplier != 1f)
		{
			timeToResetJumpMultiplier -= Time.fixedDeltaTime;
			yield return null;
		}
		jumpMultiplier = 1f;
	}

	public virtual void AirControl()
	{
		if ((!isGrounded || isJumping) && !isSliding)
		{
			if (base.transform.position.y > heightReached)
			{
				heightReached = base.transform.position.y;
			}
			inputSmooth = Vector3.Lerp(inputSmooth, input, airSmooth * Time.fixedDeltaTime);
			if (jumpWithRigidbodyForce && !isGrounded)
			{
				_rigidbody.AddForce(moveDirection * airSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
				return;
			}
			moveDirection.y = 0f;
			moveDirection.x = Mathf.Clamp(moveDirection.x, -1f, 1f);
			moveDirection.z = Mathf.Clamp(moveDirection.z, -1f, 1f);
			Vector3 b = (_rigidbody.position + moveDirection * airSpeed * Time.fixedDeltaTime - base.transform.position) / Time.fixedDeltaTime;
			b.y = _rigidbody.velocity.y;
			_rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, b, airSmooth * Time.fixedDeltaTime);
		}
	}

	public virtual void UseAutoCrouch(bool value)
	{
		autoCrouch = value;
	}

	public virtual void AutoCrouch()
	{
		if (autoCrouch)
		{
			isCrouching = true;
		}
		if (autoCrouch && !inCrouchArea && CanExitCrouch())
		{
			autoCrouch = false;
			isCrouching = false;
		}
	}

	public virtual bool CanExitCrouch()
	{
		if (isCrouching)
		{
			float radius = _capsuleCollider.radius * 0.9f;
			Vector3 origin = base.transform.position + Vector3.up * (colliderHeight * 0.5f - colliderRadius);
			if (Physics.SphereCast(new Ray(origin, Vector3.up), radius, out groundHit, crouchHeadDetect - colliderRadius * 0.1f, autoCrouchLayer))
			{
				return false;
			}
			return true;
		}
		return true;
	}

	protected virtual void AutoCrouchExit(Collider other)
	{
		if (other.CompareTag("AutoCrouch"))
		{
			inCrouchArea = false;
		}
	}

	protected virtual void CheckForAutoCrouch(Collider other)
	{
		if (other.gameObject.CompareTag("AutoCrouch"))
		{
			autoCrouch = true;
			inCrouchArea = true;
		}
	}

	protected virtual void RollBehavior()
	{
		if (isRolling)
		{
			if (rollControl)
			{
				inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
			}
			RotateToDirection(moveDirection, rollRotationSpeed);
			Vector3 velocity = (useRollRootMotion ? new Vector3(base.animator.deltaPosition.x, 0f, base.animator.deltaPosition.z) : (base.transform.forward * Time.deltaTime)) * ((rollSpeed > 0f) ? rollSpeed : 1f) / Time.deltaTime * (1f - stopMoveWeight);
			if (rollUseGravity && base.animator.GetNormalizedTime(baseLayer) >= rollUseGravityTime)
			{
				velocity.y = _rigidbody.velocity.y;
			}
			_rigidbody.velocity = velocity;
		}
	}

	protected virtual void CheckGround()
	{
		CheckGroundDistance();
		SlideOnSteepSlope();
		ControlMaterialPhysics();
		if (base.isDead || customAction || disableCheckGround || isSliding)
		{
			isGrounded = true;
			heightReached = base.transform.position.y;
		}
		else if (groundDistance <= groundMinDistance || applyingStepOffset)
		{
			CheckFallDamage();
			isGrounded = true;
			if (!useSnapGround && !applyingStepOffset && !isJumping && groundDistance > 0.05f && extraGravity != 0f)
			{
				_rigidbody.AddForce(base.transform.up * (extraGravity * 2f * Time.fixedDeltaTime), ForceMode.VelocityChange);
			}
			heightReached = base.transform.position.y;
		}
		else if (groundDistance >= groundMaxDistance)
		{
			isGrounded = false;
			verticalVelocity = _rigidbody.velocity.y;
			if (!applyingStepOffset && !isJumping && extraGravity != 0f)
			{
				_rigidbody.AddForce(base.transform.up * extraGravity * Time.fixedDeltaTime, ForceMode.VelocityChange);
			}
		}
		else if (!applyingStepOffset && !isJumping && extraGravity != 0f)
		{
			_rigidbody.AddForce(base.transform.up * (extraGravity * 2f * Time.fixedDeltaTime), ForceMode.VelocityChange);
		}
	}

	protected virtual void CheckFallDamage()
	{
		if (!isGrounded && !(verticalVelocity > fallMinVerticalVelocity) && _canApplyFallDamage && fallMinHeight != 0f && fallDamage != 0f)
		{
			float num = heightReached - base.transform.position.y;
			num -= fallMinHeight;
			if (num > 0f)
			{
				int value = (int)(fallDamage * num);
				TakeDamage(new vDamage(value, ignoreReaction: true));
			}
		}
	}

	private void ControlMaterialPhysics()
	{
		PhysicMaterial physicMaterial = currentMaterialPhysics;
		if (isGrounded && input.magnitude < 0.1f && !isSliding && physicMaterial != maxFrictionPhysics)
		{
			physicMaterial = maxFrictionPhysics;
		}
		else if (isGrounded && input.magnitude > 0.1f && !isSliding && physicMaterial != frictionPhysics)
		{
			physicMaterial = frictionPhysics;
		}
		else if (physicMaterial != slippyPhysics && (isSliding || !isGrounded))
		{
			physicMaterial = slippyPhysics;
		}
		if (currentMaterialPhysics != physicMaterial)
		{
			_capsuleCollider.material = physicMaterial;
			currentMaterialPhysics = physicMaterial;
		}
	}

	protected virtual void CheckGroundDistance()
	{
		if (base.isDead || !(_capsuleCollider != null))
		{
			return;
		}
		float radius = _capsuleCollider.radius * 0.9f;
		float num = groundDetectionDistance;
		if (Physics.Raycast(new Ray(base.transform.position + new Vector3(0f, colliderHeight / 2f, 0f), Vector3.down), out groundHit, colliderHeight / 2f + num, groundLayer) && !groundHit.collider.isTrigger)
		{
			num = base.transform.position.y - groundHit.point.y;
		}
		if (groundCheckMethod == GroundCheckMethod.High && num >= groundMinDistance)
		{
			Vector3 origin = base.transform.position + Vector3.up * _capsuleCollider.radius;
			if (Physics.SphereCast(new Ray(origin, -Vector3.up), radius, out groundHit, _capsuleCollider.radius + groundMaxDistance, groundLayer) && !groundHit.collider.isTrigger)
			{
				Physics.Linecast(groundHit.point + Vector3.up * 0.1f, groundHit.point + Vector3.down * 0.15f, out groundHit, groundLayer);
				float num2 = base.transform.position.y - groundHit.point.y;
				if (num > num2)
				{
					num = num2;
				}
			}
		}
		groundDistance = (float)Math.Round(num, 2);
	}

	public virtual float GroundAngle()
	{
		return Vector3.Angle(groundHit.normal, Vector3.up);
	}

	public virtual float GroundAngleFromDirection()
	{
		return Vector3.Angle((isStrafing && input.magnitude > 0f) ? (base.transform.right * input.x + base.transform.forward * input.z).normalized : base.transform.forward, groundHit.normal) - 90f;
	}

	protected virtual void AlignWithSurface()
	{
		Ray ray = new Ray(base.transform.position, -base.transform.up);
		Quaternion b = base.transform.rotation;
		if (Physics.Raycast(ray, out var hitInfo, 1.5f, groundLayer))
		{
			b = Quaternion.FromToRotation(base.transform.up, hitInfo.normal) * base.transform.localRotation;
		}
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, 10f * Time.fixedDeltaTime);
	}

	protected bool CheckForSlope(ref Vector3 targetVelocity)
	{
		if (debugWindow)
		{
			Debug.DrawLine(base.transform.position + Vector3.up * (_capsuleCollider.height * slopeLimitHeight), base.transform.position + moveDirection.normalized * (steepSlopeAhead ? (_capsuleCollider.radius + slopeMaxDistance) : (_capsuleCollider.radius + slopeMinDistance)), Color.red, 0.01f);
		}
		if (!useSlopeLimit || moveDirection.magnitude == 0f || targetVelocity.magnitude == 0f)
		{
			_slopeSidewaysSmooth = 1f;
			return false;
		}
		if (Physics.Linecast(base.transform.position + Vector3.up * (_capsuleCollider.height * slopeLimitHeight), base.transform.position + moveDirection.normalized * (steepSlopeAhead ? (_capsuleCollider.radius + slopeMaxDistance) : (_capsuleCollider.radius + slopeMinDistance)), out slopeHitInfo, groundLayer))
		{
			float num = Vector3.Angle(Vector3.up, slopeHitInfo.normal);
			if (num > slopeLimit && num < 85f)
			{
				Vector3 normal = slopeHitInfo.normal;
				normal.y = 0f;
				Vector3 vector = targetVelocity.normalized.AngleFormOtherDirection(-normal.normalized);
				Vector3 a = Quaternion.AngleAxis((vector.y > 0f) ? 90f : (-90f), Vector3.up) * normal.normalized * targetVelocity.magnitude;
				if (Mathf.Abs(vector.y) > stopSlopeMargin)
				{
					_slopeSidewaysSmooth = Mathf.Clamp(_slopeSidewaysSmooth - Time.deltaTime * slopeSidewaysSmooth, 0f, 1f);
				}
				else
				{
					_slopeSidewaysSmooth = 1f;
				}
				targetVelocity = Vector3.Lerp(a, Vector3.zero, _slopeSidewaysSmooth);
				return true;
			}
		}
		_slopeSidewaysSmooth = 1f;
		return false;
	}

	protected virtual void SlideOnSteepSlope()
	{
		if (useSlide && isGrounded && GroundAngle() > slopeLimit && !disableCheckGround)
		{
			if (_slidingEnterTime <= 0f || isSliding)
			{
				Vector3 normal = groundHit.normal;
				normal.y = 0f;
				Vector3 normalized = Vector3.ProjectOnPlane(normal.normalized, groundHit.normal).normalized;
				if (!Physics.Raycast(base.transform.position + Vector3.up * groundMinDistance, normalized, groundMaxDistance, groundLayer))
				{
					isSliding = true;
				}
			}
			else
			{
				_slidingEnterTime -= Time.fixedDeltaTime;
			}
		}
		else
		{
			_rotateSlopeEnterTime = rotateSlopeEnterTime;
			_slidingEnterTime = (isGrounded ? slidingEnterTime : 0f);
			isSliding = false;
		}
	}

	protected virtual void SlideMovementBehavior()
	{
		if (!isSliding)
		{
			return;
		}
		Vector3 normal = groundHit.normal;
		normal.y = 0f;
		Vector3 normalized = Vector3.ProjectOnPlane(normal.normalized, groundHit.normal).normalized;
		if (debugWindow)
		{
			Debug.DrawRay(base.transform.position, normalized * slideDownVelocity);
		}
		_rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, normalized * slideDownVelocity, slideDownSmooth * Time.fixedDeltaTime);
		normalized.y = 0f;
		if (_rotateSlopeEnterTime <= 0f)
		{
			Quaternion rot = Quaternion.LookRotation(Vector3.RotateTowards(base.transform.forward, normalized, rotateDownSlopeSmooth * Time.fixedDeltaTime, 0f));
			_rigidbody.MoveRotation(rot);
			Vector3 direction = base.transform.InverseTransformDirection(moveDirection);
			direction.y = 0f;
			direction.z = 0f;
			direction = base.transform.TransformDirection(direction);
			if (debugWindow)
			{
				Debug.DrawRay(base.transform.position, direction * slideSidewaysVelocity, Color.blue);
			}
			_rigidbody.AddForce(direction * slideSidewaysVelocity, ForceMode.VelocityChange);
			if (debugWindow)
			{
				Debug.DrawRay(base.transform.position, Vector3.ProjectOnPlane(normal.normalized, groundHit.normal).normalized, Color.blue);
				Debug.DrawRay(base.transform.position, Quaternion.AngleAxis(90f, groundHit.normal) * Vector3.ProjectOnPlane(normal.normalized, groundHit.normal).normalized, Color.red);
				Debug.DrawRay(base.transform.position, base.transform.TransformDirection(direction.normalized * 2f), Color.green);
			}
		}
		else
		{
			_rotateSlopeEnterTime -= Time.fixedDeltaTime;
		}
	}

	public virtual void ControlCapsuleHeight()
	{
		if (isCrouching || isRolling)
		{
			_capsuleCollider.center = colliderCenter / crouchHeightReduction;
			_capsuleCollider.height = colliderHeight / crouchHeightReduction;
			_capsuleCollider.radius = colliderRadius;
		}
		else
		{
			_capsuleCollider.center = colliderCenter;
			_capsuleCollider.radius = colliderRadius;
			_capsuleCollider.height = colliderHeight;
		}
	}

	public void ResetCapsule()
	{
		colliderCenter = colliderCenterDefault;
		colliderRadius = colliderRadiusDefault;
		colliderHeight = colliderHeightDefault;
	}

	public virtual void DisableGravityAndCollision()
	{
		base.animator.SetFloat("InputHorizontal", 0f);
		base.animator.SetFloat("InputVertical", 0f);
		base.animator.SetFloat("VerticalVelocity", 0f);
		_rigidbody.useGravity = false;
		_rigidbody.isKinematic = true;
		_capsuleCollider.isTrigger = true;
		_rigidbody.velocity = Vector3.zero;
	}

	public virtual void EnableGravityAndCollision()
	{
		_capsuleCollider.isTrigger = false;
		_rigidbody.useGravity = true;
		_rigidbody.isKinematic = false;
	}

	protected virtual void CheckRagdoll()
	{
		if (ragdollVelocity != 0f && verticalVelocity <= ragdollVelocity && groundDistance <= 0.1f && _canApplyFallDamage && !base.ragdolled)
		{
			base.onActiveRagdoll.Invoke(null);
		}
	}

	public override void ResetRagdoll()
	{
		onDisableRagdoll.Invoke();
		verticalVelocity = 0f;
		base.ragdolled = false;
		_rigidbody.WakeUp();
		_rigidbody.useGravity = true;
		_rigidbody.isKinematic = false;
		_capsuleCollider.isTrigger = false;
		_capsuleCollider.enabled = true;
	}

	public override void EnableRagdoll()
	{
		StopCharacter();
		base.animator.SetFloat("InputHorizontal", 0f);
		base.animator.SetFloat("InputVertical", 0f);
		base.animator.SetFloat("InputMagnitude", 0f);
		base.animator.SetFloat("VerticalVelocity", 0f);
		base.ragdolled = true;
		_capsuleCollider.isTrigger = true;
		_rigidbody.useGravity = false;
		_rigidbody.isKinematic = true;
		lockAnimMovement = true;
	}

	public virtual string DebugInfo(string additionalText = "")
	{
		string result = string.Empty;
		if (debugWindow)
		{
			float smoothDeltaTime = Time.smoothDeltaTime;
			float num = 1f / smoothDeltaTime;
			result = " \nFPS " + num.ToString("#,##0 fps") + "\nHealth = " + base.currentHealth + "\nInput Vertical = " + inputSmooth.z.ToString("0.0") + "\nInput Horizontal = " + inputSmooth.x.ToString("0.0") + "\nInput Magnitude = " + inputMagnitude.ToString("0.0") + "\nRotation Magnitude = " + rotationMagnitude.ToString("0.0") + "\nVertical Velocity = " + verticalVelocity.ToString("0.00") + "\nCurrent MoveSpeed = " + moveSpeed.ToString("0.00") + "\nGround Distance = " + groundDistance.ToString("0.00") + "\nGround Angle = " + GroundAngleFromDirection().ToString("0.00") + "\nIs Grounded = " + BoolToRichText(isGrounded) + "\nIs Strafing = " + BoolToRichText(isStrafing) + "\nIs Trigger = " + BoolToRichText(_capsuleCollider.isTrigger) + "\nUse Gravity = " + BoolToRichText(_rigidbody.useGravity) + "\nIs Kinematic = " + BoolToRichText(_rigidbody.isKinematic) + "\nLock Movement = " + BoolToRichText(lockMovement) + "\nLock AnimMov = " + BoolToRichText(lockAnimMovement) + "\nLock Rotation = " + BoolToRichText(lockRotation) + "\nLock AnimRot = " + BoolToRichText(lockAnimRotation) + "\n--- Actions Bools ---\nIs Sliding = " + BoolToRichText(isSliding) + "\nIs Sprinting = " + BoolToRichText(isSprinting) + "\nIs Crouching = " + BoolToRichText(isCrouching) + "\nIs Rolling = " + BoolToRichText(isRolling) + "\nIs Jumping = " + BoolToRichText(isJumping) + "\nIs Airborne = " + BoolToRichText(isInAirborne) + "\nIs Ragdolled = " + BoolToRichText(base.ragdolled) + "\nCustomAction = " + BoolToRichText(customAction) + "\n" + additionalText;
		}
		return result;
	}

	protected virtual string BoolToRichText(bool value)
	{
		if (!value)
		{
			return "<color=red> False </color>";
		}
		return "<color=yellow> True </color>";
	}

	protected virtual void OnDrawGizmos()
	{
		if (Application.isPlaying && debugWindow)
		{
			Vector3 origin = base.transform.position + Vector3.up * (colliderHeight * 0.5f - colliderRadius);
			Gizmos.DrawWireSphere(new Ray(origin, Vector3.up).GetPoint(crouchHeadDetect - colliderRadius * 0.1f), colliderRadius * 0.9f);
		}
	}
}
