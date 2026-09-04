using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.vActions;

[vClassHeader("Swimming Action", true, "icon_v2", false, "")]
public class vSwimming : vActionListener
{
	[vEditorToolbar("Settings", false, "", false, false)]
	[Tooltip("Name of the tag assign into the Water object")]
	public string waterTag = "Water";

	[Header("Speed & Extra Options")]
	[Tooltip("Uncheck if you don't want to go under water")]
	public bool swimUpAndDown = true;

	[Tooltip("Speed to swim forward")]
	public float swimForwardSpeed = 4f;

	[Tooltip("Speed to rotate the character")]
	public float swimRotationSpeed = 4f;

	[Tooltip("Smooth value for the character movement")]
	public float swimMovementSmooth = 2f;

	[Tooltip("Smooth value for the character animation transition")]
	public float swimAnimationSmooth = 1f;

	[Tooltip("Smooth value for the character movement up and down")]
	public float swimUpDownSmooth = 2f;

	[vHelpBox("! Assign a curve here, otherwise the character won't move up or down !", vHelpBoxAttribute.MessageType.None)]
	public AnimationCurve updownSmoothCurve;

	[Tooltip("Speed to swim up")]
	public float swimUpSpeed = 3f;

	[Tooltip("Speed to swim down")]
	public float swimDownSpeed = -3f;

	[Tooltip("Increase the radius of the capsule collider to avoid enter walls")]
	public float colliderRadius = 0.5f;

	[Tooltip("Increase the radius of the capsule collider to avoid enter walls")]
	public float colliderHeight = 0.5f;

	[Tooltip("Height offset to match the character Y position")]
	public float heightOffset = 0.3f;

	[Header("Health/Stamina Consuption")]
	[Tooltip("Leave with 0 if you don't want to use stamina consuption")]
	public float stamina = 15f;

	[Tooltip("How much health will drain after all the stamina were consumed")]
	public int healthConsumption = 1;

	[Header("Particle Effects")]
	public GameObject impactEffect;

	[Tooltip("Check the Rigibody.Y of the character to trigger the ImpactEffect Particle")]
	public float velocityToImpact = -4f;

	public GameObject waterRingEffect;

	[Tooltip("Frequency to instantiate the WaterRing effect while standing still")]
	public float waterRingFrequencyIdle = 0.8f;

	[Tooltip("Frequency to instantiate the WaterRing effect while swimming")]
	public float waterRingFrequencySwim = 0.15f;

	[Tooltip("Instantiate a prefab when exit the water")]
	public GameObject waterDrops;

	[Tooltip("Y Offset based at the capsule collider")]
	public float waterDropsYOffset = 1.6f;

	[Header("Inputs")]
	[Tooltip("Input to make the character go up")]
	public GenericInput swimUpInput = new GenericInput("Space", "X", "X");

	[Tooltip("Input to make the character go down")]
	public GenericInput swimDownInput = new GenericInput("LeftShift", "Y", "Y");

	[vEditorToolbar("Events", false, "", false, false)]
	public UnityEvent OnEnterWater;

	public UnityEvent OnExitWater;

	public UnityEvent OnAboveWater;

	public UnityEvent OnUnderWater;

	[vEditorToolbar("Debug", false, "", false, false)]
	[Tooltip("Debug Mode will show the current behaviour at the console window")]
	public bool debugMode;

	[vReadOnly(false)]
	public float curretCharacterDepth;

	[vReadOnly(false)]
	public GameObject water;

	[vReadOnly(false)]
	public bool isSwimming;

	[vReadOnly(false)]
	public bool inTheWater;

	[vReadOnly(false)]
	[SerializeField]
	public bool isUnderWater;

	protected float swimUpInterpolate;

	protected float swimDownInterpolate;

	protected float waterHeightLevel;

	protected vThirdPersonInput tpInput;

	protected float timer;

	protected float originalMoveSpeed;

	protected float originalRotationSpeed;

