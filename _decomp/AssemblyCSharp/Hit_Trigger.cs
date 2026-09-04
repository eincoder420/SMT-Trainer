using System.Collections;
using Invector.vCharacterController;
using Invector.vCharacterController.AI;
using UnityEngine;

public class Hit_Trigger : MonoBehaviour
{
	private Animator anim;

	private vRagdoll ragdoll;

	private Roxanne_Control player;

	private vThirdPersonController controller;

	public AudioClip WallHit;

	public AudioClip MetalHit;

	private AudioSource audiosource;

	public float Run_Time;

	private Collider current_col;

	private void Start()
	{
		player = Object.FindObjectOfType<Roxanne_Control>();
		audiosource = GetComponent<AudioSource>();
		anim = player.GetComponent<Animator>();
		ragdoll = player.GetComponent<vRagdoll>();
		controller = player.GetComponent<vThirdPersonController>();
	}

	public void Hitten_By_Car()
	{
		if (!controller.ragdolled)
		{
			player.Stop_All_Actions();
			anim.SetFloat("InputMagnitude", 0f);
			Run_Time = 0f;
			audiosource.volume = 0.5f;
			if (!audiosource.isPlaying)
			{
				audiosource.PlayOneShot(MetalHit);
			}
			ragdoll.horizontalMultiplier = 0.1f;
			ragdoll.verticalMultiplier = 0.1f;
			anim.SetBool("Ragdolled", value: true);
			ragdoll.ActivateRagdoll(null, 2.5f);
			StartCoroutine(Start_Speech());
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.layer == 0 && !controller.ragdolled)
		{
			if (!col.GetComponent<Rigidbody>() && anim.GetFloat("InputMagnitude") > 0.99f && Run_Time > 2.9f && col != current_col)
			{
				player.Stop_All_Actions();
				anim.SetFloat("InputMagnitude", 0f);
				Run_Time = 0f;
				audiosource.volume = 0.5f;
				if (col.tag == "Metal")
				{
					audiosource.PlayOneShot(MetalHit);
				}
				else
				{
					audiosource.PlayOneShot(WallHit);
				}
				ragdoll.horizontalMultiplier = 0.1f;
				ragdoll.verticalMultiplier = 0.1f;
				anim.SetBool("Ragdolled", value: true);
				ragdoll.ActivateRagdoll(null, 2.5f);
				StartCoroutine(Start_Speech());
				current_col = col;
			}
		}
		else if (col.gameObject.layer == 9 && (bool)col.GetComponent<vRagdoll>() && !controller.ragdolled && anim.GetFloat("InputMagnitude") < 0.75f)
		{
			if (!col.GetComponent<AudioSource>().isPlaying)
			{
				col.GetComponent<AudioSource>().PlayOneShot(col.GetComponent<NPC_generator>().Hit_voice);
			}
			col.GetComponent<NPC_generator>().Hitten();
		}
	}

	private void OnTriggerExit(Collider col)
	{
		if (current_col == col)
		{
			current_col = null;
		}
	}

	private void OnTriggerStay(Collider col)
	{
		if (col.gameObject.layer == 9 && (bool)col.GetComponent<vRagdoll>() && !controller.ragdolled && anim.GetFloat("InputMagnitude") >= 0.75f && !col.GetComponent<vControlAI>().ragdolled)
		{
			col.GetComponent<AudioSource>().PlayOneShot(col.GetComponent<NPC_generator>().Hit_voice);
			col.GetComponent<vRagdoll>().ActivateRagdoll(null, 2f);
			ragdoll.horizontalMultiplier = 0.1f;
			ragdoll.verticalMultiplier = 0.1f;
			anim.SetBool("Ragdolled", value: true);
			ragdoll.ActivateRagdoll(null, 2f);
			player.inventory.data.progress_data.People_Knocked++;
		}
	}

	private void Update()
	{
		if (anim.GetFloat("InputMagnitude") > 0.99f)
		{
			if (Run_Time < 3f)
			{
				Run_Time += Time.deltaTime;
			}
		}
		else if (Run_Time != 0f)
		{
			Run_Time = 0f;
		}
	}

	private IEnumerator Start_Speech()
	{
		yield return new WaitForSeconds(3.5f);
		anim.SetBool("Ragdolled", value: false);
		if (player.Drunk)
		{
			yield break;
		}
		if (player.Happiness > 25f)
		{
			player.New_Happiness(-25);
		}
		anim.SetTrigger("Pain");
		int num = Random.Range(0, 2);
		if (num == 0)
		{
			if (player.inventory.data.Language == 0)
			{
				player.interface_script.Rox_Interface.tip_text.text = "Ай!";
			}
			if (player.inventory.data.Language == 1)
			{
				player.interface_script.Rox_Interface.tip_text.text = "Ouch!";
			}
		}
		if (num == 1)
		{
			if (player.inventory.data.Language == 0)
			{
				player.interface_script.Rox_Interface.tip_text.text = "Больно...";
			}
			if (player.inventory.data.Language == 1)
			{
				player.interface_script.Rox_Interface.tip_text.text = "It hurts...";
			}
		}
		player.Interface_anim.SetTrigger("Tip_Common");
	}
}
