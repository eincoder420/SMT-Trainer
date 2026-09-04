using UnityEngine;

public class Rotation : MonoBehaviour
{
	public float rotSpeed = 2f;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(0f, 1f * Time.deltaTime * rotSpeed, 0f);
	}
}
