namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAIIsInDestination : vStateDecision
{
	public override string categoryName => "Movement/";

	public override string defaultName => "Is In Destination";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		return fsmBehaviour.aiController.isInDestination;
	}
}
