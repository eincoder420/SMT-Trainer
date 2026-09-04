using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI;

public class vAINoiseManager : MonoBehaviour
{
	public delegate void NoiseOperator(vNoise noise);

	private static vAINoiseManager _instance;

	public List<vNoise> noises;

	public List<vAINoiseListener> noiseListeners = new List<vAINoiseListener>();

	public static vAINoiseManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindObjectOfType<vAINoiseManager>();
			}
			if (_instance == null)
			{
				_instance = new GameObject("AI Noise Manager").AddComponent<vAINoiseManager>();
				_instance.noises = new List<vNoise>();
			}
			return _instance;
		}
	}

	public event NoiseOperator OnAddNoise;

	public event NoiseOperator OnRemoveNoise;

	public void AddListener(vAINoiseListener listener)
	{
		if (!noiseListeners.Contains(listener))
		{
			OnAddNoise += listener.AddNoise;
			OnRemoveNoise += listener.RemoveNoise;
			noiseListeners.Add(listener);
		}
	}

	public void RemoveListener(vAINoiseListener listener)
	{
		if (noiseListeners.Contains(listener))
		{
			OnAddNoise -= listener.AddNoise;
			OnRemoveNoise -= listener.RemoveNoise;
			noiseListeners.Remove(listener);
		}
	}

	public void AddNoise(vNoise noise)
	{
		if (noises == null)
		{
			noises = new List<vNoise>();
		}
		if (noises.Contains(noise))
		{
			noises[noises.IndexOf(noise)].AddDuration(noise.duration);
		}
		else
		{
			noise.onFinishNoise.AddListener(RemoveNoise);
			noises.Add(noise);
			this.OnAddNoise?.Invoke(noise);
		}
		if (!noise.isPlaying)
		{
			StartCoroutine(noise.Play());
		}
	}

	public void RemoveNoise(vNoise noise)
	{
		if (noises == null)
		{
			noises = new List<vNoise>();
		}
		if (noises.Contains(noise))
		{
			this.OnRemoveNoise?.Invoke(noise);
			noises.Remove(noise);
		}
	}
}
