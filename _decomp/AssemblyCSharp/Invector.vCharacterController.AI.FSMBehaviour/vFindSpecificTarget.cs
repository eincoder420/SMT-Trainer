using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vFindSpecificTarget : vStateAction
{
	public LayerMask _detectLayer;

	public vTagMask _detectTags;

	public bool checkForObstacles = true;

	public override string categoryName => "Detection/";

	public override string defaultName => "Find Specific Target";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		FindTarget(fsmBehaviour.aiController);
	}

	public virtual void FindTarget(vIControlAI vIControl)
	{
		vIControl.FindSpecificTarget(_detectTags, _detectLayer, checkForObstacles);
	}
}
