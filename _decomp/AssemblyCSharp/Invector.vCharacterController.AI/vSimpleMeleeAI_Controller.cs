using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI;

[vClassHeader("Simple Melee AI", "This is a Simple Melee AI that comes with the MeleeCombat package as a bonus, if you want a more advanced AI check our AI Template")]
public class vSimpleMeleeAI_Controller : vSimpleMeleeAI_Animator, vIMeleeFighter, vIAttackReceiver, vIAttackListener
{
	[vEditorToolbar("Iterations", false, "", false, false)]
	public float stateRoutineIteration = 0.15f;

	public float destinationRoutineIteration = 0.25f;

	public float findTargetIteration = 0.25f;

	public float smoothSpeed = 5f;

	[vEditorToolbar("Events", false, "", false, false)]
	[Header("--- On Change State Events ---")]
	public UnityEvent onIdle;

	public UnityEvent onChase;

	public UnityEvent onPatrol;

	protected AIStates oldState;

	protected float ignorePatrolTimer;

	protected float moveToSpeed;

	protected Vector3 moveToDestination;

	public vICharacter character => this;

	protected override void Start()
	{
		base.Start();
		ignorePatrolTimer = -1f;
		moveToDestination = base.transform.position;
		Init();
		StartCoroutine(StateRoutine());
		StartCoroutine(FindTarget());
		StartCoroutine(DestinationBehaviour());
	}

	protected void FixedUpdate()
	{
		ControlLocomotion();
	}

	public virtual void SetCurrentTarget(Transform target)
	{
		if (target != currentTarget.transform)
		{
			currentTarget.transform = target;
			currentTarget.colliderTarget = target.GetComponent<Collider>();
			currentTarget.character = target.GetComponent<vIHealthController>();
		}
		AddTagsToDetect(target.gameObject.tag);
		sphereSensor.AddTarget(target);
	}

	public virtual void RemoveCurrentTarget()
	{
		if ((bool)currentTarget.transform)
		{
			currentTarget.transform = null;
			currentTarget.colliderTarget = null;
			currentTarget.character = null;
		}
	}

	public virtual void AddTagsToDetect(string tag)
	{
		if (!tagsToDetect.Contains(tag))
		{
			tagsToDetect.Add(tag);
		}
	}

	public virtual void RemoveTagToDetect(string tag)
	{
		if (tagsToDetect.Contains(tag))
		{
			tagsToDetect.Remove(tag);
		}
	}

	protected void SetTarget()
	{
		if (base.currentHealth > 0f && sphereSensor != null)
		{
			if (currentTarget.transform == null || sortTargetFromDistance)
			{
				sphereSensor.CheckTargetsAround(fieldOfView, minDetectDistance, maxDetectDistance, tagsToDetect, layersToDetect, sortTargetFromDistance);
				vIHealthController targetvCharacter = sphereSensor.GetTargetvCharacter();
				if (targetvCharacter != null && targetvCharacter.currentHealth > 0f)
				{
					currentTarget.transform = targetvCharacter.transform;
					currentTarget.character = targetvCharacter;
				}
			}
			if (!CheckTargetIsAlive() || base.TargetDistance > base.lostTargetDistance)
			{
				currentTarget.transform = null;
			}
		}
		else if (base.currentHealth <= 0f)
		{
			destination = base.transform.position;
			currentTarget.transform = null;
		}
	}

	private bool CheckTargetIsAlive()
	{
		if (currentTarget.transform == null || currentTarget.character == null)
		{
			return false;
		}
		if (currentTarget.character.currentHealth > 0f)
		{
			return true;
		}
		return false;
	}

	protected IEnumerator FindTarget()
	{
		while (true)
		{
			yield return new WaitForSeconds(findTargetIteration);
			if (base.currentHealth > 0f)
			{
				SetTarget();
				CheckTarget();
			}
		}
	}

