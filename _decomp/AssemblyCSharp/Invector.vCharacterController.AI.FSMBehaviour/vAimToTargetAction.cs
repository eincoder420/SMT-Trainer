using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAimToTargetAction : vStateAction
{
	public bool onlyIfCanSeeTarget;

	[Tooltip("This action will check if aim is enabled using vAnimatorTag with  (Upperbody Pose) tag on layer setted")]
	public int aimLayer = 4;

	public override string categoryName => "Combat/";

	public override string defaultName => "Aim To Target";

	public vAimToTargetAction()
	{
		executionType = vFSMComponentExecutionType.OnStateUpdate;
	}

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (fsmBehaviour.aiController is vIControlAIShooter)
		{
			ControlAttack(fsmBehaviour, fsmBehaviour.aiController as vIControlAIShooter, executionType);
		}
	}

	protected virtual void ControlAttack(vIFSMBehaviourController fsmBehaviour, vIControlAIShooter combat, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (!onlyIfCanSeeTarget || combat.targetInLineOfSight)
		{
			combat.AimToTarget(0.1f);
		}
	}
}
