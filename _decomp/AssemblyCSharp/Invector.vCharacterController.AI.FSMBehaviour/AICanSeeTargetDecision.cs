namespace Invector.vCharacterController.AI.FSMBehaviour;

public class AICanSeeTargetDecision : vStateDecision
{
	public override string categoryName => "Detection/";

	public override string defaultName => "Can See Target";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		return CanSeeTarget(fsmBehaviour);
	}

	protected virtual bool CanSeeTarget(vIFSMBehaviourController fsmBehaviour)
	{
		return fsmBehaviour.aiController.targetInLineOfSight;
	}
}
