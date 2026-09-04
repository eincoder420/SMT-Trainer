using System.Collections;
using Invector.vCharacterController.AI;
using Invector.vCharacterController.AI.FSMBehaviour;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class NPC_generator : MonoBehaviour
{
	[Header("Прочее")]
	public bool Visible;

	public Transform armature;

	public float min_size;

	public float max_size;

	public bool Girl;

	public vControlAI AI;

	private Animator anim;

	private Roxanne_Control player;

	private House_Transfer transferer;

	public bool Seen_Naked_Rox;

	public bool Already_Seen;

	public bool Watcher;

	public int Man_react_id;

	public vFSMBehaviourController Controller;

	public AudioClip Hit_voice;

	public AudioClip Excite_Voice;

	public AudioClip Photo_Shot;

	public AudioClip Tired_Voice;

	public Slider Shame_Slider;

	public Text Name_text;

	private Rigidbody rig;

	private float Shame;

	public string Name;

	private bool Embarassed_NPC_Counted;

	public RotationConstraint Cam_Constraint;

	public Transform Icon;

	public Transform Speech_Collider;

	public AudioClip[] Speeches;

	public bool In_Dialogue_Radius;

	public bool Speaking;

	private bool Shame_Reason;

	private bool Naked_Man;

	public bool Naked;

	public Transform[] Stuff;

	public bool Drinker;

	public bool Smoker;

	public bool Speaker;

	public bool Presser;

	public bool Force_Stopped;

	[Header("Особые мобы")]
	public bool Swimmer;

	public bool Seller;

	public bool Neigbor;

	public bool Neighbor_Usual;

	public bool Cop;

	public bool Mom;

	public Transform Neighbor_Cloth;

	public Transform Mom_Cloth;

	public Transform Swim_Suit;

	public Transform Bage;

	[Header("Стандартный прикид")]
	public int Chosen_Shirt;

	public int Chosen_Pants;

	public int Chosen_Hairs;

	public Transform Chosen_Folder;

	public Transform Cloth;

	public Transform Hairs;

	public Transform[] Shirts_0;

	public Transform[] Pants_0;

	public Transform[] Shoes_0;

	[Header("Менты")]
	public int Remain_Energy;

	public bool Restoring_Energy;

	public Transform Cop_Hand_L;

	public Transform Cop_Hand_R;

	public Transform Cop_Arrest_Position;

	public bool Arrested;

	public string cop_arrest_speech;

	public bool Pointed;

	[Header("Сидение")]
	public bool Moving_To_Sit;

	public Transform Sit_Target;

	public Vector3 Before_Sit_Position;

	public void Make_Naked()
	{
		Naked = true;
		Turn_Clothes();
	}

	public void Turn_Clothes()
	{
		bool flag = !Mom && !Neigbor && !Naked && !Swimmer;
		Chosen_Shirt = 0;
		Chosen_Pants = 0;
		if (flag)
		{
			Chosen_Shirt = Random.Range(0, Shirts_0.Length);
			Chosen_Pants = Random.Range(0, Pants_0.Length);
		}
		for (int i = 0; i < Shirts_0.Length; i++)
		{
			Shirts_0[i].gameObject.SetActive(flag && Chosen_Shirt == i);
		}
		for (int j = 0; j < Pants_0.Length; j++)
		{
			Pants_0[j].gameObject.SetActive(flag && Chosen_Pants == j);
		}
		for (int k = 0; k < Shoes_0.Length; k++)
		{
			Shoes_0[k].gameObject.SetActive(flag);
		}
		Shirts_0[Chosen_Shirt].transform.parent = Chosen_Folder;
		Pants_0[Chosen_Pants].transform.parent = Chosen_Folder;
		Shoes_0[0].transform.parent = Chosen_Folder;
		if (Swimmer)
		{
			Swim_Suit.transform.parent = Chosen_Folder;
		}
		if (Seller)
		{
			Bage.transform.parent = Chosen_Folder;
		}
	}

	public void Dress_Undress_Mom(bool On)
	{
		Naked = On;
	}

	public void Make_Swimmer()
	{
		Turn_Clothes();
		Swim_Suit.gameObject.SetActive(value: true);
		Swimmer = true;
	}

	public void Make_Mom()
	{
		Turn_Clothes();
		for (int i = 0; i < Mom_Cloth.childCount; i++)
		{
			if (i == 0 || i == 1)
			{
				Mom_Cloth.GetChild(i).gameObject.SetActive(value: true);
			}
		}
	}

	public void Make_Usual()
	{
		Turn_Clothes();
		if (!Neigbor && !Girl)
		{
			Object.Destroy(Neighbor_Cloth.gameObject);
		}
		if (!Mom && Girl)
		{
			Object.Destroy(Mom_Cloth.gameObject);
		}
	}

	public void Turn_Dialogue_Radius(bool On)
	{
		In_Dialogue_Radius = On;
		if (On)
		{
			player.Opponent = this;
		}
	}

	public void Check_For_Dialogue()
	{
		if (In_Dialogue_Radius && !AI.ragdolled)
		{
			Dialogue();
		}
	}

	public void FindComponents()
	{
		AI = GetComponent<vControlAI>();
		anim = GetComponent<Animator>();
		Controller = GetComponent<vFSMBehaviourController>();
	}

	public void Turn_Components(bool On)
	{
		Controller.enabled = On;
		AI.enabled = On;
		anim.enabled = On;
	}

	private void Start()
	{
		player = Object.FindObjectOfType<Roxanne_Control>();
		transferer = player.GetComponent<House_Transfer>();
		FindComponents();
		if (!Girl)
		{
			if (!Cop)
			{
				if (player.inventory.data.Language == 0)
				{
					Name = player.inventory.data.Mans_Names[Random.Range(0, player.inventory.data.Mans_Names.Length)];
				}
				if (player.inventory.data.Language == 1)
				{
					Name = player.inventory.data.Mans_Names_1[Random.Range(0, player.inventory.data.Mans_Names_1.Length)];
				}
			}
			else
			{
				Name = player.inventory.data.Cops_Names[Random.Range(0, player.inventory.data.Cops_Names.Length)];
			}
		}
		else if (!Mom)
		{
			if (player.inventory.data.Language == 0)
			{
				Name = player.inventory.data.Girl_Names[Random.Range(0, player.inventory.data.Girl_Names.Length)];
			}
			if (player.inventory.data.Language == 1)
			{
				Name = player.inventory.data.Girl_Names_1[Random.Range(0, player.inventory.data.Girl_Names_1.Length)];
			}
		}
		else
		{
			if (player.inventory.data.Language == 0)
			{
				Name = "Мачеха " + player.inventory.data.Name2;
			}
			if (player.inventory.data.Language == 1)
			{
				Name = player.inventory.data.Name + "'s Stepmom";
			}
		}
		Name_text.text = Name;
		ConstraintSource source = default(ConstraintSource);
		source.sourceTransform = Camera.main.transform;
		source.weight = 1f;
		Cam_Constraint.AddSource(source);
		Cam_Constraint.constraintActive = true;
		if (Cop)
		{
			Remain_Energy = 10;
			StartCoroutine(Energy());
		}
		Check_Sitting();
		Icon.gameObject.SetActive(value: true);
		if (Smoker || Drinker)
		{
			StartCoroutine(Action_Drinkers());
		}
		if (Speaker || Presser)
		{
			StartCoroutine(Action_Speakers());
		}
		if (!Seller)
		{
			Bage.gameObject.SetActive(value: false);
		}
		Make_Usual();
		Random_Choose_Proportions();
		Random_Choose_Colors();
		Random_Choose_Hairstyle();
		Object.Destroy(Cloth.gameObject);
		Object.Destroy(Hairs.gameObject);
	}

	public void Check_Sitting()
	{
		if (AI.Sitting)
		{
			anim.SetTrigger("Sit");
			Turn_Rigidbody(On: false);
		}
		else
		{
			Turn_Rigidbody(On: true);
		}
	}

	private void Random_Choose_Proportions()
	{
		if (!Mom)
		{
			float num = Random.Range(min_size, max_size);
			armature.transform.localScale = new Vector3(num, num, num);
		}
	}

	private void Random_Choose_Hairstyle()
	{
		if (!Mom)
		{
			Chosen_Hairs = Random.Range(0, Hairs.childCount);
		}
		for (int i = 0; i < Hairs.childCount; i++)
		{
			Hairs.GetChild(i).gameObject.SetActive(i == Chosen_Hairs);
		}
		Hairs.GetChild(Chosen_Hairs).transform.parent = Chosen_Folder;
	}

	private void Random_Choose_Colors()
	{
		if (!Cop && !Mom)
		{
			Renderer component = Shirts_0[Chosen_Shirt].GetComponent<Renderer>();
			Renderer component2 = Pants_0[Chosen_Pants].GetComponent<Renderer>();
			Material material = component.material;
			Material material2 = component2.material;
			if (!Girl)
			{
				int num = Random.Range(0, player.inventory.data.npc_Data.NPC_Man_Shirt_Colors.Length);
				material.color = player.inventory.data.npc_Data.NPC_Man_Shirt_Colors[num];
				int num2 = Random.Range(0, player.inventory.data.npc_Data.NPC_Man_Pants_Colors.Length);
				material2.color = player.inventory.data.npc_Data.NPC_Man_Pants_Colors[num2];
			}
			if (Girl)
			{
				int num3 = Random.Range(0, player.inventory.data.npc_Data.NPC_Girl_Shirt_Colors.Length);
				material.color = player.inventory.data.npc_Data.NPC_Girl_Shirt_Colors[num3];
				int num4 = Random.Range(0, player.inventory.data.npc_Data.NPC_Girl_Pants_Colors.Length);
				material2.color = player.inventory.data.npc_Data.NPC_Girl_Pants_Colors[num4];
			}
			if (Seller)
			{
				material.color = player.inventory.data.npc_Data.Seller_Color;
			}
		}
	}

	private void Move_To_Sit()
	{
		if ((bool)Sit_Target)
		{
			anim.transform.position = Vector3.Lerp(base.transform.position, Sit_Target.transform.position, Time.deltaTime);
		}
		else
		{
			anim.transform.position = Vector3.Lerp(base.transform.position, Before_Sit_Position, Time.deltaTime);
		}
	}

	public void Turn_Rigidbody(bool On)
	{
		anim.GetComponent<Rigidbody>().isKinematic = !On;
	}

	private void Update()
	{
		if (Visible)
		{
			if (Moving_To_Sit)
			{
				Move_To_Sit();
				if (!Sit_Target && Vector3.Distance(base.transform.position, Before_Sit_Position) < 0.1f)
				{
					Turn_Rigidbody(On: true);
					Moving_To_Sit = false;
				}
			}
			if (player.inventory.In_Locker_Room)
			{
				Shame_Reason = player.Masturbating || player.Toy_Mode;
			}
			else if (!Mom)
			{
				if (Girl)
				{
					Shame_Reason = player.Nake_Level >= 2;
				}
				else
				{
					Shame_Reason = player.Nake_Level >= 2 || Naked;
				}
			}
			else
			{
				Shame_Reason = player.Nake_Level >= 2 || Naked;
			}
			if (AI.targetInLineOfSight && Shame_Reason && !AI.ragdolled)
			{
				Naked_Man = !Girl && Naked;
				anim.SetBool("Naked_Man", Naked_Man);
				if (Shame < 100f)
				{
					Increase_Shame();
				}
				else if (!Embarassed_NPC_Counted)
				{
					player.Show_Shamed_People_Counter();
					if (!Girl)
					{
						anim.SetTrigger("Embar");
					}
					Embarassed_NPC_Counted = true;
				}
				if (!Seen_Naked_Rox)
				{
					Play_Reaction_Animation();
					Already_Seen = true;
					Seen_Naked_Rox = true;
				}
				if (!Watcher)
				{
					Show_Watcher_Speech();
					Change_Watcher_Status(Add: true);
				}
			}
			else
			{
				if (Watcher)
				{
					Change_Watcher_Status(Add: false);
				}
				if (Seen_Naked_Rox)
				{
					Seen_Naked_Rox = false;
				}
			}
		}
		else
		{
			if (Watcher)
			{
				Change_Watcher_Status(Add: false);
			}
			if (Already_Seen)
			{
				Seen_Naked_Rox = false;
				Already_Seen = false;
			}
		}
		if (!Cop || !Controller)
		{
			return;
		}
		if (Controller.currentState.Name == "Scream")
		{
			if (!Already_Seen)
			{
				Pointed = false;
			}
			if (!Pointed)
			{
				Cop_Point_Player();
				Pointed = true;
			}
		}
		if (Controller.currentState.Name == "Patrol")
		{
			if (Pointed)
			{
				Pointed = false;
			}
			if (Remain_Energy < 10)
			{
				Remain_Energy = 10;
				anim.SetTrigger("Not_Tired");
				Restoring_Energy = false;
			}
		}
	}

	private void Increase_Shame()
	{
		if (!Naked_Man)
		{
			Shame += Time.deltaTime * 5f;
			Shame_Slider.value = Shame;
			Shame = Mathf.Clamp(Shame, 0f, 100f);
		}
		else
		{
			Shame += Time.deltaTime * 50f;
			Shame_Slider.value = Shame;
			Shame = Mathf.Clamp(Shame, 0f, 100f);
		}
	}

	private void Change_Watcher_Status(bool Add)
	{
		if (Add)
		{
			player.Watchers_Count++;
			if (Mom || Neighbor_Usual)
			{
				player.Known_Watching = true;
			}
			Watcher = true;
		}
		else
		{
			if (player.Watchers_Count > 0)
			{
				player.Watchers_Count--;
			}
			Watcher = false;
		}
	}

	private void Play_Reaction_Animation()
	{
		if (!Girl)
		{
			if (!Naked_Man)
			{
				anim.SetTrigger("See_Man");
				anim.SetInteger("Reaction", Man_react_id);
				GetComponent<AudioSource>().PlayOneShot(Excite_Voice);
			}
		}
		else
		{
			if (Mom && Naked)
			{
				anim.SetTrigger("Shame");
			}
			else
			{
				anim.SetTrigger("See_Girl");
				anim.SetInteger("Reaction", Random.Range(0, 2));
			}
			GetComponent<AudioSource>().PlayOneShot(Excite_Voice);
		}
	}

	private void Show_Watcher_Speech()
	{
		if (!Girl)
		{
			if (!Naked_Man)
			{
				if (player.inventory.data.Language == 0)
				{
					player.interface_script.Print_In_Chat(Name, player.inventory.data.Mans_First_Seen[Random.Range(0, player.inventory.data.Mans_First_Seen.Length)]);
				}
				if (player.inventory.data.Language == 1)
				{
					player.interface_script.Print_In_Chat(Name, player.inventory.data.Mans_First_Seen_1[Random.Range(0, player.inventory.data.Mans_First_Seen.Length)]);
				}
			}
		}
		else
		{
			if (player.inventory.data.Language == 0)
			{
				player.interface_script.Print_In_Chat(Name, player.inventory.data.Girls_First_Seen[Random.Range(0, player.inventory.data.Girls_First_Seen.Length)]);
			}
			if (player.inventory.data.Language == 1)
			{
				player.interface_script.Print_In_Chat(Name, player.inventory.data.Girls_First_Seen_1[Random.Range(0, player.inventory.data.Girls_First_Seen.Length)]);
			}
		}
	}

	public void Cop_Point_Player()
	{
		anim.SetTrigger("Cop_Point");
		GetComponent<AudioSource>().PlayOneShot(Excite_Voice);
		if (!Already_Seen)
		{
			Already_Seen = true;
		}
	}

	public void Hitten()
	{
		GetComponent<Animator>().Play("Reaction");
		anim.SetTrigger("Hitten");
		if (!rig)
		{
			rig = GetComponent<Rigidbody>();
		}
		if (AI.Sitting)
		{
			AI.Sitting = false;
			anim.SetTrigger("Stay");
			Check_Sitting();
		}
		if (!Cop)
		{
			StartCoroutine(Angry_Speech());
			Already_Seen = true;
			if (!Girl)
			{
				GetComponent<Animator>().SetTrigger("Boy_Angry");
			}
			else
			{
				GetComponent<Animator>().SetTrigger("Girl_Angry");
			}
			GetComponent<Animator>().Play("Angry_Talk");
		}
		if (player.inventory.data.Language == 0)
		{
			player.interface_script.Print_In_Chat(Name, player.inventory.data.Hitten_Speech[Random.Range(0, player.inventory.data.Hitten_Speech.Length)]);
		}
		if (player.inventory.data.Language == 1)
		{
			player.interface_script.Print_In_Chat(Name, player.inventory.data.Hitten_Speech_1[Random.Range(0, player.inventory.data.Hitten_Speech.Length)]);
		}
	}

	private IEnumerator Angry_Speech()
	{
		Speaking = true;
		yield return new WaitForSeconds(5f);
		Speaking = false;
	}

	public void Photo_Shoot()
	{
		GetComponent<AudioSource>().PlayOneShot(Photo_Shot);
	}

	public void Cop_Speak_Arrest()
	{
		player.interface_script.Print_In_Chat(Name, cop_arrest_speech);
		Already_Seen = false;
	}

	public void Force_Stop(bool On)
	{
		Force_Stopped = On;
	}

	private IEnumerator Energy()
	{
		while (base.gameObject.activeInHierarchy)
		{
			yield return new WaitForSeconds(1f);
			if (anim.GetFloat("InputMagnitude") > 0.9f && Controller.currentState.Name == "Chase" && !Restoring_Energy)
			{
				if (Remain_Energy >= 1)
				{
					Remain_Energy--;
				}
				else
				{
					anim.SetTrigger("Cop_Tired");
					GetComponent<AudioSource>().PlayOneShot(Tired_Voice);
					Restoring_Energy = true;
				}
			}
			if (Restoring_Energy)
			{
				if (Remain_Energy < 10)
				{
					anim.SetFloat("InputMagnitude", 0f);
					Remain_Energy++;
				}
				else
				{
					if (Controller.currentState.Name == "Chase")
					{
						anim.SetTrigger("Not_Tired");
					}
					Restoring_Energy = false;
				}
			}
			Shame_Slider.maxValue = 10f;
			Shame_Slider.value = Remain_Energy;
		}
	}

	public void Set_Speak_Progress(int id)
	{
		if (Mom)
		{
			player.inventory.data.progress_data.Mom_Progress = id;
		}
	}

	public void Dialogue()
	{
		if (Speaking)
		{
			return;
		}
		if (Mom)
		{
			if (player.inventory.data.progress_data.Mom_Progress != 1)
			{
				player.Speak(player.Talk_Mom[player.inventory.data.progress_data.Mom_Progress]);
				player.anim.SetTrigger("Cant");
			}
			StartCoroutine(Answer(5));
		}
		else
		{
			player.anim.SetTrigger("Cant");
			if (!Girl)
			{
				player.Speak(player.God_Ev_Sir);
			}
			else
			{
				player.Speak(player.Good_Ev_Maam);
			}
			StartCoroutine(Answer(3));
		}
		player.inventory.data.progress_data.People_Talked++;
	}

	public void Speak(int id)
	{
		if (player.inventory.data.Language == 0)
		{
			player.interface_script.Print_In_Chat(Name, player.inventory.data.Mom_Speeches[id]);
		}
		if (player.inventory.data.Language == 1)
		{
			player.interface_script.Print_In_Chat(Name, player.inventory.data.Mom_Speeches_1[id]);
		}
		if (!GetComponent<AudioSource>().isPlaying)
		{
			GetComponent<AudioSource>().PlayOneShot(Speeches[id]);
		}
		GetComponent<Animator>().SetTrigger("Speak");
	}

	private IEnumerator Answer(int delay)
	{
		Speaking = true;
		yield return new WaitForSeconds(delay);
		if (Mom)
		{
			GetComponent<Animator>().SetTrigger("Speak");
			if (player.inventory.data.progress_data.Mom_Progress == 0)
			{
				if (player.inventory.data.Language == 0)
				{
					player.interface_script.Print_In_Chat(Name, player.inventory.data.Mom_Speeches[0]);
				}
				if (player.inventory.data.Language == 1)
				{
					player.interface_script.Print_In_Chat(Name, player.inventory.data.Mom_Speeches_1[0]);
				}
				if (!GetComponent<AudioSource>().isPlaying)
				{
					GetComponent<AudioSource>().PlayOneShot(Speeches[0]);
				}
			}
			if (player.inventory.data.progress_data.Mom_Progress == 1)
			{
				if (player.inventory.data.Language == 0)
				{
					player.interface_script.Print_In_Chat(Name, player.inventory.data.Mom_Speeches[1]);
				}
				if (player.inventory.data.Language == 1)
				{
					player.interface_script.Print_In_Chat(Name, player.inventory.data.Mom_Speeches_1[1]);
				}
				if (!GetComponent<AudioSource>().isPlaying)
				{
					GetComponent<AudioSource>().PlayOneShot(Speeches[1]);
				}
				Speech_Collider.gameObject.SetActive(value: false);
			}
			if (player.inventory.data.progress_data.Mom_Progress == 7)
			{
				if (player.inventory.data.Language == 0)
				{
					player.interface_script.Print_In_Chat(Name, player.inventory.data.Mom_Speeches[7]);
				}
				if (player.inventory.data.Language == 1)
				{
					player.interface_script.Print_In_Chat(Name, player.inventory.data.Mom_Speeches_1[7]);
				}
				if (!GetComponent<AudioSource>().isPlaying)
				{
					GetComponent<AudioSource>().PlayOneShot(Speeches[7]);
				}
				player.mission_Explorer.mission().Scenario_Objects.Turn_By_Script_Objects[0].Object.gameObject.SetActive(value: true);
				Speech_Collider.gameObject.SetActive(value: false);
			}
			player.Close_Dialogue();
			yield return new WaitForSeconds(5f);
			player.mission_Explorer.Complete_Speak_Mission();
			Speaking = false;
			if (player.inventory.data.progress_data.Mom_Progress == 0)
			{
				player.inventory.data.progress_data.Mom_Progress++;
			}
		}
		else
		{
			GetComponent<Animator>().SetTrigger("Speak");
			int num = Random.Range(0, player.inventory.data.Hello_Speeches.Length);
			if (player.inventory.data.Language == 0)
			{
				player.interface_script.Print_In_Chat(Name, player.inventory.data.Hello_Speeches[num]);
			}
			if (player.inventory.data.Language == 1)
			{
				player.interface_script.Print_In_Chat(Name, player.inventory.data.Hello_Speeches_1[num]);
			}
			if (!GetComponent<AudioSource>().isPlaying)
			{
				GetComponent<AudioSource>().PlayOneShot(Speeches[num]);
			}
		}
	}

	private IEnumerator Action_Drinkers()
	{
		while (base.gameObject.activeInHierarchy)
		{
			int num = Random.Range(15, 20);
			Stuff[0].gameObject.SetActive(value: true);
			yield return new WaitForSeconds(num);
			if (Smoker && !Seen_Naked_Rox)
			{
				GetComponent<Animator>().SetTrigger("Smoke_Girl");
			}
			if (Drinker && !Seen_Naked_Rox)
			{
				GetComponent<Animator>().SetTrigger("Drink_Man");
			}
		}
	}

	private IEnumerator Action_Speakers()
	{
		while (base.gameObject.activeInHierarchy)
		{
			int num = Random.Range(5, 8);
			yield return new WaitForSeconds(num);
			if (Speaker && !Seen_Naked_Rox && !Already_Seen)
			{
				GetComponent<Animator>().SetTrigger("Speak");
			}
			if (Presser && !Seen_Naked_Rox && !Already_Seen)
			{
				GetComponent<Animator>().SetTrigger("Push_Button");
			}
		}
	}
}
