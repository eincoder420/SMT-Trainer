using System;
using UnityEngine;

namespace Invector.vCharacterController;

internal class vAnimatorMoveSender : MonoBehaviour
{
	public Action animatorMoveEvent;

	private void Awake()
	{
		base.hideFlags = HideFlags.HideInInspector;
		vIAnimatorMoveReceiver[] components = GetComponents<vIAnimatorMoveReceiver>();
		foreach (vIAnimatorMoveReceiver receiver in components)
		{
			animatorMoveEvent = (Action)Delegate.Combine(animatorMoveEvent, (Action)delegate
			{
				if (receiver.enabled)
				{
					receiver.OnAnimatorMoveEvent();
				}
			});
		}
	}

	private void OnAnimatorMove()
	{
		animatorMoveEvent?.Invoke();
	}
}
