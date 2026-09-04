using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vRandomDecision : vStateDecision
{
	[Range(0f, 100f)]
	[Tooltip("Percentage Chance between true and false")]
	public float randomTrueFalse;

	public float frequency;

	public override string categoryName => "Generic/";

	public override string defaultName => "Random Decision";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (frequency > 0f)
		{
			if (InTimer(fsmBehaviour, frequency))
			{
				return (float)Random.Range(0, 100) < randomTrueFalse;
			}
			return false;
		}
		return (float)Random.Range(0, 100) < randomTrueFalse;
	}
}
