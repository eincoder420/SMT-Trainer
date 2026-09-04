using UnityEngine;

public class OptDemo_Flicker : MonoBehaviour
{
	public Light Light;

	public float FlickerSpeed = 3f;

	public float FlickerAmount = 0.25f;

	private float initIntensity;

	private float initRange;

	private float time;

	private float random;

	private void Reset()
	{
		Light = GetComponent<Light>();
	}

	private void Awake()
	{
		if (!Light)
		{
			Light = GetComponent<Light>();
		}
		if (!Light)
		{
			Object.Destroy(this);
			return;
		}
		random = Random.Range(-100f, 100f);
		time = Random.Range(-100f, 100f);
		initIntensity = Light.intensity;
		initRange = Light.range;
	}

	private void Update()
	{
		time += Time.deltaTime * FlickerSpeed;
		float num = Mathf.PerlinNoise(random + time, random / 2f + time * 0.75f);
		Light.intensity = initIntensity - initIntensity * 0.8f * FlickerAmount * num;
		Light.range = initRange - initIntensity * 0.5f * FlickerAmount * num;
	}
}
