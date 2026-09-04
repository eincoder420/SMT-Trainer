using UnityEngine;

public class vAnimatorIncreaseDecreaseValue : StateMachineBehaviour
{
	public string targetFloat;

	public bool decrease;

	private float time;

	public float speed = 1f;

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!decrease)
		{
			time += Time.deltaTime * speed;
		}
		else
		{
			time -= Time.deltaTime * speed;
		}
		animator.SetFloat(targetFloat, time);
	}
}
