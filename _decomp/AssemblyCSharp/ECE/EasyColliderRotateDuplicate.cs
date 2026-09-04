using System;
using UnityEngine;

namespace ECE;

[Serializable]
public class EasyColliderRotateDuplicate
{
	public enum ROTATE_AXIS
	{
		X,
		Y,
		Z
	}

	public bool enabled;

	public ROTATE_AXIS axis;

	public int NumberOfDuplications = 4;

	public float StartRotation;

	public float EndRotation = 360f;

	public GameObject pivot;

	public GameObject attachTo;
}
