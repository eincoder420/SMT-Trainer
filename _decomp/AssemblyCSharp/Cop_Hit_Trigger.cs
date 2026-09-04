using Invector.vCharacterController;
using Invector.vCharacterController.AI;
using UnityEngine;

public class Cop_Hit_Trigger : MonoBehaviour
{
	public Animator anim;

	private void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.layer != 9 || !col.GetComponent<vRagdoll>())
		{
			return;
		}
		if (anim.GetFloat("InputMagnitude") >= 0.75f)
		{
			if (!col.GetComponent<vControlAI>().ragdolled)
			{
				col.GetComponent<AudioSource>().PlayOneShot(col.GetComponent<NPC_generator>().Hit_voice);
				col.GetComponent<vRagdoll>().ActivateRagdoll(null, 2f);
			}
			return;
		}
		if (!col.GetComponent<AudioSource>().isPlaying)
		{
			col.GetComponent<AudioSource>().PlayOneShot(col.GetComponent<NPC_generator>().Hit_voice);
		}
		col.GetComponent<Animator>().Play("Reaction");
		col.GetComponent<Animator>().SetTrigger("Hit_By_Cop");
	}
}
