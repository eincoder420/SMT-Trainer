using System.Linq;
using UnityEngine;

namespace FCG;

public class CityGenerator : MonoBehaviour
{
	private int nB;

	private Vector3 center;

	private int residential;

	private bool _residential;

	private GameObject cityMaker;

	[HideInInspector]
	public GameObject[] miniBorder;

	[HideInInspector]
	public GameObject[] smallBorder;

	[HideInInspector]
	public GameObject[] mediumBorder;

	[HideInInspector]
	public GameObject[] largeBorder;

	[HideInInspector]
	public GameObject[] miniBorderFlat;

	[HideInInspector]
	public GameObject[] smallBorderFlat;

	[HideInInspector]
	public GameObject[] mediumBorderFlat;

	[HideInInspector]
	public GameObject[] largeBorderFlat;

	[HideInInspector]
	public GameObject[] miniBorderWithExitOfCity;

	[HideInInspector]
	public GameObject[] smallBorderWithExitOfCity;

	[HideInInspector]
	public GameObject[] mediumBorderWithExitOfCity;

	[HideInInspector]
	public GameObject[] largeBorderWithExitOfCity;

	[HideInInspector]
	public GameObject[] largeBlocks;

	private bool[] _largeBlocks;

	[HideInInspector]
	public GameObject[] bigLargeBlocks;

	[HideInInspector]
	public GameObject[] forward50;

	[HideInInspector]
	public GameObject[] forward100;

	[HideInInspector]
	public GameObject[] forward300;

	[HideInInspector]
	public GameObject[] forward400;

	[HideInInspector]
	public GameObject[] forwardLeft400;

	[HideInInspector]
	public GameObject[] forwardRight400;

	[HideInInspector]
	public GameObject[] left200;

	[HideInInspector]
	public GameObject[] left300;

	[HideInInspector]
	public GameObject[] right200;

	[HideInInspector]
	public GameObject[] right300;

	private bool[] _bigLargeBlocks;

	[HideInInspector]
	public GameObject[] BB;

	[HideInInspector]
	public GameObject[] BC;

	[HideInInspector]
	public GameObject[] BR;

	[HideInInspector]
	public GameObject[] DC;

	[HideInInspector]
	public GameObject[] EB;

	[HideInInspector]
	public GameObject[] EC;

	[HideInInspector]
	public GameObject[] MB;

	[HideInInspector]
	public GameObject[] BK;

	[HideInInspector]
	public GameObject[] SB;

	[HideInInspector]
	public GameObject[] BBS;

	[HideInInspector]
	public GameObject[] BCS;

	private int[] _BB;

	private int[] _BC;

	private int[] _BR;

	private int[] _EB;

	private int[] _EC;

	private int[] _EBS;

	private int[] _ECS;

	private int[] _MB;

	private int[] _BK;

	private int[] _SB;

	private int[] _BBS;

	private int[] _BCS;

	private GameObject[] tempArray;

	private int numB;

	private float distCenter = 300f;

	private bool withDowntownArea = true;

	private float downTownSize = 100f;

	private GameObject pB;

	public void ClearCity()
	{
		if (!cityMaker)
		{
			cityMaker = GameObject.Find("City-Maker");
		}
		if ((bool)cityMaker)
		{
			Object.DestroyImmediate(cityMaker);
		}
	}

