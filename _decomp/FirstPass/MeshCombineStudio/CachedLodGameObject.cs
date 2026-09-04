using System;
using UnityEngine;

namespace MeshCombineStudio;

[Serializable]
public class CachedLodGameObject : CachedGameObject
{
	public Vector3 center;

	public int lodCount;

	public int lodLevel;

	public CachedLodGameObject(CachedGameObject cachedGO, int lodCount, int lodLevel)
		: base(cachedGO.searchParentT, cachedGO.go, cachedGO.t, cachedGO.mr, cachedGO.mf, cachedGO.mesh)
	{
		this.lodCount = lodCount;
		this.lodLevel = lodLevel;
	}
}
