using UnityEngine;

public class Walk_Check : StateMachineBehaviour
{
	private Roxanne_Control player;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		player = animator.GetComponent<Roxanne_Control>();
		player.Crouching = true;
		player.Switch_Walk();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		player = animator.GetComponent<Roxanne_Control>();
		player.Crouching = false;
		player.Switch_Walk();
	}
}
