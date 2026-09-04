using UnityEngine;

public class LookAt : MonoBehaviour
{
	public GameObject target;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.LookAt(target.transform);
	}
}
