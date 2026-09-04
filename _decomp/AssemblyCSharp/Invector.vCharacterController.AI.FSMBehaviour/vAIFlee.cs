using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAIFlee : vStateAction
{
	public vAIMovementSpeed fleeSpeed = vAIMovementSpeed.Running;

	public float fleeDistance = 10f;

	public bool debugMode;

	public bool debugFleeDirection;

	public override string categoryName => "Movement/";

	public override string defaultName => "Flee";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		switch (executionType)
		{
		case vFSMComponentExecutionType.OnStateUpdate:
			Flee(fsmBehaviour);
			break;
		case vFSMComponentExecutionType.OnStateEnter:
			Flee(fsmBehaviour);
			fsmBehaviour.aiController.ForceUpdatePath();
			break;
		}
	}

	protected virtual void Flee(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour != null && fsmBehaviour.aiController.receivedDamage.isValid && fsmBehaviour.aiController.receivedDamage.lastSender != null)
		{
			if (!(fsmBehaviour.aiController.remainingDistance < 1f))
			{
				return;
			}
			for (int i = 1; i < 36; i++)
			{
				if (InTimer(fsmBehaviour, 1f, "FleeTimer"))
				{
					if (Vector3.Distance(fsmBehaviour.aiController.targetDestination, fsmBehaviour.aiController.transform.position) < fleeDistance * 0.25f + fsmBehaviour.aiController.stopingDistance || fsmBehaviour.aiController.isInDestination)
					{
						if (debugMode)
						{
							Debug.Log("Fleeing from damage sender");
						}
						Vector3 position = fsmBehaviour.aiController.receivedDamage.lastSender.position;
						Vector3 vector = fsmBehaviour.aiController.transform.position - position;
						vector = Quaternion.Euler(0f, Random.Range(-(5 * i), 5 * i), 0f) * vector.normalized;
						vector.y = 0f;
						if (debugFleeDirection)
						{
							Debug.DrawRay(fsmBehaviour.aiController.transform.position, vector * fleeDistance, Color.yellow, 10f);
						}
						fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.transform.position + vector * fleeDistance, fleeSpeed);
						fsmBehaviour.aiController.ForceUpdatePath();
					}
				}
				else
				{
					i--;
				}
			}
		}
		else if (fsmBehaviour != null && fsmBehaviour.aiController.currentTarget.transform != null)
		{
			for (int j = 1; j < 36; j++)
			{
				if (InTimer(fsmBehaviour, 1f, "FleeTimer"))
				{
					if (Vector3.Distance(fsmBehaviour.aiController.targetDestination, fsmBehaviour.aiController.transform.position) < fleeDistance * 0.25f + fsmBehaviour.aiController.stopingDistance || fsmBehaviour.aiController.isInDestination)
					{
						if (debugMode)
						{
							Debug.Log("Fleeing from a target");
						}
						Vector3 position2 = fsmBehaviour.aiController.currentTarget.transform.position;
						Vector3 vector2 = fsmBehaviour.aiController.transform.position - position2;
						vector2 = Quaternion.Euler(0f, Random.Range(-(5 * j), 5 * j), 0f) * vector2.normalized;
						if (debugFleeDirection)
						{
							Debug.DrawRay(fsmBehaviour.aiController.transform.position, vector2 * fleeDistance, Color.yellow, 10f);
						}
						fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.transform.position + vector2 * fleeDistance, fleeSpeed);
						fsmBehaviour.aiController.ForceUpdatePath();
					}
				}
				else
				{
					j--;
				}
			}
		}
		else
		{
			if (fsmBehaviour == null)
			{
				return;
			}
			for (int k = 1; k < 36; k++)
			{
				if (InTimer(fsmBehaviour, 1f, "FleeTimer"))
				{
					if (Vector3.Distance(fsmBehaviour.aiController.targetDestination, fsmBehaviour.aiController.transform.position) < fleeDistance * 0.25f + fsmBehaviour.aiController.stopingDistance || fsmBehaviour.aiController.isInDestination)
					{
						if (debugMode)
						{
							Debug.Log("Fleeing without target or damage sender");
						}
						Vector3 forward = fsmBehaviour.aiController.transform.forward;
						forward = Quaternion.Euler(0f, Random.Range(-(10 * k), 10 * k), 0f) * forward.normalized;
						if (debugFleeDirection)
						{
							Debug.DrawRay(fsmBehaviour.aiController.transform.position, forward * fleeDistance, Color.yellow, 10f);
						}
						fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.transform.position + forward * fleeDistance, fleeSpeed);
						fsmBehaviour.aiController.ForceUpdatePath();
					}
				}
				else
				{
					k--;
				}
			}
		}
	}
}
