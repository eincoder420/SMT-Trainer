using UnityEngine;

namespace Invector.vCharacterController.AI;

[DisallowMultipleComponent]
[vClassHeader("Noise Object", "Call the method 'TriggerNoise' or use the option 'TriggerOnStart' to instantly trigger your noise", openClose = false)]
public class vNoiseObject : vMonoBehaviour
{
	public string noiseType = "noise";

	public float minDistance = 1f;

	public float maxDistance = 4f;

	public float volume = 1f;

	[Range(0.1f, 10f)]
	public float duration = 0.1f;

	public bool triggerOnStart;

	public bool looping;

	public vNoise.vAINoiseEvent onTriggerNoise;

	public vNoise.vAINoiseEvent onFinishNoise;

	private vNoise noise;

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.position, minDistance);
		if (maxDistance < minDistance)
		{
			minDistance = maxDistance;
		}
		Gizmos.DrawWireSphere(base.transform.position, maxDistance);
	}

	private void Start()
	{
		if (triggerOnStart)
		{
			TriggerNoise();
		}
	}

	public void TriggerNoise()
	{
		if (noise == null)
		{
			noise = new vNoise(noiseType, base.transform.position, volume, minDistance, maxDistance, duration);
			noise.onFinishNoise.AddListener(OnFinishNoise);
		}
		noise.position = base.transform.position;
		onTriggerNoise.Invoke(noise);
		vAINoiseManager.Instance.AddNoise(noise);
	}

	private void OnFinishNoise(vNoise noise)
	{
		onFinishNoise.Invoke(noise);
		if (looping)
		{
			Invoke("TriggerNoise", 0.1f);
		}
	}
}
