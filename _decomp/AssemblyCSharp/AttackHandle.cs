using System;
using UnityEngine.Events;

[Serializable]
public class AttackHandle
{
	public string AnimationPlay;

	public float enableAttackTriggerTime;

	public float disableAttackTriggerTime;

	public float timeToFinish;

	public float crossFade = 0.1f;

	public float animatorSpeed;

	public UnityEvent onEnableAttackTrigger;

	public UnityEvent onDisableAttackTrigger;
}
