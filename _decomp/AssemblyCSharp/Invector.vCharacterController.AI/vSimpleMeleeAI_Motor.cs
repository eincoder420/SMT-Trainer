using System;
using System.Collections;
using System.Collections.Generic;
using Invector.vMelee;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI;

public class vSimpleMeleeAI_Motor : vCharacter
{
	public class OnSetAgressiveEvent : UnityEvent<bool>
	{
	}

	public enum AIPatrolWithOutAreaStyle
	{
		GoToStartPoint,
		Wander,
		Idle
	}

	public enum AIStates
	{
		Idle,
		PatrolSubPoints,
		PatrolWaypoints,
		Wander,
		Chase
	}

	[Serializable]
	public struct CharacterTarget
	{
		public Transform transform;

		public vIHealthController character;

		public Collider colliderTarget;
	}

	[vEditorToolbar("Layers", false, "", false, false)]
	[Tooltip("Layers that the character can walk on")]
	public LayerMask groundLayer = 1;

	[Tooltip("Distance to became not grounded")]
	[SerializeField]
	protected float groundCheckDistance = 0.5f;

	[Tooltip("What objects can make the character auto crouch")]
	public LayerMask autoCrouchLayer = 1;

	[Tooltip("[SPHERECAST] ADJUST IN PLAY MODE - White Spherecast put just above the head, this will make the character Auto-Crouch if something hit the sphere.")]
	public float headDetect = 0.95f;

	[vEditorToolbar("Locomotion", false, "", false, false)]
	[Tooltip("Use to limit your locomotion animation, if you want to patrol walking set this value to 0.5f")]
	[Range(0f, 1.5f)]
	public float patrolSpeed = 0.5f;

	[Tooltip("Use to limit your locomotion animation, if you want to wander walking set this value to 0.5f")]
	[Range(0f, 1.5f)]
	public float wanderSpeed = 0.5f;

	[Tooltip("Use to limit your locomotion animation, if you want to chase the target walking set this value to 0.5f")]
	[Range(0f, 1.5f)]
	public float chaseSpeed = 1f;

	[Tooltip("Use to limit your locomotion animation, if you want to strafe the target walking set this value to 0.5f")]
	[Range(0f, 1.5f)]
	public float strafeSpeed = 1f;

	[Header("--- Strafe ---")]
	[Tooltip("Strafe around the target")]
	public bool strafeSideways = true;

	[Tooltip("Strafe a few steps backwards")]
	public bool strafeBackward = true;

	[Tooltip("Distance to switch to the strafe locomotion, leave with 0 if you don't want your character to strafe")]
	public float strafeDistance = 3f;

	[Tooltip("Min time to change the strafe direction")]
	public float minStrafeSwape = 2f;

	[Tooltip("Max time to change the strafe direction")]
	public float maxStrafeSwape = 5f;

	[Tooltip("Velocity to rotate the character while strafing")]
	public float strafeRotationSpeed = 5f;

	[vEditorToolbar("Detection", false, "", false, false)]
	public AIStates currentState = AIStates.PatrolWaypoints;

	[Header("Who is your Target?")]
	public vSimpleMeleeAI_SphereSensor sphereSensor;

	public vTagMask tagsToDetect = new vTagMask { "Player" };

	public LayerMask layersToDetect = 1;

	public LayerMask obstaclesLayer;

	public bool sortTargetFromDistance;

	[Range(0f, 360f)]
	public float fieldOfView = 95f;

	[Tooltip("Max Distance to detect the Target with FOV")]
	public float maxDetectDistance = 5f;

	[Tooltip("Min Distance to noticed the Target without FOV")]
	public float minDetectDistance = 2f;

	[Tooltip("Distance to lost the Target")]
	public float distanceToLostTarget = 5f;

	[Tooltip("Distance to stop when chasing the Player")]
	public float chaseStopDistance = 1f;

	public bool drawAgentPath;

	public bool displayGizmos;

