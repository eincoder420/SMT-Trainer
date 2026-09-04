using UnityEngine;

namespace FIMSpace.FOptimizing;

public sealed class FLOD_ParticleSystem : FLOD_Base
{
	[Space(4f)]
	[FPD_Percentage(0f, 1f, false, true)]
	[Tooltip("Percentage value of emmision rate for LOD level (percentage of initial emmission rate)")]
	public float EmmissionAmount = 1f;

	[FPD_Percentage(0f, 1f, false, true)]
	[Tooltip("Percentage value of burst rates for LOD level (percentage of initial burst rates)")]
	public float BurstsAmount = 1f;

	[FPD_Percentage(0f, 5f, true, true)]
	[Tooltip("Multiplier for particles size, if you make emmission smaller, particle size should become bigger to mask lower quality in distance")]
	public float ParticleSizeMul = 1f;

	[SerializeField]
	[HideInInspector]
	private ParticleSystem.Burst[] Bursts;

	[FPD_Percentage(0f, 1f, false, true)]
	[Tooltip("Percentage value of 'Max Particles' count for LOD level (percentage of initial 'Max Particles' count)")]
	public float MaxParticlAmount = 1f;

	[Tooltip("Percentage value of emmision rate over distance for LOD level (percentage of initial emmission rate)")]
	[FPD_Percentage(0f, 1f, false, true)]
	public float OverDistanceMul = 1f;

	[FPD_Percentage(0f, 1f, false, true)]
	[Tooltip("Percentage Alpha values of 'ColorOverLifetimeAlpha' for LOD level (percentage of initial 'ColorOverLifetimeAlpha' alpha keys on gradient)")]
	public float LifetimeAlpha = 1f;

	[SerializeField]
	[HideInInspector]
	private ParticleSystem.MinMaxGradient ColorOverLifetime;

	public FLOD_ParticleSystem()
	{
		DrawLowererSlider = true;
		SupportingTransitions = true;
		HeaderText = "Particle System LOD Settings";
	}

	public override FLOD_Base GetLODInstance()
	{
		FLOD_ParticleSystem fLOD_ParticleSystem = ScriptableObject.CreateInstance<FLOD_ParticleSystem>();
		fLOD_ParticleSystem.CopyBase(this);
		return fLOD_ParticleSystem;
	}

	public override FLOD_Base CreateNewCopy()
	{
		FLOD_ParticleSystem fLOD_ParticleSystem = ScriptableObject.CreateInstance<FLOD_ParticleSystem>();
		fLOD_ParticleSystem.CopyBase(this);
		fLOD_ParticleSystem.EmmissionAmount = EmmissionAmount;
		fLOD_ParticleSystem.OverDistanceMul = OverDistanceMul;
		fLOD_ParticleSystem.BurstsAmount = BurstsAmount;
		fLOD_ParticleSystem.Bursts = Bursts;
		fLOD_ParticleSystem.MaxParticlAmount = MaxParticlAmount;
		fLOD_ParticleSystem.LifetimeAlpha = LifetimeAlpha;
		fLOD_ParticleSystem.ColorOverLifetime = ColorOverLifetime;
		fLOD_ParticleSystem.ParticleSizeMul = ParticleSizeMul;
		return fLOD_ParticleSystem;
	}

	public override void SetSameValuesAsComponent(Component component)
	{
		if (component == null)
		{
			Debug.LogError("[OPTIMIZERS] Given component is null instead of ParticleSystem!");
		}
		ParticleSystem particleSystem = component as ParticleSystem;
		if (particleSystem != null)
		{
			EmmissionAmount = particleSystem.emission.rateOverTimeMultiplier;
			OverDistanceMul = particleSystem.emission.rateOverDistanceMultiplier;
			BurstsAmount = 1f;
			ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[particleSystem.emission.burstCount];
			particleSystem.emission.GetBursts(bursts);
			Bursts = bursts;
			MaxParticlAmount = particleSystem.main.maxParticles;
			LifetimeAlpha = 1f;
			ColorOverLifetime = particleSystem.colorOverLifetime.color;
			ParticleSizeMul = particleSystem.main.startSizeMultiplier;
		}
	}

