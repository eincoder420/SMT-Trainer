using UnityEngine;

public class Cop_Catch : StateMachineBehaviour
{
	private Roxanne_Control player;

	private NPC_generator generator;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!player)
		{
			player = Object.FindObjectOfType<Roxanne_Control>();
		}
		if (!generator)
		{
			generator = animator.GetComponent<NPC_generator>();
		}
		if (!player.Arrested)
		{
			generator.Cop_Speak_Arrest();
			animator.GetComponent<Rigidbody>().isKinematic = true;
			animator.GetComponent<CapsuleCollider>().enabled = false;
			player.StartCoroutine(player.Fail_With_Delay(2));
			player.body_ik.solver.leftHandEffector.target = generator.Cop_Hand_L;
			player.body_ik.solver.rightHandEffector.target = generator.Cop_Hand_R;
			player.inventory.Replace_Shirt();
			player.GetComponent<Animator>().SetFloat("InputMagnitude", 0f);
			player.GetComponent<Animator>().SetTrigger("Screaming");
			player.Arrested = true;
			player.Stop_All_Actions();
			player.GetComponent<Rigidbody>().isKinematic = true;
			player.transform.position = generator.Cop_Arrest_Position.position;
			player.transform.rotation = generator.Cop_Arrest_Position.rotation;
			player.body_ik.solver.leftHandEffector.positionWeight = 1f;
			player.body_ik.solver.rightHandEffector.positionWeight = 1f;
			player.body_ik.solver.leftHandEffector.rotationWeight = 1f;
			player.body_ik.solver.rightHandEffector.rotationWeight = 1f;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponent<Rigidbody>().isKinematic = false;
		animator.GetComponent<CapsuleCollider>().enabled = true;
	}
}