	private void ControlLocomotion()
	{
		if ((AgentDone() && agent.updatePosition) || lockMovement)
		{
			agent.speed = 0f;
			combatMovement = Vector3.zero;
		}
		if (agent.isOnOffMeshLink)
		{
			float magnitude = agent.desiredVelocity.magnitude;
			UpdateAnimator(AgentDone() ? 0f : magnitude, direction);
			return;
		}
		Vector3 vector = ((!agent.enabled) ? (destination - base.transform.position) : (agent.updatePosition ? agent.desiredVelocity : (agent.nextPosition - base.transform.position)));
		if (base.OnStrafeArea)
		{
			Vector3 normalized = base.transform.InverseTransformDirection(vector).normalized;
			combatMovement = Vector3.Lerp(combatMovement, normalized, 2f * Time.deltaTime);
			UpdateAnimator(AgentDone() ? 0f : combatMovement.z, combatMovement.x);
		}
		else
		{
			float magnitude2 = vector.magnitude;
			combatMovement = Vector3.zero;
			UpdateAnimator(AgentDone() ? 0f : magnitude2, 0f);
		}
	}

	private Vector3 AgentDirection()
	{
		Vector3 b = ((!AgentDone()) ? agent.desiredVelocity : ((currentTarget.transform != null && base.OnStrafeArea && canSeeTarget) ? (new Vector3(destination.x, base.transform.position.y, destination.z) - base.transform.position) : base.transform.forward));
		fwd = Vector3.Lerp(fwd, b, 20f * Time.deltaTime);
		return fwd;
	}

	protected virtual IEnumerator DestinationBehaviour()
	{
		while (true)
		{
			yield return new WaitForSeconds(destinationRoutineIteration);
			CheckGroundDistance();
			if (agent.updatePosition)
			{
				UpdateDestination(destination);
			}
		}
	}

	protected virtual void UpdateDestination(Vector3 position)
	{
		if (agent.isOnNavMesh)
		{
			agent.SetDestination(position);
		}
		if (agent.enabled && agent.hasPath && drawAgentPath)
		{
			Debug.DrawLine(base.transform.position, position, Color.red, 0.5f);
			Vector3 start = base.transform.position;
			for (int i = 0; i < agent.path.corners.Length; i++)
			{
				Vector3 vector = agent.path.corners[i];
				Debug.DrawLine(start, vector, Color.green, 0.5f);
				start = vector;
			}
		}
	}

	protected void CheckIsOnNavMesh()
	{
		if (!agent.isOnNavMesh && agent.enabled && !base.ragdolled)
		{
			Debug.LogWarning("Missing NavMesh Bake, character will die - Please Bake your navmesh again!");
			base.currentHealth = 0f;
		}
	}

	protected IEnumerator StateRoutine()
	{
		while (base.enabled)
		{
			CheckIsOnNavMesh();
			CheckAutoCrouch();
			yield return new WaitForSeconds(stateRoutineIteration);
			if (lockMovement)
			{
				continue;
			}
			switch (currentState)
			{
			case AIStates.Idle:
				if (currentState != oldState)
				{
					onIdle.Invoke();
					oldState = currentState;
				}
				yield return StartCoroutine(Idle());
				break;
			case AIStates.Chase:
				if (currentState != oldState)
				{
					onChase.Invoke();
					oldState = currentState;
				}
				yield return StartCoroutine(Chase());
				break;
			case AIStates.PatrolSubPoints:
				if (currentState != oldState)
				{
					onPatrol.Invoke();
					oldState = currentState;
				}
				yield return StartCoroutine(PatrolSubPoints());
				break;
			case AIStates.PatrolWaypoints:
				if (currentState != oldState)
				{
					onPatrol.Invoke();
					oldState = currentState;
				}
				yield return StartCoroutine(PatrolWaypoints());
				break;
			case AIStates.Wander:
				if (currentState != oldState)
				{
					onPatrol.Invoke();
					oldState = currentState;
				}
				yield return StartCoroutine(Wander());
				break;
			}
		}
	}

	protected IEnumerator Idle()
	{
		while (base.currentHealth <= 0f)
		{
			yield return null;
		}
		if (canSeeTarget)
		{
			currentState = AIStates.Chase;
		}
		else
		{
			agent.speed = Mathf.Lerp(agent.speed, 0f, smoothSpeed * Time.deltaTime);
		}
	}

