using System.Collections;
using UnityEngine;

public class Switcher_Script : MonoBehaviour
{
	private Roxanne_Control rox;

	public AudioClip switch_sound;

	public Renderer lamp;

	public Color Start_Color;

	private bool Off;

	private Usable_Object Use;

	private void Start()
	{
		rox = Object.FindObjectOfType<Roxanne_Control>();
		if ((bool)lamp)
		{
			Start_Color = lamp.material.GetColor("_EmissionColor");
		}
		Use = GetComponent<Usable_Object>();
		if ((bool)Use)
		{
			Change_Usable_Text();
		}
	}

	public void Switch_Light(GameObject Light_Obj)
	{
		StartCoroutine(Light_Delay(Light_Obj));
	}

	public void Start_Return_Trigger()
	{
		StartCoroutine(Return_Trigger());
	}

	public void Sit_Masturbation()
	{
		rox.Change_Masturbation_Pose(4);
	}

	public void Sit_Masturbation_second()
	{
		rox.Change_Masturbation_Pose(6);
	}

	public void Stop_Masturbation()
	{
		rox.Stop_Masturbating();
	}

	private IEnumerator Return_Trigger()
	{
		GetComponent<Collider>().enabled = false;
		yield return new WaitForSeconds(2f);
		GetComponent<Collider>().enabled = true;
	}

	private IEnumerator Light_Delay(GameObject Light_Obj)
	{
		yield return new WaitForSeconds(0.4f);
		Off = !Off;
		GetComponent<AudioSource>().PlayOneShot(switch_sound);
		Light_Obj.SetActive(!Light_Obj.activeInHierarchy);
		if ((bool)lamp)
		{
			if (Light_Obj.activeInHierarchy)
			{
				lamp.material.SetColor("_EmissionColor", Start_Color);
			}
			else
			{
				lamp.material.SetColor("_EmissionColor", Color.black);
			}
		}
		if ((bool)Use)
		{
			Change_Usable_Text();
		}
	}

	private void Change_Usable_Text()
	{
		if (!Off)
		{
			Use.Text_Actions[0] = "Выключить свет";
			Use.Text_Actions[1] = "Turn off light";
		}
		else
		{
			Use.Text_Actions[0] = "Включить свет";
			Use.Text_Actions[1] = "Turn on light";
		}
	}
}