	protected float originalMovementSmooth;

	protected float originalAnimationSmooth;

	protected float waterRingSpawnFrequency;

	protected bool triggerSwimState;

	protected bool triggerUnderWater;

	protected bool triggerAboveWater;

	public virtual Vector3 characterCenter => base.transform.position + Vector3.up * (tpInput.cc.colliderHeightDefault * 0.5f + heightOffset);

	protected override void Start()
	{
		base.Start();
		tpInput = GetComponentInParent<vThirdPersonInput>();
		if ((bool)tpInput)
		{
			tpInput.onUpdate -= UpdateSwimmingBehavior;
			tpInput.onUpdate += UpdateSwimmingBehavior;
		}
	}

	protected override void SetUpListener()
	{
		base.actionEnter = true;
		base.actionExit = true;
		base.actionStay = false;
	}

	protected virtual void OnDrawGizmosSelected()
	{
		if ((bool)water)
		{
			Matrix4x4 matrix = Gizmos.matrix;
			Vector3 vector = new Vector3(base.transform.position.x, waterHeightLevel, base.transform.position.z);
			Gizmos.color = Color.blue * 0.8f;
			Gizmos.matrix = Matrix4x4.TRS(vector, Quaternion.identity, new Vector3(1f, 0.001f, 1f));
			Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
			Gizmos.matrix = matrix;
			Gizmos.color = Color.green * 0.8f;
			Gizmos.DrawLine(vector, characterCenter);
			Gizmos.matrix = Matrix4x4.TRS(characterCenter, Quaternion.identity, new Vector3(1f, 0.001f, 1f));
			Gizmos.DrawWireSphere(Vector3.zero, 0.25f);
		}
	}

	protected virtual void UpdateSwimmingBehavior()
	{
		if (inTheWater)
		{
			UnderWaterBehaviour();
			SwimmingBehaviour();
		}
	}

	protected virtual void SwimmingBehaviour()
	{
		if ((bool)water)
		{
			waterHeightLevel = water.transform.position.y;
			curretCharacterDepth = 0f - (characterCenter.y - waterHeightLevel);
			isUnderWater = curretCharacterDepth > 0.5f;
			isSwimming = (isSwimming ? (curretCharacterDepth >= -0.2f) : (curretCharacterDepth >= 0f));
		}
		if (isSwimming)
		{
			if (tpInput.cc.currentHealth > 0f)
			{
				if (!triggerSwimState)
				{
					EnterSwimState();
				}
				SwimUpOrDownInput();
				tpInput.SetStrafeLocomotion(value: false);
				tpInput.MoveInput();
				tpInput.cc.SetAnimatorMoveSpeed(tpInput.cc.freeSpeed);
			}
		}
		else
		{
			ExitSwimState();
		}
	}

	protected virtual void UnderWaterBehaviour()
	{
		if (isUnderWater)
		{
			StaminaConsumption();
			if (!triggerUnderWater)
			{
				tpInput.cc.colliderRadius = colliderRadius;
				tpInput.cc.colliderHeight = colliderHeight;
				triggerUnderWater = true;
				triggerAboveWater = false;
				OnUnderWater.Invoke();
			}
		}
		else
		{
			WaterRingEffect();
			if (!triggerAboveWater && triggerSwimState)
			{
				tpInput.cc.ResetCapsule();
				triggerUnderWater = false;
				triggerAboveWater = true;
				OnAboveWater.Invoke();
			}
		}
	}

	protected virtual void StaminaConsumption()
	{
		if (tpInput.cc.currentStamina <= 0f)
		{
			tpInput.cc.ChangeHealth(-healthConsumption);
			return;
		}
		tpInput.cc.ReduceStamina(stamina, accumulative: true);
		tpInput.cc.currentStaminaRecoveryDelay = 0.25f;
	}

