namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAIGetCoverAction : vStateAction
{
	public vAIMovementSpeed speed = vAIMovementSpeed.Running;

	public override string categoryName => "Movement/";

	public override string defaultName => "Get Cover";

	public vAIGetCoverAction()
	{
		executionType = vFSMComponentExecutionType.OnStateUpdate | vFSMComponentExecutionType.OnStateExit;
	}

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (!(fsmBehaviour.aiController is vIControlAICombat vIControlAICombat))
		{
			return;
		}
		if (executionType == vFSMComponentExecutionType.OnStateUpdate && fsmBehaviour.aiController.HasComponent<vAICover>())
		{
			vAICover aIComponent = fsmBehaviour.aiController.GetAIComponent<vAICover>();
			if ((bool)fsmBehaviour.aiController.currentTarget.transform)
			{
				aIComponent.GetCoverFromTargetThreat(speed);
			}
			else
			{
				aIComponent.GetCoverFromRandomThreat(speed);
			}
		}
		if (executionType == vFSMComponentExecutionType.OnStateExit)
		{
			if (fsmBehaviour.aiController.HasComponent<vAICover>())
			{
				fsmBehaviour.aiController.GetAIComponent<vAICover>().OnExitCover();
			}
			vIControlAICombat.isInCombat = false;
			vIControlAICombat.isCrouching = false;
		}
		if (executionType == vFSMComponentExecutionType.OnStateEnter)
		{
			vIControlAICombat.isInCombat = true;
		}
	}
}
