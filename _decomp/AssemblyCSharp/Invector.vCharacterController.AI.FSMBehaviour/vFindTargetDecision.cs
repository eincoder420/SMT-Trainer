namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vFindTargetDecision : vStateDecision
{
	public override string categoryName => "Detection/";

	public override string defaultName => "FindTarget Decision";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour.aiController != null)
		{
			fsmBehaviour.aiController.FindTarget();
			return fsmBehaviour.aiController.currentTarget.transform != null;
		}
		return true;
	}
}
