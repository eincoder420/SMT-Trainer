using System;
using UnityEngine;
using UnityEngine.UI;

public class Smartphone : MonoBehaviour
{
	[Serializable]
	public struct Interior_Achieve_Progress_UI
	{
		public Transform Parent;
	}

	[Serializable]
	public struct Street_Achieve_Progress_UI
	{
		public Transform Parent;
	}

	[Serializable]
	public struct Statistics
	{
		public Text Clothes_Owned;

		public Text Hairstyles_Owned;

		public Text Sex_Toys_Owned;

		public Text Accesories_Owned;

		public Text Selfie_Made;

		public Text Danced;

		public Text Fucked_Toy;

		public Text Money_Got;

		public Text Weight;

		public Text Noodles_Eaten;

		public Text Beer_Drunk;

		public Text Energy_Drunk;

		public Text People_Embarassed;

		public Text People_talked;

		public Text People_Knocked_Down;

		public Text Masturbated;

		public Text Naked_On_Public;

		public Text Had_sex;

		public Text Sex_partners;

		public Text Arrested;

		public Text Raped;
	}

	[HideInInspector]
	public Start_Menu menu;

	private ScreenshotHandler screenshot;

	private Edit_Base edit;

	[HideInInspector]
	public Roxanne_Control player;

	public Button Map;

	public Button Photo;

	public Button Chat;

	public Button Tutorial;

	public Button Return_Button;

	public Image Achieve_Image;

	public Text Achieve_Text;

	public Interior_Achieve_Progress_UI[] Interior_Achieves_UI;

	public Street_Achieve_Progress_UI[] Street_Achieves_UI;

	public Transform Progress_Achieve_Prefab;

	public Transform Tasks_Achieve_Prefab;

	public Transform Achieves_Folder;

	public ScrollRect Achieve_Scroll;

	public Text Rank;

	public Text Score;

	public Text New_Rank_Message;

	public Text[] Score_Requied;

	public Text Jerk_Level;

	public Text Dance_Level;

	public Text Toys_level;

	public Text Sex_Level;

	public Statistics statistics;

	private bool Vertical;

	public RectTransform Phone_Model;

	public RectTransform Horizontal_Aspect;

	public RectTransform Vertical_Aspect;

	public RectTransform[] Return_Aspect;

	public RectTransform[] Windows;

	public RectTransform Main_Screen;

	public RectTransform New_Achieve;

	public RectTransform New_Rank;

	public RectTransform Money_Message;

	public RectTransform Call;

	public RectTransform Shop;

	private bool Achieves_Instantiated;

	public RectTransform Map_Screen;

	public bool Pointer_On_Map;

	public Transform Education_Parent;

	public Text Player_Name_Text;

	public Text Money_Remain_Text;

	public Text Money_Added_Text;

	public Text Money_Balance_Text;

	public Text Money_Cash_Text;

	public Text Money_Summary_Text;

	public Text Shop_Money_Balance;

	public bool Have_New_Achieve;

	public bool Have_New_Money;

	public bool Have_New_Rank;

	public bool Have_New_Call;

	public bool Open_Shop;

	public Text Money_Earned;

	public Transform Haircuts_Folder;

	public Transform Cloth_Folder;

	public Transform[] Store_pages;

	private AudioSource audio;

	public AudioClip Buy_Sound;

	private void Start()
	{
		if (!menu)
		{
			menu = UnityEngine.Object.FindObjectOfType<Start_Menu>();
		}
		edit = UnityEngine.Object.FindObjectOfType<Edit_Base>();
		screenshot = UnityEngine.Object.FindObjectOfType<ScreenshotHandler>();
		if (!menu.Menu && !player)
		{
			player = UnityEngine.Object.FindObjectOfType<Roxanne_Control>();
		}
		audio = GetComponent<AudioSource>();
		Photo.interactable = !menu.Menu;
		Chat.interactable = false;
		Map.interactable = false;
		Tutorial.interactable = false;
		if (!menu.Menu && menu.Loader.level == 2)
		{
			Map.interactable = true;
		}
		Vertical = true;
		Set_Ratio();
		Recount_Money();
		Check_Shop_Objects();
		for (int i = 0; i < Store_pages.Length; i++)
		{
			Store_pages[i].gameObject.SetActive(i == 0 || i == 1);
		}
		Player_Name_Text.text = menu.data.Player_Name;
	}

