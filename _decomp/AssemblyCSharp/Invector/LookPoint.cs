using System;
using UnityEngine;

namespace Invector;

[Serializable]
public class LookPoint
{
	public string pointName;

	public Vector3 positionPoint;

	public Vector3 eulerAngle;

	public bool freeRotation;
}
