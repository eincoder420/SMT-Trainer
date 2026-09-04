using UnityEngine;

public class Action_Event : StateMachineBehaviour
{
	private Roxanne_Control rox_control;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!rox_control)
		{
			rox_control = animator.GetComponent<Roxanne_Control>();
		}
		rox_control.Stop_All_Actions();
	}
}
