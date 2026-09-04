namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vFSMChangeBehaviour : vStateAction
{
	public vFSMBehaviour newBehaviour;

	public override string categoryName => "Controller/";

	public override string defaultName => "Change FSM Behaviour";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		fsmBehaviour.ChangeBehaviour(newBehaviour);
	}
}
