using System.Collections;
using UnityEngine;

public class Mission_Explorer : MonoBehaviour
{
	public Transform Missions_Folder;

	public Mission[] missions;

	public Unblock_Missions Important_Missions;

	public Game_Data data;

	[HideInInspector]
	public Animator Interface_anim;

	[HideInInspector]
	public Roxanne_Control player;

	[HideInInspector]
	public PauseMenuScript interface_script;

	public Menu_Level_Loader Loader;

	private bool Playing_End_Speech;

	public bool Earning_Experience;

	public float Remain_Experience;

	public float Dancing_Timer;

	public float Requied_Dance_Time = 5f;

	public Material Standart_Material;

	public Material Blocked_Material;

	public Material Used_Material;

	public Sex_Mission[] sex_missions;

	public Mission_Object[] Mission_Objects;

	public Replacable_Objects replacable_Objects;

	public int Cloth_Exp()
	{
		int num = 0;
		if (!data.Clothes[0].Weared)
		{
			num += 5;
		}
		if (!data.Clothes[1].Weared)
		{
			num += 5;
		}
		if (!data.Clothes[2].Weared)
		{
			num += 10;
		}
		if (!data.Clothes[3].Weared)
		{
			num += 10;
		}
		return num;
	}

	public string Name_Cloth()
	{
		string result = "";
		if (data.Clothes[0].Weared || data.Clothes[1].Weared)
		{
			if (data.Language == 0)
			{
				result = " в белье ";
			}
			if (data.Language == 1)
			{
				result = " in underwear ";
			}
		}
		if (data.Clothes[2].Weared || data.Clothes[3].Weared)
		{
			if (data.Language == 0)
			{
				result = " в одежде ";
			}
			if (data.Language == 1)
			{
				result = " in clothes ";
			}
		}
		return result;
	}

	public void Complete_Speak_Mission()
	{
		if (mission().Type == Mission_Type.speak)
		{
			Complete_Mission();
		}
	}

	public void Complete_Food_Mission()
	{
		if (mission().Type == Mission_Type.food)
		{
			Complete_Mission();
		}
	}

	public void Complete_Buy_Energy_Mission()
	{
		if (mission().Type == Mission_Type.food)
		{
			Complete_Mission();
		}
	}

	public void Complete_Buy_Mission()
	{
		if (mission().Type == Mission_Type.buy)
		{
			Mission_Counter();
		}
	}

	public void Complete_Use_Mission()
	{
		if (mission().Type == Mission_Type.use)
		{
			Complete_Mission();
		}
	}

	public void Complete_Drop_Trash_Mission()
	{
		if (mission().Type == Mission_Type.use)
		{
			Complete_Mission();
		}
	}

	public void Complete_Take_Vibrator_Mission()
	{
		if (mission().Type == Mission_Type.use)
		{
			Complete_Mission();
			Unblock_Param(2);
		}
	}

	public void Complete_Take_Noodles_Part()
	{
		if (mission().Type == Mission_Type.use)
		{
			Unblock_Param(3);
		}
	}

	public void Complete_Take_Card_Mission()
	{
		if (mission().Type == Mission_Type.use)
		{
			Complete_Mission();
			data.money.Remain_Atm_Balance += data.Start_Card_Money;
		}
	}

	public void Complete_Take_Phone_Mission()
	{
		if (mission().Type == Mission_Type.use)
		{
			Complete_Mission();
			Unblock_Param(0);
		}
	}

	public void Complete_Take_Mission()
	{
		if (mission().Type == Mission_Type.use)
		{
			Complete_Mission();
		}
	}

	public void Complete_Toy_Mission(bool Inside)
	{
		if (mission().Type == Mission_Type.toy_in && Inside)
		{
			Complete_Mission();
			Unblock_Param(1);
		}
		if (mission().Type == Mission_Type.toy_out && !Inside)
		{
			Complete_Mission();
		}
		data.progress_data.Fucked_Toy++;
	}

	public void Complete_Go_To_Point_Mission()
	{
		if (mission().Type == Mission_Type.go_to)
		{
			Complete_Mission();
		}
	}