	public override void OnActionEnter(Collider other)
	{
		if (other.gameObject.CompareTag(waterTag) && !tpInput.cc.customAction)
		{
			if (debugMode)
			{
				Debug.Log("Player enter the Water");
			}
			inTheWater = true;
			water = other.gameObject;
			waterHeightLevel = other.transform.position.y;
			originalMoveSpeed = tpInput.cc.moveSpeed;
			originalRotationSpeed = tpInput.cc.freeSpeed.rotationSpeed;
			originalAnimationSmooth = tpInput.cc.freeSpeed.animationSmooth;
			originalMovementSmooth = tpInput.cc.freeSpeed.movementSmooth;
			if (tpInput.cc.verticalVelocity <= velocityToImpact && (bool)impactEffect)
			{
				Object.Instantiate(position: new Vector3(base.transform.position.x, other.transform.position.y, base.transform.position.z), original: impactEffect, rotation: tpInput.transform.rotation).transform.SetParent(vObjectContainer.root, worldPositionStays: true);
			}
		}
	}

	public override void OnActionExit(Collider other)
	{
		if (!other.gameObject.CompareTag(waterTag))
		{
			return;
		}
		if (debugMode)
		{
			Debug.Log("Player left the Water");
		}
		if (other.gameObject == water)
		{
			water = null;
			inTheWater = false;
			isSwimming = false;
			ExitSwimState();
			if ((bool)waterDrops)
			{
				Object.Instantiate(position: new Vector3(base.transform.position.x, base.transform.position.y + waterDropsYOffset, base.transform.position.z), original: waterDrops, rotation: tpInput.transform.rotation).transform.parent = base.transform;
			}
		}
	}

	protected virtual void EnterSwimState()
	{
		if (debugMode)
		{
			Debug.Log("Player is Swimming");
		}
		triggerUnderWater = false;
		triggerAboveWater = false;
		triggerSwimState = true;
		OnEnterWater.Invoke();
		tpInput.SetLockAllInput(value: true);
		tpInput.cc.disableCheckGround = true;
		tpInput.cc.lockSetMoveSpeed = true;
		tpInput.cc.moveSpeed = swimForwardSpeed;
		tpInput.cc.freeSpeed.rotationSpeed = swimRotationSpeed;
		tpInput.cc.freeSpeed.animationSmooth = swimAnimationSmooth;
		tpInput.cc.freeSpeed.movementSmooth = swimMovementSmooth;
		ResetPlayerValues();
		tpInput.cc.animator.CrossFadeInFixedTime("Swimming", 0.25f);
		tpInput.cc._rigidbody.useGravity = false;
		tpInput.cc._rigidbody.drag = 10f;
		tpInput.cc._capsuleCollider.isTrigger = false;
	}

	protected virtual void ExitSwimState()
	{
		if (triggerSwimState)
		{
			if (debugMode)
			{
				Debug.Log("Player Stop Swimming");
			}
			isUnderWater = false;
			triggerSwimState = false;
			OnExitWater.Invoke();
			tpInput.SetLockAllInput(value: false);
			tpInput.cc.disableCheckGround = false;
			tpInput.cc.lockSetMoveSpeed = false;
			tpInput.cc.moveSpeed = originalMoveSpeed;
			tpInput.cc.freeSpeed.rotationSpeed = originalRotationSpeed;
			tpInput.cc.freeSpeed.animationSmooth = originalAnimationSmooth;
			tpInput.cc.freeSpeed.movementSmooth = originalMovementSmooth;
			tpInput.cc.animator.SetInteger(vAnimatorParameters.ActionState, 0);
			tpInput.cc.ResetCapsule();
			tpInput.cc._rigidbody.useGravity = true;
			tpInput.cc._rigidbody.drag = 0f;
		}
	}

