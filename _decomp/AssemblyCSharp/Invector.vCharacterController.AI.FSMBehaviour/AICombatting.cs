namespace Invector.vCharacterController.AI.FSMBehaviour;

public class AICombatting : vStateDecision
{
	public override string categoryName => "Combat/";

	public override string defaultName => "Is in Combat";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (!(fsmBehaviour.aiController is vIControlAICombat))
		{
			return false;
		}
		return (fsmBehaviour.aiController as vIControlAICombat).isInCombat;
	}
}
