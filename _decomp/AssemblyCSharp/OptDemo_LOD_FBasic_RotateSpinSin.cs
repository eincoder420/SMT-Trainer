using FIMSpace.Basics;
using FIMSpace.FOptimizing;
using UnityEngine;

[CreateAssetMenu(menuName = "FImpossible Creations/DEMO/FBasic_RotateSpinSin LOD (just first reference then remove this menu)")]
public sealed class OptDemo_LOD_FBasic_RotateSpinSin : FLOD_Base
{
	[Space(4f)]
	[Range(0f, 1f)]
	public float RotationRange = 1f;

	public OptDemo_LOD_FBasic_RotateSpinSin()
	{
		SupportingTransitions = false;
		HeaderText = "DEMO SpinSin LOD Settings";
	}

	public override FLOD_Base GetLODInstance()
	{
		return ScriptableObject.CreateInstance<OptDemo_LOD_FBasic_RotateSpinSin>();
	}

	public override FLOD_Base CreateNewCopy()
	{
		OptDemo_LOD_FBasic_RotateSpinSin optDemo_LOD_FBasic_RotateSpinSin = ScriptableObject.CreateInstance<OptDemo_LOD_FBasic_RotateSpinSin>();
		optDemo_LOD_FBasic_RotateSpinSin.CopyBase(this);
		optDemo_LOD_FBasic_RotateSpinSin.RotationRange = 1f;
		return optDemo_LOD_FBasic_RotateSpinSin;
	}

	public override void SetSameValuesAsComponent(Component component)
	{
		if (component == null)
		{
			Debug.LogError("Given component is null instead of FBasic_RotateSpinSin!");
		}
		FBasic_RotateSpinSin fBasic_RotateSpinSin = component as FBasic_RotateSpinSin;
		if (fBasic_RotateSpinSin != null)
		{
			RotationRange = fBasic_RotateSpinSin.RotationRange;
		}
	}

	public override void InterpolateBetween(FLOD_Base lodA, FLOD_Base lodB, float transitionToB)
	{
		base.InterpolateBetween(lodA, lodB, transitionToB);
		OptDemo_LOD_FBasic_RotateSpinSin optDemo_LOD_FBasic_RotateSpinSin = lodA as OptDemo_LOD_FBasic_RotateSpinSin;
		OptDemo_LOD_FBasic_RotateSpinSin optDemo_LOD_FBasic_RotateSpinSin2 = lodB as OptDemo_LOD_FBasic_RotateSpinSin;
		RotationRange = Mathf.Lerp(optDemo_LOD_FBasic_RotateSpinSin.RotationRange, optDemo_LOD_FBasic_RotateSpinSin2.RotationRange, transitionToB);
	}

	public override void ApplySettingsToComponent(Component component, FLOD_Base initialSettingsReference)
	{
		OptDemo_LOD_FBasic_RotateSpinSin optDemo_LOD_FBasic_RotateSpinSin = initialSettingsReference as OptDemo_LOD_FBasic_RotateSpinSin;
		if (optDemo_LOD_FBasic_RotateSpinSin == null)
		{
			Debug.Log("Target LOD is not FBasic_RotateSpinSin LOD or is null");
			return;
		}
		(component as FBasic_RotateSpinSin).RotationRange = RotationRange * optDemo_LOD_FBasic_RotateSpinSin.RotationRange;
		base.ApplySettingsToComponent(component, initialSettingsReference);
	}

	public override void SetAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source)
	{
		if (source as FBasic_RotateSpinSin == null)
		{
			Debug.LogError("Given component for reference values is null or is not FBasic_RotateSpinSin Component!");
		}
		float valueForLODLevel = GetValueForLODLevel(1f, 0f, lodIndex, lodCount);
		if (lodIndex > 0)
		{
			RotationRange = valueForLODLevel;
		}
		base.name = "LOD" + (lodIndex + 2);
	}

	public override void SetSettingsAsForCulled(Component component)
	{
		base.SetSettingsAsForCulled(component);
		RotationRange = 0f;
	}

	public override void SetSettingsAsForNearest(Component component)
	{
		base.SetSettingsAsForNearest(component);
		RotationRange = 1f;
	}

	public override void SetSettingsAsForHidden(Component component)
	{
		base.SetSettingsAsForHidden(component);
		Disable = true;
	}

	public override FComponentLODsController GenerateLODController(Component target, FOptimizer_Base optimizer)
	{
		FBasic_RotateSpinSin fBasic_RotateSpinSin = target as FBasic_RotateSpinSin;
		if (!fBasic_RotateSpinSin)
		{
			fBasic_RotateSpinSin = target.GetComponentInChildren<FBasic_RotateSpinSin>();
		}
		if ((bool)fBasic_RotateSpinSin && !optimizer.ContainsComponent(fBasic_RotateSpinSin))
		{
			return new FComponentLODsController(optimizer, fBasic_RotateSpinSin, "DEMO SpinSin Properties", this);
		}
		return null;
	}
}
