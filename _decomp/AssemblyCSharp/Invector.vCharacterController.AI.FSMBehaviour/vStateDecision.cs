using System;
using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public abstract class vStateDecision : ScriptableObject
{
	public vFSMBehaviour parentFSM;

	public abstract string categoryName { get; }

	public abstract string defaultName { get; }

	public string Name
	{
		get
		{
			return base.name;
		}
		set
		{
			base.name = value;
		}
	}

	public virtual Type requiredType => typeof(vIControlAI);

	public abstract bool Decide(vIFSMBehaviourController fsmBehaviour);

	protected virtual bool InTimer(vIFSMBehaviourController fsmBehaviour, float compareTimer = 1f, string timerTag = "")
	{
		string key = (string.IsNullOrEmpty(timerTag) ? base.name : timerTag);
		float timer = fsmBehaviour.GetTimer(key);
		fsmBehaviour.SetTimer(key, timer + Time.deltaTime);
		if (timer > compareTimer)
		{
			fsmBehaviour.SetTimer(key, 0f);
			return true;
		}
		return false;
	}

	protected virtual bool InRandomTimer(vIFSMBehaviourController fsmBehaviour, float minTimer, float maxTimer, string timerTag = "")
	{
		string key = (string.IsNullOrEmpty(timerTag) ? base.name : timerTag);
		if (!fsmBehaviour.HasTimer(key))
		{
			fsmBehaviour.SetTimer(key, UnityEngine.Random.Range(minTimer, maxTimer) + Time.time);
		}
		if (fsmBehaviour.GetTimer(key) < Time.time)
		{
			fsmBehaviour.SetTimer(key, UnityEngine.Random.Range(minTimer, maxTimer) + Time.time);
			return true;
		}
		return false;
	}
}