	public void Complete_Go_Inside_House_Mission(House_Place House_Spot)
	{
		bool flag = false;
		if (mission().Mission_Go_To_Spots.Length != 0)
		{
			for (int i = 0; i < mission().Mission_Go_To_Spots.Length; i++)
			{
				if (House_Spot == mission().Mission_Go_To_Spots[i])
				{
					flag = true;
				}
			}
		}
		else
		{
			flag = true;
		}
		if (mission().Type == Mission_Type.go_inside && flag)
		{
			Complete_Mission();
		}
	}

	public void Check_Spot_For_Available(Jerk_Place jerk_Place)
	{
	}

	public void Complete_Jerk_Mission()
	{
		if (interface_script.Mission_Started && mission().Type == Mission_Type.jerk)
		{
			Complete_Mission();
		}
	}

	public void Jerk_Task_Start(string Task_Text)
	{
		bool flag = false;
		bool flag2 = !data.Clothes[0].Weared && !data.Clothes[1].Weared && !data.Clothes[2].Weared && !data.Clothes[3].Weared;
		if (mission().Mission_Jerk_Spots.Length != 0)
		{
			for (int i = 0; i < mission().Mission_Jerk_Spots.Length; i++)
			{
				if (player.Current_Jerk_Place == mission().Mission_Jerk_Spots[i])
				{
					flag = true;
				}
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			if (mission().Unwear)
			{
				if (flag2)
				{
					interface_script.Start_Task(On: true);
				}
				else
				{
					player.Speak(player.Need_To_Be_Naked);
				}
			}
			else
			{
				interface_script.Start_Task(On: true);
			}
		}
		if (player.Happiness > 90f)
		{
			player.Happiness = 90f;
		}
		interface_script.Rox_Interface.Jerk_Task_Text.text = Task_Text;
		interface_script.Rox_Interface.Experience_Slider.value = data.progress_data.Level_Experience;
		interface_script.Rox_Interface.Experience_Slider.maxValue = data.progress_data.Requied_Experience[data.progress_data.Jerk_Level];
		interface_script.Rox_Interface.Experience_Text.text = "EXP." + data.progress_data.Level_Experience + " / " + data.progress_data.Requied_Experience[data.progress_data.Jerk_Level];
	}

	private IEnumerator Start_Dancing_Task()
	{
		while (Dancing_Timer < Requied_Dance_Time)
		{
			yield return new WaitForSeconds(0.1f);
			Dancing_Timer += 0.1f;
			interface_script.Rox_Interface.Task_Progress_Slider.value = Dancing_Timer * (interface_script.Rox_Interface.Task_Progress_Slider.maxValue / Requied_Dance_Time);
		}
		player.Current_Dance_Place.Earn_Experience();
		Complete_Mission();
		data.progress_data.Danced++;
	}

	public void Dance_Task_Start(string Task_Text)
	{
		if (mission().Type == Mission_Type.dance && !mission().Updress_Skirt && !mission().Unwear)
		{
			Dancing_Timer = 0f;
			Requied_Dance_Time = 10f;
			if (player.Inside_Dance_Place)
			{
				StartCoroutine(Start_Dancing_Task());
			}
		}
		if (mission().Type == Mission_Type.dance && mission().Updress_Skirt)
		{
			Dancing_Timer = 0f;
			Requied_Dance_Time = 10f;
			if (player.Showing && player.Inside_Dance_Place)
			{
				StartCoroutine(Start_Dancing_Task());
			}
		}
		if (mission().Type == Mission_Type.dance && mission().Unwear)
		{
			Dancing_Timer = 0f;
			Requied_Dance_Time = 10f;
			if (!data.Clothes[0].Weared && !data.Clothes[1].Weared && !data.Clothes[2].Weared && !data.Clothes[3].Weared && player.Inside_Dance_Place)
			{
				StartCoroutine(Start_Dancing_Task());
			}
		}
		if (mission().Type == Mission_Type.dance)
		{
			interface_script.Start_Task(On: true);
		}
		if (player.Happiness > 90f)
		{
			player.Happiness = 90f;
		}
		interface_script.Rox_Interface.Jerk_Task_Text.text = Task_Text;
		interface_script.Rox_Interface.Experience_Slider.value = data.progress_data.Level_Experience;
		interface_script.Rox_Interface.Experience_Slider.maxValue = data.progress_data.Requied_Experience[data.progress_data.Jerk_Level];
		interface_script.Rox_Interface.Experience_Text.text = "EXP." + data.progress_data.Level_Experience + " / " + data.progress_data.Requied_Experience[data.progress_data.Jerk_Level];
	}

	public void Go_Inside_Locker(bool Inside)
	{
		player.inventory.In_Locker_Room = Inside;
		player.inventory.Check_Nake_Level();
	}

	public void Complete_Cloth_Mission()
	{
		if (mission().Type != Mission_Type.clothes)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < mission().cloth_Requiements.Length; i++)
		{
			if (data.Clothes[mission().cloth_Requiements[i].id].Weared != mission().cloth_Requiements[i].Weared)
			{
				flag = false;
			}
		}
		if (flag)
		{
			Complete_Mission();
		}
	}

