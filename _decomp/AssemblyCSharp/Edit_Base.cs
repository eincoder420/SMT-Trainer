using System;
using System.Collections;
using RootMotion.FinalIK;
using SFB;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Edit_Base : MonoBehaviour
{
	[Serializable]
	public struct Cloth_Bone
	{
		public Transform[] source_bone;

		public Transform[] parent_bone;
	}

	[Serializable]
	public struct Body_Bones
	{
		public Transform Butt_L;

		public Transform Butt_R;

		public Transform Boob_L;

		public Transform Boob_R;

		public Transform Body;
	}

	[Serializable]
	public struct Proportions
	{
		public Transform Body_Fat;

		public Transform Body_Slim;

		public Transform Butt_L_Min;

		public Transform Butt_L_Max;

		public Transform Butt_R_Min;

		public Transform Butt_R_Max;

		public Transform Boob_L_Min;

		public Transform Boob_L_Max;

		public Transform Boob_R_Min;

		public Transform Boob_R_Max;
	}

	[Serializable]
	public struct Hair_Collider
	{
		public Transform Hair_Col;

		public Transform Parent;
	}

	[Serializable]
	public struct UI
	{
		public Text pussy_text_ui;

		public Text piercing_text_ui;

		public Text hairstyle_text_ui;

		public Text skincolor_text_ui;

		public Slider Boobs_Slider;

		public Slider Ass_Slider;

		public Slider Fat_Slider;

		public Slider Eye_Slider;

		public RawImage Skirt_Image;

		public RawImage Shirt_Image;

		public RawImage Panties_Image;

		public RawImage Bra_Image;

		public RawImage Tatoo_Image;

		public ColorPreview Hair_Color;

		public ColorPreview Eye_Color;

		public ColorPreview Pussy_Color;

		public ColorPreview Finger_Color;

		public ColorPreview Lips_Color;

		public ColorPreview Eyeshadows_Color;

		public ColorPreview Skirt_Color;

		public ColorPreview Shirt_Color;

		public ColorPreview Shirt_Color2;

		public ColorPreview Panties_Color;

		public ColorPreview Bra_Color;

		public ColorPreview Stockings_Color;

		public Slider Panties_x;

		public Slider Panties_y;

		public Slider Bra_x;

		public Slider Bra_y;

		public Slider Skirt_x;

		public Slider Skirt_y;

		public Slider[] Wall_Slider_x;

		public Slider[] Wall_Slider_y;

		public Transform Shadow_Eye_Enable;

		public Transform Shadow_Eye_Disable;

		public RawImage[] Wall_Image;

		public RawImage[] Poster_Image;

		public Transform Hairstyle_Buttons;

		public Transform Pubic_Buttons;

		public Transform Skin_Buttons;

		public Transform Piercing_Pussy_buttons;
	}

	[Serializable]
	public struct Inventory_Model
	{
		public Transform pussy_hairs;

		public Transform hairstyles;

		public Transform piercings;

		public SkinnedMeshRenderer body;
	}

	[Serializable]
	public struct Body_Custom
	{
		public Transform pussy_hairs;

		public Transform hairstyles;

		public Transform piercings;

		public Transform parent;

		public Transform Orig;
	}

	[Serializable]
	public struct Print
	{
		public Texture2D Start_Image;

		public bool Tatoo_Mode;

		public Slider Tatoo_O_x;

		public Slider Tatoo_O_y;

		public Tatoo_Object Chosen_Tatoo;

		public Transform Tatoo_Tip;

		public Transform[] Tatoo_hidden;

		public Transform[] Tatoo_shown;

		public Id_Object[][] tatoo_ids;

		public Transform[] Chosen;

		public Transform Not_Chosen;

		public Material Base_Mat;

		public Material[] tatoo_mats;

		public Text Place_Name;

		public Transform Position_Param;
	}

	[Serializable]
	public struct Skin
	{
		public Material Head;

		public Material Body;
	}

	public float horizontalSpeed = 2f;

	public Transform Rox;

	public Inventory_Script inventory;

	public AimConstraint Cam_Constraint;

	public AudioSource audio;

	public AudioClip subparam_open_sound;

	public Game_Data data;

	public Start_Menu menu;

	public GameObject Button_Pussy_Active;

	public GameObject Button_Pussy_Blocked;

	public Transform pussy_hairs;

	public Transform piercings;

	public Transform hairstyles;

	public Transform pussy_hairs_Menu;

	public Transform piercings_Menu;

	public Transform hairstyles_Menu;

	public Transform Cloth_Mesh;

	public Transform Cloth_Mesh_Parent;

	public Body_Custom[] Custom_Model;

	public Skin[] skin_materials;

	public Material[] hair_material;

	public Material[] inventory_hair_material;

	public Material scalp_material;

	public Material pussy_hair_material;

	public Material eye_material;

	public Material fingernail_material;

	public Material lips_material;

	public Material eyeshadow_material;

	public SkinnedMeshRenderer body;

	public SkinnedMeshRenderer body_Menu;

	public SkinnedMeshRenderer head;

	public SkinnedMeshRenderer head_Menu;

	public SkinnedMeshRenderer eyes;

	public SkinnedMeshRenderer eyes_Menu;

	public CapsuleCollider Ass_Col_L;

	public CapsuleCollider Ass_Col_R;

	public Cloth_Bone cloth_bones;

	public Cloth_Bone cloth_second_bones;

	public Cloth_Bone cloth_cinematic_bones;

	public UI UI_Elements;

	public Inventory_Model Inv_Model;

	public Print print_params;

	public Body_Bones body_bones;

	public Body_Bones menu_bones;

	public Proportions proportions;

	public Subparam_Window[] Sub_Params;

	public Material[] Rox_Materials;

	public Material[] Sam_Body_Materials;

	public Material[] Sam_Head_Materials;

	public Material[] Sam_Eye_Materials;

	public ColorPicker Color_Picker;

	public Transform Picker_Closer;

	private float x;

	private float y;

	private float z;

	private float Angle_x;

	public float new_y;

	public float new_x;

	public float Body_y;

	public float Pussy_y;

	public float Face_y;

	public float Boobs_y;

	public Vector3 Real_Scale;

	public bool Rotate_Back;

	public float Start_Aim_Angle;

	private PauseMenuScript interface_script;

	public LookAtIK Look_Ik;

	public Transform House_Edit_Window;

	public Transform Character_Edit_Window;

	public RawImage Phone_Image;

	public Transform Fixed_Rox_Pos;

	public Transform Colliders_Folder;

	public Transform Hair_Bones_Folder;

	public Cloth_Bone Customization_bones;

	public Cloth_Bone Customization_bones_menu;

	public Hair_Collider[] main_hair_colliders;

	public Hair_Collider[] menu_hair_colliders;

	public Transform[] Hair_Bones;

	public Transform Haircuts_edit_folder;

	private void Awake()
	{
		if (!menu.Menu)
		{
			inventory = Rox.GetComponent<Inventory_Script>();
			Set_Parent_Start[] componentsInChildren = inventory.Cloth_Holder_Main.GetComponentsInChildren<Set_Parent_Start>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Set_Unwear_Cloth_Parent();
			}
			inventory.Transfer_Cloth_Mesh();
			Set_Cloth_Parents();
			Rox.GetComponent<Animator>().Rebind();
		}
	}

	public void Play_Subparam_Sound()
	{
		audio.PlayOneShot(subparam_open_sound);
	}

	private void Start()
	{
		if (!menu)
		{
			menu = UnityEngine.Object.FindObjectOfType<Start_Menu>();
		}
		if (!menu.Menu)
		{
			interface_script = UnityEngine.Object.FindObjectOfType<PauseMenuScript>();
		}
		if (menu.Menu)
		{
			UI_Elements.pussy_text_ui.text = data.Character.pussy_hairs.ToString();
			UI_Elements.piercing_text_ui.text = data.Character.piercing.ToString();
			UI_Elements.hairstyle_text_ui.text = data.Character.hairstyle.ToString();
			UI_Elements.Ass_Slider.value = data.Character.ass_size;
			UI_Elements.Boobs_Slider.value = data.Character.boobs_size;
			UI_Elements.Fat_Slider.value = data.Character.fatness;
			UI_Elements.Eye_Slider.value = data.Character.eye_size;
		}
		Set_Tatoo_id();
		Get_Character_Settings();
		Show_Hide_Piercing();
		if (menu.Menu)
		{
			Show_Color_Picker(on: false);
			Get_Tatoo_Mode();
			Set_Cloth_Parents();
			menu.Rox_Animator.Rebind();
			menu.Edit_Rox_Animator.Rebind();
			Check_All_Buttons();
			Check_Edit_Haircuts();
		}
		else
		{
			Set_Cinematic_Cloth();
			menu.Cinematic_Animator.Rebind();
		}
		Set_Zero_Value();
		Set_Hairs_Collider_Parent();
		Set_Customization_Bones();
		if (!menu.Menu)
		{
			Transfer_Custom_Mesh();
		}
	}

	public void Check_Edit_Haircuts()
	{
		for (int i = 0; i < Haircuts_edit_folder.childCount; i++)
		{
			Haircuts_edit_folder.GetChild(i).GetComponent<Button>().interactable = data.items.Haircuts_Bought[i] && data.items.Remain_Tools[6] > 0;
		}
	}

	public void Transfer_Custom_Mesh()
	{
		for (int i = 0; i < Custom_Model.Length; i++)
		{
			Custom_Model[i].pussy_hairs.transform.parent = Custom_Model[i].parent;
			Custom_Model[i].piercings.transform.parent = Custom_Model[i].parent;
			Custom_Model[i].hairstyles.transform.parent = Custom_Model[i].parent;
			UnityEngine.Object.Destroy(Custom_Model[i].Orig.gameObject);
		}
	}

	public void Set_Customization_Bones()
	{
		for (int i = 0; i < Customization_bones.source_bone.Length; i++)
		{
			SetClothParent(Customization_bones.source_bone[i], Customization_bones.parent_bone[i]);
			Customization_bones.source_bone[i].localPosition = Vector3.zero;
			Customization_bones.source_bone[i].localRotation = Quaternion.Euler(0f, 0f, 0f);
			Customization_bones.source_bone[i].localScale = new Vector3(1f, 1f, 1f);
		}
		for (int j = 0; j < Customization_bones_menu.source_bone.Length; j++)
		{
			SetClothParent(Customization_bones_menu.source_bone[j], Customization_bones_menu.parent_bone[j]);
			Customization_bones_menu.source_bone[j].localPosition = Vector3.zero;
			Customization_bones_menu.source_bone[j].localRotation = Quaternion.Euler(0f, 0f, 0f);
			Customization_bones_menu.source_bone[j].localScale = new Vector3(1f, 1f, 1f);
		}
	}

	private void Set_Hairs_Collider_Parent()
	{
		for (int i = 0; i < main_hair_colliders.Length; i++)
		{
			main_hair_colliders[i].Hair_Col.parent = main_hair_colliders[i].Parent;
			menu_hair_colliders[i].Hair_Col.parent = menu_hair_colliders[i].Parent;
			main_hair_colliders[i].Hair_Col.transform.localPosition = Vector3.zero;
			main_hair_colliders[i].Hair_Col.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			main_hair_colliders[i].Hair_Col.transform.localScale = Vector3.one;
			menu_hair_colliders[i].Hair_Col.transform.localPosition = Vector3.zero;
			menu_hair_colliders[i].Hair_Col.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			menu_hair_colliders[i].Hair_Col.transform.localScale = Vector3.one;
		}
	}

	[ContextMenu("Connect_Hair_Colliders")]
	public void Connect_Hair_Colliders()
	{
		DynamicBone[] componentsInChildren = Hair_Bones_Folder.GetComponentsInChildren<DynamicBone>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].m_Colliders.Clear();
			for (int j = 0; j < Colliders_Folder.childCount; j++)
			{
				componentsInChildren[i].m_Colliders.Add(Colliders_Folder.GetChild(j).GetComponent<DynamicBoneCollider>());
			}
		}
	}

	public void Set_Cinematic_Cloth()
	{
		for (int i = 0; i < cloth_cinematic_bones.source_bone.Length; i++)
		{
			if (cloth_cinematic_bones.source_bone[i] != null && cloth_cinematic_bones.parent_bone[i] != null)
			{
				Set_Cinematic_Parent(cloth_cinematic_bones.source_bone[i], cloth_cinematic_bones.parent_bone[i]);
			}
		}
	}

	public void Set_Cloth_Parents()
	{
		for (int i = 0; i < cloth_bones.source_bone.Length; i++)
		{
			SetClothParent(cloth_bones.source_bone[i], cloth_bones.parent_bone[i]);
		}
		for (int j = 0; j < cloth_second_bones.source_bone.Length; j++)
		{
			SetClothParent(cloth_second_bones.source_bone[j], cloth_second_bones.parent_bone[j]);
		}
		if (menu.Menu)
		{
			Cloth_Mesh.parent = Cloth_Mesh_Parent;
			Cloth_Mesh.localPosition = Vector3.zero;
			Cloth_Mesh.localRotation = Quaternion.Euler(0f, 0f, 0f);
			Cloth_Mesh.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	private void Set_Cinematic_Parent(Transform source_bone, Transform parent_bone)
	{
		source_bone.parent = parent_bone;
		source_bone.localPosition = Vector3.zero;
		source_bone.localRotation = Quaternion.Euler(0f, 0f, 0f);
		source_bone.localScale = new Vector3(1f, 1f, 1f);
	}

	private void SetClothParent(Transform source_bone, Transform parent_bone)
	{
		source_bone.parent = parent_bone;
		if (!menu.Menu)
		{
			string text = source_bone.name;
			source_bone.name = text + " cloth";
		}
	}

	private void Set_Zero_Value()
	{
		for (int i = 0; i < cloth_bones.source_bone.Length; i++)
		{
			cloth_bones.source_bone[i].localPosition = Vector3.zero;
			cloth_bones.source_bone[i].localRotation = Quaternion.Euler(0f, 0f, 0f);
			cloth_bones.source_bone[i].localScale = new Vector3(1f, 1f, 1f);
		}
		for (int j = 0; j < cloth_second_bones.source_bone.Length; j++)
		{
			cloth_second_bones.source_bone[j].localPosition = Vector3.zero;
			cloth_second_bones.source_bone[j].localRotation = Quaternion.Euler(0f, 0f, 0f);
			cloth_second_bones.source_bone[j].localScale = new Vector3(1f, 1f, 1f);
		}
	}

	public void Show_Color_Picker(bool on)
	{
		Color_Picker.gameObject.SetActive(on);
		Picker_Closer.gameObject.SetActive(on);
	}

	public void Open_Wall_Browser(int id)
	{
		Open_Wall_FileBrowser(id, House: true, UI_Elements.Wall_Image[id]);
	}

	public void Open_Poster_Browser(int id)
	{
		Open_Wall_FileBrowser(id, House: false, UI_Elements.Poster_Image[id]);
	}

	private void Update()
	{
		if (!menu.Menu)
		{
			return;
		}
		if (menu.In_Edit_Menu)
		{
			if (menu.Edit_Rox_Animator.GetBool("Aim"))
			{
				Look_Ik.solver.IKPositionWeight = Mathf.Lerp(Look_Ik.solver.IKPositionWeight, 1f, Time.deltaTime * 2f);
			}
			else
			{
				Look_Ik.solver.IKPositionWeight = Mathf.Lerp(Look_Ik.solver.IKPositionWeight, 0f, Time.deltaTime * 2f);
			}
			if (print_params.Tatoo_Mode)
			{
				if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo))
				{
					print_params.Tatoo_Tip.gameObject.SetActive(hitInfo.collider.tag == "Tatoo");
					if (hitInfo.collider.tag == "Tatoo" && Input.GetMouseButtonDown(0))
					{
						print_params.Chosen_Tatoo = hitInfo.collider.GetComponent<Tatoo_Object>();
						Id_Object component = print_params.Chosen_Tatoo.Tatoo.GetComponent<Id_Object>();
						print_params.Place_Name.text = component.Part_Name[data.Language];
						if (data.Tatoo[component.id].Path_To_File_Tatoo.Length > 0)
						{
							print_params.Position_Param.gameObject.SetActive(value: true);
							UI_Elements.Tatoo_Image.texture = print_params.Chosen_Tatoo.Tatoo.material.mainTexture;
							print_params.Chosen_Tatoo.Tatoo.material.mainTexture.wrapMode = TextureWrapMode.Clamp;
							print_params.Tatoo_O_x.value = print_params.Chosen_Tatoo.Tatoo.material.mainTextureOffset.x;
							print_params.Tatoo_O_y.value = print_params.Chosen_Tatoo.Tatoo.material.mainTextureOffset.y;
						}
						else
						{
							print_params.Position_Param.gameObject.SetActive(value: false);
							UI_Elements.Tatoo_Image.texture = print_params.Start_Image;
							print_params.Tatoo_O_x.value = 0f;
							print_params.Tatoo_O_y.value = 0f;
						}
						for (int i = 0; i < print_params.Chosen.Length; i++)
						{
							print_params.Chosen[i].gameObject.SetActive(value: true);
						}
						print_params.Not_Chosen.gameObject.SetActive(value: false);
					}
				}
			}
			else if (print_params.Tatoo_Tip.gameObject.activeInHierarchy)
			{
				print_params.Tatoo_Tip.gameObject.SetActive(value: false);
			}
			if (!menu.Switching_Menu_Process)
			{
				if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
				{
					new_y += Input.GetAxis("Mouse Y") * Time.deltaTime * 2f;
					new_y = Mathf.Clamp(new_y, -0.75f, 0.9f);
					new_x += Input.GetAxis("Mouse X") * Time.deltaTime * 2f;
					new_x = Mathf.Clamp(new_x, -0.75f, 0.75f);
				}
				if (Input.GetMouseButton(1) && !EventSystem.current.IsPointerOverGameObject())
				{
					Angle_x -= Input.GetAxis("Mouse Y") * Time.deltaTime * 30f;
					Angle_x = Mathf.Clamp(Angle_x, -110f, 90f);
				}
				z += Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 10f;
				z = Mathf.Clamp(z, 0f, 1.9f);
			}
			else
			{
				Angle_x = Mathf.Lerp(Angle_x, 0f, Time.deltaTime);
				new_x = Mathf.Lerp(new_x, 0f, Time.deltaTime);
				new_y = Mathf.Lerp(new_y, 0f, Time.deltaTime);
				z = Mathf.Lerp(z, 0f, Time.deltaTime);
			}
			if (!menu.Edit_Rox_Animator.GetBool("Moving"))
			{
				if (Input.GetKey(KeyCode.D))
				{
					menu.Edit_Rox_Animator.SetBool("Rotate_L", value: true);
				}
				if (menu.Edit_Rox_Animator.GetBool("Rotate_L") && Input.GetKeyUp(KeyCode.D))
				{
					menu.Edit_Rox_Animator.SetBool("Rotate_L", value: false);
					menu.Edit_Rox_Animator.SetTrigger("Stop_Rotate");
				}
				if (Input.GetKey(KeyCode.A))
				{
					menu.Edit_Rox_Animator.SetBool("Rotate_R", value: true);
				}
				if (menu.Edit_Rox_Animator.GetBool("Rotate_R") && Input.GetKeyUp(KeyCode.A))
				{
					menu.Edit_Rox_Animator.SetBool("Rotate_R", value: false);
					menu.Edit_Rox_Animator.SetTrigger("Stop_Rotate");
				}
			}
			y = Mathf.Lerp(y, new_y, Time.deltaTime * 2f);
			x = Mathf.Lerp(x, new_x, Time.deltaTime * 2f);
			if (!menu.Switching_Menu_Process)
			{
				Cam_Constraint.transform.localPosition = new Vector3(0.25f + x, y, z);
				Cam_Constraint.weight = 1f;
				Cam_Constraint.rotationOffset = new Vector3(Start_Aim_Angle + Angle_x, 0f, 0f);
			}
			else
			{
				Cam_Constraint.weight -= Time.deltaTime / 2f;
			}
		}
		else
		{
			Cam_Constraint.weight = 0f;
			Cam_Constraint.rotationAtRest = new Vector3(0f, 0f, 0f);
			Cam_Constraint.transform.localPosition = new Vector3(0f, 0f, 0f);
			Cam_Constraint.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			x = 0f;
			y = 0f;
			z = 0f;
			new_x = 0f;
			new_y = 0f;
			Angle_x = 0f;
		}
		if (Rotate_Back)
		{
			Rox.transform.rotation = Quaternion.RotateTowards(Rox.transform.rotation, Quaternion.Euler(0f, 150f, 0f), Time.deltaTime * 250f);
		}
	}

	public void Stop_Emotions()
	{
		menu.Edit_Rox_Animator.SetBool("Stop", value: true);
	}

	public void Change_Edit_Mode(bool Character)
	{
		House_Edit_Window.gameObject.SetActive(!Character);
		Character_Edit_Window.gameObject.SetActive(Character);
	}

	public void Get_Character_Settings()
	{
		Get_Pussy_Settings();
		Get_Piercing_Settings();
		Get_Hairstyle_Settings();
		Get_Body_Colors_Settings();
		Get_Cloth_Color_Settings();
		Get_Ass_Settings();
		Get_Boobs_Settings();
		Get_Fatness_Settings();
		Get_Eye_Size_Settings();
		Get_Character_Texture_Settings();
		Get_House_Texture_Settings();
		Get_Texture_Tiling();
		Get_House_Tiling();
		Get_Poster_Params();
		Get_Tatoo();
		x = 0f;
		z = 0f;
		y = Body_y;
		new_y = Body_y;
		new_x = 0f;
	}

	public void Show_Hide_Piercing()
	{
		if (menu.Menu)
		{
			piercings.gameObject.SetActive(data.Clothes[0].Weared || menu.Clothes[0].Edit_Weared);
			piercings_Menu.gameObject.SetActive(piercings.gameObject.activeInHierarchy);
		}
		else
		{
			piercings.gameObject.SetActive(data.Clothes[0].Weared);
			Inv_Model.piercings.gameObject.SetActive(piercings.gameObject.activeInHierarchy);
		}
	}

	private void Get_House_Texture_Settings()
	{
		for (int i = 0; i < 4; i++)
		{
			if (data.Room.Path_To_File_House[i].Length > 0)
			{
				if (menu.Menu)
				{
					StartCoroutine(Output_House_Routine(data.Room.Path_To_File_House[i], menu.House_Materials[i], UI_Elements.Wall_Image[i]));
				}
				else
				{
					StartCoroutine(Output_House_Routine(data.Room.Path_To_File_House[i], menu.House_Materials[i], null));
				}
			}
			else if (menu.Menu)
			{
				UI_Elements.Wall_Image[i].texture = menu.Start_House_Texture[i];
			}
		}
		for (int j = 0; j < 3; j++)
		{
			if (data.Room.Path_To_File_Poster[j].Length > 0)
			{
				if (menu.Menu)
				{
					StartCoroutine(Output_House_Routine(data.Room.Path_To_File_Poster[j], menu.Poster_Materials[j], UI_Elements.Poster_Image[j]));
				}
				else
				{
					StartCoroutine(Output_House_Routine(data.Room.Path_To_File_Poster[j], menu.Poster_Materials[j], null));
				}
			}
			else if (menu.Menu)
			{
				UI_Elements.Poster_Image[j].texture = menu.Start_Poster_Texture[j];
			}
		}
	}

	private void Get_Character_Texture_Settings()
	{
		for (int i = 0; i < 4; i++)
		{
			if (data.Clothes[i].Path_To_File.Length > 0)
			{
				if (menu.Menu)
				{
					RawImage rawImage = UI_Elements.Panties_Image;
					if (i == 0)
					{
						rawImage = UI_Elements.Panties_Image;
					}
					if (i == 1)
					{
						rawImage = UI_Elements.Bra_Image;
					}
					if (i == 2)
					{
						rawImage = UI_Elements.Skirt_Image;
					}
					if (i == 3)
					{
						rawImage = UI_Elements.Shirt_Image;
					}
					if (rawImage != null)
					{
						StartCoroutine(OutputRoutine(i, data.Clothes[i].Path_To_File, rawImage));
					}
				}
				else
				{
					StartCoroutine(OutputGameRoutine(i, data.Clothes[i].Path_To_File));
				}
			}
			else if (menu.Menu)
			{
				_ = UI_Elements;
				if (i == 0)
				{
					UI_Elements.Panties_Image.texture = menu.Clothes[i].start_texture;
				}
				if (i == 1)
				{
					UI_Elements.Bra_Image.texture = menu.Clothes[i].start_texture;
				}
				if (i == 2)
				{
					UI_Elements.Skirt_Image.texture = menu.Clothes[i].start_texture;
				}
				if (i == 3)
				{
					UI_Elements.Shirt_Image.texture = menu.Clothes[i].start_texture2;
				}
			}
		}
	}

	private void Get_Pussy_Settings()
	{
		for (int i = 1; i < pussy_hairs.childCount; i++)
		{
			pussy_hairs.GetChild(i).gameObject.SetActive(data.Character.pussy_hairs == i);
			pussy_hairs_Menu.GetChild(i).gameObject.SetActive(data.Character.pussy_hairs == i);
			if (!menu.Menu)
			{
				Inv_Model.pussy_hairs.GetChild(i).gameObject.SetActive(data.Character.pussy_hairs == i);
				interface_script.Rox_Interface.pussy_hairs_mast.GetChild(i).gameObject.SetActive(data.Character.pussy_hairs == i);
			}
		}
	}

	private void Get_Piercing_Settings()
	{
		for (int i = 1; i < piercings.childCount; i++)
		{
			piercings.GetChild(i).gameObject.SetActive(data.Character.piercing == i);
			piercings_Menu.GetChild(i).gameObject.SetActive(data.Character.piercing == i);
			if (!menu.Menu)
			{
				Inv_Model.piercings.GetChild(i).gameObject.SetActive(data.Character.piercing == i);
				interface_script.Rox_Interface.piercings_mast.GetChild(i).gameObject.SetActive(data.Character.piercing == i);
			}
		}
	}

	private void Get_Hairstyle_Settings()
	{
		for (int i = 0; i < hairstyles.childCount; i++)
		{
			hairstyles.GetChild(i).gameObject.SetActive(data.Character.hairstyle == i);
			hairstyles_Menu.GetChild(i).gameObject.SetActive(data.Character.hairstyle == i);
			if (!menu.Menu)
			{
				Inv_Model.hairstyles.GetChild(i).gameObject.SetActive(data.Character.hairstyle == i);
				interface_script.Rox_Interface.hairstyles_mast.GetChild(i).gameObject.SetActive(data.Character.hairstyle == i);
			}
			for (int j = 0; j < Hair_Bones.Length; j++)
			{
				Hair_Bones[j].GetChild(i).gameObject.SetActive(data.Character.hairstyle == i);
			}
		}
	}

	public void Get_Ass_Settings()
	{
		body_bones.Butt_L.localPosition = Size(proportions.Butt_L_Min.localPosition, proportions.Butt_L_Max.localPosition, data.Character.ass_size);
		body_bones.Butt_L.localScale = Size(proportions.Butt_L_Min.localScale, proportions.Butt_L_Max.localScale, data.Character.ass_size);
		body_bones.Butt_R.localPosition = Size(proportions.Butt_R_Min.localPosition, proportions.Butt_R_Max.localPosition, data.Character.ass_size);
		body_bones.Butt_R.localScale = Size(proportions.Butt_R_Min.localScale, proportions.Butt_R_Max.localScale, data.Character.ass_size);
		Ass_Col_L.radius = Mathf.Lerp(0.0008f, 0.0011f, data.Character.ass_size);
		Ass_Col_R.radius = Mathf.Lerp(0.0008f, 0.0011f, data.Character.ass_size);
		Ass_Col_L.transform.localPosition = Vector3.Lerp(new Vector3(0f, 0.0004f, -0.0004f), new Vector3(0.0001f, 0.00045f, -0.0006f), data.Character.ass_size);
		Ass_Col_R.transform.localPosition = Vector3.Lerp(new Vector3(0f, 0.0004f, -0.0004f), new Vector3(-0.0001f, 0.00045f, -0.0006f), data.Character.ass_size);
		menu_bones.Butt_L.localPosition = body_bones.Butt_L.localPosition;
		menu_bones.Butt_L.localScale = body_bones.Butt_L.localScale;
		menu_bones.Butt_R.localPosition = body_bones.Butt_R.localPosition;
		menu_bones.Butt_R.localScale = body_bones.Butt_R.localScale;
	}

	public void Get_Boobs_Settings()
	{
		body_bones.Boob_L.localPosition = Size(proportions.Boob_L_Min.localPosition, proportions.Boob_L_Max.localPosition, data.Character.boobs_size);
		body_bones.Boob_L.localScale = Size(proportions.Boob_L_Min.localScale, proportions.Boob_L_Max.localScale, data.Character.boobs_size);
		body_bones.Boob_R.localPosition = Size(proportions.Boob_R_Min.localPosition, proportions.Boob_R_Max.localPosition, data.Character.boobs_size);
		body_bones.Boob_R.localScale = Size(proportions.Boob_R_Min.localScale, proportions.Boob_R_Max.localScale, data.Character.boobs_size);
		menu_bones.Boob_L.localPosition = body_bones.Boob_L.localPosition;
		menu_bones.Boob_L.localScale = body_bones.Boob_L.localScale;
		menu_bones.Boob_R.localPosition = body_bones.Boob_R.localPosition;
		menu_bones.Boob_R.localScale = body_bones.Boob_R.localScale;
	}

	public void Get_Fatness_Settings()
	{
	}

	public void Get_Eye_Size_Settings()
	{
	}

	public void Set_Pussy_Hairs(int id)
	{
		new_y = Pussy_y;
		data.Character.pussy_hairs = id;
		Get_Pussy_Settings();
		UI_Elements.pussy_text_ui.text = data.Character.pussy_hairs.ToString();
	}

	public void Set_Piercing(int id)
	{
		new_y = Pussy_y;
		data.Character.piercing = id;
		Get_Piercing_Settings();
		UI_Elements.piercing_text_ui.text = data.Character.piercing.ToString();
	}

	public void Set_Hairstyle(int id)
	{
		new_y = Face_y;
		data.Character.hairstyle = id;
		Get_Hairstyle_Settings();
		UI_Elements.hairstyle_text_ui.text = data.Character.hairstyle.ToString();
	}

	public void Set_Skincolor(int id)
	{
		new_y = Body_y;
		data.Character.skincolor = id;
		Get_Body_Colors_Settings();
		UI_Elements.skincolor_text_ui.text = data.Character.skincolor.ToString();
	}

	private void Attach_Body_Colors()
	{
		Sam_Body_Materials[0] = skin_materials[data.Character.skincolor].Body;
		Sam_Head_Materials[0] = skin_materials[data.Character.skincolor].Head;
		body.materials = Sam_Body_Materials;
		head.materials = Sam_Head_Materials;
		eyes.materials = Sam_Eye_Materials;
		body_Menu.sharedMaterials = body.sharedMaterials;
		head_Menu.sharedMaterials = head.sharedMaterials;
		eyes_Menu.sharedMaterials = eyes.sharedMaterials;
	}

	private void Get_Body_Colors_Settings()
	{
		if (menu.Menu)
		{
			UI_Elements.Hair_Color.GetComponent<Image>().color = data.Character.hair_color;
			UI_Elements.Eye_Color.GetComponent<Image>().color = data.Character.eye_color;
			UI_Elements.Pussy_Color.GetComponent<Image>().color = data.Character.pussy_color;
			UI_Elements.Finger_Color.GetComponent<Image>().color = data.Character.finger_color;
			UI_Elements.Lips_Color.GetComponent<Image>().color = data.Character.lips_color;
			UI_Elements.Eyeshadows_Color.GetComponent<Image>().color = data.Character.eyeshadow_color;
			UI_Elements.Panties_Color.GetComponent<Image>().color = data.Clothes[2].main_color;
			UI_Elements.Bra_Color.GetComponent<Image>().color = data.Clothes[2].main_color;
			UI_Elements.Skirt_Color.GetComponent<Image>().color = data.Clothes[2].main_color;
			UI_Elements.Shirt_Color.GetComponent<Image>().color = data.Clothes[2].main_color;
			UI_Elements.Shadow_Eye_Enable.gameObject.SetActive(data.Character.Eyeshadows);
			UI_Elements.Shadow_Eye_Disable.gameObject.SetActive(!data.Character.Eyeshadows);
		}
		Set_Hair_Materials();
		eye_material.color = data.Character.eye_color;
		fingernail_material.color = data.Character.finger_color;
		pussy_hair_material.color = data.Character.pussy_color;
		lips_material.color = data.Character.lips_color;
		eyeshadow_material.color = data.Character.eyeshadow_color;
		Attach_Body_Colors();
	}

	public void Set_Hair_Materials()
	{
		for (int i = 0; i < hair_material.Length; i++)
		{
			hair_material[i].color = data.Character.hair_color;
			hair_material[i].SetColor("_Emission", hair_material[i].color / 4f);
			inventory_hair_material[i].color = hair_material[i].color;
		}
		scalp_material.color = data.Character.hair_color;
	}

	private void Get_Cloth_Color_Settings()
	{
		if (menu.Menu)
		{
			menu.Clothes[0].Edit_Model[0].GetComponentInChildren<Renderer>().sharedMaterial.SetColor("_Color", data.Clothes[0].main_color);
			menu.Clothes[1].Edit_Model[0].GetComponentInChildren<Renderer>().sharedMaterial.SetColor("_Color", data.Clothes[1].main_color);
			menu.Clothes[8].Edit_Model[2].GetComponentInChildren<Renderer>().sharedMaterial.color = data.Clothes[8].main_color;
		}
	}

	public void Set_Haircolor()
	{
		new_y = Face_y;
		Stop_Color_Previews(UI_Elements.Hair_Color);
		if (Color_Picker.Current_Preview == UI_Elements.Hair_Color && Color_Picker.gameObject.activeInHierarchy)
		{
			Show_Color_Picker(on: false);
		}
		else
		{
			Show_Color_Picker(on: true);
		}
		Color_Picker.Current_Preview = UI_Elements.Hair_Color;
	}

	public void Set_PussyColor()
	{
		new_y = Pussy_y;
		Stop_Color_Previews(UI_Elements.Pussy_Color);
		if (Color_Picker.Current_Preview == UI_Elements.Pussy_Color && Color_Picker.gameObject.activeInHierarchy)
		{
			Show_Color_Picker(on: false);
		}
		else
		{
			Show_Color_Picker(on: true);
		}
		Color_Picker.Current_Preview = UI_Elements.Pussy_Color;
	}

	public void Set_Eyecolor()
	{
		new_y = Face_y;
		Stop_Color_Previews(UI_Elements.Eye_Color);
		if (Color_Picker.Current_Preview == UI_Elements.Eye_Color && Color_Picker.gameObject.activeInHierarchy)
		{
			Show_Color_Picker(on: false);
		}
		else
		{
			Show_Color_Picker(on: true);
		}
		Color_Picker.Current_Preview = UI_Elements.Eye_Color;
	}

	public void Set_Finger_Color()
	{
		new_y = Body_y;
		Stop_Color_Previews(UI_Elements.Finger_Color);
		if (Color_Picker.Current_Preview == UI_Elements.Finger_Color && Color_Picker.gameObject.activeInHierarchy)
		{
			Show_Color_Picker(on: false);
		}
		else
		{
			Show_Color_Picker(on: true);
		}
		Color_Picker.Current_Preview = UI_Elements.Finger_Color;
	}

	public void Set_Lips_Color()
	{
		new_y = Face_y;
		Stop_Color_Previews(UI_Elements.Lips_Color);
		if (Color_Picker.Current_Preview == UI_Elements.Lips_Color && Color_Picker.gameObject.activeInHierarchy)
		{
			Show_Color_Picker(on: false);
		}
		else
		{
			Show_Color_Picker(on: true);
		}
		Color_Picker.Current_Preview = UI_Elements.Lips_Color;
	}

	public void Set_Eyeshadows_Color()
	{
		new_y = Face_y;
		Stop_Color_Previews(UI_Elements.Eyeshadows_Color);
		if (Color_Picker.Current_Preview == UI_Elements.Eyeshadows_Color && Color_Picker.gameObject.activeInHierarchy)
		{
			Show_Color_Picker(on: false);
		}
		else
		{
			Show_Color_Picker(on: true);
		}
		Color_Picker.Current_Preview = UI_Elements.Eyeshadows_Color;
	}

	public void Turn_EyeShadows(bool On)
	{
		data.Character.Eyeshadows = On;
		Attach_Body_Colors();
	}

	public void Set_Panties_Color()
	{
		new_y = Pussy_y;
		Stop_Color_Previews(UI_Elements.Panties_Color);
		if (Color_Picker.Current_Preview == UI_Elements.Panties_Color && Color_Picker.gameObject.activeInHierarchy)
		{
			Show_Color_Picker(on: false);
		}
		else
		{
			Show_Color_Picker(on: true);
		}
		Color_Picker.Current_Preview = UI_Elements.Panties_Color;
	}

	public void Set_Bra_Color()
	{
		new_y = Boobs_y;
		Stop_Color_Previews(UI_Elements.Bra_Color);
		if (Color_Picker.Current_Preview == UI_Elements.Bra_Color && Color_Picker.gameObject.activeInHierarchy)
		{
			Show_Color_Picker(on: false);
		}
		else
		{
			Show_Color_Picker(on: true);
		}
		Color_Picker.Current_Preview = UI_Elements.Bra_Color;
	}

	public void Set_Stockings_Color()
	{
		new_y = Pussy_y;
		Stop_Color_Previews(UI_Elements.Stockings_Color);
		if (Color_Picker.Current_Preview == UI_Elements.Stockings_Color && Color_Picker.gameObject.activeInHierarchy)
		{
			Show_Color_Picker(on: false);
		}
		else
		{
			Show_Color_Picker(on: true);
		}
		Color_Picker.Current_Preview = UI_Elements.Stockings_Color;
	}

	public void Set_Skirt_Color()
	{
		new_y = Pussy_y;
		Stop_Color_Previews(UI_Elements.Skirt_Color);
		if (Color_Picker.Current_Preview == UI_Elements.Skirt_Color && Color_Picker.gameObject.activeInHierarchy)
		{
			Show_Color_Picker(on: false);
		}
		else
		{
			Show_Color_Picker(on: true);
		}
		Color_Picker.Current_Preview = UI_Elements.Skirt_Color;
	}

	public void Set_Shirt_Color(int id)
	{
		new_y = Boobs_y;
		if (id == 0)
		{
			Stop_Color_Previews(UI_Elements.Shirt_Color);
			if (Color_Picker.Current_Preview == UI_Elements.Shirt_Color && Color_Picker.gameObject.activeInHierarchy)
			{
				Show_Color_Picker(on: false);
			}
			else
			{
				Show_Color_Picker(on: true);
			}
			Color_Picker.Current_Preview = UI_Elements.Shirt_Color;
		}
		if (id == 1)
		{
			Stop_Color_Previews(UI_Elements.Shirt_Color2);
			if (Color_Picker.Current_Preview == UI_Elements.Shirt_Color2 && Color_Picker.gameObject.activeInHierarchy)
			{
				Show_Color_Picker(on: false);
			}
			else
			{
				Show_Color_Picker(on: true);
			}
			Color_Picker.Current_Preview = UI_Elements.Shirt_Color2;
		}
	}

	public void Set_Tatoo_Color()
	{
	}

	public void Stop_Color_Previews(ColorPreview Chosen)
	{
		UI_Elements.Hair_Color.enabled = UI_Elements.Hair_Color == Chosen;
		UI_Elements.Finger_Color.enabled = UI_Elements.Finger_Color == Chosen;
		UI_Elements.Skirt_Color.enabled = UI_Elements.Skirt_Color == Chosen;
		UI_Elements.Shirt_Color.enabled = UI_Elements.Shirt_Color == Chosen;
		UI_Elements.Shirt_Color2.enabled = UI_Elements.Shirt_Color2 == Chosen;
		UI_Elements.Panties_Color.enabled = UI_Elements.Panties_Color == Chosen;
		UI_Elements.Bra_Color.enabled = UI_Elements.Bra_Color == Chosen;
	}

	public void Set_Ass_Size()
	{
		new_y = Pussy_y;
		data.Character.ass_size = UI_Elements.Ass_Slider.value;
		Get_Ass_Settings();
	}

	public void Set_Boobs_Size()
	{
		new_y = Boobs_y;
		data.Character.boobs_size = UI_Elements.Boobs_Slider.value;
		Get_Boobs_Settings();
	}

	public void Set_Fatness()
	{
		new_y = Body_y;
		data.Character.fatness = UI_Elements.Fat_Slider.value;
		Get_Fatness_Settings();
	}

	public void Set_Eye_Size()
	{
		new_y = Face_y;
		data.Character.eye_size = UI_Elements.Eye_Slider.value;
		Get_Eye_Size_Settings();
	}

	public void Set_Panties_Image()
	{
		new_y = Pussy_y;
		OpenFileBrowser(0, UI_Elements.Panties_Image);
		menu.Clothes[0].Edit_Model[0].GetComponent<Renderer>().sharedMaterial.mainTexture = UI_Elements.Panties_Image.texture;
	}

	public void Set_Bra_Image()
	{
		new_y = Boobs_y;
		OpenFileBrowser(1, UI_Elements.Bra_Image);
		menu.Clothes[1].Edit_Model[0].GetComponent<Renderer>().sharedMaterial.mainTexture = UI_Elements.Bra_Image.texture;
	}

	public void Set_Skirt_Image()
	{
		new_y = Pussy_y;
		OpenFileBrowser(2, UI_Elements.Skirt_Image);
		menu.Clothes[2].Edit_Model[0].GetComponent<Renderer>().sharedMaterial.mainTexture = UI_Elements.Skirt_Image.texture;
	}

	public void Set_Shirt_Image()
	{
		new_y = Boobs_y;
		OpenFileBrowser(3, UI_Elements.Shirt_Image);
	}

	public void Set_Tatoo_Image()
	{
		OpenTatooFileBrowser();
	}

	public void Set_Tatoo_Orig_Image()
	{
		OpenTatooFileBrowser();
	}

	public void Set_Tatoo_id()
	{
		print_params.tatoo_ids = new Id_Object[print_params.Tatoo_hidden.Length][];
		for (int i = 0; i < print_params.Tatoo_hidden.Length; i++)
		{
			print_params.tatoo_ids[i] = print_params.Tatoo_hidden[i].GetComponentsInChildren<Id_Object>(includeInactive: true);
			for (int j = 0; j < print_params.tatoo_ids[i].Length; j++)
			{
				print_params.tatoo_ids[i][j].id = j;
			}
		}
	}

	public void Get_Tatoo()
	{
		print_params.tatoo_mats = new Material[data.Tatoo.Length];
		StartCoroutine(Output_Get_TatooRoutine());
	}

	public void Erase_Tatoo()
	{
		data.Tatoo[print_params.Chosen_Tatoo.Tatoo.GetComponent<Id_Object>().id].Path_To_File_Tatoo = "";
		StartCoroutine(Output_Get_TatooRoutine());
	}

	public void Get_Texture_Tiling()
	{
		if (menu.Menu)
		{
			for (int i = 0; i < 3; i++)
			{
			}
			UI_Elements.Panties_x.value = data.Clothes[0].Tiling.x;
			UI_Elements.Panties_y.value = data.Clothes[0].Tiling.y;
			UI_Elements.Bra_x.value = data.Clothes[1].Tiling.x;
			UI_Elements.Bra_y.value = data.Clothes[1].Tiling.y;
			UI_Elements.Skirt_x.value = data.Clothes[2].Tiling.x;
			UI_Elements.Skirt_y.value = data.Clothes[2].Tiling.y;
		}
	}

	public void Set_Texture_Tiling(int id)
	{
		if (id == 0)
		{
			data.Clothes[0].Tiling.x = UI_Elements.Panties_x.value;
			data.Clothes[0].Tiling.y = UI_Elements.Panties_y.value;
			menu.Clothes[0].Edit_Model[0].GetComponent<Renderer>().sharedMaterial.mainTextureScale = data.Clothes[0].Tiling;
		}
		if (id == 1)
		{
			data.Clothes[1].Tiling.x = UI_Elements.Bra_x.value;
			data.Clothes[1].Tiling.y = UI_Elements.Bra_y.value;
			menu.Clothes[1].Edit_Model[0].GetComponent<Renderer>().sharedMaterial.mainTextureScale = data.Clothes[1].Tiling;
		}
		if (id == 2)
		{
			data.Clothes[2].Tiling.x = UI_Elements.Skirt_x.value;
			data.Clothes[2].Tiling.y = UI_Elements.Skirt_y.value;
			menu.Clothes[2].Edit_Model[0].GetComponent<Renderer>().sharedMaterial.mainTextureScale = data.Clothes[2].Tiling;
		}
	}

	public void Get_House_Tiling()
	{
		if (menu.Menu)
		{
			for (int i = 0; i < 4; i++)
			{
				UI_Elements.Wall_Slider_x[i].value = data.Room.Tiling[0].x;
				UI_Elements.Wall_Slider_y[i].value = data.Room.Tiling[0].y;
				menu.House_Materials[i].mainTextureScale = data.Room.Tiling[i];
			}
		}
	}

	public void Set_House_Tiling(int id)
	{
		data.Room.Tiling[id] = new Vector2(UI_Elements.Wall_Slider_x[id].value, UI_Elements.Wall_Slider_y[id].value);
		menu.House_Materials[id].mainTextureScale = new Vector2(UI_Elements.Wall_Slider_x[id].value, UI_Elements.Wall_Slider_y[id].value);
	}

	public void Get_Poster_Params()
	{
		for (int i = 0; i < menu.Poster.Length; i++)
		{
			if ((bool)menu.Poster[i])
			{
				menu.Poster[i].gameObject.SetActive(data.Room.Poster_Show[i]);
			}
		}
	}

	public void Poster_Show(int id)
	{
		data.Room.Poster_Show[id] = true;
		Get_Poster_Params();
	}

	public void Poster_Hide(int id)
	{
		data.Room.Poster_Show[id] = false;
		Get_Poster_Params();
	}

	public void OpenPhoneFileBrowser()
	{
		string[] array = StandaloneFileBrowser.OpenFilePanel("Photo", "", "png", multiselect: false);
		if (array.Length != 0)
		{
			StartCoroutine(OutputPhotoRoutine(new Uri(array[0]).AbsoluteUri));
		}
	}

	private IEnumerator OutputPhotoRoutine(string url)
	{
		WWW loader = new WWW(url);
		yield return loader;
		Phone_Image.texture = loader.texture;
		data.Photo_Background_Path = url;
	}

	public void OpenTatooFileBrowser()
	{
		string[] array = StandaloneFileBrowser.OpenFilePanel("Tatoo", "", "png", multiselect: false);
		if (array.Length != 0)
		{
			StartCoroutine(OutputTatooRoutine(new Uri(array[0]).AbsoluteUri, print_params.Chosen_Tatoo));
		}
	}

	private IEnumerator OutputTatooRoutine(string url, Tatoo_Object tatoo)
	{
		WWW loader = new WWW(url);
		yield return loader;
		if (data.Tatoo[tatoo.Tatoo.GetComponent<Id_Object>().id].Path_To_File_Tatoo.Length > 0)
		{
			UI_Elements.Tatoo_Image.texture = loader.texture;
		}
		else
		{
			UI_Elements.Tatoo_Image.texture = print_params.Start_Image;
		}
		data.Tatoo[tatoo.Tatoo.GetComponent<Id_Object>().id].Path_To_File_Tatoo = url;
		print_params.Position_Param.gameObject.SetActive(value: true);
		menu.Use_Tool(7);
		menu.Use_Tool(8);
		StartCoroutine(Output_Get_TatooRoutine());
	}

	private IEnumerator Output_Get_TatooRoutine()
	{
		for (int b = 0; b < print_params.Tatoo_hidden.Length; b++)
		{
			for (int a = 0; a < data.Tatoo.Length; a++)
			{
				if (data.Tatoo[a].Path_To_File_Tatoo.Length > 0)
				{
					print_params.tatoo_mats[a] = new Material(print_params.Base_Mat);
					print_params.tatoo_ids[b][a].GetComponent<Renderer>().material = print_params.tatoo_mats[a];
					WWW loader = new WWW(data.Tatoo[a].Path_To_File_Tatoo);
					yield return loader;
					print_params.tatoo_ids[b][a].GetComponent<Renderer>().material.mainTexture = loader.texture;
					print_params.tatoo_ids[b][a].GetComponent<Renderer>().material.mainTextureScale = data.Tatoo[a].Start_Tiling;
					print_params.tatoo_ids[b][a].GetComponent<Renderer>().material.mainTextureOffset = data.Tatoo[a].Offset;
					print_params.tatoo_ids[b][a].GetComponent<Renderer>().material.mainTexture.wrapMode = TextureWrapMode.Clamp;
					print_params.tatoo_ids[b][a].transform.parent = print_params.Tatoo_shown[b];
				}
				else
				{
					print_params.tatoo_ids[b][a].transform.parent = print_params.Tatoo_hidden[b];
				}
			}
		}
	}

	public void OpenFileBrowser(int id, RawImage image)
	{
		string[] array = StandaloneFileBrowser.OpenFilePanel("Title", "", "png", multiselect: false);
		if (array.Length != 0)
		{
			StartCoroutine(OutputRoutine(id, new Uri(array[0]).AbsoluteUri, image));
		}
		data.Clothes[id].Path_To_File = new Uri(array[0]).AbsoluteUri;
	}

	private IEnumerator OutputRoutine(int id, string url, RawImage image)
	{
		WWW loader = new WWW(url);
		yield return loader;
		image.texture = loader.texture;
		if (id != 3)
		{
			menu.Clothes[id].Edit_Model[0].GetComponent<Renderer>().sharedMaterial.mainTexture = image.texture;
		}
		else
		{
			menu.Clothes[id].Edit_Model[0].GetComponent<Renderer>().sharedMaterials[1].mainTexture = image.texture;
		}
	}

	private IEnumerator OutputGameRoutine(int id, string url)
	{
		WWW loader = new WWW(url);
		yield return loader;
		if (id != 3)
		{
			inventory.Clothes[id].Mesh[0].GetComponent<Renderer>().sharedMaterial.mainTexture = loader.texture;
			inventory.Clothes[id].Inv_Mesh[0].GetComponent<Renderer>().sharedMaterial.mainTexture = loader.texture;
		}
		else
		{
			inventory.Clothes[id].Mesh[0].GetComponent<Renderer>().sharedMaterials[1].mainTexture = loader.texture;
			inventory.Clothes[id].Inv_Mesh[0].GetComponent<Renderer>().sharedMaterials[1].mainTexture = loader.texture;
		}
	}

	public void Open_Wall_FileBrowser(int id, bool House, RawImage image)
	{
		string[] array = StandaloneFileBrowser.OpenFilePanel("Title", "", "png", multiselect: false);
		if (array.Length != 0)
		{
			if (House)
			{
				StartCoroutine(Output_House_Routine(new Uri(array[0]).AbsoluteUri, menu.House_Materials[id], image));
				data.Room.Path_To_File_House[id] = new Uri(array[0]).AbsoluteUri;
			}
			else
			{
				StartCoroutine(Output_House_Routine(new Uri(array[0]).AbsoluteUri, menu.Poster_Materials[id], image));
				data.Room.Path_To_File_Poster[id] = new Uri(array[0]).AbsoluteUri;
			}
		}
	}

	private IEnumerator Output_House_Routine(string url, Material mat, RawImage image)
	{
		if (!data.first_game)
		{
			WWW loader = new WWW(url);
			yield return loader;
			if ((bool)image)
			{
				image.texture = loader.texture;
			}
			mat.mainTexture = loader.texture;
		}
		else if ((bool)image)
		{
			image.texture = mat.mainTexture;
		}
	}

	public void Close_All_Subparams()
	{
		for (int i = 0; i < Sub_Params.Length; i++)
		{
			Sub_Params[i].Close_Subparams();
		}
	}

	public Vector3 Size(Vector3 min, Vector3 max, float value)
	{
		return Vector3.Lerp(min, max, value);
	}

	public void Turn_Tatoo_Mode()
	{
		print_params.Tatoo_Mode = !print_params.Tatoo_Mode;
		Get_Tatoo_Mode();
	}

	public void Get_Tatoo_Mode()
	{
		print_params.Tatoo_hidden[0].gameObject.SetActive(print_params.Tatoo_Mode);
		if (!print_params.Tatoo_Mode)
		{
			print_params.Position_Param.gameObject.SetActive(value: false);
			for (int i = 0; i < print_params.Chosen.Length; i++)
			{
				print_params.Chosen[i].gameObject.SetActive(value: false);
			}
			print_params.Not_Chosen.gameObject.SetActive(value: true);
			print_params.Place_Name.text = "";
		}
	}

	public void Set_Tatto_Offset()
	{
		print_params.Chosen_Tatoo.Tatoo.material.mainTextureOffset = new Vector2(print_params.Tatoo_O_x.value, print_params.Tatoo_O_y.value);
		int id = print_params.Chosen_Tatoo.Tatoo.GetComponent<Id_Object>().id;
		data.Tatoo[id].Offset = print_params.Chosen_Tatoo.Tatoo.material.mainTextureOffset;
	}

	public void Check_Button_Hairstyle()
	{
		for (int i = 0; i < UI_Elements.Hairstyle_Buttons.childCount; i++)
		{
			UI_Elements.Hairstyle_Buttons.GetChild(i).GetComponent<Outline>().effectDistance = ((data.Character.hairstyle == UI_Elements.Hairstyle_Buttons.GetChild(i).GetComponent<Id_Object>().id) ? new Vector2(6f, -6f) : new Vector2(3f, -3f));
		}
	}

	public void Check_Button_Pussy_hair()
	{
		for (int i = 0; i < UI_Elements.Pubic_Buttons.childCount; i++)
		{
			UI_Elements.Pubic_Buttons.GetChild(i).GetComponent<Outline>().effectDistance = ((data.Character.pussy_hairs == UI_Elements.Pubic_Buttons.GetChild(i).GetComponent<Id_Object>().id) ? new Vector2(6f, -6f) : new Vector2(3f, -3f));
		}
	}

	public void Check_Button_Piercing()
	{
		for (int i = 0; i < UI_Elements.Piercing_Pussy_buttons.childCount; i++)
		{
			UI_Elements.Piercing_Pussy_buttons.GetChild(i).GetComponent<Outline>().effectDistance = ((data.Character.piercing == UI_Elements.Piercing_Pussy_buttons.GetChild(i).GetComponent<Id_Object>().id) ? new Vector2(6f, -6f) : new Vector2(3f, -3f));
		}
	}

	public void Check_Button_Skin()
	{
		for (int i = 0; i < UI_Elements.Piercing_Pussy_buttons.childCount; i++)
		{
			UI_Elements.Skin_Buttons.GetChild(i).GetComponent<Outline>().effectDistance = ((data.Character.skincolor == UI_Elements.Skin_Buttons.GetChild(i).GetComponent<Id_Object>().id) ? new Vector2(6f, -6f) : new Vector2(3f, -3f));
		}
	}

	private void Check_All_Buttons()
	{
		Check_Button_Hairstyle();
		Check_Button_Pussy_hair();
		Check_Button_Piercing();
		Check_Button_Skin();
	}
}
