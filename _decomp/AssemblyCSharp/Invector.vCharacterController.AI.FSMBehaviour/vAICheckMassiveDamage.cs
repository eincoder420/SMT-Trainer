namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAICheckMassiveDamage : vStateDecision
{
	[vToggleOption("Compare Value", "Less", "Greater or Equals")]
	public bool greater;

	[vToggleOption("Compare Type", "Total Hits", "Total Damage")]
	public bool massiveValue = true;

	public int valueToCompare;

	public override string categoryName => "Health/";

	public override string defaultName => "Check Damage Amount";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		return HasDamage(fsmBehaviour);
	}

	protected virtual bool HasDamage(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour.aiController.receivedDamage == null)
		{
			return false;
		}
		int num = (massiveValue ? fsmBehaviour.aiController.receivedDamage.massiveValue : fsmBehaviour.aiController.receivedDamage.massiveCount);
		if (!greater)
		{
			return num < valueToCompare;
		}
		return num >= valueToCompare;
	}
}