	public void Complete_Home_Mission()
	{
		if (mission().Type == Mission_Type.home)
		{
			Complete_Mission();
		}
	}

	public void Complete_Sleep_Mission()
	{
		if (mission().Type == Mission_Type.sleep)
		{
			Complete_Mission();
		}
	}

	public void Complete_Phone_Mission()
	{
		if (mission().Type == Mission_Type.call)
		{
			Complete_Mission();
		}
	}

	public void Complete_Pool_Mission()
	{
		if (mission().Type == Mission_Type.swim)
		{
			if (!data.Clothes[2].Weared && !data.Clothes[3].Weared)
			{
				Complete_Mission();
			}
			else
			{
				player.Speak(player.Need_To_Be_Naked);
			}
		}
	}

	public void Complete_Photo_Mission()
	{
		if (mission().Type != Mission_Type.selfie)
		{
			return;
		}
		bool flag = !data.Clothes[2].Weared && !data.Clothes[3].Weared;
		if (interface_script.Mission_Started)
		{
			if (flag)
			{
				Mission_Counter();
			}
			else
			{
				player.Speak(player.Need_To_Be_Naked);
			}
		}
	}

	public void Complete_Show_Mission()
	{
		if (mission().Type == Mission_Type.expose && interface_script.Mission_Started)
		{
			Mission_Counter();
		}
	}

	public void Complete_Run_Mission()
	{
		if (mission().Type != Mission_Type.run)
		{
			return;
		}
		if (!data.Clothes[0].Weared && !data.Clothes[1].Weared && !data.Clothes[2].Weared && !data.Clothes[3].Weared && !data.Clothes[6].Weared)
		{
			mission().Targets_Folder.GetChild(mission().current).gameObject.SetActive(value: false);
			if (mission().current == 0)
			{
				interface_script.Start_Task(On: true);
			}
			Mission_Counter();
			if (mission().current < mission().max)
			{
				mission().Targets_Folder.GetChild(mission().current).gameObject.SetActive(value: true);
			}
			interface_script.interface_audio.PlayOneShot(interface_script.Bonus_Sound);
		}
		else
		{
			player.Speak(player.Need_To_Be_Naked);
		}
	}

	public void Complete_Sex_Mission()
	{
		if (mission().Type == Mission_Type.sex)
		{
			Mission_Counter();
			if (mission().current == mission().max)
			{
				completed_mission().Scenario_Objects.Turn_By_Script_Objects[0].Object.gameObject.SetActive(value: true);
			}
		}
	}

	public Mission mission()
	{
		return missions[data.progress_data.Mission_Progress];
	}

	public Mission completed_mission()
	{
		return missions[data.progress_data.Mission_Progress - 1];
	}

	public void Restore_Mission_Targets()
	{
		mission().current = 0;
		if ((bool)mission().Targets_Folder)
		{
			for (int i = 0; i < mission().Targets_Folder.childCount; i++)
			{
				if (i == 0)
				{
					mission().Targets_Folder.GetChild(i).gameObject.SetActive(value: true);
				}
				else
				{
					mission().Targets_Folder.GetChild(i).gameObject.SetActive(value: false);
				}
			}
			Mark_Mission_Target();
		}
		Show_Task_Text();
	}

