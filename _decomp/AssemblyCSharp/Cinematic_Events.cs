using UnityEngine;

public class Cinematic_Events : StateMachineBehaviour
{
	public int Scene_Id;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<Cinematic_Sam>().Interface_Script.Loader.End_Scene(Scene_Id);
	}
}
