using System;
using UnityEngine;

namespace FIMSpace.FOptimizing;

public sealed class FLOD_Light : FLOD_Base
{
	public enum EOptLightMode
	{
		Auto,
		Important,
		NotImportant
	}

	[Space(4f)]
	[FPD_Percentage(0f, 1f, false, true)]
	[Tooltip("Percentage value of light intensity for LOD level (percentage of initial light intensity)")]
	public float IntensityMul = 1f;

	[FPD_Percentage(0f, 1f, false, true)]
	[Tooltip("Percentage value of light range for LOD level (percentage of initial light range)")]
	public float RangeMul = 1f;

	[Space(3f)]
	public LightShadows ShadowsMode = LightShadows.Soft;

	[FPD_Percentage(0f, 1f, false, true)]
	[Tooltip("Percentage value of shadows intensity for LOD level (percentage of initial shadow value)")]
	public float ShadowsStrength = 1f;

	public EOptLightMode RenderMode;

	[HideInInspector]
	[Tooltip("If component should change intensity and range of light component (disable if you using flickering or something)")]
	public bool ChangeIntensity = true;

	public FLOD_Light()
	{
		SupportingTransitions = true;
		HeaderText = "Light LOD Settings";
		CustomEditor = true;
	}

	public override FLOD_Base GetLODInstance()
	{
		return ScriptableObject.CreateInstance<FLOD_Light>();
	}

	public override FLOD_Base CreateNewCopy()
	{
		FLOD_Light fLOD_Light = ScriptableObject.CreateInstance<FLOD_Light>();
		fLOD_Light.CopyBase(this);
		fLOD_Light.IntensityMul = IntensityMul;
		fLOD_Light.RangeMul = RangeMul;
		fLOD_Light.ShadowsMode = ShadowsMode;
		fLOD_Light.ShadowsStrength = ShadowsStrength;
		fLOD_Light.RenderMode = RenderMode;
		fLOD_Light.ChangeIntensity = ChangeIntensity;
		return fLOD_Light;
	}

	public override void SetSameValuesAsComponent(Component component)
	{
		if (component == null)
		{
			Debug.LogError("[OPTIMIZERS] Given component is null instead of Light!");
		}
		Light light = component as Light;
		if (light != null)
		{
			IntensityMul = light.intensity;
			RangeMul = light.range;
			ShadowsMode = light.shadows;
			ShadowsStrength = light.shadowStrength;
			RenderMode = (EOptLightMode)light.renderMode;
		}
	}

	public override void InterpolateBetween(FLOD_Base lodA, FLOD_Base lodB, float transitionToB)
	{
		base.InterpolateBetween(lodA, lodB, transitionToB);
		FLOD_Light fLOD_Light = lodA as FLOD_Light;
		FLOD_Light fLOD_Light2 = lodB as FLOD_Light;
		if (ChangeIntensity)
		{
			IntensityMul = Mathf.Lerp(fLOD_Light.IntensityMul, fLOD_Light2.IntensityMul, transitionToB);
			RangeMul = Mathf.Lerp(fLOD_Light.RangeMul, fLOD_Light2.RangeMul, transitionToB);
		}
		if (fLOD_Light2.ShadowsMode == LightShadows.None)
		{
			fLOD_Light2.ShadowsStrength = 0f;
		}
		ShadowsStrength = Mathf.Lerp(fLOD_Light.ShadowsStrength, fLOD_Light2.ShadowsStrength, transitionToB);
		if (fLOD_Light2.ShadowsStrength > 0f)
		{
			if (fLOD_Light.ShadowsMode == LightShadows.None && transitionToB >= 1f)
			{
				RenderMode = fLOD_Light2.RenderMode;
			}
			ShadowsMode = fLOD_Light2.ShadowsMode;
		}
		if (RenderMode == EOptLightMode.Important)
		{
			if (transitionToB >= 1f)
			{
				RenderMode = fLOD_Light2.RenderMode;
			}
		}
		else if (fLOD_Light2.RenderMode == EOptLightMode.Important || fLOD_Light2.RenderMode == EOptLightMode.Auto)
		{
			RenderMode = fLOD_Light2.RenderMode;
		}
		if (transitionToB >= 1f)
		{
			ShadowsMode = fLOD_Light2.ShadowsMode;
			RenderMode = fLOD_Light2.RenderMode;
		}
		else if (transitionToB <= 0f)
		{
			ShadowsMode = fLOD_Light.ShadowsMode;
			RenderMode = fLOD_Light.RenderMode;
		}
	}