	public override void InterpolateBetween(FLOD_Base lodA, FLOD_Base lodB, float transitionToB)
	{
		base.InterpolateBetween(lodA, lodB, transitionToB);
		FLOD_ParticleSystem fLOD_ParticleSystem = lodA as FLOD_ParticleSystem;
		FLOD_ParticleSystem fLOD_ParticleSystem2 = lodB as FLOD_ParticleSystem;
		EmmissionAmount = Mathf.Lerp(fLOD_ParticleSystem.EmmissionAmount, fLOD_ParticleSystem2.EmmissionAmount, transitionToB);
		OverDistanceMul = Mathf.Lerp(fLOD_ParticleSystem.OverDistanceMul, fLOD_ParticleSystem2.OverDistanceMul, transitionToB);
		BurstsAmount = Mathf.Lerp(fLOD_ParticleSystem.BurstsAmount, fLOD_ParticleSystem2.BurstsAmount, transitionToB);
		MaxParticlAmount = Mathf.Lerp(fLOD_ParticleSystem.MaxParticlAmount, fLOD_ParticleSystem2.MaxParticlAmount, transitionToB);
		LifetimeAlpha = Mathf.Lerp(fLOD_ParticleSystem.LifetimeAlpha, fLOD_ParticleSystem2.LifetimeAlpha, transitionToB);
		ParticleSizeMul = Mathf.Lerp(fLOD_ParticleSystem.ParticleSizeMul, fLOD_ParticleSystem2.ParticleSizeMul, transitionToB);
	}

