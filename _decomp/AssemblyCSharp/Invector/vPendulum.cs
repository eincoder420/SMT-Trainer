using System;
using System.Collections;
using UnityEngine;

namespace Invector;

public class vPendulum : MonoBehaviour
{
	[Range(0f, 360f)]
	public float angle = 90f;

	[Range(0f, 4f)]
	public float speed = 1.5f;

	public float startDelay;

	private Quaternion qStart;

	private Quaternion qEnd;

	private float startTime;

	private bool work;

	private bool working;

	private IEnumerator Start()
	{
		qStart = PendulumRotation(angle);
		qEnd = PendulumRotation(0f - angle);
		yield return new WaitForSeconds(startDelay);
		work = true;
	}

	private void FixedUpdate()
	{
		if (!work)
		{
			return;
		}
		if (!working)
		{
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, qEnd, speed);
			if (Vector3.Distance(base.transform.rotation.eulerAngles, qEnd.eulerAngles) < 0.1f)
			{
				working = true;
			}
		}
		else
		{
			startTime += Time.deltaTime;
			base.transform.rotation = Quaternion.Lerp(qStart, qEnd, (Mathf.Sin(startTime * speed + (float)Math.PI / 2f) + 1f) / 2f);
		}
	}

	private void resetTimer()
	{
		startTime = 0f;
	}

	private Quaternion PendulumRotation(float _angle)
	{
		Quaternion rotation = base.transform.rotation;
		float num = rotation.eulerAngles.z + _angle;
		if (num > 180f)
		{
			num -= 360f;
		}
		else if (num < -180f)
		{
			num += 360f;
		}
		rotation.eulerAngles = new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, num);
		return rotation;
	}
}
