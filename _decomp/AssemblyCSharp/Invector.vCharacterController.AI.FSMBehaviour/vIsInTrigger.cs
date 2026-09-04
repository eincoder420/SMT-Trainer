namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vIsInTrigger : vStateDecision
{
	[vToggleOption("Method", "Compare tag", "Compare name")]
	public bool useName;

	public string compareTrigger;

	public override string categoryName => "Trigger/";

	public override string defaultName => "vIsInTrigger";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (!useName)
		{
			return fsmBehaviour.aiController.IsInTriggerWithTag(compareTrigger);
		}
		return fsmBehaviour.aiController.IsInTriggerWithName(compareTrigger);
	}
}