	public override void ApplySettingsToComponent(Component component, FLOD_Base initialSettingsReference)
	{
		FLOD_Light fLOD_Light = initialSettingsReference as FLOD_Light;
		if (fLOD_Light == null)
		{
			Debug.Log("[OPTIMIZERS] Target LOD is not LightLOD or is null");
			return;
		}
		Light light = component as Light;
		if (ChangeIntensity)
		{
			light.intensity = IntensityMul * fLOD_Light.IntensityMul;
			light.range = RangeMul * fLOD_Light.RangeMul;
		}
		light.shadowStrength = ShadowsStrength * fLOD_Light.ShadowsStrength;
		light.shadows = ShadowsMode;
		light.renderMode = (LightRenderMode)RenderMode;
		if (Disable)
		{
			light.enabled = false;
		}
		else
		{
			light.enabled = true;
		}
	}

	public override void SetAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source)
	{
		Light light = source as Light;
		if (light == null)
		{
			Debug.LogError("[OPTIMIZERS] Given component for reference values is null or is not Light Component!");
		}
		float valueForLODLevel = GetValueForLODLevel(1f, 0f, lodIndex - 2, lodCount);
		base.name = "LOD" + (lodIndex + 2);
		if (lodIndex > 2 && lodCount > 2)
		{
			RangeMul = valueForLODLevel;
			ShadowsStrength = valueForLODLevel;
		}
		ShadowsMode = light.shadows;
		RenderMode = (EOptLightMode)light.renderMode;
		if (lodCount == 2 && light.shadows == LightShadows.Soft)
		{
			ShadowsMode = LightShadows.Hard;
		}
		if (lodCount > 2 && light.shadows == LightShadows.Soft)
		{
			ShadowsMode = LightShadows.Hard;
		}
		if (light.renderMode == LightRenderMode.ForcePixel)
		{
			RenderMode = EOptLightMode.Auto;
		}
		if (lodIndex > 0 && light.renderMode == LightRenderMode.ForcePixel)
		{
			RenderMode = EOptLightMode.Auto;
		}
		if (lodIndex >= lodCount - 2 && lodCount > 2)
		{
			ShadowsMode = LightShadows.None;
			ShadowsStrength = 0f;
		}
		if (lodIndex >= 1 && lodCount == 3)
		{
			RenderMode = EOptLightMode.NotImportant;
		}
		if (lodIndex >= 2)
		{
			RenderMode = EOptLightMode.NotImportant;
		}
		if (RenderMode == EOptLightMode.NotImportant)
		{
			IntensityMul = 0.4f;
			RangeMul = 0.5f;
		}
	}

	public override void SetSettingsAsForCulled(Component component)
	{
		base.SetSettingsAsForCulled(component);
		IntensityMul = 0f;
		RangeMul = 0f;
		ShadowsStrength = 0f;
		ShadowsMode = LightShadows.None;
		RenderMode = EOptLightMode.NotImportant;
	}

	public override void SetSettingsAsForNearest(Component component)
	{
		base.SetSettingsAsForNearest(component);
		Light light = component as Light;
		ShadowsMode = light.shadows;
		RenderMode = (EOptLightMode)light.renderMode;
	}

	public override void SetSettingsAsForHidden(Component component)
	{
		base.SetSettingsAsForHidden(component);
		Disable = true;
	}

	public override void AssignToggler(FLOD_Base reference)
	{
		FLOD_Light fLOD_Light = reference as FLOD_Light;
		if (fLOD_Light != null)
		{
			ChangeIntensity = fLOD_Light.ChangeIntensity;
		}
	}

	public override void DrawTogglers(FComponentLODsController lodsController)
	{
	}

	public override void EditorWindow()
	{
	}

	public override FComponentLODsController GenerateLODController(Component target, FOptimizer_Base optimizer)
	{
		Light light = target as Light;
		if (!light)
		{
			light = target.gameObject.GetComponentInChildren<Light>();
		}
		if ((bool)light && !optimizer.ContainsComponent(light))
		{
			optimizer.DetectionRadius = light.range;
			if (optimizer.transform.lossyScale.x != 0f)
			{
				optimizer.DetectionRadius *= 1f / optimizer.transform.lossyScale.x;
			}
			optimizer.DetectionBounds = Vector3.one * light.range * 1.8f;
			if (optimizer.transform != light.transform)
			{
				optimizer.DetectionOffset = optimizer.transform.InverseTransformPoint(light.transform.position);
			}
			return new FComponentLODsController(optimizer, light, "Light Properties", this);
		}
		return null;
	}

	internal void GetLODsController(object target, FOptimizer_Base fOptimizer_Base)
	{
		throw new NotImplementedException();
	}
}
