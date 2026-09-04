namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vRotateToTargetAction : vStateAction
{
	public bool onlyIfIsInLineOfSight = true;

	public override string categoryName => "Movement/";

	public override string defaultName => "Rotate To Target";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (fsmBehaviour != null && (bool)fsmBehaviour.aiController.currentTarget.transform && (!onlyIfIsInLineOfSight || fsmBehaviour.aiController.targetInLineOfSight))
		{
			fsmBehaviour.aiController.RotateTo(fsmBehaviour.aiController.lastTargetPosition - fsmBehaviour.transform.position);
		}
	}
}
