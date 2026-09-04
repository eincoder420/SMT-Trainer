using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCG;

public class TrafficSystem : MonoBehaviour
{
	[Serializable]
	public class WpData
	{
		public bool[] tsActive;

		public Vector3[] tf01;

		public FCGWaypointsContainer[] tsParent;

		public bool[] tsOneway;

		public bool[] tsOnewayDoubleLine;

		public int[] tsSide;
	}

	[Serializable]
	public class WpDataSpawn
	{
		public Vector3 position;

		public Quaternion rotation;

		public float locateZ;

		public int side;

		public int node;

		public FCGWaypointsContainer wayScript;
	}

	public Transform player;

	[Header("Traffic Light:  0=Right  1=Left  2=Japan")]
	[Range(0f, 2f)]
	public int trafficLightHand;

	[Space(10f)]
	public GameObject[] IaCars;

	public int nVehicles;

	public int maxVehiclesWithPlayer = 50;

	[Range(100f, 200f)]
	public float around = 150f;

	private ArrayList spawnsPoints;

	private bool firstTime = true;

	private WpData wpData = new WpData();

	[HideInInspector]
	private List<WpDataSpawn> wpDataSpawn;

	private Transform downTowmPosition;

	public void UpdateAllWayPoints()
	{
		FCGWaypointsContainer[] array = UnityEngine.Object.FindObjectsOfType<FCGWaypointsContainer>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ResetWay();
			array[i].GetWaypoints();
		}
		GetWpData();
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].transform.childCount > 1)
			{
				array[j].wpData = wpData;
			}
		}
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k].transform.childCount > 1)
			{
				array[k].NextWaysCloseOnly();
			}
		}
		for (int l = 0; l < array.Length; l++)
		{
			if (array[l].transform.childCount > 1)
			{
				array[l].NextWays();
			}
		}
	}

	public void GetWpData()
	{
		FCGWaypointsContainer[] array = UnityEngine.Object.FindObjectsOfType<FCGWaypointsContainer>();
		wpData.tsActive = new bool[array.Length * 2];
		wpData.tf01 = new Vector3[array.Length * 2];
		wpData.tsParent = new FCGWaypointsContainer[array.Length * 2];
		wpData.tsOneway = new bool[array.Length * 2];
		wpData.tsOnewayDoubleLine = new bool[array.Length * 2];
		wpData.tsSide = new int[array.Length * 2];
		int num = -1;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].waypoints.Count > 1)
			{
				num++;
				if (!array[i].oneway || array[i].doubleLine)
				{
					wpData.tsActive[num] = true;
					wpData.tf01[num] = array[i].Node(0, 0);
					wpData.tsParent[num] = array[i];
					wpData.tsSide[num] = 0;
					wpData.tsOneway[num] = array[i].oneway;
					wpData.tsOnewayDoubleLine[num] = array[i].oneway && array[i].doubleLine;
				}
				else
				{
					wpData.tsActive[num] = false;
				}
				num++;
				wpData.tsActive[num] = true;
				wpData.tf01[num] = array[i].Node(1, 0);
				wpData.tsParent[num] = array[i];
				wpData.tsSide[num] = 1;
				wpData.tsOneway[num] = array[i].oneway;
				wpData.tsOnewayDoubleLine[num] = array[i].oneway && array[i].doubleLine;
			}
			else
			{
				num++;
				wpData.tsActive[num] = false;
				num++;
				wpData.tsActive[num] = false;
			}
		}
	}

	private void Start()
	{
		if ((bool)GameObject.Find("DTPosition"))
		{
			downTowmPosition = GameObject.Find("DTPosition").transform;
		}
		else
		{
			downTowmPosition = null;
		}
		LoadCars(trafficLightHand);
	}

	public void LoadCars(int right_Hand)
	{
		if (maxVehiclesWithPlayer == 0)
		{
			Debug.LogError("You need to set the maximum number of vehicles in the Traffic System");
			return;
		}
		FCGWaypointsContainer[] array = UnityEngine.Object.FindObjectsOfType<FCGWaypointsContainer>();
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			if (array[i].transform.childCount == 0)
			{
				UnityEngine.Object.DestroyImmediate(array[i].gameObject);
			}
		}
		UpdateAllWayPoints();
		if (!player)
		{
			Debug.LogWarning("You have not set the player in the Traffic System on Inspector. This drastically decreases performance in big cities");
		}
		GameObject gameObject = GameObject.Find("CarContainer");
		if (!gameObject)
		{
			gameObject = new GameObject("CarContainer");
			nVehicles = 0;
		}
		else
		{
			nVehicles = gameObject.transform.childCount;
		}
		trafficLightHand = right_Hand;
		DeffineDirection(right_Hand);
		wpDataSpawn = new List<WpDataSpawn>();
		array = UnityEngine.Object.FindObjectsOfType<FCGWaypointsContainer>();
		num = array.Length;
		for (int j = 0; j < num; j++)
		{
			if (array[j].bloked || array[j].waypoints.Count <= 1)
			{
				continue;
			}
			for (int k = 0; k <= 1; k++)
			{
				if (array[j].oneway && !array[j].doubleLine && (k != 1 || trafficLightHand != 0) && (k != 0 || trafficLightHand == 0))
				{
					continue;
				}
				for (int l = 0; l < array[j].waypoints.Count - 1; l++)
				{
					float num2 = Vector3.Distance(array[j].Node(k, l), array[j].Node(k, l + 1));
					if (num2 > 20f)
					{
						PlaceSpawnPoint(array[j], k, l, num2 / 2f);
					}
				}
			}
		}
		if (!Application.isPlaying)
		{
			firstTime = true;
		}
		if ((bool)player && Application.isPlaying)
		{
			InvokeRepeating("LoadCars2", 0f, 5f);
		}
		else
		{
			LoadCars2();
		}
	}

	private void PlaceSpawnPoint(FCGWaypointsContainer f, int side, int node, float locate)
	{
		wpDataSpawn.Add(new WpDataSpawn
		{
			locateZ = locate,
			position = f.AvanceNode(side, node, locate),
			rotation = f.NodeRotation(side, node),
			side = side,
			node = node,
			wayScript = f
		});
	}

	public void LoadCars2()
	{
		if (!player && !firstTime)
		{
			return;
		}
		if (firstTime && (bool)player && nVehicles > 0)
		{
			TrafficCar[] array = UnityEngine.Object.FindObjectsOfType<TrafficCar>();
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				array[i].GetComponent<TrafficCar>().distanceToSelfDestroy = around;
				array[i].GetComponent<TrafficCar>().player = player;
				array[i].GetComponent<TrafficCar>().tSystem = this;
				array[i].GetComponent<TrafficCar>().SelfDestructWhenAwayFromThePlayerInit();
			}
		}
		GameObject gameObject = GameObject.Find("CarContainer");
		if ((bool)gameObject)
		{
			nVehicles = gameObject.transform.childCount;
		}
		else
		{
			nVehicles = 0;
		}
		if (firstTime && nVehicles > 0)
		{
			firstTime = false;
		}
		else
		{
			if ((bool)player && nVehicles >= maxVehiclesWithPlayer)
			{
				return;
			}
			int count = wpDataSpawn.Count;
			_ = nVehicles;
			bool flag = UnityEngine.Random.Range(1, 20) < 10;
			Transform transform = new GameObject("verify").transform;
			for (int j = 0; j < count; j++)
			{
				int index = (flag ? (count - 1 - j) : j);
				if ((bool)player && nVehicles >= maxVehiclesWithPlayer)
				{
					break;
				}
				if ((bool)player)
				{
					float num2 = Vector3.Distance(wpDataSpawn[index].position, player.position);
					if (((bool)player && (num2 > around || (!firstTime && num2 < 80f))) || (!firstTime && InTheFieldOfVision(player.position, wpDataSpawn[index].position)))
					{
						continue;
					}
				}
				bool flag2 = false;
				if (firstTime || !Physics.Linecast(wpDataSpawn[index].wayScript.Node(wpDataSpawn[index].side, wpDataSpawn[index].node + 1) + Vector3.up * 1f, wpDataSpawn[index].wayScript.Node(wpDataSpawn[index].side, wpDataSpawn[index].node) + Vector3.up * 1f, out var _))
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(IaCars[Mathf.Clamp(UnityEngine.Random.Range(0, IaCars.Length), 0, IaCars.Length - 1)], wpDataSpawn[index].position + Vector3.up * 0.1f, wpDataSpawn[index].rotation);
					gameObject2.transform.SetParent(gameObject.transform);
					gameObject2.GetComponent<TrafficCar>().sideAtual = ((!wpDataSpawn[index].wayScript.oneway || !wpDataSpawn[index].wayScript.doubleLine || wpDataSpawn[index].wayScript.rightHand == 0) ? wpDataSpawn[index].side : ((wpDataSpawn[index].side != 1) ? 1 : 0));
					gameObject2.GetComponent<TrafficCar>().atualWay = wpDataSpawn[index].wayScript.transform;
					gameObject2.GetComponent<TrafficCar>().atualWayScript = wpDataSpawn[index].wayScript;
					gameObject2.GetComponent<TrafficCar>().currentNode = wpDataSpawn[index].node + 1;
					if ((bool)player)
					{
						gameObject2.GetComponent<TrafficCar>().distanceToSelfDestroy = around;
						gameObject2.GetComponent<TrafficCar>().player = player;
						gameObject2.GetComponent<TrafficCar>().tSystem = this;
						gameObject2.GetComponent<TrafficCar>().ActivateSelfDestructWhenAwayFromThePlayer();
					}
					nVehicles++;
				}
			}
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(transform.gameObject);
			}
			if (nVehicles > 0)
			{
				firstTime = false;
			}
			else if (UnityEngine.Object.FindObjectsOfType<FCGWaypointsContainer>().Length == 0)
			{
				Debug.Log("Need to generate the city again to use the updated traffic system");
			}
		}
	}

	private void Pause(Vector3 position)
	{
	}

	private bool InTheFieldOfVision(Vector3 source, Vector3 target)
	{
		if (Physics.Linecast(source + Vector3.up * 4f, target + Vector3.up * 4f, out var hitInfo))
		{
			return !Physics.Linecast(source + Vector3.up * 1f, target + Vector3.up * 1f, out hitInfo);
		}
		return true;
	}

	public void DeffineDirection(int hand_Right)
	{
		trafficLightHand = hand_Right;
		TFShiftHand2[] array = UnityEngine.Object.FindObjectsOfType<TFShiftHand2>();
		if (array.Length == 0 && (bool)GameObject.Find("Traffic-Light-T"))
		{
			Debug.LogError("It is not compatible with the previous traffic system.\nTo use the new system you need to generate the city again");
			UpdateAllWayPoints();
			return;
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RightHand(trafficLightHand);
		}
		FCGWaypointsContainer[] array2 = UnityEngine.Object.FindObjectsOfType<FCGWaypointsContainer>();
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].InvertNodesDirection(trafficLightHand);
		}
		GameObject[] array3 = (from g in UnityEngine.Object.FindObjectsOfType(typeof(GameObject))
			select g as GameObject into g
			where g.name.Equals("Road-Mark")
			select g).ToArray();
		for (int k = 0; k < array3.Length; k++)
		{
			if ((bool)array3[k].transform.Find("RoadMark"))
			{
				array3[k].transform.Find("RoadMark").gameObject.SetActive(trafficLightHand == 0);
			}
		}
		array3 = (from g in UnityEngine.Object.FindObjectsOfType(typeof(GameObject))
			select g as GameObject into g
			where g.name.Equals("Road-Mark-Rev")
			select g).ToArray();
		for (int l = 0; l < array3.Length; l++)
		{
			if ((bool)array3[l].transform.Find("RoadMarkRev"))
			{
				array3[l].transform.Find("RoadMarkRev").gameObject.SetActive(trafficLightHand != 0);
			}
		}
		UpdateAllWayPoints();
	}
}
