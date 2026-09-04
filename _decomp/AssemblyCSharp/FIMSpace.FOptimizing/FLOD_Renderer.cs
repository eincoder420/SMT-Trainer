using UnityEngine;
using UnityEngine.Rendering;

namespace FIMSpace.FOptimizing;

public sealed class FLOD_Renderer : FLOD_Base
{
	[Space(4f)]
	[Tooltip("If model should cast and receive shadows (receive will be always false if renderer have it marked as false by default)")]
	public bool UseShadows = true;

	internal ShadowCastingMode ShadowsCast = ShadowCastingMode.On;

	internal bool ShadowsReceive;

	public MotionVectorGenerationMode MotionVectors = MotionVectorGenerationMode.Object;

	[Tooltip("If it is skinned mesh renderer we can switch bones weights spread quality")]
	public SkinQuality SkinnedQuality;

	[SerializeField]
	[HideInInspector]
	private bool skinned;

	public FLOD_Renderer()
	{
		SupportingTransitions = false;
		HeaderText = "Renderer LOD Settings";
	}

	public override FLOD_Base GetLODInstance()
	{
		return ScriptableObject.CreateInstance<FLOD_Renderer>();
	}

	public override FLOD_Base CreateNewCopy()
	{
		FLOD_Renderer fLOD_Renderer = ScriptableObject.CreateInstance<FLOD_Renderer>();
		fLOD_Renderer.CopyBase(this);
		fLOD_Renderer.UseShadows = UseShadows;
		fLOD_Renderer.ShadowsCast = ShadowsCast;
		fLOD_Renderer.ShadowsReceive = ShadowsReceive;
		fLOD_Renderer.MotionVectors = MotionVectors;
		fLOD_Renderer.SkinnedQuality = SkinnedQuality;
		return fLOD_Renderer;
	}

	public override void SetSameValuesAsComponent(Component component)
	{
		if (component == null)
		{
			Debug.LogError("[OPTIMIZERS] Given component is null instead of Renderer!");
		}
		Renderer renderer = component as Renderer;
		if (renderer != null)
		{
			UseShadows = true;
			if (renderer.shadowCastingMode == ShadowCastingMode.Off)
			{
				UseShadows = false;
			}
			ShadowsCast = renderer.shadowCastingMode;
			ShadowsReceive = renderer.receiveShadows;
			MotionVectors = renderer.motionVectorGenerationMode;
			SkinnedMeshRenderer skinnedMeshRenderer = RefreshSkinned(component);
			if ((bool)skinnedMeshRenderer)
			{
				SkinnedQuality = skinnedMeshRenderer.quality;
			}
		}
	}

	private SkinnedMeshRenderer RefreshSkinned(Component comp)
	{
		if (skinned)
		{
			return null;
		}
		SkinnedMeshRenderer obj = comp as SkinnedMeshRenderer;
		if ((bool)obj)
		{
			skinned = true;
		}
		return obj;
	}

	public override void ApplySettingsToComponent(Component component, FLOD_Base initialSettingsReference)
	{
		FLOD_Renderer fLOD_Renderer = initialSettingsReference as FLOD_Renderer;
		if (component == null)
		{
			Debug.Log("[OPTIMIZERS] Target component is null");
			return;
		}
		if (fLOD_Renderer == null)
		{
			Debug.Log("[OPTIMIZERS] Target LOD is not Renderer LOD or is null");
			return;
		}
		Renderer renderer = component as Renderer;
		if (UseShadows)
		{
			renderer.shadowCastingMode = fLOD_Renderer.ShadowsCast;
			renderer.receiveShadows = fLOD_Renderer.ShadowsReceive;
		}
		else
		{
			renderer.shadowCastingMode = ShadowCastingMode.Off;
			renderer.receiveShadows = false;
		}
		renderer.motionVectorGenerationMode = MotionVectors;
		if (QualitySettings.skinWeights != SkinWeights.OneBone && skinned)
		{
			if (QualitySettings.skinWeights == SkinWeights.TwoBones && SkinnedQuality == SkinQuality.Bone4)
			{
				SkinnedQuality = SkinQuality.Bone2;
			}
			(renderer as SkinnedMeshRenderer).quality = SkinnedQuality;
		}
		if (Disable)
		{
			renderer.enabled = false;
		}
		else
		{
			renderer.enabled = true;
		}
	}

