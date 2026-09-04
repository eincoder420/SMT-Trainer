using UnityEngine;

public class Speaking : StateMachineBehaviour
{
	private Roxanne_Control player;

	public bool Start_Speak;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!player)
		{
			player = Object.FindObjectOfType<Roxanne_Control>();
		}
		if (Start_Speak)
		{
			player.Speaking = true;
			player.interface_script.Turn_Cursor();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!player)
		{
			player = Object.FindObjectOfType<Roxanne_Control>();
		}
		if (!Start_Speak)
		{
			player.Speaking = false;
			player.interface_script.Turn_Cursor();
		}
	}
}
