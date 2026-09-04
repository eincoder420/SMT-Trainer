using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

[Serializable]
public class vStateTransition
{
	public List<vStateDecisionObject> decisions = new List<vStateDecisionObject>();

	public vFSMState trueState;

	public vFSMState falseState;

	public bool muteTrue;

	public bool muteFalse;

	public vTransitionOutputType transitionType;

	public float transitionDelay;

	public vFSMState parentState;

	private Dictionary<vIFSMBehaviourController, float> transitionTimers;

	public bool useTruState
	{
		get
		{
			if (transitionType != vTransitionOutputType.TrueFalse)
			{
				return transitionType == vTransitionOutputType.Default;
			}
			return true;
		}
	}

	public bool useFalseState => transitionType == vTransitionOutputType.TrueFalse;

	public vStateTransition(vStateDecision decision)
	{
		if ((bool)decision)
		{
			decisions.Add(new vStateDecisionObject(decision));
		}
	}

	public vFSMState TransitTo(vIFSMBehaviourController fsmBehaviour)
	{
		bool flag = true;
		vFSMState vFSMState2 = null;
		for (int i = 0; i < decisions.Count; i++)
		{
			if (!decisions[i].Validate(fsmBehaviour))
			{
				flag = false;
			}
		}
		if (flag && (bool)trueState)
		{
			vFSMState2 = ((useTruState && !muteTrue) ? trueState : null);
		}
		else if (!flag && (bool)falseState)
		{
			vFSMState2 = ((useFalseState && !muteFalse) ? falseState : null);
		}
		if (transitionTimers == null)
		{
			transitionTimers = new Dictionary<vIFSMBehaviourController, float>();
		}
		if (!transitionTimers.ContainsKey(fsmBehaviour))
		{
			transitionTimers.Add(fsmBehaviour, 0f);
		}
		if (transitionTimers[fsmBehaviour] < transitionDelay && (bool)vFSMState2)
		{
			transitionTimers[fsmBehaviour] += Time.deltaTime;
			if (fsmBehaviour.debugMode)
			{
				fsmBehaviour.SendDebug("<color=green>" + parentState.name + " Delay " + (transitionDelay - transitionTimers[fsmBehaviour]).ToString("00") + " To Enter in " + vFSMState2.Name + "</color>", parentState);
			}
			return null;
		}
		transitionTimers[fsmBehaviour] = 0f;
		if (fsmBehaviour.debugMode && (bool)vFSMState2)
		{
			fsmBehaviour.SendDebug("<color=yellow>" + parentState.name + " Transited to " + vFSMState2.name + "</color>", parentState);
		}
		return vFSMState2;
	}
}