	public override void ApplySettingsToComponent(Component component, FLOD_Base initialSettingsReference)
	{
		FLOD_ParticleSystem fLOD_ParticleSystem = initialSettingsReference as FLOD_ParticleSystem;
		if (fLOD_ParticleSystem == null)
		{
			Debug.Log("[OPTIMIZERS] Target LOD is not ParticleSystem LOD or is null (" + component.name + ")");
			return;
		}
		ParticleSystem particleSystem = component as ParticleSystem;
		ParticleSystemRenderer component2 = particleSystem.GetComponent<ParticleSystemRenderer>();
		if (Disable)
		{
			component2.enabled = false;
		}
		else
		{
			component2.enabled = true;
		}
		ParticleSystem.EmissionModule emission = particleSystem.emission;
		ParticleSystem.MainModule main = particleSystem.main;
		emission.rateOverTimeMultiplier = fLOD_ParticleSystem.EmmissionAmount * EmmissionAmount;
		emission.rateOverDistanceMultiplier = fLOD_ParticleSystem.OverDistanceMul * OverDistanceMul;
		if (fLOD_ParticleSystem.Bursts != null)
		{
			ParticleSystem.Burst[] array = new ParticleSystem.Burst[fLOD_ParticleSystem.Bursts.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = fLOD_ParticleSystem.Bursts[i];
				array[i].minCount = (short)((float)fLOD_ParticleSystem.Bursts[i].minCount * BurstsAmount);
				array[i].maxCount = (short)((float)fLOD_ParticleSystem.Bursts[i].maxCount * BurstsAmount);
			}
			emission.SetBursts(array);
		}
		main.maxParticles = (int)(fLOD_ParticleSystem.MaxParticlAmount * MaxParticlAmount);
		ParticleSystem.MinMaxGradient color = particleSystem.colorOverLifetime.color;
		if (fLOD_ParticleSystem.ColorOverLifetime.mode == ParticleSystemGradientMode.Gradient)
		{
			if (fLOD_ParticleSystem.ColorOverLifetime.gradient != null)
			{
				GradientAlphaKey[] array2 = new GradientAlphaKey[fLOD_ParticleSystem.ColorOverLifetime.gradient.alphaKeys.Length];
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j].alpha = fLOD_ParticleSystem.ColorOverLifetime.gradient.alphaKeys[j].alpha * LifetimeAlpha;
					array2[j].time = fLOD_ParticleSystem.ColorOverLifetime.gradient.alphaKeys[j].time;
				}
				color.gradient.SetKeys(particleSystem.colorOverLifetime.color.gradient.colorKeys, array2);
			}
		}
		else if (fLOD_ParticleSystem.ColorOverLifetime.gradientMin != null)
		{
			GradientAlphaKey[] array3 = new GradientAlphaKey[fLOD_ParticleSystem.ColorOverLifetime.gradientMin.alphaKeys.Length];
			for (int k = 0; k < array3.Length; k++)
			{
				color.gradientMin.alphaKeys[k].alpha = fLOD_ParticleSystem.ColorOverLifetime.gradientMin.alphaKeys[k].alpha * LifetimeAlpha;
				color.gradientMin.alphaKeys[k].time = fLOD_ParticleSystem.ColorOverLifetime.gradientMin.alphaKeys[k].time;
			}
			color.gradientMin.SetKeys(particleSystem.colorOverLifetime.color.gradient.colorKeys, array3);
			array3 = new GradientAlphaKey[fLOD_ParticleSystem.ColorOverLifetime.gradientMax.alphaKeys.Length];
			for (int l = 0; l < array3.Length; l++)
			{
				color.gradientMax.alphaKeys[l].alpha = fLOD_ParticleSystem.ColorOverLifetime.gradientMax.alphaKeys[l].alpha * LifetimeAlpha;
				color.gradientMax.alphaKeys[l].time = fLOD_ParticleSystem.ColorOverLifetime.gradientMax.alphaKeys[l].time;
			}
			color.gradientMax.SetKeys(particleSystem.colorOverLifetime.color.gradient.colorKeys, array3);
		}
		ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
		colorOverLifetime.color = color;
		main.startSizeMultiplier = fLOD_ParticleSystem.ParticleSizeMul * ParticleSizeMul;
		ToCullDelay = particleSystem.main.startLifetime.constantMax;
	}

	public override void SetAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source)
	{
		if (source as ParticleSystem == null)
		{
			Debug.LogError("[OPTIMIZERS] Given component for reference values is null or is not ParticleSystem Component!");
		}
		float valueForLODLevel = GetValueForLODLevel(1f, 0f, lodIndex, lodCount);
		float num = (BurstsAmount = (OverDistanceMul = (EmmissionAmount = valueForLODLevel * QualityLowerer)));
		MaxParticlAmount = Mathf.Min(1f, valueForLODLevel * 1.5f);
		ParticleSizeMul = 1.75f - num * 0.75f;
		base.name = "LOD" + (lodIndex + 2);
	}

	public override void SetSettingsAsForCulled(Component component)
	{
		base.SetSettingsAsForCulled(component);
		EmmissionAmount = 0f;
		OverDistanceMul = 0f;
		BurstsAmount = 0f;
		MaxParticlAmount = 0f;
		ParticleSizeMul = 1.5f;
		LifetimeAlpha = 0f;
	}

	public override void SetSettingsAsForHidden(Component component)
	{
		base.SetSettingsAsForHidden(component);
		MaxParticlAmount = 0.1f;
	}

	public override FComponentLODsController GenerateLODController(Component target, FOptimizer_Base optimizer)
	{
		ParticleSystem particleSystem = target as ParticleSystem;
		if (!particleSystem)
		{
			particleSystem = target.GetComponentInChildren<ParticleSystem>();
		}
		if ((bool)particleSystem && !optimizer.ContainsComponent(particleSystem))
		{
			return new FComponentLODsController(optimizer, particleSystem, "Particles", this);
		}
		return null;
	}
}
