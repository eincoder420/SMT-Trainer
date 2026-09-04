using UnityEngine;

public class Npc_Stuff : StateMachineBehaviour
{
	public int id;

	private NPC_generator generator;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		generator = Object.FindObjectOfType<NPC_generator>();
		if (generator.Mom)
		{
			for (int i = 0; i < generator.Stuff.Length; i++)
			{
				generator.Stuff[i].gameObject.SetActive(value: false);
			}
			generator.Stuff[id].gameObject.SetActive(value: true);
			if (id == 2)
			{
				generator.Stuff[3].gameObject.SetActive(value: true);
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (generator.Mom)
		{
			for (int i = 0; i < generator.Stuff.Length; i++)
			{
				generator.Stuff[i].gameObject.SetActive(value: false);
			}
		}
	}
}
