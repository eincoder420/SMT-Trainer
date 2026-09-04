using System;
using System.Collections;
using System.IO;
using AmplifyBloom;
using BeautifyEffect;
using MagicaCloth;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.AzureSky;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Start_Menu : MonoBehaviour
{
	[Serializable]
	public struct Graphic_Settings
	{
		public Text graphics;

		public Text resolution;

		public Slider visibility;

		public Slider Music_Slider;

		public Slider Sound_Slider;

		public Toggle Fullscreen;

		public Toggle Motion;

		public Toggle Vsync;

		public Toggle Shadows;

		public Toggle Bloom;

		public Toggle Occlusion;
	}

	[Serializable]
	public struct Cloth
	{
		public string Name;

		public Transform[] Main_Model;

		public Transform[] Edit_Model;

		public bool Edit_Weared;

		public Texture2D start_texture;

		public Texture2D start_texture2;
	}

	[Serializable]
	public struct Toy
	{
		public Transform Main_Model;

		public Transform Edit_Model;
	}

	[Serializable]
	public struct Names_Text
	{
		public InputField Rox_Name;

		public InputField Rox_Name2;

		public InputField Rox_Name3;

		public InputField Player_Name;

		public InputField Player_Name2;

		public InputField Whore_Name;

		public InputField Owner_Name;
	}

	[Serializable]
	public struct Tools
	{
		public string Name;

		public Text[] texts;

		public Transform[] Parameters_Parent;
	}

	[HideInInspector]
	public Menu_Level_Loader Loader;

	public Transform player;

	public Edit_Base edit_base;

	[HideInInspector]
	public Transform Camera_player;

	[HideInInspector]
	public Text level_name_text;

	[HideInInspector]
	public Slider progress_bar;

	[HideInInspector]
	public Speech tip_speech;

	public Transform Demo_Text;

	public AudioMixer Audio_Mixer;

	public AudioSource Music_Source;

	public AudioClip Home_Clip;

	public AudioClip City_Clip;

	public AudioClip Editor_Clip;

	public AudioClip Menu_Clip;

	public AudioClip Home_Music;

	public AudioClip City_Music;

	public AudioMixerGroup[] Audio_Group;

	public Transform Start_Position;

	public Transform demo_text;

	public Animator Rox_Animator;

	public Animator Edit_Rox_Animator;

	public Animator Cinematic_Animator;

	public Game_Data data;

	public Hidden_Data hidden_data;

	public bool Menu;

	public AudioSource rox_audio;

	public Transform Close_Button;

	public Transform Gallery;

	public Transform Credits;

	public Transform Settings;

	public Transform Achievements;

	public Transform Education;

	public Transform Balance;

	public Transform Whatsnew;

	private PostProcessVolume volume;

	public Animator anim;

	public Transform Main_Menu;

	public Transform Start_Game_Menu;

	public Canvas Main_UI;

	public AzureTimeController timeController;

	public AzureSkyProfile sky_profile;

	public Image[] Load_Images;

	public Transform[] Choose_Name_Menu;

	public Transform[] Menu_Elements;

	public Transform[] Edit_Mode_Deactivate;

	public Transform Edition_Menu;

	public bool In_Edit_Menu;

	public bool Switching_Menu_Process;

	public Text Boosty;

	public Text Support;

	public string[] Boosty_Text;

	public string[] Support_Text;

	public Cloth[] Clothes;

	public Transform[] Clothes_main;

	public Transform[] Clothes_edit;

	public Toy[] Toys_Pussy;

	public Toy[] Toys_Ass;

	public Names_Text[] Names;

	private bool Underwear_On;

	private bool Clothes_On;

	public Transform Cross_Clothes;

	public Transform Cross_Underwear;

	public Button Button_Underwear_On;

	public Button Button_Underwear_Off;

	public Button Continue_Button;

	public Text Game_Name_Text;

	public Text Rotate_Character_Name_Text;

	public Text Playable_Character_Name_Text;

	public Material[] House_Materials;

	public Material[] Poster_Materials;

	public Transform[] Poster;

	public Transform Start_Tatto_Folder;

	public Texture2D[] Start_House_Texture;

	public Texture2D[] Start_Poster_Texture;

	public bool Phone_On;

	public Transform Phone_Object;

	public Image Bought_Image;

	public Text Item_Added_text;

	public Tools[] tools_texts;

	public MagicaBoneSpring[] Boob;

	public bool Loading_Process;

	private Settings_Menu settings_menu;

	public PositionConstraint Chest_Constraint;

	public Transform Input_Mission_Object;

	public Text Input_Mission_progress;

	public void Open_Choose_Name(bool Open)
	{
		Choose_Name_Menu[data.Language].gameObject.SetActive(Open);
	}

	public void Recount_Tools()
	{
		for (int i = 0; i < tools_texts.Length; i++)
		{
			for (int j = 0; j < tools_texts[i].texts.Length; j++)
			{
				tools_texts[i].texts[j].text = "x " + data.items.Remain_Tools[i];
				Text[] componentsInChildren = tools_texts[i].Parameters_Parent[j].GetComponentsInChildren<Text>(includeInactive: true);
				Button[] componentsInChildren2 = tools_texts[i].Parameters_Parent[j].GetComponentsInChildren<Button>(includeInactive: true);
				if (i != 7 && i != 8)
				{
					for (int k = 0; k < componentsInChildren.Length; k++)
					{
						componentsInChildren[k].color = ((data.items.Remain_Tools[i] == 0) ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : new Color(1f, 1f, 1f, 1f));
					}
					for (int l = 0; l < componentsInChildren2.Length; l++)
					{
						if (i != 6)
						{
							componentsInChildren2[l].interactable = data.items.Remain_Tools[i] > 0;
						}
					}
					if (i == 6)
					{
						edit_base.Check_Edit_Haircuts();
					}
				}
				else
				{
					for (int m = 0; m < componentsInChildren.Length; m++)
					{
						componentsInChildren[m].color = ((data.items.Remain_Tools[7] == 0 || data.items.Remain_Tools[8] == 0) ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : new Color(1f, 1f, 1f, 1f));
					}
					for (int n = 0; n < componentsInChildren2.Length; n++)
					{
						componentsInChildren2[n].interactable = data.items.Remain_Tools[7] > 0 && data.items.Remain_Tools[8] > 0;
					}
				}
			}
		}
	}

	public void Use_Tool(int id)
	{
		data.items.Remain_Tools[id]--;
		Recount_Tools();
	}

	public void Item_Added(Buy_Button Item)
	{
		Bought_Image.sprite = Item.Icon.sprite;
		if (Item.Haircut)
		{
			if (data.Language == 0)
			{
				Item_Added_text.text = "Новая прическа доступна в редакторе";
			}
			if (data.Language == 1)
			{
				Item_Added_text.text = "A new hairstyle is available in the editor";
			}
		}
		if (Item.Stuff)
		{
			if (data.Language == 0)
			{
				Item_Added_text.text = "Вы приобрели предмет";
			}
			if (data.Language == 1)
			{
				Item_Added_text.text = "You purchased an item";
			}
		}
		if (Item.Cloth)
		{
			if (data.Language == 0)
			{
				Item_Added_text.text = "Новая вещь добавлена в гардероб";
			}
			if (data.Language == 1)
			{
				Item_Added_text.text = "A new cloth has been added to the wardrobe";
			}
		}
		anim.SetTrigger("Item_Added");
	}

	public void Turn_Smartphone()
	{
		Phone_On = !Phone_On;
		if (Phone_On)
		{
			Rox_Animator.SetBool("Phone", value: true);
			if (Menu)
			{
				Edit_Rox_Animator.SetBool("Phone", value: true);
			}
			anim.SetBool("Phone", value: true);
		}
		else
		{
			Rox_Animator.SetBool("Phone", value: false);
			if (Menu)
			{
				Edit_Rox_Animator.SetBool("Phone", value: false);
			}
			anim.SetBool("Phone", value: false);
		}
	}

	public void Close_All()
	{
		if (Phone_On)
		{
			Turn_Smartphone();
		}
		Gallery.gameObject.SetActive(value: false);
		Settings.gameObject.SetActive(value: false);
		Credits.gameObject.SetActive(value: false);
		Achievements.gameObject.SetActive(value: false);
		Education.gameObject.SetActive(value: false);
		Whatsnew.gameObject.SetActive(value: false);
	}

	public void Close_All_Except(Transform Window)
	{
		if ((bool)Settings)
		{
			Settings.gameObject.SetActive(Settings == Window && !Window.gameObject.activeSelf);
		}
		if ((bool)Credits)
		{
			Credits.gameObject.SetActive(Credits == Window && !Window.gameObject.activeSelf);
		}
		if ((bool)Whatsnew)
		{
			Whatsnew.gameObject.SetActive(Whatsnew == Window && !Window.gameObject.activeSelf);
		}
	}

	public void Check_Occlusion()
	{
		Camera_player.GetComponent<AmplifyOcclusionEffect>().enabled = data.Graphics.Occlusion;
	}

	public void Check_Fullscreen()
	{
		if (data.Graphics.Full_Screen)
		{
			Screen.fullScreen = true;
			Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
		}
		else
		{
			Screen.fullScreen = false;
			Screen.fullScreenMode = FullScreenMode.Windowed;
		}
	}

	public void Change_Motion_Blur()
	{
		volume.profile.GetSetting<MotionBlur>().active = data.Graphics.Motion_blur;
	}

	public void Check_Shadows()
	{
		if (!data.Graphics.Shadows)
		{
			QualitySettings.shadows = ShadowQuality.Disable;
		}
		else
		{
			QualitySettings.shadows = ShadowQuality.All;
		}
	}

	public void Set_All_Sounds()
	{
		Audio_Mixer.SetFloat("Sounds_Volume", data.Sounds.Sound_Volume);
		Audio_Mixer.SetFloat("Music_Volume", data.Sounds.Music_Volume);
		Audio_Mixer.SetFloat("Interface_Volume", data.Sounds.Interface_Volume);
	}

	public void Turn_Off_Sounds()
	{
		Audio_Mixer.SetFloat("Sounds_Volume", -80f);
		Audio_Mixer.SetFloat("Music_Volume", -80f);
		Audio_Mixer.SetFloat("Interface_Volume", -80f);
	}

	public void Set_Camera_And_Fog()
	{
		if (!sky_profile)
		{
			sky_profile = timeController.GetComponent<AzureSkyController>().defaultProfileList[0];
		}
		if (!Menu)
		{
			if (Loader.level == 1)
			{
				data.Graphics.visibility = 30f;
			}
			if (Loader.level == 2)
			{
				data.Graphics.visibility = 500f;
			}
		}
		Camera_player.GetComponent<Camera>().farClipPlane = data.Graphics.visibility;
		sky_profile.globalFogDistance.slider = data.Graphics.visibility;
	}

	public void Change_Vsync()
	{
		if (data.Graphics.Vsync)
		{
			QualitySettings.vSyncCount = 1;
		}
		else
		{
			QualitySettings.vSyncCount = 0;
		}
	}

	public void Set_Graphics()
	{
		QualitySettings.SetQualityLevel(data.Graphics.Graphics_Level, applyExpensiveChanges: true);
		if (data.Graphics.Graphics_Level == 0)
		{
			if (data.Language == 0)
			{
				settings_menu.settings.graphics.text = "НИЗКАЯ";
			}
			if (data.Language == 1)
			{
				settings_menu.settings.graphics.text = "Low";
			}
			Camera_player.GetComponent<Beautify>().enabled = false;
			Camera_player.GetComponent<PostProcessLayer>().enabled = false;
			settings_menu.settings.Motion.interactable = false;
			settings_menu.settings.Bloom.interactable = false;
		}
		if (data.Graphics.Graphics_Level == 1)
		{
			if (data.Language == 0)
			{
				settings_menu.settings.graphics.text = "СРЕДНЯЯ";
			}
			if (data.Language == 1)
			{
				settings_menu.settings.graphics.text = "Medium";
			}
			Camera_player.GetComponent<Beautify>().enabled = false;
			Camera_player.GetComponent<PostProcessLayer>().enabled = true;
			settings_menu.settings.Motion.interactable = true;
			settings_menu.settings.Bloom.interactable = false;
		}
		if (data.Graphics.Graphics_Level == 2)
		{
			if (data.Language == 0)
			{
				settings_menu.settings.graphics.text = "ВЫСОКАЯ";
			}
			if (data.Language == 1)
			{
				settings_menu.settings.graphics.text = "High";
			}
			Camera_player.GetComponent<PostProcessLayer>().enabled = true;
			settings_menu.settings.Motion.interactable = true;
			if (!Menu)
			{
				settings_menu.settings.Bloom.interactable = true;
			}
		}
	}

	public void Change_Resolution()
	{
		Screen.SetResolution(data.Resolutions[data.Graphics.Resolution_level].x, data.Resolutions[data.Graphics.Resolution_level].y, fullscreen: true);
		settings_menu.settings.resolution.text = data.Resolutions[data.Graphics.Resolution_level].x + "/" + data.Resolutions[data.Graphics.Resolution_level].y;
		Check_Fullscreen();
	}

	public void Change_Bloom()
	{
		Camera_player.GetComponent<AmplifyBloomEffect>().enabled = data.Graphics.Occlusion;
	}

	public void Check_Street_Progress()
	{
	}

	public void Load_Data()
	{
		JsonUtility.FromJsonOverwrite(File.ReadAllText(Application.streamingAssetsPath + "/saved_data.json"), data);
	}

	[ContextMenu("Save")]
	public void Save_Data()
	{
		File.WriteAllText(Application.streamingAssetsPath + "/saved_data.json", JsonUtility.ToJson(data));
	}

	public void Forbidden_In_Demo(GameObject Text_Object)
	{
		if (In_Edit_Menu)
		{
			Text_Object.SetActive(value: true);
		}
	}

	private void Awake()
	{
		Time.timeScale = 1f;
		timeController = UnityEngine.Object.FindObjectOfType<AzureTimeController>();
		volume = UnityEngine.Object.FindObjectOfType<PostProcessVolume>();
		anim = GetComponent<Animator>();
		settings_menu = Settings.GetComponent<Settings_Menu>();
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		if (!Menu)
		{
			player = UnityEngine.Object.FindObjectOfType<Roxanne_Control>().transform;
			Rox_Animator = player.GetComponent<Animator>();
			Loader = UnityEngine.Object.FindObjectOfType<Menu_Level_Loader>();
			Game_Name_Text.text = data.Name;
			Playable_Character_Name_Text = player.GetComponent<Roxanne_Control>().Char_Name.GetComponentInChildren<Text>();
			if (data.Language == 0)
			{
				Playable_Character_Name_Text.text = data.Name + ", " + data.Whore_Name + " " + data.Player_Name2;
			}
			if (data.Language == 1)
			{
				Playable_Character_Name_Text.text = data.Name + ", " + data.Player_Name + "'s " + data.Whore_Name;
			}
			Camera_player = Loader.Camera_player;
			level_name_text = Loader.level_name_text;
			progress_bar = Loader.progress_bar;
			tip_speech = Loader.tip_speech;
			_ = Loader.level;
			_ = 2;
		}
		if (Menu)
		{
			Main_UI.gameObject.SetActive(value: true);
			Edition_Menu.gameObject.SetActive(value: false);
			Close_All();
			for (int i = 0; i < Menu_Elements.Length; i++)
			{
				Menu_Elements[i].gameObject.SetActive(value: true);
			}
			for (int j = 0; j < Edit_Mode_Deactivate.Length; j++)
			{
				Edit_Mode_Deactivate[j].gameObject.SetActive(value: true);
			}
			Demo_Text.gameObject.SetActive(hidden_data.Demo);
			data.saved_data.Spawn_position_id = 0;
			if (data.first_game)
			{
				data.Test_Game = false;
				if (data.Language == 0)
				{
					data.Name = "Саманта";
					data.Name2 = "Саманты";
					data.Name3 = "Саманту";
					data.Player_Name = "Игрок";
					data.Player_Name2 = "Игрока";
					data.Whore_Name = "сучка";
					data.Owner_Name = "милый";
				}
				if (data.Language == 1)
				{
					data.Name = "Samantha";
					data.Name2 = "Samantha";
					data.Name3 = "Samantha";
					data.Player_Name = "Player";
					data.Player_Name2 = "Player's";
					data.Whore_Name = "bitch";
					data.Owner_Name = "my lord";
				}
				Back_To_Default();
				data.money.Remain_Atm_Balance = 0;
				data.money.Remain_Money = 0;
				for (int k = 0; k < data.items.Remain_Tools.Length; k++)
				{
					data.items.Remain_Tools[k] = 3;
				}
				for (int l = 0; l < data.items.Haircuts_Bought.Length; l++)
				{
					if (l == 0 || l == 7 || l == 10)
					{
						data.items.Haircuts_Bought[l] = true;
					}
					else
					{
						data.items.Haircuts_Bought[l] = false;
					}
				}
				for (int m = 0; m < data.Clothes.Length; m++)
				{
					for (int n = 0; n < data.Clothes[m].Spawned_Cloth.Length; n++)
					{
						data.Clothes[m].Spawned_Cloth[n].Bought = false;
					}
				}
				data.Clothes[0].Spawned_Cloth[0].Bought = true;
				data.Clothes[1].Spawned_Cloth[0].Bought = true;
				data.Clothes[2].Spawned_Cloth[0].Bought = true;
				data.Clothes[2].Spawned_Cloth[2].Bought = true;
				data.Clothes[3].Spawned_Cloth[0].Bought = true;
				data.Clothes[3].Spawned_Cloth[2].Bought = true;
				data.Clothes[8].Spawned_Cloth[2].Bought = true;
				data.Clothes[10].Spawned_Cloth[0].Bought = true;
				data.Clothes[4].Spawned_Cloth[0].Bought = true;
				data.Clothes[4].Spawned_Cloth[1].Bought = true;
				data.Clothes[5].Spawned_Cloth[0].Bought = true;
				data.Clothes[6].Spawned_Cloth[0].Bought = true;
				data.Clothes[7].Spawned_Cloth[1].Bought = true;
				data.Graphics.Graphics_Level = 1;
				data.Graphics.visibility = 30f;
				data.Graphics.Resolution_level = 1;
				data.Graphics.Occlusion = true;
				data.Graphics.Shadows = false;
				data.Graphics.Full_Screen = false;
				data.Graphics.Motion_blur = true;
				data.Graphics.Vsync = false;
				data.Graphics.Bloom = true;
				data.Graphics.Small_Decor = true;
				data.Sounds.Music_Volume = 0f;
				data.Sounds.Sound_Volume = 0f;
				Change_Resolution();
				data.Display.Show_Tasks = true;
				data.Display.Show_Sliders = true;
				data.Current_Happiness = data.Start_Happiness;
				data.Current_Night = 1;
				data.Character.pussy_hairs = 0;
				data.Character.piercing = 0;
				data.Character.hairstyle = 0;
				data.Character.boobs_size = 0f;
				data.Character.ass_size = 0f;
				data.Character.fatness = 0f;
				data.Character.skincolor = 0;
				data.Character.eye_size = 0.25f;
				data.Character.Eyeshadows = false;
				data.Character.hair_color = data.Character.start_hair_color;
				data.Character.eye_color = data.Character.start_eye_color;
				data.Character.pussy_color = data.Character.start_pussy_color;
				data.Character.finger_color = data.Character.start_finger_color;
				data.Clothes[0].main_color = data.Clothes[0].start_color;
				data.Clothes[1].main_color = data.Clothes[1].start_color;
				data.Clothes[0].Weared = true;
				data.Clothes[0].Current_Variant = 0;
				data.Clothes[1].Weared = true;
				data.Clothes[1].Current_Variant = 0;
				data.Clothes[2].Weared = true;
				data.Clothes[2].Current_Variant = 0;
				data.Clothes[3].Weared = true;
				data.Clothes[3].Current_Variant = 0;
				data.Clothes[4].Weared = true;
				data.Clothes[4].Current_Variant = 0;
				data.Clothes[5].Weared = false;
				data.Clothes[5].Current_Variant = 0;
				data.Clothes[6].Weared = false;
				data.Clothes[6].Current_Variant = 0;
				data.Clothes[7].Weared = false;
				data.Clothes[7].Current_Variant = 0;
				data.Clothes[8].Weared = true;
				data.Clothes[8].Current_Variant = 2;
				data.Clothes[9].Weared = false;
				data.Clothes[9].Current_Variant = 0;
				data.Clothes[10].Weared = false;
				data.Clothes[10].Current_Variant = 0;
				for (int num = 0; num < data.Toys_Pussy.Length; num++)
				{
					data.Toys_Pussy[num].Weared = false;
				}
				for (int num2 = 0; num2 < data.Toys_Ass.Length; num2++)
				{
					data.Toys_Ass[num2].Weared = false;
				}
				data.Clothes[0].Tiling = new Vector3(1f, 1f);
				data.Clothes[1].Tiling = new Vector3(1f, 1f);
				data.Clothes[2].Tiling = new Vector3(1f, 1f);
				data.Clothes[0].Path_To_File = "";
				data.Clothes[1].Path_To_File = "";
				data.Clothes[2].Path_To_File = "";
				data.Clothes[3].Path_To_File = "";
				House_Materials[0].mainTexture = Start_House_Texture[0];
				House_Materials[1].mainTexture = Start_House_Texture[1];
				House_Materials[2].mainTexture = Start_House_Texture[2];
				House_Materials[3].mainTexture = Start_House_Texture[3];
				Poster_Materials[0].mainTexture = Start_Poster_Texture[0];
				Poster_Materials[1].mainTexture = Start_Poster_Texture[1];
				Poster_Materials[2].mainTexture = Start_Poster_Texture[2];
				data.Room.Path_To_File_Poster[0] = "";
				data.Room.Path_To_File_Poster[1] = "";
				data.Room.Path_To_File_Poster[2] = "";
				data.Room.Path_To_File_House[0] = "";
				data.Room.Path_To_File_House[1] = "";
				data.Room.Path_To_File_House[2] = "";
				data.Room.Path_To_File_House[3] = "";
				data.Room.Tiling[0] = new Vector2(1f, 1f);
				data.Room.Tiling[1] = new Vector2(1f, 1f);
				data.Room.Tiling[2] = new Vector2(1f, 1f);
				data.Room.Tiling[3] = new Vector2(1f, 1f);
				data.items.Remain_Items[0] = 0;
				data.items.Remain_Items[1] = 0;
				data.items.Remain_Items[2] = 0;
				for (int num3 = 0; num3 < data.Tatoo.Length; num3++)
				{
					data.Tatoo[num3].Path_To_File_Tatoo = "";
					data.Tatoo[num3].Start_Tiling = Start_Tatto_Folder.GetChild(num3).GetComponent<Renderer>().material.mainTextureScale;
					data.Tatoo[num3].Start_Offset = Start_Tatto_Folder.GetChild(num3).GetComponent<Renderer>().material.mainTextureOffset;
					data.Tatoo[num3].Offset = Start_Tatto_Folder.GetChild(num3).GetComponent<Renderer>().material.mainTextureOffset;
				}
				data.saved_data.Inside_Building = 30;
				Remove_Spawned_Clothes();
				data.Clothes[4].Spawned_Cloth[1].Spawned = true;
				data.Clothes[6].Spawned_Cloth[0].Spawned = true;
				data.Clothes[7].Spawned_Cloth[1].Spawned = true;
				data.time = 19f;
				timeController.timeline = data.time;
			}
			else
			{
				Load_Data();
			}
			Check_Continue_Button();
			Clothes_On = true;
			Underwear_On = true;
			Bra_Shape_Check();
			Set_Edit_Clothes_Names();
			Show_Hide_Cloth();
			Show_Hide_Underwear();
			Set_Names();
			for (int num4 = 0; num4 < Clothes.Length; num4++)
			{
				for (int num5 = 0; num5 < Clothes[num4].Main_Model.Length; num5++)
				{
					if ((bool)Clothes[num4].Main_Model[num5].GetComponent<Renderer>())
					{
						Clothes[num4].Main_Model[num5].GetComponent<Renderer>().enabled = data.Clothes[num4].Weared && data.Clothes[num4].Current_Variant == num5;
					}
					else
					{
						Renderer[] componentsInChildren = Clothes[num4].Main_Model[num5].GetComponentsInChildren<Renderer>(includeInactive: true);
						for (int num6 = 0; num6 < componentsInChildren.Length; num6++)
						{
							componentsInChildren[num6].enabled = data.Clothes[num4].Weared && data.Clothes[num4].Current_Variant == num5;
						}
					}
					if ((bool)Clothes[num4].Edit_Model[num5].GetComponent<Renderer>())
					{
						Clothes[num4].Edit_Model[num5].GetComponent<Renderer>().enabled = data.Clothes[num4].Weared && data.Clothes[num4].Current_Variant == num5;
						continue;
					}
					Renderer[] componentsInChildren2 = Clothes[num4].Edit_Model[num5].GetComponentsInChildren<Renderer>(includeInactive: true);
					for (int num7 = 0; num7 < componentsInChildren2.Length; num7++)
					{
						componentsInChildren2[num7].enabled = data.Clothes[num4].Weared && data.Clothes[num4].Current_Variant == num5;
					}
				}
			}
			data.first_game = false;
			Save_Data();
			Recount_Tools();
		}
		Wear_Clothes_On_New_Gaame();
		Set_Toys();
		Set_Graphics();
		Change_Vsync();
		volume.profile.GetSetting<MotionBlur>().active = data.Graphics.Motion_blur;
		Check_Occlusion();
		Check_Fullscreen();
		Check_Shadows();
		Set_Camera_And_Fog();
		Change_Bloom();
		if (!Music_Source)
		{
			Music_Source = UnityEngine.Object.FindObjectOfType<Music_Player>().GetComponent<AudioSource>();
		}
		if (!Menu)
		{
			if (Loader.level == 1)
			{
				Music_Source.clip = Home_Music;
			}
			if (Loader.level == 2)
			{
				Music_Source.clip = City_Music;
			}
			Music_Source.Play();
		}
		RenderSettings.fog = false;
	}

	public void Check_Continue_Button()
	{
		Continue_Button.enabled = data.Entered_level;
		Continue_Button.GetComponentInChildren<Text>().color = (data.Entered_level ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.5f));
	}

	public void Wear_Clothes_On_New_Gaame()
	{
		if (!Menu && Loader.level == 1 && data.progress_data.Mission_Progress == 0)
		{
			data.Clothes[0].Weared = true;
			data.Clothes[0].Current_Variant = 0;
			data.Clothes[1].Weared = true;
			data.Clothes[1].Current_Variant = 0;
			data.Clothes[2].Weared = false;
			data.Clothes[2].Current_Variant = 0;
			data.Clothes[3].Weared = false;
			data.Clothes[3].Current_Variant = 0;
			data.Clothes[4].Weared = false;
			data.Clothes[4].Current_Variant = 0;
			data.Clothes[5].Weared = false;
			data.Clothes[5].Current_Variant = 0;
			data.Clothes[6].Weared = false;
			data.Clothes[6].Current_Variant = 0;
			data.Clothes[7].Weared = false;
			data.Clothes[7].Current_Variant = 0;
			data.Clothes[8].Weared = false;
			data.Clothes[8].Current_Variant = 0;
			data.Clothes[9].Weared = false;
			data.Clothes[9].Current_Variant = 0;
		}
	}

	[ContextMenu("Add_Cloth_Meshes")]
	public void Add_Cloth_Mesh()
	{
		for (int i = 0; i < Clothes.Length; i++)
		{
			Clothes[i].Main_Model = new Transform[Clothes_main[i].childCount];
			Clothes[i].Edit_Model = new Transform[Clothes_main[i].childCount];
			for (int j = 0; j < Clothes[i].Main_Model.Length; j++)
			{
				Clothes[i].Main_Model[j] = Clothes_main[i].GetChild(j);
				Clothes[i].Edit_Model[j] = Clothes_edit[i].GetChild(j);
			}
		}
	}

	private void Start()
	{
		Set_All_Sounds();
	}

	public void Remove_Spawned_Clothes()
	{
		for (int i = 0; i < data.Clothes.Length; i++)
		{
			for (int j = 0; j < data.Clothes[i].Spawned_Cloth.Length; j++)
			{
				data.Clothes[i].Spawned_Cloth[j].Spawned = false;
			}
		}
	}

	public void Set_Names()
	{
		Names[data.Language].Rox_Name.text = data.Name;
		if (data.Language == 0)
		{
			Names[data.Language].Rox_Name2.text = data.Name2;
		}
		if (data.Language == 0)
		{
			Names[data.Language].Rox_Name3.text = data.Name3;
		}
		Names[data.Language].Player_Name.text = data.Player_Name;
		if (data.Language == 0)
		{
			Names[data.Language].Player_Name2.text = data.Player_Name2;
		}
		Names[data.Language].Whore_Name.text = data.Whore_Name;
		Names[data.Language].Owner_Name.text = data.Owner_Name;
		if (data.Language == 0)
		{
			data.Levels[1].Name[data.Language] = "Квартира " + data.Name2;
		}
		if (data.Language == 1)
		{
			data.Levels[1].Name[data.Language] = data.Name + "'s home";
		}
		if (data.Language == 0)
		{
			Rotate_Character_Name_Text.text = "Удерживайте кнопки A/D, чтобы вращать " + data.Name3;
		}
		if (data.Language == 1)
		{
			Rotate_Character_Name_Text.text = "Hold A and D buttons to rotate " + data.Name;
		}
		string text = ((data.Whore_Name.Length > 0) ? ", " : " ");
		if (!Menu)
		{
			if (data.Language == 0)
			{
				Playable_Character_Name_Text.text = data.Name + text + data.Whore_Name + " " + data.Player_Name2;
			}
			if (data.Language == 1)
			{
				Playable_Character_Name_Text.text = data.Name + text + data.Player_Name + "'s " + data.Whore_Name;
			}
		}
		if (Menu)
		{
			if (data.Language == 0)
			{
				Playable_Character_Name_Text.text = "Редактировать " + data.Name3;
			}
			if (data.Language == 1)
			{
				Playable_Character_Name_Text.text = "Edit " + data.Name + text + data.Player_Name + "'s " + data.Whore_Name;
			}
		}
		Game_Name_Text.text = data.Name;
	}

	public void Accept_Name(int id)
	{
		if (id == 0)
		{
			data.Name = Names[data.Language].Rox_Name.text;
		}
		if (id == 1)
		{
			data.Name2 = Names[data.Language].Rox_Name2.text;
		}
		if (id == 2)
		{
			data.Name3 = Names[data.Language].Rox_Name3.text;
		}
		Set_Names();
	}

	public void Accept_Player_Name(int id)
	{
		if (id == 0)
		{
			data.Player_Name = Names[data.Language].Player_Name.text;
		}
		if (id == 1)
		{
			data.Player_Name2 = Names[data.Language].Player_Name2.text;
		}
		Set_Names();
	}

	public void Accept_Whore_Name()
	{
		data.Whore_Name = Names[data.Language].Whore_Name.text;
		Set_Names();
	}

	public void Accept_Owner_Name()
	{
		data.Owner_Name = Names[data.Language].Owner_Name.text;
		Set_Names();
	}

	public void Open_Edit_Menu(bool Main_Menu)
	{
		Close_All();
		anim.SetInteger("Cam_Position", 0);
		if (!Main_Menu)
		{
			anim.SetTrigger("Edit");
			Rox_Animator.SetTrigger("Go");
			Rox_Animator.SetBool("Chest_Constraint", value: false);
			StartCoroutine(Delay_To_Turn_Edit());
		}
		else
		{
			StartCoroutine(Go_Out_From_Edit());
		}
		Save_Data();
	}

	private IEnumerator Go_Out_From_Edit()
	{
		Edit_Base edit = GetComponent<Edit_Base>();
		edit.Rotate_Back = true;
		Transform rox = edit.Rox;
		Set_Rox_Speed(1);
		rox.GetComponent<Animator>().SetTrigger("Move_Back");
		anim.SetTrigger("Back");
		Boob[0].BlendWeight = 0f;
		Boob[1].BlendWeight = 0f;
		Switching_Menu_Process = true;
		yield return new WaitForSeconds(1.5f);
		edit.Rotate_Back = false;
		Edition_Menu.gameObject.SetActive(value: false);
		for (int i = 0; i < Menu_Elements.Length; i++)
		{
			Menu_Elements[i].gameObject.SetActive(value: true);
		}
		for (int j = 0; j < Edit_Mode_Deactivate.Length; j++)
		{
			Edit_Mode_Deactivate[j].gameObject.SetActive(value: true);
		}
		Demo_Text.gameObject.SetActive(hidden_data.Demo);
		Rox_Animator.SetTrigger("Back");
		Rox_Animator.SetBool("In_Bra", Underwear_On);
		In_Edit_Menu = false;
		Switching_Menu_Process = false;
		Music_Source.clip = Menu_Clip;
		Music_Source.Play();
	}

	private IEnumerator Delay_To_Turn_Edit()
	{
		Transform Rox = GetComponent<Edit_Base>().Rox;
		Rox.transform.position = Start_Position.position;
		Rox.transform.rotation = Start_Position.rotation;
		Boob[0].BlendWeight = 75f;
		Boob[1].BlendWeight = 75f;
		Set_Rox_Speed(1);
		yield return new WaitForSeconds(1f);
		Music_Source.clip = Editor_Clip;
		Music_Source.Play();
		yield return new WaitForSeconds(0.5f);
		Edition_Menu.gameObject.SetActive(value: true);
		Rox.GetComponent<Animator>().SetTrigger("Move");
		Edit_Rox_Animator.SetBool("In_Bra", Underwear_On);
		for (int i = 0; i < Menu_Elements.Length; i++)
		{
			Menu_Elements[i].gameObject.SetActive(value: false);
		}
		for (int j = 0; j < Edit_Mode_Deactivate.Length; j++)
		{
			Edit_Mode_Deactivate[j].gameObject.SetActive(value: false);
		}
		Demo_Text.gameObject.SetActive(value: false);
		In_Edit_Menu = true;
	}

	public void Switch_Menu(bool Main)
	{
		Main_Menu.gameObject.SetActive(Main);
		Start_Game_Menu.gameObject.SetActive(!Main);
	}

	public void Camera_Back()
	{
		anim.SetTrigger("Back");
		Set_Rox_Speed(1);
	}

	public void Set_Level_Text(int level)
	{
		level_name_text.text = data.Levels[level].Name[data.Language];
	}

	public void Set_Rox_Speed(int value)
	{
		Edit_Rox_Animator.speed = value;
	}

	public void Load_Saved_Scene()
	{
		if ((bool)rox_audio)
		{
			rox_audio.enabled = false;
		}
		for (int i = 0; i < Load_Images.Length; i++)
		{
			Load_Images[i].sprite = hidden_data.level_Pictures[1].Pictures[i];
		}
		data.Loaded_game = true;
		anim.SetTrigger("Start_Saved");
		Rox_Animator.SetTrigger("Go");
		Set_Level_Text(1);
		Save_Data();
		StartCoroutine(LoadSceneAsync(1));
	}

	public void Back_To_Default()
	{
		data.progress_data.Mission_Progress = 0;
		data.Blocked_Param[0] = true;
		data.Blocked_Param[1] = true;
		data.Blocked_Param[2] = true;
		data.Blocked_Param[3] = true;
		data.items.Remain_Items[0] = 0;
		data.items.Remain_Items[1] = 0;
		data.items.Remain_Items[2] = 0;
		data.saved_data.Jerk_Places.Used = new bool[500];
		data.saved_data.Dance_Places.Used = new bool[500];
		data.saved_data.Toys_Places.Used = new bool[500];
		data.saved_data.Sex_Places.Used = new bool[500];
		for (int i = 0; i < data.progress_data.Interior_Achieves.Length; i++)
		{
			data.progress_data.Interior_Achieves[i].Progress = 0;
			data.progress_data.Interior_Achieves[i].Tasks_Completed = 0;
			data.progress_data.Interior_Achieves[i].Progress_Requied = 3;
		}
		for (int j = 0; j < data.progress_data.Street_Achieves.Length; j++)
		{
			data.progress_data.Street_Achieves[j].Tasks_Completed = 0;
		}
		data.progress_data.Rank = 1;
		data.progress_data.Sum_Score = 0;
		data.progress_data.Jerk_Level = 1;
		data.progress_data.Dance_Level = 1;
		data.progress_data.Toy_Level = 1;
		data.progress_data.Sex_Level = 1;
		data.progress_data.Sum_Experience = 0f;
		data.progress_data.Level_Experience = 0f;
		data.progress_data.People_Embarassed = 0;
		data.progress_data.People_Talked = 0;
		data.progress_data.People_Knocked = 0;
		data.progress_data.Money_Earned = 0;
		data.progress_data.Masturbated = 0;
		data.progress_data.Danced = 0;
		data.progress_data.Fucked_Toy = 0;
		data.progress_data.Selfie_Made = 0;
		data.progress_data.Mom_Progress = 0;
		data.items.Items_Used[0] = 0;
		data.items.Items_Used[1] = 0;
		data.items.Items_Used[2] = 0;
		data.progress_data.Five_People_Counter = 0;
		data.Current_Happiness = data.Start_Happiness;
		data.Current_Night = 1;
		data.time = 19f;
		data.Start_Video_Showed = false;
		data.Entered_level = false;
		data.saved_data.Inside_Building = 30;
		data.progress_data.Interior_Achieves[30].Progress = 1;
	}

	public void NewGame()
	{
		Set_Rox_Speed(1);
		data.Loaded_game = false;
		data.Entered_level = true;
		Back_To_Default();
		Save_Data();
		for (int i = 0; i < Clothes.Length; i++)
		{
			Clothes[i].Edit_Weared = true;
			for (int j = 0; j < Clothes[i].Main_Model.Length; j++)
			{
				if ((bool)Clothes[i].Main_Model[j])
				{
					Clothes[i].Main_Model[j].gameObject.SetActive(Clothes[i].Edit_Weared);
				}
				if ((bool)Clothes[i].Edit_Model[j])
				{
					Clothes[i].Edit_Model[j].gameObject.SetActive(Clothes[i].Edit_Weared);
				}
			}
		}
		if ((bool)rox_audio)
		{
			rox_audio.volume = 0f;
		}
		anim.SetBool("Game_Started", value: true);
		anim.SetTrigger("Start");
		Set_Level_Text(1);
		StartCoroutine(LoadSceneAsync(1));
	}

	public void LoadScene(int id)
	{
		if ((bool)rox_audio)
		{
			rox_audio.volume = 0f;
		}
		data.Loaded_game = false;
		anim.SetTrigger("Start");
		Set_Level_Text(id);
		if ((bool)Main_UI)
		{
			Main_UI.enabled = false;
		}
		Loading_Process = true;
		StartCoroutine(LoadSceneAsync(id));
		Debug.Log("Loading");
	}

	private IEnumerator LoadSceneAsync(int id)
	{
		AsyncOperation operation = SceneManager.LoadSceneAsync(id, LoadSceneMode.Single);
		if (id == 1)
		{
			Music_Source.clip = Home_Clip;
		}
		Music_Source.Play();
		if (id == 2)
		{
			Music_Source.clip = City_Clip;
		}
		Music_Source.Play();
		while (!operation.isDone)
		{
			float value = Mathf.Clamp01(operation.progress / 0.9f);
			progress_bar.value = value;
			yield return null;
		}
	}

	public void QuitGame()
	{
		Save_level_data();
		Application.Quit();
	}

	public void Save_level_data()
	{
		if (!Menu)
		{
			data.saved_data.Sam_Position = player.position;
			data.saved_data.Sam_Rotation = player.eulerAngles;
		}
		Save_Data();
	}

	private void Update()
	{
		if (Menu && !In_Edit_Menu)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				anim.SetTrigger("Miss");
			}
			if (Rox_Animator.GetBool("Chest_Constraint"))
			{
				Chest_Constraint.weight += Time.deltaTime * 2f;
			}
			else
			{
				Chest_Constraint.weight -= Time.deltaTime * 2f;
			}
			Chest_Constraint.weight = Mathf.Clamp01(Chest_Constraint.weight);
		}
		if (Menu && Input.GetKeyDown(KeyCode.Backspace))
		{
			Input_Mission_Object.gameObject.SetActive(!Input_Mission_Object.gameObject.activeInHierarchy);
		}
	}

	public void Accept_Input_Mission()
	{
		data.progress_data.Mission_Progress = int.Parse(Input_Mission_progress.text);
		data.Start_Video_Showed = true;
		Save_Data();
	}

	public void Test_Game()
	{
		if ((bool)rox_audio)
		{
			rox_audio.enabled = false;
		}
		for (int i = 0; i < Load_Images.Length; i++)
		{
			Load_Images[i].sprite = hidden_data.level_Pictures[1].Pictures[i];
		}
		data.Loaded_game = false;
		data.Entered_level = true;
		anim.SetTrigger("Start_Saved");
		Rox_Animator.SetTrigger("Go");
		Set_Level_Text(1);
		Save_Data();
		StartCoroutine(LoadSceneAsync(1));
	}

	public void Turn_Cloth(bool Underwear)
	{
		if (!Underwear)
		{
			Clothes_On = !Clothes_On;
			Show_Hide_Cloth();
		}
		else
		{
			Underwear_On = !Underwear_On;
			Show_Hide_Underwear();
		}
		Set_Edit_Clothes_Names();
	}

	private void Bra_Shape_Check()
	{
		if (data.Clothes[3].Weared && Clothes_On)
		{
			Clothes[1].Main_Model[0].GetComponentInChildren<SkinnedMeshRenderer>(includeInactive: true).SetBlendShapeWeight(1, 100f);
		}
		else
		{
			Clothes[1].Main_Model[0].GetComponentInChildren<SkinnedMeshRenderer>(includeInactive: true).SetBlendShapeWeight(1, 0f);
		}
		bool flag = data.Clothes[4].Weared || data.Clothes[8].Weared;
		int num;
		if (data.Clothes[4].Weared)
		{
			num = ((data.Clothes[4].Current_Variant >= 2) ? 1 : 0);
			if (num != 0)
			{
				goto IL_00fb;
			}
		}
		else
		{
			num = 0;
		}
		if (!flag)
		{
			Edit_Rox_Animator.SetBool("In_Boots", value: false);
			Edit_Rox_Animator.SetBool("In_Heels", value: false);
		}
		goto IL_00fb;
		IL_00fb:
		if (num == 0 && flag)
		{
			Edit_Rox_Animator.SetBool("In_Boots", value: true);
			Edit_Rox_Animator.SetBool("In_Heels", value: false);
		}
		if (((uint)num & (flag ? 1u : 0u)) != 0)
		{
			Edit_Rox_Animator.SetBool("In_Boots", value: false);
			Edit_Rox_Animator.SetBool("In_Heels", value: true);
		}
	}

	private void Show_Hide_Cloth()
	{
		for (int i = 2; i < Clothes.Length; i++)
		{
			Clothes[i].Edit_Weared = Clothes_On;
			for (int j = 0; j < Clothes[i].Main_Model.Length; j++)
			{
				if ((bool)Clothes[i].Main_Model[j])
				{
					Clothes[i].Main_Model[j].gameObject.SetActive(Clothes[i].Edit_Weared && data.Clothes[i].Weared && data.Clothes[i].Current_Variant == j);
				}
				if ((bool)Clothes[i].Edit_Model[j])
				{
					Clothes[i].Edit_Model[j].gameObject.SetActive(Clothes[i].Edit_Weared && data.Clothes[i].Weared && data.Clothes[i].Current_Variant == j);
				}
			}
		}
		Bra_Shape_Check();
	}

	private void Show_Hide_Underwear()
	{
		for (int i = 0; i < 2; i++)
		{
			Clothes[i].Edit_Weared = Underwear_On;
			for (int j = 0; j < Clothes[i].Main_Model.Length; j++)
			{
				if ((bool)Clothes[i].Main_Model[j])
				{
					Clothes[i].Main_Model[j].gameObject.SetActive(Clothes[i].Edit_Weared && data.Clothes[i].Weared && data.Clothes[i].Current_Variant == j);
				}
				if ((bool)Clothes[i].Edit_Model[j])
				{
					Clothes[i].Edit_Model[j].gameObject.SetActive(Clothes[i].Edit_Weared && data.Clothes[i].Weared && data.Clothes[i].Current_Variant == j);
				}
			}
		}
		Rox_Animator.SetBool("In_Bra", Underwear_On);
		Edit_Rox_Animator.SetBool("In_Bra", Underwear_On);
		UnityEngine.Object.FindObjectOfType<Edit_Base>().Show_Hide_Piercing();
	}

	private void Set_Edit_Clothes_Names()
	{
		Cross_Clothes.gameObject.SetActive(!Clothes_On);
		Cross_Underwear.gameObject.SetActive(!Underwear_On);
		Rox_Animator.SetBool("In_Bra", Underwear_On);
		Edit_Rox_Animator.SetBool("In_Bra", Underwear_On);
		Edit_Rox_Animator.SetTrigger(Clothes_On ? "Boots_On" : "Boots_Off");
	}

	public void Set_Toys()
	{
		for (int i = 0; i < data.Toys_Pussy.Length; i++)
		{
			Toys_Pussy[i].Edit_Model.gameObject.SetActive(data.Toys_Pussy[i].Weared);
			Toys_Pussy[i].Main_Model.gameObject.SetActive(data.Toys_Pussy[i].Weared);
		}
		for (int j = 0; j < data.Toys_Ass.Length; j++)
		{
			Toys_Ass[j].Edit_Model.gameObject.SetActive(data.Toys_Ass[j].Weared);
			Toys_Ass[j].Main_Model.gameObject.SetActive(data.Toys_Ass[j].Weared);
		}
	}
}
