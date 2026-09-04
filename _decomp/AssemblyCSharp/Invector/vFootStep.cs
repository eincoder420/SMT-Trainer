using System.Collections.Generic;
using UnityEngine;

namespace Invector;

public class vFootStep : vFootStepBase
{
	public AnimationType animationType;

	public bool debugTextureName;

	[SerializeField]
	[Range(0f, 1f)]
	protected float _volume = 1f;

	[vHelpBox("Enable or disable spawn particle when foot step is triggered", vHelpBoxAttribute.MessageType.None)]
	[SerializeField]
	protected bool _spawnParticle = true;

	[vHelpBox("Enable or disable spawn step mark when foot step is triggered", vHelpBoxAttribute.MessageType.None)]
	[SerializeField]
	protected bool _spawnStepMark = true;

	[vHelpBox("The step effect is spawned from on trigger enter event of the Foot Step Triggers. If you need to play step sound only by external events you need to disable this variable.<b>\n*Disable this to play step sound using animation events</b>", vHelpBoxAttribute.MessageType.None)]
	[SerializeField]
	protected bool _useTriggerEnter = true;

	protected int surfaceIndex;

	protected Terrain terrain;

	protected TerrainCollider terrainCollider;

	protected TerrainData terrainData;

	protected Vector3 terrainPos;

	public vFootStepTrigger leftFootTrigger;

	public vFootStepTrigger rightFootTrigger;

	public Transform currentStep;

	public List<vFootStepTrigger> footStepTriggers;

	protected FootStepObject currentFootStep;

	public float Volume
	{
		get
		{
			return _volume;
		}
		set
		{
			_volume = value;
		}
	}

	public bool SpawnParticle
	{
		get
		{
			return _spawnParticle;
		}
		set
		{
			_spawnParticle = value;
		}
	}

	public bool SpawnStepMark
	{
		get
		{
			return _spawnStepMark;
		}
		set
		{
			_spawnStepMark = value;
		}
	}

	protected virtual void Start()
	{
		InitFootStep();
	}

