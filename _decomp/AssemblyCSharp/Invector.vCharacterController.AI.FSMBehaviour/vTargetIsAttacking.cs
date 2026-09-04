namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vTargetIsAttacking : vStateDecision
{
	public override string categoryName => "Detection/";

	public override string defaultName => "Target Is Attacking?";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour.aiController == null || !fsmBehaviour.aiController.currentTarget.isFighter)
		{
			return false;
		}
		return fsmBehaviour.aiController.currentTarget.isAttacking;
	}
}
