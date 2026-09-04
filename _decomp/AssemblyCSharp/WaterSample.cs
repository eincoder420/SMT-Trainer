using UnityEngine;

public class WaterSample : MonoBehaviour
{
	private Renderer r;

	private Material mat;

	private void Start()
	{
		r = GetComponent<Renderer>();
		if ((bool)r)
		{
			mat = r.sharedMaterial;
		}
	}

	private void Update()
	{
		if ((bool)r && (bool)mat)
		{
			Vector4 vector = mat.GetVector("WaveSpeed");
			float @float = mat.GetFloat("_WaveScale");
			float num = Time.time / 20f;
			Vector4 vector2 = vector * (num * @float);
			Vector4 value = new Vector4(Mathf.Repeat(vector2.x, 1f), Mathf.Repeat(vector2.y, 1f), Mathf.Repeat(vector2.z, 1f), Mathf.Repeat(vector2.w, 1f));
			mat.SetVector("_WaveOffset", value);
		}
	}
}
