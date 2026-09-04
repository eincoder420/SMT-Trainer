using UnityEngine;

public class vEnableRandomObject : MonoBehaviour
{
	public GameObject[] objects;

	public bool enableOnStart;

	protected void Awake()
	{
		if (enableOnStart)
		{
			EnableObject();
		}
	}

	public virtual void EnableObject()
	{
		int num = Random.Range(0, objects.Length * 10) & (objects.Length - 1);
		for (int i = 0; i < objects.Length; i++)
		{
			objects[i].SetActive(i == num);
		}
	}
}
