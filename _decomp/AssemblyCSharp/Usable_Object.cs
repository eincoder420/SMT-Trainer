using Invector;
using UnityEngine;

public class Usable_Object : MonoBehaviour
{
	public string[] Text_Actions;

	public bool Buy_Point;

	public bool Speak_Trigger;

	private void Start()
	{
		if (Buy_Point)
		{
			vRotateObject componentInChildren = base.transform.GetComponentInChildren<vRotateObject>(includeInactive: true);
			componentInChildren.GetComponent<Renderer>().material = Object.FindObjectOfType<PauseMenuScript>().Spot_Colors[0];
			componentInChildren.Set_Light_Color();
		}
	}

	public void Turn_Button(bool On)
	{
		Inventory_Script inventory_Script = Object.FindObjectOfType<Inventory_Script>();
		Jerk_Place component = GetComponent<Jerk_Place>();
		if (Text_Actions.Length == 0)
		{
			Text_Actions = new string[2];
			Text_Actions[0] = "Использовать (E)";
			Text_Actions[1] = "Use (E)";
		}
		if (!inventory_Script)
		{
			return;
		}
		if ((bool)component)
		{
			if (component.Requied_Level > inventory_Script.data.progress_data.Jerk_Level)
			{
				if (inventory_Script.data.Language == 0)
				{
					inventory_Script.Player.ChangeUseName("Требуемый уровень - " + component.Requied_Level + " (E)", Buy_Point: false);
				}
				if (inventory_Script.data.Language == 1)
				{
					inventory_Script.Player.ChangeUseName("Level " + component.Requied_Level + " required", Buy_Point: false);
				}
			}
			else
			{
				inventory_Script.Player.ChangeUseName(Text_Actions[inventory_Script.data.Language] + " (E)", Buy_Point: false);
			}
		}
		else if (Speak_Trigger)
		{
			inventory_Script.Player.ChangeUseName(Text_Actions[inventory_Script.data.Language], Buy_Point: false);
		}
		else
		{
			inventory_Script.Player.ChangeUseName(Text_Actions[inventory_Script.data.Language] + " (E)", Buy_Point);
		}
		inventory_Script.Turn_Use_Button(On);
	}

	public void Speak()
	{
		Roxanne_Control roxanne_Control = Object.FindObjectOfType<Roxanne_Control>();
		if ((bool)roxanne_Control)
		{
			roxanne_Control.Speak(GetComponent<Speech>());
		}
	}
}
