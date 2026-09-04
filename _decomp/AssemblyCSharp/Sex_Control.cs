using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Sex_Control : MonoBehaviour
{
	private Animator sex_anim;

	public Animator Interface_anim;

	private Mission_Explorer mission_Explorer;

	public int Increase_Speed;

	public int Decrease_Speed;

	public float Girl_Orgasm;

	private int Male_Orgasm;

	public Slider Girl_Orgasm_Slider;

	public Text Girl_Orgasm_Text;

	public int Requied_Pose;

	public int Requied_Speed;

	public int Requied_Place;

	public bool Orgasm_Process;

	public Transform Male_Camera;

	public Transform Free_Camera;

	public Text Pose_Text;

	public Text Place_Text;

	public Text Speed_Text;

	public int Current_Pose;

	public int Current_Place;

	public int Current_Speed;

	public float Speed;

	public float Speed_Multiplier;

	public int[] Poses_Doggy;

	public int[] Poses_Mis;

	public int[] Poses_Cow;

	public string[] Names_Poses;

	public string[] Names_Places;

	public string[] Names_Speed;

	public Text Current_Pose_Text;

	public Text Current_Place_Text;

	public Text Current_Speed_Text;

	public Image Pleasure_Color;

	public bool Satisfaction;

	public Speech[] Fuck_Speeches;

	public Speech[] Pose_Speeches;

	public Transform Sex_Interface;

	private void Start()
	{
		sex_anim = GetComponentInChildren<Animator>();
		mission_Explorer = Object.FindObjectOfType<Mission_Explorer>();
		Girl_Orgasm_Slider.value = Girl_Orgasm;
		Girl_Orgasm_Text.text = Girl_Orgasm.ToString();
		Requied_Pose = 1;
		Requied_Place = 0;
		Requied_Speed = 1;
		Current_Pose = 1;
		Current_Place = 0;
		Speed = 1f;
		Speed_Multiplier = 1f;
		sex_anim.SetFloat("Speed", Speed);
		sex_anim.SetInteger("Pose", Current_Pose);
		Set_Sex_Text();
		Check_Speed_Text();
		StartCoroutine(Random_Change_Pose());
		StartCoroutine(Random_Change_Speed());
	}

	public void Change_Girl_Pleasure(float Value)
	{
		Girl_Orgasm += Value;
		Girl_Orgasm_Slider.value = Girl_Orgasm;
		Girl_Orgasm_Text.text = Girl_Orgasm + " / 100";
		Girl_Orgasm = Mathf.Clamp(Girl_Orgasm, 0f, 100f);
		if (Value > 0f)
		{
			if (Speed_Multiplier == 1f)
			{
				Pleasure_Color.color = new Color(0f, 1f, 0f);
			}
			if (Speed_Multiplier == 2f)
			{
				Pleasure_Color.color = new Color(0.6f, 1f, 0.6f);
			}
		}
		else
		{
			Pleasure_Color.color = new Color(1f, 0f, 0f);
		}
		if (Girl_Orgasm >= 100f)
		{
			Girl_Orgasm = 0f;
			sex_anim.SetTrigger("Orgasm");
			StartCoroutine(Delay_Orgasm());
		}
	}

	private void Update()
	{
		Satisfaction = Current_Pose == Requied_Pose && Current_Place == Requied_Place;
		if (!sex_anim.GetBool("Switch_Pose"))
		{
			if (Satisfaction)
			{
				if (!Orgasm_Process)
				{
					Change_Girl_Pleasure(Time.deltaTime * (float)Increase_Speed * Speed_Multiplier);
				}
				else
				{
					Pleasure_Color.color = new Color(1f, 1f, 1f);
				}
			}
			else if (Girl_Orgasm > 0f)
			{
				Change_Girl_Pleasure((0f - Time.deltaTime) * (float)Decrease_Speed);
			}
			else
			{
				Girl_Orgasm = 0f;
			}
		}
		else
		{
			Pleasure_Color.color = new Color(1f, 1f, 0f);
		}
		if (Input.GetMouseButton(1))
		{
			Speed += Time.deltaTime * 0.5f;
		}
		else
		{
			Speed -= Time.deltaTime;
		}
		if (Requied_Place == 2 && Orgasm_Process)
		{
			Sex_Interface.gameObject.SetActive(value: false);
		}
		sex_anim.SetFloat("Speed", Speed);
		Check_Speed_Text();
		Speed = Mathf.Clamp(Speed, 0.8f, 3f);
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			Change_Sex_Pose(1);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			Change_Sex_Pose(2);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			Change_Sex_Pose(3);
		}
		if (Input.GetKeyDown(KeyCode.J))
		{
			Change_Place(0);
		}
		if (Input.GetKeyDown(KeyCode.K))
		{
			Change_Place(1);
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			Change_Place(2);
		}
	}

	public void Random_Update_Requied_Pose()
	{
		int requied_Pose = Requied_Pose;
		int num = Random.Range(0, 2);
		if (requied_Pose == 1)
		{
			Requied_Pose = Poses_Doggy[num];
		}
		if (requied_Pose == 2)
		{
			Requied_Pose = Poses_Mis[num];
		}
		if (requied_Pose == 3)
		{
			Requied_Pose = Poses_Cow[num];
		}
		Set_Sex_Text();
		if (Current_Pose != Requied_Pose)
		{
			mission_Explorer.player.Speak(Pose_Speeches[Requied_Pose]);
		}
	}

	public void Random_Update_Requied_Speed()
	{
		Requied_Speed = Random.Range(0, 3);
		Set_Sex_Text();
	}

	private void Set_Sex_Text()
	{
		Pose_Text.text = Names_Poses[Requied_Pose] + " pose";
		if (Requied_Place <= 2)
		{
			Place_Text.text = "In " + Names_Places[Requied_Place];
		}
		Speed_Text.text = "At " + Names_Speed[Requied_Speed] + " speed";
		Current_Pose_Text.text = Names_Poses[Current_Pose];
		Current_Place_Text.text = Names_Places[Current_Place];
		Check_Speed_Text();
	}

	public void Check_Speed_Text()
	{
		if (Speed < 1f)
		{
			Current_Speed_Text.text = Names_Speed[0];
			Current_Speed = 0;
		}
		if (Speed >= 1f && Speed < 2.5f)
		{
			Current_Speed_Text.text = Names_Speed[1];
			Current_Speed = 1;
		}
		if (Speed >= 2.5f)
		{
			Current_Speed_Text.text = Names_Speed[2];
			Current_Speed = 2;
		}
		if (Requied_Speed == Current_Speed)
		{
			Speed_Multiplier = 2f;
		}
		else
		{
			Speed_Multiplier = 1f;
		}
	}

	public void Change_Place(int id)
	{
		if (id != Current_Place)
		{
			Interface_anim.Play("Darkness_Sex");
			StartCoroutine(Transfer_Delay(id));
		}
	}

	private IEnumerator Transfer_Delay(int id)
	{
		yield return new WaitForSeconds(1f);
		sex_anim.transform.position = mission_Explorer.sex_missions[0].Sex_Places[id].transform.position;
		sex_anim.transform.rotation = mission_Explorer.sex_missions[0].Sex_Places[id].transform.rotation;
		Current_Place = id;
		Set_Sex_Text();
	}

	public void Complete_Sex()
	{
		mission_Explorer.Complete_Sex_Mission();
		Requied_Place = mission_Explorer.mission().current;
		Set_Sex_Text();
		mission_Explorer.player.Speak(Fuck_Speeches[mission_Explorer.mission().current]);
	}

	private IEnumerator Delay_Orgasm()
	{
		Orgasm_Process = true;
		yield return new WaitForSeconds(7f);
		Orgasm_Process = false;
		Girl_Orgasm_Slider.value = Girl_Orgasm;
		Girl_Orgasm_Text.text = Girl_Orgasm + " / 100";
		Complete_Sex();
	}

	public void Change_Camera(bool Free)
	{
		Male_Camera.gameObject.SetActive(!Free);
		Free_Camera.gameObject.SetActive(Free);
	}

	public void Change_Sex_Pose(int id)
	{
		if (id != Current_Pose)
		{
			if (id == 1)
			{
				sex_anim.SetTrigger("Pose_1");
			}
			if (id == 2)
			{
				sex_anim.SetTrigger("Pose_2");
			}
			if (id == 3)
			{
				sex_anim.SetTrigger("Pose_3");
			}
			sex_anim.SetInteger("Pose", id);
			Current_Pose = id;
			Set_Sex_Text();
		}
	}

	private IEnumerator Random_Change_Pose()
	{
		while (base.gameObject.activeInHierarchy)
		{
			int num = Random.Range(20, 25);
			yield return new WaitForSeconds(num);
			if (!Orgasm_Process && !mission_Explorer.player.Speaking)
			{
				Random_Update_Requied_Pose();
			}
		}
	}

	private IEnumerator Random_Change_Speed()
	{
		while (base.gameObject.activeInHierarchy)
		{
			int num = Random.Range(10, 15);
			yield return new WaitForSeconds(num);
			Random_Update_Requied_Speed();
		}
	}
}
