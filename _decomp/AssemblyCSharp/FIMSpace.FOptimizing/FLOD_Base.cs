using UnityEngine;

namespace FIMSpace.FOptimizing;

public abstract class FLOD_Base : ScriptableObject
{
	[HideInInspector]
	public bool CustomEditor;

	[Tooltip("If target component should be disabled (not game object) at 'Culled' LOD state.\n\nSometimes you want some of optimized components to be deactivated at certain LOD level and some only at Culled level.")]
	public bool Disable;

	[HideInInspector]
	public bool DrawDisableOption = true;

	[HideInInspector]
	public bool SupportingTransitions;

	[HideInInspector]
	public bool DrawLowererSlider;

	[HideInInspector]
	[Range(0f, 1f)]
	public float QualityLowerer = 1f;

	[HideInInspector]
	public string HeaderText = "";

	[HideInInspector]
	public float ToCullDelay;

	internal int Version;

	public void CopyBase(FLOD_Base copyFrom)
	{
		Disable = copyFrom.Disable;
		CustomEditor = copyFrom.CustomEditor;
		QualityLowerer = copyFrom.QualityLowerer;
		DrawLowererSlider = copyFrom.DrawLowererSlider;
		DrawDisableOption = copyFrom.DrawDisableOption;
	}

	public virtual void InterpolateBetween(FLOD_Base lodA, FLOD_Base lodB, float transitionToB)
	{
		Disable = BoolTransition(Disable, lodA.Disable, lodB.Disable, transitionToB);
	}

	public virtual FLOD_Base CreateNewCopy()
	{
		return (FLOD_Base)MemberwiseClone();
	}

	public virtual void SetAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source)
	{
	}

	public virtual void SetSettingsAsForNearest(Component component)
	{
	}

	public virtual void SetSameValuesAsComponent(Component target)
	{
	}

	public virtual void ApplySettingsToComponent(Component component, FLOD_Base initialSettingsReference)
	{
		Behaviour behaviour = component as Behaviour;
		if ((bool)behaviour)
		{
			if (Disable)
			{
				behaviour.enabled = false;
			}
			else
			{
				behaviour.enabled = true;
			}
		}
	}

	public virtual FLOD_Base GetLODInstance()
	{
		return null;
	}

	public virtual void SetSettingsAsForCulled(Component component)
	{
		Disable = true;
	}

	public virtual void SetSettingsAsForHidden(Component component)
	{
	}

	protected static bool BoolTransition(bool defaultV, bool a, bool b, float transition)
	{
		if (!b && a)
		{
			return false;
		}
		if (transition >= 1f)
		{
			return b;
		}
		if (transition <= 0f)
		{
			return a;
		}
		return defaultV;
	}

	protected static object ObjectTransition(object defaultV, object a, object b, float transition)
	{
		if (transition >= 1f)
		{
			return b;
		}
		if (transition <= 0f)
		{
			return a;
		}
		return defaultV;
	}

	protected float GetValueForLODLevel(float from, float to, float lodLevel, float lodLevels)
	{
		return Mathf.Lerp(from, to, (lodLevel + 1f) / lodLevels);
	}

	public virtual FComponentLODsController GenerateLODController(Component target, FOptimizer_Base optimizer)
	{
		return null;
	}

	public virtual void EditorWindow()
	{
	}

	public virtual void DrawTogglers(FComponentLODsController lodsController)
	{
	}

	public virtual void AssignToggler(FLOD_Base reference)
	{
	}
}
