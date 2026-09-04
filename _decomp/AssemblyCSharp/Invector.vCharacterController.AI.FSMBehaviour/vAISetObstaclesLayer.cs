using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAISetObstaclesLayer : vStateAction
{
	public LayerMask newLayer;

	public override string categoryName => "Detection/";

	public override string defaultName => "Set Obstacles Layer";

	public vAISetObstaclesLayer()
	{
		executionType = vFSMComponentExecutionType.OnStateEnter;
	}

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (executionType == vFSMComponentExecutionType.OnStateEnter)
		{
			fsmBehaviour.aiController.SetObstaclesLayer(newLayer);
		}
	}
}
