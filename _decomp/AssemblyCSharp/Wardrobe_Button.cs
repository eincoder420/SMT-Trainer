using UnityEngine;
using UnityEngine.UI;

public class Wardrobe_Button : MonoBehaviour
{
	private Inventory_Script inventory;

	public int id;

	public int Variant;

	public bool Wardrobe_Cloth;

	public Renderer Cloth_Render;

	public Color base_color;

	public int[] Reqied_Unwear;

	public int Requied_Mood;

	public bool Bought;

	private void Start()
	{
		inventory = Object.FindObjectOfType<Inventory_Script>();
		if ((bool)Cloth_Render)
		{
			if (id == 0 || id == 1 || id == 2 || id == 3)
			{
				if (Variant == 1)
				{
					base_color = Cloth_Render.material.color;
				}
			}
			else
			{
				base_color = Cloth_Render.material.color;
			}
		}
		if (Wardrobe_Cloth)
		{
			Bought = inventory.data.Clothes[id].Spawned_Cloth[Variant].Bought;
			Check_For_Wear();
		}
	}

	private void Check_For_Wear()
	{
		if ((inventory.data.Clothes[id].Weared && inventory.data.Clothes[id].Current_Variant == Variant) || inventory.data.Clothes[id].Spawned_Cloth[Variant].Spawned || !Bought)
		{
			Deactivate_Button();
		}
	}

	public void Deactivate_Button()
	{
		base.transform.GetChild(0).GetComponent<Text>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		base.transform.GetChild(1).GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
	}

	public void Reactivate_Button()
	{
		base.transform.GetChild(0).GetComponent<Text>().color = new Color(1f, 1f, 1f, 1f);
		base.transform.GetChild(1).GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
	}

	public void Wear_Chosen_Cloth_Variant()
	{
		Bought = inventory.data.Clothes[id].Spawned_Cloth[Variant].Bought;
		if (inventory.data.Test_Game)
		{
			Bought = true;
		}
		if (Bought)
		{
			if (Wardrobe_Cloth)
			{
				inventory.Wardrobe_Wear = true;
				if (inventory.data.Clothes[id].Weared && inventory.data.Clothes[id].Current_Variant != Variant && !inventory.data.Clothes[id].Spawned_Cloth[Variant].Spawned)
				{
					inventory.Replace_Weared_Cloth(id, Variant);
				}
			}
			if (Reqied_Unwear.Length == 0)
			{
				if (Wardrobe_Cloth)
				{
					Wear_New_Cloth();
				}
				else if (inventory.Player.Happiness >= (float)Requied_Mood)
				{
					Wear_New_Cloth();
				}
				else
				{
					Not_Enough_Mood();
				}
				return;
			}
			inventory.Wardrobe_Wear = true;
			bool flag = false;
			for (int i = 0; i < Reqied_Unwear.Length; i++)
			{
				if (inventory.data.Clothes[Reqied_Unwear[i]].Weared)
				{
					flag = true;
				}
			}
			if (flag)
			{
				inventory.Player.Interface_anim.SetTrigger("Tip_Cloth");
				for (int j = 0; j < inventory.cloth_tip_icons.Length; j++)
				{
					bool flag2 = false;
					for (int k = 0; k < Reqied_Unwear.Length; k++)
					{
						if (Reqied_Unwear[k] == j)
						{
							flag2 = true;
						}
					}
					if (inventory.cloth_tip_icons[j] != null)
					{
						inventory.cloth_tip_icons[j].gameObject.SetActive(!flag2);
					}
				}
			}
			else if (Wardrobe_Cloth)
			{
				Wear_Other_Variant();
			}
			else if (inventory.Player.Happiness >= (float)Requied_Mood)
			{
				Wear_Other_Variant();
			}
			else
			{
				Not_Enough_Mood();
			}
		}
		else
		{
			inventory.Player.Speak(inventory.Not_Bought_Speech);
			inventory.Player.anim.SetTrigger("No");
		}
	}

	private void Not_Enough_Mood()
	{
		inventory.Player.Dont_Want_Undress();
		if (inventory.data.Language == 0)
		{
			inventory.Player.interface_script.Rox_Interface.Mood_Text.text = "Чтобы снять " + inventory.Clothes[id].Names[inventory.data.Language] + " настроение должно быть выше " + Requied_Mood;
		}
		if (inventory.data.Language == 1)
		{
			inventory.Player.interface_script.Rox_Interface.Mood_Text.text = "To take off " + inventory.Clothes[id].Names[inventory.data.Language] + " happiness has to be above " + Requied_Mood;
		}
		inventory.Player.Interface_anim.SetTrigger("Tip_Mood");
	}

	public void Wear_Other_Variant()
	{
		if (inventory.data.Clothes[id].Weared && inventory.data.Clothes[id].Current_Variant != Variant)
		{
			inventory.Replace_Weared_Cloth(id, Variant);
		}
		inventory.Clothes[id].Chosen_Variant = Variant;
		inventory.Wear(id);
		Check_For_Wear();
	}

	public void Wear_New_Cloth()
	{
		inventory.Clothes[id].Chosen_Variant = Variant;
		inventory.Wear(id);
		Check_For_Wear();
	}

	public void Highlight_Mat(bool Point_In)
	{
		if (Point_In)
		{
			if (id != 3)
			{
				Cloth_Render.material.color = new Color(2f, 2f, 0f, 1f);
				return;
			}
			Cloth_Render.materials[0].color = new Color(2f, 2f, 0f, 1f);
			Cloth_Render.materials[1].color = new Color(2f, 2f, 0f, 1f);
		}
		else
		{
			if (!Cloth_Render)
			{
				return;
			}
			if (id == 0 || id == 1 || id == 2 || id == 3)
			{
				if (Variant == 1)
				{
					Cloth_Render.material.color = base_color;
				}
				else
				{
					Cloth_Render.material.color = inventory.data.Clothes[id].main_color;
				}
			}
			else
			{
				Cloth_Render.material.color = base_color;
			}
		}
	}
}