	public virtual void InitFootStep()
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		if (animationType == AnimationType.Humanoid)
		{
			if (leftFootTrigger == null && rightFootTrigger == null)
			{
				Debug.Log("Missing FootStep Sphere Trigger, please unfold the FootStep Component to create the triggers.");
				return;
			}
			leftFootTrigger.trigger.isTrigger = true;
			rightFootTrigger.trigger.isTrigger = true;
			Physics.IgnoreCollision(leftFootTrigger.trigger, rightFootTrigger.trigger);
			foreach (Collider collider in componentsInChildren)
			{
				if (collider.enabled && collider.gameObject != leftFootTrigger.gameObject)
				{
					Physics.IgnoreCollision(leftFootTrigger.trigger, collider);
				}
				if (collider.enabled && collider.gameObject != rightFootTrigger.gameObject)
				{
					Physics.IgnoreCollision(rightFootTrigger.trigger, collider);
				}
			}
			return;
		}
		foreach (Collider collider2 in componentsInChildren)
		{
			for (int k = 0; k < footStepTriggers.Count; k++)
			{
				vFootStepTrigger vFootStepTrigger2 = footStepTriggers[k];
				vFootStepTrigger2.trigger.isTrigger = true;
				if (collider2.enabled && collider2.gameObject != vFootStepTrigger2.gameObject)
				{
					Physics.IgnoreCollision(vFootStepTrigger2.trigger, collider2);
				}
			}
		}
	}

	protected virtual void UpdateTerrainInfo(Terrain newTerrain)
	{
		if (terrain == null || terrain != newTerrain)
		{
			terrain = newTerrain;
			if (terrain != null)
			{
				terrainData = terrain.terrainData;
				terrainPos = terrain.transform.position;
				terrainCollider = terrain.GetComponent<TerrainCollider>();
			}
		}
	}

	protected virtual float[] GetTextureMix(FootStepObject footStepObj)
	{
		UpdateTerrainInfo(footStepObj.terrain);
		Vector3 position = footStepObj.sender.position;
		int x = (int)((position.x - terrainPos.x) / terrainData.size.x * (float)terrainData.alphamapWidth);
		int y = (int)((position.z - terrainPos.z) / terrainData.size.z * (float)terrainData.alphamapHeight);
		if (!terrainCollider.bounds.Contains(position))
		{
			return new float[0];
		}
		float[,,] alphamaps = terrainData.GetAlphamaps(x, y, 1, 1);
		float[] array = new float[alphamaps.GetUpperBound(2) + 1];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = alphamaps[0, 0, i];
		}
		return array;
	}

	protected virtual int GetMainTexture(FootStepObject footStepObj)
	{
		float[] textureMix = GetTextureMix(footStepObj);
		if (textureMix == null)
		{
			return -1;
		}
		float num = 0f;
		int result = 0;
		for (int i = 0; i < textureMix.Length; i++)
		{
			if (textureMix[i] > num)
			{
				result = i;
				num = textureMix[i];
			}
		}
		return result;
	}

	protected virtual void OnDestroy()
	{
		if (leftFootTrigger != null)
		{
			Object.Destroy(leftFootTrigger.gameObject);
		}
		if (rightFootTrigger != null)
		{
			Object.Destroy(rightFootTrigger.gameObject);
		}
		if (footStepTriggers == null || footStepTriggers.Count <= 0)
		{
			return;
		}
		foreach (vFootStepTrigger footStepTrigger in footStepTriggers)
		{
			Object.Destroy(footStepTrigger.gameObject);
		}
	}

	public override void StepOnTerrain(FootStepObject footStepObject)
	{
		if (currentStep != null && currentStep == footStepObject.sender && _useTriggerEnter)
		{
			return;
		}
		currentStep = footStepObject.sender;
		surfaceIndex = GetMainTexture(footStepObject);
		if (surfaceIndex == -1)
		{
			return;
		}
		string text = (footStepObject.name = ((terrainData != null && terrainData.terrainLayers.Length != 0) ? terrainData.terrainLayers[surfaceIndex].diffuseTexture.name : ""));
		currentFootStep = footStepObject;
		if (_useTriggerEnter)
		{
			PlayFootStepEffect();
			if (debugTextureName)
			{
				Debug.Log(terrain.name + " " + text);
			}
		}
	}

	public override void StepOnMesh(FootStepObject footStepObject)
	{
		if (currentStep != null && currentStep == footStepObject.sender && _useTriggerEnter)
		{
			return;
		}
		currentStep = footStepObject.sender;
		currentFootStep = footStepObject;
		if (_useTriggerEnter)
		{
			PlayFootStepEffect();
			if (debugTextureName)
			{
				Debug.Log(footStepObject.name);
			}
		}
	}

	public override void PlayFootStepEffect()
	{
		if (currentFootStep != null)
		{
			currentFootStep.volume = Volume;
			currentFootStep.spawnParticleEffect = SpawnParticle;
			currentFootStep.spawnStepMarkEffect = SpawnStepMark;
			SpawnSurfaceEffect(currentFootStep);
		}
	}

	public override void PlayFootStep(AnimationEvent evt)
	{
		if ((double)evt.animatorClipInfo.weight > 0.5)
		{
			PlayFootStepEffect();
		}
	}

	public override void PlayFootStepLeft(AnimationEvent evt)
	{
		if ((double)evt.animatorClipInfo.weight > 0.5)
		{
			currentFootStep.sender = leftFootTrigger.transform;
			PlayFootStepEffect();
		}
	}

	public override void PlayFootStepRight(AnimationEvent evt)
	{
		if ((double)evt.animatorClipInfo.weight > 0.15)
		{
			currentFootStep.sender = rightFootTrigger.transform;
			PlayFootStepEffect();
		}
	}
}
