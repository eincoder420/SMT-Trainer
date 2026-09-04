using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI;

public class vWaypointArea : MonoBehaviour
{
	public List<vWaypoint> waypoints;

	public bool randomWayPoint;

	public vWaypoint GetRandomWayPoint()
	{
		System.Random random = new System.Random(100);
		List<vWaypoint> validPoints = GetValidPoints();
		int num = random.Next(0, waypoints.Count - 1);
		if (validPoints != null && validPoints.Count > 0 && num < validPoints.Count)
		{
			return validPoints[num];
		}
		return null;
	}

	public List<vWaypoint> GetValidPoints(bool reverse = false)
	{
		List<vWaypoint> list = waypoints.FindAll((vWaypoint node) => node.isValid);
		if (reverse)
		{
			list.Reverse();
		}
		return list;
	}

	public List<vPoint> GetValidSubPoints(vWaypoint waipoint, bool reverse = false)
	{
		List<vPoint> list = waipoint.subPoints.FindAll((vPoint node) => node.isValid);
		if (reverse)
		{
			list.Reverse();
		}
		return list;
	}

	public vWaypoint GetWayPoint(int index)
	{
		List<vWaypoint> validPoints = GetValidPoints();
		if (validPoints != null && validPoints.Count > 0 && index < validPoints.Count)
		{
			return validPoints[index];
		}
		return null;
	}
}
