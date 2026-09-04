using UnityEngine;

public class SimpleMotion : MonoBehaviour
{
	private float xx;

	private float yy;

	public float height = 0.1f;

	public float width = 0.1f;

	private float nw;

	private float nh;

	public float wspeed = 0.1f;

	public float hspeed = 0.1f;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Translate(Vector3.left * Time.deltaTime * xx);
		base.transform.Translate(Vector3.up * Time.deltaTime * yy);
		xx = Mathf.Sin(nw) * width;
		nw += wspeed;
		yy = Mathf.Cos(nh) * height;
		nh -= hspeed;
	}
}
