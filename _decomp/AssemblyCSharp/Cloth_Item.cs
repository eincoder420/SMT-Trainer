using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Cloth_Item : MonoBehaviour
{
	public int id;

	public int variant;

	public string[] Names_Cloth;

	[HideInInspector]
	public Text Text_Name;

	private Inventory_Script inventory;

	public bool Have_Requiement;

	public int[] Reqied_Unwear;

	private void Awake()
	{
		inventory = Object.FindObjectOfType<Inventory_Script>();
		Text_Name = GetComponentInChildren<Text>(includeInactive: true);
		if ((bool)inventory)
		{
			Text_Name.text = Names_Cloth[inventory.data.Language];
		}
		Display_Cloth_Name();
		if ((bool)inventory)
		{
			inventory.Clothes[id].Dropped_cloth[variant] = this;
		}
	}

	public void Display_Cloth_Name()
	{
		PauseMenuScript pauseMenuScript = Object.FindObjectOfType<PauseMenuScript>();
		if ((bool)inventory)
		{
			Text_Name.gameObject.SetActive(inventory.data.Display.Show_Cloth_Names && !pauseMenuScript.Playing_Scene);
		}
	}

	public void Wear()
	{
		inventory.Wardrobe_Wear = false;
		bool flag = false;
		if (id == 0)
		{
			inventory.Check_For_Toy_Inside();
			if (inventory.data.Have_Toy_Inside)
			{
				flag = true;
				inventory.Player.anim.SetBool("Have_Toy", value: true);
				inventory.Player.Speak(inventory.Player.Cant_Wear_With_Toy);
			}
		}
		if (flag)
		{
			return;
		}
		inventory.Player.anim.SetBool("Have_Toy", value: false);
		if (!Have_Requiement)
		{
			if (inventory.data.Clothes[id].Weared && inventory.data.Clothes[id].Current_Variant != variant)
			{
				inventory.Replace_Weared_Cloth(id, variant);
			}
			StartCoroutine(Wait_For_Pick_Up());
			inventory.Clothes[id].Chosen_Variant = variant;
			inventory.Wear(id);
			return;
		}
		bool flag2 = false;
		for (int i = 0; i < Reqied_Unwear.Length; i++)
		{
			if (inventory.data.Clothes[Reqied_Unwear[i]].Weared)
			{
				flag2 = true;
			}
		}
		if (flag2)
		{
			inventory.Player.Interface_anim.SetTrigger("Tip_Cloth");
			return;
		}
		inventory.Player.anim.SetTrigger("Accept_Requied_Unwear");
		if (inventory.data.Clothes[id].Weared && inventory.data.Clothes[id].Current_Variant != variant)
		{
			inventory.Replace_Weared_Cloth(id, variant);
		}
		StartCoroutine(Wait_For_Pick_Up());
		inventory.Clothes[id].Chosen_Variant = variant;
		inventory.Wear(id);
	}

	public void Turn_Button(bool On)
	{
		if ((bool)inventory)
		{
			if (inventory.data.Language == 0)
			{
				inventory.Player.ChangeUseName("Надеть " + Names_Cloth[inventory.data.Language] + " (E)", Buy_Point: false);
			}
			if (inventory.data.Language == 1)
			{
				inventory.Player.ChangeUseName("Wear " + Names_Cloth[inventory.data.Language] + " (E)", Buy_Point: false);
			}
			inventory.Turn_Use_Button(On);
		}
	}

	private IEnumerator Wait_For_Pick_Up()
	{
		yield return new WaitForSeconds(0.5f);
		Turn_Button(On: false);
		base.gameObject.SetActive(value: false);
	}
}
