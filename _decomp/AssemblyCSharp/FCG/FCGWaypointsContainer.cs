using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCG;

public class FCGWaypointsContainer : MonoBehaviour
{
	private TrafficSystem trafficSystem;

	public bool oneway;

	public bool doubleLine;

	[Range(1f, 5f)]
	public float width = 2f;

	public FCGWaypointsContainer[] doNotConnectTo;

	[Range(10f, 40f)]
	public float limitNodeDistance = 30f;

	private float widthToUse = 2f;

	[HideInInspector]
	public List<Transform> waypoints = new List<Transform>();

	[HideInInspector]
	public FCGWaypointsContainer[] nextWay0;

	[HideInInspector]
	public FCGWaypointsContainer[] nextWay1;

	[HideInInspector]
	public int[] nextWaySide0;

	[HideInInspector]
	public int[] nextWaySide1;

	[HideInInspector]
	public int rightHand;

	[HideInInspector]
	public bool bloked;

	[HideInInspector]
	public Transform nodeZeroCar0;

	[HideInInspector]
	public Transform nodeZeroCar1;

	[HideInInspector]
	public Transform nodeZeroWay0;

	[HideInInspector]
	public Transform nodeZeroWay1;

	[HideInInspector]
	public FCGWaypointsContainer[] directConnectSide = new FCGWaypointsContainer[2];

	private Vector3 nodeBegin;

	private Vector3 nodeEnd;

	[HideInInspector]
	public TrafficSystem.WpData wpData;

	private Vector3 oldPosition;

	private float _timer;

	public void NextWaysCloseOnly()
	{
		for (int num = 1; num >= 0; num--)
		{
			if ((!oneway || doubleLine || num != 0) && !directConnectSide[num])
			{
				int num2 = wpData.tf01.Length;
				if (num2 >= 1)
				{
					Vector3 a = Node(num, waypoints.Count - 1);
					for (int i = 0; i < num2; i++)
					{
						if (wpData.tsActive[i] && Vector3.Distance(a, wpData.tf01[i]) < 3f && !TestDoNotConnectTo(wpData.tsParent[i]) && !wpData.tsParent[i].TestDoNotConnectTo(this) && wpData.tsOneway[i] == oneway && wpData.tsOnewayDoubleLine[i] == doubleLine && !(wpData.tsParent[i].transform == base.transform))
						{
							if (num == 0)
							{
								nextWay0 = new FCGWaypointsContainer[1];
								nextWaySide0 = new int[1];
								nextWay0[0] = wpData.tsParent[i];
								nextWaySide0[0] = wpData.tsSide[i];
							}
							else
							{
								nextWay1 = new FCGWaypointsContainer[1];
								nextWaySide1 = new int[1];
								nextWay1[0] = wpData.tsParent[i];
								nextWaySide1[0] = wpData.tsSide[i];
							}
							directConnectSide[num] = wpData.tsParent[i];
							if (oneway)
							{
								wpData.tsParent[i].directConnectSide[0] = this;
							}
							break;
						}
					}
				}
			}
		}
	}

	public void ResetWay()
	{
		nextWay0 = null;
		nextWay1 = null;
		nextWay0 = new FCGWaypointsContainer[0];
		nextWay1 = new FCGWaypointsContainer[0];
		directConnectSide[0] = null;
		directConnectSide[1] = null;
		width = Mathf.Abs(width);
	}

