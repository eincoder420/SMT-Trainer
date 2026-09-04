using System.Collections;
using Invector;
using UnityEngine;

public class Jerk_Place : MonoBehaviour
{
	public int id;

	public int Requied_Level;

	public int Place_Type;

	public bool Interior_Spot;

	public int Interior_id;

	[HideInInspector]
	public Transform target;

	private Roxanne_Control player;

	private House_Transfer Transferer;

	private Mission_Explorer mission_Explorer;

	private Collider Trig;

	public Rigidbody Object_Rig;

	public Vector3 Start_Pos;

	public int jerk_pose = 4;

	public string[] Task_Names;

	public bool Have_Task;

	public bool Money;

	public int Reward;

	public Jerk_Place[] Additional_Places;

	public bool Have_Additional;

	public bool Completed_Jerk;

	private bool Initiated;

	private vRotateObject Icon;

	private Transform Yena;

	private void Start()
	{
		Init();
		Set_Jerk_Place_Color();
	}

	private void Init()
	{
		Trig = GetComponent<Collider>();
		if (!player)
		{
			player = Object.FindObjectOfType<Roxanne_Control>();
		}
		if (!Transferer)
		{
			Transferer = Object.FindObjectOfType<House_Transfer>();
		}
		if (!mission_Explorer)
		{
			mission_Explorer = Object.FindObjectOfType<Mission_Explorer>();
		}
		Icon = GetComponentInChildren<vRotateObject>(includeInactive: true);
		if ((bool)Icon)
		{
			Icon.transform.localScale = new Vector3(0.3f, 0.03f, 0.3f);
		}
		target = base.transform.GetChild(0);
		mission_Explorer.Check_Spot_For_Available(this);
		if (Task_Names.Length == 0)
		{
			Task_Names = new string[2];
			Task_Names[0] = "МАСТУРБАЦИЯ НА ПУБЛИКЕ";
			Task_Names[1] = "Public masturbation";
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
		Initiated = true;
	}

	public void Set_Jerk_Place_Color()
	{
		if (!Initiated)
		{
			Init();
		}
		bool num = mission_Explorer.data.saved_data.Jerk_Places.Used[id];
		if (!Icon)
		{
			Icon = GetComponentInChildren<vRotateObject>(includeInactive: true);
		}
		if (num)
		{
			Icon.GetComponent<Renderer>().material = mission_Explorer.Used_Material;
			Have_Task = false;
			if (Money)
			{
				Yena.gameObject.SetActive(value: false);
			}
		}
		else if (Requied_Level > player.inventory.data.progress_data.Jerk_Level)
		{
			Icon.GetComponent<Renderer>().material = mission_Explorer.Blocked_Material;
		}
		else
		{
			Icon.GetComponent<Renderer>().material = mission_Explorer.Standart_Material;
		}
		Icon.Set_Light_Color();
	}

	private void Turn_Place_Rigidbody(bool On)
	{
		Object_Rig.isKinematic = !On;
		Object_Rig.GetComponent<Collider>().enabled = On;
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
		mission_Explorer.Jerk_Task_Start(Task_Names[player.inventory.data.Language] + mission_Explorer.Name_Cloth() + text);
		if (Have_Additional)
		{
			for (int i = 0; i < Additional_Places.Length; i++)
			{
				Additional_Places[i].Have_Task = false;
			}
		}
	}

	public void Go_To_Place()
	{
		if (Requied_Level <= player.inventory.data.progress_data.Jerk_Level)
		{
			player.Check_Pose_For_Unwear(jerk_pose);
			if (!player.poses[jerk_pose].No_Requied_Unwear)
			{
				Transferer.Jerk_Animator.Play("In");
				player.Inside_Jerk_Place = true;
				player.Current_Jerk_Place = this;
				Start_Pos = player.transform.position;
				StartCoroutine(Delay_In());
			}
			else
			{
				player.Interface_anim.SetTrigger("Tip");
				player.Show_Requied_Cloth_Tip(jerk_pose);
			}
		}
		else
		{
			player.Speak(mission_Explorer.player.Low_Level);
			player.anim.SetTrigger("Cant");
		}
	}

	private IEnumerator Delay_In()
	{
		yield return new WaitForSeconds(0.5f);
		if ((bool)Icon)
		{
			Icon.GetComponent<Renderer>().enabled = false;
			if ((bool)Icon.GetComponent<Light>())
			{
				Icon.GetComponent<Light>().enabled = false;
			}
		}
		if (Money && (bool)Yena)
		{
			Yena.gameObject.SetActive(value: false);
		}
		if ((bool)Object_Rig)
		{
			Turn_Place_Rigidbody(On: false);
		}
		Trig.enabled = false;
		Transferer.Tip_Jerk.enabled = true;
		Transferer.Tip_Jerk_Text.enabled = true;
		if (Have_Task)
		{
			Release_Task();
		}
		else
		{
			mission_Explorer.Jerk_Task_Start(Task_Names[player.inventory.data.Language]);
			player.Speak(mission_Explorer.player.Already_Jerk);
		}
		player.Change_Masturbation_Pose(jerk_pose);
	}

	public void Used_Place()
	{
		mission_Explorer.data.saved_data.Jerk_Places.Used[id] = true;
	}

	public void Jerk_Task_Completed()
	{
		Completed_Jerk = true;
		Go_Out_From_Jerk_Place();
		mission_Explorer.Complete_Jerk_Mission();
		Reach_Achieve();
	}

	public void Reach_Achieve()
	{
		mission_Explorer.data.progress_data.Masturbated++;
		if (Interior_Spot)
		{
			mission_Explorer.data.progress_data.Interior_Achieves[Interior_id].Tasks_Completed++;
			int progress = mission_Explorer.data.progress_data.Interior_Achieves[Interior_id].Progress;
			int num = 0;
			for (int i = 0; i < mission_Explorer.Loader.interior_Places.interiors.Length; i++)
			{
				if (Interior_id == mission_Explorer.Loader.interior_Places.interiors[i].id)
				{
					num = i;
				}
			}
			int num2 = mission_Explorer.Loader.interior_Places.interiors[num].jerk_places.Length;
			int tasks_Completed = mission_Explorer.data.progress_data.Interior_Achieves[Interior_id].Tasks_Completed;
			if (progress == 1 && tasks_Completed == 1)
			{
				player.Get_Interior_Achieve(Interior_id);
			}
			if (progress == 2 && num2 == tasks_Completed)
			{
				player.Get_Interior_Achieve(Interior_id);
			}
		}
		else if (!mission_Explorer.data.progress_data.Street_Achieves[0].Tasks[Place_Type].Completed)
		{
			mission_Explorer.data.progress_data.Street_Achieves[0].Tasks_Completed++;
			mission_Explorer.data.progress_data.Street_Achieves[0].Tasks[Place_Type].Completed = true;
			if (mission_Explorer.data.progress_data.Street_Achieves[0].Tasks_Completed == mission_Explorer.data.progress_data.Street_Achieves[0].Tasks.Length)
			{
				player.Get_Street_Achieve(0);
			}
		}
	}

	public void Go_Out_From_Jerk_Place()
	{
		Transferer.Jerk_Animator.Play("Out");
		player.interface_script.Rox_Interface.Jerk_Task_Object.gameObject.SetActive(value: false);
		StartCoroutine(Delay_Out());
	}

	private IEnumerator Delay_Out()
	{
		if (Completed_Jerk)
		{
			Used_Place();
			Set_Jerk_Place_Color();
		}
		yield return new WaitForSeconds(0.5f);
		Trig.enabled = true;
		player.Inside_Jerk_Place = false;
		player.Current_Jerk_Place = null;
		player.transform.position = Start_Pos;
		player.Stop_Masturbating();
		Icon.GetComponent<Renderer>().enabled = true;
		if ((bool)Icon.GetComponent<Light>())
		{
			Icon.GetComponent<Light>().enabled = true;
		}
		if (Money && (bool)Yena && Have_Task)
		{
			Yena.gameObject.SetActive(value: true);
		}
		if ((bool)Object_Rig)
		{
			Turn_Place_Rigidbody(On: true);
		}
		player.interface_script.Mission_Rewarded = false;
	}
}
