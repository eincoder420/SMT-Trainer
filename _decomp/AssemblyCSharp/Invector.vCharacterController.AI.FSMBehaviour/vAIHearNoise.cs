using System.Collections.Generic;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAIHearNoise : vStateDecision
{
	[vToggleOption("Noise Type", "Any Noise", "Specific Noise")]
	public bool specific;

	[vHideInInspector("specific", false)]
	public List<string> noiseTypes;

	public override string categoryName => "Noise/";

	public override string defaultName => "Check for Noise";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour.aiController != null && fsmBehaviour.aiController.HasComponent<vAINoiseListener>())
		{
			vAINoiseListener aIComponent = fsmBehaviour.aiController.GetAIComponent<vAINoiseListener>();
			if (specific)
			{
				return aIComponent.IsListeningSpecificNoises(noiseTypes);
			}
			return aIComponent.IsListeningNoise();
		}
		return false;
	}
}