	private void OnEnable()
	{
		if (!menu)
		{
			menu = UnityEngine.Object.FindObjectOfType<Start_Menu>();
		}
		Recount_Money();
		Set_Phone_Achieves();
	}

	public void Set_Scroll_Content(RectTransform content)
	{
		Achieve_Scroll.content = content;
	}

	private void Set_Phone_Progress()
	{
		for (int i = 2; i < Score_Requied.Length; i++)
		{
			Score_Requied[i].text = menu.data.progress_data.Score_Requied_For_Rank[i].ToString();
			if (menu.data.progress_data.Sum_Score >= menu.data.progress_data.Score_Requied_For_Rank[i])
			{
				menu.data.progress_data.Rank = i;
			}
		}
		if (menu.data.Language == 0)
		{
			Rank.text = menu.data.progress_data.Rank_Name_0[menu.data.progress_data.Rank];
		}
		if (menu.data.Language == 1)
		{
			Rank.text = menu.data.progress_data.Rank_Name_1[menu.data.progress_data.Rank];
		}
		if (menu.data.Language == 0)
		{
			New_Rank_Message.text = menu.data.progress_data.Rank_Name_0[menu.data.progress_data.Rank];
		}
		if (menu.data.Language == 1)
		{
			New_Rank_Message.text = menu.data.progress_data.Rank_Name_1[menu.data.progress_data.Rank];
		}
		Score.text = menu.data.progress_data.Sum_Score.ToString();
		Jerk_Level.text = menu.data.progress_data.Jerk_Level.ToString();
		Dance_Level.text = menu.data.progress_data.Dance_Level.ToString();
		Toys_level.text = menu.data.progress_data.Toy_Level.ToString();
		Sex_Level.text = menu.data.progress_data.Sex_Level.ToString();
	}

