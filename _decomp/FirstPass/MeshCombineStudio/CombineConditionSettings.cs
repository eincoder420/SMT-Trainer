using System;
using UnityEngine;

namespace MeshCombineStudio;

[Serializable]
public class CombineConditionSettings
{
	public bool foldout = true;

	public bool sameMaterial = true;

	public bool sameShadowCastingMode;

	public bool sameReceiveShadows;

	public bool sameReceiveGI;

	public bool sameLightmapScale;

	public bool sameLightProbeUsage;

	public bool sameReflectionProbeUsage;

	public bool sameProbeAnchor;

	public bool sameMotionVectorGenerationMode;

	public bool sameStaticEditorFlags;

	public bool sameLayer;

	public Material material;

	public CombineCondition combineCondition = CombineCondition.Default;
}
