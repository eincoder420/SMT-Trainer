namespace Invector.vCharacterController.AI.FSMBehaviour;

public class Check_Is_Girl : vStateDecision
{
	public override string categoryName => "MyCustomDecisions/";

	public override string defaultName => "Check_Is_Girl";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		return fsmBehaviour.gameObject.GetComponent<NPC_generator>().Girl;
	}
}