	private void Set_Phone_Statistics()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < menu.data.Clothes.Length; i++)
		{
			num2 += menu.data.Clothes[i].Spawned_Cloth.Length;
			for (int j = 0; j < menu.data.Clothes[i].Spawned_Cloth.Length; j++)
			{
				if (menu.data.Clothes[i].Spawned_Cloth[j].Bought)
				{
					num++;
				}
			}
		}
		statistics.Clothes_Owned.text = num + "/" + num2;
		int num3 = 0;
		for (int k = 0; k < menu.data.items.Haircuts_Bought.Length; k++)
		{
			if (menu.data.items.Haircuts_Bought[k])
			{
				num3++;
			}
		}
		statistics.Hairstyles_Owned.text = num3 + "/" + menu.data.items.Haircuts_Bought.Length;
		statistics.Sex_Toys_Owned.text = "6/6";
		statistics.Accesories_Owned.text = "12/12";
		statistics.Money_Got.text = menu.data.progress_data.Money_Earned.ToString();
		statistics.Masturbated.text = menu.data.progress_data.Masturbated.ToString();
		statistics.Beer_Drunk.text = menu.data.items.Items_Used[0].ToString();
		statistics.Noodles_Eaten.text = menu.data.items.Items_Used[1].ToString();
		statistics.Energy_Drunk.text = menu.data.items.Items_Used[2].ToString();
		statistics.People_Embarassed.text = menu.data.progress_data.People_Embarassed.ToString();
		statistics.People_talked.text = menu.data.progress_data.People_Talked.ToString();
		statistics.People_Knocked_Down.text = menu.data.progress_data.People_Knocked.ToString();
		statistics.Selfie_Made.text = menu.data.Photo_id.ToString();
		statistics.Danced.text = menu.data.progress_data.Danced.ToString();
		statistics.Fucked_Toy.text = menu.data.progress_data.Fucked_Toy.ToString();
	}

	public void Set_Phone_Achieves()
	{
		Tasks_Achieve_Prefab.gameObject.SetActive(value: false);
		Progress_Achieve_Prefab.gameObject.SetActive(value: false);
		Set_Phone_Progress();
		Set_Phone_Statistics();
		if (!Achieves_Instantiated)
		{
			Interior_Achieves_UI = new Interior_Achieve_Progress_UI[menu.data.progress_data.Interior_Achieves.Length];
			Street_Achieves_UI = new Street_Achieve_Progress_UI[menu.data.progress_data.Street_Achieves.Length];
			Achieves_Instantiated = true;
		}
		for (int i = 0; i < Interior_Achieves_UI.Length; i++)
		{
			if (!Interior_Achieves_UI[i].Parent)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(Progress_Achieve_Prefab.gameObject, Achieves_Folder);
				Interior_Achieves_UI[i].Parent = gameObject.transform;
				gameObject.SetActive(value: true);
			}
			Transform child = Interior_Achieves_UI[i].Parent.GetChild(1);
			Transform child2 = Interior_Achieves_UI[i].Parent.GetChild(2);
			Transform child3 = Interior_Achieves_UI[i].Parent.GetChild(3);
			Transform child4 = Interior_Achieves_UI[i].Parent.GetChild(4);
			int progress = menu.data.progress_data.Interior_Achieves[i].Progress;
			for (int j = 0; j < child4.childCount; j++)
			{
				Transform child5 = child4.GetChild(j);
				child5.GetChild(1).GetComponent<Image>().color = ((progress >= j) ? new Color(1f, 1f, 1f) : new Color(0.5f, 0.5f, 0.5f));
				child5.GetChild(1).GetComponent<Image>().sprite = menu.hidden_data.Interior_Settings[i].Picture;
				child5.GetChild(2).GetComponent<Text>().text = menu.data.progress_data.Interior_Achieve_Money[j].ToString();
				child5.GetChild(4).gameObject.SetActive(j < child4.childCount - 1);
				child3.GetChild(j).GetChild(0).GetComponent<Image>()
					.color = ((progress > j) ? new Color(0f, 1f, 0f) : new Color(0.35f, 0.35f, 0.35f));
			}
			child.name = menu.data.progress_data.Interior_Achieves[i].Name;
			child.GetComponent<Text>().text = menu.data.progress_data.Interior_Achieves[i].Name_Achieve[menu.data.Language];
			child2.GetComponent<Text>().text = progress + "/" + child4.childCount;
		}
		for (int k = 0; k < Street_Achieves_UI.Length; k++)
		{
			if (!Street_Achieves_UI[k].Parent)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(Tasks_Achieve_Prefab.gameObject, Achieves_Folder);
				Street_Achieves_UI[k].Parent = gameObject2.transform;
				gameObject2.SetActive(value: true);
			}
			Transform child6 = Street_Achieves_UI[k].Parent.GetChild(1);
			Transform child7 = Street_Achieves_UI[k].Parent.GetChild(2);
			Transform child8 = Street_Achieves_UI[k].Parent.GetChild(3);
			Transform child9 = Street_Achieves_UI[k].Parent.GetChild(4);
			for (int l = 0; l < child9.childCount; l++)
			{
				Transform child10 = child9.GetChild(l);
				child10.GetChild(1).GetComponent<Image>().color = (menu.data.progress_data.Street_Achieves[k].Tasks[l].Completed ? new Color(1f, 1f, 1f) : new Color(0.5f, 0.5f, 0.5f));
				child10.GetChild(2).GetComponent<Text>().text = menu.data.progress_data.Street_Achieves[k].Tasks[l].Name;
				child10.GetChild(3).GetComponent<Image>().color = (menu.data.progress_data.Street_Achieves[k].Tasks[l].Completed ? new Color(0f, 1f, 0f) : new Color(1f, 1f, 1f, 0.35f));
			}
			child6.name = menu.data.progress_data.Street_Achieves[k].Name;
			child6.GetComponent<Text>().text = menu.data.progress_data.Street_Achieves[k].Name_Achieve[menu.data.Language];
			child8.GetChild(1).GetComponent<Text>().text = menu.data.progress_data.Street_Achieves[k].Complete_Reward.ToString();
			child7.GetComponent<Text>().text = menu.data.progress_data.Street_Achieves[k].Tasks_Completed + "/" + child9.childCount;
		}
	}

	public void Check_For_messages()
	{
		if (!Have_New_Money && !Have_New_Achieve && !Have_New_Call && !Have_New_Rank)
		{
			Choose_Window(Main_Screen);
		}
		if (Have_New_Rank)
		{
			Choose_Window(New_Rank);
		}
		if (Have_New_Money)
		{
			Choose_Window(Money_Message);
		}
		if (Have_New_Achieve)
		{
			Choose_Window(New_Achieve);
		}
		if (Have_New_Call)
		{
			Choose_Window(Call);
		}
		if (Open_Shop)
		{
			Choose_Window(Shop);
			Open_Shop = false;
		}
		Have_New_Rank = false;
		Have_New_Money = false;
		Have_New_Achieve = false;
		Have_New_Call = false;
	}

	public void Choose_Shop()
	{
		Open_Shop = true;
	}

	public void Point_Map(bool press)
	{
		Pointer_On_Map = press;
		player.cameras.Cam_Control.isFreezed = press;
	}

	public void Turn_Ratio()
	{
		Vertical = !Vertical;
		Set_Ratio();
	}

	public void Set_Ratio()
	{
		if (Vertical)
		{
			Phone_Model.localScale = new Vector3(1f, 1f, 1f);
			Phone_Model.localRotation = Quaternion.Euler(0f, 0f, 0f);
			Phone_Model.localPosition = new Vector3(0f, 0f, 0f);
			for (int i = 0; i < Education_Parent.childCount; i++)
			{
				Education_Parent.GetChild(i).GetComponent<RectTransform>().localScale = new Vector3(0.6f, 0.6f, 1f);
			}
		}
		else
		{
			Phone_Model.localScale = new Vector3(1.78f, 1.78f, 1f);
			Phone_Model.localEulerAngles = new Vector3(0f, 0f, -90f);
			Phone_Model.localPosition = new Vector3(660f, 0f, 0f);
			for (int j = 0; j < Education_Parent.childCount; j++)
			{
				Education_Parent.GetChild(j).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
			}
		}
		for (int k = 0; k < Windows.Length; k++)
		{
			if (k == 7)
			{
				Windows[k].anchoredPosition = new Vector3(0f, 17f, 0f);
			}
			else
			{
				Windows[k].anchoredPosition = (Vertical ? Vertical_Aspect.anchoredPosition : Horizontal_Aspect.anchoredPosition);
			}
			Windows[k].sizeDelta = (Vertical ? Vertical_Aspect.sizeDelta : Horizontal_Aspect.sizeDelta);
			Windows[k].localRotation = (Vertical ? Vertical_Aspect.localRotation : Horizontal_Aspect.localRotation);
		}
		Main_Screen.sizeDelta = (Vertical ? new Vector2(876f, 1420f) : new Vector2(1420f, 876f));
		Main_Screen.localRotation = (Vertical ? Vertical_Aspect.localRotation : Horizontal_Aspect.localRotation);
		Map_Screen.sizeDelta = (Vertical ? new Vector2(876f, 1420f) : new Vector2(1420f, 876f));
		Map_Screen.localRotation = (Vertical ? Vertical_Aspect.localRotation : Horizontal_Aspect.localRotation);
	}

	public void Choose_Window(Transform window)
	{
		if (!menu)
		{
			menu = UnityEngine.Object.FindObjectOfType<Start_Menu>();
		}
		if (menu.Menu)
		{
			menu.Rox_Animator.SetTrigger("Click");
		}
		Close_All_Except(window);
		Return_Button.gameObject.SetActive(window != Main_Screen);
		if (!menu.Menu && player.Loader.level == 2 && player.Map)
		{
			player.Turn_Map_Mode();
			Map_Screen.gameObject.SetActive(value: false);
		}
	}

	public void Close_All_Except(Transform Window)
	{
		for (int i = 0; i < Windows.Length; i++)
		{
			Windows[i].gameObject.SetActive(Windows[i] == Window && !Window.gameObject.activeSelf);
		}
	}

	public void Show_Achieve_Earned(Sprite sprite, string str)
	{
		Achieve_Image.sprite = sprite;
		Achieve_Text.text = str;
		Have_New_Achieve = true;
	}

	public void Check_Shop_Objects()
	{
		Buy_Button[] componentsInChildren = Haircuts_Folder.GetComponentsInChildren<Buy_Button>(includeInactive: true);
		Buy_Button[] componentsInChildren2 = Cloth_Folder.GetComponentsInChildren<Buy_Button>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(!menu.data.items.Haircuts_Bought[componentsInChildren[i].id]);
		}
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].gameObject.SetActive(!menu.data.Clothes[componentsInChildren2[j].id].Spawned_Cloth[componentsInChildren2[j].variant].Bought);
		}
	}

	public void Buy_Thing(Buy_Button Item)
	{
		menu.data.money.Remain_Atm_Balance -= Item.Price;
		Recount_Money();
		if (Item.Haircut)
		{
			menu.data.items.Haircuts_Bought[Item.id] = true;
			if (menu.Menu)
			{
				menu.edit_base.Check_Edit_Haircuts();
			}
			if (!menu.Menu)
			{
				player.Buy_Cloth_Happiness();
			}
			menu.Item_Added(Item);
		}
		if (Item.Stuff)
		{
			if (Item.Price > 25)
			{
				menu.data.items.Remain_Tools[Item.id] += 3;
			}
			else
			{
				menu.data.items.Remain_Tools[Item.id]++;
			}
			menu.Item_Added(Item);
			menu.Recount_Tools();
		}
		if (Item.Cloth)
		{
			menu.data.Clothes[Item.id].Spawned_Cloth[Item.variant].Bought = true;
			menu.Item_Added(Item);
			if (!menu.Menu)
			{
				player.Buy_Cloth_Happiness();
				player.inventory.Clothes[Item.id].wardrobe_button[Item.variant].Bought = true;
				player.inventory.Clothes[Item.id].wardrobe_button[Item.variant].Reactivate_Button();
			}
		}
		Check_Shop_Objects();
		audio.PlayOneShot(Buy_Sound);
	}

	public void Money_Added(int count)
	{
		menu.data.money.Remain_Atm_Balance += count;
		Money_Added_Text.text = count.ToString();
		Money_Remain_Text.text = menu.data.money.Remain_Atm_Balance.ToString();
		Recount_Money();
		Have_New_Money = true;
		menu.data.progress_data.Money_Earned += count;
	}

	public void Recount_Money()
	{
		int num = menu.data.money.Remain_Atm_Balance + menu.data.money.Remain_Money;
		Money_Balance_Text.text = menu.data.money.Remain_Atm_Balance.ToString();
		Shop_Money_Balance.text = menu.data.money.Remain_Atm_Balance.ToString();
		Money_Cash_Text.text = menu.data.money.Remain_Money.ToString();
		Money_Summary_Text.text = num.ToString();
	}

	public void Turn_Photo_Mode()
	{
		player.Turn_Photo_Mode();
	}

	public void Turn_Map_Mode()
	{
		player.Turn_Map_Mode();
		Map_Screen.gameObject.SetActive(value: true);
		Return_Button.gameObject.SetActive(value: true);
	}

	public void Turn_Gallery()
	{
		screenshot.Turn_Gallery();
		Return_Button.gameObject.SetActive(value: true);
	}

	public void Change_Wallpaper()
	{
		edit.OpenPhoneFileBrowser();
	}

	public void Open_Photo_Folder()
	{
		screenshot.Open_Photo_Folder();
	}
}
