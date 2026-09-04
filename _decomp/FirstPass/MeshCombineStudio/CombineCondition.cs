using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshCombineStudio;

[Serializable]
public struct CombineCondition
{
	public static HashSet<object> countSet = new HashSet<object>();

	public int matInstanceId;

	public int lightmapIndex;

	public ShadowCastingMode shadowCastingMode;

	public bool receiveShadows;

	public float lightmapScale;

	public LightProbeUsage lightProbeUsage;

	public ReflectionProbeUsage reflectionProbeUsage;

	public Transform probeAnchor;

	public MotionVectorGenerationMode motionVectorGenerationMode;

	public int layer;

	public int rootInstanceId;

	public static CombineCondition Default
	{
		get
		{
			CombineCondition result = default(CombineCondition);
			result.matInstanceId = -1;
			result.lightmapIndex = -1;
			result.shadowCastingMode = ShadowCastingMode.On;
			result.receiveShadows = true;
			result.lightmapScale = 1f;
			result.lightProbeUsage = LightProbeUsage.BlendProbes;
			result.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
			result.probeAnchor = null;
			result.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
			result.layer = 0;
			result.rootInstanceId = -1;
			return result;
		}
	}

	public static void MakeFoundReport(FoundCombineConditions fcc)
	{
		countSet.Clear();
		foreach (CombineCondition combineCondition in fcc.combineConditions)
		{
			countSet.Add(combineCondition.matInstanceId);
		}
		fcc.matCount = countSet.Count;
		countSet.Clear();
		foreach (CombineCondition combineCondition2 in fcc.combineConditions)
		{
			countSet.Add(combineCondition2.lightmapIndex);
		}
		fcc.lightmapIndexCount = countSet.Count;
		countSet.Clear();
		foreach (CombineCondition combineCondition3 in fcc.combineConditions)
		{
			countSet.Add(combineCondition3.shadowCastingMode);
		}
		fcc.shadowCastingCount = countSet.Count;
		countSet.Clear();
		foreach (CombineCondition combineCondition4 in fcc.combineConditions)
		{
			countSet.Add(combineCondition4.receiveShadows);
		}
		fcc.receiveShadowsCount = countSet.Count;
		countSet.Clear();
		foreach (CombineCondition combineCondition5 in fcc.combineConditions)
		{
			countSet.Add(combineCondition5.lightmapScale);
		}
		fcc.lightmapScale = countSet.Count;
		countSet.Clear();
		foreach (CombineCondition combineCondition6 in fcc.combineConditions)
		{
			countSet.Add(combineCondition6.lightProbeUsage);
		}
		fcc.lightProbeUsageCount = countSet.Count;
		countSet.Clear();
		foreach (CombineCondition combineCondition7 in fcc.combineConditions)
		{
			countSet.Add(combineCondition7.reflectionProbeUsage);
		}
		fcc.reflectionProbeUsageCount = countSet.Count;
		countSet.Clear();
		foreach (CombineCondition combineCondition8 in fcc.combineConditions)
		{
			countSet.Add(combineCondition8.probeAnchor);
		}
		fcc.probeAnchorCount = countSet.Count;
		countSet.Clear();
		foreach (CombineCondition combineCondition9 in fcc.combineConditions)
		{
			countSet.Add(combineCondition9.motionVectorGenerationMode);
		}
		fcc.motionVectorGenerationModeCount = countSet.Count;
		countSet.Clear();
		foreach (CombineCondition combineCondition10 in fcc.combineConditions)
		{
			countSet.Add(combineCondition10.layer);
		}
		fcc.layerCount = countSet.Count;
		fcc.combineConditionsCount = fcc.combineConditions.Count;
	}

	public void ReadFromGameObject(int rootInstanceId, CombineConditionSettings combineConditions, bool copyBakedLighting, GameObject go, Transform t, MeshRenderer mr, Material mat)
	{
		matInstanceId = (combineConditions.sameMaterial ? mat.GetInstanceID() : combineConditions.combineCondition.matInstanceId);
		lightmapIndex = (copyBakedLighting ? mr.lightmapIndex : (lightmapIndex = -1));
		shadowCastingMode = (combineConditions.sameShadowCastingMode ? mr.shadowCastingMode : combineConditions.combineCondition.shadowCastingMode);
		receiveShadows = (combineConditions.sameReceiveShadows ? mr.receiveShadows : combineConditions.combineCondition.receiveShadows);
		lightmapScale = (combineConditions.sameLightmapScale ? GetLightmapScale(mr) : combineConditions.combineCondition.lightmapScale);
		lightProbeUsage = (combineConditions.sameLightProbeUsage ? mr.lightProbeUsage : combineConditions.combineCondition.lightProbeUsage);
		reflectionProbeUsage = (combineConditions.sameReflectionProbeUsage ? mr.reflectionProbeUsage : combineConditions.combineCondition.reflectionProbeUsage);
		probeAnchor = (combineConditions.sameProbeAnchor ? mr.probeAnchor : combineConditions.combineCondition.probeAnchor);
		motionVectorGenerationMode = (combineConditions.sameMotionVectorGenerationMode ? mr.motionVectorGenerationMode : combineConditions.combineCondition.motionVectorGenerationMode);
		layer = (combineConditions.sameLayer ? go.layer : combineConditions.combineCondition.layer);
		this.rootInstanceId = rootInstanceId;
	}

	private float GetLightmapScale(MeshRenderer mr)
	{
		return 1f;
	}

	private void SetLightmapScale(MeshRenderer mr, float lightmapScale)
	{
	}

	public void WriteToGameObject(GameObject go, MeshRenderer mr)
	{
		mr.lightmapIndex = lightmapIndex;
		mr.shadowCastingMode = shadowCastingMode;
		mr.receiveShadows = receiveShadows;
		if (lightmapScale != 1f)
		{
			SetLightmapScale(mr, lightmapScale);
		}
		mr.lightProbeUsage = lightProbeUsage;
		mr.reflectionProbeUsage = reflectionProbeUsage;
		mr.motionVectorGenerationMode = motionVectorGenerationMode;
		go.layer = layer;
	}
}