	public void GenerateCity(int size, bool withSatteliteCity = false, bool borderFlat = false)
	{
		bool flag = false;
		switch (size)
		{
		case 1:
			flag = GenerateStreetsVerySmall(borderFlat, withSatteliteCity);
			break;
		case 2:
			flag = GenerateStreetsSmall(borderFlat, withSatteliteCity);
			break;
		case 3:
			flag = GenerateStreets(borderFlat, withSatteliteCity);
			break;
		case 4:
			flag = GenerateStreetsBig(borderFlat, withSatteliteCity);
			break;
		}
		if (flag)
		{
			Transform transform = CityExitPosition();
			if (transform != null)
			{
				switch ((int)Random.Range(1f, 10f))
				{
				case 8:
					GenerateStreetsVerySmall(borderFlat: false, withSatteliteCity: false, satteliteCity: true, 0f, -1516f);
					Object.Instantiate(forward400[Random.Range(0, forward400.Length)], transform.position, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					Object.Instantiate(forward400[Random.Range(0, forward400.Length)], transform.position + transform.forward * 400f, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					Object.Instantiate(forward400[Random.Range(0, forward400.Length)], transform.position + transform.forward * 800f, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					break;
				case 7:
					GenerateStreetsVerySmall(borderFlat: false, withSatteliteCity: false, satteliteCity: true, -300f, -1516f);
					Object.Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], transform.position, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					Object.Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], transform.position + transform.forward * 400f + transform.right * 100f, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					Object.Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], transform.position + transform.forward * 800f + transform.right * 200f, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					break;
				case 6:
					GenerateStreetsVerySmall(borderFlat: false, withSatteliteCity: false, satteliteCity: true, 200f, -1516f);
					Object.Instantiate(forward400[Random.Range(0, forward400.Length)], transform.position, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					Object.Instantiate(forwardLeft400[Random.Range(0, forwardLeft400.Length)], transform.position + transform.forward * 400f, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					Object.Instantiate(forwardLeft400[Random.Range(0, forwardLeft400.Length)], transform.position + transform.forward * 800f - transform.right * 100f, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					break;
				case 5:
					GenerateStreetsVerySmall(borderFlat: false, withSatteliteCity: false, satteliteCity: true, -100f, -1516f);
					Object.Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], transform.position, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					Object.Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], transform.position + transform.forward * 400f + transform.right * 100f, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					Object.Instantiate(forwardLeft400[Random.Range(0, forwardRight400.Length)], transform.position + transform.forward * 800f + transform.right * 200f, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					break;
				case 4:
					GenerateStreetsVerySmall(borderFlat: false, withSatteliteCity: false, satteliteCity: true, 700f, -1316f);
					Object.Instantiate(left300[Random.Range(0, left300.Length)], transform.position, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					Object.Instantiate(right300[Random.Range(0, right300.Length)], transform.position + transform.forward * 300f - transform.right * 300f, Quaternion.Euler(0f, 270f, 0f), cityMaker.transform);
					Object.Instantiate(forwardLeft400[Random.Range(0, forwardRight400.Length)], transform.position + transform.forward * 600f - transform.right * 600f, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					break;
				case 3:
					GenerateStreetsVerySmall(borderFlat: false, withSatteliteCity: false, satteliteCity: true, 500f, -1316f);
					Object.Instantiate(left300[Random.Range(0, left300.Length)], transform.position, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					Object.Instantiate(right300[Random.Range(0, right300.Length)], transform.position + transform.forward * 300f - transform.right * 300f, Quaternion.Euler(0f, 270f, 0f), cityMaker.transform);
					Object.Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], transform.position + transform.forward * 600f - transform.right * 600f, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					break;
				case 2:
					GenerateStreetsVerySmall(borderFlat: false, withSatteliteCity: false, satteliteCity: true, -700f, -1316f);
					Object.Instantiate(right300[Random.Range(0, right300.Length)], transform.position, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					Object.Instantiate(left300[Random.Range(0, left300.Length)], transform.position + transform.forward * 300f + transform.right * 300f, Quaternion.Euler(0f, 90f, 0f), cityMaker.transform);
					Object.Instantiate(forwardRight400[Random.Range(0, forwardRight400.Length)], transform.position + transform.forward * 600f + transform.right * 600f, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					break;
				default:
					GenerateStreetsVerySmall(borderFlat: false, withSatteliteCity: false, satteliteCity: true, -500f, -1316f);
					Object.Instantiate(right300[Random.Range(0, right300.Length)], transform.position, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					Object.Instantiate(left300[Random.Range(0, left300.Length)], transform.position + transform.forward * 300f + transform.right * 300f, Quaternion.Euler(0f, 90f, 0f), cityMaker.transform);
					Object.Instantiate(forwardLeft400[Random.Range(0, forwardLeft400.Length)], transform.position + transform.forward * 600f + transform.right * 600f, Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
					break;
				}
			}
			else
			{
				Debug.Log("ExitCity gameobject not found");
			}
		}
		DayNight dayNight = Object.FindObjectOfType<DayNight>();
		if ((bool)dayNight)
		{
			dayNight.ChangeMaterial();
		}
	}

	private Transform CityExitPosition()
	{
		if ((bool)GameObject.Find("ExitCity"))
		{
			return GameObject.Find("ExitCity").transform;
		}
		return null;
	}

	private GameObject InstantiatePrefab(GameObject gameObject, Vector3 pos, Quaternion rot, Transform parent)
	{
		GameObject obj = Object.Instantiate(gameObject, parent);
		obj.transform.position = pos;
		obj.transform.rotation = rot;
		return obj;
	}

	private bool GenerateStreetsVerySmall(bool borderFlat = false, bool withSatteliteCity = false, bool satteliteCity = false, float satteliteCityPositionX = 0f, float satteliteCityPositionZ = 0f)
	{
		if (satteliteCity && !cityMaker)
		{
			satteliteCity = false;
		}
		if (!satteliteCity)
		{
			ClearCity();
			cityMaker = new GameObject("City-Maker");
		}
		if (!satteliteCity)
		{
			distCenter = 150f;
		}
		int num = 0;
		int max = largeBlocks.Length;
		num = Random.Range(0, max);
		GameObject gameObject;
		if (satteliteCity && smallBorderWithExitOfCity.Length != 0)
		{
			gameObject = Object.Instantiate(largeBlocks[num], CityExitPosition().position + new Vector3(satteliteCityPositionX, 0f, satteliteCityPositionZ) - new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
		}
		else
		{
			gameObject = Object.Instantiate(largeBlocks[num], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform);
		}
		gameObject = (((withSatteliteCity || satteliteCity) && miniBorderWithExitOfCity.Length != 0) ? ((!satteliteCity) ? Object.Instantiate(miniBorderWithExitOfCity[Random.Range(0, miniBorderWithExitOfCity.Length)], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform) : Object.Instantiate(miniBorderWithExitOfCity[Random.Range(0, miniBorderWithExitOfCity.Length)], CityExitPosition().position + new Vector3(satteliteCityPositionX, 0f, satteliteCityPositionZ), Quaternion.Euler(0f, 180f, 0f), cityMaker.transform)) : ((!borderFlat) ? Object.Instantiate(miniBorder[Random.Range(0, miniBorder.Length)], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform) : Object.Instantiate(miniBorderFlat[Random.Range(0, miniBorderFlat.Length)], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform)));
		gameObject.transform.SetParent(cityMaker.transform);
		if (withSatteliteCity)
		{
			return miniBorderWithExitOfCity.Length != 0;
		}
		return false;
	}

	private bool GenerateStreetsSmall(bool borderFlat = false, bool withSatteliteCity = false, bool satteliteCity = false)
	{
		if (satteliteCity && !cityMaker)
		{
			satteliteCity = false;
		}
		if (!satteliteCity)
		{
			ClearCity();
			cityMaker = new GameObject("City-Maker");
		}
		if (!satteliteCity)
		{
			distCenter = 200f;
		}
		int num = 0;
		int max = largeBlocks.Length;
		_largeBlocks = new bool[largeBlocks.Length];
		Vector3[] array = new Vector3[3];
		int[] array2 = new int[3];
		if (Random.Range(0f, 6f) < 3f)
		{
			array[1] = new Vector3(0f, 0f, 0f);
			array2[1] = 0;
			array[2] = new Vector3(0f, 0f, 300f);
			array2[2] = 0;
		}
		else
		{
			array[1] = new Vector3(-150f, 0f, 150f);
			array2[1] = 90;
			array[2] = new Vector3(150f, 0f, 150f);
			array2[2] = 90;
		}
		for (int i = 1; i < 3; i++)
		{
			for (int j = 0; j < 100; j++)
			{
				num = Random.Range(0, max);
				if (!_largeBlocks[num])
				{
					break;
				}
			}
			_largeBlocks[num] = true;
			if (satteliteCity && smallBorderWithExitOfCity.Length != 0)
			{
				Object.Instantiate(largeBlocks[num], array[i] + CityExitPosition().position + new Vector3(0f, 0f, -1516f) - new Vector3(0f, 0f, 300f), Quaternion.Euler(0f, array2[i] + 180, 0f), cityMaker.transform);
			}
			else
			{
				Object.Instantiate(largeBlocks[num], array[i], Quaternion.Euler(0f, array2[i], 0f), cityMaker.transform);
			}
		}
		GameObject gameObject = (((withSatteliteCity || satteliteCity) && smallBorderWithExitOfCity.Length != 0) ? ((!satteliteCity) ? Object.Instantiate(smallBorderWithExitOfCity[Random.Range(0, smallBorderWithExitOfCity.Length)], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform) : Object.Instantiate(smallBorderWithExitOfCity[Random.Range(0, smallBorderWithExitOfCity.Length)], CityExitPosition().position + new Vector3(0f, 0f, -1516f), Quaternion.Euler(0f, 180f, 0f), cityMaker.transform)) : ((!borderFlat) ? Object.Instantiate(smallBorder[Random.Range(0, smallBorder.Length)], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform) : Object.Instantiate(smallBorderFlat[Random.Range(0, smallBorderFlat.Length)], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform)));
		gameObject.transform.SetParent(cityMaker.transform);
		if (withSatteliteCity)
		{
			return smallBorderWithExitOfCity.Length != 0;
		}
		return false;
	}

	private bool GenerateStreets(bool borderFlat = false, bool withSatteliteCity = false, bool satteliteCity = false)
	{
		if (satteliteCity && !cityMaker)
		{
			satteliteCity = false;
		}
		if (!satteliteCity)
		{
			ClearCity();
			cityMaker = new GameObject("City-Maker");
		}
		if (!satteliteCity)
		{
			distCenter = 300f;
		}
		int num = 0;
		int max = largeBlocks.Length;
		_largeBlocks = new bool[largeBlocks.Length];
		Vector3[] array = new Vector3[5];
		int[] array2 = new int[5];
		float num2 = Random.Range(0f, 6f);
		if (num2 < 2f)
		{
			array[1] = new Vector3(0f, 0f, 0f);
			array2[1] = 0;
			array[2] = new Vector3(0f, 0f, 300f);
			array2[2] = 0;
			array[3] = new Vector3(450f, 0f, 150f);
			array2[3] = 90;
			array[4] = new Vector3(-450f, 0f, 150f);
			array2[4] = 90;
		}
		else if (num2 < 3f)
		{
			array[1] = new Vector3(-450f, 0f, 150f);
			array2[1] = 90;
			array[2] = new Vector3(-150f, 0f, 150f);
			array2[2] = 90;
			array[3] = new Vector3(150f, 0f, 150f);
			array2[3] = 90;
			array[4] = new Vector3(450f, 0f, 150f);
			array2[4] = 90;
		}
		else if (num2 < 4f)
		{
			array[1] = new Vector3(-450f, 0f, 150f);
			array2[1] = 90;
			array[2] = new Vector3(-150f, 0f, 150f);
			array2[2] = 90;
			array[3] = new Vector3(300f, 0f, 0f);
			array2[3] = 0;
			array[4] = new Vector3(300f, 0f, 300f);
			array2[4] = 0;
		}
		else
		{
			array[1] = new Vector3(450f, 0f, 150f);
			array2[1] = 90;
			array[2] = new Vector3(150f, 0f, 150f);
			array2[2] = 90;
			array[3] = new Vector3(-300f, 0f, 0f);
			array2[3] = 0;
			array[4] = new Vector3(-300f, 0f, 300f);
			array2[4] = 0;
		}
		for (int i = 1; i < 5; i++)
		{
			for (int j = 0; j < 100; j++)
			{
				num = Random.Range(0, max);
				if (!_largeBlocks[num])
				{
					break;
				}
			}
			_largeBlocks[num] = true;
			Object.Instantiate(largeBlocks[num], array[i], Quaternion.Euler(0f, array2[i], 0f), cityMaker.transform);
		}
		GameObject gameObject = (((withSatteliteCity || satteliteCity) && mediumBorderWithExitOfCity.Length != 0) ? Object.Instantiate(mediumBorderWithExitOfCity[Random.Range(0, mediumBorderWithExitOfCity.Length)], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform) : ((!borderFlat) ? Object.Instantiate(mediumBorder[Random.Range(0, mediumBorder.Length)], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform) : Object.Instantiate(mediumBorderFlat[Random.Range(0, mediumBorderFlat.Length)], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform)));
		gameObject.transform.SetParent(cityMaker.transform);
		if (withSatteliteCity)
		{
			return mediumBorderWithExitOfCity.Length != 0;
		}
		return false;
	}

	private bool GenerateStreetsBig(bool borderFlat = false, bool withSatteliteCity = false, bool satteliteCity = false)
	{
		if (satteliteCity && !cityMaker)
		{
			satteliteCity = false;
		}
		if (!satteliteCity)
		{
			ClearCity();
			cityMaker = new GameObject("City-Maker");
		}
		distCenter = 350f;
		int num = 0;
		int max = largeBlocks.Length;
		int max2 = bigLargeBlocks.Length;
		_largeBlocks = new bool[largeBlocks.Length];
		_bigLargeBlocks = new bool[bigLargeBlocks.Length];
		Vector3[] array = new Vector3[7];
		int[] array2 = new int[7];
		int[] array3 = new int[7];
		float num2 = Random.Range(0f, 7f);
		int num3;
		if (num2 < 3f)
		{
			num3 = 6;
			array[1] = new Vector3(0f, 0f, 0f);
			array2[1] = 0;
			array3[1] = 1;
			array[2] = new Vector3(0f, 0f, 300f);
			array2[2] = 0;
			array3[2] = 1;
			array[3] = new Vector3(450f, 0f, 150f);
			array2[3] = 90;
			array3[3] = 1;
			array[4] = new Vector3(-450f, 0f, 150f);
			array2[4] = 90;
			array3[4] = 1;
			array[5] = new Vector3(-300f, 0f, 600f);
			array2[5] = 0;
			array3[5] = 1;
			array[6] = new Vector3(300f, 0f, 600f);
			array2[6] = 0;
			array3[6] = 1;
		}
		else if (num2 < 3f)
		{
			num3 = 6;
			array[1] = new Vector3(-450f, 0f, 150f);
			array2[1] = 90;
			array3[1] = 1;
			array[2] = new Vector3(-150f, 0f, 150f);
			array2[2] = 90;
			array3[2] = 1;
			array[3] = new Vector3(150f, 0f, 150f);
			array2[3] = 90;
			array3[3] = 1;
			array[4] = new Vector3(450f, 0f, 150f);
			array2[4] = 90;
			array3[4] = 1;
			array[5] = new Vector3(-300f, 0f, 600f);
			array2[5] = 0;
			array3[5] = 1;
			array[6] = new Vector3(300f, 0f, 600f);
			array2[6] = 0;
			array3[6] = 1;
		}
		else if (num2 < 4f)
		{
			num3 = 6;
			array[1] = new Vector3(-300f, 0f, 300f);
			array2[1] = 0;
			array3[1] = 1;
			array[2] = new Vector3(-300f, 0f, 0f);
			array2[2] = 0;
			array3[2] = 1;
			array[3] = new Vector3(150f, 0f, 150f);
			array2[3] = 90;
			array3[3] = 1;
			array[4] = new Vector3(450f, 0f, 150f);
			array2[4] = 90;
			array3[4] = 1;
			array[5] = new Vector3(-300f, 0f, 600f);
			array2[5] = 0;
			array3[5] = 1;
			array[6] = new Vector3(300f, 0f, 600f);
			array2[6] = 0;
			array3[6] = 1;
		}
		else if (num2 < 5f)
		{
			num3 = 5;
			array[1] = new Vector3(-300f, 0f, 0f);
			array2[1] = 0;
			array3[1] = 1;
			array[2] = new Vector3(300f, 0f, 0f);
			array2[2] = 0;
			array3[2] = 1;
			array[3] = new Vector3(-300f, 0f, 600f);
			array2[3] = 0;
			array3[3] = 1;
			array[4] = new Vector3(300f, 0f, 600f);
			array2[4] = 0;
			array3[4] = 1;
			array[5] = new Vector3(0f, 0f, 300f);
			array2[5] = 0;
			array3[5] = 2;
		}
		else
		{
			num3 = 6;
			array[1] = new Vector3(-450f, 0f, 150f);
			array2[1] = 90;
			array3[1] = 1;
			array[2] = new Vector3(300f, 0f, 0f);
			array2[2] = 0;
			array3[2] = 1;
			array[3] = new Vector3(-150f, 0f, 150f);
			array2[3] = 90;
			array3[3] = 1;
			array[4] = new Vector3(450f, 0f, 450f);
			array2[4] = 90;
			array3[4] = 1;
			array[5] = new Vector3(-300f, 0f, 600f);
			array2[5] = 0;
			array3[5] = 1;
			array[6] = new Vector3(150f, 0f, 450f);
			array2[6] = 90;
			array3[6] = 1;
		}
		for (int i = 1; i <= num3; i++)
		{
			if (array3[i] == 1)
			{
				for (int j = 0; j < 100; j++)
				{
					num = Random.Range(0, max);
					if (!_largeBlocks[num])
					{
						break;
					}
				}
				_largeBlocks[num] = true;
				Object.Instantiate(largeBlocks[num], array[i], Quaternion.Euler(0f, array2[i], 0f), cityMaker.transform);
			}
			else
			{
				if (array3[i] != 2)
				{
					continue;
				}
				for (int k = 0; k < 100; k++)
				{
					num = Random.Range(0, max2);
					if (!_bigLargeBlocks[num])
					{
						break;
					}
				}
				_bigLargeBlocks[num] = true;
				Object.Instantiate(bigLargeBlocks[num], array[i], Quaternion.Euler(0f, array2[i], 0f), cityMaker.transform);
			}
		}
		GameObject gameObject = (((withSatteliteCity || satteliteCity) && largeBorderWithExitOfCity.Length != 0) ? Object.Instantiate(largeBorderWithExitOfCity[Random.Range(0, largeBorderWithExitOfCity.Length)], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform) : ((!borderFlat) ? Object.Instantiate(largeBorder[Random.Range(0, largeBorder.Length)], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform) : Object.Instantiate(largeBorderFlat[Random.Range(0, largeBorderFlat.Length)], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), cityMaker.transform)));
		gameObject.transform.SetParent(cityMaker.transform);
		if (withSatteliteCity)
		{
			return largeBorderWithExitOfCity.Length != 0;
		}
		return false;
	}

	public void GenerateAllBuildings(bool _withDowntownArea, float _downTownSize)
	{
		downTownSize = _downTownSize;
		withDowntownArea = _withDowntownArea;
		if (withDowntownArea)
		{
			GameObject[] array = (from g in Object.FindObjectsOfType(typeof(GameObject))
				select g as GameObject into g
				where g.name == "Marcador"
				select g).ToArray();
			if (array.Length == 1)
			{
				center = array[0].transform.position;
			}
			else
			{
				center = array[Random.Range(1, array.Length - 1)].transform.position;
			}
			if ((bool)GameObject.Find("DownTownPosition") && Random.Range(1, 10) < 5)
			{
				center = GameObject.Find("DownTownPosition").transform.position;
			}
		}
		_BB = new int[BB.Length];
		_BC = new int[BC.Length];
		_BR = new int[BR.Length];
		_EB = new int[EB.Length];
		_EC = new int[EC.Length];
		_MB = new int[MB.Length];
		_BK = new int[BK.Length];
		_SB = new int[SB.Length];
		_EBS = new int[EB.Length];
		_ECS = new int[EC.Length];
		_BBS = new int[BBS.Length];
		_BCS = new int[BCS.Length];
		residential = 0;
		DestroyBuildings();
		GameObject obj = new GameObject();
		nB = 0;
		CreateBuildingsInSuperBlocks();
		CreateBuildingsInBlocks();
		CreateBuildingsInLines();
		CreateBuildingsInDouble();
		Debug.Log(nB + " buildings were created");
		Object.DestroyImmediate(obj);
		DayNight dayNight = Object.FindObjectOfType<DayNight>();
		if ((bool)dayNight)
		{
			dayNight.ChangeMaterial();
		}
	}

	public void CreateBuildingsInLines()
	{
		tempArray = (from g in Object.FindObjectsOfType(typeof(GameObject))
			select g as GameObject into g
			where g.name == "Marcador"
			select g).ToArray();
		GameObject[] array = tempArray;
		foreach (GameObject gameObject in array)
		{
			_residential = residential < 15 && Vector3.Distance(center, gameObject.transform.position) > 400f && Random.Range(0, 100) < 30;
			foreach (Transform item in gameObject.transform)
			{
				if (item.name == "E")
				{
					CreateBuildingsInCorners(item.gameObject);
				}
				else if (item.name == "EL")
				{
					int num = 0;
					do
					{
						num++;
					}
					while (!CreateBuildingsInCorners(item.gameObject, notAnyone: true) && num < 300);
				}
				else if (item.name.Substring(0, 1) == "S")
				{
					CreateBuildingsInLine(item.gameObject, 90f, slope: true);
				}
				else
				{
					CreateBuildingsInLine(item.gameObject, 90f);
				}
			}
			_residential = false;
		}
	}

	public bool CreateBuildingsInCorners(GameObject child, bool notAnyone = false)
	{
		pB = null;
		int num = 0;
		int num2 = 0;
		float num3 = 0f;
		float num4 = Vector3.Distance(center, child.transform.position);
		int num5 = 0;
		int num6 = 0;
		float num7 = distCenter * (Mathf.Clamp(downTownSize, 50f, 200f) / 100f);
		while (num2 < 100)
		{
			num2++;
			if (num4 < num7 && withDowntownArea)
			{
				do
				{
					num5++;
					num6 = 0;
					do
					{
						num6++;
						num = Random.Range(0, EC.Length);
					}
					while (notAnyone && _ECS[num] > 0 && num6 < 2000);
				}
				while (_EC[num] != 0 && (num5 <= 100 || _EC[num] > 1) && (num5 <= 150 || _EC[num] > 2) && (num5 <= 200 || _EC[num] > 3) && num5 <= 250 && num5 < 300);
				num3 = GetWith(EC[num]);
				if (num3 <= 0f)
				{
					Debug.LogWarning("Error: EC: " + num);
					_EC[num] = 100;
					return false;
				}
				if (num3 <= 36.3f)
				{
					_EC[num]++;
					pB = EC[num];
					break;
				}
				continue;
			}
			do
			{
				num5++;
				do
				{
					num6++;
					num = Random.Range(0, EB.Length);
				}
				while (notAnyone && _EBS[num] >= 100 && num6 < 2000);
			}
			while (_EB[num] != 0 && (num5 <= 100 || _EB[num] > 1) && (num5 <= 150 || _EB[num] > 2) && (num5 <= 200 || _EB[num] > 3) && num5 <= 250 && num5 < 300);
			num3 = GetWith(EB[num]);
			if (num3 <= 0f)
			{
				Debug.LogWarning("Error: EB: " + num);
				_EB[num] = 100;
				return false;
			}
			if (num3 <= 36.3f)
			{
				_EB[num]++;
				pB = EB[num];
				break;
			}
		}
		GameObject gameObject = Object.Instantiate(pB, new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f));
		if (notAnyone && !TestBaseBuildindCornerOnTheSlope(gameObject.transform))
		{
			if (num4 < num7 && withDowntownArea)
			{
				_ECS[num] = 100;
				_EC[num]--;
			}
			else
			{
				_EBS[num] = 100;
				_EB[num]--;
			}
			Object.DestroyImmediate(gameObject);
			return false;
		}
		gameObject.name = gameObject.name;
		gameObject.transform.SetParent(child.transform);
		gameObject.transform.localPosition = new Vector3(0f - num3 * 0.5f, 0f, 0f);
		gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		nB++;
		float height = GetHeight(pB);
		if (height < 29.9f)
		{
			GameObject gameObject2 = new GameObject("Marcador");
			gameObject2.transform.SetParent(child.transform);
			gameObject2.transform.localPosition = new Vector3(0f, 0f, -36f);
			gameObject2.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			gameObject2.name = (36f - height).ToString();
			CreateBuildingsInLine(gameObject2, 90f);
		}
		else
		{
			float num8 = 36f - height;
			float z = 1f + num8 / height;
			gameObject.transform.localScale = new Vector3(1f, 1f, z);
		}
		if (num3 < 29.9f)
		{
			GameObject gameObject2 = new GameObject("Marcador");
			gameObject2.transform.SetParent(child.transform);
			gameObject2.transform.localPosition = new Vector3(0f - num3, 0f, 0f);
			gameObject2.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
			gameObject2.name = (36f - num3).ToString();
			CreateBuildingsInLine(gameObject2, 90f);
		}
		else
		{
			float num8 = 36f - num3;
			float z = 1f + num8 / num3;
			gameObject.transform.localScale = new Vector3(z, 1f, 1f);
		}
		return true;
	}

	private bool TestBaseBuildindCornerOnTheSlope(Transform buildingCornerOnTheSlope)
	{
		if (!buildingCornerOnTheSlope.Find("Base-Corner-0-Collider") && !buildingCornerOnTheSlope.Find("Base-Corner-03-Collider"))
		{
			return buildingCornerOnTheSlope.Find("Base-Corner-06-Collider");
		}
		return true;
	}

	private int RandRotation()
	{
		int num = 0;
		return Random.Range(0, 4) switch
		{
			3 => 180, 
			2 => 90, 
			1 => 270, 
			_ => 0, 
		};
	}

	public void CreateBuildingsInBlocks()
	{
		int num = 0;
		tempArray = (from g in Object.FindObjectsOfType(typeof(GameObject))
			select g as GameObject into g
			where g.name == "Blocks"
			select g).ToArray();
		GameObject[] array = tempArray;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Transform item in array[i].transform)
			{
				if (Random.Range(0, 20) > 5)
				{
					int num2 = 0;
					do
					{
						num2++;
						num = Random.Range(0, BK.Length);
					}
					while (_BK[num] != 0 && (num2 <= 125 || _BK[num] > 1) && (num2 <= 150 || _BK[num] > 2) && (num2 <= 200 || _BK[num] > 3) && num2 <= 250 && num2 < 300);
					_BK[num]++;
					Object.Instantiate(BK[num], item.position, item.rotation, item);
					nB++;
					continue;
				}
				for (int j = 1; j <= 4; j++)
				{
					GameObject gameObject = new GameObject("E");
					gameObject.transform.SetParent(item);
					if (j == 1)
					{
						gameObject.transform.localPosition = new Vector3(-36f, 0f, -36f);
						gameObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
					}
					if (j == 2)
					{
						gameObject.transform.localPosition = new Vector3(-36f, 0f, 36f);
						gameObject.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
					}
					if (j == 3)
					{
						gameObject.transform.localPosition = new Vector3(36f, 0f, 36f);
						gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
					}
					if (j == 4)
					{
						gameObject.transform.localPosition = new Vector3(36f, 0f, -36f);
						gameObject.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
					}
					CreateBuildingsInCorners(gameObject);
				}
			}
		}
	}

	public void CreateBuildingsInSuperBlocks()
	{
		int num = 0;
		tempArray = (from g in Object.FindObjectsOfType(typeof(GameObject))
			select g as GameObject into g
			where g.name == "SuperBlocks"
			select g).ToArray();
		GameObject[] array = tempArray;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Transform item in array[i].transform)
			{
				int num2 = 0;
				do
				{
					num2++;
					num = Random.Range(0, SB.Length);
				}
				while (_SB[num] != 0 && (num2 <= 125 || _SB[num] > 1) && (num2 <= 150 || _SB[num] > 2) && (num2 <= 200 || _SB[num] > 3) && num2 <= 250 && num2 < 300);
				_SB[num]++;
				Object.Instantiate(SB[num], item.position, item.rotation, item);
				nB++;
			}
		}
	}

	private void CreateBuildingsInLine(GameObject line, float angulo, bool slope = false)
	{
		int num = -1;
		GameObject[] array = new GameObject[50];
		string text = line.name;
		text = (slope ? line.name.Substring(1) : line.name);
		float num2 = ((!text.Contains(".")) ? float.Parse(text) : (float.Parse(text.Split('.')[0]) + float.Parse(text.Split('.')[1]) / float.Parse("1" + "0000000".Substring(0, text.Split('.')[1].Length))));
		float num3 = 0f;
		float num4 = 0f;
		int num5 = 0;
		float num6 = Vector3.Distance(center, line.transform.position);
		float num7 = distCenter * (Mathf.Clamp(downTownSize, 50f, 200f) / 100f);
		while (num5 < 100)
		{
			num5++;
			int num8 = 0;
			int num9 = 0;
			while (num8 < 200 && num3 <= num2 - 4f)
			{
				num8++;
				if (slope)
				{
					if (num6 < num7 && withDowntownArea)
					{
						do
						{
							num9++;
							numB = Random.Range(0, BCS.Length);
						}
						while (_BCS[numB] != 0 && (num9 <= 125 || _BCS[numB] > 1) && (num9 <= 150 || _BCS[numB] > 2) && (num9 <= 200 || _BCS[numB] > 3) && num9 <= 250 && num9 < 300);
						num4 = GetWith(BCS[numB]);
						if (num4 > 0f && num3 + num4 <= num2 + 4f)
						{
							pB = BCS[numB];
							_BCS[numB]++;
							break;
						}
					}
					else
					{
						do
						{
							num9++;
							numB = Random.Range(0, BBS.Length);
						}
						while (_BBS[numB] != 0 && (num9 <= 125 || _BBS[numB] > 1) && (num9 <= 150 || _BBS[numB] > 2) && (num9 <= 200 || _BBS[numB] > 3) && num9 <= 250 && num9 < 300);
						num4 = GetWith(BBS[numB]);
						if (num4 > 0f && num3 + num4 <= num2 + 4f)
						{
							pB = BBS[numB];
							_BBS[numB]++;
							break;
						}
					}
				}
				else if (num6 < num7 && withDowntownArea)
				{
					do
					{
						num9++;
						numB = Random.Range(0, BC.Length);
					}
					while (_BC[numB] != 0 && (num9 <= 125 || _BC[numB] > 1) && (num9 <= 150 || _BC[numB] > 2) && (num9 <= 200 || _BC[numB] > 3) && num9 <= 250 && num9 < 300);
					num4 = GetWith(BC[numB]);
					if (num4 > 0f && num3 + num4 <= num2 + 4f)
					{
						pB = BC[numB];
						_BC[numB]++;
						break;
					}
				}
				else if (_residential)
				{
					do
					{
						num9++;
						numB = Random.Range(0, BR.Length);
					}
					while (_BR[numB] != 0 && (num9 <= 100 || _BR[numB] > 1) && (num9 <= 150 || _BR[numB] > 2) && (num9 <= 200 || _BR[numB] > 3) && num9 <= 250 && num9 < 300);
					num4 = GetWith(BR[numB]);
					if (num4 <= 0f)
					{
						Debug.LogWarning("Error: BR: " + numB);
						_BR[numB]++;
					}
					else if (num3 + num4 <= num2 + 4f)
					{
						pB = BR[numB];
						_BR[numB]++;
						residential++;
						break;
					}
				}
				else
				{
					do
					{
						num9++;
						numB = Random.Range(0, BB.Length);
					}
					while (_BB[numB] != 0 && (num9 <= 100 || _BB[numB] > 1) && (num9 <= 150 || _BB[numB] > 2) && (num9 <= 200 || _BB[numB] > 3) && num9 <= 250 && num9 < 300);
					num4 = GetWith(BB[numB]);
					if (num4 <= 0f)
					{
						Debug.LogWarning("Error: BB: " + numB);
						_BB[numB]++;
					}
					if (num3 + num4 <= num2 + 4f)
					{
						pB = BB[numB];
						_BB[numB]++;
						break;
					}
				}
			}
			if (num8 >= 200 || num3 > num2 - 4f)
			{
				AdjustsWidth(array, num + 1, num2 - num3, 0f, slope);
				break;
			}
			num++;
			nB++;
			array[num] = Object.Instantiate(pB, new Vector3(0f, 0f, num3 + num4 * 0.5f), Quaternion.Euler(0f, angulo, 0f), line.transform);
			array[num].transform.SetParent(line.transform);
			array[num].transform.localPosition = new Vector3(0f, 0f, num3 + num4 * 0.5f);
			array[num].transform.localRotation = Quaternion.Euler(0f, angulo, 0f);
			num3 += num4;
			if (num3 > num2 - 6f)
			{
				AdjustsWidth(array, num + 1, num2 - num3, 0f, slope);
				break;
			}
		}
	}

	private float GetY(Transform pos, float width)
	{
		Vector3 vector = pos.transform.position + pos.transform.forward * 2f + pos.transform.up * 20f;
		float num = 20f;
		float num2 = 20f;
		if (Physics.Raycast(vector + pos.transform.right * width, Vector3.down, out var hitInfo, 40f))
		{
			num2 = hitInfo.distance;
		}
		if (Physics.Raycast(vector - pos.transform.right * width, Vector3.down, out hitInfo, 40f))
		{
			num = hitInfo.distance;
		}
		return pos.transform.localPosition.y + 20f - ((num2 < num) ? num2 : num);
	}

	private void CreateBuildingsInDoubleLine(GameObject line)
	{
		int num = -1;
		GameObject[] array = new GameObject[20];
		string text = line.name;
		float num2 = ((!text.Contains(".")) ? float.Parse(text) : (float.Parse(text.Split('.')[0]) + float.Parse(text.Split('.')[1]) / float.Parse("1" + "0000000".Substring(0, text.Split('.')[1].Length))));
		float num3 = 0f;
		float num4 = 0f;
		int num5 = 0;
		while (num5 < 100)
		{
			num5++;
			int num6 = 0;
			int num7 = 0;
			while (num6 < 200 && num3 <= num2 - 4f)
			{
				num6++;
				do
				{
					num7++;
					numB = Random.Range(0, MB.Length);
				}
				while (_MB[numB] != 0 && (num7 <= 100 || _MB[numB] > 1) && (num7 <= 150 || _MB[numB] > 2) && num7 <= 200 && num7 < 300);
				num4 = GetWith(MB[numB]);
				if (num4 <= 0f)
				{
					Debug.LogWarning("Error: MB: " + numB);
					_MB[numB]++;
				}
				else if (num3 + num4 <= num2 + 4f)
				{
					_MB[numB]++;
					break;
				}
			}
			if (num6 >= 200 || num3 > num2 - 4f)
			{
				AdjustsWidth(array, num + 1, num2 - num3, 0f);
				break;
			}
			num++;
			array[num] = Object.Instantiate(MB[numB], new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), line.transform);
			nB++;
			array[num].name = "building";
			array[num].transform.SetParent(line.transform);
			array[num].transform.localPosition = new Vector3(0f, 0f, num3 + num4 * 0.5f);
			array[num].transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
			num3 += num4;
			if (num3 > num2 - 6f)
			{
				AdjustsWidth(array, num + 1, num2 - num3, 0f);
			}
		}
	}

	private void CreateBuildingsInDouble()
	{
		tempArray = (from g in Object.FindObjectsOfType(typeof(GameObject))
			select g as GameObject into g
			where g.name == "Double"
			select g).ToArray();
		GameObject[] array = tempArray;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Transform item in array[i].transform)
			{
				float num = ((!item.name.Contains(".")) ? float.Parse(item.name) : (float.Parse(item.name.Split('.')[0]) + float.Parse(item.name.Split('.')[1]) / float.Parse("1" + "0000000".Substring(0, item.name.Split('.')[1].Length))));
				if (Random.Range(0, 10) < 5)
				{
					float height;
					do
					{
						numB = Random.Range(0, DC.Length);
						height = GetHeight(DC[numB]);
					}
					while (height > num / 2f);
					Object.Instantiate(DC[numB], item.transform.position, item.transform.rotation, item.transform);
					nB++;
					float height2;
					do
					{
						numB = Random.Range(0, DC.Length);
						height2 = GetHeight(DC[numB]);
					}
					while (height2 > num - (height + 26f));
					GameObject obj = Object.Instantiate(DC[numB], item.transform.position, item.rotation, item.transform);
					obj.transform.SetParent(item.transform);
					obj.transform.localPosition = new Vector3(0f, 0f, 0f - num);
					obj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
					GameObject gameObject = new GameObject(string.Concat(num - height - height2));
					gameObject.transform.SetParent(item.transform);
					gameObject.transform.localPosition = new Vector3(0f, 0f, 0f - (num - height2));
					gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
					gameObject.name = string.Concat(num - height - height2);
					CreateBuildingsInDoubleLine(gameObject);
					continue;
				}
				GameObject gameObject2 = new GameObject("Marcador");
				gameObject2.transform.SetParent(item);
				gameObject2.transform.localPosition = new Vector3(0f, 0f, 0f);
				gameObject2.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
				GameObject gameObject3;
				for (int j = 1; j <= 4; j++)
				{
					gameObject3 = new GameObject("E");
					gameObject3.transform.SetParent(gameObject2.transform);
					if (j == 1)
					{
						gameObject3.transform.localPosition = new Vector3(36f, 0f, 0f - num);
						gameObject3.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
					}
					if (j == 2)
					{
						gameObject3.transform.localPosition = new Vector3(36f, 0f, 0f);
						gameObject3.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
					}
					if (j == 3)
					{
						gameObject3.transform.localPosition = new Vector3(-36f, 0f, 0f);
						gameObject3.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
					}
					if (j == 4)
					{
						gameObject3.transform.localPosition = new Vector3(-36f, 0f, 0f - num);
						gameObject3.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
					}
					CreateBuildingsInCorners(gameObject3);
				}
				gameObject3 = new GameObject(string.Concat(num - 72f));
				gameObject3.transform.SetParent(gameObject2.transform);
				gameObject3.transform.localPosition = new Vector3(-36f, 0.001f, -36f);
				gameObject3.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
				CreateBuildingsInLine(gameObject3, 90f);
				gameObject3 = new GameObject(string.Concat(num - 72f));
				gameObject3.transform.SetParent(gameObject2.transform);
				gameObject3.transform.localPosition = new Vector3(36f, 0.001f, 0f - (num - 36f));
				gameObject3.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
				CreateBuildingsInLine(gameObject3, 90f);
			}
		}
	}

