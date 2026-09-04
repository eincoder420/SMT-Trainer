namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAISetDetectionTags : vStateAction
{
	public vTagMask tags;

	public override string categoryName => "Detection/";

	public override string defaultName => "Set Detections Tags";

	public vAISetDetectionTags()
	{
		executionType = vFSMComponentExecutionType.OnStateEnter;
	}

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (executionType == vFSMComponentExecutionType.OnStateEnter)
		{
			fsmBehaviour.aiController.SetDetectionTags(tags);
		}
	}
}
