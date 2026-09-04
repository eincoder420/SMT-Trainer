using System;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

[Serializable]
public class vStateDecisionObject
{
	public bool trueValue = true;

	public vStateDecision decision;

	[SerializeField]
	public bool isValid;

	public bool validated;

	public vStateDecisionObject(vStateDecision decision)
	{
		this.decision = decision;
	}

	public vStateDecisionObject Copy()
	{
		return new vStateDecisionObject(decision)
		{
			trueValue = trueValue
		};
	}

	public bool Validate(vIFSMBehaviourController fsmBehaviour)
	{
		if (trueValue)
		{
			isValid = !decision || decision.Decide(fsmBehaviour);
		}
		else
		{
			isValid = !decision || !decision.Decide(fsmBehaviour);
		}
		return isValid;
	}
}