	public void Set_Missions_Order()
	{
		missions = Missions_Folder.GetComponentsInChildren<Mission>(includeInactive: true);
		for (int i = 0; i < missions.Length; i++)
		{
			missions[i].GetComponent<Mission>().Id = i;
			string text = missions[i].GetComponent<Mission>().Mission_Name[1];
			missions[i].name = text + " " + i;
		}
		Check_Mission_Objects();
	}

	private void Start()
	{
		if (!data.Start_Video_Showed && data.progress_data.Mission_Progress == 0 && Loader.level == 1)
		{
			interface_script.GameIsStarted = false;
			Loader.Cut_Scene[0].SetActive(value: true);
			data.Start_Video_Showed = true;
			data.Entered_level = true;
		}
		else
		{
			interface_script.Activate_Game_Process();
			Play_Mission();
		}
		StartCoroutine(Earning_Task_Experience());
		interface_script.Rox_Interface.Experience_Slider.value = data.progress_data.Level_Experience;
		interface_script.Rox_Interface.Level_Text.text = data.progress_data.Jerk_Level.ToString();
		interface_script.Rox_Interface.Experience_Text.text = "EXPERIENCE " + data.progress_data.Level_Experience + " / " + data.progress_data.Requied_Experience[data.progress_data.Jerk_Level];
		if ((bool)mission().Event_Object)
		{
			mission().Event_Object.SetActive(value: true);
		}
		Unblock_Ability();
	}

	public void Check_Mission_Objects()
	{
		for (int i = 0; i < Mission_Objects.Length; i++)
		{
			for (int j = 0; j < Mission_Objects[i].Objects.Length; j++)
			{
				if ((bool)Mission_Objects[i].Objects[j].gameObject)
				{
					GameObject gameObject = Mission_Objects[i].Objects[j].gameObject;
					bool num = Mission_Objects[i].Appear != null && Mission_Objects[i].Disappear != null;
					bool flag = Mission_Objects[i].Appear != null && Mission_Objects[i].Disappear == null;
					bool flag2 = Mission_Objects[i].Appear == null && Mission_Objects[i].Disappear != null;
					if (num)
					{
						gameObject.SetActive(mission().Id >= Mission_Objects[i].Appear.Id && mission().Id < Mission_Objects[i].Disappear.Id);
					}
					if (flag)
					{
						gameObject.SetActive(mission().Id >= Mission_Objects[i].Appear.Id);
					}
					if (flag2)
					{
						gameObject.SetActive(mission().Id <= Mission_Objects[i].Disappear.Id);
					}
					if ((bool)Mission_Objects[i].Deactivate_Temporary)
					{
						gameObject.SetActive(mission().Id != Mission_Objects[i].Deactivate_Temporary.Id);
					}
				}
			}
		}
		Unblock_Ability();
	}

	public void Unblock_Ability()
	{
		if (Loader.level == 1)
		{
			Have_Vibrator_Completed(mission().Id >= Important_Missions.Have_Vibrator.Id);
			Can_Undress_Completed(mission().Id >= Important_Missions.Can_Undress.Id);
			Have_Phone_Completed(mission().Id >= Important_Missions.Have_Phone.Id);
			Have_Wardrobe_Completed(mission().Id >= Important_Missions.Have_Wardrobe.Id);
			Have_Item_Completed(mission().Id >= Important_Missions.Have_Item.Id);
		}
	}

	public void Unblock_Param(int id)
	{
		interface_script.Inventory_Window.Unblock_Param(id);
	}

	public void Can_Undress_Completed(bool On)
	{
		if (On)
		{
			interface_script.Inventory_Window.Unblock_Quiet(1);
		}
	}

	public void Have_Vibrator_Completed(bool On)
	{
		if (On)
		{
			interface_script.Inventory_Window.Unblock_Quiet(2);
		}
	}

	public void Have_Phone_Completed(bool On)
	{
		if (On)
		{
			interface_script.Inventory_Window.Unblock_Quiet(0);
			data.progress_data.Have_Phone = On;
		}
	}

	public void Have_Item_Completed(bool On)
	{
		if (On)
		{
			interface_script.Inventory_Window.Unblock_Quiet(3);
		}
	}

	public void Have_Wardrobe_Completed(bool On)
	{
		if (On)
		{
			data.progress_data.Have_Wardrobe = On;
		}
	}

	public void Play_Mission()
	{
		StartCoroutine(Delay_Task());
	}

