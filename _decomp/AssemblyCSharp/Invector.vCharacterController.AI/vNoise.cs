using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI;

[Serializable]
public class vNoise
{
	[Serializable]
	public class vAINoiseEvent : UnityEvent<vNoise>
	{
	}

	public bool isPlaying;

	public vAINoiseEvent onFinishNoise = new vAINoiseEvent();

	public float duration;

	public Vector3 position;

	public string noiseType;

	public float volume;

	public float minDistance;

	public float maxDistance;

	private float noiseFinishTime;

	public vNoise(string noiseType, Vector3 position, float volume, float minDistance, float maxDistance, float duration)
	{
		this.noiseType = noiseType;
		this.position = position;
		this.volume = volume;
		this.minDistance = minDistance;
		this.maxDistance = maxDistance;
		this.duration = duration;
		AddDuration(duration);
	}

	public void AddDuration(float duration)
	{
		noiseFinishTime = Time.time + duration;
	}

	public void CancelNoise()
	{
		noiseFinishTime = 0f;
	}

	public IEnumerator Play()
	{
		if (!isPlaying)
		{
			AddDuration(duration);
			isPlaying = true;
			while (noiseFinishTime > Time.time)
			{
				yield return null;
			}
			isPlaying = false;
			if (onFinishNoise != null)
			{
				onFinishNoise.Invoke(this);
			}
		}
	}
}
