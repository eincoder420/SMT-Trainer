using Invector;
using UnityEngine;

public class Dance_Place : MonoBehaviour
{
	public int id;

	public bool Interior_Spot;

	public int Interior_id;

	private Collider trig;

	private Roxanne_Control player;

	private House_Transfer Transferer;

	private Mission_Explorer mission_Explorer;

	[HideInInspector]
	public int Requied_Level;

	public string[] Task_Names;

	public bool Have_Task;

	public bool Money;

	public int Reward;

	private vRotateObject Icon;

	private Transform Yena;

	private void Start()
	{
		trig = GetComponent<Collider>();
		player = Object.FindObjectOfType<Roxanne_Control>();
		Transferer = Object.FindObjectOfType<House_Transfer>();
		mission_Explorer = Object.FindObjectOfType<Mission_Explorer>();
		Icon = GetComponentInChildren<vRotateObject>(includeInactive: true);
		if ((bool)Icon)
		{
			Icon.transform.localScale = new Vector3(0.3f, 0.03f, 0.3f);
		}
		if (!Money)
		{
			Reward = 20 + mission_Explorer.Cloth_Exp();
		}
		else
		{
			Reward = 50 + mission_Explorer.Cloth_Exp();
			Yena = Icon.transform.GetChild(0);
		}
		if (Task_Names.Length == 0)
		{
			Task_Names = new string[2];
			Task_Names[0] = "СТРИПТИЗ НА ПУБЛИКЕ";
			Task_Names[1] = "Public striptease";
		}
		Set_Dance_Place_Color();
	}

	public void Set_Dance_Place_Color()
	{
		if (mission_Explorer.data.saved_data.Dance_Places.Used[id])
		{
			Used_Place();
		}
		else if ((bool)Icon && Requied_Level > player.inventory.data.progress_data.Jerk_Level)
		{
			Icon.GetComponent<Renderer>().material = mission_Explorer.Blocked_Material;
		}
		else
		{
			Icon.GetComponent<Renderer>().material = mission_Explorer.Standart_Material;
		}
		Icon.Set_Light_Color();
	}

	public void Used_Place()
	{
		if ((bool)Icon)
		{
			Icon.GetComponent<Renderer>().material = mission_Explorer.Used_Material;
		}
		Have_Task = false;
		if (Money)
		{
			Yena.gameObject.SetActive(value: false);
		}
		mission_Explorer.data.saved_data.Dance_Places.Used[id] = true;
	}

	public void Go_To_Dance_Place()
	{
		player.Current_Dance_Place = this;
		player.Inside_Dance_Place = true;
		player.Release_Place_Action(2);
		trig.enabled = false;
		Transferer.Tip_Jerk.enabled = true;
		Transferer.Tip_Jerk_Text.enabled = true;
		player.interface_script.Rox_Interface.Button_Open.gameObject.SetActive(value: false);
		player.interface_script.Rox_Interface.Jerk_Task_Object.gameObject.SetActive(value: true);
		if (Have_Task)
		{
			Release_Task();
			return;
		}
		mission_Explorer.Dance_Task_Start(Task_Names[player.inventory.data.Language]);
		player.Speak(mission_Explorer.player.Already_Dance);
	}

	public void Go_Out_From_Dance_Place()
	{
		player.Current_Dance_Place = null;
		player.Inside_Dance_Place = false;
		player.Release_Place_Action(2);
		trig.enabled = true;
		Transferer.Tip_Jerk.enabled = false;
		Transferer.Tip_Jerk_Text.enabled = false;
		player.interface_script.Rox_Interface.Jerk_Task_Object.gameObject.SetActive(value: false);
	}

	private void Release_Task()
	{
		string text = "НАГРАДА";
		if (!Money)
		{
			Reward = 20 + mission_Explorer.Cloth_Exp();
		}
		else
		{
			Reward = 50 + mission_Explorer.Cloth_Exp();
		}
		if (Money)
		{
			if (player.inventory.data.Language == 0)
			{
				text = ". НАГРАДА - " + Reward + " ЙЕН";
			}
			if (player.inventory.data.Language == 1)
			{
				text = ". REWARD - " + Reward + " YEN";
			}
		}
		else
		{
			if (player.inventory.data.Language == 0)
			{
				text = ". НАГРАДА - " + Reward + " ОПЫТА";
			}
			if (player.inventory.data.Language == 1)
			{
				text = ". REWARD - " + Reward + " EXP";
			}
		}
		mission_Explorer.Dance_Task_Start(Task_Names[player.inventory.data.Language] + mission_Explorer.Name_Cloth() + text);
	}

	public void Earn_Experience()
	{
		mission_Explorer.Remain_Experience = Reward;
		mission_Explorer.Earning_Experience = true;
	}
}
