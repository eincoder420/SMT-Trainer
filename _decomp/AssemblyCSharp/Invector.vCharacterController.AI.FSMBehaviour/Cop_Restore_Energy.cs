using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class Cop_Restore_Energy : vStateAction
{
	private NPC_generator generator;

	public override string categoryName => "MyCustomActions/";

	public override string defaultName => "Cop_Restore_Energy";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateEnter)
	{
		if (!generator)
		{
			generator = fsmBehaviour.aiController.animator.GetComponent<NPC_generator>();
		}
		generator.Remain_Energy = 10;
		Debug.Log(generator.name);
	}
}
