namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vHasWayPointArea : vStateDecision
{
	public override string categoryName => "Movement/";

	public override string defaultName => "Has WayPointArea?";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		return fsmBehaviour.aiController.waypointArea != null;
	}
}
