using UnityEngine;

public class Street_Light : MonoBehaviour
{
	private float Intensity;

	private float Range;

	private Light light;

	private float Clamp_Distance;

	public float max_distance;

	[HideInInspector]
	public Transform player;

	public void Find_Light_Components()
	{
		light = GetComponent<Light>();
		Intensity = light.intensity;
		Range = light.range;
	}

	public void Check_Street_Light()
	{
		Clamp_Distance = Mathf.Clamp01(Mathf.InverseLerp(max_distance, 10f, Vector3.Distance(player.position, new Vector3(base.transform.position.x, player.position.y, base.transform.position.z))));
		light.intensity = Intensity * Clamp_Distance;
		light.range = Range * Clamp_Distance;
	}
}
