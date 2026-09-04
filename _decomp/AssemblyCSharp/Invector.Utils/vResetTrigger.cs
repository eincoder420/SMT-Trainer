using UnityEngine;

namespace Invector.Utils;

public class vResetTrigger : StateMachineBehaviour
{
	public bool resetOnEnter;

	public bool resetOnExit;

	public string trigger;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (resetOnEnter)
		{
			animator.ResetTrigger(trigger);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (resetOnExit)
		{
			animator.ResetTrigger(trigger);
		}
	}
}
