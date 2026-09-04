using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vCheckTargetDistance : vStateDecision
{
	protected enum CompareValueMethod
	{
		Greater,
		Less,
		Equal
	}

	[SerializeField]
	protected CompareValueMethod compareMethod;

	public float distance;

	public override string categoryName => "Detection/";

	public override string defaultName => "Check Target Distance";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (!fsmBehaviour.aiController.currentTarget.transform)
		{
			return false;
		}
		float targetDistance = fsmBehaviour.aiController.targetDistance;
		return CompareDistance(targetDistance, distance);
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
