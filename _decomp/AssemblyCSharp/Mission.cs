using System;
using UnityEngine;

public class Mission : MonoBehaviour
{
	[Serializable]
	public struct Cloth_Requiments
	{
		public int id;

		public bool Weared;
	}

	public int Id;

	public string[] Mission_Name;

	public Mission_Type Type;

	public int current;

	public int max;

	public bool Show_Counter;

	public bool Have_Reward;

	public string Reward_Text;

	public bool No_Auto_Load;

	public bool Outside_Current_Mission_Place;

	public bool Unwear;

	public bool Updress_Skirt;

	public bool Updress_Shirt;

	public Cloth_Requiments[] cloth_Requiements;

	public GameObject Event_Object;

	public Transform Target;

	public Transform Targets_Folder;

	public Transform Alternative_Target;

	public House_Place[] Mission_Go_To_Spots;

	public Jerk_Place[] Mission_Jerk_Spots;

	public Speech Mission_Start_Speech;

	public Speech Mission_End_Speech;

	[Header("Синематики")]
	public Cinematic End_Mission_Cinematic;

	[Header("Объекты")]
	public Scenario_Object_List Scenario_Objects;
}
