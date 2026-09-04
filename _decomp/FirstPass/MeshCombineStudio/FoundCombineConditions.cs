using System;
using System.Collections.Generic;

namespace MeshCombineStudio;

[Serializable]
public class FoundCombineConditions
{
	public HashSet<CombineCondition> combineConditions = new HashSet<CombineCondition>();

	public int combineConditionsCount;

	public int matCount;

	public int lightmapIndexCount;

	public int shadowCastingCount;

	public int receiveShadowsCount;

	public int lightmapScale;

	public int receiveGICount;

	public int lightProbeUsageCount;

	public int reflectionProbeUsageCount;

	public int probeAnchorCount;

	public int motionVectorGenerationModeCount;

	public int layerCount;

	public int staticEditorFlagsCount;
}
