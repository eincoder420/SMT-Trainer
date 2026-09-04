namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAnimatorSetTrigger : vStateAction
{
	public string trigger;

	[vToggleOption("Method", "Set", "Reset")]
	public bool reset;

	public override string categoryName => "Animator/";

	public override string defaultName => "vAnimatorSetTrigger";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (reset)
		{
			fsmBehaviour.aiController.animator.ResetTrigger(trigger);
		}
		else
		{
			fsmBehaviour.aiController.animator.SetTrigger(trigger);
		}
	}
}
