using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vWanderAction : vStateAction
{
	public bool wanderInStrafe;

	public vAIMovementSpeed speed = vAIMovementSpeed.Walking;

	public override string categoryName => "Movement/";

	public override string defaultName => "Wander";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		DoWander(fsmBehaviour);
	}

	protected virtual void DoWander(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour != null && !fsmBehaviour.aiController.isDead && (fsmBehaviour.aiController.isInDestination || Vector3.Distance(fsmBehaviour.aiController.targetDestination, fsmBehaviour.aiController.transform.position) <= 0.5f + fsmBehaviour.aiController.stopingDistance))
		{
			Vector3 vector = Quaternion.AngleAxis(Random.Range(-90f, 90f), Vector3.up) * fsmBehaviour.aiController.transform.forward;
			Vector3 destination = fsmBehaviour.aiController.transform.position + vector.normalized * (Random.Range(1f, 4f) + fsmBehaviour.aiController.stopingDistance);
			if (wanderInStrafe)
			{
				fsmBehaviour.aiController.StrafeMoveTo(destination, vector.normalized, speed);
			}
			else
			{
				fsmBehaviour.aiController.MoveTo(destination, speed);
			}
		}
	}
}
