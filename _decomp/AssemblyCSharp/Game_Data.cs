using UnityEngine;

[CreateAssetMenu]
public class Game_Data : ScriptableObject
{
	public bool Test_Game;

	public bool first_game;

	public bool Entered_level;

	public bool Start_Video_Showed;

	public bool Loaded_game;

	public string Name;

	public string Name2;

	public string Name3;

	public string Player_Name;

	public string Player_Name2;

	public string Whore_Name;

	public string Owner_Name;

	public int Language;

	public Saved_Data saved_data;

	public Progress_Data progress_data;

	public Level_Data[] Levels;

	public Cloth_Data[] Clothes;

	public Character_data Character;

	public Vector2Int[] Resolutions;

	public Graphics_Data Graphics;

	public Sounds_Data Sounds;

	public Display_Data Display;

	public Room_Data Room;

	public Tatoo_Data[] Tatoo;

	public Toys_Data[] Toys_Pussy;

	public Toys_Data[] Toys_Ass;

	public bool Have_Toy_Inside;

	public Money money;

	public Items items;

	public string[] Mans_Names;

	public string[] Girl_Names;

	public string[] Mans_Names_1;

	public string[] Girl_Names_1;

	public string[] Cops_Names;

	public string[] Mans_First_Seen;

	public string[] Mans_First_Seen_1;

	public string[] Girls_First_Seen;

	public string[] Girls_First_Seen_1;

	public string[] Cops_First_Seen;

	public string[] Hello_Speeches;

	public string[] Hello_Speeches_1;

	public string[] Mom_Speeches;

	public string[] Mom_Speeches_1;

	public string[] Hitten_Speech;

	public string[] Hitten_Speech_1;

	public NPC_Data npc_Data;

	public Explanations explanations;

	public int Current_Night;

	public string Photo_Background_Path;

	public float time;

	public float saved_time;

	public float Current_Happiness;

	public float Start_Happiness;

	public int Start_Card_Money;

	public bool[] Blocked_Param;

	public int Photo_id;
}