	protected IEnumerator Chase()
	{
		while (base.currentHealth <= 0f)
		{
			yield return null;
		}
		agent.speed = Mathf.Lerp(agent.speed, chaseSpeed, smoothSpeed * Time.deltaTime);
		agent.stoppingDistance = chaseStopDistance;
		if (!base.isBlocking && !tryingBlock)
		{
			StartCoroutine(CheckChanceToBlock(chanceToBlockInStrafe, lowerShield));
		}
		if (currentTarget.transform == null || !agressiveAtFirstSight)
		{
			currentState = AIStates.PatrolWaypoints;
		}
		if (base.TargetDistance <= base.distanceToAttack && meleeManager != null && canAttack && !actions)
		{
			canAttack = false;
			yield return StartCoroutine(MeleeAttackRotine());
		}
		if (attackCount <= 0 && !inResetAttack && !base.isAttacking)
		{
			StartCoroutine(ResetAttackCount());
			yield return null;
		}
		if (base.OnStrafeArea && strafeSideways)
		{
			if (strafeSwapeFrequency <= 0f)
			{
				sideMovement = GetRandonSide();
				strafeSwapeFrequency = Random.Range(minStrafeSwape, maxStrafeSwape);
			}
			else
			{
				strafeSwapeFrequency -= Time.deltaTime;
			}
			fwdMovement = ((!(base.TargetDistance < base.distanceToAttack)) ? ((base.TargetDistance > base.distanceToAttack) ? 1 : 0) : (strafeBackward ? (-1) : 0));
			Vector3 vector = base.transform.right * sideMovement + base.transform.forward * fwdMovement;
			Ray ray = new Ray(new Vector3(base.transform.position.x, (currentTarget.transform != null) ? currentTarget.transform.position.y : base.transform.position.y, base.transform.position.z), vector);
			if (base.TargetDistance < strafeDistance - 0.5f)
			{
				destination = (base.OnStrafeArea ? ray.GetPoint(agent.stoppingDistance + 0.5f) : currentTarget.transform.position);
			}
			else if (currentTarget.transform != null)
			{
				destination = currentTarget.transform.position;
			}
		}
		else if (!base.OnStrafeArea && currentTarget.transform != null)
		{
			destination = currentTarget.transform.position;
		}
		else
		{
			fwdMovement = ((!(base.TargetDistance < base.distanceToAttack)) ? ((base.TargetDistance > base.distanceToAttack) ? 1 : 0) : (strafeBackward ? (-1) : 0));
			Ray ray2 = new Ray(base.transform.position, base.transform.forward * fwdMovement);
			if (base.TargetDistance < strafeDistance - 0.5f)
			{
				destination = ((fwdMovement != 0f) ? ray2.GetPoint(agent.stoppingDistance + ((fwdMovement > 0f) ? base.TargetDistance : 1f)) : base.transform.position);
			}
			else if (currentTarget.transform != null)
			{
				destination = currentTarget.transform.position;
			}
		}
	}

	protected IEnumerator PatrolSubPoints()
	{
		while (!agent.enabled)
		{
			yield return null;
		}
		if ((bool)targetWaypoint)
		{
			if (targetPatrolPoint == null || !targetPatrolPoint.isValid)
			{
				targetPatrolPoint = GetPatrolPoint(targetWaypoint);
			}
			else
			{
				agent.speed = Mathf.Lerp(agent.speed, (agent.hasPath && targetPatrolPoint.isValid) ? patrolSpeed : 0f, smoothSpeed * Time.deltaTime);
				agent.stoppingDistance = patrollingStopDistance;
				destination = (targetPatrolPoint.isValid ? targetPatrolPoint.position : base.transform.position);
				if (Vector3.Distance(base.transform.position, destination) < targetPatrolPoint.areaRadius && targetPatrolPoint.CanEnter(base.transform) && !targetPatrolPoint.IsOnWay(base.transform))
				{
					targetPatrolPoint.Enter(base.transform);
					wait = Time.time + targetPatrolPoint.timeToStay;
					visitedPatrolPoint.Add(targetPatrolPoint);
				}
				else if (Vector3.Distance(base.transform.position, destination) < targetPatrolPoint.areaRadius && (!targetPatrolPoint.CanEnter(base.transform) || !targetPatrolPoint.isValid))
				{
					targetPatrolPoint = GetPatrolPoint(targetWaypoint);
				}
				if (targetPatrolPoint != null && targetPatrolPoint.IsOnWay(base.transform) && Vector3.Distance(base.transform.position, destination) < distanceToChangeWaypoint && (wait < Time.time || !targetPatrolPoint.isValid))
				{
					wait = 0f;
					if (visitedPatrolPoint.Count == pathArea.GetValidSubPoints(targetWaypoint).Count)
					{
						currentState = AIStates.PatrolWaypoints;
						targetWaypoint.Exit(base.transform);
						targetPatrolPoint.Exit(base.transform);
						targetWaypoint = null;
						targetPatrolPoint = null;
						visitedPatrolPoint.Clear();
					}
					else
					{
						targetPatrolPoint.Exit(base.transform);
						targetPatrolPoint = GetPatrolPoint(targetWaypoint);
					}
				}
			}
		}
		if (canSeeTarget)
		{
			currentState = AIStates.Chase;
		}
	}