	[vEditorToolbar("Combat", false, "", false, false)]
	[Tooltip("Check if you want the Enemy to be passive even if you attack him")]
	public bool passiveToDamage;

	[Tooltip("Check if you want the Enemy to chase the Target at first sight")]
	public bool agressiveAtFirstSight = true;

	[Tooltip("Velocity to rotate the character while attacking")]
	public float attackRotationSpeed = 0.5f;

	[Tooltip("Delay to trigger the first attack when close to the target")]
	public float firstAttackDelay;

	[Tooltip("Min frequency to attack")]
	public float minTimeToAttack = 4f;

	[Tooltip("Max frequency to attack")]
	public float maxTimeToAttack = 6f;

	[Tooltip("How many attacks the AI will make on a combo")]
	public int maxAttackCount = 3;

	[Tooltip("Randomly attacks based on the maxAttackCount")]
	public bool randomAttackCount = true;

	[Range(0f, 1f)]
	public float chanceToRoll = 0.1f;

	[Range(0f, 1f)]
	public float chanceToBlockInStrafe = 0.1f;

	[Range(0f, 1f)]
	public float chanceToBlockAttack;

	[Tooltip("How much time the character will stand up the shield")]
	public float raiseShield = 4f;

	[Tooltip("How much time the character will lower the shield")]
	public float lowerShield = 2f;

	[vEditorToolbar("Waypoint", false, "", false, false)]
	[Tooltip("Max Distance to change waypoint")]
	[Range(0.5f, 100f)]
	public float distanceToChangeWaypoint = 1f;

	[Tooltip("Min Distance to stop when Patrolling through waypoints")]
	[Range(0.5f, 100f)]
	public float patrollingStopDistance = 0.5f;

	public AIPatrolWithOutAreaStyle patrolWithoutAreaStyle;

	public vWaypointArea pathArea;

	public bool randomWaypoints;

	public vFisherYatesRandom randomWaypoint = new vFisherYatesRandom();

	public vFisherYatesRandom randomPatrolPoint = new vFisherYatesRandom();

	[HideInInspector]
	public CapsuleCollider _capsuleCollider;

	[HideInInspector]
	public v_SpriteHealth healthSlider;

	[HideInInspector]
	public vMeleeManager meleeManager;

	public OnSetAgressiveEvent onSetAgressive = new OnSetAgressiveEvent();

	[HideInInspector]
	public bool lockMovement;

	public CharacterTarget currentTarget;

	protected Vector3 targetPos;

	[SerializeField]
	[vReadOnly(true)]
	protected bool canSeeTarget;

	protected Vector3 destination;

	protected Vector3 fwd;

	protected bool isGrounded;

	protected bool isStrafing;

	protected bool inResetAttack;

	protected bool firstAttack = true;

	protected int attackCount;

	protected int currentWaypoint;

	protected int currentPatrolPoint;

	protected float direction;

	protected float timer;

	protected float wait;

	protected float fovAngle;

	protected float sideMovement;

	protected float fwdMovement;

	protected float strafeSwapeFrequency;

	protected float groundDistance;

	protected Vector3 startPosition;

	protected RaycastHit groundHit;

	protected NavMeshAgent agent;

	protected NavMeshPath agentPath;

	protected Quaternion freeRotation;

	protected Quaternion desiredRotation;

	protected Vector3 oldPosition;

	protected Vector3 combatMovement;

	protected Vector3 rollDirection;

	protected Rigidbody _rigidbody;

	protected PhysicMaterial frictionPhysics;

	protected Transform head;

	protected Collider colliderTarget;

	protected vWaypoint targetWaypoint;

	protected vPoint targetPatrolPoint;

	protected List<vPoint> visitedPatrolPoint = new List<vPoint>();

	protected List<vWaypoint> visitedWaypoint = new List<vWaypoint>();

	protected bool isCrouched;

	protected bool canAttack;

	protected bool tryingBlock;

	protected bool isRolling;

