namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vIsClosestListenerToNoise : vStateDecision
{
	public override string categoryName => "Noise/";

	public override string defaultName => "IsClosestListenerToNoise?";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		vAINoiseListener aIComponent = fsmBehaviour.aiController.GetAIComponent<vAINoiseListener>();
		if (aIComponent != null && aIComponent.LastListenedNoise != null)
		{
			return aIComponent.IsClosestListenerToNoise(aIComponent.LastListenedNoise);
		}
		return false;
	}
}
