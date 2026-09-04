using System.Collections;
using UnityEngine;
using UnityEngine.AzureSky;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu_Level_Loader : MonoBehaviour
{
	public enum Game_Mode
	{
		Standart,
		Sex,
		Cinematic
	}

	public int level;

	public Game_Data data;

	public Game_Mode game_mode;

	[HideInInspector]
	public Interior_Place interior_Places;

	[HideInInspector]
	public Street_Control street_Control;

	[HideInInspector]
	public Start_Menu menu;

	[HideInInspector]
	public Roxanne_Control player;

	[HideInInspector]
	public PauseMenuScript interface_script;

	[HideInInspector]
	public Mission_Explorer mission_Explorer;

	[HideInInspector]
	public AzureTimeController timeController;

	[HideInInspector]
	public Animator anim;

	public AzureSkyProfile sky_profile;

	public Transform Camera_player;

	public Text level_name_text;

	public Slider progress_bar;

	public Speech tip_speech;

	public Transform[] Street_Block;

	public Transform Map_Object;

	public Camera Map_Camera;

	public Camera Icon_Camera;

	public Transform[] Sam_Start_Points;

	public GameObject[] Cut_Scene;

	public GameObject[] Cut_Scene_Ending;

	public AudioClip Level_Music;

	public AudioClip Run_Music;

	public AudioClip Win_Music_Demo;

	public bool Playing_Override_Music;

	private AudioClip Overrided_Music;

	public AudioSource Main_Music_Audio;

	public Transform[] Playmode_Objects;

	public Transform[] Cinematic_Objects;

	public Animator Cinematic_Animator;

	public void Find_Components()
	{
		if (!anim)
		{
			anim = GetComponent<Animator>();
		}
		if (!player)
		{
			player = Object.FindObjectOfType<Roxanne_Control>();
		}
		if (!menu)
		{
			menu = Object.FindObjectOfType<Start_Menu>();
		}
		if (!Camera_player)
		{
			Camera_player = Camera.main.transform;
		}
		if (!interface_script)
		{
			interface_script = Object.FindObjectOfType<PauseMenuScript>();
		}
		if (!mission_Explorer)
		{
			mission_Explorer = Object.FindObjectOfType<Mission_Explorer>();
		}
		if (!street_Control)
		{
			street_Control = Object.FindObjectOfType<Street_Control>();
		}
		if (!timeController)
		{
			timeController = Object.FindObjectOfType<AzureTimeController>();
		}
		interface_script.Loader = this;
		interface_script.mission_Explorer = mission_Explorer;
		Set_Sky_And_Time();
		player.Find_Sam_Components();
		player.interface_script = interface_script;
		menu.Loader = this;
		menu.player = player.transform;
		street_Control.menu = menu;
		street_Control.interface_script = interface_script;
		street_Control.player = player.transform;
		mission_Explorer.interface_script = interface_script;
		mission_Explorer.player = player;
		mission_Explorer.Interface_anim = player.Interface_anim;
		mission_Explorer.interface_script = interface_script;
	}

	public void Set_Missions_Order()
	{
		mission_Explorer.Set_Missions_Order();
	}

	public void Find_Entrances()
	{
		interior_Places.Entrances = street_Control.GetComponentsInChildren<House_Place>(includeInactive: true);
		for (int i = 0; i < interior_Places.Entrances.Length; i++)
		{
			bool flag = false;
			int num = 0;
			for (int j = 0; j < interior_Places.interiors.Length; j++)
			{
				if (interior_Places.Entrances[i].Id == interior_Places.interiors[j].id)
				{
					flag = true;
					num = j;
				}
			}
			if (flag)
			{
				interior_Places.Entrances[i].gameObject.name = interior_Places.interiors[num].name;
			}
			else
			{
				interior_Places.Entrances[i].gameObject.name = interior_Places.Entrances[i].Id.ToString();
			}
			interior_Places.Entrances[i].transform.parent = street_Control.Entrances_Folder;
		}
	}

	public void Turn_Off_Collider_Meshes()
	{
		for (int i = 0; i < street_Control.Colliders_Folder.childCount; i++)
		{
			if ((bool)street_Control.Colliders_Folder.GetChild(i).GetComponent<MeshRenderer>())
			{
				street_Control.Colliders_Folder.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
			}
		}
	}

	public void Set_Sky_And_Time()
	{
		sky_profile = timeController.GetComponent<AzureSkyController>().defaultProfileList[0];
		if (data.progress_data.Mission_Progress == 0)
		{
			timeController.timeline = 19f;
			data.time = timeController.timeline;
		}
		else
		{
			timeController.timeline = data.time;
		}
		timeController.dayLength = 96f;
	}

	[ContextMenu("Подготовить уровень")]
	public void Prepare_Level()
	{
		Find_Components();
		Add_Interiors_To_List();
		Check_Interiors_For_Load();
		Set_Task_Places();
		Find_Entrances();
		Set_Missions_Order();
		Turn_Off_Collider_Meshes();
	}

	public void Start_Playing_Scene(string Name)
	{
		interface_script.Playing_Scene = true;
		Turn_Cinematic_Objects(On: true);
		interface_script.Find_Floor_Names();
		Cinematic_Animator.Play(Name);
		player.input.enabled = false;
	}

	public void Turn_Cinematic_Objects(bool On)
	{
		for (int i = 0; i < Playmode_Objects.Length; i++)
		{
			Playmode_Objects[i].gameObject.SetActive(!On);
		}
		for (int j = 0; j < Cinematic_Objects.Length; j++)
		{
			Cinematic_Objects[j].gameObject.SetActive(On);
		}
	}

	public void End_Scene(int id)
	{
		Cut_Scene_Ending[id].gameObject.SetActive(value: true);
		interface_script.Playing_Scene = false;
		Turn_Cinematic_Objects(On: false);
		interface_script.Find_Floor_Names();
		player.input.enabled = true;
		interface_script.Activate_Game_Process();
		mission_Explorer.Play_Mission();
	}

	private void Awake()
	{
		Prepare_Level();
		player.Init_Sam();
		if (!data.Loaded_game)
		{
			Spawn_Player();
		}
		else
		{
			Spawn_Player_To_Saved_Position();
		}
		data.Loaded_game = false;
		StartCoroutine(Check_For_Player_Fall());
		Check_Map();
	}

	public void Spawn_Player()
	{
		if (data.saved_data.Inside_Building == -1)
		{
			Spawn_Player_To_Start_Position();
		}
		else
		{
			int num = -1;
			for (int i = 0; i < interior_Places.interiors.Length; i++)
			{
				if (interior_Places.interiors[i].id == data.saved_data.Inside_Building)
				{
					num = i;
				}
			}
			if (num != -1)
			{
				Spawn_Player_To_Interior_Position(num);
			}
			else
			{
				Spawn_Player_To_Start_Position();
			}
			if (data.progress_data.Mission_Progress == 0)
			{
				data.saved_data.Interior_Out_Position = Sam_Start_Points[0].position;
				data.saved_data.Interior_Out_Rotation = Sam_Start_Points[0].eulerAngles;
			}
		}
		if (!player.input.enabled)
		{
			player.input.enabled = true;
		}
		if (!player.controller.enabled)
		{
			player.controller.enabled = true;
		}
		menu.Save_level_data();
	}

	private void Spawn_Player_To_Interior_Position(int Interior_Id)
	{
		player.transform.position = interior_Places.interiors[Interior_Id].Start_Place.transform.position;
		player.transform.rotation = interior_Places.interiors[Interior_Id].Start_Place.transform.rotation;
	}

	private void Spawn_Player_To_Start_Position()
	{
		player.transform.position = Sam_Start_Points[data.saved_data.Spawn_position_id].transform.position;
		player.transform.rotation = Sam_Start_Points[data.saved_data.Spawn_position_id].transform.rotation;
	}

	public void Spawn_Player_To_Saved_Position()
	{
		player.transform.position = data.saved_data.Sam_Position;
		player.transform.eulerAngles = data.saved_data.Sam_Rotation;
		timeController.timeline = data.saved_time;
	}

	public void Return_To_Start_Point_If_Fall()
	{
		if (player.transform.position.y < -30f)
		{
			player.transform.position = Sam_Start_Points[0].position;
			player.transform.rotation = Sam_Start_Points[0].rotation;
		}
	}

	public void Check_Map()
	{
		if ((bool)Map_Object)
		{
			Map_Object.gameObject.SetActive(value: false);
		}
	}

	public void Add_Interiors_To_List()
	{
		interior_Places.interiors = street_Control.Interiors_Folder.GetComponentsInChildren<Interior>(includeInactive: true);
		for (int i = 0; i < interior_Places.interiors.Length; i++)
		{
			interior_Places.interiors[i].name = data.progress_data.Interior_Achieves[interior_Places.interiors[i].id].Name;
		}
	}

	public void Set_Task_Places()
	{
		data.saved_data.Jerk_Places.Name = new string[data.saved_data.Jerk_Places.Used.Length];
		data.saved_data.Dance_Places.Name = new string[data.saved_data.Dance_Places.Used.Length];
		data.saved_data.Toys_Places.Name = new string[data.saved_data.Toys_Places.Used.Length];
		data.saved_data.Sex_Places.Name = new string[data.saved_data.Sex_Places.Used.Length];
		Jerk_Place[] componentsInChildren = street_Control.Street_Folder.GetComponentsInChildren<Jerk_Place>(includeInactive: true);
		Dance_Place[] componentsInChildren2 = street_Control.Street_Folder.GetComponentsInChildren<Dance_Place>(includeInactive: true);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].id = num;
			data.saved_data.Jerk_Places.Name[num] = componentsInChildren[num].gameObject.name;
			num++;
			componentsInChildren[i].Interior_Spot = false;
			componentsInChildren[i].Interior_id = -1;
		}
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].id = num2;
			data.saved_data.Dance_Places.Name[num] = componentsInChildren2[num].gameObject.name;
			num2++;
			componentsInChildren2[j].Interior_Spot = false;
			componentsInChildren2[j].Interior_id = -1;
		}
		for (int k = 0; k < interior_Places.interiors.Length; k++)
		{
			interior_Places.interiors[k].jerk_places = interior_Places.interiors[k].GetComponentsInChildren<Jerk_Place>(includeInactive: true);
			for (int l = 0; l < interior_Places.interiors[k].jerk_places.Length; l++)
			{
				Jerk_Place jerk_Place = interior_Places.interiors[k].jerk_places[l];
				jerk_Place.id = num;
				data.saved_data.Jerk_Places.Name[num] = jerk_Place.gameObject.name;
				num++;
				jerk_Place.Interior_Spot = true;
				jerk_Place.Interior_id = interior_Places.interiors[k].id;
			}
			for (int m = 0; m < interior_Places.interiors[k].dance_places.Length; m++)
			{
				Dance_Place dance_Place = interior_Places.interiors[k].dance_places[m];
				dance_Place.id = num2;
				data.saved_data.Dance_Places.Name[num] = dance_Place.gameObject.name;
				num2++;
				dance_Place.Interior_Spot = true;
				dance_Place.Interior_id = interior_Places.interiors[k].id;
			}
		}
		menu.Save_Data();
	}

	public void Check_Interiors_For_Load()
	{
		interior_Places.Check_Interiors_For_Load();
	}

	public void Location_Visited(int id)
	{
		data.saved_data.Inside_Building = id;
		Count_Visit(id);
		Show_Place_Name(id);
	}

	public void Visit_Street_Place(int id)
	{
		if (data.saved_data.Inside_Building != id)
		{
			Location_Visited(id);
		}
		else
		{
			data.saved_data.Inside_Building = -1;
		}
		menu.Save_level_data();
	}

	public void Count_Visit(int id)
	{
		if (data.progress_data.Interior_Achieves[id].Progress == 0)
		{
			player.Get_Interior_Achieve(id);
		}
	}

	public void Show_Place_Name(int id)
	{
		interface_script.Rox_Interface.Street_Name.text = data.progress_data.Interior_Achieves[id].Name_Achieve[data.Language];
		interface_script.interface_anim.SetTrigger("Show_Street");
	}

	public void Switch_Audio(AudioClip Clip)
	{
		Playing_Override_Music = !Playing_Override_Music;
		Overrided_Music = Clip;
		if (!Playing_Override_Music)
		{
			Main_Music_Audio.clip = Level_Music;
		}
		else
		{
			Main_Music_Audio.clip = Overrided_Music;
		}
		Main_Music_Audio.Play();
	}

	public void Restore_Audio()
	{
		if (Main_Music_Audio.clip != Level_Music)
		{
			if (!Playing_Override_Music)
			{
				Main_Music_Audio.clip = Level_Music;
			}
			else
			{
				Main_Music_Audio.clip = Overrided_Music;
			}
			Main_Music_Audio.Play();
		}
	}

	public void Set_Audio(AudioClip Clip)
	{
		Main_Music_Audio.clip = Clip;
		Main_Music_Audio.Play();
	}

	public void Play_Cut_Scene(int scene)
	{
		Cut_Scene[scene].SetActive(value: true);
	}

	public void Save_Time()
	{
		data.time = menu.timeController.timeline;
	}

	public void LoadScene(int id)
	{
		data.Loaded_game = false;
		Save_Time();
		if (id > 0)
		{
			anim.SetTrigger("Start");
			menu.Set_Level_Text(id);
		}
		menu.Main_UI.enabled = false;
		menu.player.GetComponent<Inventory_Script>().Main_Folder.gameObject.SetActive(value: false);
		menu.Loading_Process = true;
		StartCoroutine(LoadSceneAsync(id));
		Debug.Log("Loading");
	}

	private IEnumerator LoadSceneAsync(int id)
	{
		AsyncOperation operation = SceneManager.LoadSceneAsync(id, LoadSceneMode.Single);
		if (id == 1)
		{
			menu.Music_Source.clip = menu.Home_Clip;
		}
		menu.Music_Source.Play();
		if (id == 2)
		{
			menu.Music_Source.clip = menu.City_Clip;
		}
		menu.Music_Source.Play();
		while (!operation.isDone)
		{
			float value = Mathf.Clamp01(operation.progress / 0.9f);
			progress_bar.value = value;
			yield return null;
		}
	}

	public void Count_Night()
	{
		Object.FindObjectOfType<Roxanne_Control>().Count_Night();
	}

	private IEnumerator Check_For_Player_Fall()
	{
		while (base.gameObject.activeInHierarchy)
		{
			Return_To_Start_Point_If_Fall();
			yield return new WaitForSeconds(5f);
		}
	}
}
