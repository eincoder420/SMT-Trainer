namespace Invector.vCharacterController.AI.FSMBehaviour;

public class Check_Already_Seen : vStateDecision
{
	public override string categoryName => "MyCustomDecisions/";

	public override string defaultName => "Check_Already_Seen";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		return fsmBehaviour.gameObject.GetComponent<NPC_generator>().Already_Seen;
	}
}
