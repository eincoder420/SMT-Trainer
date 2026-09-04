using System.Collections;
using Invector;
using UnityEngine;

public class House_Place : MonoBehaviour
{
	public int Id;

	private House_Transfer Transferer;

	public Transform Out_Pos;

	public bool Have_Task;

	public bool Monetize;

	public Speech Already_Jerk_Speech;

	public Jerk_Place[] Additional_Places;

	public bool Have_Additional;

	private Transform Jerk_Icon;

	private Interior_Place interiors;

	public AudioClip Place_Music;

	private void Start()
	{
		interiors = Object.FindObjectOfType<Interior_Place>();
		Jerk_Icon = GetComponentInChildren<vRotateObject>(includeInactive: true).transform;
		if ((bool)Jerk_Icon)
		{
			Jerk_Icon.transform.localScale = new Vector3(0.35f, 0.035f, 0.35f);
		}
		if (!Transferer)
		{
			Transferer = Object.FindObjectOfType<House_Transfer>();
		}
	}

	public void Go_To_Place()
	{
		if (interiors.Check_For_Rank(Id))
		{
			if (!Transferer)
			{
				Transferer = Object.FindObjectOfType<House_Transfer>();
			}
			Transferer.Jerk_Animator.Play("In_House");
			Transferer.Tip_Jerk.enabled = false;
			Transferer.Tip_Jerk_Text.enabled = false;
			Transferer.Transfering = true;
			StartCoroutine(Delay_In());
		}
		else
		{
			interiors.Low_Rank(Id);
		}
	}

	public void Go_Out_From_Place()
	{
		if (!Transferer)
		{
			Transferer = Object.FindObjectOfType<House_Transfer>();
		}
		Transferer.Jerk_Animator.Play("Out_House");
		StartCoroutine(Delay_Out());
	}

	private int Matched_Interior()
	{
		int result = 0;
		for (int i = 0; i < interiors.interiors.Length; i++)
		{
			if (Id == interiors.interiors[i].id)
			{
				result = i;
			}
		}
		return result;
	}

	private IEnumerator Delay_In()
	{
		yield return new WaitForSeconds(0.5f);
		Transferer.GetComponent<Inventory_Script>().Check_Nake_Level();
		interiors.interiors[Matched_Interior()].gameObject.SetActive(value: true);
		Transferer.transform.position = interiors.interiors[Matched_Interior()].Place.position;
		Transferer.transform.rotation = interiors.interiors[Matched_Interior()].Place.rotation;
		Transferer.Transfering = false;
		Transferer.GetComponent<Roxanne_Control>().Watchers_Count = 0;
		if ((bool)Place_Music)
		{
			interiors.Loader.Set_Audio(Place_Music);
		}
		Mission_Explorer mission_Explorer = Object.FindObjectOfType<Mission_Explorer>();
		mission_Explorer.Out_Of_Mission_Building(Outside: false);
		mission_Explorer.Complete_Go_Inside_House_Mission(this);
		interiors.Save_Building_Data(Matched_Interior(), Inside: true, Out_Pos.position, Out_Pos.rotation.eulerAngles);
	}

	private IEnumerator Delay_Out()
	{
		yield return new WaitForSeconds(0.5f);
		if (!Transferer)
		{
			Transferer = Object.FindObjectOfType<House_Transfer>();
		}
		Transferer.GetComponent<Inventory_Script>().Check_Nake_Level();
		Transferer.transform.position = interiors.Loader.data.saved_data.Interior_Out_Position;
		Transferer.transform.eulerAngles = interiors.Loader.data.saved_data.Interior_Out_Rotation;
		Transferer.GetComponent<Roxanne_Control>().Watchers_Count = 0;
		if ((bool)Place_Music)
		{
			interiors.Loader.Restore_Audio();
		}
		Object.FindObjectOfType<Mission_Explorer>().Out_Of_Mission_Building(Outside: true);
		interiors.Save_Building_Data(Matched_Interior(), Inside: false, Transferer.transform.position, Transferer.transform.eulerAngles);
	}
}
