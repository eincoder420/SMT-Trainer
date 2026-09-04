using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vEventSystems;

[vClassHeader("Animator Event Receiver", true, "icon_v2", false, "")]
public class vAnimatorEventReceiver : vMonoBehaviour
{
	[Serializable]
	public class vAnimatorEvent
	{
		[Serializable]
		public class StateEvent : UnityEvent<string>
		{
		}

		public string eventName;

		public bool debug;

		public StateEvent onTriggerEvent;

		public virtual void OnTriggerEvent(string eventName)
		{
			if (debug)
			{
				Debug.Log("<color=green><b>Event " + eventName + " was called</b></color>");
			}
			onTriggerEvent.Invoke(eventName);
		}
	}

	[Tooltip("Check this option if the Animator component is on the parent of this GameObject")]
	public bool getAnimatorInParent;

	[vHelpBox("Use <b>vAnimatorEvent</b> on a AnimatorState to trigger a Event below", vHelpBoxAttribute.MessageType.Info)]
	public List<vAnimatorEvent> animatorEvents;

	private bool hasValidBehaviours;

	private bool hasAnimator;

	private void Start()
	{
		RegisterEvents();
	}

	private void OnDisable()
	{
		RemoveEvents();
	}

	public void OnEnable()
	{
		if (hasAnimator && hasValidBehaviours)
		{
			RemoveEvents();
			RegisterEvents();
		}
	}

	private void OnDestroy()
	{
		RemoveEvents();
	}

	public virtual void RegisterEvents()
	{
		if (animatorEvents.Count <= 0)
		{
			return;
		}
		Animator animator = (getAnimatorInParent ? GetComponentInParent<Animator>() : GetComponent<Animator>());
		if ((bool)animator)
		{
			hasAnimator = true;
			Invector.vEventSystems.vAnimatorEvent[] behaviours = animator.GetBehaviours<Invector.vEventSystems.vAnimatorEvent>();
			for (int i = 0; i < animatorEvents.Count; i++)
			{
				bool flag = false;
				for (int j = 0; j < behaviours.Length; j++)
				{
					if (behaviours[j].HasEvent(animatorEvents[i].eventName))
					{
						behaviours[j].RegisterEvents(animatorEvents[i].eventName, animatorEvents[i].OnTriggerEvent);
						if (animatorEvents[i].debug)
						{
							Debug.Log("<color=green>" + base.gameObject.name + " Register event : " + animatorEvents[i].eventName + "</color> in the " + animator.gameObject.name, base.gameObject);
						}
						hasValidBehaviours = true;
						flag = true;
					}
				}
				if (!flag && animatorEvents[i].debug)
				{
					Debug.LogWarning(animator.gameObject.name + " Animator doesn't have Event with name: " + animatorEvents[i].eventName, base.gameObject);
				}
			}
		}
		else
		{
			Debug.LogWarning("Can't Find Animator to register Events in " + base.gameObject.name + (getAnimatorInParent ? " Parent" : ""), base.gameObject);
		}
	}

	public virtual void RemoveEvents()
	{
		if (!hasAnimator || !hasValidBehaviours || animatorEvents.Count <= 0)
		{
			return;
		}
		Animator animator = (getAnimatorInParent ? GetComponentInParent<Animator>() : GetComponent<Animator>());
		if (!animator)
		{
			return;
		}
		Invector.vEventSystems.vAnimatorEvent[] behaviours = animator.GetBehaviours<Invector.vEventSystems.vAnimatorEvent>();
		for (int i = 0; i < animatorEvents.Count; i++)
		{
			for (int j = 0; j < behaviours.Length; j++)
			{
				if (behaviours[j].HasEvent(animatorEvents[i].eventName))
				{
					behaviours[j].RemoveEvents(animatorEvents[i].eventName, animatorEvents[i].OnTriggerEvent);
					if (animatorEvents[i].debug)
					{
						Debug.Log("<color=red>" + base.gameObject.name + " Remove event : " + animatorEvents[i].eventName + "</color> Of the " + animator.gameObject.name, base.gameObject);
					}
				}
			}
		}
	}
}
