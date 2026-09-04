using UnityEngine;

namespace Invector;

public class vRotateObject : MonoBehaviour
{
	public Vector3 rotationSpeed;

	private MeshRenderer renderer;

	private Color color;

	private Light light;

	private Transform player;

	private float Clamp_Distance;

	private float Max;

	public bool Yellow;

	public bool Buying;

	private void Awake()
	{
		renderer = GetComponent<MeshRenderer>();
		player = Object.FindObjectOfType<Roxanne_Control>().transform;
		light = GetComponent<Light>();
		base.transform.localScale = new Vector3(0.35f, 0.035f, 0.35f);
		if (Yellow)
		{
			Max = 2f;
		}
		else
		{
			Max = 6f;
		}
		if (!Buying)
		{
			Set_Light_Color();
		}
	}

	public void Set_Light_Color()
	{
		if ((bool)light && (bool)renderer)
		{
			color = renderer.material.GetColor("_TintColor");
			light.color = color;
			light.range = 1.5f;
		}
	}

	private void Update()
	{
		if ((bool)renderer && (bool)light)
		{
			Clamp_Distance = Mathf.Clamp01(Mathf.InverseLerp(Max, 0.5f, Vector3.Distance(player.position, base.transform.position)));
			renderer.material.SetColor("_TintColor", new Color(color.r, color.g, color.b, 0.7f * Clamp_Distance));
			light.intensity = 0.8f * Clamp_Distance;
			light.range = 1.5f * Clamp_Distance;
		}
		base.transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
	}
}