	private void AdjustsWidth(GameObject[] tBuildings, int quantity, float remainingMeters, float init, bool slope = false)
	{
		if (remainingMeters == 0f)
		{
			return;
		}
		float num = remainingMeters / (float)quantity;
		float num2 = init;
		for (int i = 0; i < quantity; i++)
		{
			float with = GetWith(tBuildings[i]);
			if (with > 0f)
			{
				float num3 = 1f + num / with;
				float num4 = with + num;
				tBuildings[i].transform.localPosition = new Vector3(tBuildings[i].transform.localPosition.x, tBuildings[i].transform.localPosition.y, num2 + num4 * 0.5f);
				tBuildings[i].transform.localScale = new Vector3(num3, 1f, 1f);
				num2 += num4;
				if (slope)
				{
					float y = GetY(tBuildings[i].transform, with * num3 * 0.5f);
					tBuildings[i].transform.position += new Vector3(0f, y, 0f);
				}
			}
		}
	}

	private float GetWith(GameObject building)
	{
		if (!building)
		{
			return 0f;
		}
		if (building.transform.GetComponent<MeshFilter>() != null)
		{
			if (building.transform.GetComponent<MeshFilter>().sharedMesh == null)
			{
				Debug.LogError("Error:  " + building.name + " does not have a mesh renderer at the root. The prefab must be the floor/base mesh. I nside it you place the building. More info: https://youtu.be/kVrWir_WjNY");
			}
			return building.transform.GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
		}
		Debug.LogError("Error:  " + building.name + " does not have a mesh renderer at the root. The prefab must be the floor/base mesh. I nside it you place the building. More info: https://youtu.be/kVrWir_WjNY");
		return 0f;
	}

	private float GetHeight(GameObject building)
	{
		if (building.GetComponent<MeshFilter>() != null)
		{
			return building.GetComponent<MeshFilter>().sharedMesh.bounds.size.z;
		}
		Debug.LogError("Error:  " + building.name + " does not have a mesh renderer at the root. The prefab must be the floor/base mesh. I nside it you place the building. More info: https://youtu.be/kVrWir_WjNY");
		return 0f;
	}

	public void DestroyBuildings()
	{
		DestryObjetcs("Marcador");
		DestryObjetcs("Blocks");
		DestryObjetcs("SuperBlocks");
		DestryObjetcs("Double");
	}

	private void DestryObjetcs(string tag)
	{
		tempArray = (from g in Object.FindObjectsOfType(typeof(GameObject))
			select g as GameObject into g
			where g.name == tag
			select g).ToArray();
		GameObject[] array = tempArray;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Transform item in array[i].transform)
			{
				for (int num = item.childCount - 1; num >= 0; num--)
				{
					Object.DestroyImmediate(item.GetChild(num).gameObject);
				}
			}
		}
	}
}
