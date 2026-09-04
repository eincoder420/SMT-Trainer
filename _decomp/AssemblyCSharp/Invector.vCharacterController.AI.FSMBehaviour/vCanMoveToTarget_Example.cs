namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vCanMoveToTarget_Example : vStateDecision
{
	public override string categoryName => "Custom Example/";

	public override string defaultName => "Can MoveToTarget Example";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		return fsmBehaviour.aiController._moveToTarget;
	}
}
