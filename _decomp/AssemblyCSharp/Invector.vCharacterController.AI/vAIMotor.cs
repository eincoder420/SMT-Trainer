using System;
using System.Collections;
using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI;

[vClassHeader("AI Controller", true, "icon_v2", false, "")]
public abstract class vAIMotor : vCharacter, vIAnimatorStateInfoController
{
	[Serializable]
	public class vMovementSpeed
	{
		[Tooltip("Rotation speed of the character")]
		public float rotationSpeed = 10f;

		[Tooltip("Speed to Walk using rigibody force or extra speed if you're using RootMotion")]
		public float walkSpeed = 2f;

		[Tooltip("Speed to Run using rigibody force or extra speed if you're using RootMotion")]
		public float runningSpeed = 3f;

		[Tooltip("Speed to Sprint using rigibody force or extra speed if you're using RootMotion")]
		public float sprintSpeed = 4f;

		[Tooltip("Speed to Crouch using rigibody force or extra speed if you're using RootMotion")]
		public float crouchSpeed = 2f;
	}

	[vEditorToolbar("Start", false, "", false, false)]
	public bool disableControllerOnStart;

	[vEditorToolbar("Movement", false, "", false, false, order = 1)]
	[SerializeField]
	protected vAIMovementSpeed currentSpeed;

	[Tooltip("Change the velocity of all animations")]
	public float animatorSpeed = 1f;

	[Tooltip("Smooth the  InputMagniture Animator parameter Update")]
	public float inputMagnitudeSmooth = 0.2f;

	[vHelpBox("When checked, make sure to reset the speed values to 1 to use the root motion original speed, increase or decrease this value to have extraSpeed", vHelpBoxAttribute.MessageType.Info)]
	[Tooltip("Turn off if you have 'in place' animations and use this values above to move the character, or use with root motion as extra speed")]
	public bool useRootMotion;

	public vMovementSpeed freeSpeed;

	public vMovementSpeed strafeSpeed;

	[vHelpBox("Check this options only if the Agent needs to walk on complex meshes.", vHelpBoxAttribute.MessageType.Info)]
	[vEditorToolbar("Step Offset", false, "", false, false, order = 2)]
	[SerializeField]
	protected bool _useStepOffSet = true;

	[vHideInInspector("useStepOffSet", false)]
	[Tooltip("ADJUST IN PLAY MODE - Offset height limit for sters - GREY Raycast in front of the legs")]
	[SerializeField]
	protected float stepOffsetEnd = 0.45f;

	[vHideInInspector("useStepOffSet", false)]
	[Tooltip("ADJUST IN PLAY MODE - Offset height origin for sters, make sure to keep slight above the floor - GREY Raycast in front of the legs")]
	[SerializeField]
	protected float stepOffsetStart = 0.05f;

	[vHideInInspector("useStepOffSet", false)]
	[Tooltip("Higher value will result jittering on ramps, lower values will have difficulty on steps")]
	[SerializeField]
	protected float stepSmooth = 4f;

	[vEditorToolbar("Ground & Jump", false, "", false, false, order = 3)]
	[Tooltip("Make sure to bake the navmesh with the jump distance value higher then 0")]
	[SerializeField]
	protected float jumpSpeedPerMeter = 0.15f;

	[SerializeField]
	protected float jumpHeight = 0.75f;

	public float checkGroundDistance = 0.3f;

	[vHelpBox("Make sure to bake the navmesh and use the correct layer on your ground mesh.", vHelpBoxAttribute.MessageType.Info)]
	public LayerMask groundLayer = 1;

	[vEditorToolbar("Auto Crouch", false, "", false, false, order = 4)]
	public bool useAutoCrouch;

	[SerializeField]
	[Range(0f, 1f)]
	protected float headDetectStart = 0.4f;

	[SerializeField]
	protected float headDetectHeight = 0.4f;

	[SerializeField]
	protected float headDetectMargin = 0.02f;

	[vHideInInspector("useAutoCrouch", false)]
	public LayerMask autoCrouchLayer = 1;

	[SerializeField]
	protected bool debugAutoCrouch;

	[vEditorToolbar("Events", false, "", false, false)]
	public UnityEvent onEnableController;

	public UnityEvent onDisableController;

	[HideInInspector]
	public Rigidbody _rigidbody;

	public bool Sitting;

