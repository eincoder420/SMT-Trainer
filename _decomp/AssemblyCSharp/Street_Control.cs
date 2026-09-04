using System;
using System.Collections;
using Invector;
using UnityEngine;
using UnityEngine.AzureSky;

public class Street_Control : MonoBehaviour
{
	[Serializable]
	public struct Sitting_Npc
	{
		public NPC_generator NPC;

		public Transform bench;
	}

	[Serializable]
	public struct Street_Name
	{
		public string[] Name;
	}

	private AzureTimeController timeController;

	[HideInInspector]
	public PauseMenuScript interface_script;

	[HideInInspector]
	public Start_Menu menu;

	public Transform Street_Folder;

	public Transform Entrances_Folder;

	public Transform Interiors_Folder;

	public Transform NPC_Folder;

	public Transform Lights_Folder;

	public Transform Colliders_Folder;

	private Restaurant[] Restaurants;

	private Bench[] Bench;

	public Transform Bench_Girl;

	public Transform Bench_Man;

	public Street_Name[] Street_Names;

	public NPC_generator[] Npcs;

	private Street_Light[] Lights;

	private vRotateObject[] Spots;

	private Rigidbody[] Rigidbodies;

	public int Max_D_Rigs;

	public int Max_D_Lights;

	public int Max_D_Npc;

	[HideInInspector]
	public Transform player;

	private float active_rigs;

	private float active_street_lights;

	private float active_spots;

	private float active_npc;

	private float active_triggers;

	public void Show_Street_Name(int id)
	{
		interface_script.Rox_Interface.Street_Name.text = Street_Names[id].Name[menu.data.Language];
		interface_script.interface_anim.SetTrigger("Show_Street");
	}

	private void Start()
	{
		if (!interface_script)
		{
			interface_script = UnityEngine.Object.FindObjectOfType<PauseMenuScript>();
		}
		if (!menu)
		{
			menu = UnityEngine.Object.FindObjectOfType<Start_Menu>();
		}
		Restaurants = Street_Folder.GetComponentsInChildren<Restaurant>(includeInactive: true);
		Bench = Street_Folder.GetComponentsInChildren<Bench>(includeInactive: true);
		for (int i = 0; i < Restaurants.Length; i++)
		{
			if (!Restaurants[i].Seller)
			{
				Restaurants[i].Lamp.gameObject.SetActive(value: false);
				Restaurants[i].Trigger.gameObject.SetActive(value: false);
			}
			else
			{
				Restaurants[i].Seller.transform.parent = NPC_Folder;
				Restaurants[i].Seller.gameObject.SetActive(value: true);
			}
		}
		for (int j = 0; j < Bench.Length; j++)
		{
			bool naked = Bench[j].Naked;
			bool girl = Bench[j].Girl;
			if ((bool)Bench[j].Sit_Position_1 && Bench[j].Spawn)
			{
				Transform original = Bench_Girl;
				if (!girl)
				{
					original = Bench_Man;
				}
				Transform transform = UnityEngine.Object.Instantiate(original, Bench[j].Sit_Position_1.position, Bench[j].Sit_Position_1.rotation);
				if ((bool)Bench[j].Parent)
				{
					transform.transform.parent = Bench[j].Parent;
				}
				else
				{
					transform.transform.parent = NPC_Folder;
				}
				NPC_generator componentInChildren = transform.GetComponentInChildren<NPC_generator>(includeInactive: true);
				componentInChildren.AI.Sitting = true;
				if (naked)
				{
					componentInChildren.Make_Naked();
				}
			}
		}
		Npcs = NPC_Folder.GetComponentsInChildren<NPC_generator>(includeInactive: true);
		Rigidbodies = GetComponentsInChildren<Rigidbody>(includeInactive: true);
		Lights = GetComponentsInChildren<Street_Light>(includeInactive: true);
		Prepare_Lights();
		Spots = GetComponentsInChildren<vRotateObject>(includeInactive: true);
		for (int k = 0; k < Npcs.Length; k++)
		{
			Npcs[k].FindComponents();
		}
		StartCoroutine(Turn_Memory_Objects());
		timeController = UnityEngine.Object.FindObjectOfType<AzureTimeController>();
	}

	public void Prepare_Lights()
	{
		for (int i = 0; i < Lights.Length; i++)
		{
			Lights[i].Find_Light_Components();
			Lights[i].player = player;
			Lights[i].max_distance = Max_D_Lights;
			Lights[i].transform.parent = Lights_Folder;
		}
	}

	public void Restore_Npc()
	{
		for (int i = 0; i < Npcs.Length; i++)
		{
			Npcs[i].Already_Seen = false;
			Npcs[i].Seen_Naked_Rox = false;
		}
	}

	public void Turn_Street_Light()
	{
		if (timeController.timeline > 5.5f)
		{
			_ = timeController.timeline < 6.5f;
		}
		else
			_ = 0;
		if (timeController.timeline > 18.5f)
		{
			_ = timeController.timeline < 19.5f;
		}
		else
			_ = 0;
	}

	public void Spawn_NPC(Transform point)
	{
	}

	private IEnumerator Turn_Memory_Objects()
	{
		while (base.gameObject.activeInHierarchy)
		{
			yield return new WaitForSeconds(1f);
			for (int i = 0; i < Npcs.Length; i++)
			{
				bool flag = Vector3.Distance(Npcs[i].transform.position, player.transform.position) < (float)Max_D_Npc;
				bool on = ((!Npcs[i].AI.waypointArea) ? (Vector3.Distance(Npcs[i].transform.position, player.transform.position) < 10f) : flag);
				Npcs[i].gameObject.SetActive(flag);
				Npcs[i].Visible = flag && !Npcs[i].Force_Stopped;
				Npcs[i].Turn_Components(on);
			}
			for (int j = 0; j < Lights.Length; j++)
			{
				bool flag2 = Vector3.Distance(Lights[j].transform.position, player.transform.position) < (float)Max_D_Lights;
				Lights[j].enabled = flag2;
				if (flag2)
				{
					Lights[j].Check_Street_Light();
				}
			}
			for (int k = 0; k < Spots.Length; k++)
			{
				bool flag3 = Vector3.Distance(Spots[k].transform.position, player.transform.position) < (float)Max_D_Rigs;
				Spots[k].enabled = flag3;
			}
			for (int l = 0; l < Rigidbodies.Length; l++)
			{
				bool flag4 = Vector3.Distance(Rigidbodies[l].transform.position, player.transform.position) < (float)Max_D_Rigs;
				Rigidbodies[l].isKinematic = !flag4;
			}
		}
	}
}
