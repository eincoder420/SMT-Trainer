using FIMSpace.FOptimizing;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class FLOD_Terrain : FLOD_Base
{
	[Range(1f, 200f)]
	public float PixelError = 5f;

	[Range(0f, 2000f)]
	public float BasemapDistance = 1250f;

	[Space(3f)]
	[Range(0f, 250f)]
	public float DetailDistance = 100f;

	[Range(0f, 1f)]
	public float DetailDensity = 1f;

	[Space(3f)]
	[Range(0f, 2000f)]
	public float TreeDistance = 2000f;

	[Range(1f, 5f)]
	public float TreeLODBias = 1f;

	[Range(5f, 2000f)]
	public float BillboardStart = 50f;

	[Space(3f)]
	public bool DrawFoliage = true;

	public ShadowCastingMode Mode;

	public bool CastShadows = true;

	public bool DrawHeightmap = true;

	[Tooltip("Dividing resolution of heightmap")]
	[Range(0f, 3f)]
	public int ResolutionDivider;

	[Space(3f)]
	[Tooltip("Optional - Replace drawing terrain with target gameObject with mesh renderer for final optimization when terrain is far away (terrain collider will still work)")]
	public GameObject MeshReplacement;

	public FLOD_Terrain()
	{
		SupportingTransitions = true;
		HeaderText = "Terrain LOD Settings";
	}

	public override FLOD_Base GetLODInstance()
	{
		return ScriptableObject.CreateInstance<FLOD_Terrain>();
	}

	public override FLOD_Base CreateNewCopy()
	{
		FLOD_Terrain fLOD_Terrain = ScriptableObject.CreateInstance<FLOD_Terrain>();
		fLOD_Terrain.CopyBase(this);
		fLOD_Terrain.PixelError = PixelError;
		fLOD_Terrain.BasemapDistance = BasemapDistance;
		fLOD_Terrain.DetailDistance = DetailDistance;
		fLOD_Terrain.DetailDensity = DetailDensity;
		fLOD_Terrain.TreeDistance = TreeDistance;
		fLOD_Terrain.BillboardStart = BillboardStart;
		fLOD_Terrain.DrawFoliage = DrawFoliage;
		fLOD_Terrain.Mode = Mode;
		fLOD_Terrain.CastShadows = CastShadows;
		fLOD_Terrain.TreeLODBias = TreeLODBias;
		fLOD_Terrain.DrawHeightmap = DrawHeightmap;
		fLOD_Terrain.MeshReplacement = MeshReplacement;
		fLOD_Terrain.ResolutionDivider = ResolutionDivider;
		return fLOD_Terrain;
	}

	public override void SetSameValuesAsComponent(Component component)
	{
		if (component == null)
		{
			Debug.LogError("[OPTIMIZERS] Given component is null instead of Terrain!");
		}
		Terrain terrain = component as Terrain;
		if (terrain != null)
		{
			PixelError = terrain.heightmapPixelError;
			BasemapDistance = terrain.basemapDistance;
			DetailDistance = terrain.detailObjectDistance;
			DetailDensity = terrain.detailObjectDensity;
			TreeDistance = terrain.treeDistance;
			BillboardStart = terrain.treeBillboardDistance;
			DrawFoliage = terrain.drawTreesAndFoliage;
			Mode = terrain.shadowCastingMode;
			TreeLODBias = terrain.treeLODBiasMultiplier;
			ResolutionDivider = terrain.heightmapMaximumLOD;
			DrawHeightmap = terrain.drawHeightmap;
			MeshReplacement = null;
		}
	}

	public override void InterpolateBetween(FLOD_Base lodA, FLOD_Base lodB, float transitionToB)
	{
		base.InterpolateBetween(lodA, lodB, transitionToB);
		FLOD_Terrain fLOD_Terrain = lodA as FLOD_Terrain;
		FLOD_Terrain fLOD_Terrain2 = lodB as FLOD_Terrain;
		PixelError = Mathf.Lerp(fLOD_Terrain.PixelError, fLOD_Terrain2.PixelError, transitionToB);
		BasemapDistance = Mathf.Lerp(fLOD_Terrain.BasemapDistance, fLOD_Terrain2.BasemapDistance, transitionToB);
		DetailDistance = Mathf.Lerp(fLOD_Terrain.DetailDistance, fLOD_Terrain2.DetailDistance, transitionToB);
		DetailDensity = Mathf.Lerp(fLOD_Terrain.DetailDensity, fLOD_Terrain2.DetailDensity, transitionToB);
		TreeDistance = Mathf.Lerp(fLOD_Terrain.TreeDistance, fLOD_Terrain2.TreeDistance, transitionToB);
		BillboardStart = Mathf.Lerp(fLOD_Terrain.BillboardStart, fLOD_Terrain2.BillboardStart, transitionToB);
		TreeLODBias = Mathf.Lerp(fLOD_Terrain.TreeLODBias, fLOD_Terrain2.TreeLODBias, transitionToB);
		ResolutionDivider = (int)Mathf.Lerp(fLOD_Terrain.ResolutionDivider, fLOD_Terrain2.ResolutionDivider, transitionToB);
		DrawFoliage = FLOD_Base.BoolTransition(DrawFoliage, fLOD_Terrain.DrawFoliage, fLOD_Terrain2.DrawFoliage, transitionToB);
		if (transitionToB > 0f)
		{
			Mode = fLOD_Terrain2.Mode;
		}
		DrawHeightmap = FLOD_Base.BoolTransition(DrawHeightmap, fLOD_Terrain.DrawHeightmap, fLOD_Terrain2.DrawHeightmap, transitionToB);
		MeshReplacement = (GameObject)FLOD_Base.ObjectTransition(MeshReplacement, fLOD_Terrain.MeshReplacement, fLOD_Terrain2.MeshReplacement, transitionToB);
	}

	public override void ApplySettingsToComponent(Component component, FLOD_Base initialSettingsReference)
	{
		Terrain terrain = component as Terrain;
		if (terrain == null)
		{
			Debug.LogError(string.Concat("[OPTIMIZERS] Target component is null or is not Terrain! (", component, ")"));
			return;
		}
		FLOD_Terrain fLOD_Terrain = initialSettingsReference as FLOD_Terrain;
		if (MeshReplacement == null)
		{
			if (Disable)
			{
				terrain.enabled = false;
			}
			else
			{
				if (!terrain.enabled)
				{
					terrain.enabled = true;
				}
				terrain.heightmapPixelError = PixelError;
				if (terrain.detailObjectDistance != BasemapDistance)
				{
					terrain.detailObjectDistance = BasemapDistance;
				}
				if (terrain.detailObjectDensity != DetailDistance)
				{
					terrain.detailObjectDensity = DetailDistance;
				}
				if (terrain.detailObjectDensity != DetailDensity)
				{
					terrain.detailObjectDensity = DetailDensity;
				}
				if (terrain.treeDistance != TreeDistance)
				{
					terrain.treeDistance = TreeDistance;
				}
				if (terrain.treeBillboardDistance != BillboardStart)
				{
					terrain.treeBillboardDistance = BillboardStart;
				}
				terrain.drawTreesAndFoliage = DrawFoliage;
				terrain.shadowCastingMode = Mode;
				terrain.treeLODBiasMultiplier = TreeLODBias;
				terrain.drawHeightmap = DrawHeightmap;
				if (!terrain.drawTreesAndFoliage || !terrain.drawHeightmap)
				{
					terrain.collectDetailPatches = false;
				}
				else
				{
					terrain.collectDetailPatches = true;
				}
				terrain.heightmapMaximumLOD = ResolutionDivider;
			}
			if ((bool)fLOD_Terrain.MeshReplacement)
			{
				fLOD_Terrain.MeshReplacement.SetActive(value: false);
			}
		}
		else
		{
			terrain.shadowCastingMode = ShadowCastingMode.Off;
			terrain.drawHeightmap = false;
			terrain.drawTreesAndFoliage = false;
			terrain.collectDetailPatches = false;
			Transform transform = terrain.transform.Find(terrain.name);
			if (!transform)
			{
				transform = Object.Instantiate(MeshReplacement).transform;
				transform.name = terrain.name;
				transform.position = terrain.transform.position;
				transform.SetParent(terrain.transform, worldPositionStays: true);
				fLOD_Terrain.MeshReplacement = transform.gameObject;
			}
			transform.gameObject.SetActive(value: true);
		}
	}

	public override void SetAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source)
	{
		Terrain terrain = source as Terrain;
		if (terrain == null)
		{
			Debug.LogError("[OPTIMIZERS] Given component for reference values is null or is not Terrain Component!");
		}
		float valueForLODLevel = GetValueForLODLevel(1f, 0f, lodIndex, lodCount);
		PixelError = (int)Mathf.Lerp(terrain.heightmapPixelError + 22f, terrain.heightmapPixelError, valueForLODLevel);
		BasemapDistance = Mathf.Lerp(terrain.basemapDistance / 5f, terrain.basemapDistance / 1f, valueForLODLevel);
		DetailDistance = Mathf.Lerp(terrain.detailObjectDistance / 4f, terrain.detailObjectDistance, valueForLODLevel);
		DetailDensity = Mathf.Lerp(terrain.detailObjectDensity / 5f, terrain.detailObjectDensity, valueForLODLevel);
		TreeDistance = terrain.treeDistance;
		BillboardStart = terrain.treeBillboardDistance;
		TreeLODBias = 1f;
		DrawHeightmap = true;
		DrawDisableOption = false;
		ResolutionDivider = 0;
		Mode = ShadowCastingMode.Off;
		DrawFoliage = false;
		if (lodIndex >= 1)
		{
			DrawFoliage = false;
			TreeLODBias = Mathf.Lerp(2f, 1f, valueForLODLevel);
			if (lodCount <= 3)
			{
				PixelError = terrain.heightmapPixelError + 16f;
			}
		}
		if (lodIndex >= 2)
		{
			ResolutionDivider = 1;
			PixelError = terrain.heightmapPixelError + 18f;
		}
		base.name = "LOD" + (lodIndex + 2);
	}

	public override void SetSettingsAsForCulled(Component component)
	{
		base.SetSettingsAsForCulled(component);
		Disable = false;
		PixelError = 200f;
		BasemapDistance = 500f;
		DetailDistance = 0f;
		DetailDensity = 0f;
		TreeDistance = 0f;
		BillboardStart = 5f;
		DrawFoliage = false;
		Mode = ShadowCastingMode.Off;
		TreeLODBias = 1f;
		ResolutionDivider = 0;
		DrawHeightmap = false;
		DrawDisableOption = false;
	}

	public override void SetSettingsAsForHidden(Component component)
	{
		base.SetSettingsAsForHidden(component);
		DrawFoliage = false;
		Mode = ShadowCastingMode.Off;
		TreeLODBias = 1f;
		ResolutionDivider = 0;
		DrawHeightmap = false;
		DrawDisableOption = false;
	}

	public override void SetSettingsAsForNearest(Component component)
	{
		base.SetSettingsAsForNearest(component);
		Terrain sameValuesAsComponent = component as Terrain;
		SetSameValuesAsComponent(sameValuesAsComponent);
		DrawDisableOption = false;
	}

	public override FComponentLODsController GenerateLODController(Component target, FOptimizer_Base optimizer)
	{
		Terrain terrain = target as Terrain;
		if (!terrain)
		{
			terrain = target.GetComponentInChildren<Terrain>();
		}
		if ((bool)terrain && !optimizer.ContainsComponent(terrain))
		{
			return new FComponentLODsController(optimizer, terrain, "Terrain", this);
		}
		return null;
	}
}
