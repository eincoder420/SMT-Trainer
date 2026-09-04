namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vGoToFriend : vStateAction
{
	public vAIMovementSpeed speed = vAIMovementSpeed.Running;

	public override string categoryName => "Movement/";

	public override string defaultName => "Go To Friend(Companion AI)";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (fsmBehaviour.aiController.HasComponent<vAICompanion>())
		{
			MoveToFriendPosition(fsmBehaviour.aiController.GetAIComponent<vAICompanion>());
		}
	}

	public virtual void MoveToFriendPosition(vAICompanion aICompanion)
	{
		if ((bool)aICompanion)
		{
			aICompanion.GoToFriend(speed);
		}
	}
}