	protected IEnumerator PatrolWaypoints()
	{
		while (!agent.enabled)
		{
			yield return null;
		}
		if (pathArea != null && pathArea.waypoints.Count > 0)
		{
			if (targetWaypoint == null || !targetWaypoint.isValid)
			{
				targetWaypoint = GetWaypoint();
			}
			else
			{
				agent.speed = Mathf.Lerp(agent.speed, (agent.hasPath && targetWaypoint.isValid) ? patrolSpeed : 0f, smoothSpeed * Time.deltaTime);
				agent.stoppingDistance = patrollingStopDistance;
				destination = targetWaypoint.position;
				if (Vector3.Distance(base.transform.position, destination) < targetWaypoint.areaRadius && targetWaypoint.CanEnter(base.transform) && !targetWaypoint.IsOnWay(base.transform))
				{
					targetWaypoint.Enter(base.transform);
					wait = Time.time + targetWaypoint.timeToStay;
				}
				else if (Vector3.Distance(base.transform.position, destination) < targetWaypoint.areaRadius && (!targetWaypoint.CanEnter(base.transform) || !targetWaypoint.isValid))
				{
					targetWaypoint = GetWaypoint();
				}
				if (targetWaypoint != null && targetWaypoint.IsOnWay(base.transform) && Vector3.Distance(base.transform.position, destination) < distanceToChangeWaypoint && (wait < Time.time || !targetWaypoint.isValid))
				{
					wait = 0f;
					if (targetWaypoint.subPoints.Count > 0)
					{
						currentState = AIStates.PatrolSubPoints;
					}
					else
					{
						targetWaypoint.Exit(base.transform);
						visitedPatrolPoint.Clear();
						targetWaypoint = GetWaypoint();
					}
				}
			}
		}
		else if (ignorePatrolTimer < Time.time)
		{
			switch (patrolWithoutAreaStyle)
			{
			case AIPatrolWithOutAreaStyle.GoToStartPoint:
				yield return StartCoroutine(GoToStartingPoint());
				break;
			case AIPatrolWithOutAreaStyle.Idle:
				currentState = AIStates.Idle;
				break;
			case AIPatrolWithOutAreaStyle.Wander:
				currentState = AIStates.Wander;
				break;
			}
		}
		else if (ignorePatrolTimer > Time.time)
		{
			yield return StartCoroutine(GoToDestionation());
		}
		if (canSeeTarget)
		{
			currentState = AIStates.Chase;
		}
	}

	protected virtual IEnumerator GoToDestionation()
	{
		yield return null;
		agent.speed = Mathf.Lerp(agent.speed, moveToSpeed, smoothSpeed * Time.deltaTime);
		agent.stoppingDistance = patrollingStopDistance;
		destination = moveToDestination;
	}

	protected virtual IEnumerator GoToStartingPoint()
	{
		yield return null;
		agent.speed = Mathf.Lerp(agent.speed, patrolSpeed, smoothSpeed * Time.deltaTime);
		agent.stoppingDistance = patrollingStopDistance;
		destination = startPosition;
	}

	protected virtual IEnumerator Wander()
	{
		agent.speed = Mathf.Lerp(agent.speed, wanderSpeed, smoothSpeed * Time.deltaTime);
		do
		{
			yield return null;
			destination = base.transform.position + Quaternion.AngleAxis(Random.Range(-120, 120), base.transform.up) * base.transform.forward * (patrollingStopDistance + 4f);
		}
		while (agent.enabled && agent.isOnNavMesh && agent.remainingDistance <= patrollingStopDistance);
		if (canSeeTarget)
		{
			currentState = AIStates.Chase;
		}
	}

