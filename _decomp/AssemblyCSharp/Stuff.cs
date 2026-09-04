using UnityEngine;

public class Stuff : StateMachineBehaviour
{
	public int Type;

	private Transform Object;

	public bool Dissapear_End;

	public bool Throw_End;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!Object)
		{
			Object = animator.GetComponent<Roxanne_Control>().Hand_Stuff[Type];
		}
		Object.gameObject.SetActive(value: true);
		if (Type == 1)
		{
			animator.GetComponent<Roxanne_Control>().Hand_Stuff[3].gameObject.SetActive(value: true);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (Dissapear_End)
		{
			if (!Object)
			{
				Object = animator.GetComponent<Roxanne_Control>().Hand_Stuff[Type];
			}
			Object.gameObject.SetActive(value: false);
			if (Type == 1)
			{
				animator.GetComponent<Roxanne_Control>().Hand_Stuff[3].gameObject.SetActive(value: false);
			}
			if (Throw_End)
			{
				animator.GetComponent<Roxanne_Control>().Throw_Item(Type);
			}
		}
	}
}
