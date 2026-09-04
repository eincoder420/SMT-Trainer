using UnityEngine;

public class Masturbating : StateMachineBehaviour
{
	private Inventory_Script inventory;

	public bool start_Upskirt;

	private bool masturbation_started;

	public bool Orgasm;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!inventory)
		{
			inventory = animator.GetComponent<Inventory_Script>();
		}
		if (start_Upskirt)
		{
			masturbation_started = false;
		}
		Cinematic_Sam component = animator.GetComponent<Cinematic_Sam>();
		if (Orgasm)
		{
			component.Play_Sound(component.Orgasm_Sound);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime % 1f >= 0.75f && start_Upskirt && !masturbation_started)
		{
			animator.SetTrigger("Masturbation");
			masturbation_started = true;
		}
		if (stateInfo.normalizedTime % 1f >= 0.85f && Orgasm && !animator.GetBool("Orgasm_Played"))
		{
			animator.SetBool("Orgasm_Played", value: true);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animator.GetBool("Orgasm_Played"))
		{
			animator.SetBool("Orgasm_Played", value: false);
		}
	}
}
