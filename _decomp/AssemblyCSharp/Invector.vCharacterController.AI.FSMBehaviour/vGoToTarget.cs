using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vGoToTarget : vStateAction
{
	public bool useStrafeMovement;

	[vHideInInspector("useStrafeMovement", false)]
	public bool updateRotationInStrafe;

	public vAIMovementSpeed speed = vAIMovementSpeed.Walking;

	public override string categoryName => "Movement/";

	public override string defaultName => "Chase Target";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (fsmBehaviour.aiController == null)
		{
			return;
		}
		if (executionType == vFSMComponentExecutionType.OnStateEnter)
		{
			fsmBehaviour.aiController.ForceUpdatePath(2f);
		}
		if (useStrafeMovement)
		{
			if (updateRotationInStrafe)
			{
				Vector3 forwardDirection = (fsmBehaviour.aiController.targetInLineOfSight ? (fsmBehaviour.aiController.lastTargetPosition - fsmBehaviour.transform.position) : fsmBehaviour.aiController.desiredVelocity);
				fsmBehaviour.aiController.StrafeMoveTo(fsmBehaviour.aiController.lastTargetPosition, forwardDirection, speed);
			}
			else
			{
				fsmBehaviour.aiController.StrafeMoveTo(fsmBehaviour.aiController.lastTargetPosition, speed);
			}
		}
		else
		{
			fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.lastTargetPosition, speed);
		}
	}
}
