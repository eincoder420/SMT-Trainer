using UnityEngine;

public class Interior_Place : MonoBehaviour
{
	public Interior[] interiors;

	public House_Place[] Entrances;

	public Menu_Level_Loader Loader;

	private NPC_generator[] Npcs;

	private void Start()
	{
		Npcs = Loader.street_Control.Interiors_Folder.GetComponentsInChildren<NPC_generator>(includeInactive: true);
		for (int i = 0; i < Npcs.Length; i++)
		{
			Npcs[i].Visible = true;
		}
	}

	public void Low_Rank(int id)
	{
		Speech speech = new Speech();
		speech.speeches = new string[2];
		speech.lenght = 1250;
		string text = Loader.data.progress_data.Rank_Name_0[Loader.data.progress_data.Interior_Achieves[id].Requied_Rank];
		string text2 = Loader.data.progress_data.Rank_Name_1[Loader.data.progress_data.Interior_Achieves[id].Requied_Rank];
		speech.speeches[0] = "*, я не могу пока посетить " + Loader.data.progress_data.Interior_Achieves[id].Name[0] + ". Мне необходимо иметь ранг '" + text + "' или выше";
		speech.speeches[1] = "*, I can't enter the " + Loader.data.progress_data.Interior_Achieves[id].Name[1] + " yet. I need to have the rank '" + text2 + "' or higher";
		Loader.player.anim.SetTrigger("Cant");
		Loader.player.Speak(speech);
	}

	public bool Check_For_Rank(int id)
	{
		return Loader.data.progress_data.Rank >= Loader.data.progress_data.Interior_Achieves[id].Requied_Rank;
	}

	public void Save_Building_Data(int id, bool Inside, Vector3 Out_position, Vector3 Out_Rotation)
	{
		if (Inside)
		{
			Loader.Location_Visited(interiors[id].id);
			Loader.data.saved_data.Interior_Out_Position = Out_position;
			Loader.data.saved_data.Interior_Out_Rotation = Out_Rotation;
		}
		else
		{
			Loader.data.saved_data.Inside_Building = -1;
		}
		Loader.menu.Save_level_data();
		Check_Interiors_For_Load();
	}

	public void Check_Interiors_For_Load()
	{
		for (int i = 0; i < interiors.Length; i++)
		{
			bool flag = Loader.data.saved_data.Inside_Building == interiors[i].id;
			if (!interiors[i].Start_Place)
			{
				interiors[i].Start_Place = interiors[i].Place;
			}
			interiors[i].gameObject.SetActive(flag);
			for (int j = 0; j < interiors[i].Deactivate_If_Inside.Length; j++)
			{
				interiors[i].Deactivate_If_Inside[j].gameObject.SetActive(!flag);
			}
		}
	}
}
