using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class DayNight : MonoBehaviour
{
	public Material[] materialDay;

	public Material[] materialNight;

	public Material skyBoxDay;

	public Material skyBoxNight;

	public Light directionalLight;

	public Transform player;

	public int light_Distance;

	private GameObject[] Light_Array_1;

	private GameObject[] Light_Array_2;

	[HideInInspector]
	public bool isNight;

	[HideInInspector]
	public bool isMoonLight;

	[HideInInspector]
	public bool isSpotLights;

	[HideInInspector]
	public bool night;

	[HideInInspector]
	public bool moonLight;

	[HideInInspector]
	public bool spotLights;

	[HideInInspector]
	public float intenseMoonLight = 0.2f;

	[HideInInspector]
	public float _intenseMoonLight;

	[HideInInspector]
	public float intenseSunLight = 1f;

	[HideInInspector]
	public float _intenseSunLight;

	[HideInInspector]
	public Color skyColorDay = new Color(0.74f, 0.62f, 0.6f);

	[HideInInspector]
	public Color equatorColorDay = new Color(0.74f, 0.74f, 0.74f);

	[HideInInspector]
	public Color _skyColorDay;

	[HideInInspector]
	public Color _equatorColorDay;

	[HideInInspector]
	public Color skyColorNight = new Color(0.78f, 0.72f, 0.72f);

	[HideInInspector]
	public Color equatorColorNight = new Color(0.16f, 0.16f, 0.16f);

	[HideInInspector]
	public Color _skyColorNight;

	[HideInInspector]
	public Color _equatorColorNight;

	[HideInInspector]
	public Color sunLightColor;

	[HideInInspector]
	public Color _sunLightColor;

	[HideInInspector]
	public Color moonLightColor;

	[HideInInspector]
	public Color _moonLightColor;

	public void ChangeMaterial()
	{
		RenderSettings.skybox = (isNight ? skyBoxNight : skyBoxDay);
		UpdateColor();
		GameObject gameObject = GameObject.Find("City-Maker");
		if (gameObject == null)
		{
			return;
		}
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] sharedMaterials = componentsInChildren[i].GetComponent<Renderer>().sharedMaterials;
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				for (int k = 0; k < materialDay.Length; k++)
				{
					if (isNight)
					{
						if (sharedMaterials[j] == materialDay[k])
						{
							sharedMaterials[j] = materialNight[k];
						}
					}
					else if (sharedMaterials[j] == materialNight[k])
					{
						sharedMaterials[j] = materialDay[k];
					}
				}
				componentsInChildren[i].GetComponent<MeshRenderer>().sharedMaterials = sharedMaterials;
			}
		}
		SetDirectionalLight();
		SetStreetLights();
	}

	public void UpdateColor()
	{
		if (isNight)
		{
			if ((bool)directionalLight)
			{
				directionalLight.GetComponent<Light>().color = moonLightColor;
			}
			RenderSettings.ambientMode = AmbientMode.Trilight;
			RenderSettings.ambientSkyColor = skyColorNight;
			RenderSettings.ambientEquatorColor = equatorColorNight;
			RenderSettings.ambientGroundColor = new Color(0.07f, 0.07f, 0.07f);
		}
		else
		{
			if ((bool)directionalLight)
			{
				directionalLight.GetComponent<Light>().color = sunLightColor;
			}
			RenderSettings.ambientMode = AmbientMode.Trilight;
			RenderSettings.ambientSkyColor = skyColorDay;
			RenderSettings.ambientEquatorColor = equatorColorDay;
			RenderSettings.ambientGroundColor = new Color(0.4f, 0.4f, 0.4f);
		}
	}

	public void SetDirectionalLight()
	{
		if ((bool)directionalLight)
		{
			directionalLight.GetComponent<Light>().enabled = !isNight || isMoonLight;
			directionalLight.intensity = (isNight ? (intenseMoonLight / 100f) : (intenseSunLight / 100f));
		}
	}

	private bool isNear(Transform light_source)
	{
		if (light_Distance > 0)
		{
			return Vector3.Distance(player.position, light_source.position) < (float)light_Distance;
		}
		return true;
	}

	public void SetStreetLights()
	{
		Light_Array_1 = (from g in Object.FindObjectsOfType(typeof(GameObject))
			select g as GameObject into g
			where g.name == "_LightV"
			select g).ToArray();
		Light_Array_2 = (from g in Object.FindObjectsOfType(typeof(GameObject))
			select g as GameObject into g
			where g.name == "_Spot_Light"
			select g).ToArray();
		Control_Street_Lights();
	}

	private void Control_Street_Lights()
	{
		GameObject[] light_Array_ = Light_Array_1;
		foreach (GameObject gameObject in light_Array_)
		{
			gameObject.GetComponent<MeshRenderer>().enabled = isNight;
			if ((bool)gameObject.transform.GetChild(0))
			{
				gameObject.transform.GetChild(0).GetComponent<Light>().enabled = isSpotLights && isNight && isNear(gameObject.transform.GetChild(0));
			}
		}
		light_Array_ = Light_Array_2;
		foreach (GameObject gameObject2 in light_Array_)
		{
			gameObject2.GetComponent<Light>().enabled = isSpotLights && isNight && isNear(gameObject2.transform);
		}
	}

	private void Start()
	{
		SetStreetLights();
		StartCoroutine(Turn_Lights());
	}

	private IEnumerator Turn_Lights()
	{
		while (base.gameObject.activeInHierarchy)
		{
			yield return new WaitForSeconds(1f);
			Control_Street_Lights();
		}
	}
}
