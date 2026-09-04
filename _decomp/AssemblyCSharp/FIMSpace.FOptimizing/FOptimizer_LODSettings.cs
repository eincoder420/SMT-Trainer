using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing;

public class FOptimizer_LODSettings : ScriptableObject
{
	public List<FLOD_Base> LevelOfDetailSets;

	public FOptimizer_LODSettings()
	{
		LevelOfDetailSets = new List<FLOD_Base>();
	}

	public FOptimizer_LODSettings CreateCopy()
	{
		FOptimizer_LODSettings fOptimizer_LODSettings = ScriptableObject.CreateInstance<FOptimizer_LODSettings>();
		for (int i = 0; i < LevelOfDetailSets.Count; i++)
		{
			fOptimizer_LODSettings.LevelOfDetailSets.Add(LevelOfDetailSets[i].CreateNewCopy());
		}
		return fOptimizer_LODSettings;
	}
}
