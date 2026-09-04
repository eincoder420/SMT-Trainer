using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vCheckForNoiseDistance : vStateDecision
{
	protected enum CompareValueMethod
	{
		Greater,
		Less,
		Equal
	}

	public bool findNewNoise;

	public bool specificType;

	[vHideInInspector("findNewNoise;specificType", false)]
	public List<string> noiseTypes;

	[SerializeField]
	protected CompareValueMethod compareMethod;

	public float distance;

	public override string categoryName => "Noise/";

	public override string defaultName => "Check For Noise Distance";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour.aiController != null && fsmBehaviour.aiController.HasComponent<vAINoiseListener>())
		{
			vAINoiseListener aIComponent = fsmBehaviour.aiController.GetAIComponent<vAINoiseListener>();
			vNoise vNoise = null;
			vNoise = ((!findNewNoise) ? aIComponent.LastListenedNoise : ((!specificType) ? aIComponent.GetNearNoise() : aIComponent.GetNearNoiseByTypes(noiseTypes)));
			if (vNoise != null)
			{
				return CompareDistance(Vector3.Distance(fsmBehaviour.aiController.transform.position, vNoise.position), distance);
			}
		}
		return true;
	}

	private bool CompareDistance(float distA, float distB)
	{
		return compareMethod switch
		{
			CompareValueMethod.Equal => distA.Equals(distB), 
			CompareValueMethod.Greater => distA > distB, 
			CompareValueMethod.Less => distA < distB, 
			_ => false, 
		};
	}
}
