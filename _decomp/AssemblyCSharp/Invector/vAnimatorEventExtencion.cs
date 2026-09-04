using Invector.vEventSystems;
using UnityEngine;

namespace Invector;

public static class vAnimatorEventExtencion
{
	public static void RegisterEvent(this Animator animator, string eventName, vAnimatorEvent.OnTriggerEvent onTriggerEventAction)
	{
		if (!animator)
		{
			return;
		}
		vAnimatorEvent[] behaviours = animator.GetBehaviours<vAnimatorEvent>();
		for (int i = 0; i < behaviours.Length; i++)
		{
			if (behaviours[i].HasEvent(eventName))
			{
				behaviours[i].RegisterEvents(eventName, onTriggerEventAction);
			}
		}
	}

	public static void RemoveEvent(this Animator animator, string eventName, vAnimatorEvent.OnTriggerEvent onTriggerEventAction)
	{
		if (!animator)
		{
			return;
		}
		vAnimatorEvent[] behaviours = animator.GetBehaviours<vAnimatorEvent>();
		for (int i = 0; i < behaviours.Length; i++)
		{
			if (behaviours[i].HasEvent(eventName))
			{
				behaviours[i].RemoveEvents(eventName, onTriggerEventAction);
			}
		}
	}
}
