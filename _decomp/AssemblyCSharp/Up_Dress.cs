using UnityEngine;

public class Up_Dress : StateMachineBehaviour
{
	public int Cloth_id;

	public int Inv_Cloth_id;

	public bool Start;

	public bool Upped;

	public bool Back;

	private Inventory_Script inventory;

	private Roxanne_Control player;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		player = animator.GetComponent<Roxanne_Control>();
		inventory = animator.GetComponent<Inventory_Script>();
		player.Anim_UpDress_Process = true;
		if (Start)
		{
			animator.SetBool("Upped_Dress", Upped);
			player.Current_Cloth = Cloth_id;
			if (Cloth_id == 1)
			{
				player.Set_Up_Skirt();
			}
			animator.GetComponent<Inventory_Script>().Start_Undressing(Inv_Cloth_id);
		}
		if (!Start)
		{
			animator.SetBool("ReadyWalkUpdressed", value: false);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!player)
		{
			player = animator.GetComponent<Roxanne_Control>();
		}
		if (Start || Back)
		{
			animator.SetFloat("InputMagnitude", Mathf.MoveTowards(animator.GetFloat("InputMagnitude"), 0f, Time.deltaTime * 4f));
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!player)
		{
			player = animator.GetComponent<Roxanne_Control>();
		}
		if (!inventory)
		{
			inventory = animator.GetComponent<Inventory_Script>();
		}
		player.cameras.new_const_value = 0f;
		player.Anim_UpDress_Process = false;
		if (!Start)
		{
			if (Cloth_id == 1)
			{
				player.Set_Free_Skirt();
			}
			animator.SetBool("Upped_Dress", Upped);
			player.Showing = false;
			if (player.Current_Cloth != player.Requied_Cloth)
			{
				player.Check_Cloth_For_Showing(player.Requied_Cloth);
			}
			else if (inventory.Waiting_For_Wear)
			{
				inventory.Wear(inventory.Waiting_id);
				inventory.Waiting_For_Wear = false;
			}
			else
			{
				animator.SetBool("Excited", value: false);
			}
			inventory.End_Undressing(Inv_Cloth_id);
		}
		if (Start && animator.GetBool("Dancing"))
		{
			animator.SetTrigger("Dancing_Updress");
		}
		if (Start)
		{
			animator.SetBool("ReadyWalkUpdressed", value: true);
		}
	}
}
