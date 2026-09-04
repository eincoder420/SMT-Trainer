using UnityEngine;

namespace MeshCombineStudio;

public class SimpleMove : MonoBehaviour
{
	public Vector3 rotDirMulti = Vector3.one;

	public float moveMulti = 50f;

	public float rotMulti = 50f;

	private Vector3 dir;

	private float t;

	private void Start()
	{
		dir = Random.onUnitSphere;
		t = Random.value * moveMulti;
	}

	private void Update()
	{
		float num = Mathf.Sin(Time.time + t) * moveMulti;
		if (moveMulti != 0f)
		{
			base.transform.Translate(dir * num * Time.deltaTime, Space.World);
		}
		base.transform.Rotate(Vector3.Scale(dir, rotDirMulti) * Time.deltaTime * rotMulti, Space.Self);
	}
}
