using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class Cop_Pointing : vStateAction
{
	private NPC_generator generator;

	public override string categoryName => "MyCustomActions/";

	public override string defaultName => "Cop_Pointing";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateEnter)
	{
		if (!generator)
		{
			generator = fsmBehaviour.aiController.animator.GetComponent<NPC_generator>();
		}
		generator.Cop_Point_Player();
		Debug.Log(generator.name);
	}
}
