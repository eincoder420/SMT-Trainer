using UnityEngine;

public class Dressing : StateMachineBehaviour
{
	public int id;

	public bool Unwearing;

	public bool Pick_Up;

	public bool No_End_Constraint;

	public bool Excite;

	public bool Toy_Dressing;

	public bool Toy_Undressing;

	private Inventory_Script inventory;

	private Roxanne_Control player;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		inventory = animator.GetComponent<Inventory_Script>();
		player = animator.GetComponent<Roxanne_Control>();
		if (Toy_Dressing && player.Toy_Mode)
		{
			inventory.Replace_Panties(Separate: true);
		}
		if (!Pick_Up && !Toy_Dressing && (bool)inventory)
		{
			inventory.Start_Undressing(id);
			player.rox_audio.clip = player.Cloth[id];
			if (!player.rox_audio.isPlaying)
			{
				player.rox_audio.Play();
			}
			if (id == 3 || id == 1)
			{
				for (int i = 0; i < inventory.Clothes[3].constraints.Length; i++)
				{
					inventory.Clothes[3].constraints[i].constraintActive = true;
				}
				for (int j = 0; j < 2; j++)
				{
					inventory.Boobs[j].BlendWeight = 0f;
				}
			}
			if (id != 2)
			{
				_ = id;
			}
		}
		if (!inventory)
		{
			return;
		}
		if (id == 1 && !Unwearing)
		{
			animator.SetBool("In_Bra", value: true);
		}
		if (id == 4 && !Unwearing)
		{
			animator.SetBool("In_Boots", value: true);
		}
		if (id == 6 || id == 2)
		{
			if (!Unwearing)
			{
				inventory.Butt_Spring[0].BlendWeight = 0f;
				inventory.Butt_Spring[1].BlendWeight = 0f;
			}
			else
			{
				inventory.Butt_Spring[0].BlendWeight = 0.75f;
				inventory.Butt_Spring[1].BlendWeight = 0.75f;
			}
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!player)
		{
			return;
		}
		if (!inventory)
		{
			inventory = animator.GetComponent<Inventory_Script>();
		}
		if (!Pick_Up && !Toy_Dressing)
		{
			inventory.Wearing = true;
			if (id == 3 && stateInfo.normalizedTime % 1f >= 0.5f)
			{
				for (int i = 0; i < inventory.Clothes[3].constraints.Length; i++)
				{
					inventory.Clothes[3].constraints[i].constraintActive = false;
				}
			}
			float num = ((id >= 6) ? 0.95f : 0.99f);
			if (stateInfo.normalizedTime % 1f >= num)
			{
				if (Unwearing)
				{
					inventory.Spawn_Cloth_Item(id);
				}
				if (!Unwearing)
				{
					inventory.End_Undressing(id);
				}
			}
		}
		if (Pick_Up && stateInfo.normalizedTime % 1f >= 0.5f && inventory.Clothes[animator.GetInteger("Cloth_Id")].Mesh[animator.GetInteger("Cloth_Variant")].gameObject.activeInHierarchy)
		{
			inventory.Clothes[animator.GetInteger("Cloth_Id")].Mesh[animator.GetInteger("Cloth_Variant")].gameObject.SetActive(value: false);
		}
		if (Toy_Undressing && stateInfo.normalizedTime % 1f >= 0.35f)
		{
			inventory.Clothes[0].Mesh[0].gameObject.SetActive(value: false);
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
		if (!No_End_Constraint && (bool)player)
		{
			player.cameras.new_const_value = 0f;
		}
		if (!Pick_Up && !Toy_Dressing)
		{
			if (id == 3 || id == 1)
			{
				for (int i = 0; i < inventory.Clothes[3].constraints.Length; i++)
				{
					inventory.Clothes[3].constraints[i].constraintActive = true;
				}
				for (int j = 0; j < 2; j++)
				{
					inventory.Boobs[j].BlendWeight = 0.5f;
				}
			}
			if (id != 2)
			{
				_ = id;
			}
			if (player.rox_audio.isPlaying)
			{
				player.rox_audio.Stop();
			}
			inventory.Wearing = false;
			inventory.Turn_Cloth_Items(On: true);
		}
		if (Pick_Up && (bool)inventory)
		{
			inventory.Clothes[animator.GetInteger("Cloth_Id")].Mesh[animator.GetInteger("Cloth_Variant")].gameObject.SetActive(value: true);
		}
		if (Excite)
		{
			player.StartCoroutine(player.Temporary_Excited());
		}
		if (Toy_Dressing && !player.Toy_Mode)
		{
			inventory.Replace_Panties(Separate: false);
		}
	}
}
