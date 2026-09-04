using UnityEngine;

public class vAnimatorSetTrigger : StateMachineBehaviour
{
	public bool setOnEnter;

	public bool setOnExit;

	public string trigger;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (setOnEnter)
		{
			animator.SetTrigger(trigger);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (setOnExit)
		{
			animator.SetTrigger(trigger);
		}
	}
}
