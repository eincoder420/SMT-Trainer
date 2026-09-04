using FCG;
using UnityEngine;

public class RunTimeSample : MonoBehaviour
{
	public GameObject cg;

	private CityGenerator generator;

	private TrafficSystem trafficSystem;

	private bool withDownTownArea = true;

	private bool rightHand = true;

	private void Awake()
	{
		generator = cg.GetComponent<CityGenerator>();
	}

	public void GenerateCityAtRuntime(int citySize)
	{
		Object.Destroy(GameObject.Find("CarContainer"));
		generator = cg.GetComponent<CityGenerator>();
		generator.GenerateCity(citySize);
	}

	public void WithDownTownArea(bool value)
	{
		withDownTownArea = value;
	}

	public void RightHand(bool value)
	{
		rightHand = value;
	}

	public void GenerateBuildings()
	{
		float downTownSize = 100f;
		generator.GenerateAllBuildings(withDownTownArea, downTownSize);
	}

	public void AddTrafficSystem()
	{
		trafficSystem = Object.FindObjectOfType<TrafficSystem>();
		if ((bool)trafficSystem)
		{
			trafficSystem.LoadCars((!rightHand) ? 1 : 0);
			Debug.LogWarning("Move the camera to the streets so that vehicles are generated around it");
		}
		else
		{
			Debug.LogError("Traffic System prefab not found in Hierarchy");
		}
	}
}
