using System;
using UnityEngine;

namespace FIMSpace.Basics;

public sealed class FBasic_StateEventsBehaviour : StateMachineBehaviour
{
	public event Action<int> OnEnterState;

	public event Action<int> OnExitState;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.OnEnterState != null)
		{
			this.OnEnterState(stateInfo.shortNameHash);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.OnExitState != null)
		{
			this.OnExitState(stateInfo.shortNameHash);
		}
	}
}
