using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vMoveToTarget_Example : vStateAction
{
	public vAIMovementSpeed speed = vAIMovementSpeed.Walking;

	public override string categoryName => "Custom Example/";

	public override string defaultName => "MoveToTarget Example";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		Debug.Log("FSM Calling 'MoveToTargetExample' ");
		fsmBehaviour.aiController.MoveToTargetExample(speed);
	}
}