	public void NextWays()
	{
		for (int num = 1; num >= 0; num--)
		{
			if ((!oneway || doubleLine || num != 0) && !(directConnectSide[num] != null))
			{
				if (num == 0)
				{
					nextWay0 = new FCGWaypointsContainer[0];
					nextWaySide0 = new int[0];
				}
				else
				{
					nextWay1 = new FCGWaypointsContainer[0];
					nextWaySide1 = new int[0];
				}
				Vector3 a = Node(num, waypoints.Count - 1);
				ArrayList arrayList = new ArrayList();
				ArrayList arrayList2 = new ArrayList();
				int num2 = wpData.tf01.Length;
				if (num2 >= 2)
				{
					arrayList.Clear();
					arrayList2.Clear();
					for (int i = 0; i < num2; i++)
					{
						if (!wpData.tsActive[i])
						{
							continue;
						}
						float num3 = Vector3.Distance(a, wpData.tf01[i]);
						if (num3 < limitNodeDistance && num3 >= 3f && (bool)wpData.tsParent[i] && (!wpData.tsOneway[i] || wpData.tsOnewayDoubleLine[i] || wpData.tsSide[i] != 0) && (!wpData.tsOneway[i] || wpData.tsOnewayDoubleLine[i] || !(wpData.tsParent[i].directConnectSide[0] != null)) && !TestDoNotConnectTo(wpData.tsParent[i]) && !wpData.tsParent[i].TestDoNotConnectTo(this) && (wpData.tsOneway[i] || !(wpData.tsParent[i].directConnectSide[(wpData.tsSide[i] != 1) ? 1u : 0u] != null)) && !(wpData.tsParent[i].transform == base.transform))
						{
							FCGWaypointsContainer fCGWaypointsContainer = wpData.tsParent[i];
							if ((Vector3.Distance(a, fCGWaypointsContainer.Node((wpData.tsSide[i] != 1) ? 1 : 0, 0)) > num3 || wpData.tsOneway[i]) && (Vector3.Distance(Node((num != 1) ? 1 : 0, waypoints.Count - 1), wpData.tf01[i]) > num3 || oneway))
							{
								arrayList.Add(wpData.tsParent[i]);
								arrayList2.Add(wpData.tsSide[i]);
							}
						}
					}
					int count = arrayList.Count;
					if (count >= 1)
					{
						FCGWaypointsContainer[] array = new FCGWaypointsContainer[count];
						int[] array2 = new int[count];
						for (int j = 0; j < count; j++)
						{
							array[j] = (FCGWaypointsContainer)arrayList[j];
							array2[j] = (int)arrayList2[j];
						}
						if (num == 0)
						{
							nextWay0 = array;
							nextWaySide0 = array2;
						}
						else
						{
							nextWay1 = array;
							nextWaySide1 = array2;
						}
					}
				}
			}
		}
		widthToUse = ((oneway && !doubleLine) ? 0f : ((rightHand == 0) ? Mathf.Abs(width) : (0f - Mathf.Abs(width))));
		bloked = (!oneway && (nextWay0.Length < 1 || nextWay1.Length < 1)) || (oneway && nextWay0.Length < 1 && nextWay1.Length < 1);
	}

	public bool TestDoNotConnectTo(FCGWaypointsContainer t)
	{
		if (doNotConnectTo == null)
		{
			return false;
		}
		if (doNotConnectTo.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < doNotConnectTo.Length; i++)
		{
			if (doNotConnectTo[i] == t)
			{
				return true;
			}
		}
		return false;
	}

	private float GetAngulo180(Transform origem, Vector3 target)
	{
		return Vector3.Angle(target - origem.position, origem.forward);
	}

	public void InvertNodesDirection(int hand_Right)
	{
		rightHand = hand_Right;
		widthToUse = ((rightHand == 0) ? Mathf.Abs(width) : (0f - Mathf.Abs(width)));
	}

	public void RefreshAllWayPoints()
	{
		if (Time.time - _timer < 0.2f)
		{
			return;
		}
		_timer = Time.time;
		if (!trafficSystem)
		{
			trafficSystem = Object.FindObjectOfType<TrafficSystem>();
			if (!trafficSystem)
			{
				Debug.LogError("Traffic System.prefab was not found in 'Assets/Fantastic City Generator/Traffic System'");
			}
		}
		trafficSystem.UpdateAllWayPoints();
	}

	public int GetTotalNodes()
	{
		return waypoints.Count - 1;
	}

	public Transform GetNodeZeroCar(int side)
	{
		if (side != 0)
		{
			return nodeZeroCar1;
		}
		return nodeZeroCar0;
	}

	public Transform GetNodeZeroWay(int side)
	{
		if (side != 0)
		{
			return nodeZeroWay1;
		}
		return nodeZeroWay0;
	}

	public Transform GetNodeZeroOldWay(int side)
	{
		if (side != 0)
		{
			return nodeZeroCar1.GetComponent<TrafficCar>().myOldWay;
		}
		return nodeZeroCar0.GetComponent<TrafficCar>().myOldWay;
	}

	public bool SetNodeZero(int side, Transform nodeWay, Transform nodeCar, bool force = false)
	{
		if (side == 0)
		{
			if (nodeZeroCar0 == null || force)
			{
				nodeZeroWay0 = nodeWay;
				nodeZeroCar0 = nodeCar;
			}
			return nodeZeroCar0 == nodeCar;
		}
		if (nodeZeroCar1 == null || force)
		{
			nodeZeroWay1 = nodeWay;
			nodeZeroCar1 = nodeCar;
		}
		return nodeZeroCar1 == nodeCar;
	}

	public bool UnSetNodeZero(int side, Transform carTransform, bool force = false)
	{
		if (side == 0)
		{
			if (nodeZeroCar0 == carTransform || force)
			{
				nodeZeroWay0 = null;
				nodeZeroCar0 = null;
			}
			return nodeZeroCar0 == null;
		}
		if (nodeZeroCar1 == carTransform || force)
		{
			nodeZeroWay1 = null;
			nodeZeroCar1 = null;
		}
		return nodeZeroCar1 == null;
	}