	private vWaypoint GetWaypoint()
	{
		List<vWaypoint> validPoints = pathArea.GetValidPoints();
		if (randomWaypoints)
		{
			currentWaypoint = randomWaypoint.Next(validPoints.Count);
		}
		else
		{
			currentWaypoint++;
		}
		if (currentWaypoint >= validPoints.Count)
		{
			currentWaypoint = 0;
		}
		if (validPoints.Count == 0)
		{
			agent.isStopped = true;
			return null;
		}
		if (visitedWaypoint.Count == validPoints.Count)
		{
			visitedWaypoint.Clear();
		}
		if (visitedWaypoint.Contains(validPoints[currentWaypoint]))
		{
			return null;
		}
		agent.isStopped = false;
		return validPoints[currentWaypoint];
	}

	private vPoint GetPatrolPoint(vWaypoint waypoint)
	{
		List<vPoint> validSubPoints = pathArea.GetValidSubPoints(waypoint);
		if (waypoint.randomPatrolPoint)
		{
			currentPatrolPoint = randomPatrolPoint.Next(validSubPoints.Count);
		}
		else
		{
			currentPatrolPoint++;
		}
		if (currentPatrolPoint >= validSubPoints.Count)
		{
			currentPatrolPoint = 0;
		}
		if (validSubPoints.Count == 0)
		{
			agent.isStopped = true;
			return null;
		}
		if (visitedPatrolPoint.Contains(validSubPoints[currentPatrolPoint]))
		{
			return null;
		}
		agent.isStopped = false;
		return validSubPoints[currentPatrolPoint];
	}

	protected IEnumerator MeleeAttackRotine()
	{
		if (!base.isAttacking && !actions && attackCount > 0 && !lockMovement && !isRolling)
		{
			sideMovement = GetRandonSide();
			agent.stoppingDistance = base.distanceToAttack;
			attackCount--;
			MeleeAttack();
			yield return null;
		}
	}

	public void FinishAttack()
	{
		canAttack = true;
	}

	private IEnumerator ResetAttackCount()
	{
		inResetAttack = true;
		canAttack = false;
		float seconds;
		if (firstAttack)
		{
			firstAttack = false;
			seconds = firstAttackDelay;
		}
		else
		{
			seconds = Random.Range(minTimeToAttack, maxTimeToAttack);
		}
		yield return new WaitForSeconds(seconds);
		attackCount = (randomAttackCount ? Random.Range(1, maxAttackCount + 1) : maxAttackCount);
		canAttack = true;
		inResetAttack = false;
	}

	public void OnEnableAttack()
	{
		base.isAttacking = true;
	}

	public void OnDisableAttack()
	{
		base.isAttacking = false;
		canAttack = true;
	}

	public void ResetAttackTriggers()
	{
		base.animator.ResetTrigger("WeakAttack");
	}

	public void BreakAttack(int breakAtkID)
	{
		ResetAttackCount();
		ResetAttackTriggers();
		OnRecoil(breakAtkID);
	}

	public void OnRecoil(int recoilID)
	{
		TriggerRecoil(recoilID);
	}

	public void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
	{
		StartCoroutine(CheckChanceToBlock(chanceToBlockAttack, 0f));
		Vector3 attackPoint = ((attacker != null && attacker.character != null) ? attacker.character.transform.position : damage.hitPosition);
		if (!damage.ignoreDefense && base.isBlocking && meleeManager != null && meleeManager.CanBlockAttack(attackPoint))
		{
			int num = ((meleeManager != null) ? meleeManager.GetDefenseRate() : 0);
			if (num > 0)
			{
				damage.ReduceDamage(num);
			}
			if (attacker != null && meleeManager != null && meleeManager.CanBreakAttack())
			{
				attacker.OnRecoil(meleeManager.GetDefenseRecoilID());
			}
			meleeManager.OnDefense();
		}
		if (!passiveToDamage && damage.sender != null)
		{
			SetCurrentTarget(damage.sender);
			currentState = AIStates.Chase;
		}
		damage.hitReaction = !base.isBlocking;
		if (!passiveToDamage)
		{
			SetAgressive(value: true);
		}
		TakeDamage(damage);
	}

	public virtual void MoveTo(Vector3 position, float moveToSpeed = 1f, float ignorePatrolTimer = 2f)
	{
		moveToDestination = position;
		currentState = AIStates.PatrolWaypoints;
		this.moveToSpeed = moveToSpeed;
		this.ignorePatrolTimer = Time.time + ignorePatrolTimer;
	}

	[SpecialName]
	Transform vIMeleeFighter.get_transform()
	{
		return base.transform;
	}

	[SpecialName]
	GameObject vIMeleeFighter.get_gameObject()
	{
		return base.gameObject;
	}
}
