using UnityEngine;
using UnityEngine.UI;

public class PowerUp : MonoBehaviour
{
	private PauseMenuScript Interface_Script;

	public Text Text_Name;

	public void Display_Item_Name()
	{
		if (!Interface_Script)
		{
			Interface_Script = Object.FindObjectOfType<PauseMenuScript>();
		}
		if ((bool)Text_Name)
		{
			Text_Name.gameObject.SetActive(Interface_Script.data.Display.Show_Cloth_Names && !Interface_Script.Playing_Scene);
		}
	}

	public void Use_Item(int Type)
	{
		if (!Interface_Script)
		{
			Interface_Script = Object.FindObjectOfType<PauseMenuScript>();
		}
		Interface_Script.Use_Item(Type);
	}

	public void Take_Item(int Type)
	{
		if (!Interface_Script)
		{
			Interface_Script = Object.FindObjectOfType<PauseMenuScript>();
		}
		Interface_Script.Take_Item(Type);
	}
}
