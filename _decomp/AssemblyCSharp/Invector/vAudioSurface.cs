using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Invector;

public class vAudioSurface : ScriptableObject
{
	public AudioSource audioSource;

	public AudioMixerGroup audioMixerGroup;

	public List<string> TextureOrMaterialNames;

	public List<AudioClip> audioClips;

	public GameObject particleObject;

	private vFisherYatesRandom randomSource = new vFisherYatesRandom();

	public bool useStepMark;

	[vHideInInspector("useStepMark", false)]
	public GameObject stepMark;

	[vHideInInspector("useStepMark", false)]
	public LayerMask stepLayer;

	[vHideInInspector("useStepMark", false)]
	public float timeToDestroy = 5f;

	public vAudioSurface()
	{
		audioClips = new List<AudioClip>();
		TextureOrMaterialNames = new List<string>();
	}

	public virtual void SpawnSurfaceEffect(FootStepObject footStepObject)
	{
		if (randomSource == null)
		{
			randomSource = new vFisherYatesRandom();
		}
		if (footStepObject.spawnSoundEffect)
		{
			PlaySound(footStepObject);
		}
		if (footStepObject.spawnParticleEffect && (bool)particleObject && (bool)footStepObject.ground && stepLayer.ContainsLayer(footStepObject.ground.gameObject.layer))
		{
			SpawnParticle(footStepObject);
		}
		if (footStepObject.spawnStepMarkEffect && useStepMark)
		{
			StepMark(footStepObject);
		}
	}

	protected virtual void PlaySound(FootStepObject footStepObject)
	{
		if (audioClips != null && audioClips.Count != 0)
		{
			AudioSource audioSource = null;
			if (this.audioSource != null)
			{
				audioSource = Object.Instantiate(this.audioSource, footStepObject.sender.position, Quaternion.identity);
			}
			if ((bool)this.audioSource && audioMixerGroup != null)
			{
				audioSource.outputAudioMixerGroup = audioMixerGroup;
			}
			int index = randomSource.Next(audioClips.Count);
			audioSource.PlayOneShot(audioClips[index], footStepObject.volume);
		}
	}

	protected virtual void SpawnParticle(FootStepObject footStepObject)
	{
		Object.Instantiate(particleObject, footStepObject.sender.position, footStepObject.sender.rotation).transform.SetParent(vObjectContainer.root, worldPositionStays: true);
	}

	protected virtual void StepMark(FootStepObject footStep)
	{
		if (Physics.Raycast(footStep.sender.transform.position + new Vector3(0f, 0.25f, 0f), Vector3.down, out var hitInfo, 1f, stepLayer) && (bool)stepMark)
		{
			Quaternion quaternion = Quaternion.FromToRotation(footStep.sender.up, hitInfo.normal);
			GameObject gameObject = Object.Instantiate(stepMark, hitInfo.point, quaternion * footStep.sender.rotation);
			gameObject.transform.SetParent(vObjectContainer.root, worldPositionStays: true);
			Object.Destroy(gameObject, timeToDestroy);
		}
	}
}
