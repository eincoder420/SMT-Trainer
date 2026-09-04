using UnityEngine;

public class DayAndNight : MonoBehaviour
{
	public GameObject Rain;

	public GameObject Day;

	public GameObject Night;

	public Material Mat;

	public float NightIntensity;

	public float DayIntensity;

	public Material SkyboxNight;

	public Material SkyboxDay;

	public Color FogNight;

	public Color FogDay;

	public GameObject volumeGO;

	public float dist;

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown("h"))
		{
			volumeGO.SetActive(value: false);
		}
		if (Input.GetKeyDown("j"))
		{
			volumeGO.SetActive(value: true);
		}
		if (Input.GetKeyDown("t"))
		{
			if (Day.activeSelf)
			{
				Day.SetActive(value: false);
				Night.SetActive(value: true);
				Mat.SetFloat("_Intensity", NightIntensity);
				RenderSettings.skybox = SkyboxNight;
				RenderSettings.fogColor = FogNight;
			}
			else
			{
				Night.SetActive(value: false);
				Day.SetActive(value: true);
				Mat.SetFloat("_Intensity", DayIntensity);
				RenderSettings.skybox = SkyboxDay;
				RenderSettings.fogColor = FogDay;
			}
		}
	}
}
