namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAISetLineOfSight : vStateAction
{
	[vHelpBox("If you don't want to overwrite a value leave it to -1", vHelpBoxAttribute.MessageType.None)]
	public float fieldOfView;

	public float minDistanceToDetect;

	public float maxDistanceToDetect;

	public float lostTargetDistance;

	public override string categoryName => "Detection/";

	public override string defaultName => "Set Line Of Sight";

	public vAISetLineOfSight()
	{
		executionType = vFSMComponentExecutionType.OnStateEnter;
		fieldOfView = -1f;
		minDistanceToDetect = -1f;
		maxDistanceToDetect = -1f;
		lostTargetDistance = -1f;
	}

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (executionType == vFSMComponentExecutionType.OnStateEnter)
		{
			fsmBehaviour.aiController.SetLineOfSight(fieldOfView, minDistanceToDetect, maxDistanceToDetect, lostTargetDistance);
		}
	}
}
