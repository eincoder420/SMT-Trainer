namespace Invector.vCharacterController.AI.FSMBehaviour;

public class Check_Energy_Cop : vStateDecision
{
	private NPC_generator generator;

	public override string categoryName => "MyCustomDecisions/";

	public override string defaultName => "Check_Energy_Cop";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (!generator)
		{
			generator = fsmBehaviour.gameObject.GetComponent<NPC_generator>();
		}
		if (generator.Remain_Energy > 0 && !generator.Restoring_Energy)
		{
			return true;
		}
		return false;
	}
}
