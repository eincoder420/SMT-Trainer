using System;

namespace UnityEngine.AzureSky;

[Serializable]
public sealed class AzureThunderSettings
{
	public Transform thunderPrefab;

	public AudioClip audioClip;

	public AnimationCurve lightFrequency;

	public float audioDelay;

	public Vector3 position;
}
