using System;
using System.Collections;
using Invector.vCamera;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using RootMotion.Demos;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Roxanne_Control : MonoBehaviour
{
	public enum Actions_Mode
	{
		none,
		jerk,
		dance,
		toy
	}

	public enum Fail_Reason
	{
		shame,
		unhappy,
		cops,
		morning
	}

	[Serializable]
	public struct Photo_Camera
	{
		public Camera Photo_Cam;

		public Transform Photo_UI;

		public Transform Hand_Goal;

		public float current_ik_weight;

		public float target_ik_weight;

		public float cam_target_y;

		public LookAtConstraint Head_constraint;

		public ScreenshotHandler photo_handler;
	}

	[Serializable]
	public struct Poses
	{
		public string Name;

		public int id;

		public int Requied_Achieve;

		public int[] Requied_Unweared_Cloth;

		public bool No_Requied_Unwear;

		public Image Lock;
	}

	[Serializable]
	public struct Dances
	{
		public string Name;

		public int id;

		public int Requied_Achieve;

		public Image Lock;
	}

	[Serializable]
	public struct Toy_Poses
	{
		public string Name;

		public int id;

		public int Requied_Achieve;

		public Image Lock;
	}

	[Serializable]
	public struct Sounds
	{
		public AudioClip BarrelHit;

		public AudioClip PlasticHit;

		public AudioClip FlowerpotHit;

		public AudioClip CardboardHit;

		public AudioClip DoorHit;

		public AudioClip WoodHit;

		public AudioSource Hit_Audio;
	}

	[Serializable]
	public struct Cameras
	{
		public Transform Main_Cam;

		public ParentConstraint constraint;

		public float Fps_const_weight;

		public float Fps_Rotation_Speed;

		public float new_const_value;

		public Vector3 new_translation_offset;

		public float const_change_speed;

		public Vector3 Head_Cam_Idle_Pos;

		public Vector3 Head_Cam_Walk_Pos;

		public Vector3 Head_Cam_Crouch_Pos;

		public Vector3 Head_Cam_Run_Pos;

		public Vector3 Masturb_Pos;

		public Vector3 Inv_Pos;

		public Vector3 Inv_Rot;

		public float Cam_Y_Offset;

		public float[] Cam_Masturbation_Y;

		public float Cam_Toy_Offset_Y;

		public float Additional_Y_Offset;

		public vThirdPersonCamera Cam_Control;

		public vThirdPersonInput Input;

		public vThirdPersonController Player_Control;

		public Transform Head;

		public Transform Torso;

		public Transform Armature;

		[HideInInspector]
		public ConstraintSource Source_Head;

		[HideInInspector]
		public ConstraintSource Source_Torso;

		[HideInInspector]
		public ConstraintSource Source_Inv;
	}

	[Serializable]
	public struct Wish
	{
		public string Name;

		public string[] Texts;

		public Speech speech;
	}

	[HideInInspector]
	public Animator anim;

	[HideInInspector]
	public AudioSource rox_audio;

	[Header("Значения")]
	public float Shame;

	public float Happiness = 50f;

	public float Excitement;

	public float Hiding;

	public int Nake_Level;

	public int Showing_Coef;

	public int Watchers_Count;

	public bool Known_Watching;

	public bool Have_Known_Watchers;

	[HideInInspector]
	public bool Eaten;

	[HideInInspector]
	public bool Runned;

	[HideInInspector]
	public bool Jerked;

	public Speech First_Eating_Speech;

	public Speech First_Run_Speech;

	public Speech Low_Level;

	public Speech Already_Jerk;

	public Speech Already_Dance;

	public Speech Need_To_Be_Naked;

	public Inventory_Script inventory;

	[HideInInspector]
	public Mission_Explorer mission_Explorer;

	public Cameras cameras;

	private House_Transfer transferer;

	[HideInInspector]
	public bool Crouching;

	[HideInInspector]
	public bool Running;

	[HideInInspector]
	public bool Walking;

	[HideInInspector]
	public bool WasNonFps;

	[HideInInspector]
	public bool Showing;

	[HideInInspector]
	public bool Covered;

	[HideInInspector]
	public bool Anim_UpDress_Process;

	[HideInInspector]
	public int Current_Cloth;

	[HideInInspector]
	public int Requied_Cloth;

	[HideInInspector]
	public int cover_pos;

	public Renderer Redness;

	private float Current_Red_value;

	private float New_Red_Value;

	private float[] Red_Parameter;

	public AudioClip[] Cloth;

	public AudioClip[] Voice;

	private float Shame_Time_Counter;

	[Header("Места для дрочки")]
	public bool Inside_Jerk_Place;

	public Jerk_Place Current_Jerk_Place;

	[Header("Дрочка")]
	public float Jerk_Speed;

	public bool Masturbating;

	public GameObject Squirt;

	public bool isCuming;

	public Speech Jerk_First_Speech;

	public Speech Need_To_Sit;

	public Slider Speed_Slider;

	public bool Mirror_Pose;

	public Poses[] poses;

	public Dances[] dances;

	[Header("Ебля игрушкой")]
	public Toy_Poses[] toy_poses;

	public AudioSource Pussy_Sound;

	public AudioSource Toy_Sound;

	public AudioClip Toy_In;

	public AudioClip Toy_Out;

	public AudioClip Squirt_Sound;

	public bool Fucked;

	public Speech Fucked_First_Speech;

	public Speech Cant_Wear_With_Toy;

	[HideInInspector]
	public Animator Interface_anim;

	[HideInInspector]
	public int Current_Mastrurbation_Pose;

	private Rigidbody rigidb;

	public vThirdPersonController controller;

	public vThirdPersonInput input;

	private vSwimming swimming;

	public Animator Inventory_Anim;

	public int Window_Size = 200;

	public bool Map;

	private bool OnLadder;

	public PauseMenuScript interface_script;

	[HideInInspector]
	public Menu_Level_Loader Loader;

	public Start_Menu menu;

	public bool Speaking;

	public Sounds sounds;

	public bool Photo_Mode;

	public Photo_Camera photo;

	public LookAtConstraint Head_Photo;

	public Speech Dont_Have_Phone;

	public Transform[] Head_Parts_Tps;

	public bool Was_TPS_Mode;

	private vFirstPersonCamera FPS_Control;

	public bool Photo_Making;

	public float Remain_Photo_Counter_Time;

	public bool Toy_Mode;

	private float z_Toy_Pos;

	public bool Fucking;

	private bool back;

	private float Remain_Energized_Time;

	private float Remain_Drunk_Time;

	private float Remain_Full_Food_Time;

	public Transform Arrow_Object;

	public Arrow_Target arrow;

	public Transform Map_arrow;

	public Transform Char_Name;

	public Speech[] Demo_Block_Speech;

	private float right_offset_camera;

	public Speech First_Dance_Speech;

	public bool Danced;

	public bool Dancing;

	public int Current_Dance_Pose;

	public Dance_Place Current_Dance_Place;

	public bool Inside_Toy_Place;

	public bool Inside_Dance_Place;

	public Actions_Mode actions_Mode;

	public Transform[] action_names;

	public int Current_Action;

	public Image Mast_Block;

	public UI_Model_Animated Actions_UI;

	public float Happiness_Coef;

	private bool Showing_Actions;

	private bool Delayed_Happiness_Show;

	public Speech[] Wrong_Action_Speech;

	private bool Wrong_Speech_Said;

	private bool Action_Did;

	public Speech Any_Action_First_Speech;

	private bool Tired_Animation_Played;

	[HideInInspector]
	public bool Eating;

	[HideInInspector]
	public bool Drinking;

	[HideInInspector]
	public bool Withdrawaling;

	public Smartphone smartphone;

	[Header("Желания")]
	public bool Bad_Mood_Showed;

	[Header("Желания")]
	public bool Good_Mood_Showed;

	public Speech Good_Mood_Speech;

	public Speech Bad_Mood_Speech;

	public Speech Dont_Want_Unwear_Speech;

	public bool Full_Of_Food;

	public bool Drunk;

	private float Multi_Happiness;

	private float Multi_Shame;

	public Fail_Reason fail_reason;

	public Speech Fail_Shame;

	public Speech Fail_Unhappy;

	public Speech Fail_Cops;

	public Speech Fail_Morning;

	public Speech Evening;

	public Speech Morning;

	public Speech Tired_Action;

	public Speech Tired_Run;

	public Transform[] Hand_Stuff;

	public Transform[] Throw_Stuff;

	[Header("ИК")]
	public EffectorOffset Ik_Offset;

	public FullBodyBipedIK body_ik;

	public Transform Left_Foot_Effector;

	public Transform Right_Foot_Effector;

	public bool Arrested;

	[Header("Одежка с клотом")]
	private ClothSkinningCoefficient[] upskirt_coefficients;

	private ClothSkinningCoefficient[] free_coefficients;

	private ClothSkinningCoefficient[] jerk_coefficients;

	private ClothSkinningCoefficient[] toy_coefficients;

	public Cloth Skirt;

	public Cloth Free_Skirt;

	public Cloth Jerk_Skirt;

	public Cloth Toy_Skirt;

	[Header("Деньги")]
	public Speech Not_Enough_Money;

	public Speech Cant_Open_District_In_Demo;

	public Transform Out_Pos;

	public Speech God_Ev_Sir;

	public Speech Good_Ev_Maam;

	public Speech[] Talk_Mom;

	public NPC_generator Opponent;

	public bool New_Day_Played;

	public bool Night_Fail_Started;

	public void Find_Sam_Components()
	{
		rigidb = GetComponent<Rigidbody>();
		anim = GetComponent<Animator>();
		rox_audio = GetComponent<AudioSource>();
		inventory = UnityEngine.Object.FindObjectOfType<Inventory_Script>();
		Ik_Offset = GetComponent<EffectorOffset>();
		body_ik = GetComponent<FullBodyBipedIK>();
		controller = GetComponent<vThirdPersonController>();
		input = GetComponent<vThirdPersonInput>();
		swimming = GetComponent<vSwimming>();
		if (!menu)
		{
			menu = UnityEngine.Object.FindObjectOfType<Start_Menu>();
		}
		if (!Loader)
		{
			Loader = UnityEngine.Object.FindObjectOfType<Menu_Level_Loader>();
		}
		if (!interface_script)
		{
			interface_script = UnityEngine.Object.FindObjectOfType<PauseMenuScript>();
		}
		Interface_anim = menu.GetComponent<Animator>();
		transferer = GetComponent<House_Transfer>();
		mission_Explorer = UnityEngine.Object.FindObjectOfType<Mission_Explorer>();
		FPS_Control = GetComponent<vFirstPersonCamera>();
	}

	public void Init_Sam()
	{
		Hiding = 1f;
		Jerk_Speed = 1f;
		Mirror_Pose = false;
		Red_Parameter = new float[5];
		interface_script.Rox_Interface.Happiness_Slider.value = Happiness;
		interface_script.Rox_Interface.Jerk_Task_Object.gameObject.SetActive(value: false);
		upskirt_coefficients = Skirt.coefficients;
		free_coefficients = Free_Skirt.coefficients;
		jerk_coefficients = Jerk_Skirt.coefficients;
		toy_coefficients = Toy_Skirt.coefficients;
		Skirt.coefficients = free_coefficients;
		Skirt.enabled = true;
		Shame = 0f;
		interface_script.Rox_Interface.Shame_Slider.value = Shame;
		interface_script.Rox_Interface.Task_Progress_Slider.value = Excitement;
		cameras.Source_Head.sourceTransform = cameras.Head;
		cameras.Source_Head.weight = 1f;
		cameras.Source_Torso.sourceTransform = cameras.Torso;
		cameras.Source_Torso.weight = 1f;
		cameras.Source_Inv.sourceTransform = cameras.Armature;
		cameras.Source_Inv.weight = 1f;
		WasNonFps = true;
		Map_arrow.gameObject.SetActive(value: false);
		Happiness = inventory.data.Current_Happiness;
		smartphone = interface_script.transform.GetComponentInChildren<Smartphone>(includeInactive: true);
		smartphone.menu = menu;
		smartphone.player = this;
	}

	private void OnEnable()
	{
		Init_Sam_Coroutines();
	}

	private void Init_Sam_Coroutines()
	{
		StartCoroutine(Shame_Decreasing());
		StartCoroutine(Change_Happiness());
		StartCoroutine(Fatness_Decreasing());
		StartCoroutine(Power_Up_Couter());
	}

	public void Speak_Time()
	{
		bool num = interface_script.time.Hours == 5 && interface_script.time.Minutes >= 30;
		if (interface_script.time.Hours == 19)
		{
			_ = interface_script.time.Minutes == 15;
		}
		else
			_ = 0;
		if (num && !New_Day_Played)
		{
			Speak(Morning);
			New_Day_Played = true;
		}
	}

	public void Speak(Speech speech)
	{
		string text = speech.speeches[inventory.data.Language].Replace("*", Loader.data.Owner_Name);
		interface_script.Rox_Interface.tip_text.text = text;
		if (!Speaking)
		{
			interface_script.Rox_Interface.tip_text.text = text;
			if (!Interface_anim.GetBool("Speech_Shows"))
			{
				Interface_anim.SetTrigger("Tip_Common");
				if (Inventory_Anim.gameObject.activeInHierarchy)
				{
					Inventory_Anim.SetTrigger("Speak");
				}
			}
			interface_script.Turn_Cursor();
		}
		else
		{
			Interface_anim.SetTrigger("Replace_Speech");
		}
	}

	public void Close_Dialogue()
	{
		Interface_anim.SetTrigger("Close_Tip");
		interface_script.Turn_Cursor();
	}

	public void Red_Control()
	{
	}

	public void Set_Free_Skirt()
	{
		Skirt.coefficients = free_coefficients;
		Skirt.ClearTransformMotion();
	}

	public void Set_Up_Skirt()
	{
		Skirt.coefficients = upskirt_coefficients;
		Skirt.ClearTransformMotion();
	}

	public void Set_Jerk_Skirt()
	{
		Skirt.coefficients = jerk_coefficients;
		Skirt.ClearTransformMotion();
	}

	public void Set_Toy_Skirt()
	{
		Skirt.coefficients = toy_coefficients;
		Skirt.ClearTransformMotion();
	}

	public void Check_Cloth_For_Showing(int Requied)
	{
		if (!Showing)
		{
			inventory.Check_Nake_Level();
			Nake_Level += Showing_Coef;
			Showing = true;
			Stop_Covering();
			anim.SetTrigger("Show");
			anim.SetBool("Excited", value: true);
			anim.SetInteger("Cloth_Up", Requied);
			mission_Explorer.Expose_Task_Start();
		}
		else if (!Anim_UpDress_Process)
		{
			if (Current_Cloth == Requied_Cloth)
			{
				inventory.Check_Nake_Level();
				anim.SetTrigger("Hide");
				anim.SetInteger("Cloth_Up", 0);
			}
			else
			{
				anim.SetInteger("Cloth_Up", Current_Cloth);
				anim.SetTrigger("Hide");
			}
		}
	}

	public void Stop_Covering()
	{
		anim.SetInteger("Cover", 0);
		Covered = false;
		cover_pos = 0;
	}

	private void Cover(int Body_Part, int Inv_Cloth)
	{
		if (inventory.data.Clothes[Inv_Cloth].Weared)
		{
			Covered = !Covered;
			if (cover_pos == 0)
			{
				cover_pos = 1;
			}
			else
			{
				cover_pos = 0;
			}
		}
		else if (cover_pos == 0)
		{
			Covered = !Covered;
			cover_pos++;
		}
		else if (cover_pos == 1)
		{
			cover_pos++;
		}
		else
		{
			cover_pos = 0;
			Covered = !Covered;
		}
		if (Covered)
		{
			anim.SetTrigger("Hide");
			if (cover_pos == 1)
			{
				anim.SetInteger("Cover", Body_Part);
			}
			else if (cover_pos == 2)
			{
				anim.SetInteger("Cover", 3);
			}
			Hiding = 0.5f;
		}
		else
		{
			anim.SetInteger("Cover", 0);
			Hiding = 1f;
		}
	}

	private void Change_Dance_Pose(int id)
	{
		anim.SetBool("Dancing", value: true);
		anim.SetTrigger("Dance_Start");
		anim.SetInteger("Dance_Id", id);
		Dancing = true;
		Current_Dance_Pose = id;
		Current_Action = 2;
	}

	public void Stop_Dancing()
	{
		anim.SetBool("Dancing", value: false);
		anim.SetInteger("Dance_Id", 0);
		anim.SetTrigger("Dance_End");
		Current_Dance_Pose = 0;
		Dancing = false;
		Current_Action = 0;
	}

	private void Set_Toy_Mode()
	{
		inventory.Check_Checkmarks();
		if (Toy_Mode)
		{
			mission_Explorer.Toys_Task_Start();
			Nake_Level += 2;
			Set_Toy_Skirt();
			if (!Fucked)
			{
				Fucked = true;
				Speak(Fucked_First_Speech);
			}
			if (inventory.data.Clothes[0].Weared)
			{
				anim.SetInteger("Check_Cloth_Toy", 2);
			}
			else
			{
				anim.SetInteger("Check_Cloth_Toy", 1);
			}
			anim.SetTrigger("Toy_Start");
			inventory.Check_Legs_Size();
			return;
		}
		inventory.Check_Nake_Level();
		Set_Free_Skirt();
		if (Fucking)
		{
			Stop_Fucking();
		}
		inventory.Check_For_Toy_Inside();
		inventory.Put_Toys_Pussy();
		inventory.Put_Toys_Ass();
		anim.SetBool("Have_Toy", inventory.data.Have_Toy_Inside);
		if (inventory.data.Have_Toy_Inside)
		{
			inventory.data.Clothes[0].Weared = false;
			inventory.Check_Nake_Level();
			inventory.Show_Hide_Inventory_Mesh(0);
		}
		else
		{
			inventory.current_toy = inventory.empty_toy;
		}
		anim.SetTrigger("Toy_End");
		if (rox_audio.isPlaying)
		{
			rox_audio.Stop();
		}
	}

	public void Stop_Fucking()
	{
		Fucking = false;
		Current_Action = 0;
		anim.SetBool("Fucking", Fucking);
		if (rox_audio.isPlaying)
		{
			rox_audio.Stop();
		}
	}

	public void Stop_Toying()
	{
		Toy_Mode = false;
		Set_Toy_Mode();
	}

	public void Change_Action(bool next)
	{
		if (actions_Mode == Actions_Mode.jerk)
		{
			if (next && Actions_UI.Pose_id < 8)
			{
				Actions_UI.Pose_id++;
			}
			if (!next && Actions_UI.Pose_id > 1)
			{
				Actions_UI.Pose_id--;
			}
			Actions_UI.actions_anim.SetInteger("Animation_id", Actions_UI.Poses_List[Actions_UI.Pose_id]);
		}
		if (actions_Mode == Actions_Mode.dance)
		{
			if (next && Actions_UI.Dance_id < 10)
			{
				Actions_UI.Dance_id++;
			}
			if (!next && Actions_UI.Dance_id > 1)
			{
				Actions_UI.Dance_id--;
			}
			Mast_Block.gameObject.SetActive(inventory.data.progress_data.Dance_Level >= dances[Actions_UI.Dance_id].Requied_Achieve);
			Actions_UI.Choose_Dance(Actions_UI.Dance_id);
		}
	}

	public void Set_Action()
	{
		if (actions_Mode == Actions_Mode.jerk)
		{
			if (Current_Mastrurbation_Pose != Actions_UI.Poses_List[Actions_UI.Pose_id])
			{
				int num = Actions_UI.Poses_List[Actions_UI.Pose_id];
				if (num != 4 && num != 6)
				{
					if (inventory.data.progress_data.Jerk_Level >= poses[num].Requied_Achieve)
					{
						Change_Masturbation_Pose(num);
					}
					else
					{
						if (num == 2)
						{
							Speak(inventory.Jerk_Block_Speech[1]);
						}
						if (num == 3)
						{
							Speak(inventory.Jerk_Block_Speech[2]);
						}
						if (num == 7)
						{
							Speak(inventory.Jerk_Block_Speech[3]);
						}
						if (num == 8)
						{
							Speak(inventory.Jerk_Block_Speech[4]);
						}
						if (num == 5)
						{
							Speak(inventory.Jerk_Block_Speech[5]);
						}
						anim.SetTrigger("Cant");
					}
				}
				else if (!Inside_Jerk_Place)
				{
					Speak(Need_To_Sit);
				}
				else
				{
					Change_Masturbation_Pose(Actions_UI.Poses_List[Actions_UI.Pose_id]);
				}
			}
			else
			{
				Stop_Masturbating();
			}
		}
		if (actions_Mode == Actions_Mode.dance)
		{
			if (Current_Dance_Pose != Actions_UI.Dance_id)
			{
				if (inventory.data.progress_data.Dance_Level >= dances[Actions_UI.Dance_id].Requied_Achieve)
				{
					if (anim.GetInteger("Cloth_Up") != 0)
					{
						anim.SetTrigger("Dancing_Updress");
					}
					Change_Dance_Pose(Actions_UI.Dance_id);
				}
				else
				{
					Speak(inventory.Dance_Block_Speech[Actions_UI.Dance_id]);
					anim.SetTrigger("Cant");
				}
			}
			else
			{
				Stop_Dancing();
			}
		}
		if (actions_Mode == Actions_Mode.toy)
		{
			if (!Toy_Mode)
			{
				Toy_Mode = true;
			}
			else
			{
				Toy_Mode = false;
			}
			Set_Toy_Mode();
		}
	}

	private void Show_Current_Action(int Action)
	{
		for (int i = 0; i < 4; i++)
		{
			action_names[i].gameObject.SetActive(i == Action);
		}
	}

	public void Show_Action_Buttons()
	{
		Showing_Actions = !Showing_Actions;
		if (Showing_Actions)
		{
			for (int i = 0; i < 4; i++)
			{
				action_names[i].gameObject.SetActive(value: true);
			}
		}
		else
		{
			for (int j = 0; j < 4; j++)
			{
				action_names[j].gameObject.SetActive(j == Current_Action);
			}
		}
	}

	public void Turn_Action_Indicators(int Type, int Value)
	{
		if (Type == 0)
		{
			Show_Current_Action(0);
			Actions_UI.actions_anim.SetBool("Toy", value: false);
			Actions_UI.actions_anim.SetBool("Dance", value: false);
			Actions_UI.actions_anim.SetBool("Jerk", value: false);
		}
		if (Type == 1)
		{
			Show_Current_Action(1);
			if (Actions_UI.Pose_id == 0)
			{
				Actions_UI.Pose_id = 1;
			}
			Actions_UI.actions_anim.SetInteger("Animation_id", Actions_UI.Poses_List[Actions_UI.Pose_id]);
			Actions_UI.actions_anim.SetBool("Toy", value: false);
			Actions_UI.actions_anim.SetBool("Dance", value: false);
			Actions_UI.actions_anim.SetBool("Jerk", value: true);
			Actions_UI.Choose_Pose(Value);
		}
		if (Type == 2)
		{
			Show_Current_Action(2);
			if (Actions_UI.Dance_id == 0)
			{
				Actions_UI.Dance_id = 1;
			}
			Actions_UI.actions_anim.SetInteger("Animation_id", Actions_UI.Dance_id);
			Actions_UI.actions_anim.SetBool("Toy", value: false);
			Actions_UI.actions_anim.SetBool("Dance", value: true);
			Actions_UI.actions_anim.SetBool("Jerk", value: false);
			Actions_UI.Choose_Dance(Value);
		}
		if (Type == 3)
		{
			Show_Current_Action(3);
			Actions_UI.actions_anim.SetBool("Toy", value: true);
			Actions_UI.actions_anim.SetBool("Dance", value: false);
			Actions_UI.actions_anim.SetBool("Jerk", value: false);
		}
		Actions_UI.Set_Root(Type);
	}

	public void Block_Ladder_Actions(bool On)
	{
		OnLadder = On;
	}

	public void Start_Action_Speech()
	{
		if (!Action_Did)
		{
			Speak(Any_Action_First_Speech);
			Action_Did = true;
		}
	}

	public void Go_Out_From_Toy_Place()
	{
		Release_Place_Action(3);
	}

	public void Release_Place_Action(int Type)
	{
		int value = 0;
		if (Type == 2)
		{
			value = UnityEngine.Random.Range(0, 8);
		}
		Release_Action(Type, value);
		if (Type == 3)
		{
			Inside_Toy_Place = !Inside_Toy_Place;
		}
	}

	private void Release_Action(int Type, int Value)
	{
		if (OnLadder || swimming.isSwimming)
		{
			return;
		}
		Wrong_Speech_Said = false;
		if (Type == 1)
		{
			if (Dancing)
			{
				Stop_Dancing();
			}
			if (Toy_Mode)
			{
				Stop_Toying();
			}
			actions_Mode = Actions_Mode.jerk;
			Set_Masturbation_Buttons();
			if (Masturbating)
			{
				if (!Inside_Jerk_Place)
				{
					Stop_Masturbating();
				}
			}
			else
			{
				Change_Masturbation_Pose(Value);
			}
		}
		if (Type == 2)
		{
			if (Masturbating)
			{
				Stop_Masturbating();
			}
			if (Toy_Mode)
			{
				Stop_Toying();
			}
			if (!Danced)
			{
				Speak(First_Dance_Speech);
				Danced = true;
			}
			Dancing = !Dancing;
			actions_Mode = Actions_Mode.dance;
			if (!Dancing)
			{
				Stop_Dancing();
			}
			else
			{
				Change_Dance_Pose(Value);
			}
		}
		if (Type == 3)
		{
			if (Masturbating)
			{
				Stop_Masturbating();
			}
			if (Dancing)
			{
				Stop_Dancing();
			}
			Toy_Mode = !Toy_Mode;
			actions_Mode = Actions_Mode.toy;
			Set_Toy_Mode();
			Debug.Log(Toy_Mode);
		}
	}

	public void Turn_Smartphone()
	{
		menu.Turn_Smartphone();
		interface_script.Turn_Cursor();
		mission_Explorer.Complete_Phone_Mission();
		smartphone.Check_For_messages();
	}

	private bool Mouse_Unblocked()
	{
		if (!EventSystem.current.IsPointerOverGameObject() && !Masturbating && !inventory.Wearing && !swimming.isSwimming && !Toy_Mode && !OnLadder && !smartphone.Pointer_On_Map && interface_script.GameIsStarted && !interface_script.GameIsPause)
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.E))
		{
			if (Inside_Jerk_Place && (bool)Current_Jerk_Place)
			{
				Current_Jerk_Place.Go_Out_From_Jerk_Place();
			}
			if (Inside_Dance_Place && (bool)Current_Dance_Place)
			{
				Current_Dance_Place.Go_Out_From_Dance_Place();
			}
			if (Inside_Toy_Place)
			{
				Go_Out_From_Toy_Place();
			}
		}
		if (Input.GetKeyDown(KeyCode.Q) && (bool)Opponent)
		{
			Opponent.Check_For_Dialogue();
		}
		if (!interface_script.Sex_Mode)
		{
			for (int i = 0; i < inventory.Actions.Length; i++)
			{
				if (Input.GetKeyDown(inventory.Actions[i].Key))
				{
					Release_Action(inventory.Actions[i].Action_Type, inventory.Actions[i].Action_Value);
				}
			}
			if (Input.GetKeyDown(KeyCode.J))
			{
				interface_script.Use_Item(0);
			}
			if (Input.GetKeyDown(KeyCode.K))
			{
				interface_script.Use_Item(1);
			}
			if (Input.GetKeyDown(KeyCode.L))
			{
				interface_script.Use_Item(2);
			}
		}
		if (Toy_Mode && (bool)inventory.current_toy.Mesh)
		{
			if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButton(0))
			{
				if (inventory.current_toy.Deepness == 1f)
				{
					back = true;
				}
				else if (inventory.current_toy.Deepness == 0f)
				{
					back = false;
				}
				if (back && inventory.current_toy.Deepness > 0f)
				{
					inventory.current_toy.Deepness = Mathf.MoveTowards(inventory.current_toy.Deepness, 0f, Time.deltaTime * Jerk_Speed * 1.5f);
				}
				else if (!back && inventory.current_toy.Deepness < 1f)
				{
					inventory.current_toy.Deepness = Mathf.MoveTowards(inventory.current_toy.Deepness, 1f, Time.deltaTime * Jerk_Speed * 1.5f);
				}
				if (inventory.current_toy.Deepness == 0.9f && !Toy_Sound.isPlaying)
				{
					if (back)
					{
						Toy_Sound.PlayOneShot(Toy_Out);
					}
					else
					{
						Toy_Sound.PlayOneShot(Toy_In);
					}
				}
				Current_Action = 3;
				if (!Fucking)
				{
					Nake_Level += 2;
					Fucking = true;
				}
				anim.SetBool("Fucking", Fucking);
				if (inventory.current_toy.Pussy)
				{
					anim.SetFloat("Toy_Deepness", inventory.current_toy.Deepness);
				}
				z_Toy_Pos = Mathf.Lerp(inventory.current_toy.Min, inventory.current_toy.Max, inventory.current_toy.Deepness);
				inventory.Save_Deepness();
				inventory.Check_Checkmarks();
				inventory.current_toy.Mesh.transform.localPosition = new Vector3(inventory.current_toy.Mesh.transform.localPosition.x, inventory.current_toy.Mesh.transform.localPosition.y, z_Toy_Pos);
			}
			else if (Fucking)
			{
				Nake_Level -= 2;
				Stop_Fucking();
			}
		}
		if (Masturbating && !inventory.Wearing)
		{
			cameras.Cam_Y_Offset = cameras.Cam_Masturbation_Y[anim.GetInteger("Masturbate_Pose")];
		}
		else if (Toy_Mode)
		{
			cameras.Cam_Y_Offset = cameras.Cam_Toy_Offset_Y;
		}
		else
		{
			cameras.Cam_Y_Offset = 0f;
		}
		if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
		{
			if (Input.GetKey(KeyCode.UpArrow) && cameras.Cam_Control.offSetPlayerPivot < 0.48f)
			{
				cameras.Additional_Y_Offset += 0.2f * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.DownArrow) && cameras.Cam_Control.offSetPlayerPivot > -0.9f)
			{
				cameras.Additional_Y_Offset -= 0.2f * Time.deltaTime;
			}
			interface_script.Rox_Interface.offset_camera_slider_up.gameObject.SetActive(value: true);
			interface_script.Rox_Interface.offset_camera_slider_up.value = cameras.Cam_Control.offSetPlayerPivot;
		}
		else if (interface_script.Rox_Interface.offset_camera_slider_up.gameObject.activeInHierarchy)
		{
			interface_script.Rox_Interface.offset_camera_slider_up.gameObject.SetActive(value: false);
		}
		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
		{
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				right_offset_camera -= 0.4f * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				right_offset_camera += 0.4f * Time.deltaTime;
			}
			right_offset_camera = Mathf.Clamp(right_offset_camera, -0.7f, 0.7f);
			cameras.Cam_Control.CameraStateList.tpCameraStates[0].right = right_offset_camera;
			interface_script.Rox_Interface.offset_camera_slider_right.gameObject.SetActive(value: true);
			interface_script.Rox_Interface.offset_camera_slider_right.value = right_offset_camera;
		}
		else if (interface_script.Rox_Interface.offset_camera_slider_right.gameObject.activeInHierarchy)
		{
			interface_script.Rox_Interface.offset_camera_slider_right.gameObject.SetActive(value: false);
		}
		cameras.Cam_Control.offSetPlayerPivot = Mathf.MoveTowards(cameras.Cam_Control.offSetPlayerPivot, cameras.Cam_Y_Offset + cameras.Additional_Y_Offset, Time.deltaTime);
		if (Input.GetMouseButtonDown(0) && Mouse_Unblocked())
		{
			if (!inventory.data.Clothes[3].Weared && !inventory.data.Clothes[6].Weared)
			{
				Cover(1, 2);
			}
			else
			{
				anim.SetBool("In_Bra", inventory.data.Clothes[1].Weared);
				Requied_Cloth = (inventory.data.Clothes[6].Weared ? 3 : 2);
				Showing_Coef = (inventory.data.Clothes[1].Weared ? 2 : 4);
				Check_Cloth_For_Showing(Requied_Cloth);
			}
		}
		if (Input.GetMouseButtonDown(1) && Mouse_Unblocked())
		{
			if (!inventory.data.Clothes[2].Weared && !inventory.data.Clothes[6].Weared)
			{
				Cover(2, 3);
			}
			else
			{
				Requied_Cloth = ((!inventory.data.Clothes[6].Weared) ? 1 : 3);
				Showing_Coef = (inventory.data.Clothes[0].Weared ? 2 : 4);
				Check_Cloth_For_Showing(Requied_Cloth);
			}
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			if (!controller.ragdolled)
			{
				anim.SetBool("Ragdolled", value: true);
				Stop_All_Actions();
				GetComponent<vRagdoll>().horizontalMultiplier = anim.GetFloat("InputMagnitude");
				GetComponent<vRagdoll>().verticalMultiplier = anim.GetFloat("InputMagnitude");
				GetComponent<vRagdoll>().ActivateRagdoll(null, 100f);
			}
			else
			{
				anim.SetBool("Ragdolled", value: false);
				GetComponent<vRagdoll>().ResetStayRagdolled();
			}
		}
		if (Map && smartphone.Pointer_On_Map)
		{
			Control_Map();
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			if (inventory.data.progress_data.Have_Phone)
			{
				Turn_Photo_Mode();
			}
			else
			{
				Speak(Dont_Have_Phone);
			}
		}
		if (Remain_Photo_Counter_Time > 0f)
		{
			Remain_Photo_Counter_Time -= Time.deltaTime;
		}
		else if (Photo_Making)
		{
			Photo_Making = false;
		}
		Smooth_Constraint_Weight();
		if (Photo_Mode)
		{
			Move_Photo_Camera();
		}
		if (Input.GetKeyDown(KeyCode.O))
		{
			Turn_Smartphone();
		}
		Show_Energy();
		Fill_Excite();
		if (Watchers_Count > 0 && !Drunk)
		{
			Fill_Shame();
		}
		Change_Jerk_Speed();
	}

	public void Stop_All_Actions()
	{
		if (Covered)
		{
			Stop_Covering();
		}
		if (Showing)
		{
			Check_Cloth_For_Showing(Current_Cloth);
		}
		if (Photo_Mode)
		{
			Turn_Photo_Mode();
		}
		if (Masturbating)
		{
			Stop_Masturbating();
		}
	}

	public void Turn_Photo_Mode()
	{
		Photo_Mode = !Photo_Mode;
		photo.Photo_Cam.gameObject.SetActive(Photo_Mode);
		photo.Photo_UI.gameObject.SetActive(Photo_Mode);
		cameras.Main_Cam.gameObject.SetActive(!Photo_Mode);
		Head_Photo.constraintActive = Photo_Mode;
		if (Photo_Mode)
		{
			Turn_On_TPS_Mode();
		}
		else
		{
			Return_Old_Mode();
		}
	}

	private void Move_Photo_Camera()
	{
		photo.Photo_Cam.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 1000f;
		photo.Photo_Cam.fieldOfView = Mathf.Clamp(photo.Photo_Cam.fieldOfView, 55f, 100f);
		float x = photo.Hand_Goal.transform.localPosition.x - Input.GetAxis("Mouse X") * Time.deltaTime * 5f;
		float y = photo.Hand_Goal.transform.localPosition.y + Input.GetAxis("Mouse Y") * Time.deltaTime * 5f;
		photo.Hand_Goal.transform.localPosition = new Vector3(x, y, photo.Hand_Goal.transform.localPosition.z);
	}

	private void OnAnimatorIK(int layerIndex)
	{
		anim.SetIKPositionWeight(AvatarIKGoal.RightHand, photo.current_ik_weight);
		anim.SetIKRotationWeight(AvatarIKGoal.RightHand, photo.current_ik_weight);
		anim.SetIKPosition(AvatarIKGoal.RightHand, photo.Hand_Goal.position);
		anim.SetIKRotation(AvatarIKGoal.RightHand, photo.Hand_Goal.rotation);
		if (Photo_Mode)
		{
			photo.current_ik_weight = 1f;
			photo.Head_constraint.constraintActive = true;
		}
		else
		{
			photo.current_ik_weight = 0f;
			photo.Head_constraint.constraintActive = false;
		}
	}

	public void Turn_Head_Parts()
	{
		for (int i = 0; i < Head_Parts_Tps.Length; i++)
		{
			Head_Parts_Tps[i].gameObject.SetActive(FPS_Control.isThirdPerson);
		}
	}

	public void Turn_On_TPS_Mode()
	{
		Was_TPS_Mode = FPS_Control.isThirdPerson;
		if (!FPS_Control.isThirdPerson)
		{
			FPS_Control.FpcSwap();
		}
		Turn_Head_Parts();
	}

	public void Return_Old_Mode()
	{
		if (!Was_TPS_Mode && FPS_Control.isThirdPerson)
		{
			FPS_Control.FpcSwap();
		}
		Turn_Head_Parts();
	}

	public void Turn_Map_Mode()
	{
		Map = !Map;
		Loader.Map_Object.gameObject.SetActive(Map);
		Loader.Map_Camera.transform.position = new Vector3(base.transform.position.x, 200f, base.transform.position.z);
		interface_script.Turn_Cursor();
	}

	private void Control_Map()
	{
		float x = Loader.Map_Camera.transform.position.x + Input.GetAxis("Mouse X") * Time.deltaTime * 100f;
		float z = Loader.Map_Camera.transform.position.z + Input.GetAxis("Mouse Y") * Time.deltaTime * 100f;
		Loader.Map_Camera.transform.position = new Vector3(x, Loader.Map_Camera.transform.position.y, z);
		Loader.Map_Camera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 1000f;
		Loader.Icon_Camera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 1000f;
	}

	public void ChangeUseName(string UseText, bool Buy_Point)
	{
		interface_script.Rox_Interface.Use_Text.gameObject.SetActive(!Buy_Point);
		interface_script.Rox_Interface.Buy_Text.gameObject.SetActive(Buy_Point);
		interface_script.Rox_Interface.Use_Text.text = UseText;
		interface_script.Rox_Interface.Buy_Text.text = UseText;
	}

	public void Smooth_Constraint_Weight()
	{
		if (cameras.constraint.weight != cameras.new_const_value)
		{
			cameras.constraint.weight = Mathf.MoveTowards(cameras.constraint.weight, cameras.new_const_value, Time.deltaTime * cameras.const_change_speed);
		}
	}

	public void Change_Mirror_Pose()
	{
		Mirror_Pose = !Mirror_Pose;
		Actions_UI.actions_anim.SetBool("Mirror", Mirror_Pose);
		anim.SetBool("Mirror_Pose", Mirror_Pose);
	}

	public void Change_Jerk_Speed()
	{
		if (Masturbating || Fucking)
		{
			if (Input.GetMouseButton(1))
			{
				if (Jerk_Speed < 2.5f)
				{
					Jerk_Speed += Time.deltaTime * 0.25f;
				}
			}
			else if (Jerk_Speed > 1f)
			{
				Jerk_Speed -= Time.deltaTime * 0.75f;
			}
		}
		else if (Jerk_Speed > 1f)
		{
			Jerk_Speed -= Time.deltaTime * 0.75f;
		}
		Jerk_Speed = Mathf.Clamp(Jerk_Speed, 1f, 2.5f);
		Speed_Slider.value = Jerk_Speed;
	}

	public void Change_Masturbation_Pose(int pose_id)
	{
		Set_Masturbation_Buttons();
		Check_Pose_For_Unwear(pose_id);
		if (Covered)
		{
			Stop_Covering();
		}
		if (Photo_Mode)
		{
			Turn_Photo_Mode();
		}
		anim.SetBool("In_Bra", inventory.data.Clothes[1].Weared);
		if (!poses[pose_id].No_Requied_Unwear)
		{
			if (!Jerked)
			{
				Jerked = true;
				Speak(Jerk_First_Speech);
			}
			Current_Action = 1;
			if (pose_id != 1)
			{
				Set_Jerk_Skirt();
			}
			else
			{
				Set_Up_Skirt();
			}
			if (!Masturbating)
			{
				Masturbating = true;
				if (Showing)
				{
					if (Showing_Coef == 2)
					{
						Nake_Level += 2;
					}
				}
				else
				{
					Nake_Level += 4;
				}
			}
			Ik_Offset.leftHandOffset = Vector3.Lerp(new Vector3(0f, 0f, 0f), new Vector3(-0.013f, 0f, 0.023f), inventory.data.Character.boobs_size);
			if (Showing)
			{
				if (Current_Cloth == 2)
				{
					anim.SetTrigger("Masturbation");
				}
			}
			else
			{
				anim.SetTrigger("Masturbation");
			}
			anim.SetInteger("Masturbate_Pose", pose_id);
			Current_Mastrurbation_Pose = pose_id;
			if ((bool)Current_Jerk_Place)
			{
				interface_script.Rox_Interface.Jerk_Task_Object.gameObject.SetActive(!mission_Explorer.data.saved_data.Jerk_Places.Used[Current_Jerk_Place.id]);
				base.transform.position = Current_Jerk_Place.target.position;
				base.transform.rotation = Current_Jerk_Place.target.rotation;
			}
			if (pose_id == 1)
			{
				Actions_UI.Choose_And_Mark_Pose(1);
			}
		}
		else
		{
			Interface_anim.SetTrigger("Tip");
		}
		Show_Requied_Cloth_Tip(pose_id);
	}

	public void Stop_Masturbating()
	{
		if (Masturbating)
		{
			Masturbating = false;
			inventory.Check_Nake_Level();
		}
		if (Showing && Current_Cloth == 1)
		{
			Set_Up_Skirt();
		}
		else
		{
			Set_Free_Skirt();
		}
		Current_Action = 0;
		Current_Mastrurbation_Pose = 0;
		Replace_Masturbation_Clothes();
		cameras.Cam_Y_Offset = 0f;
		Ik_Offset.leftHandOffset = new Vector3(0f, 0f, 0f);
		anim.SetInteger("Masturbate_Pose", 0);
		Set_Masturbation_Buttons();
	}

	private void Replace_Masturbation_Clothes()
	{
		if (inventory.data.Clothes[3].Weared && (!Showing || Current_Cloth != 2) && Current_Mastrurbation_Pose == 0 && Masturbating)
		{
			anim.SetTrigger("Shirt_Masturbation_Start");
		}
	}

	public void Set_Masturbation_Buttons()
	{
		for (int i = 1; i < poses.Length; i++)
		{
			Check_Pose_For_Unwear(i);
		}
	}

	public void Check_Pose_For_Unwear(int id)
	{
		poses[id].No_Requied_Unwear = false;
		for (int i = 0; i < poses[id].Requied_Unweared_Cloth.Length; i++)
		{
			if (inventory.data.Clothes[poses[id].Requied_Unweared_Cloth[i]].Weared)
			{
				poses[id].No_Requied_Unwear = true;
			}
		}
	}

	public void Show_Requied_Cloth_Tip(int pose_id)
	{
		for (int i = 0; i < interface_script.Rox_Interface.Cloth_Tip_Image.Length; i++)
		{
			if (i < poses[pose_id].Requied_Unweared_Cloth.Length)
			{
				if (inventory.data.Clothes[poses[pose_id].Requied_Unweared_Cloth[i]].Weared)
				{
					interface_script.Rox_Interface.Cloth_Tip_Image[i].color = new Color(1f, 1f, 1f, 1f);
				}
				else
				{
					interface_script.Rox_Interface.Cloth_Tip_Image[i].color = new Color(1f, 1f, 1f, 0.3f);
				}
				interface_script.Rox_Interface.Cloth_Tip_Image[i].sprite = inventory.Clothes[poses[pose_id].Requied_Unweared_Cloth[i]].inventory_button[0].GetComponent<Image>().sprite;
			}
			else
			{
				interface_script.Rox_Interface.Cloth_Tip_Image[i].color = new Color(1f, 1f, 1f, 0f);
				interface_script.Rox_Interface.Cloth_Tip_Image[i].sprite = null;
			}
		}
		if (inventory.data.Language == 0)
		{
			interface_script.Rox_Interface.Tip_Jerk_Text.text = "ЧТОБЫ ПРИНЯТЬ ЭТУ ПОЗУ НУЖНО СНЯТЬ: ";
		}
		if (inventory.data.Language == 1)
		{
			interface_script.Rox_Interface.Tip_Jerk_Text.text = "To masturbate in this position, you have to take off: ";
		}
	}

	public void Fill_Excite()
	{
		if (Masturbating || Fucking || Dancing)
		{
			anim.SetFloat("Jerk_Speed", Jerk_Speed);
			if (Masturbating)
			{
				if (anim.GetFloat("InputMagnitude") >= 0.4f && anim.GetInteger("Masturbate_Pose") > 1 && !Inside_Jerk_Place)
				{
					Use_Stay_Pose();
				}
				if (controller.ragdolled)
				{
					Stop_Masturbating();
				}
			}
		}
		if (Toy_Mode && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)))
		{
			Stop_Toying();
		}
		if (transferer.Transfering || (bool)Current_Jerk_Place || Toy_Mode || Arrested)
		{
			rigidb.isKinematic = true;
		}
		else
		{
			rigidb.isKinematic = false;
		}
		if ((bool)Current_Jerk_Place && !interface_script.Mission_Rewarded && anim.GetBool("Orgasm_Played"))
		{
			if (Current_Jerk_Place.Money)
			{
				smartphone.Money_Added(Current_Jerk_Place.Reward);
				StartCoroutine(New_Message_With_Delay());
			}
			Current_Jerk_Place.Jerk_Task_Completed();
			anim.SetBool("Orgasm_Played", value: false);
			interface_script.Mission_Rewarded = true;
		}
		Change_Excitement();
	}

	public void Change_Excitement()
	{
		if (Masturbating || Fucking)
		{
			if (!isCuming)
			{
				if (Excitement < 100f)
				{
					Excitement += Time.deltaTime * 5f * Jerk_Speed;
				}
				else
				{
					StartCoroutine(Cuming());
				}
			}
			interface_script.Rox_Interface.Task_Progress_Slider.value = Excitement;
			return;
		}
		if (Excitement > 0f)
		{
			Excitement -= Time.deltaTime * 20f;
			interface_script.Rox_Interface.Task_Progress_Slider.value = Excitement;
		}
		if (Excitement < 0f)
		{
			Excitement = 0f;
		}
	}

	public void Use_Stay_Pose()
	{
		Check_Pose_For_Unwear(1);
		if (!poses[1].No_Requied_Unwear)
		{
			Change_Masturbation_Pose(1);
		}
		else
		{
			Stop_Masturbating();
		}
	}

	private IEnumerator Cuming()
	{
		if ((bool)Current_Jerk_Place)
		{
			mission_Explorer.Earn_Experience(Current_Jerk_Place.Reward);
		}
		if (!isCuming)
		{
			anim.SetTrigger("Orgasm");
		}
		isCuming = true;
		yield return new WaitForSeconds(1f);
		if (!Fucking)
		{
			if (!inventory.data.Clothes[0].Weared)
			{
				Squirt.SetActive(value: true);
			}
		}
		else
		{
			if (!Pussy_Sound.isPlaying)
			{
				Pussy_Sound.PlayOneShot(Squirt_Sound);
			}
			Squirt.SetActive(value: true);
		}
		while (Excitement > 0f)
		{
			Excitement -= Time.deltaTime * 20f;
			interface_script.Rox_Interface.Task_Progress_Slider.value = Excitement;
		}
		if (Excitement <= 0f)
		{
			Excitement = 0f;
			yield return new WaitForSeconds(2f);
			Squirt.SetActive(value: false);
			yield return new WaitForSeconds(2f);
			isCuming = false;
		}
	}

	public void Set_Constraint(bool Fps)
	{
		if (Fps)
		{
			cameras.constraint.SetSource(0, cameras.Source_Head);
			cameras.new_const_value = cameras.Fps_const_weight;
			cameras.Player_Control.freeSpeed.rotationSpeed = cameras.Fps_Rotation_Speed;
			cameras.const_change_speed = 0.5f;
			cameras.constraint.SetRotationOffset(0, new Vector3(0f, 180f, 0f));
		}
		else
		{
			cameras.Player_Control.freeSpeed.rotationSpeed = 9f;
			cameras.new_const_value = 0f;
			cameras.const_change_speed = 1f;
		}
	}

	public void Switch_Walk()
	{
		if (WasNonFps)
		{
			cameras.constraint.SetTranslationOffset(0, cameras.new_translation_offset);
		}
		else
		{
			cameras.constraint.SetTranslationOffset(0, Vector3.MoveTowards(cameras.constraint.GetTranslationOffset(0), cameras.new_translation_offset, Time.deltaTime * 1.25f));
		}
		if (Crouching)
		{
			cameras.new_translation_offset = cameras.Head_Cam_Crouch_Pos;
		}
		else if (Running)
		{
			cameras.new_translation_offset = cameras.Head_Cam_Run_Pos;
		}
		else if (Walking)
		{
			cameras.new_translation_offset = cameras.Head_Cam_Walk_Pos;
		}
		else
		{
			cameras.new_translation_offset = cameras.Head_Cam_Idle_Pos;
		}
	}

	private IEnumerator Fatness_Decreasing()
	{
		while (base.gameObject.activeInHierarchy)
		{
			yield return new WaitForSeconds(0.5f);
			if (anim.GetFloat("InputMagnitude") > 0.9f)
			{
				int num = Mathf.RoundToInt(Mathf.Lerp(50f, 100f, inventory.Editing.data.Character.fatness));
				if (inventory.Editing.data.Character.fatness > 0.001f)
				{
					inventory.Editing.data.Character.fatness -= 0.001f;
					inventory.Editing.Get_Fatness_Settings();
				}
				if (Mathf.RoundToInt(Mathf.Lerp(50f, 100f, inventory.Editing.data.Character.fatness)) < num && inventory.Editing.data.Character.fatness > 0f)
				{
					Show_Fatness(Increase: false);
				}
			}
		}
	}

	public void Throw_Item(int id)
	{
		UnityEngine.Object.Instantiate(Throw_Stuff[id], Hand_Stuff[id].transform.position, Hand_Stuff[id].transform.rotation, null);
	}

	public void Check_First_Run()
	{
		if (!Runned)
		{
			Speak(First_Run_Speech);
			Runned = true;
		}
	}

	public void Fatness_Increasing()
	{
		if (inventory.Editing.data.Character.fatness < 1f)
		{
			inventory.Editing.data.Character.fatness += 0.1f;
			inventory.Editing.Get_Fatness_Settings();
			Show_Fatness(Increase: true);
		}
		if (!Eaten)
		{
			Speak(First_Eating_Speech);
			Eaten = true;
		}
	}

	public void Show_Fatness(bool Increase)
	{
		Interface_anim.SetTrigger("New_Weight");
		int num = Mathf.RoundToInt(Mathf.Lerp(50f, 100f, inventory.Editing.data.Character.fatness));
		if (!Increase)
		{
			if (inventory.data.Language == 0)
			{
				interface_script.Rox_Interface.Weight_Text.text = inventory.data.Name + " похудела. Ее вес - " + num + " кг.";
			}
			if (inventory.data.Language == 1)
			{
				interface_script.Rox_Interface.Weight_Text.text = inventory.data.Name + " has lost weight from running. Her weight is " + num + " kg.";
			}
		}
		else
		{
			if (inventory.data.Language == 0)
			{
				interface_script.Rox_Interface.Weight_Text.text = inventory.data.Name + " потолстела. Ее вес - " + num + " кг.";
			}
			if (inventory.data.Language == 1)
			{
				interface_script.Rox_Interface.Weight_Text.text = inventory.data.Name + " got fat on noodles. Her weight is " + num + " kg.";
			}
		}
	}

	public void Show_Infinity_Energy()
	{
		Interface_anim.SetTrigger("New_Weight");
		if (inventory.data.Language == 0)
		{
			interface_script.Rox_Interface.Weight_Text.text = inventory.data.Name + " чувствует невероятный прилив энергии";
		}
		if (inventory.data.Language == 1)
		{
			interface_script.Rox_Interface.Weight_Text.text = inventory.data.Name + " feels an incredible burst of energy";
		}
	}

	public void Show_Infinity_Embarassment()
	{
		Interface_anim.SetTrigger("New_Weight");
		if (inventory.data.Language == 0)
		{
			interface_script.Rox_Interface.Weight_Text.text = inventory.data.Name + " чувствует опьянение. Теперь ничто не смутит ее";
		}
		if (inventory.data.Language == 1)
		{
			interface_script.Rox_Interface.Weight_Text.text = inventory.data.Name + " feels like she's drunk. Nothing can embarrass her now";
		}
	}

	public void Show_Shamed_People_Counter()
	{
		inventory.data.progress_data.People_Embarassed++;
		inventory.data.progress_data.Five_People_Counter++;
		interface_script.Rox_Interface.Remain_People.gameObject.SetActive(inventory.data.progress_data.Five_People_Counter < 5);
		if (!Interface_anim.GetBool("People_Shows"))
		{
			Interface_anim.SetTrigger("Show_People");
		}
		if (inventory.data.Language == 0)
		{
			interface_script.Rox_Interface.Weight_Text.text = "ПРОХОЖИХ СМУЩЕНО: " + inventory.data.progress_data.People_Embarassed;
		}
		if (inventory.data.Language == 1)
		{
			interface_script.Rox_Interface.Weight_Text.text = "PEOPLE EMBARRASSED BY YOU: " + inventory.data.progress_data.People_Embarassed;
		}
		int num = 5 - inventory.data.progress_data.Five_People_Counter;
		if (inventory.data.Language == 0)
		{
			interface_script.Rox_Interface.Remain_People.text = "ДО НАГРАДЫ ОСТАЛОСЬ: " + num;
		}
		if (inventory.data.Language == 1)
		{
			interface_script.Rox_Interface.Remain_People.text = "THE REWARD WILL BE AFTER: " + num;
		}
		if (inventory.data.progress_data.Five_People_Counter >= 5)
		{
			smartphone.Money_Added(100);
			StartCoroutine(New_Message_With_Delay());
			inventory.data.progress_data.Five_People_Counter = 0;
		}
		mission_Explorer.Complete_Show_Mission();
	}

	private IEnumerator New_Message_With_Delay()
	{
		yield return new WaitForSeconds(4f);
		interface_script.Show_Phone_Icon();
	}

	public void Fill_Shame()
	{
		Nake_Level = Mathf.Clamp(Nake_Level, 0, 8);
		if (!transferer.Transfering)
		{
			Shame += Multi_Shame * Time.deltaTime;
			interface_script.Rox_Interface.Shame_Slider.value = Shame;
		}
		if (!(Shame >= 100f))
		{
			return;
		}
		if (Shame_Time_Counter < 10f)
		{
			if (!interface_script.Show_Warning_Shame)
			{
				interface_script.Turn_Warning_Shame(On: true);
			}
			Shame_Time_Counter += Time.deltaTime;
		}
		else
		{
			Fail_Hard_Mode(0);
		}
	}

	public IEnumerator Fail_With_Delay(int reason)
	{
		GetComponent<vThirdPersonInput>().enabled = false;
		yield return new WaitForSeconds(2f);
		Fail_Hard_Mode(reason);
	}

	public void Fail_Hard_Mode(int reason)
	{
		if (reason == 0)
		{
			fail_reason = Fail_Reason.shame;
		}
		if (reason == 1)
		{
			fail_reason = Fail_Reason.unhappy;
		}
		if (reason == 2)
		{
			fail_reason = Fail_Reason.cops;
		}
		if (reason == 3)
		{
			fail_reason = Fail_Reason.morning;
		}
		GetComponent<vThirdPersonInput>().enabled = false;
		StartCoroutine(Darkness(win: false));
	}

	public void Decrease_Shame()
	{
		if (Watchers_Count <= 0)
		{
			Shame -= 1f;
			Shame = Mathf.Clamp(Shame, 0f, 100f);
			interface_script.Rox_Interface.Shame_Slider.value = Shame;
			if (interface_script.Show_Warning_Shame)
			{
				interface_script.Turn_Warning_Shame(On: false);
				Shame_Time_Counter = 0f;
			}
		}
	}

	private IEnumerator Shame_Decreasing()
	{
		while (base.gameObject.activeInHierarchy)
		{
			yield return new WaitForSeconds(0.1f);
			Decrease_Shame();
			if (!Drunk)
			{
				interface_script.Rox_Interface.Shame_Object.gameObject.SetActive(Shame > 0f);
			}
			else
			{
				interface_script.Rox_Interface.Shame_Object.gameObject.SetActive(value: true);
			}
		}
	}

	public void Tired_Of_Action()
	{
		if (!Tired_Animation_Played)
		{
			anim.SetTrigger("Tired");
			if (Dancing)
			{
				Stop_Dancing();
			}
			Speak(Tired_Action);
			Tired_Animation_Played = true;
		}
	}

	public void Tired_Of_Run()
	{
		if (!Tired_Animation_Played)
		{
			anim.SetTrigger("Tired");
			Speak(Tired_Run);
			Tired_Animation_Played = true;
		}
	}

	public void Energy_Drink()
	{
		controller.Energized = true;
		interface_script.Rox_Interface.Energy_Color.color = Color.yellow;
	}

	public void Beer_Drunked()
	{
		Drunk = true;
		interface_script.Rox_Interface.Embarassment_Color.color = Color.yellow;
	}

	public void Show_Energy()
	{
		if (controller.currentStamina > 10f && Tired_Animation_Played)
		{
			Tired_Animation_Played = false;
		}
		interface_script.Rox_Interface.Hungryness_Slider.value = controller.currentStamina;
		if (!controller.Energized)
		{
			interface_script.Rox_Interface.Hungry_Object.SetActive(controller.currentStamina < 100f);
		}
		else
		{
			interface_script.Rox_Interface.Hungry_Object.SetActive(value: true);
		}
	}

	private void OnCollisionEnter(Collision col)
	{
		sounds.Hit_Audio.volume = 0.5f;
		if (col.gameObject.tag == "Barrel")
		{
			sounds.Hit_Audio.PlayOneShot(sounds.BarrelHit);
		}
		if (col.gameObject.tag == "Wood")
		{
			sounds.Hit_Audio.PlayOneShot(sounds.WoodHit);
		}
		if (col.gameObject.tag == "Plastic")
		{
			sounds.Hit_Audio.PlayOneShot(sounds.PlasticHit);
		}
		if (col.gameObject.tag == "Flowerpot")
		{
			sounds.Hit_Audio.PlayOneShot(sounds.FlowerpotHit);
		}
		if (col.gameObject.tag == "Cardboard")
		{
			sounds.Hit_Audio.PlayOneShot(sounds.CardboardHit);
		}
		if (col.gameObject.tag == "Door" && !sounds.Hit_Audio.isPlaying)
		{
			sounds.Hit_Audio.PlayOneShot(sounds.DoorHit);
		}
	}

	private void OnCollisionExit(Collision col)
	{
		if (col.gameObject.tag == "Door" && sounds.Hit_Audio.isPlaying)
		{
			sounds.Hit_Audio.Stop();
		}
	}

	private IEnumerator Darkness(bool win)
	{
		transferer.Transfering = true;
		interface_script.Rox_Interface.Jerk_Task_Object.gameObject.SetActive(value: false);
		anim.SetFloat("InputMagnitude", 0f);
		if (Masturbating)
		{
			Stop_Masturbating();
		}
		if (Dancing)
		{
			Stop_Dancing();
		}
		if (Toy_Mode)
		{
			Stop_Toying();
		}
		if (Inside_Jerk_Place)
		{
			Current_Jerk_Place.GetComponent<Jerk_Place>().Go_Out_From_Jerk_Place();
		}
		interface_script.Rox_Interface.Shame_Slider.value = Shame;
		Interface_anim.Play("Darkness");
		yield return new WaitForSeconds(1.1f);
		Return_Base_Clothes();
		Restart_Player();
		if (interface_script.Show_Warning_Shame)
		{
			interface_script.Turn_Warning_Shame(On: false);
			Shame_Time_Counter = 0f;
		}
		body_ik.solver.leftHandEffector.positionWeight = 0f;
		body_ik.solver.rightHandEffector.positionWeight = 0f;
		body_ik.solver.leftHandEffector.rotationWeight = 0f;
		body_ik.solver.rightHandEffector.rotationWeight = 0f;
		body_ik.solver.leftHandEffector.target = null;
		body_ik.solver.rightHandEffector.target = null;
		Arrested = false;
		if (!win)
		{
			interface_script.Fail_Game();
		}
		else
		{
			interface_script.Win_Game();
		}
		UnityEngine.Object.FindObjectOfType<Street_Control>().Restore_Npc();
		yield return new WaitForSeconds(1f);
		input.enabled = true;
		if (!win)
		{
			anim.SetTrigger("Fail");
			if (fail_reason == Fail_Reason.shame)
			{
				Speak(Fail_Shame);
				if (inventory.data.Language == 0)
				{
					interface_script.Rox_Interface.Weight_Text.text = "НОЧЬ ОБНУЛЕНА. ПРИЧИНА - " + inventory.data.Name + " СМУЩЕНА";
				}
				if (inventory.data.Language == 1)
				{
					interface_script.Rox_Interface.Weight_Text.text = "Night is failed. Reason - " + inventory.data.Name + " is embarrassed";
				}
			}
			if (fail_reason == Fail_Reason.unhappy)
			{
				Speak(Fail_Unhappy);
				if (inventory.data.Language == 0)
				{
					interface_script.Rox_Interface.Weight_Text.text = "НОЧЬ ОБНУЛЕНА. ПРИЧИНА - " + inventory.data.Name + " НЕСЧАСТНА";
				}
				if (inventory.data.Language == 1)
				{
					interface_script.Rox_Interface.Weight_Text.text = "Night is failed. Reason - " + inventory.data.Name + " is unhappy";
				}
			}
			if (fail_reason == Fail_Reason.cops)
			{
				Speak(Fail_Cops);
				int remain_Money = inventory.data.money.Remain_Money;
				inventory.data.money.Remain_Money = 0;
				smartphone.Recount_Money();
				if (inventory.data.Language == 0)
				{
					interface_script.Rox_Interface.Weight_Text.text = inventory.data.Name + " ЗАДЕРЖАНА КОПАМИ. С ВАС ВЗЯЛИ ШТРАФ - " + remain_Money + " ЙЕН";
				}
				if (inventory.data.Language == 1)
				{
					interface_script.Rox_Interface.Weight_Text.text = inventory.data.Name + "'s been catched by the cops. She was fined " + remain_Money + " yen";
				}
			}
			if (fail_reason == Fail_Reason.morning)
			{
				Speak(Fail_Morning);
				if (inventory.data.Language == 0)
				{
					interface_script.Rox_Interface.Weight_Text.text = "НОЧЬ ОБНУЛЕНА. ПРИЧИНА - " + inventory.data.Name + " НЕ УСПЕЛА В ДОМ ДО РАССВЕТА";
				}
				if (inventory.data.Language == 1)
				{
					interface_script.Rox_Interface.Weight_Text.text = "Night is failed. Reason - " + inventory.data.Name + " didn't get back home till dawn";
				}
			}
			Interface_anim.SetTrigger("New_Weight");
		}
		transferer.Transfering = false;
		New_Day_Played = false;
		Night_Fail_Started = false;
	}

	private void Return_Base_Clothes()
	{
		inventory.Check_For_Toy_Inside();
		if (!inventory.data.progress_data.Have_Wardrobe)
		{
			if (!inventory.data.Clothes[0].Weared && !inventory.data.Have_Toy_Inside)
			{
				inventory.data.Clothes[0].Weared = true;
			}
			if (!inventory.data.Clothes[1].Weared)
			{
				inventory.data.Clothes[1].Weared = true;
			}
			if (inventory.data.Clothes[0].Weared)
			{
				inventory.Wear_Cloth_On_Model(0);
			}
			inventory.Wear_Cloth_On_Model(1);
		}
		inventory.Check_Nake_Level();
		inventory.Check_Legs_Size();
	}

	private void Restart_Player()
	{
		Shame = 0f;
		Watchers_Count = 0;
		Happiness = inventory.data.Start_Happiness;
		inventory.data.Current_Happiness = inventory.data.Start_Happiness;
		Stop_All_Actions();
		Loader.data.saved_data.Spawn_position_id = 0;
		Loader.Spawn_Player();
		mission_Explorer.Try_Skip_Mission();
	}

	public IEnumerator Temporary_Excited()
	{
		anim.SetTrigger("Start_Fill_Excite");
		anim.SetBool("Excited", value: true);
		yield return new WaitForSeconds(7f);
		anim.SetBool("Excited", value: false);
	}

	public bool is_Ragdolled()
	{
		return controller.ragdolled;
	}

	private void Set_Achieve_Progress(int id)
	{
		inventory.data.progress_data.Interior_Achieves[id].Progress++;
		inventory.data.progress_data.Sum_Score++;
		smartphone.Set_Phone_Achieves();
	}

	private void Reward_Achieve(int id)
	{
		Interior_Settings interior_Settings = inventory.hidden_data.Interior_Settings[id];
		string text = "I";
		if (inventory.data.progress_data.Interior_Achieves[id].Progress == 1)
		{
			text = "II";
		}
		if (inventory.data.progress_data.Interior_Achieves[id].Progress == 2)
		{
			text = "III";
		}
		string str = inventory.data.progress_data.Interior_Achieves[id].Name_Achieve[inventory.data.Language] + " " + text;
		int progress = inventory.data.progress_data.Interior_Achieves[id].Progress;
		smartphone.Show_Achieve_Earned(interior_Settings.Picture, str);
		smartphone.Money_Added(inventory.data.progress_data.Interior_Achieve_Money[progress]);
	}

	public void Get_Interior_Achieve(int id)
	{
		Reward_Achieve(id);
		Set_Achieve_Progress(id);
		StartCoroutine(New_Message_With_Delay());
	}

	public void Get_Street_Achieve(int id)
	{
		smartphone.Money_Added(inventory.data.progress_data.Street_Achieves[id].Complete_Reward);
		StartCoroutine(New_Message_With_Delay());
	}

	public void New_Happiness(int count)
	{
		Happiness += count;
	}

	public void Count_And_Show_Shame_Happiness()
	{
		Multi_Shame = (float)Nake_Level * Hiding + (float)(Watchers_Count * 2);
		if (Nake_Level <= 2)
		{
			_ = Watchers_Count;
		}
		else
		{
			_ = Nake_Level / 2;
			_ = Hiding;
		}
		Multi_Happiness = (float)Nake_Level * Hiding + (float)(Watchers_Count * 2);
		_ = Nake_Level / 2;
		_ = Hiding;
		_ = Watchers_Count;
		interface_script.Rox_Interface.Risk_Slider.value = Nake_Level + Watchers_Count;
	}

	public void Decrease_Happiness()
	{
		if (Full_Of_Food)
		{
			interface_script.Rox_Interface.Happiness_Color.color = new Color(1f, 1f, 0f);
		}
		else if (!Have_Known_Watchers && Shame < 100f)
		{
			interface_script.Rox_Interface.Happiness_Color.color = new Color(1f, 1f, 0f);
		}
		else
		{
			interface_script.Rox_Interface.Happiness_Color.color = new Color(1f, 0f, 0f);
		}
		if (!menu.Loading_Process && interface_script.GameIsStarted)
		{
			if (!Full_Of_Food)
			{
				if (Shame >= 100f)
				{
					Happiness -= 0.25f;
				}
				if (Have_Known_Watchers)
				{
					Happiness -= 0.25f;
				}
			}
			Happiness = Mathf.Clamp(Happiness, 0f, 100f);
		}
		interface_script.Rox_Interface.Happiness_Slider.value = Happiness;
		if (Happiness == 0f && !Bad_Mood_Showed)
		{
			anim.SetTrigger("Unhappy");
			Speak(Bad_Mood_Speech);
			Bad_Mood_Showed = true;
			Fail_Hard_Mode(1);
		}
	}

	public void Increase_Happiness()
	{
		if (Current_Action != 0)
		{
			interface_script.Rox_Interface.Happiness_Color.color = new Color(0f, 1f, 0f);
			if (Dancing)
			{
				Happiness_Coef = 3f;
			}
			if (Masturbating)
			{
				if (isCuming)
				{
					Happiness_Coef = 5f;
				}
				else
				{
					Happiness_Coef = 0.6f * Jerk_Speed;
				}
			}
			if (Fucking)
			{
				if (isCuming)
				{
					Happiness_Coef = 5f;
				}
				else
				{
					Happiness_Coef = 1.2f * Jerk_Speed;
				}
			}
			Happiness += 0.025f * Happiness_Coef + 0.025f * Multi_Happiness;
			if (Drinking)
			{
				Happiness_Coef = 5f;
			}
			if (Photo_Making)
			{
				Happiness_Coef = 5f;
			}
		}
		else
		{
			interface_script.Rox_Interface.Happiness_Color.color = new Color(1f, 1f, 0f);
		}
		Happiness = Mathf.Clamp(Happiness, 0f, 100f);
		interface_script.Rox_Interface.Happiness_Slider.value = Happiness;
	}

	private IEnumerator Change_Happiness()
	{
		while (base.gameObject.activeInHierarchy)
		{
			yield return new WaitForSeconds(0.1f);
			interface_script.Rox_Interface.Happiness_Text.text = Happiness.ToString();
			Count_And_Show_Shame_Happiness();
			if (Watchers_Count == 0)
			{
				Known_Watching = false;
			}
			Have_Known_Watchers = Known_Watching && Watchers_Count > 0;
			if (Have_Known_Watchers || Shame >= 100f || controller.currentStamina < 1f)
			{
				Good_Mood_Showed = false;
				Decrease_Happiness();
			}
			else
			{
				Bad_Mood_Showed = false;
				Increase_Happiness();
			}
			if (Delayed_Happiness_Show)
			{
				Delayed_Happiness_Show = false;
			}
			inventory.data.Current_Happiness = Happiness;
		}
	}

	public void Buy_Cloth_Happiness()
	{
		if (Happiness < 75f)
		{
			New_Happiness(25);
		}
		else
		{
			Happiness = 99f;
		}
	}

	private IEnumerator Power_Up_Couter()
	{
		while (base.gameObject.activeInHierarchy)
		{
			yield return new WaitForSeconds(1f);
			if (controller.Energized)
			{
				if (Remain_Energized_Time < 120f)
				{
					if (Remain_Energized_Time == 0f)
					{
						controller.currentStamina = controller.maxStamina;
					}
					Remain_Energized_Time += 1f;
					interface_script.Rox_Interface.Energy_PowerUp.gameObject.SetActive(value: true);
					interface_script.Rox_Interface.Energy_PowerUp.value = 120f - Remain_Energized_Time;
				}
				else
				{
					interface_script.Rox_Interface.Energy_PowerUp.gameObject.SetActive(value: false);
					controller.Energized = false;
					Remain_Energized_Time = 0f;
					interface_script.Rox_Interface.Energy_Color.color = Color.red;
				}
			}
			if (Drunk)
			{
				if (Remain_Drunk_Time < 120f)
				{
					Remain_Drunk_Time += 1f;
					interface_script.Rox_Interface.Beer_PowerUp.gameObject.SetActive(value: true);
					interface_script.Rox_Interface.Beer_PowerUp.value = 120f - Remain_Drunk_Time;
				}
				else
				{
					Drunk = false;
					interface_script.Rox_Interface.Beer_PowerUp.gameObject.SetActive(value: false);
					Remain_Drunk_Time = 0f;
					interface_script.Rox_Interface.Embarassment_Color.color = Color.red;
				}
			}
			if (Full_Of_Food)
			{
				if (Remain_Full_Food_Time < 120f)
				{
					Remain_Full_Food_Time += 1f;
					interface_script.Rox_Interface.Food_PowerUp.gameObject.SetActive(value: true);
					interface_script.Rox_Interface.Food_PowerUp.value = 120f - Remain_Full_Food_Time;
				}
				else
				{
					Full_Of_Food = false;
					interface_script.Rox_Interface.Food_PowerUp.gameObject.SetActive(value: false);
					Remain_Full_Food_Time = 0f;
				}
			}
		}
	}

	public void Start_Eating()
	{
		StartCoroutine(Eating_Process());
	}

	private IEnumerator Eating_Process()
	{
		anim.SetTrigger("Eating");
		Full_Of_Food = true;
		Eating = true;
		yield return new WaitForSeconds(3f);
		if (Happiness < 75f)
		{
			New_Happiness(25);
		}
		else
		{
			Happiness = 99f;
		}
		Eating = false;
		interface_script.Play_Power_Up_Sound();
		Fatness_Increasing();
		yield return new WaitForSeconds(1f);
		mission_Explorer.Complete_Food_Mission();
	}

	public void Start_Drinking(bool Booze)
	{
		StartCoroutine(Drinking_Process(Booze));
	}

	private IEnumerator Drinking_Process(bool Booze)
	{
		if (!Booze)
		{
			anim.SetTrigger("Drinking_Juice");
		}
		else
		{
			anim.SetTrigger("Drinking_Beer");
		}
		Current_Action = 5;
		Drinking = true;
		if (!Booze)
		{
			Energy_Drink();
		}
		else
		{
			Beer_Drunked();
		}
		yield return new WaitForSeconds(3f);
		if (Happiness < 90f)
		{
			New_Happiness(10);
		}
		else
		{
			Happiness = 99f;
		}
		Drinking = false;
		Current_Action = 0;
		if (!Booze)
		{
			Show_Infinity_Energy();
		}
		else
		{
			Show_Infinity_Embarassment();
		}
		interface_script.Play_Power_Up_Sound();
		yield return new WaitForSeconds(1f);
		mission_Explorer.Complete_Food_Mission();
	}

	public void Dont_Want_Undress()
	{
		anim.SetTrigger("Dont_Want");
		Speak(inventory.Player.Dont_Want_Unwear_Speech);
	}

	public void Failed_Night()
	{
		Fail_Hard_Mode(3);
	}

	public void Night_Win()
	{
		inventory.data.Current_Night++;
		interface_script.Rox_Interface.Current_Night_Text.text = inventory.data.Current_Night.ToString();
		interface_script.Loader.timeController.timeline = 7f;
		inventory.data.time = interface_script.Loader.timeController.timeline;
		interface_script.Rox_Interface.Arrow_Object.gameObject.SetActive(value: false);
		Map_arrow.gameObject.SetActive(value: false);
		Return_Base_Clothes();
		Restart_Player();
	}

	public void Count_Night()
	{
		if (inventory.data.Language == 0)
		{
			interface_script.Show_Night_Win_Name("НОЧЬ " + inventory.data.Current_Night + " ПРОЙДЕНА");
		}
		if (inventory.data.Language == 1)
		{
			interface_script.Show_Night_Win_Name("Night " + inventory.data.Current_Night + " Completed");
		}
		inventory.data.Current_Night++;
		interface_script.Show_Phone_Icon();
		interface_script.Rox_Interface.Current_Night_Text.text = inventory.data.Current_Night.ToString();
		interface_script.Loader.timeController.timeline = 7f;
		inventory.data.time = interface_script.Loader.timeController.timeline;
		interface_script.Rox_Interface.Arrow_Object.gameObject.SetActive(value: false);
		Map_arrow.gameObject.SetActive(value: false);
	}
}