	private IEnumerator Delay_Task()
	{
		if ((bool)mission().Event_Object)
		{
			mission().Event_Object.SetActive(value: true);
		}
		Check_Mission_Objects();
		if (Playing_End_Speech)
		{
			yield return new WaitForSeconds(7f);
			Playing_End_Speech = false;
		}
		yield return new WaitForSeconds(1f);
		if ((bool)mission().Mission_Start_Speech)
		{
			player.Speak(mission().Mission_Start_Speech);
		}
		yield return new WaitForSeconds(3f);
		Interface_anim.SetTrigger("Show_Task");
		Mark_Mission_Target();
		interface_script.Rox_Interface.Arrow_Object.gameObject.SetActive(interface_script.inventory.data.Display.Show_Arrow);
		Show_Task_Text();
		if ((bool)mission().Target)
		{
			Check_For_Outside_Position();
		}
	}

	public void Complete_Mission()
	{
		_ = data.progress_data;
		interface_script.Start_Task(On: false);
		Interface_anim.SetTrigger("Task_Done");
		Show_Arrow_Target(On: false);
		if ((bool)mission().Mission_End_Speech)
		{
			player.Speak(mission().Mission_End_Speech);
			Playing_End_Speech = true;
		}
		data.progress_data.Mission_Progress++;
		if ((bool)completed_mission().End_Mission_Cinematic)
		{
			Play_Mission_Cinematic();
		}
		if (!mission().No_Auto_Load)
		{
			Play_Mission();
		}
	}

	public void Play_Mission_Cinematic()
	{
		completed_mission().End_Mission_Cinematic.transform.GetChild(0).gameObject.SetActive(value: true);
	}

	public void Mission_Counter()
	{
		_ = data.progress_data;
		mission().current++;
		Show_Task_Text();
		Show_Mission_Progress();
	}

	public void Show_Task_Text()
	{
		interface_script.Rox_Interface.Mission_Name.text = mission().Mission_Name[data.Language];
		if (data.Language == 0)
		{
			interface_script.Rox_Interface.Counter.text = "Выполнено " + mission().current + "/" + mission().max;
		}
		if (data.Language == 1)
		{
			interface_script.Rox_Interface.Counter.text = "Completed " + mission().current + "/" + mission().max;
		}
		interface_script.Rox_Interface.Counter.gameObject.SetActive(mission().max > 0 && mission().Show_Counter);
	}

	public void Show_Mission_Progress()
	{
		Mark_Mission_Target();
		if (mission().max > 0)
		{
			if (mission().current >= mission().max)
			{
				Complete_Mission();
			}
		}
		else if (mission().current > 0)
		{
			Complete_Mission();
		}
	}

	public void Mark_Mission_Target()
	{
		if ((bool)mission().Target && interface_script.GameIsStarted && data.Display.Show_Arrow)
		{
			player.Map_arrow.gameObject.SetActive(value: true);
			Show_Arrow_Target(On: true);
			if (!mission().Targets_Folder)
			{
				if (!mission().Outside_Current_Mission_Place)
				{
					player.arrow.target.position = mission().Target.position;
				}
				else
				{
					player.arrow.target.position = mission().Alternative_Target.position;
				}
			}
			else if (!mission().Outside_Current_Mission_Place)
			{
				if (mission().current < mission().Targets_Folder.childCount)
				{
					player.arrow.target.position = mission().Targets_Folder.GetChild(mission().current).position;
				}
				else
				{
					Show_Arrow_Target(On: false);
				}
			}
			else
			{
				player.arrow.target.position = mission().Alternative_Target.position;
			}
		}
		else
		{
			player.Map_arrow.gameObject.SetActive(value: false);
			Show_Arrow_Target(On: false);
		}
	}

	public void Show_Arrow_Target(bool On)
	{
		player.arrow.Have_Target = On;
	}

	private void Check_For_Outside_Position()
	{
		if (Vector3.Distance(player.transform.position, mission().Target.position) > 1000f)
		{
			Out_Of_Mission_Building(Outside: true);
		}
	}

	public void Out_Of_Mission_Building(bool Outside)
	{
		if ((bool)mission().Alternative_Target)
		{
			mission().Outside_Current_Mission_Place = Outside;
		}
		Mark_Mission_Target();
	}