	[HideInInspector]
	public PhysicMaterial frictionPhysics;

	[HideInInspector]
	public PhysicMaterial maxFrictionPhysics;

	[HideInInspector]
	public PhysicMaterial slippyPhysics;

	[HideInInspector]
	public CapsuleCollider _capsuleCollider;

	[HideInInspector]
	public Vector3 targetDirection;

	[HideInInspector]
	public Vector3 input;

	[HideInInspector]
	public bool lockMovement;

	[HideInInspector]
	public bool lockRotation;

	[HideInInspector]
	public bool stopMove;

	[HideInInspector]
	public bool isJumping;

	[HideInInspector]
	public bool doingCustomAction;

	private UnityEvent onUpdateAI = new UnityEvent();

	private bool _isStrafingRef;

	private bool _isGroundedRef;

	private float _verticalVelocityRef;

	private float _groundDistanceRef;

	private bool _isCrouchingRef;

	private bool _isCrouchingFromCast;

	[HideInInspector]
	public bool customAction;

	protected float direction;

	protected float speed;

	protected float velocity;

	protected float strafeMagnitude;

	protected float rotationMagnitude;

	protected float colliderHeight;

	protected float verticalVelocity;

	protected RaycastHit groundHit;

	protected Quaternion freeRotation;

	protected Vector3 colliderCenter;

	protected Vector3 lastCharacterAngle;

	protected Vector3 temporaryDirection;

	protected float temporaryDirectionTime;

	public AnimatorStateInfo baseLayerInfo;

	public AnimatorStateInfo rightArmInfo;

	public AnimatorStateInfo leftArmInfo;

	public AnimatorStateInfo fullBodyInfo;

	public AnimatorStateInfo upperBodyInfo;

	public AnimatorStateInfo underBodyInfo;

	[HideInInspector]
	public bool triggerDieBehaviour;

	[HideInInspector]
	public bool isStrafing { get; protected set; }

	public bool isGrounded { get; protected set; }

	public vAnimatorStateInfos animatorStateInfos { get; protected set; }

	public override bool isCrouching
	{
		get
		{
			if (!_isCrouching)
			{
				return _isCrouchingFromCast;
			}
			return true;
		}
		set
		{
			_isCrouching = value;
		}
	}

	public bool isRolling { get; protected set; }

	public vAIMovementSpeed movementSpeed
	{
		get
		{
			return currentSpeed;
		}
		protected set
		{
			currentSpeed = value;
		}
	}

	public bool useCustomRotationSpeed { get; set; }

	public float customRotationSpeed { get; set; }

	public UnityEvent OnUpdateAI => onUpdateAI;

	public virtual bool actions
	{
		get
		{
			if (!customAction && !isRolling)
			{
				return isJumping;
			}
			return true;
		}
	}

	public virtual int baseLayer => base.animator.GetLayerIndex("Base Layer");

	public virtual int underBodyLayer => base.animator.GetLayerIndex("UnderBody");

	public virtual int rightArmLayer => base.animator.GetLayerIndex("RightArm");

	public virtual int leftArmLayer => base.animator.GetLayerIndex("LeftArm");

	public virtual int upperBodyLayer => base.animator.GetLayerIndex("UpperBody");

	public virtual int fullbodyLayer => base.animator.GetLayerIndex("FullBody");

	protected virtual float rotationSpeed
	{
		get
		{
			if (lockRotation)
			{
				return 0f;
			}
			if (!useCustomRotationSpeed)
			{
				if (!isStrafing)
				{
					return freeSpeed.rotationSpeed;
				}
				return strafeSpeed.rotationSpeed;
			}
			return customRotationSpeed;
		}
	}

	protected virtual bool isSprinting => movementSpeed == vAIMovementSpeed.Sprinting;

	protected virtual float maxSpeed => movementSpeed switch
	{
		vAIMovementSpeed.Idle => 0f, 
		vAIMovementSpeed.Walking => 0.5f, 
		vAIMovementSpeed.Running => 1f, 
		vAIMovementSpeed.Sprinting => 1.5f, 
		_ => 0f, 
	};

	protected virtual bool IsStrafingAnim
	{
		get
		{
			return _isStrafingRef;
		}
		set
		{
			if (_isStrafingRef != value || base.animator.GetBool("IsStrafing") != value)
			{
				_isStrafingRef = value;
				base.animator.SetBool("IsStrafing", value);
			}
		}
	}

