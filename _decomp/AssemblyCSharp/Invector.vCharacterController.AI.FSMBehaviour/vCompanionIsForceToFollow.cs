namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vCompanionIsForceToFollow : vStateDecision
{
	public override string categoryName => "Companion/";

	public override string defaultName => "Force Companion To Follow";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour.aiController.HasComponent<vAICompanion>())
		{
			return fsmBehaviour.aiController.GetAIComponent<vAICompanion>().forceFollow;
		}
		return true;
	}
}
