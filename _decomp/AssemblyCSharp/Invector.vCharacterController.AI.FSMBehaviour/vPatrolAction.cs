using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vPatrolAction : vStateAction
{
	public bool debugMode;

	public vAIMovementSpeed patrolSpeed = vAIMovementSpeed.Walking;

	public bool patrolInStrafe;

	[vHideInInspector("patrolInStrafe", false)]
	public bool updateRotationInStrafe = true;

	public override string categoryName => "Movement/";

	public override string defaultName => "Patrol";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		DoPatrolWaypoints(fsmBehaviour);
	}

	protected virtual void DoPatrolWaypoints(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour == null || fsmBehaviour.aiController.isDead)
		{
			return;
		}
		if (fsmBehaviour.aiController.waypointArea != null && fsmBehaviour.aiController.waypointArea.waypoints.Count > 0)
		{
			if (fsmBehaviour.aiController.targetWaypoint == null || !fsmBehaviour.aiController.targetWaypoint.isValid)
			{
				fsmBehaviour.aiController.NextWayPoint();
				return;
			}
			if (Vector3.Distance(fsmBehaviour.aiController.transform.position, fsmBehaviour.aiController.targetWaypoint.position) < fsmBehaviour.aiController.stopingDistance + fsmBehaviour.aiController.targetWaypoint.areaRadius + fsmBehaviour.aiController.changeWaypointDistance && fsmBehaviour.aiController.targetWaypoint.CanEnter(fsmBehaviour.aiController.transform) && !fsmBehaviour.aiController.targetWaypoint.IsOnWay(fsmBehaviour.aiController.transform))
			{
				fsmBehaviour.aiController.targetWaypoint.Enter(fsmBehaviour.aiController.transform);
			}
			else if (Vector3.Distance(fsmBehaviour.aiController.transform.position, fsmBehaviour.aiController.targetWaypoint.position) < fsmBehaviour.aiController.stopingDistance + fsmBehaviour.aiController.targetWaypoint.areaRadius && (!fsmBehaviour.aiController.targetWaypoint.CanEnter(fsmBehaviour.aiController.transform) || !fsmBehaviour.aiController.targetWaypoint.isValid))
			{
				fsmBehaviour.aiController.NextWayPoint();
			}
			if (fsmBehaviour.aiController.targetWaypoint != null && fsmBehaviour.aiController.targetWaypoint.IsOnWay(fsmBehaviour.aiController.transform) && Vector3.Distance(fsmBehaviour.aiController.transform.position, fsmBehaviour.aiController.targetWaypoint.position) <= fsmBehaviour.aiController.targetWaypoint.areaRadius + fsmBehaviour.aiController.changeWaypointDistance)
			{
				if (!(fsmBehaviour.aiController.remainingDistance <= fsmBehaviour.aiController.stopingDistance + fsmBehaviour.aiController.changeWaypointDistance) && !fsmBehaviour.aiController.isInDestination)
				{
					return;
				}
				float timer = fsmBehaviour.GetTimer("Patrol");
				if (timer >= fsmBehaviour.aiController.targetWaypoint.timeToStay || !fsmBehaviour.aiController.targetWaypoint.isValid)
				{
					fsmBehaviour.aiController.targetWaypoint.Exit(fsmBehaviour.aiController.transform);
					fsmBehaviour.aiController.visitedWaypoints.Clear();
					fsmBehaviour.aiController.NextWayPoint();
					if (debugMode)
					{
						Debug.Log("Sort new Waypoint");
					}
					fsmBehaviour.aiController.Stop();
					fsmBehaviour.SetTimer("Patrol", 0f);
				}
				else if (timer < fsmBehaviour.aiController.targetWaypoint.timeToStay)
				{
					if (debugMode)
					{
						Debug.Log("Stay");
					}
					if (fsmBehaviour.aiController.targetWaypoint.rotateTo)
					{
						fsmBehaviour.aiController.Stop();
						fsmBehaviour.aiController.RotateTo(fsmBehaviour.aiController.targetWaypoint.transform.forward);
					}
					else
					{
						fsmBehaviour.aiController.Stop();
					}
					fsmBehaviour.SetTimer("Patrol", timer + Time.deltaTime);
				}
				return;
			}
			if (patrolInStrafe)
			{
				if (updateRotationInStrafe)
				{
					fsmBehaviour.aiController.StrafeMoveTo(fsmBehaviour.aiController.targetWaypoint.position, fsmBehaviour.aiController.desiredVelocity, patrolSpeed);
				}
				else
				{
					fsmBehaviour.aiController.StrafeMoveTo(fsmBehaviour.aiController.targetWaypoint.position, patrolSpeed);
				}
			}
			else
			{
				fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.targetWaypoint.position, patrolSpeed);
			}
			if (debugMode)
			{
				Debug.Log("Go to new Waypoint");
			}
		}
		else if (fsmBehaviour.aiController.selfStartingPoint)
		{
			if (fsmBehaviour.debugMode)
			{
				fsmBehaviour.SendDebug("MoveTo SelfStartPosition", this);
			}
			if (patrolInStrafe)
			{
				if (updateRotationInStrafe)
				{
					fsmBehaviour.aiController.StrafeMoveTo(fsmBehaviour.aiController.selfStartPosition, fsmBehaviour.aiController.desiredVelocity, patrolSpeed);
				}
				else
				{
					fsmBehaviour.aiController.StrafeMoveTo(fsmBehaviour.aiController.selfStartPosition, patrolSpeed);
				}
			}
			else
			{
				fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.selfStartPosition, patrolSpeed);
			}
		}
		else if (fsmBehaviour.aiController.customStartPoint)
		{
			if (fsmBehaviour.debugMode)
			{
				fsmBehaviour.SendDebug("MoveTo CustomStartPosition", this);
			}
			if (patrolInStrafe)
			{
				if (updateRotationInStrafe)
				{
					fsmBehaviour.aiController.StrafeMoveTo(fsmBehaviour.aiController.customStartPosition, fsmBehaviour.aiController.desiredVelocity, patrolSpeed);
				}
				else
				{
					fsmBehaviour.aiController.StrafeMoveTo(fsmBehaviour.aiController.customStartPosition, patrolSpeed);
				}
			}
			else
			{
				fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.customStartPosition, patrolSpeed);
			}
		}
		else
		{
			if (fsmBehaviour.debugMode)
			{
				fsmBehaviour.SendDebug("Stop Patrolling", this);
			}
			fsmBehaviour.aiController.Stop();
		}
	}
}
