using UnityEngine;

namespace FIMSpace.FOptimizing;

public sealed class FLOD_AudioSource : FLOD_Base
{
	[Range(0f, 1f)]
	[Tooltip("Setted to zero will result with priority = 256 so marked as NOT important audio source, marked as 100% will result with priority level like audio source had when initialized")]
	public float PriorityFactor = 1f;

	[HideInInspector]
	public float Volume = 1f;

	private bool unPause;

	public FLOD_AudioSource()
	{
		SupportingTransitions = true;
		HeaderText = "AudioSource LOD Settings";
	}

	public override FLOD_Base GetLODInstance()
	{
		return ScriptableObject.CreateInstance<FLOD_AudioSource>();
	}

	public override FLOD_Base CreateNewCopy()
	{
		FLOD_AudioSource fLOD_AudioSource = ScriptableObject.CreateInstance<FLOD_AudioSource>();
		fLOD_AudioSource.CopyBase(this);
		fLOD_AudioSource.PriorityFactor = PriorityFactor;
		return fLOD_AudioSource;
	}

	public override void SetSameValuesAsComponent(Component component)
	{
		if (component == null)
		{
			Debug.LogError("[OPTIMIZERS] Given component is null instead of AudioSource!");
		}
		AudioSource audioSource = component as AudioSource;
		if (audioSource != null)
		{
			PriorityFactor = audioSource.priority;
			Volume = audioSource.volume;
		}
	}

	public override void InterpolateBetween(FLOD_Base lodA, FLOD_Base lodB, float transitionToB)
	{
		base.InterpolateBetween(lodA, lodB, transitionToB);
		FLOD_AudioSource fLOD_AudioSource = lodA as FLOD_AudioSource;
		FLOD_AudioSource fLOD_AudioSource2 = lodB as FLOD_AudioSource;
		PriorityFactor = fLOD_AudioSource2.PriorityFactor;
		Volume = Mathf.Lerp(fLOD_AudioSource.Volume, fLOD_AudioSource2.Volume, transitionToB);
	}

	public override void ApplySettingsToComponent(Component component, FLOD_Base initialSettingsReference)
	{
		FLOD_AudioSource fLOD_AudioSource = initialSettingsReference as FLOD_AudioSource;
		if (fLOD_AudioSource == null)
		{
			Debug.Log("[OPTIMIZERS] Target LOD is not AudioSource LOD or is null");
			return;
		}
		AudioSource audioSource = component as AudioSource;
		audioSource.priority = (int)Mathf.Lerp(255f, fLOD_AudioSource.PriorityFactor, PriorityFactor);
		audioSource.volume = fLOD_AudioSource.Volume * Volume;
		if (Disable)
		{
			if (audioSource.isPlaying && audioSource.loop)
			{
				audioSource.Pause();
				unPause = true;
			}
			audioSource.enabled = false;
		}
		else
		{
			if (unPause)
			{
				unPause = false;
				audioSource.UnPause();
			}
			audioSource.enabled = true;
		}
	}

	public override void SetAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source)
	{
		if (source as AudioSource == null)
		{
			Debug.LogError("[OPTIMIZERS] Given component for reference values is null or is not AudioSource Component!");
		}
		float valueForLODLevel = GetValueForLODLevel(1f, 0f, lodIndex - 1, lodCount);
		if (lodIndex > 0)
		{
			PriorityFactor = valueForLODLevel;
		}
		base.name = "LOD" + (lodIndex + 2);
		Volume = 1f;
	}

	public override void SetSettingsAsForCulled(Component component)
	{
		base.SetSettingsAsForCulled(component);
		PriorityFactor = 0f;
		Volume = 0f;
	}

	public override void SetSettingsAsForNearest(Component component)
	{
		base.SetSettingsAsForNearest(component);
		PriorityFactor = 1f;
		Volume = 1f;
	}

	public override FComponentLODsController GenerateLODController(Component target, FOptimizer_Base optimizer)
	{
		AudioSource audioSource = target as AudioSource;
		if (!audioSource)
		{
			audioSource = target.GetComponentInChildren<AudioSource>();
		}
		if ((bool)audioSource && !optimizer.ContainsComponent(audioSource))
		{
			return new FComponentLODsController(optimizer, audioSource, "Audio Source", this);
		}
		return null;
	}
}
