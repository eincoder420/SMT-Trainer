using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI;

[Serializable]
public class vWaypoint : vPoint
{
	public List<vPoint> subPoints;

	public bool randomPatrolPoint;

	public bool rotateTo = true;

	public Vector3 GetRandomSubPoint()
	{
		int index = new System.Random(100).Next(0, subPoints.Count - 1);
		return GetSubPoint(index);
	}

	public Vector3 GetSubPoint(int index)
	{
		if (subPoints != null && subPoints.Count > 0 && index < subPoints.Count)
		{
			return subPoints[index].position;
		}
		return base.transform.position;
	}
}
