namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vTargetLost : vStateDecision
{
	public override string categoryName => "Detection/";

	public override string defaultName => "Lost the Target?";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour != null && fsmBehaviour.aiController != null && (bool)fsmBehaviour.aiController.currentTarget.transform)
		{
			return fsmBehaviour.aiController.currentTarget.isLost;
		}
		return true;
	}
}
