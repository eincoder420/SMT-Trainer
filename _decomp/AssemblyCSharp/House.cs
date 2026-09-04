using UnityEngine;

public class House : MonoBehaviour
{
	public GameObject[] Lights;

	public Transform[] Spawn_Point;

	private void Start()
	{
		for (int i = 0; i < Lights.Length; i++)
		{
			int num = Random.Range(0, 2);
			Lights[i].SetActive(num == 1);
		}
	}

	public void Spawn_NPC()
	{
		int num = 0;
		for (int i = 0; i < Spawn_Point.Length; i++)
		{
			if (Random.Range(0, 3) > 0)
			{
				Object.FindObjectOfType<Street_Control>().Spawn_NPC(Spawn_Point[i]);
				num++;
			}
		}
		if (num == 0)
		{
			Object.FindObjectOfType<Street_Control>().Spawn_NPC(Spawn_Point[Random.Range(0, Spawn_Point.Length)]);
		}
	}
}