	public bool BookNodeZero(TrafficCar trafficCar)
	{
		if (trafficCar.sideAtual == 0)
		{
			if (SetNodeZero(0, trafficCar.myOldWay, trafficCar.transform))
			{
				return true;
			}
			if (!trafficCar.nodeSteerCarefully2 && !trafficCar.nodeSteerCarefully && nodeZeroCar0.GetComponent<TrafficCar>().Get_avanceNode() && trafficCar.GetBehind() != nodeZeroCar0)
			{
				SetNodeZero(0, trafficCar.myOldWay, trafficCar.transform, force: true);
			}
			if (!(nodeZeroCar0 == trafficCar.transform))
			{
				if (nodeZeroWay0 == trafficCar.myOldWay)
				{
					return trafficCar.myOldSideAtual == nodeZeroCar0.GetComponent<TrafficCar>().myOldSideAtual;
				}
				return false;
			}
			return true;
		}
		if (SetNodeZero(1, trafficCar.myOldWay, trafficCar.transform))
		{
			return true;
		}
		if (!trafficCar.nodeSteerCarefully && nodeZeroCar1.GetComponent<TrafficCar>().Get_avanceNode() && trafficCar.GetBehind() != nodeZeroCar1)
		{
			SetNodeZero(1, trafficCar.myOldWay, trafficCar.transform, force: true);
		}
		if (!(nodeZeroCar1 == trafficCar.transform))
		{
			if (nodeZeroWay1 == trafficCar.myOldWay)
			{
				return trafficCar.myOldSideAtual == nodeZeroCar1.GetComponent<TrafficCar>().myOldSideAtual;
			}
			return false;
		}
		return true;
	}

	public Vector3 AvanceNode(int side, int idx, float mts = 1f)
	{
		if ((!oneway && side == 0) || (oneway && rightHand != 0))
		{
			int index = waypoints.Count - 1 - idx;
			return waypoints[index].position - waypoints[index].transform.forward * mts - waypoints[index].transform.right * ((doubleLine && side == 0) ? (0f - widthToUse) : widthToUse);
		}
		return waypoints[idx].position + waypoints[idx].transform.forward * mts + waypoints[idx].transform.right * ((doubleLine && side == 0) ? (0f - widthToUse) : widthToUse);
	}

	public Quaternion NodeRotation(int side, int idx)
	{
		if ((!oneway && side == 1) || (oneway && rightHand == 0))
		{
			return Quaternion.LookRotation(waypoints[idx + 1].position - waypoints[idx].position);
		}
		int num = waypoints.Count - 1 - idx;
		return Quaternion.LookRotation(waypoints[num - 1].position - waypoints[num].position);
	}

	public Vector3 Node(int side, int idx, float nodeSteerCarefully = 0f)
	{
		if (oneway)
		{
			int index = ((rightHand == 0) ? idx : (waypoints.Count - 1 - idx));
			if (!doubleLine)
			{
				return waypoints[index].position;
			}
			return waypoints[index].position + ((side == 1) ? (waypoints[index].transform.right * widthToUse) : (waypoints[index].transform.right * (0f - widthToUse)));
		}
		int index2 = ((side == 1) ? idx : (waypoints.Count - 1 - idx));
		if (nodeSteerCarefully > 0f && idx == 0)
		{
			if (side == 1)
			{
				return waypoints[index2].position - waypoints[index2].transform.forward * nodeSteerCarefully + waypoints[index2].transform.right * ((side == 1) ? widthToUse : (0f - widthToUse));
			}
			return waypoints[index2].position + waypoints[index2].transform.forward * nodeSteerCarefully + waypoints[index2].transform.right * ((side == 1) ? widthToUse : (0f - widthToUse));
		}
		return waypoints[index2].position + waypoints[index2].transform.right * ((side == 1) ? widthToUse : (0f - widthToUse));
	}

	public void GetWaypoints()
	{
		waypoints = new List<Transform>();
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			waypoints.Add(componentsInChildren[i]);
		}
		WaypointsSetAngle();
	}

	public void WaypointsSetAngle()
	{
		int count = waypoints.Count;
		if (count > 1)
		{
			waypoints[0].LookAt(waypoints[1]);
			for (int i = 1; i < count - 1; i++)
			{
				waypoints[i].rotation = Quaternion.LookRotation(waypoints[i + 1].position - waypoints[i - 1].position);
			}
			waypoints[count - 1].rotation = Quaternion.LookRotation(waypoints[count - 1].position - waypoints[count - 2].position);
		}
	}
}
