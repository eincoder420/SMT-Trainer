namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAIFindTargetAction : vStateAction
{
	public bool checkForObstacles = true;

	public override string categoryName => "Detection/";

	public override string defaultName => "Find Target";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		FindTarget(fsmBehaviour);
	}

	protected virtual void FindTarget(vIFSMBehaviourController fsmBehaviour)
	{
		fsmBehaviour?.aiController.FindTarget(checkForObstacles);
	}
}