	public override void SetAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source)
	{
		Renderer renderer = source as Renderer;
		if (renderer == null)
		{
			Debug.LogError("[OPTIMIZERS] Given component for reference values is null or is not Renderer Component!");
		}
		float valueForLODLevel = GetValueForLODLevel(1f, 0f, lodIndex, lodCount);
		UseShadows = renderer.shadowCastingMode != ShadowCastingMode.Off;
		if (lodIndex >= 0 && renderer.motionVectorGenerationMode != MotionVectorGenerationMode.ForceNoMotion)
		{
			MotionVectors = MotionVectorGenerationMode.Camera;
		}
		if (lodCount == 2 && renderer.motionVectorGenerationMode == MotionVectorGenerationMode.Object)
		{
			MotionVectors = MotionVectorGenerationMode.Camera;
		}
		if (valueForLODLevel > 0.43f)
		{
			SkinnedQuality = SkinQuality.Bone2;
		}
		if (lodIndex == lodCount - 2)
		{
			UseShadows = false;
			if (lodCount != 2)
			{
				MotionVectors = MotionVectorGenerationMode.ForceNoMotion;
			}
			SkinnedQuality = SkinQuality.Bone1;
		}
		base.name = "LOD" + (lodIndex + 2);
	}

	public override void SetSettingsAsForCulled(Component component)
	{
		base.SetSettingsAsForCulled(component);
		UseShadows = false;
		MotionVectors = MotionVectorGenerationMode.ForceNoMotion;
		SkinnedQuality = SkinQuality.Bone1;
	}

	public override void SetSettingsAsForHidden(Component component)
	{
		base.SetSettingsAsForHidden(component);
		Disable = true;
		UseShadows = false;
		MotionVectors = MotionVectorGenerationMode.ForceNoMotion;
		SkinnedQuality = SkinQuality.Bone1;
	}

	public override FComponentLODsController GenerateLODController(Component target, FOptimizer_Base optimizer)
	{
		Renderer renderer = target as Renderer;
		if (!renderer)
		{
			renderer = target.GetComponent<Renderer>();
		}
		if ((bool)renderer && !optimizer.ContainsComponent(renderer))
		{
			SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
			if ((bool)skinnedMeshRenderer)
			{
				if (optimizer.ToOptimize != null)
				{
					bool flag = false;
					for (int i = 0; i < optimizer.ToOptimize.Count; i++)
					{
						if (optimizer.ToOptimize[i].Component is Light)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						optimizer.DetectionRadius = skinnedMeshRenderer.bounds.extents.magnitude;
						optimizer.DetectionBounds = skinnedMeshRenderer.bounds.size * 1.2f;
						if (optimizer.DetectionOffset == Vector3.zero)
						{
							optimizer.DetectionOffset = skinnedMeshRenderer.transform.InverseTransformPoint(skinnedMeshRenderer.bounds.center);
						}
					}
				}
				return new FComponentLODsController(optimizer, skinnedMeshRenderer, "Skinned Renderer", this);
			}
			MeshRenderer meshRenderer = renderer as MeshRenderer;
			if ((bool)meshRenderer)
			{
				if (optimizer.ToOptimize != null)
				{
					bool flag2 = false;
					for (int j = 0; j < optimizer.ToOptimize.Count; j++)
					{
						if (optimizer.ToOptimize[j].Component is Light)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						float num = FOptimizer_Base.GetScaler(optimizer.transform);
						if (num == 0f)
						{
							num = 1f;
						}
						optimizer.DetectionRadius = meshRenderer.bounds.extents.magnitude / num;
						optimizer.DetectionBounds = meshRenderer.bounds.size * 1.05f / num;
						if (optimizer.DetectionOffset == Vector3.zero)
						{
							optimizer.DetectionOffset = meshRenderer.transform.InverseTransformPoint(meshRenderer.bounds.center);
						}
					}
				}
				return new FComponentLODsController(optimizer, meshRenderer, "MeshRenderer", this);
			}
		}
		return null;
	}

	public static void AutoBounds(FOptimizer_Base targetOptimizer, Mesh sourceMesh)
	{
	}
}