	public void Add_Experience()
	{
		interface_script.Rox_Interface.Experience_Slider.value = data.progress_data.Level_Experience;
		interface_script.Rox_Interface.Experience_Text.text = "EXP." + interface_script.Rox_Interface.Experience_Slider.value + " / " + data.progress_data.Requied_Experience[data.progress_data.Jerk_Level].ToString();
		if (data.progress_data.Level_Experience >= (float)data.progress_data.Requied_Experience[data.progress_data.Jerk_Level])
		{
			data.progress_data.Jerk_Level++;
			data.progress_data.Level_Experience = 0f;
			interface_script.Rox_Interface.Level_Text.text = data.progress_data.Jerk_Level.ToString();
			interface_script.Rox_Interface.Experience_Slider.maxValue = data.progress_data.Requied_Experience[data.progress_data.Jerk_Level];
			if (data.Language == 0)
			{
				interface_script.Rox_Interface.New_level_text.text = "Вы достигли уровня " + data.progress_data.Jerk_Level;
			}
			if (data.Language == 1)
			{
				interface_script.Rox_Interface.New_level_text.text = "You've reached the level " + data.progress_data.Jerk_Level;
			}
			Interface_anim.SetTrigger("New_Level");
			interface_script.interface_audio.PlayOneShot(interface_script.Level_Up_Sound);
			Jerk_Place[] componentsInChildren = Loader.street_Control.Interiors_Folder.GetComponentsInChildren<Jerk_Place>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Set_Jerk_Place_Color();
			}
			componentsInChildren = Loader.street_Control.Street_Folder.GetComponentsInChildren<Jerk_Place>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Set_Jerk_Place_Color();
			}
		}
		interface_script.Rox_Interface.Experience_Slider.value = data.progress_data.Level_Experience;
		if (Remain_Experience > 0f)
		{
			Remain_Experience -= 1f;
			data.progress_data.Sum_Experience += 1f;
			data.progress_data.Level_Experience += 1f;
		}
		else
		{
			Earning_Experience = false;
		}
	}

	private IEnumerator Earning_Task_Experience()
	{
		while (base.gameObject.activeInHierarchy)
		{
			yield return new WaitForSeconds(0.05f);
			interface_script.Switch_Task_Experience_Slider(Earning_Experience);
			if (Earning_Experience)
			{
				Add_Experience();
			}
		}
	}

	public void Toys_Task_Start()
	{
		if (mission().Type == Mission_Type.toy_in || mission().Type == Mission_Type.toy_out)
		{
			interface_script.Start_Task(On: true);
		}
	}

	public void Expose_Task_Start()
	{
		if (mission().Type == Mission_Type.expose)
		{
			interface_script.Start_Task(On: true);
		}
	}

	public void Photo_Task_Start()
	{
		if (mission().Type == Mission_Type.selfie)
		{
			interface_script.Start_Task(On: true);
		}
	}

	public void Try_Skip_Mission()
	{
		if (mission().Type == Mission_Type.go_outside)
		{
			Complete_Mission();
		}
	}

	public void Earn_Experience(int Reward)
	{
		Remain_Experience = Reward;
		Earning_Experience = true;
	}

	public void Replace_Object(GameObject Prefab_Object)
	{
		for (int i = 0; i < replacable_Objects.Original_And_Prefab.Length; i++)
		{
			if (replacable_Objects.Original_And_Prefab[i].Prefab == Prefab_Object)
			{
				GameObject obj = Object.Instantiate(replacable_Objects.Original_And_Prefab[i].Prefab, replacable_Objects.Original_And_Prefab[i].Original.position, replacable_Objects.Original_And_Prefab[i].Original.rotation, null);
				obj.transform.localScale = replacable_Objects.Original_And_Prefab[i].Original.localScale;
				obj.SetActive(value: true);
				obj.GetComponentInChildren<Rigidbody>().AddForce(base.transform.forward * 5f, ForceMode.VelocityChange);
				replacable_Objects.Original_And_Prefab[i].Original.gameObject.SetActive(value: false);
			}
		}
	}

	public void Set_Mom_Progress(int id)
	{
		data.progress_data.Mom_Progress = id;
	}
}
