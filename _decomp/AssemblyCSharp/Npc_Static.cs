using System.Collections;
using UnityEngine;

public class Npc_Static : MonoBehaviour
{
	private NPC_generator[] NPC;

	private Roxanne_Control player;

	private void Start()
	{
		NPC = base.transform.GetComponentsInChildren<NPC_generator>();
		player = Object.FindObjectOfType<Roxanne_Control>();
		StartCoroutine(Check_Distance());
	}

	private IEnumerator Check_Distance()
	{
		while (base.gameObject.activeInHierarchy)
		{
			yield return new WaitForSeconds(1f);
			if ((bool)player)
			{
				for (int i = 0; i < NPC.Length; i++)
				{
					NPC[i].gameObject.SetActive(Vector3.Distance(player.transform.position, NPC[i].transform.position) < 30f);
				}
			}
		}
	}
}
