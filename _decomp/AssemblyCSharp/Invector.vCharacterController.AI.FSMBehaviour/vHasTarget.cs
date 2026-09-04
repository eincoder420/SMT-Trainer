namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vHasTarget : vStateDecision
{
	public override string categoryName => "Detection/";

	public override string defaultName => "Has a CurrentTarget?";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour.aiController == null)
		{
			return false;
		}
		return fsmBehaviour.aiController.currentTarget.transform != null;
	}
}
