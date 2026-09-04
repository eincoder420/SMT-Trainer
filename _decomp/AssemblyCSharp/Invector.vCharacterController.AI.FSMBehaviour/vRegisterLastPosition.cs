namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vRegisterLastPosition : vStateAction
{
	public override string categoryName => "Movement/";

	public override string defaultName => "Set Start Position";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		fsmBehaviour.aiController.selfStartPosition = fsmBehaviour.aiController.transform.position;
	}
}
