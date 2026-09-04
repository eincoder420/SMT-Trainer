using System;
using Invector;
using Invector.vCharacterController;
using MagicaCloth;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class Inventory_Script : MonoBehaviour
{
	[Serializable]
	public struct clothes
	{
		public string Name;

		public string[] Names;

		public int Chosen_Variant;

		public Transform Parent_Inv;

		public Transform[] Inv_Mesh;

		public Transform[] Cinema_Mesh;

		public Transform[] Mesh;

		public GameObject[] Dress_Item;

		public Wardrobe_Button[] inventory_button;

		public Wardrobe_Button[] wardrobe_button;

		public ParentConstraint[] constraints;

		public Cloth_Item[] Dropped_cloth;
	}

	[Serializable]
	public struct toys
	{
		public string Name;

		public int id;

		public bool Pussy;

		public Transform Object;

		public Transform Mesh;

		public float Min;

		public float Max;

		public float Deepness;

		public Transform Checkmark;

		public DynamicBone Bone_Control;
	}

	[Serializable]
	public struct Action
	{
		public KeyCode Key;

		public int Action_Type;

		public int Action_Value;

		public Text Action_Type_Name;

		public Text Action_Value_Name;
	}

	[Serializable]
	public struct Action_Names
	{
		public string[] Value_Name;
	}

	public Transform Main_Folder;

	public Transform[] Clothes_main;

	public Transform[] Clothes_inv;

	public Transform[] Clothes_cinema;

	public Transform Wardrobe_folder;

	public Transform Inventory_folder;

	public Game_Data data;

	public Hidden_Data hidden_data;

	public Transform Sam_Parent;

	public Transform Clothes_Unwear_Parent;

	public Transform Clothes_Wear;

	public Transform Cloth_Holder_Main;

	public clothes[] Clothes;

	public Speech Not_Bought_Speech;

	public toys[] Toys_Pussy;

	public toys[] Toys_Ass;

	public toys current_toy;

	public toys empty_toy;

	public Button[] Toys_Button;

	public Transform[] Toys_Blocks;

	public Speech[] Toys_Block_Speech;

	public Speech[] Jerk_Block_Speech;

	public Speech[] Dance_Block_Speech;

	public Speech[] Toy_Pose_Block_Speech;

	public Speech Forbid_Keep_Toy_Inside;

	public Speech Im_Not_In_Toy_Mode;

	public Action[] Actions;

	public int Current_Set_Action_Key;

	public Transform Choose_Action_Menu;

	[HideInInspector]
	public bool Inventory_Menu;

	[HideInInspector]
	public bool Actions_Menu;

	[HideInInspector]
	public bool Wearing;

	public GameObject Wardrobe_Menu;

	private vThirdPersonController Player_Control;

	[HideInInspector]
	public Roxanne_Control Player;

	private Animator animator;

	private bool Underwear_Layer;

	public Transform Layer_Cloth;

	public Transform layer_Cloth_3d;

	public MagicaBoneSpring[] Boobs;

	public MagicaMeshCloth[] Skirt;

	public bool Waiting_For_Wear;

	public int Waiting_id;

	public bool In_Wardrobe_Menu;

	public bool In_Locker_Room;

	public Wardrobe wardrobe;

	public Transform[] cloth_tip_icons;

	public Transform Toys_Menu;

	private Animator Wardrobe_Anim;

	[HideInInspector]
	public Edit_Base Editing;

	public bool Wardrobe_Wear;

	public vFootStep foot_step;

	private vThirdPersonMotor Motor;

	public string[] Type_Names;

	public Action_Names[] Value_Names;

	public string[] Type_Names_1;

	public Action_Names[] Value_Names_1;

	public Text Change_Action_Button_Text;

	public Subparam_Window Inventory_Param;

	public Subparam_Window Actions_Param;

	private Cloth_Item[] Cloth_Items;

	private Wardrobe_Button[] Wardrobe_Buttons;

	public MagicaBoneSpring[] Butt_Spring;

	private void Start()
	{
		Find_Wardrobe();
		Editing = UnityEngine.Object.FindObjectOfType<Edit_Base>();
		animator = GetComponent<Animator>();
		Motor = GetComponent<vThirdPersonMotor>();
		Check_Body_Collider_Size();
		foot_step = GetComponent<vFootStep>();
		foot_step.SpawnStepMark = data.Clothes[4].Weared;
		Check_Legs_Size();
		Player_Control = GetComponent<vThirdPersonController>();
		Player = GetComponent<Roxanne_Control>();
		Wardrobe_Menu.SetActive(value: false);
		Toys_Menu.gameObject.SetActive(value: false);
		for (int i = 0; i < Clothes.Length; i++)
		{
			Wear_Cloth_On_Model(i);
			for (int j = 0; j < data.Clothes[i].Spawned_Cloth.Length; j++)
			{
				if ((bool)Clothes[i].Dress_Item[j] && data.Clothes[i].Spawned_Cloth[j].Spawned)
				{
					UnityEngine.Object.Instantiate(Clothes[i].Dress_Item[j], data.Clothes[i].Spawned_Cloth[j].Coordinates, Quaternion.Euler(0f, 0f, 0f), null);
				}
			}
		}
		current_toy = empty_toy;
		if (data.Clothes[0].Weared)
		{
			Clean_Toys();
		}
		else
		{
			Put_Toys_Pussy();
			Put_Toys_Ass();
		}
		Check_Toys_Progress();
		if (!Player.Loader)
		{
			Player.Loader = UnityEngine.Object.FindObjectOfType<Menu_Level_Loader>();
		}
		Check_Nake_Level();
		Choose_Action_Menu.gameObject.SetActive(value: false);
		for (int k = 0; k < Actions.Length; k++)
		{
			Change_Action_Button_Name(k, Actions[k].Action_Type, Actions[k].Action_Value);
		}
		if (data.Language == 0)
		{
			Change_Action_Button_Text.text = "ВЫ МОЖЕТЕ ИЗМЕНИТЬ НАЗНАЧЕНИЕ КЛАВИШ";
		}
		if (data.Language == 1)
		{
			Change_Action_Button_Text.text = "You can change the key assignment";
		}
		Remove_Locks();
	}

	private void Find_Wardrobe()
	{
		if (!wardrobe)
		{
			wardrobe = UnityEngine.Object.FindObjectOfType<Wardrobe>();
		}
		if ((bool)wardrobe)
		{
			Wardrobe_Anim = wardrobe.GetComponent<Animator>();
		}
	}

	private void Remove_Locks()
	{
		for (int i = 1; i < Player.dances.Length; i++)
		{
			if (data.progress_data.Dance_Level >= Player.dances[i].Requied_Achieve && (bool)Player.dances[i].Lock)
			{
				Player.dances[i].Lock.gameObject.SetActive(value: false);
			}
		}
		for (int j = 1; j < Player.poses.Length; j++)
		{
			if (data.progress_data.Jerk_Level >= Player.poses[j].Requied_Achieve && (bool)Player.poses[j].Lock)
			{
				Player.poses[j].Lock.gameObject.SetActive(value: false);
			}
		}
		for (int k = 1; k < Player.toy_poses.Length; k++)
		{
			if (data.progress_data.Toy_Level >= Player.toy_poses[k].Requied_Achieve && (bool)Player.toy_poses[k].Lock)
			{
				Player.toy_poses[k].Lock.gameObject.SetActive(value: false);
			}
		}
	}

	[ContextMenu("Add_Cloth_Meshes")]
	public void Add_Cloth_Mesh()
	{
		for (int i = 0; i < Clothes.Length; i++)
		{
			Clothes[i].Mesh = new Transform[Clothes_main[i].childCount];
			Clothes[i].Inv_Mesh = new Transform[Clothes_main[i].childCount];
			Clothes[i].Cinema_Mesh = new Transform[Clothes_main[i].childCount];
			Clothes[i].Dropped_cloth = new Cloth_Item[Clothes_main[i].childCount];
			Clothes[i].wardrobe_button = new Wardrobe_Button[Clothes_main[i].childCount];
			Clothes[i].inventory_button = new Wardrobe_Button[Clothes_main[i].childCount];
			for (int j = 0; j < Clothes[i].Mesh.Length; j++)
			{
				Clothes[i].Mesh[j] = Clothes_main[i].GetChild(j);
				Clothes[i].Inv_Mesh[j] = Clothes_inv[i].GetChild(j);
				Clothes[i].Cinema_Mesh[j] = Clothes_cinema[i].GetChild(j);
			}
		}
		Wardrobe_Button[] componentsInChildren = Wardrobe_folder.GetComponentsInChildren<Wardrobe_Button>(includeInactive: true);
		Wardrobe_Button[] componentsInChildren2 = Inventory_folder.GetComponentsInChildren<Wardrobe_Button>(includeInactive: true);
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			Clothes[componentsInChildren[k].id].wardrobe_button[componentsInChildren[k].Variant] = componentsInChildren[k];
			Clothes[componentsInChildren2[k].id].inventory_button[componentsInChildren2[k].Variant] = componentsInChildren2[k];
		}
	}

	public void Transfer_Cloth_Mesh()
	{
		Clothes_Wear.parent = Sam_Parent;
		Clothes_Wear.localPosition = Vector3.zero;
		Clothes_Wear.localRotation = Quaternion.Euler(0f, 0f, 0f);
		Clothes_Wear.localScale = Vector3.one;
		for (int i = 0; i < Clothes.Length; i++)
		{
			for (int j = 0; j < Clothes[i].Inv_Mesh.Length; j++)
			{
				Clothes[i].Inv_Mesh[j].parent = Clothes[i].Parent_Inv;
				Clothes[i].Inv_Mesh[j].localPosition = Vector3.zero;
				Clothes[i].Inv_Mesh[j].localRotation = Quaternion.Euler(0f, 0f, 0f);
				Clothes[i].Inv_Mesh[j].localScale = Vector3.one;
			}
		}
	}

	public void Replace_Sam_Clothes_For_Screens()
	{
		for (int i = 0; i < Clothes.Length; i++)
		{
			Wear_Cloth_On_Model(i);
		}
	}

	public void Wear_Cloth_On_Model(int a)
	{
		for (int i = 0; i < Clothes[a].Mesh.Length; i++)
		{
			if ((bool)Clothes[a].Mesh[i].GetComponent<Renderer>())
			{
				Clothes[a].Mesh[i].GetComponent<Renderer>().enabled = data.Clothes[a].Weared && data.Clothes[a].Current_Variant == i;
				continue;
			}
			Renderer[] componentsInChildren = Clothes[a].Mesh[i].GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].enabled = data.Clothes[a].Weared && data.Clothes[a].Current_Variant == i;
			}
		}
		for (int k = 0; k < Clothes[a].Inv_Mesh.Length; k++)
		{
			Clothes[a].Inv_Mesh[k].gameObject.SetActive(data.Clothes[a].Weared && data.Clothes[a].Current_Variant == k);
		}
		for (int l = 0; l < Clothes[a].Cinema_Mesh.Length; l++)
		{
			Clothes[a].Cinema_Mesh[l].gameObject.SetActive(data.Clothes[a].Weared && data.Clothes[a].Current_Variant == l);
		}
		Set_Cloth_Buttons(a);
		Delete_Floor_Cloth(a);
	}

	public void Check_Cinema_Clothes()
	{
		for (int i = 0; i < Clothes.Length; i++)
		{
			for (int j = 0; j < Clothes[i].Cinema_Mesh.Length; j++)
			{
				Clothes[i].Cinema_Mesh[j].gameObject.SetActive(data.Clothes[i].Weared && data.Clothes[i].Current_Variant == j);
			}
		}
	}

	public void Make_Sleep_Cinema_Cloth(bool On)
	{
		if (On)
		{
			for (int i = 0; i < Clothes[0].Cinema_Mesh.Length; i++)
			{
				Clothes[0].Cinema_Mesh[i].gameObject.SetActive(value: false);
			}
			for (int j = 0; j < Clothes[2].Cinema_Mesh.Length; j++)
			{
				Clothes[2].Cinema_Mesh[j].gameObject.SetActive(value: false);
			}
			for (int k = 0; k < Clothes[3].Cinema_Mesh.Length; k++)
			{
				Clothes[3].Cinema_Mesh[k].gameObject.SetActive(value: false);
			}
			for (int l = 0; l < Clothes[4].Cinema_Mesh.Length; l++)
			{
				Clothes[4].Cinema_Mesh[l].gameObject.SetActive(value: false);
			}
			for (int m = 0; m < Clothes[6].Cinema_Mesh.Length; m++)
			{
				Clothes[6].Cinema_Mesh[m].gameObject.SetActive(value: false);
			}
		}
		else
		{
			for (int n = 0; n < Clothes[0].Cinema_Mesh.Length; n++)
			{
				Clothes[0].Cinema_Mesh[n].gameObject.SetActive(data.Clothes[0].Weared && data.Clothes[0].Current_Variant == n);
			}
			for (int num = 0; num < Clothes[2].Cinema_Mesh.Length; num++)
			{
				Clothes[2].Cinema_Mesh[num].gameObject.SetActive(data.Clothes[2].Weared && data.Clothes[2].Current_Variant == num);
			}
			for (int num2 = 0; num2 < Clothes[3].Cinema_Mesh.Length; num2++)
			{
				Clothes[3].Cinema_Mesh[num2].gameObject.SetActive(data.Clothes[3].Weared && data.Clothes[3].Current_Variant == num2);
			}
			for (int num3 = 0; num3 < Clothes[4].Cinema_Mesh.Length; num3++)
			{
				Clothes[4].Cinema_Mesh[num3].gameObject.SetActive(data.Clothes[4].Weared && data.Clothes[4].Current_Variant == num3);
			}
			for (int num4 = 0; num4 < Clothes[6].Cinema_Mesh.Length; num4++)
			{
				Clothes[6].Cinema_Mesh[num4].gameObject.SetActive(data.Clothes[6].Weared && data.Clothes[6].Current_Variant == num4);
			}
		}
	}

	public void Delete_Floor_Cloth(int a)
	{
		for (int i = 0; i < Clothes[a].Mesh.Length; i++)
		{
			if (data.Clothes[a].Weared && data.Clothes[a].Spawned_Cloth[i].Spawned && Clothes[a].Dropped_cloth[i] != null)
			{
				UnityEngine.Object.Destroy(Clothes[a].Dropped_cloth[i].gameObject);
				data.Clothes[a].Spawned_Cloth[i].Spawned = false;
			}
		}
	}

	public void Check_Toys_Progress()
	{
		for (int i = 1; i < Toys_Button.Length; i++)
		{
			bool flag = data.progress_data.Mission_Progress > 3;
			Toys_Button[i].interactable = flag;
			Toys_Blocks[i].gameObject.SetActive(!flag);
		}
	}

	public void Blocked_Toy_Speech(int speech_id)
	{
		Player.Speak(Toys_Block_Speech[speech_id]);
	}

	public void Set_Toy_Pussy(int id)
	{
		if (Player.Toy_Mode)
		{
			for (int i = 0; i < data.Toys_Pussy.Length; i++)
			{
				data.Toys_Pussy[i].Weared = id == i;
			}
			current_toy = Toys_Pussy[id];
			Put_Toys_Pussy();
		}
		else
		{
			Player.Speak(Im_Not_In_Toy_Mode);
		}
	}

	public void Set_Toy_Ass(int id)
	{
		if (Player.Toy_Mode)
		{
			for (int i = 0; i < data.Toys_Ass.Length; i++)
			{
				data.Toys_Ass[i].Weared = id == i;
			}
			current_toy = Toys_Ass[id];
			Put_Toys_Ass();
		}
		else
		{
			Player.Speak(Im_Not_In_Toy_Mode);
		}
	}

	public void Clean_Toys()
	{
		for (int i = 0; i < data.Toys_Pussy.Length; i++)
		{
			data.Toys_Pussy[i].Weared = false;
			data.Toys_Pussy[i].Deepness = 1f;
		}
		for (int j = 0; j < data.Toys_Ass.Length; j++)
		{
			data.Toys_Ass[j].Weared = false;
			data.Toys_Ass[j].Deepness = 1f;
		}
		current_toy = empty_toy;
		Put_Toys_Pussy();
		Put_Toys_Ass();
		if (Player.Fucking)
		{
			Player.Stop_Fucking();
		}
	}

	public void Put_Toys_Pussy()
	{
		for (int i = 0; i < data.Toys_Pussy.Length; i++)
		{
			if (data.Toys_Pussy[i].Weared)
			{
				current_toy = Toys_Pussy[i];
			}
			else
			{
				data.Toys_Pussy[i].Deepness = 1f;
			}
			Toys_Pussy[i].Object.gameObject.SetActive(data.Toys_Pussy[i].Weared);
			float z = Mathf.Lerp(Toys_Pussy[i].Min, Toys_Pussy[i].Max, data.Toys_Pussy[i].Deepness);
			Toys_Pussy[i].Mesh.transform.localPosition = new Vector3(0f, 0f, z);
			Toys_Pussy[i].Deepness = data.Toys_Pussy[i].Deepness;
		}
		Check_Checkmarks();
	}

	public void Put_Toys_Ass()
	{
		for (int i = 0; i < data.Toys_Ass.Length; i++)
		{
			if (data.Toys_Ass[i].Weared)
			{
				current_toy = Toys_Ass[i];
			}
			else
			{
				data.Toys_Ass[i].Deepness = 1f;
			}
			Toys_Ass[i].Object.gameObject.SetActive(data.Toys_Ass[i].Weared);
			float z = Mathf.Lerp(Toys_Ass[i].Min, Toys_Ass[i].Max, data.Toys_Ass[i].Deepness);
			Toys_Ass[i].Mesh.transform.localPosition = new Vector3(0f, 0f, z);
			Toys_Ass[i].Deepness = data.Toys_Ass[i].Deepness;
		}
		Check_Checkmarks();
	}

	public void Check_Checkmarks()
	{
		for (int i = 0; i < 3; i++)
		{
			Toys_Pussy[i].Checkmark.gameObject.SetActive(data.Toys_Pussy[i].Weared && Toys_Pussy[i].Deepness < 0.5f);
			Toys_Ass[i].Checkmark.gameObject.SetActive(data.Toys_Ass[i].Weared && Toys_Ass[i].Deepness < 0.5f);
		}
	}

	public void Check_For_Toy_Inside()
	{
		data.Have_Toy_Inside = false;
		for (int i = 0; i < 3; i++)
		{
			if (data.Toys_Pussy[i].Weared)
			{
				if (Toys_Pussy[i].Deepness < 0.5f)
				{
					data.Have_Toy_Inside = true;
				}
				else
				{
					data.Toys_Pussy[i].Weared = false;
				}
			}
			if (data.Toys_Ass[i].Weared)
			{
				if (Toys_Ass[i].Deepness < 0.5f)
				{
					data.Have_Toy_Inside = true;
				}
				else
				{
					data.Toys_Ass[i].Weared = false;
				}
			}
		}
		Player.mission_Explorer.Complete_Toy_Mission(data.Have_Toy_Inside);
	}

	public void Save_Deepness()
	{
		for (int i = 0; i < 3; i++)
		{
			if (current_toy.Mesh == Toys_Pussy[i].Mesh)
			{
				Toys_Pussy[i].Deepness = current_toy.Deepness;
			}
			if (current_toy.Mesh == Toys_Ass[i].Mesh)
			{
				Toys_Ass[i].Deepness = current_toy.Deepness;
			}
			data.Toys_Pussy[i].Deepness = Toys_Pussy[i].Deepness;
			data.Toys_Ass[i].Deepness = Toys_Ass[i].Deepness;
		}
	}

	public void Check_Inventory_Open()
	{
		Inventory_Menu = Inventory_Param.params_open;
		Player.interface_script.Turn_Cursor();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.I))
		{
			Inventory_Param.Switch_Subparam();
			Check_Inventory_Open();
		}
		if (Input.GetKeyDown(KeyCode.U))
		{
			Actions_Param.Switch_Subparam();
			Actions_Menu = Actions_Param.params_open;
			Player.interface_script.Turn_Cursor();
		}
	}

	private void Change_Action_Button_Name(int id, int Type, int Value)
	{
		if (data.Language == 0)
		{
			Actions[id].Action_Type_Name.text = Type_Names[Type];
			Actions[id].Action_Value_Name.text = Value_Names[Type].Value_Name[Value];
		}
		if (data.Language == 1)
		{
			Actions[id].Action_Type_Name.text = Type_Names_1[Type];
			Actions[id].Action_Value_Name.text = Value_Names_1[Type].Value_Name[Value];
		}
	}

	public void Set_Action_Button(int id)
	{
		Current_Set_Action_Key = id;
		Choose_Action_Menu.gameObject.SetActive(value: true);
		Player.Turn_Action_Indicators(Actions[Current_Set_Action_Key].Action_Type, Actions[Current_Set_Action_Key].Action_Value);
		int num = id + 1;
		if (data.Language == 0)
		{
			Change_Action_Button_Text.text = "ВЫБЕРИТЕ ДЕЙСТВИЕ ДЛЯ КЛАВИШИ " + num;
		}
		if (data.Language == 1)
		{
			Change_Action_Button_Text.text = "Select a new action for the key button " + num;
		}
	}

	public void Choose_Masturbation_Action(int id)
	{
		Actions[Current_Set_Action_Key].Action_Type = 1;
		Actions[Current_Set_Action_Key].Action_Value = id;
		Player.Turn_Action_Indicators(Actions[Current_Set_Action_Key].Action_Type, Actions[Current_Set_Action_Key].Action_Value);
		Change_Action_Button_Name(Current_Set_Action_Key, 1, id);
		if (data.Language == 0)
		{
			Change_Action_Button_Text.text = "ВЫ МОЖЕТЕ ИЗМЕНИТЬ НАЗНАЧЕНИЕ КЛАВИШ";
		}
		if (data.Language == 1)
		{
			Change_Action_Button_Text.text = "You can change the key assignment";
		}
		if (data.progress_data.Jerk_Level >= Player.poses[id].Requied_Achieve)
		{
			int num = Player.Actions_UI.Poses_List[id];
			if (num == 2)
			{
				Player.Speak(Jerk_Block_Speech[1]);
			}
			if (num == 3)
			{
				Player.Speak(Jerk_Block_Speech[2]);
			}
			if (num == 7)
			{
				Player.Speak(Jerk_Block_Speech[3]);
			}
			if (num == 8)
			{
				Player.Speak(Jerk_Block_Speech[4]);
			}
			if (num == 5)
			{
				Player.Speak(Jerk_Block_Speech[5]);
			}
			Player.anim.SetTrigger("Cant");
		}
	}

	public void Choose_Dance_Action(int id)
	{
		Actions[Current_Set_Action_Key].Action_Type = 2;
		Actions[Current_Set_Action_Key].Action_Value = id;
		Player.Turn_Action_Indicators(Actions[Current_Set_Action_Key].Action_Type, Actions[Current_Set_Action_Key].Action_Value);
		Change_Action_Button_Name(Current_Set_Action_Key, 2, id);
		if (data.Language == 0)
		{
			Change_Action_Button_Text.text = "ВЫ МОЖЕТЕ ИЗМЕНИТЬ НАЗНАЧЕНИЕ КЛАВИШ";
		}
		if (data.Language == 1)
		{
			Change_Action_Button_Text.text = "You can change the key assignment";
		}
		if (data.progress_data.Dance_Level >= Player.dances[id].Requied_Achieve)
		{
			Player.Speak(Dance_Block_Speech[id]);
			Player.anim.SetTrigger("Cant");
		}
	}

	public void Choose_Toy_Action(int id)
	{
		Actions[Current_Set_Action_Key].Action_Type = 3;
		Actions[Current_Set_Action_Key].Action_Value = id;
		Player.Turn_Action_Indicators(Actions[Current_Set_Action_Key].Action_Type, Actions[Current_Set_Action_Key].Action_Value);
		Change_Action_Button_Name(Current_Set_Action_Key, 3, id);
		if (data.Language == 0)
		{
			Change_Action_Button_Text.text = "ВЫ МОЖЕТЕ ИЗМЕНИТЬ НАЗНАЧЕНИЕ КЛАВИШ";
		}
		if (data.Language == 1)
		{
			Change_Action_Button_Text.text = "You can change the key assignment";
		}
		if (data.progress_data.Toy_Level >= Player.toy_poses[id].Requied_Achieve)
		{
			Player.Speak(Toy_Pose_Block_Speech[id]);
			Player.anim.SetTrigger("Cant");
		}
	}

	public void Switch_Cloth_Layer(bool Underwear)
	{
		Underwear_Layer = Underwear;
		Layer_Cloth.gameObject.SetActive(!Underwear_Layer);
		layer_Cloth_3d.gameObject.SetActive(!Underwear_Layer);
	}

	public void Turn_Use_Button(bool On)
	{
		Player.interface_script.Rox_Interface.Button_Open.SetActive(On);
	}

	public void Wardrobe(bool inside)
	{
		Find_Wardrobe();
		In_Wardrobe_Menu = inside;
		Player.interface_script.Turn_Cursor();
		if (In_Wardrobe_Menu)
		{
			Wardrobe_Anim.SetTrigger("Open");
			Wardrobe_Menu.SetActive(value: true);
		}
		else
		{
			Wardrobe_Anim.SetTrigger("Close");
			Wardrobe_Menu.SetActive(value: false);
			Turn_Use_Button(On: false);
		}
	}

	public void Replace_Panties(bool Separate)
	{
	}

	public void Replace_Shirt()
	{
		_ = data.Clothes[3].Weared;
	}

	public void Turn_Cloth_Items(bool On)
	{
		Cloth_Items = UnityEngine.Object.FindObjectsOfType<Cloth_Item>();
		Wardrobe_Buttons = UnityEngine.Object.FindObjectsOfType<Wardrobe_Button>();
		if (Cloth_Items.Length != 0)
		{
			for (int i = 0; i < Cloth_Items.Length; i++)
			{
				Cloth_Items[i].GetComponent<BoxCollider>().enabled = On;
			}
		}
		if (Wardrobe_Buttons.Length == 0)
		{
			return;
		}
		for (int j = 0; j < Wardrobe_Buttons.Length; j++)
		{
			if ((bool)Wardrobe_Buttons[j].GetComponent<Button>())
			{
				Wardrobe_Buttons[j].GetComponent<Button>().enabled = On;
			}
		}
	}

	public void Wear(int id)
	{
		if (!Wearing && !Player.Anim_UpDress_Process)
		{
			if (Player.Covered)
			{
				Player.Stop_Covering();
			}
			if (Player.Showing)
			{
				if (!Waiting_For_Wear)
				{
					Waiting_id = id;
					Waiting_For_Wear = true;
				}
				Player.Check_Cloth_For_Showing(Player.Current_Cloth);
			}
			else
			{
				Wear_Animating(id);
			}
		}
		Turn_Cloth_Items(On: false);
	}

	public void Wear_Animating(int id)
	{
		if (!Wearing && !Player.Anim_UpDress_Process)
		{
			if (data.Clothes[id].Weared && Clothes[id].Chosen_Variant != data.Clothes[id].Current_Variant)
			{
				data.Clothes[id].Weared = true;
			}
			else
			{
				data.Clothes[id].Weared = !data.Clothes[id].Weared;
			}
			data.Clothes[id].Current_Variant = Clothes[id].Chosen_Variant;
			if (data.Clothes[id].Weared && Player.Masturbating)
			{
				for (int i = 0; i < Player.poses[Player.Current_Mastrurbation_Pose].Requied_Unweared_Cloth.Length; i++)
				{
					if (data.Clothes[Player.poses[Player.Current_Mastrurbation_Pose].Requied_Unweared_Cloth[i]].Weared)
					{
						Player.Stop_Masturbating();
					}
				}
			}
			Show_Hide_Inventory_Mesh(id);
			if (!data.Clothes[id].Weared)
			{
				animator.SetTrigger("Undress");
			}
			else if (Wardrobe_Wear)
			{
				animator.SetTrigger("Dress");
			}
			animator.SetInteger("Cloth_Id", id);
			animator.SetInteger("Cloth_Variant", data.Clothes[id].Current_Variant);
			Check_Nake_Level();
		}
		Player.Set_Masturbation_Buttons();
		foot_step.SpawnStepMark = data.Clothes[4].Weared;
		Check_Legs_Size();
		Check_Body_Collider_Size();
		Check_Cinema_Clothes();
		Editing.Show_Hide_Piercing();
		Player.mission_Explorer.Complete_Cloth_Mission();
	}

	public void Check_Legs_Size()
	{
		bool flag = data.Clothes[4].Weared || data.Clothes[8].Weared;
		int num;
		if (data.Clothes[4].Weared)
		{
			num = ((data.Clothes[4].Current_Variant >= 2) ? 1 : 0);
			if (num != 0)
			{
				goto IL_0091;
			}
		}
		else
		{
			num = 0;
		}
		if (!flag)
		{
			animator.SetBool("In_Boots", value: false);
			animator.SetBool("In_Heels", value: false);
		}
		goto IL_0091;
		IL_0091:
		if (num == 0 && flag)
		{
			animator.SetBool("In_Boots", value: true);
			animator.SetBool("In_Heels", value: false);
		}
		if (((uint)num & (flag ? 1u : 0u)) != 0)
		{
			animator.SetBool("In_Boots", value: false);
			animator.SetBool("In_Heels", value: true);
		}
		bool value = data.Clothes[1].Weared || data.Clothes[3].Weared || data.Clothes[6].Weared;
		animator.SetBool("In_Bra", value);
		if (data.Clothes[2].Weared || data.Clothes[6].Weared)
		{
			Butt_Spring[0].BlendWeight = 0f;
			Butt_Spring[1].BlendWeight = 0f;
		}
	}

	public void Show_Hide_Inventory_Mesh(int id)
	{
		for (int i = 0; i < Clothes[id].Inv_Mesh.Length; i++)
		{
			Clothes[id].Inv_Mesh[i].gameObject.SetActive(data.Clothes[id].Weared && data.Clothes[id].Current_Variant == i);
		}
	}

	public void Check_Body_Collider_Size()
	{
		if (!data.Clothes[4].Weared)
		{
			Motor.colliderHeight = 1.68f;
			Motor.colliderCenter = new Vector3(0f, 0.84f, 0f);
		}
		else if (data.Clothes[4].Current_Variant >= 2)
		{
			Motor.colliderHeight = 1.7f;
			Motor.colliderCenter = new Vector3(0f, 0.8f, 0f);
		}
		else
		{
			Motor.colliderHeight = 1.8f;
			Motor.colliderCenter = new Vector3(0f, 0.9f, 0f);
		}
	}

	public void Check_Nake_Level()
	{
		if (!data.Clothes[6].Weared)
		{
			if (!data.Clothes[2].Weared)
			{
				if (data.Clothes[0].Weared)
				{
					if (In_Locker_Room)
					{
						Player.Nake_Level = 0;
					}
					else if (data.saved_data.Inside_Building != 30)
					{
						Player.Nake_Level = 2;
					}
					else
					{
						Player.Nake_Level = 0;
					}
				}
				else
				{
					if (!In_Locker_Room)
					{
						Player.Nake_Level = 4;
					}
					else
					{
						Player.Nake_Level = 0;
					}
					Editing.Get_Ass_Settings();
				}
			}
			else if (!data.Clothes[3].Weared)
			{
				if (data.Clothes[1].Weared)
				{
					if (In_Locker_Room)
					{
						Player.Nake_Level = 0;
					}
					else if (data.saved_data.Inside_Building != 30)
					{
						Player.Nake_Level = 2;
					}
					else
					{
						Player.Nake_Level = 0;
					}
				}
				else
				{
					if (!In_Locker_Room)
					{
						Player.Nake_Level = 4;
					}
					else
					{
						Player.Nake_Level = 0;
					}
					Editing.Get_Boobs_Settings();
				}
			}
			else
			{
				Player.Nake_Level = 0;
			}
		}
		else
		{
			Player.Nake_Level = 0;
			Editing.Get_Boobs_Settings();
			Editing.Get_Ass_Settings();
		}
	}

	public void Spawn_Cloth_Item(int id)
	{
		for (int i = 0; i < Clothes[id].Mesh.Length; i++)
		{
			if ((bool)Clothes[id].Mesh[i].GetComponent<Renderer>())
			{
				Clothes[id].Mesh[i].GetComponent<Renderer>().enabled = false;
			}
		}
		Set_Cloth_Buttons(id);
		if (!data.Clothes[id].Weared && !data.Clothes[id].Spawned_Cloth[data.Clothes[id].Current_Variant].Spawned)
		{
			Instantiate_Cloth_Item(id);
		}
		Check_Legs_Size();
	}

	public void Instantiate_Cloth_Item(int id)
	{
		UnityEngine.Object.Instantiate(Clothes[id].Dress_Item[data.Clothes[id].Current_Variant], Player.transform.position, Player.transform.rotation, null);
		data.Clothes[id].Spawned_Cloth[data.Clothes[id].Current_Variant].Coordinates = Player.transform.position;
		data.Clothes[id].Spawned_Cloth[data.Clothes[id].Current_Variant].Spawned = true;
	}

	public void Replace_Weared_Cloth(int id, int Variant)
	{
		data.Clothes[id].Spawned_Cloth[Variant].Spawned = false;
		for (int i = 0; i < Clothes[id].inventory_button.Length; i++)
		{
			if (i != Variant && !data.Clothes[id].Spawned_Cloth[i].Spawned && data.Clothes[id].Spawned_Cloth[i].Bought)
			{
				Clothes[id].wardrobe_button[i].Reactivate_Button();
			}
		}
	}

	public void Start_Undressing(int id)
	{
		for (int i = 0; i < Clothes[id].Mesh.Length; i++)
		{
			if ((bool)Clothes[id].Mesh[i].GetComponent<Renderer>())
			{
				Clothes[id].Mesh[i].GetComponent<Renderer>().enabled = data.Clothes[id].Current_Variant == i;
				continue;
			}
			Renderer[] componentsInChildren = Clothes[id].Mesh[i].GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].enabled = data.Clothes[id].Weared && data.Clothes[id].Current_Variant == i;
			}
		}
	}

	public void End_Undressing(int id)
	{
		for (int i = 0; i < Clothes[id].Mesh.Length; i++)
		{
			if ((bool)Clothes[id].Mesh[i].GetComponent<Renderer>())
			{
				Clothes[id].Mesh[i].GetComponent<Renderer>().enabled = data.Clothes[id].Current_Variant == i;
				continue;
			}
			Renderer[] componentsInChildren = Clothes[id].Mesh[i].GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].enabled = data.Clothes[id].Weared && data.Clothes[id].Current_Variant == i;
			}
		}
		Set_Cloth_Buttons(id);
		data.Clothes[id].Spawned_Cloth[data.Clothes[id].Current_Variant].Spawned = false;
	}

	private void Set_Cloth_Buttons(int id)
	{
		for (int i = 0; i < Clothes[id].inventory_button.Length; i++)
		{
			Clothes[id].inventory_button[i].gameObject.SetActive(data.Clothes[id].Weared && data.Clothes[id].Current_Variant == i);
			Clothes[id].inventory_button[i].GetComponent<Button>().enabled = true;
		}
	}
}