	protected virtual bool IsGroundedAnim
	{
		get
		{
			return _isGroundedRef;
		}
		set
		{
			if (_isGroundedRef != value)
			{
				_isGroundedRef = value;
				base.animator.SetBool("IsGrounded", value);
			}
		}
	}

	protected virtual bool IsCrouchingAnim
	{
		get
		{
			return _isCrouchingRef;
		}
		set
		{
			if (_isCrouchingRef != value)
			{
				_isCrouchingRef = value;
				base.animator.SetBool("IsCrouching", value);
			}
		}
	}

	protected virtual float GroundDistanceAnim
	{
		get
		{
			return _groundDistanceRef;
		}
		set
		{
			if (_groundDistanceRef != value)
			{
				_groundDistanceRef = value;
				base.animator.SetFloat("GroundDistance", value);
			}
		}
	}

	protected virtual float VerticalVelocityAnim
	{
		get
		{
			return _verticalVelocityRef;
		}
		set
		{
			if (_verticalVelocityRef != value)
			{
				_verticalVelocityRef = value;
				base.animator.SetFloat("VerticalVelocity", value);
			}
		}
	}

	protected virtual void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			_capsuleCollider = GetComponent<CapsuleCollider>();
			colliderHeight = _capsuleCollider.height;
		}
		if (debugAutoCrouch)
		{
			Color green = Color.green;
			float radius = _capsuleCollider.radius + headDetectMargin;
			Vector3 vector = base.transform.position + Vector3.up * colliderHeight * headDetectStart;
			Ray ray = new Ray(vector, Vector3.up);
			green = (((Application.isPlaying || !Physics.SphereCast(ray, radius, headDetectHeight + _capsuleCollider.radius, autoCrouchLayer)) && !isCrouching) ? Color.green : Color.red);
			green.a = 0.4f;
			Gizmos.color = green;
			Gizmos.DrawWireSphere(vector + Vector3.up * (headDetectHeight + _capsuleCollider.radius), radius);
		}
	}

	protected override void Start()
	{
		base.Start();
		base.animator = GetComponent<Animator>();
		if ((bool)base.animator)
		{
			animatorStateInfos = new vAnimatorStateInfos(base.animator);
			animatorStateInfos.RegisterListener();
			hitDirectionHash = new vAnimatorParameter(base.animator, "HitDirection");
			reactionIDHash = new vAnimatorParameter(base.animator, "ReactionID");
			triggerReactionHash = new vAnimatorParameter(base.animator, "TriggerReaction");
			triggerResetStateHash = new vAnimatorParameter(base.animator, "ResetState");
			recoilIDHash = new vAnimatorParameter(base.animator, "RecoilID");
			triggerRecoilHash = new vAnimatorParameter(base.animator, "TriggerRecoil");
		}
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
		targetDirection = base.transform.forward;
		_rigidbody = GetComponent<Rigidbody>();
		_capsuleCollider = GetComponent<CapsuleCollider>();
		colliderCenter = _capsuleCollider.center;
		colliderHeight = _capsuleCollider.height;
		base.currentHealth = maxHealth;
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Physics.IgnoreCollision(_capsuleCollider, componentsInChildren[i]);
		}
		bool isGroundedAnim = (isGrounded = true);
		IsGroundedAnim = isGroundedAnim;
		targetDirection = base.transform.forward;
		if (disableControllerOnStart)
		{
			DisableAIController();
		}
	}

	protected virtual void OnEnable()
	{
		if (animatorStateInfos != null && (bool)base.animator)
		{
			animatorStateInfos.RegisterListener();
		}
	}

	protected virtual void OnDisable()
	{
		if (animatorStateInfos != null && (bool)base.animator)
		{
			animatorStateInfos.RemoveListener();
		}
	}

	protected void Update()
	{
		UpdateAI();
	}

	protected virtual void UpdateAI()
	{
		HealthControl();
		UpdateLocomotion();
		UpdateAnimator();
		onUpdateAI.Invoke();
	}

	protected virtual void SetMovementInput(Vector3 input)
	{
		targetDirection = base.transform.TransformDirection(input).normalized;
		this.input = input;
	}

	protected virtual void SetMovementInput(Vector3 input, float smooth)
	{
		targetDirection = base.transform.TransformDirection(input).normalized;
		this.input = Vector3.Lerp(this.input, input, smooth * Time.deltaTime);
	}

	protected virtual void SetMovementInput(Vector3 input, Vector3 targetDirection, float smooth)
	{
		this.targetDirection = targetDirection.normalized;
		this.input = Vector3.Lerp(this.input, input, smooth * Time.deltaTime);
	}

	protected virtual void UpdateLocomotion()
	{
		StepOffset();
		ControlLocomotion();
		PhysicsBehaviour();
		CheckGroundDistance();
		CheckAutoCrouch();
	}

	protected virtual void ControlLocomotion()
	{
		if (!base.isDead && !isJumping && isGrounded)
		{
			CalculateRotationMagnitude();
			if (isStrafing)
			{
				StrafeMovement();
			}
			else
			{
				FreeMovement();
			}
			if (temporaryDirectionTime > 0f)
			{
				temporaryDirectionTime -= Time.deltaTime;
			}
		}
	}

	protected virtual void StrafeMovement()
	{
		StrafeLimitSpeed(maxSpeed);
		if (stopMove)
		{
			strafeMagnitude = 0f;
		}
		Vector3 normalized = targetDirection.normalized;
		normalized.y = 0f;
		if (normalized.magnitude > 0.1f && input.magnitude > 0.1f && !isRolling && !base.ragdolled)
		{
			if (!lockRotation)
			{
				Rotate(normalized);
			}
		}
		else if (temporaryDirectionTime > 0f)
		{
			Rotate(temporaryDirection);
		}
		base.animator.SetFloat("InputMagnitude", isJumping ? 0f : strafeMagnitude, inputMagnitudeSmooth, Time.deltaTime);
	}

	protected virtual void CalculateRotationMagnitude()
	{
		float num = (base.transform.eulerAngles - lastCharacterAngle).NormalizeAngle().y / (isStrafing ? strafeSpeed.rotationSpeed : freeSpeed.rotationSpeed);
		rotationMagnitude = num;
		lastCharacterAngle = base.transform.eulerAngles;
	}

	protected virtual void StrafeLimitSpeed(float value)
	{
		float num = Mathf.Clamp(input.z, 0f - maxSpeed, maxSpeed);
		float num2 = Mathf.Clamp(input.x, 0f - maxSpeed, maxSpeed);
		speed = num;
		direction = num2;
		strafeMagnitude = Mathf.Clamp(new Vector2(speed, direction).magnitude, 0f, maxSpeed);
	}

	protected virtual void FreeMovement()
	{
		if (!base.animator)
		{
			return;
		}
		speed = Mathf.Abs(input.x) + Mathf.Abs(input.z);
		speed = Mathf.Clamp(speed, 0f, maxSpeed);
		if (stopMove)
		{
			speed = 0f;
		}
		base.animator.SetFloat("InputMagnitude", isJumping ? 0f : speed, inputMagnitudeSmooth, Time.deltaTime);
		bool flag = !actions;
		if (input.magnitude > 0.1f && targetDirection.magnitude > 0.2f && flag)
		{
			Vector3 normalized = targetDirection.normalized;
			freeRotation = Quaternion.LookRotation(normalized, base.transform.up);
			float y = freeRotation.eulerAngles.y;
			Vector3 euler = new Vector3(base.transform.eulerAngles.x, y, base.transform.eulerAngles.z);
			if (!lockRotation && !isRolling && speed > 0.1f && !base.ragdolled)
			{
				Rotate(Quaternion.Euler(euler));
			}
		}
		else if (temporaryDirectionTime > 0f)
		{
			Rotate(temporaryDirection);
		}
	}

	protected virtual void Rotate(Vector3 targetDirection)
	{
		if (targetDirection.magnitude > 0.1f)
		{
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(targetDirection, Vector3.up), rotationSpeed * Time.deltaTime);
		}
	}

	protected virtual void Rotate(Quaternion targetRotation)
	{
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
	}

	protected virtual void StepOffset()
	{
		if (!((double)input.sqrMagnitude < 0.1) && isGrounded && _useStepOffSet && !isJumping)
		{
			RaycastHit hitInfo = default(RaycastHit);
			Vector3 vector = ((isStrafing && input.magnitude > 0f) ? (base.transform.right * input.x + base.transform.forward * input.z).normalized : base.transform.forward);
			if (Physics.Raycast(new Ray(base.transform.position + new Vector3(0f, stepOffsetEnd, 0f) + vector * (_capsuleCollider.radius + 0.05f), Vector3.down), out hitInfo, stepOffsetEnd - stepOffsetStart, groundLayer) && hitInfo.point.y >= base.transform.position.y && hitInfo.point.y <= base.transform.position.y + stepOffsetEnd)
			{
				float num = (isStrafing ? Mathf.Clamp(input.magnitude, 0f, 1f) : speed);
				Vector3 vector2 = hitInfo.point - base.transform.position;
				Vector3 vector3 = _rigidbody.velocity;
				vector3.y = (vector2 * stepSmooth * (num * ((velocity > 1f) ? velocity : 1f))).y;
				_rigidbody.velocity = vector3;
			}
		}
	}

	protected virtual void CheckGroundDistance()
	{
		if ((_capsuleCollider != null && (_rigidbody.velocity.y > 0.1f || _rigidbody.velocity.y < -0.1f)) || isJumping)
		{
			float num = 10f;
			if (Physics.Raycast(base.transform.position + base.transform.up * _capsuleCollider.height * 0.5f, Vector3.down, out groundHit, _capsuleCollider.height, groundLayer))
			{
				num = base.transform.position.y - groundHit.point.y;
			}
			else if (Physics.SphereCast(base.transform.position + base.transform.up * _capsuleCollider.radius, _capsuleCollider.radius * 0.5f, Vector3.down, out groundHit, checkGroundDistance, groundLayer))
			{
				num = base.transform.position.y - groundHit.point.y;
			}
			GroundDistanceAnim = num;
			if (num >= checkGroundDistance)
			{
				isGrounded = false;
				verticalVelocity = _rigidbody.velocity.y;
			}
			if ((!actions || isJumping) && !isRolling && num < checkGroundDistance * 0.9f)
			{
				isGrounded = true;
			}
		}
		else if (!isJumping)
		{
			GroundDistanceAnim = 0f;
			isGrounded = true;
		}
	}

	protected virtual void PhysicsBehaviour()
	{
		if (isGrounded && input == Vector3.zero)
		{
			_capsuleCollider.material = maxFrictionPhysics;
		}
		else if (isGrounded && input != Vector3.zero)
		{
			_capsuleCollider.material = frictionPhysics;
		}
		else
		{
			_capsuleCollider.material = slippyPhysics;
		}
	}

	protected virtual void CheckAutoCrouch()
	{
		if (!useAutoCrouch)
		{
			return;
		}
		float radius = _capsuleCollider.radius + headDetectMargin;
		Vector3 origin = base.transform.position + Vector3.up * colliderHeight * headDetectStart;
		if (Physics.SphereCast(new Ray(origin, Vector3.up), radius, out var _, headDetectHeight + _capsuleCollider.radius, autoCrouchLayer))
		{
			if (!_isCrouchingFromCast)
			{
				_isCrouchingFromCast = true;
				_capsuleCollider.center = colliderCenter / 1.8f;
				_capsuleCollider.height = colliderHeight / 1.8f;
			}
		}
		else if (_isCrouchingFromCast)
		{
			_isCrouchingFromCast = false;
			_capsuleCollider.center = colliderCenter;
			_capsuleCollider.height = colliderHeight;
		}
	}

	protected virtual void HealthControl()
	{
		if (isGrounded && base.isDead)
		{
			_rigidbody.isKinematic = true;
			_capsuleCollider.enabled = false;
		}
		if (base.currentHealth > 0f && base.isDead)
		{
			base.isDead = false;
			if (!Sitting)
			{
				_rigidbody.isKinematic = false;
			}
			else
			{
				_rigidbody.isKinematic = true;
			}
			_capsuleCollider.enabled = true;
			triggerDieBehaviour = false;
			if (deathBy == DeathBy.Animation || deathBy == DeathBy.AnimationWithRagdoll)
			{
				base.animator.SetBool("isDead", base.isDead);
			}
		}
	}

	protected virtual void UpdateAnimator()
	{
		if (!(base.animator == null) && base.animator.isActiveAndEnabled)
		{
			base.animator.speed = animatorSpeed;
			AnimatorLayerControl();
			AnimatorLocomotion();
			AnimatorDeath();
			ActionsControl();
		}
	}

	protected virtual void AnimatorLocomotion()
	{
		bool flag = !stopMove && !lockMovement && !animatorStateInfos.HasTag("LockMovement");
		base.animator.SetFloat("InputHorizontal", (flag && isStrafing && !isSprinting) ? direction : 0f, 0.2f, Time.deltaTime);
		base.animator.SetFloat("InputVertical", flag ? speed : 0f, 0.2f, Time.deltaTime);
		IsStrafingAnim = isStrafing;
		IsGroundedAnim = isGrounded;
		IsCrouchingAnim = isCrouching;
		VerticalVelocityAnim = verticalVelocity;
	}

	protected virtual void AnimatorLayerControl()
	{
		if (baseLayer != -1)
		{
			baseLayerInfo = base.animator.GetCurrentAnimatorStateInfo(baseLayer);
		}
		if (underBodyLayer != -1)
		{
			underBodyInfo = base.animator.GetCurrentAnimatorStateInfo(underBodyLayer);
		}
		if (rightArmLayer != -1)
		{
			rightArmInfo = base.animator.GetCurrentAnimatorStateInfo(rightArmLayer);
		}
		if (leftArmLayer != -1)
		{
			leftArmInfo = base.animator.GetCurrentAnimatorStateInfo(leftArmLayer);
		}
		if (upperBodyLayer != -1)
		{
			upperBodyInfo = base.animator.GetCurrentAnimatorStateInfo(upperBodyLayer);
		}
		if (fullbodyLayer != -1)
		{
			fullBodyInfo = base.animator.GetCurrentAnimatorStateInfo(fullbodyLayer);
		}
	}

	protected virtual void AnimatorDeath()
	{
		if (!base.isDead)
		{
			return;
		}
		if (!triggerDieBehaviour)
		{
			triggerDieBehaviour = true;
			TriggerDeath();
		}
		if (deathBy == DeathBy.Animation)
		{
			int layerIndex = 0;
			vAnimatorStateInfos.vStateInfo stateInfoUsingTag = animatorStateInfos.GetStateInfoUsingTag("Dead");
			if (stateInfoUsingTag != null && !base.animator.IsInTransition(layerIndex) && stateInfoUsingTag.normalizedTime >= 0.99f && GroundDistanceAnim <= 0.15f)
			{
				RemoveComponents();
			}
		}
		else if (deathBy == DeathBy.AnimationWithRagdoll)
		{
			int layerIndex2 = 0;
			vAnimatorStateInfos.vStateInfo stateInfoUsingTag2 = animatorStateInfos.GetStateInfoUsingTag("Dead");
			if (stateInfoUsingTag2 != null && !base.animator.IsInTransition(layerIndex2) && stateInfoUsingTag2.normalizedTime >= 0.8f && GroundDistanceAnim <= 0.15f)
			{
				base.onActiveRagdoll.Invoke(null);
				RemoveComponents();
			}
		}
		else if (deathBy == DeathBy.Ragdoll)
		{
			base.onActiveRagdoll.Invoke(null);
			RemoveComponents();
		}
	}

	public virtual void TriggerDeath()
	{
		base.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		if (deathBy == DeathBy.Animation || deathBy == DeathBy.AnimationWithRagdoll)
		{
			base.animator.SetBool("isDead", base.isDead);
		}
	}

	public virtual void RemoveComponents()
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

	protected virtual void ControlSpeed(float velocity)
	{
		if (Time.deltaTime != 0f && !isJumping)
		{
			bool flag = !stopMove && !lockMovement && !animatorStateInfos.HasTag("LockMovement");
			if (!flag)
			{
				velocity = 0f;
			}
			if (useRootMotion && !actions && !customAction && flag)
			{
				this.velocity = velocity;
				Vector3 b = new Vector3(base.animator.deltaPosition.x, base.transform.position.y, base.animator.deltaPosition.z) * ((velocity > 0f) ? velocity : 1f) / Time.deltaTime;
				b.y = _rigidbody.velocity.y;
				_rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, b, 20f * Time.deltaTime);
			}
			else if (actions || base.isDead || !flag || customAction)
			{
				this.velocity = velocity;
				Vector3 zero = Vector3.zero;
				zero.y = _rigidbody.velocity.y;
				_rigidbody.velocity = zero;
				base.transform.position = base.animator.rootPosition;
			}
			else if (isStrafing)
			{
				Vector3 b2 = base.transform.TransformDirection(new Vector3(input.x, 0f, input.z)) * ((velocity > 0f) ? velocity : 1f);
				b2.y = _rigidbody.velocity.y;
				_rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, b2, 20f * Time.deltaTime);
			}
			else
			{
				Vector3 vector = base.transform.forward * velocity * speed;
				vector.y = _rigidbody.velocity.y;
				_rigidbody.velocity = vector;
			}
		}
	}

	protected virtual void ActionsControl()
	{
		isRolling = baseLayerInfo.IsName("Roll");
		UpdateLockMovement();
		UpdateLockRotation();
		UpdateCustomAction();
	}

	protected virtual void UpdateLockMovement()
	{
		lockMovement = IsAnimatorTag("LockMovement");
	}

	protected virtual void UpdateLockRotation()
	{
		lockRotation = IsAnimatorTag("LockRotation");
	}

	public virtual void UpdateCustomAction()
	{
		customAction = IsAnimatorTag("CustomAction");
	}

	protected virtual void OnAnimatorMove()
	{
		if (!base.animator || !isGrounded || base.ragdolled)
		{
			return;
		}
		if (customAction || input.magnitude < 0.1f)
		{
			_rigidbody.velocity = Vector3.zero;
			_rigidbody.position = base.animator.rootPosition;
			base.transform.rotation = base.animator.rootRotation;
			return;
		}
		if (lockMovement)
		{
			_rigidbody.velocity = Vector3.zero;
			_rigidbody.position = base.animator.rootPosition;
			if (lockRotation)
			{
				base.transform.rotation = base.animator.rootRotation;
			}
			return;
		}
		if (lockRotation)
		{
			_rigidbody.rotation = base.animator.rootRotation;
		}
		float num = Mathf.Abs(strafeMagnitude);
		if (isStrafing)
		{
			if (num <= 0.5f)
			{
				ControlSpeed(strafeSpeed.walkSpeed);
			}
			else if (num > 0.5f && num <= 1f)
			{
				ControlSpeed(strafeSpeed.runningSpeed);
			}
			else
			{
				ControlSpeed(strafeSpeed.sprintSpeed);
			}
			if (isCrouching)
			{
				ControlSpeed(strafeSpeed.crouchSpeed);
			}
		}
		else if (!isStrafing)
		{
			if (speed <= 0.5f)
			{
				ControlSpeed(freeSpeed.walkSpeed);
			}
			else if ((double)speed > 0.5 && speed <= 1f)
			{
				ControlSpeed(freeSpeed.runningSpeed);
			}
			else
			{
				ControlSpeed(freeSpeed.sprintSpeed);
			}
			if (isCrouching)
			{
				ControlSpeed(freeSpeed.crouchSpeed);
			}
		}
	}

	public virtual bool IsAnimatorTag(string tag)
	{
		if (base.animator == null)
		{
			return false;
		}
		if (animatorStateInfos.HasTag(tag))
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

	public virtual void Stop()
	{
		if (input != Vector3.zero)
		{
			input = Vector3.zero;
		}
		movementSpeed = vAIMovementSpeed.Idle;
	}

	public virtual void Walk()
	{
		movementSpeed = vAIMovementSpeed.Walking;
	}

	public virtual void Run()
	{
		movementSpeed = vAIMovementSpeed.Running;
	}

	public virtual void Sprint()
	{
		movementSpeed = vAIMovementSpeed.Sprinting;
	}

	public virtual void JumpTo(Vector3 jumpTarget)
	{
		if (isGrounded && !lockMovement && !actions && !isJumping)
		{
			base.animator.CrossFadeInFixedTime("JumpMove", 0.1f);
			StartCoroutine(JumpParabole(jumpTarget, jumpHeight, jumpSpeedPerMeter));
		}
	}

	protected virtual IEnumerator JumpParabole(Vector3 targetPos, float height, float duration)
	{
		base.animator.CrossFadeInFixedTime("JumpMove", 0.1f);
		isJumping = true;
		Vector3 startPos = base.transform.position;
		float normalizedTime = 0f;
		Vector3 jumpDir = targetPos - base.transform.position;
		jumpDir.y = 0f;
		while (normalizedTime < 1f)
		{
			float num = height * 4f * (normalizedTime - normalizedTime * normalizedTime);
			base.transform.transform.position = Vector3.Lerp(startPos, targetPos, normalizedTime) + num * Vector3.up;
			normalizedTime += Time.deltaTime / duration;
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(jumpDir), freeSpeed.rotationSpeed * Time.deltaTime);
			yield return null;
		}
		isJumping = false;
	}

	public virtual void RollTo(Vector3 direction)
	{
		if (!isRolling && !isJumping && isGrounded && !lockMovement && !customAction)
		{
			targetDirection = direction.normalized;
			targetDirection.y = 0f;
			base.animator.SetTrigger("ResetState");
			base.animator.CrossFadeInFixedTime("Roll", 0.01f);
			base.transform.rotation = Quaternion.LookRotation(targetDirection);
		}
	}

	public virtual void SetStrafeLocomotion()
	{
		isStrafing = true;
	}

	public virtual void SetFreeLocomotion()
	{
		isStrafing = false;
	}

	public virtual void EnableAIController()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (!Sitting)
			{
				_rigidbody.isKinematic = false;
			}
			else
			{
				_rigidbody.isKinematic = true;
			}
			_capsuleCollider.isTrigger = false;
			base.enabled = true;
			onEnableController.Invoke();
		}
	}

	public virtual void DisableAIController()
	{
		if (base.gameObject.activeInHierarchy)
		{
			targetDirection = base.transform.forward;
			input = Vector3.zero;
			movementSpeed = vAIMovementSpeed.Idle;
			if (base.animator.isActiveAndEnabled)
			{
				base.animator.SetFloat("InputHorizontal", 0f);
				base.animator.SetFloat("InputVertical", 0f);
				base.animator.SetFloat("InputMagnitude", 0f);
			}
			_rigidbody.velocity = Vector3.zero;
			_rigidbody.isKinematic = true;
			base.enabled = false;
			onDisableController.Invoke();
		}
	}

	public override void TakeDamage(vDamage damage)
	{
		base.TakeDamage(damage);
		if (damage.damageValue > 0)
		{
			TriggerDamageRection(damage);
		}
	}

	protected virtual void TriggerDamageRection(vDamage damage)
	{
		if (isRolling)
		{
			return;
		}
		if (base.animator != null && base.animator.enabled && !damage.activeRagdoll && base.currentHealth > 0f)
		{
			if (hitDirectionHash.isValid)
			{
				base.animator.SetInteger(hitDirectionHash, (int)base.transform.HitAngle(damage.sender.position));
			}
			if (damage.hitReaction)
			{
				if (reactionIDHash.isValid)
				{
					base.animator.SetInteger(reactionIDHash, damage.reaction_id);
				}
				if (triggerReactionHash.isValid)
				{
					base.animator.SetTrigger(triggerReactionHash);
				}
				if (triggerResetStateHash.isValid)
				{
					base.animator.SetTrigger(triggerResetStateHash);
				}
			}
			else
			{
				if (recoilIDHash.isValid)
				{
					base.animator.SetInteger(recoilIDHash, damage.recoil_id);
				}
				if (triggerRecoilHash.isValid)
				{
					base.animator.SetTrigger(triggerRecoilHash);
				}
				if (triggerResetStateHash.isValid)
				{
					base.animator.SetTrigger(triggerResetStateHash);
				}
			}
		}
		if (damage.activeRagdoll)
		{
			base.onActiveRagdoll.Invoke(damage);
		}
	}

	public override void ResetRagdoll()
	{
		lockMovement = false;
		verticalVelocity = 0f;
		base.ragdolled = false;
		_rigidbody.WakeUp();
		_rigidbody.useGravity = true;
		_rigidbody.isKinematic = false;
		_capsuleCollider.enabled = true;
	}

	public override void EnableRagdoll()
	{
		base.animator.SetFloat("InputHorizontal", 0f);
		base.animator.SetFloat("InputVertical", 0f);
		base.animator.SetFloat("VerticalVelocity", 0f);
		base.ragdolled = true;
		_capsuleCollider.enabled = false;
		_rigidbody.useGravity = false;
		_rigidbody.isKinematic = true;
		lockMovement = true;
	}
}
