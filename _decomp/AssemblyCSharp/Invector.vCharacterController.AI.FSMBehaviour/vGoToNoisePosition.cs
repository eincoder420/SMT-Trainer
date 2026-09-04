using System.Collections.Generic;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vGoToNoisePosition : vStateAction
{
	public bool findNewNoise;

	public bool specificType;

	[vHideInInspector("findNewNoise;specificType", false)]
	public List<string> noiseTypes;

	public bool lookToNoisePosition = true;

	public vAIMovementSpeed speed = vAIMovementSpeed.Walking;

	public override string categoryName => "Movement/Noise/";

	public override string defaultName => "Go To Noise Position";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (fsmBehaviour.aiController == null || !fsmBehaviour.aiController.HasComponent<vAINoiseListener>())
		{
			return;
		}
		vAINoiseListener aIComponent = fsmBehaviour.aiController.GetAIComponent<vAINoiseListener>();
		vNoise vNoise = null;
		vNoise = ((!findNewNoise) ? aIComponent.LastListenedNoise : ((!specificType) ? aIComponent.GetNearNoise() : aIComponent.GetNearNoiseByTypes(noiseTypes)));
		if (vNoise != null)
		{
			fsmBehaviour.aiController.MoveTo(vNoise.position, speed);
			if (lookToNoisePosition)
			{
				fsmBehaviour.aiController.LookTo(vNoise.position, 1f, 0f);
			}
		}
	}
}
