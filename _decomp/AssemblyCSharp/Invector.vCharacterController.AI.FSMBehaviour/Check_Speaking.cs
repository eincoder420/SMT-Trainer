namespace Invector.vCharacterController.AI.FSMBehaviour;

public class Check_Speaking : vStateDecision
{
	public override string categoryName => "MyCustomDecisions/";

	public override string defaultName => "Check_Speaking";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		return fsmBehaviour.gameObject.GetComponent<NPC_generator>().Speaking;
	}
}