	public bool actions;

	private int canSeeTargetIteration;

	public float lostTargetDistance => maxDetectDistance + distanceToLostTarget;

	public bool isBlocking { get; protected set; }

	public bool isAttacking { get; protected set; }

	public bool isArmed
	{
		get
		{
			if (meleeManager != null)
			{
				if (!(meleeManager.rightWeapon != null))
				{
					if (meleeManager.leftWeapon != null)
					{
						return meleeManager.leftWeapon.meleeType != vMeleeType.OnlyDefense;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public float distanceToAttack
	{
		get
		{
			if ((bool)meleeManager)
			{
				return meleeManager.GetAttackDistance();
			}
			return 1f;
		}
	}

	public bool OnCombatArea
	{
		get
		{
			if (currentTarget.transform == null)
			{
				return false;
			}
			if (Vector3.Distance(new Vector3(0f, base.transform.position.y, 0f), new Vector3(0f, currentTarget.transform.position.y, 0f)) < distanceToAttack && agressiveAtFirstSight && TargetDistance <= strafeDistance)
			{
				return !agent.isOnOffMeshLink;
			}
			return false;
		}
	}

	public bool OnStrafeArea
	{
		get
		{
			if (!canSeeTarget)
			{
				isStrafing = false;
				return false;
			}
			if (currentTarget.transform == null || !agressiveAtFirstSight)
			{
				return false;
			}
			bool num = Vector3.Distance(new Vector3(0f, base.transform.position.y, 0f), new Vector3(0f, currentTarget.transform.position.y, 0f)) < 1.5f;
			if (isStrafing)
			{
				isStrafing = TargetDistance < strafeDistance + 2f;
			}
			else
			{
				isStrafing = OnCombatArea;
			}
			if (!num)
			{
				return false;
			}
			return isStrafing;
		}
	}

	public float TargetDistance
	{
		get
		{
			if (currentTarget.transform != null)
			{
				if (!currentTarget.colliderTarget)
				{
					return Vector3.Distance(base.transform.position, currentTarget.transform.position);
				}
				return Vector3.Distance(_capsuleCollider.bounds.center, currentTarget.colliderTarget.bounds.center);
			}
			return maxDetectDistance + 1f;
		}
	}

	public Transform headTarget
	{
		get
		{
			if (currentTarget.transform != null && currentTarget.transform.GetComponent<Animator>() != null)
			{
				return currentTarget.transform.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
			}
			return null;
		}
	}

	public override void Init()
	{
		base.Init();
		fwd = base.transform.forward;
		destination = base.transform.position;
		agent = GetComponent<NavMeshAgent>();
		agentPath = new NavMeshPath();
		sphereSensor = GetComponentInChildren<vSimpleMeleeAI_SphereSensor>();
		if ((bool)sphereSensor)
		{
			sphereSensor.root = base.transform;
		}
		meleeManager = GetComponent<vMeleeManager>();
		canAttack = true;
		attackCount = 0;
		sideMovement = GetRandonSide();
		destination = base.transform.position;
		_rigidbody = GetComponent<Rigidbody>();
		_rigidbody.useGravity = true;
		_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		agent.updatePosition = false;
		agent.updateRotation = false;
		agent.enabled = false;
		_capsuleCollider = GetComponent<CapsuleCollider>();
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		Collider component = GetComponent<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Physics.IgnoreCollision(component, componentsInChildren[i]);
		}
		healthSlider = GetComponentInChildren<v_SpriteHealth>();
		head = base.animator.GetBoneTransform(HumanBodyBones.Head);
		oldPosition = base.transform.position;
		base.currentHealth = maxHealth;
		startPosition = base.transform.position;
	}

	public bool AgentDone()
	{
		if (!agent.enabled && agent.updatePosition)
		{
			return true;
		}
		if (!agent.pathPending && AgentStopping())
		{
			return agent.updatePosition;
		}
		return false;
	}

	public bool AgentStopping()
	{
		if (!agent.enabled || !agent.isOnNavMesh)
		{
			return true;
		}
		return agent.remainingDistance <= agent.stoppingDistance;
	}

	public int GetRandonSide()
	{
		int num = UnityEngine.Random.Range(-1, 1);
		if (num < 0)
		{
			return -1;
		}
		return 1;
	}

	protected void CheckGroundDistance()
	{
		if (!(_capsuleCollider != null))
		{
			return;
		}
		float num = 10f;
		if (Physics.Raycast(new Ray(base.transform.position + new Vector3(0f, _capsuleCollider.height / 2f, 0f), Vector3.down), out groundHit, _capsuleCollider.height * 0.75f, groundLayer))
		{
			num = base.transform.position.y - groundHit.point.y;
		}
		groundDistance = num;
		if (!actions && !isRolling && groundDistance < 0.3f)
		{
			if (base.currentHealth > 0f && NavMesh.SamplePosition(base.transform.position, out var _, 5f, -1))
			{
				_rigidbody.constraints = (RigidbodyConstraints)122;
				_rigidbody.useGravity = false;
				agent.updatePosition = false;
				agent.enabled = true;
			}
			if (agent.enabled && agent.isOnNavMesh && Vector3.Distance(agent.nextPosition, base.transform.position) <= 0.1f)
			{
				agent.updatePosition = true;
			}
			else if (agent.enabled && agent.isOnNavMesh && !agent.updatePosition)
			{
				agent.nextPosition = base.transform.position;
			}
		}
		if (!agent.isOnNavMesh && groundDistance > 0.3f && !base.ragdolled)
		{
			_rigidbody.useGravity = true;
			_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
			agent.enabled = false;
			agent.updatePosition = false;
		}
	}

	public void CheckAutoCrouch()
	{
		float radius = _capsuleCollider.radius * 0.9f;
		Vector3 origin = base.transform.position + Vector3.up * (_capsuleCollider.height * 0.5f - _capsuleCollider.radius);
		if (Physics.SphereCast(new Ray(origin, Vector3.up), radius, out var _, headDetect - _capsuleCollider.radius * 0.1f, autoCrouchLayer))
		{
			isCrouched = true;
		}
		else
		{
			isCrouched = false;
		}
	}

	public bool onFovAngle()
	{
		if (currentTarget.transform == null)
		{
			return false;
		}
		Vector3 eulerAngle = Quaternion.LookRotation(currentTarget.transform.position - base.transform.position, Vector3.up).eulerAngles - base.transform.eulerAngles;
		fovAngle = eulerAngle.NormalizeAngle().y;
		if (fovAngle < fieldOfView && fovAngle > 0f - fieldOfView)
		{
			return true;
		}
		return false;
	}

	public void CheckTarget()
	{
		if (currentTarget.transform == null || !agressiveAtFirstSight)
		{
			canSeeTarget = false;
			canSeeTargetIteration = 0;
			return;
		}
		if (TargetDistance > maxDetectDistance)
		{
			canSeeTarget = false;
			canSeeTargetIteration = 0;
			return;
		}
		if (currentTarget.colliderTarget == null || currentTarget.colliderTarget.transform != currentTarget.transform)
		{
			currentTarget.colliderTarget = currentTarget.transform.GetComponent<Collider>();
		}
		if (currentTarget.colliderTarget == null)
		{
			canSeeTarget = false;
			canSeeTargetIteration = 0;
			return;
		}
		Vector3 vector = new Vector3(currentTarget.colliderTarget.bounds.center.x, currentTarget.colliderTarget.bounds.max.y, currentTarget.colliderTarget.bounds.center.z);
		Vector3 vector2 = new Vector3(currentTarget.colliderTarget.bounds.center.x, currentTarget.colliderTarget.bounds.min.y, currentTarget.colliderTarget.bounds.center.z);
		float num = Vector3.Distance(vector, vector2) * 0.15f;
		vector.y -= num;
		vector2.y += num;
		if (!onFovAngle() && TargetDistance > minDetectDistance)
		{
			canSeeTarget = false;
			canSeeTargetIteration = 0;
			return;
		}
		if (canSeeTargetIteration == 0 && !Physics.Linecast(head.position, vector, out var hitInfo, obstaclesLayer))
		{
			canSeeTarget = true;
			canSeeTargetIteration = 0;
			return;
		}
		if (canSeeTargetIteration == 1 && !Physics.Linecast(head.position, vector2, out hitInfo, obstaclesLayer))
		{
			canSeeTarget = true;
			canSeeTargetIteration = 0;
			return;
		}
		if (canSeeTargetIteration == 2 && !Physics.Linecast(head.position, currentTarget.colliderTarget.bounds.center, out hitInfo, obstaclesLayer))
		{
			canSeeTarget = true;
			canSeeTargetIteration = 0;
			return;
		}
		canSeeTargetIteration++;
		if (canSeeTargetIteration > 1)
		{
			canSeeTargetIteration = 0;
		}
		canSeeTarget = false;
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
			if (agent != null)
			{
				UnityEngine.Object.Destroy(agent);
			}
			MonoBehaviour[] components = GetComponents<MonoBehaviour>();
			for (int i = 0; i < components.Length; i++)
			{
				UnityEngine.Object.Destroy(components[i]);
			}
		}
	}

	public override void TakeDamage(vDamage damage)
	{
		if (!isRolling && !(base.currentHealth <= 0f) && base.animator.enabled && (damage.ignoreDefense || actions || !CheckChanceToRoll()))
		{
			base.TakeDamage(damage);
		}
	}

	protected override void TriggerDamageReaction(vDamage damage)
	{
		if (!isRolling)
		{
			base.TriggerDamageReaction(damage);
		}
	}

	protected bool CheckChanceToRoll()
	{
		if (isAttacking || actions)
		{
			return false;
		}
		float value = UnityEngine.Random.value;
		if (value < chanceToRoll && value > 0f && currentTarget.transform != null)
		{
			base.animator.SetTrigger("ResetState");
			sideMovement = GetRandonSide();
			rollDirection = new Ray(currentTarget.transform.position, currentTarget.transform.right * sideMovement).direction;
			base.animator.CrossFadeInFixedTime("Roll", 0.1f);
			return true;
		}
		return false;
	}

	protected IEnumerator CheckChanceToBlock(float chance, float timeToEnter)
	{
		tryingBlock = true;
		float value = UnityEngine.Random.value;
		if (value < chance && value > 0f && !isBlocking)
		{
			if (timeToEnter > 0f)
			{
				yield return new WaitForSeconds(timeToEnter);
			}
			isBlocking = ((!(currentTarget.transform == null) && !actions && !isAttacking) ? true : false);
			StartCoroutine(ResetBlock());
			tryingBlock = false;
		}
		else
		{
			tryingBlock = false;
		}
	}

	protected IEnumerator ResetBlock()
	{
		yield return new WaitForSeconds((currentTarget.transform == null) ? 0f : raiseShield);
		isBlocking = false;
	}

	protected virtual void SetAgressive(bool value)
	{
		agressiveAtFirstSight = value;
		onSetAgressive.Invoke(value);
	}

	private int GetDamageResult(int damage, float defenseRate)
	{
		return (int)((float)damage - (float)damage * defenseRate / 100f);
	}

	public override void ResetRagdoll()
	{
		oldPosition = base.transform.position;
		base.ragdolled = false;
		_capsuleCollider.isTrigger = false;
		_rigidbody.isKinematic = false;
		agent.updatePosition = false;
		agent.enabled = true;
	}

	public override void EnableRagdoll()
	{
		agent.enabled = false;
		agent.updatePosition = false;
		base.ragdolled = true;
		_rigidbody.isKinematic = true;
		_capsuleCollider.isTrigger = true;
	}
}
