using System;
using UnityEngine;

namespace FIMSpace.FOptimizing;

[Serializable]
public class FComponentLODsController
{
	public FOptimizer_LODSettings LODSet;

	[SerializeField]
	private FOptimizer_LODSettings sharedLODSet;

	[SerializeField]
	private FOptimizer_LODSettings uniqueLODSet;

	[SerializeField]
	private FOptimizer_Base optimizer;

	public Component Component;

	[HideInInspector]
	public FLOD_Base RootReference;

	[SerializeField]
	[HideInInspector]
	protected bool lockFirstLOD = true;

	[HideInInspector]
	public bool UsingShared;

	internal int Version;

	public int nullTry;

	public FLOD_Base InitialSettings { get; protected set; }

	public FLOD_Base ReferenceLOD
	{
		get
		{
			if (LODSet.LevelOfDetailSets.Count > 0)
			{
				return LODSet.LevelOfDetailSets[0];
			}
			return null;
		}
	}

	public int LODLevelsCount => optimizer.LODLevels;

	public int CurrentLODLevel { get; private set; }

	public FComponentLODsController(FOptimizer_Base sourceOptimizer, Component toOptimize, string header = "", FLOD_Base rootReference = null)
	{
		optimizer = sourceOptimizer;
		Component = toOptimize;
		RootReference = rootReference;
	}

	public void OnStart()
	{
		if ((bool)RootReference)
		{
			if (InitialSettings == null)
			{
				InitialSettings = RootReference.GetLODInstance();
			}
			InitialSettings.SetSameValuesAsComponent(Component);
		}
	}

	internal void SetCurrentLODLevel(int currentLODLevel)
	{
		CurrentLODLevel = currentLODLevel;
		if (currentLODLevel >= LODSet.LevelOfDetailSets.Count)
		{
			CurrentLODLevel = LODSet.LevelOfDetailSets.Count - 1;
		}
	}

	internal void ApplyLODLevelSettings(FLOD_Base currentLOD)
	{
		if (currentLOD == null)
		{
			if (RootReference == null)
			{
				Debug.LogError(string.Concat("[OPTIMIZERS] CRITICAL ERROR: There is no root reference in Optimizer's LOD Controller! (", optimizer, ") Try adding Optimizers Manager again to the scene or import newest version from the Asset Store!"));
			}
			Debug.LogError("[OPTIMIZERS] Target LOD is NULL! (" + optimizer.name + " - " + RootReference.name + ")");
		}
		else
		{
			CurrentLODLevel = GetLODIndex(currentLOD);
			if (IsTransitioningOrOther())
			{
				CurrentLODLevel = -1;
			}
			currentLOD.ApplySettingsToComponent(Component, InitialSettings);
		}
	}

	internal FLOD_Base GetCurrentLOD()
	{
		return LODSet.LevelOfDetailSets[CurrentLODLevel];
	}

	internal FLOD_Base GetCullingLOD()
	{
		return LODSet.LevelOfDetailSets[LODSet.LevelOfDetailSets.Count - 2];
	}

	internal FLOD_Base GetHiddenLOD()
	{
		return LODSet.LevelOfDetailSets[LODSet.LevelOfDetailSets.Count - 1];
	}

	public int GetLODIndex(FLOD_Base lod)
	{
		for (int i = 0; i < LODSet.LevelOfDetailSets.Count; i++)
		{
			if (LODSet.LevelOfDetailSets[i] == lod)
			{
				return i;
			}
		}
		return -1;
	}

	public bool IsTransitioningOrOther()
	{
		if (CurrentLODLevel >= 0 && CurrentLODLevel <= LODSet.LevelOfDetailSets.Count)
		{
			return false;
		}
		return true;
	}

	public FOptimizer_Base GetOptimizer()
	{
		return optimizer;
	}
}
