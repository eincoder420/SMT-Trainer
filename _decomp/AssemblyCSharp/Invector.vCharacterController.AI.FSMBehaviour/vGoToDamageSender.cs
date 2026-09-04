namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vGoToDamageSender : vStateAction
{
	public bool goInStrafe;

	public vAIMovementSpeed speed = vAIMovementSpeed.Walking;

	public override string categoryName => "Movement/";

	public override string defaultName => "Go To Damage Sender";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (fsmBehaviour.aiController != null && !(fsmBehaviour.aiController.receivedDamage.lastSender == null))
		{
			if (executionType == vFSMComponentExecutionType.OnStateEnter)
			{
				fsmBehaviour.aiController.ForceUpdatePath(2f);
			}
			if (goInStrafe)
			{
				fsmBehaviour.aiController.StrafeMoveTo(fsmBehaviour.aiController.receivedDamage.lastSender.position, fsmBehaviour.aiController.receivedDamage.lastSender.position - fsmBehaviour.transform.position, speed);
			}
			else
			{
				fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.receivedDamage.lastSender.position, speed);
			}
		}
	}
}
