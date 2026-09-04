using System;
using Invector.vCharacterController;
using UnityEngine;

namespace Invector;

public class vPlatform : vMonoBehaviour
{
	[Serializable]
	public class vPlatformPoint
	{
		public Transform transform;

		public bool useDefaultStayTime = true;

		[vHideInInspector("useDefaultstayTime", true)]
		public float stayTime;

		public bool useDefaultSpeed = true;

		[vHideInInspector("useDefaultSpeed", true)]
		public float speedToNextPoint = 1f;
	}

	public vPlatformPoint[] points;

	[Tooltip("Movement speed between points")]
	public float defaultSpeed = 1f;

	[Tooltip("Time to stay in current point")]
	public float defaultStayTime = 2f;

	[Tooltip("Index to Starting point")]
	public int startIndex;

	public bool pause;

	[HideInInspector]
	public bool canMove;

	private Vector3 oldEuler;

	private int index;

	private bool invert;

	private float currentTime;

	private float currentSpeed;

	private float dist;

	private float currentDist;

	private Transform targetTransform;

	private void OnDrawGizmos()
	{
		if (points == null || points.Length == 0 || startIndex >= points.Length)
		{
			return;
		}
		Transform transform = points[0].transform;
		Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
		if (!Application.isPlaying)
		{
			base.transform.position = points[startIndex].transform.position;
			base.transform.eulerAngles = points[startIndex].transform.eulerAngles;
		}
		vPlatformPoint[] array = points;
		foreach (vPlatformPoint vPlatformPoint in array)
		{
			if (vPlatformPoint.transform != null && vPlatformPoint.transform != transform)
			{
				Gizmos.DrawLine(transform.position, vPlatformPoint.transform.position);
				transform = vPlatformPoint.transform;
			}
		}
		array = points;
		foreach (vPlatformPoint vPlatformPoint2 in array)
		{
			if ((bool)vPlatformPoint2.transform)
			{
				Gizmos.matrix = Matrix4x4.TRS(vPlatformPoint2.transform.position, vPlatformPoint2.transform.rotation, base.transform.lossyScale);
				Gizmos.DrawCube(Vector3.zero, Vector3.one);
			}
		}
	}

	public void SetPause(bool value)
	{
		pause = value;
	}

	private void Start()
	{
		if (points.Length != 0 && startIndex < points.Length && points.Length >= 2)
		{
			base.transform.position = points[startIndex].transform.position;
			base.transform.eulerAngles = points[startIndex].transform.eulerAngles;
			oldEuler = base.transform.eulerAngles;
			int num = startIndex;
			if (startIndex + 1 < points.Length)
			{
				num++;
			}
			else if (startIndex - 1 > 0)
			{
				num--;
				invert = true;
			}
			dist = Vector3.Distance(base.transform.position, points[num].transform.position);
			targetTransform = points[num].transform;
			currentTime = (points[startIndex].useDefaultStayTime ? defaultStayTime : points[index].stayTime);
			currentSpeed = (points[startIndex].useDefaultSpeed ? defaultSpeed : points[index].speedToNextPoint);
			index = num;
			canMove = true;
		}
	}

	private void FixedUpdate()
	{
		if ((points.Length == 0 && !canMove) || pause)
		{
			return;
		}
		currentDist = Vector3.Distance(base.transform.position, targetTransform.position);
		if (currentTime <= 0f)
		{
			float num = Mathf.Clamp((100f - 100f * currentDist / dist) * 0.01f, 0f, 1f);
			base.transform.position = Vector3.MoveTowards(base.transform.position, targetTransform.position, currentSpeed * Time.deltaTime);
			if (!float.IsNaN(num) && !float.IsInfinity(num) && oldEuler != oldEuler + (targetTransform.eulerAngles - oldEuler))
			{
				base.transform.eulerAngles = Vector3.Lerp(oldEuler, targetTransform.eulerAngles, num);
			}
		}
		else
		{
			currentTime -= Time.fixedDeltaTime;
		}
		if (!(currentDist < 0.02f))
		{
			return;
		}
		currentSpeed = (points[index].useDefaultSpeed ? defaultSpeed : points[index].speedToNextPoint);
		currentTime = (points[index].useDefaultStayTime ? defaultStayTime : points[index].stayTime);
		if (!invert)
		{
			if (index + 1 < points.Length)
			{
				index++;
			}
			else
			{
				invert = true;
			}
		}
		else if (index - 1 >= 0)
		{
			index--;
		}
		else
		{
			invert = false;
		}
		dist = Vector3.Distance(targetTransform.position, points[index].transform.position);
		targetTransform = points[index].transform;
		oldEuler = base.transform.eulerAngles;
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.transform.parent != base.transform && other.transform.CompareTag("Player") && other.GetComponent<vCharacter>() != null)
		{
			other.transform.parent = base.transform;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.transform.parent == base.transform && other.transform.CompareTag("Player"))
		{
			other.transform.parent = null;
			other.transform.eulerAngles = new Vector3(0f, other.transform.eulerAngles.y, 0f);
		}
	}
}
