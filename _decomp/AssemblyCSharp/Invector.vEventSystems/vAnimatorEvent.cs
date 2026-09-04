using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vEventSystems;

public class vAnimatorEvent : StateMachineBehaviour
{
	[Serializable]
	public class vAnimatorEventTrigger
	{
		public enum vAnimatorEventTriggerType
		{
			NormalizedTime,
			EnterState,
			ExitState
		}

		public string eventName = "New Event";

		public vAnimatorEventTriggerType eventTriggerType;

		public float normalizedTime;

		private int loopCount;

		public event OnTriggerEvent onTriggerEvent;

		public void UpdateEventTrigger(float normalizedTime)
		{
			if (Mathf.Clamp(normalizedTime, 0f, (float)loopCount + 1f) >= (float)loopCount + this.normalizedTime)
			{
				if (this.onTriggerEvent != null)
				{
					this.onTriggerEvent(eventName);
				}
				loopCount++;
			}
		}

		public void TriggerEvent()
		{
			if (this.onTriggerEvent != null)
			{
				this.onTriggerEvent(eventName);
			}
		}

		public void Init()
		{
			loopCount = 0;
		}
	}

	public delegate void OnTriggerEvent(string eventName);

	public List<vAnimatorEventTrigger> eventTriggers;

	protected bool hasNormalizedEvents;

	public bool HasEvent(string eventName)
	{
		return eventTriggers.Exists((vAnimatorEventTrigger e) => e.eventName.Equals(eventName));
	}

	public void RegisterEvents(string eventName, OnTriggerEvent onTriggerEvent)
	{
		List<vAnimatorEventTrigger> list = eventTriggers.FindAll((vAnimatorEventTrigger e) => e.eventName.Equals(eventName));
		for (int i = 0; i < list.Count; i++)
		{
			list[i].onTriggerEvent -= onTriggerEvent;
			list[i].onTriggerEvent += onTriggerEvent;
		}
	}

	public void RemoveEvents(string eventName, OnTriggerEvent onTriggerEvent)
	{
		List<vAnimatorEventTrigger> list = eventTriggers.FindAll((vAnimatorEventTrigger e) => e.eventName.Equals(eventName));
		for (int i = 0; i < list.Count; i++)
		{
			list[i].onTriggerEvent -= onTriggerEvent;
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		for (int i = 0; i < eventTriggers.Count; i++)
		{
			if (eventTriggers[i].eventTriggerType == vAnimatorEventTrigger.vAnimatorEventTriggerType.EnterState)
			{
				eventTriggers[i].TriggerEvent();
			}
			else if (eventTriggers[i].eventTriggerType == vAnimatorEventTrigger.vAnimatorEventTriggerType.NormalizedTime)
			{
				hasNormalizedEvents = true;
				eventTriggers[i].Init();
				eventTriggers[i].UpdateEventTrigger(stateInfo.normalizedTime);
			}
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if ((!stateInfo.loop && stateInfo.normalizedTime > 1f) || !hasNormalizedEvents)
		{
			return;
		}
		for (int i = 0; i < eventTriggers.Count; i++)
		{
			if (eventTriggers[i].eventTriggerType == vAnimatorEventTrigger.vAnimatorEventTriggerType.NormalizedTime)
			{
				eventTriggers[i].UpdateEventTrigger(stateInfo.normalizedTime);
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		for (int i = 0; i < eventTriggers.Count; i++)
		{
			if (eventTriggers[i].eventTriggerType == vAnimatorEventTrigger.vAnimatorEventTriggerType.ExitState)
			{
				eventTriggers[i].TriggerEvent();
			}
		}
	}
}