	protected virtual void SwimUpOrDownInput()
	{
		if (tpInput.cc.customAction)
		{
			return;
		}
		bool flag = curretCharacterDepth > 0.2f;
		if ((swimUpInput.GetButton() || !swimUpAndDown) && flag)
		{
			if (debugMode)
			{
				Debug.Log("Player Swimming UP");
			}
			swimDownInterpolate = 0f;
			swimUpInterpolate += Time.deltaTime * swimUpDownSmooth;
			swimUpInterpolate = Mathf.Clamp(swimUpInterpolate, 0f, 1f);
			Vector3 velocity = tpInput.cc._rigidbody.velocity;
			velocity.y = Mathf.Lerp(velocity.y, swimUpSpeed, updownSmoothCurve.Evaluate(swimUpInterpolate));
			tpInput.cc._rigidbody.velocity = velocity;
			tpInput.cc.input.y = 1f;
			tpInput.cc.animator.SetInteger(vAnimatorParameters.ActionState, 4);
			return;
		}
		if (swimDownInput.GetButton() && swimUpAndDown)
		{
			if (debugMode)
			{
				Debug.Log("Player Swimming Down");
			}
			swimUpInterpolate = 0f;
			swimDownInterpolate += Time.deltaTime * swimUpDownSmooth;
			swimDownInterpolate = Mathf.Clamp(swimDownInterpolate, 0f, 1f);
			Vector3 velocity2 = tpInput.cc._rigidbody.velocity;
			velocity2.y = Mathf.Lerp(velocity2.y, swimDownSpeed, updownSmoothCurve.Evaluate(swimDownInterpolate));
			tpInput.cc._rigidbody.velocity = velocity2;
			tpInput.cc.input.y = -1f;
			tpInput.cc.animator.SetInteger(vAnimatorParameters.ActionState, 3);
			return;
		}
		tpInput.cc.input.y = 0f;
		swimDownInterpolate = 0f;
		swimUpInterpolate = 0f;
		Vector3 velocity3 = tpInput.cc._rigidbody.velocity;
		velocity3.y = Mathf.Lerp(velocity3.y, 0f, swimUpDownSmooth * Time.deltaTime);
		tpInput.cc._rigidbody.velocity = velocity3;
		if (isUnderWater)
		{
			tpInput.cc.animator.SetInteger(vAnimatorParameters.ActionState, 2);
			return;
		}
		Vector3 position = base.transform.position;
		position.y = waterHeightLevel - (tpInput.cc.colliderHeightDefault * 0.5f + heightOffset);
		base.transform.position = Vector3.Lerp(base.transform.position, position, 0.5f * Time.deltaTime);
		tpInput.cc.animator.SetInteger(vAnimatorParameters.ActionState, 1);
	}

	protected virtual void WaterRingEffect()
	{
		if ((bool)waterRingEffect)
		{
			if (tpInput.cc.input != Vector3.zero)
			{
				waterRingSpawnFrequency = waterRingFrequencySwim;
			}
			else
			{
				waterRingSpawnFrequency = waterRingFrequencyIdle;
			}
			timer += Time.deltaTime;
			if (timer >= waterRingSpawnFrequency)
			{
				Object.Instantiate(position: new Vector3(base.transform.position.x, waterHeightLevel, base.transform.position.z), original: waterRingEffect, rotation: tpInput.transform.rotation).transform.SetParent(vObjectContainer.root, worldPositionStays: true);
				timer = 0f;
			}
		}
	}

	protected virtual void ResetPlayerValues()
	{
		tpInput.cc.isJumping = false;
		tpInput.cc.isSprinting = false;
		tpInput.cc.isCrouching = false;
		tpInput.cc.animator.SetFloat(vAnimatorParameters.InputHorizontal, 0f);
		tpInput.cc.animator.SetFloat(vAnimatorParameters.InputVertical, 0f);
		tpInput.cc.animator.SetInteger(vAnimatorParameters.ActionState, 1);
		tpInput.cc.isGrounded = true;
		tpInput.cc.animator.SetBool(vAnimatorParameters.IsGrounded, value: true);
		tpInput.cc.verticalVelocity = 0f;
	}
}
