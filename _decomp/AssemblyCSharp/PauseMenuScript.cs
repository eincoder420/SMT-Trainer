using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuScript : MonoBehaviour
{
	[Serializable]
	public struct Interface_Elements
	{
		public Slider Risk_Slider;

		public Slider Experience_Slider;

		public Slider Shame_Slider;

		public Slider Happiness_Slider;

		public Slider Hungryness_Slider;

		public GameObject Shame_Object;

		public GameObject Hungry_Object;

		public GameObject Happiness_Object;

		public GameObject Button_Open;

		public Text Use_Text;

		public Text Buy_Text;

		public Image[] Cloth_Tip_Image;

		public Text tip_text;

		public RectTransform speechWindow;

		public Text Counter;

		public Text Mission_Name;

		public RectTransform Mission_Panel;

		public RectTransform Task_Rect;

		public Text Street_Name;

		public Transform Arrow_Object;

		public Transform Tasks_Object;

		public Transform Time_Object;

		public Transform Sliders_Object;

		public Transform Char_Name_Object;

		public Text Current_Night_Text;

		public Transform Night_Object;

		public Slider offset_camera_slider_right;

		public Slider offset_camera_slider_up;

		public Camera Masturb_Camera;

		public Transform pussy_hairs_mast;

		public Transform hairstyles_mast;

		public Transform piercings_mast;

		public Text Weight_Text;

		public Text Remain_People;

		public Text City_Chat;

		public Text Jerk_Task_Text;

		public Transform Jerk_Task_Object;

		public Transform Task_Progress_Object;

		public Transform Experience_Object;

		public Slider Task_Progress_Slider;

		public Text Mood_Text;

		public Subparam_Window[] settings_subparams;

		public Text Police_Name;

		public Text Police_Whore_Name;

		public Text Police_Owner_Name;

		public Text Level_Text;

		public Text Experience_Text;

		public Text Happiness_Text;

		public Text Tip_Jerk_Text;

		public Text Tip_Cloth_Text;

		public Image Happiness_Color;

		public Image Energy_Color;

		public Image Embarassment_Color;

		public Slider Beer_PowerUp;

		public Slider Food_PowerUp;

		public Slider Energy_PowerUp;

		public Button[] Item_Button;

		public Text[] Remain_Items_Text;

		public Text Wish_Text;

		public Text New_level_text;

		public GameObject Sam_Icon;

		public GameObject Mom_Icon;

		public PositionConstraint Icon_Constraint;

		public Transform Atm_Money_Window;
	}

	public Game_Data data;

	[HideInInspector]
	public Menu_Level_Loader Loader;

	[HideInInspector]
	public Mission_Explorer mission_Explorer;

	public bool GameIsPause;

	public GameObject PauseMenuUI;

	public GameObject SettingsWindow;

	public GameObject Menu_Buttons;

	public Text Hour_Text;

	public Text Minute_Text;

	public Inventory_Script inventory;

	public Roxanne_Control player;

	public Start_Menu menu;

	public Edit_Base edit;

	public Animator interface_anim;

	public AudioSource interface_audio;

	public AudioSource warning_audio;

	public AudioClip Mission;

	public bool Show_Warning_Shame;

	public bool Show_Warning_Happy;

	public TimeSpan time;

	public Interface_Elements Rox_Interface;

	public bool GameIsStarted;

	public bool Mission_Started;

	public Speech How_Works_Happiness;

	public AudioClip Bonus_Sound;

	public AudioClip New_Message;

	public AudioClip New_Call;

	public AudioClip Withdraw_Sound;

	public AudioClip Atm_Button;

	public AudioClip Atm_Money_In;

	public AudioClip Use_Item_Sound;

	public AudioClip Level_Up_Sound;

	public Subparam_Window Items_Window;

	public Subparam_Window Inventory_Window;

	public bool Playing_Scene;

	public bool Mission_Rewarded;

	public bool Start_Showed;

	public bool Sex_Mode;

	public Transform Sex_Object;

	public MouseLook[] mouse_looks;

	public Material[] Spot_Colors;

	[ContextMenu("Изменить внешность")]
	public void Change_Sam()
	{
		player.inventory.Replace_Sam_Clothes_For_Screens();
		edit.Get_Character_Settings();
	}

	public void Activate_Game_Process()
	{
		GameIsStarted = true;
	}

	private void Start()
	{
		interface_anim.Play("Darkness_Ends");
		StartCoroutine(Show_Time_Delay());
		Rox_Interface.Arrow_Object = inventory.GetComponent<Roxanne_Control>().Arrow_Object;
		Rox_Interface.Char_Name_Object = inventory.GetComponent<Roxanne_Control>().Char_Name;
		Show_Display_Elements();
		Close_All_Windows();
		Set_Night();
		Try_Rain();
		Rox_Interface.Police_Name.text = menu.data.Name + ",";
		Rox_Interface.Police_Name.text = menu.data.Whore_Name;
		Rox_Interface.Police_Name.text = menu.data.Player_Name2;
		for (int i = 0; i < data.items.Remain_Items.Length; i++)
		{
			Show_Count_Items(i);
		}
		if (mission_Explorer.mission().Type == Mission_Type.sex)
		{
			Sex_Mode = true;
		}
		Turn_Sex_Mode(Sex_Mode);
	}

	public void Try_Rain()
	{
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			float slider = UnityEngine.Random.Range(0.5f, 1f);
			Loader.sky_profile.mediumRainIntensity.slider = slider;
		}
		else
		{
			Loader.sky_profile.mediumRainIntensity.slider = 0f;
		}
	}

	public void Turn_Rain(bool On)
	{
		if (On)
		{
			float slider = UnityEngine.Random.Range(0.5f, 1f);
			Loader.sky_profile.mediumRainIntensity.slider = slider;
		}
		else
		{
			Loader.sky_profile.mediumRainIntensity.slider = 0f;
		}
	}

	public void Set_Rain()
	{
		if (Loader.sky_profile.mediumRainIntensity.slider == 0f)
		{
			float slider = UnityEngine.Random.Range(0.25f, 1f);
			Loader.sky_profile.mediumRainIntensity.slider = slider;
		}
		else
		{
			Loader.sky_profile.mediumRainIntensity.slider = 0f;
		}
	}

	public void Turn_Sex_Mode(bool Sex_On)
	{
		if (Loader.level == 1)
		{
			Sex_Mode = Sex_On;
			Loader.mission_Explorer.sex_missions[0].Sex_On_Object.gameObject.SetActive(Sex_Mode);
			Sex_Object.gameObject.SetActive(Sex_Mode);
		}
	}

	public void Fail_Game()
	{
		Fail_Task();
		data.Current_Night = 1;
		Loader.timeController.timeline = 19f;
		data.time = Loader.timeController.timeline;
		Loader.Restore_Audio();
	}

	public void Win_Game()
	{
		player.Count_Night();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (GameIsPause)
			{
				Resume();
			}
			else
			{
				Pause();
			}
			edit.Close_All_Subparams();
		}
		if (Playing_Scene && Input.GetKeyDown(KeyCode.Space))
		{
			Loader.Cinematic_Animator.SetTrigger("Skip");
		}
	}

	public void Start_Task(bool On)
	{
		Mission_Started = On;
		if (On)
		{
			if (!Start_Showed)
			{
				if (inventory.data.Language == 0)
				{
					Show_Mission_Started_Text("ЗАДАНИЕ НАЧАЛОСЬ");
				}
				if (inventory.data.Language == 1)
				{
					Show_Mission_Started_Text("MISSION IS STARTED");
				}
				interface_audio.PlayOneShot(Mission);
				Start_Showed = true;
			}
			if (!inventory.data.explanations.How_Works_Happiness)
			{
				player.Speak(How_Works_Happiness);
				inventory.data.explanations.How_Works_Happiness = true;
			}
		}
		else
		{
			if (inventory.data.Language == 0)
			{
				Show_Mission_Ended_Text("ЗАДАНИЕ ВЫПОЛНЕНО");
			}
			if (inventory.data.Language == 1)
			{
				Show_Mission_Ended_Text("MISSION COMPLETED");
			}
			menu.Save_level_data();
			menu.Save_Data();
			Start_Showed = false;
		}
	}

	public void Fail_Task()
	{
		Mission_Started = false;
		if (inventory.data.Language == 0)
		{
			Show_Mission_Started_Text("ЗАДАНИЕ ПРОВАЛЕНО");
		}
		if (inventory.data.Language == 1)
		{
			Show_Mission_Started_Text("MISSION FAILED");
		}
		mission_Explorer.Restore_Mission_Targets();
		Start_Showed = false;
	}

	public void Show_Mission_Started_Text(string text)
	{
		Rox_Interface.Street_Name.text = text;
		Rox_Interface.Street_Name.color = new Color(1f, 1f, 1f);
		interface_anim.SetTrigger("Show_Street");
	}

	public void Show_Mission_Ended_Text(string text)
	{
		Rox_Interface.Street_Name.text = text;
		Rox_Interface.Street_Name.color = new Color(1f, 1f, 1f);
		interface_anim.SetTrigger("Show_Street");
		if (mission_Explorer.mission().Have_Reward)
		{
			StartCoroutine(Show_Reward());
		}
	}

	private IEnumerator Show_Reward()
	{
		yield return new WaitForSeconds(3f);
		Rox_Interface.Street_Name.color = new Color(1f, 0.8f, 0.8f);
		Rox_Interface.Street_Name.text = mission_Explorer.completed_mission().Reward_Text;
	}

	public void Turn_Warning_Shame(bool On)
	{
		if (On)
		{
			Show_Warning_Shame = true;
			interface_anim.SetTrigger("Warning_Shame");
			warning_audio.Play();
		}
		else
		{
			Show_Warning_Shame = false;
			interface_anim.SetTrigger("Warning_Stop");
			warning_audio.Stop();
		}
	}

	public void Show_Phone_Icon()
	{
		interface_anim.SetTrigger("New_Message");
		interface_audio.PlayOneShot(New_Message);
	}

	public void Show_Call_Icon()
	{
		interface_anim.SetTrigger("New_Call");
		interface_audio.PlayOneShot(New_Call);
		player.smartphone.Have_New_Call = true;
	}

	public void Resume()
	{
		PauseMenuUI.SetActive(value: false);
		Time.timeScale = 1f;
		GameIsPause = false;
		Turn_Cursor();
		Close_All_Windows();
		menu.Set_All_Sounds();
		for (int i = 0; i < mouse_looks.Length; i++)
		{
			mouse_looks[i].enabled = true;
		}
	}

	private void Close_All_Windows()
	{
		SettingsWindow.SetActive(value: false);
	}

	public void Pause()
	{
		PauseMenuUI.SetActive(value: true);
		Menu_Buttons.SetActive(value: true);
		Time.timeScale = 0f;
		GameIsPause = true;
		menu.Turn_Off_Sounds();
		if (interface_audio.isPlaying)
		{
			interface_audio.Stop();
		}
		Turn_Cursor();
		for (int i = 0; i < mouse_looks.Length; i++)
		{
			mouse_looks[i].enabled = false;
		}
	}

	public void Turn_Cursor()
	{
		if (data.Display.Show_Mouse)
		{
			if (GameIsPause)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				Cursor.lockState = CursorLockMode.Confined;
				Cursor.visible = true;
			}
		}
		else if (GameIsPause)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else if (player.inventory.Inventory_Menu || player.inventory.Actions_Menu || player.Speaking || menu.Phone_On)
		{
			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = true;
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	public void Set_Night()
	{
		Rox_Interface.Current_Night_Text.text = inventory.data.Current_Night.ToString();
	}

	private IEnumerator Show_Time_Delay()
	{
		while (base.gameObject.activeInHierarchy)
		{
			Show_Time();
			yield return new WaitForSeconds(1f);
		}
	}

	public void Show_Time()
	{
		Loader.timeController.enabled = GameIsStarted;
		if (GameIsStarted)
		{
			time = TimeSpan.FromHours(Loader.timeController.timeline);
			Hour_Text.text = time.ToString("hh");
			Minute_Text.text = time.ToString("mm");
			player.Speak_Time();
			inventory.data.saved_time = Loader.timeController.timeline;
		}
	}

	public void Show_Display_Elements()
	{
		Rox_Interface.Arrow_Object.gameObject.SetActive(inventory.data.Display.Show_Arrow && GameIsStarted && (bool)mission_Explorer.mission().Target);
		Rox_Interface.Tasks_Object.gameObject.SetActive(inventory.data.Display.Show_Tasks);
		Rox_Interface.Time_Object.gameObject.SetActive(inventory.data.Display.Show_Time);
		Rox_Interface.Sliders_Object.gameObject.SetActive(inventory.data.Display.Show_Sliders);
		Rox_Interface.Char_Name_Object.gameObject.SetActive(inventory.data.Display.Show_Char_Names);
		Find_Floor_Names();
		Turn_Cursor();
	}

	public void Find_Floor_Names()
	{
		Cloth_Item[] array = UnityEngine.Object.FindObjectsOfType<Cloth_Item>();
		PowerUp[] array2 = UnityEngine.Object.FindObjectsOfType<PowerUp>();
		if (array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Display_Cloth_Name();
			}
		}
		if (array2.Length != 0)
		{
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].Display_Item_Name();
			}
		}
	}

	public void Settings_Window()
	{
		SettingsWindow.SetActive(!SettingsWindow.activeInHierarchy);
		Menu_Buttons.SetActive(!SettingsWindow.activeSelf);
		for (int i = 0; i < Rox_Interface.settings_subparams.Length; i++)
		{
			Rox_Interface.settings_subparams[i].Close_Subparams();
		}
		menu.Turn_Off_Sounds();
	}

	public void LoadMenu()
	{
		GameIsPause = false;
		menu.Save_level_data();
		menu.Save_Data();
		SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
	}

	public void QuitGame()
	{
		menu.Save_level_data();
		menu.Save_Data();
		Application.Quit();
	}

	public void Show_Night_Win_Name(string StreetName)
	{
		Rox_Interface.Street_Name.text = StreetName;
		interface_anim.SetTrigger("Show_Win");
	}

	public void Print_In_Chat(string Name, string Speech)
	{
		Rox_Interface.City_Chat.gameObject.SetActive(value: true);
		Rox_Interface.City_Chat.text = Name + ": -" + Speech;
	}

	public void Print_Mom_Call_In_Chat(int id)
	{
		Rox_Interface.City_Chat.gameObject.SetActive(value: true);
		if (data.Language == 0)
		{
			Rox_Interface.City_Chat.text = "Мачеха " + data.Name2 + ": -" + data.Mom_Speeches[id];
		}
		if (data.Language == 1)
		{
			Rox_Interface.City_Chat.text = data.Name + "'s Stepmother: -" + data.Mom_Speeches_1[id];
		}
	}

	public void Play_Power_Up_Sound()
	{
		interface_audio.PlayOneShot(Use_Item_Sound);
	}

	public void Use_Item(int Type)
	{
		if (Type == 0 && data.items.Remain_Items[0] > 0 && !player.Drinking)
		{
			player.Start_Drinking(Booze: true);
			data.items.Items_Used[0]++;
		}
		if (Type == 1 && data.items.Remain_Items[1] > 0 && !player.Eating)
		{
			player.Start_Eating();
			data.items.Items_Used[1]++;
		}
		if (Type == 2 && data.items.Remain_Items[2] > 0 && !player.Drinking)
		{
			player.Start_Drinking(Booze: false);
			data.items.Items_Used[2]++;
		}
		for (int i = 0; i < data.items.Remain_Items.Length; i++)
		{
			if (data.items.Remain_Items[i] > 0 && i == Type)
			{
				data.items.Remain_Items[i]--;
			}
			Show_Count_Items(i);
		}
	}

	public void Show_Count_Items(int a)
	{
		Rox_Interface.Remain_Items_Text[a].text = " x " + data.items.Remain_Items[a];
		Rox_Interface.Item_Button[a].interactable = data.items.Remain_Items[a] > 0;
	}

	public void Take_Item(int Type)
	{
		for (int i = 0; i < data.items.Remain_Items.Length; i++)
		{
			if (i == Type)
			{
				data.items.Remain_Items[i]++;
				Rox_Interface.Remain_Items_Text[i].text = " x " + data.items.Remain_Items[i];
				interface_audio.PlayOneShot(Bonus_Sound);
				if (!Items_Window.params_open)
				{
					Items_Window.Switch_Subparam();
				}
			}
			Rox_Interface.Item_Button[i].interactable = data.items.Remain_Items[i] > 0;
		}
	}

	public void Choose_Money_Count(int value)
	{
		if (!player.Withdrawaling)
		{
			Withdrawal_Money(value);
			interface_audio.PlayOneShot(Atm_Button);
		}
	}

	public void Withdrawal_Money(int value)
	{
		if (inventory.data.money.Remain_Atm_Balance >= value)
		{
			StartCoroutine(Waiting_For_Money(value));
			return;
		}
		interface_anim.SetTrigger("New_Weight");
		if (data.Language == 0)
		{
			Rox_Interface.Weight_Text.text = "Невозможно выполнить операцию. На балансе - " + inventory.data.money.Remain_Atm_Balance + " йен";
		}
		if (data.Language == 1)
		{
			Rox_Interface.Weight_Text.text = "Unable to complete the transaction. Your balance is " + inventory.data.money.Remain_Atm_Balance + " yen";
		}
	}

	public void Take_Money(int price)
	{
		inventory.data.money.Remain_Money -= price;
		interface_anim.SetTrigger("New_Weight");
		if (data.Language == 0)
		{
			Rox_Interface.Weight_Text.text = inventory.data.Name + " потратила " + price + " йен. Денег в наличии -" + inventory.data.money.Remain_Money + " йен";
		}
		if (data.Language == 1)
		{
			Rox_Interface.Weight_Text.text = inventory.data.Name + " spent " + price + " yen. Cash left -" + inventory.data.money.Remain_Money + " yen";
		}
		player.smartphone.Recount_Money();
	}

	private IEnumerator Waiting_For_Money(int value)
	{
		player.Withdrawaling = true;
		yield return new WaitForSeconds(0.5f);
		interface_audio.PlayOneShot(Withdraw_Sound);
		yield return new WaitForSeconds(2.5f);
		data.money.Remain_Atm_Balance -= value;
		data.money.Remain_Money += value;
		player.smartphone.Recount_Money();
		interface_anim.SetTrigger("New_Weight");
		interface_audio.PlayOneShot(Atm_Money_In);
		if (data.Language == 0)
		{
			Rox_Interface.Weight_Text.text = data.Name + " сняла " + value + " йен. Денег в наличии - " + data.money.Remain_Money + " йен";
		}
		if (data.Language == 1)
		{
			Rox_Interface.Weight_Text.text = data.Name + " withrawed " + value + " yen. Cash left - " + data.money.Remain_Money + " yen";
		}
		player.Withdrawaling = false;
	}

	public void Switch_Task_Experience_Slider(bool Experience)
	{
		Rox_Interface.Task_Progress_Object.gameObject.SetActive(!Experience);
		Rox_Interface.Experience_Object.gameObject.SetActive(Experience);
	}

	public void Replace_Speaker(bool Sam)
	{
		Rox_Interface.Sam_Icon.SetActive(Sam);
		Rox_Interface.Mom_Icon.SetActive(!Sam);
		ConstraintSource source = Rox_Interface.Icon_Constraint.GetSource(0);
		ConstraintSource source2 = Rox_Interface.Icon_Constraint.GetSource(1);
		if (Sam)
		{
			source.weight = 1f;
			source2.weight = 0f;
		}
		else
		{
			source.weight = 0f;
			source2.weight = 1f;
		}
		Rox_Interface.Icon_Constraint.SetSource(0, source);
		Rox_Interface.Icon_Constraint.SetSource(1, source2);
	}
}
