using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace FIMSpace.FOptimizing;

public sealed class FLOD_MonoBehaviour : FLOD_Base
{
	[Serializable]
	public class ParameterHelper
	{
		public bool Change;

		public int ParamID;

		public int TypeID;

		public string ParamName;

		public string ParamType;

		public bool Supported = true;

		public int Int;

		public float Float;

		public Vector2 Vec2;

		public Vector3 Vec3;

		public Color Color;

		public bool Bool;

		public ParameterHelper(string name, string type)
		{
			ParamID = name.GetHashCode();
			ParamName = name;
			TypeID = type.GetHashCode();
			ParamType = type;
			Supported = true;
		}

		public void SetValue(int valueId, object value)
		{
			if (valueId == intId)
			{
				Int = (int)value;
			}
			else if (valueId == floatId)
			{
				Float = (float)value;
			}
			else if (valueId == boolId)
			{
				Bool = (bool)value;
			}
			else if (valueId == colorId)
			{
				Color = (Color)value;
			}
		}

		public object GetValue(int valueId)
		{
			if (valueId == intId)
			{
				return Int;
			}
			if (valueId == floatId)
			{
				return Float;
			}
			if (valueId == boolId)
			{
				return Bool;
			}
			if (valueId == colorId)
			{
				return Color;
			}
			return null;
		}
	}

	public bool BaseLOD;

	public UnityEvent Event;

	public List<ParameterHelper> Parameters;

	public List<ParameterHelper> NotSupported;

	internal bool DrawNotSupported;

	public static readonly int intId = "int".GetHashCode();

	public static readonly int floatId = "float".GetHashCode();

	public static readonly int boolId = "bool".GetHashCode();

	public static readonly int colorId = "Color".GetHashCode();

	public FLOD_MonoBehaviour()
	{
		SupportingTransitions = true;
		HeaderText = "MonoBehaviour LOD Settings";
		CustomEditor = true;
	}

	public override FLOD_Base GetLODInstance()
	{
		return ScriptableObject.CreateInstance<FLOD_MonoBehaviour>();
	}

	public override FLOD_Base CreateNewCopy()
	{
		FLOD_MonoBehaviour fLOD_MonoBehaviour = ScriptableObject.CreateInstance<FLOD_MonoBehaviour>();
		fLOD_MonoBehaviour.CopyBase(this);
		fLOD_MonoBehaviour.Parameters = new List<ParameterHelper>();
		if (Parameters != null)
		{
			for (int i = 0; i < Parameters.Count; i++)
			{
				ParameterHelper parameterHelper = new ParameterHelper(Parameters[i].ParamName, Parameters[i].ParamType);
				parameterHelper.SetValue(Parameters[i].TypeID, Parameters[i].GetValue(Parameters[i].TypeID));
				parameterHelper.Change = Parameters[i].Change;
				fLOD_MonoBehaviour.Parameters.Add(parameterHelper);
			}
		}
		return fLOD_MonoBehaviour;
	}

	public override void SetSameValuesAsComponent(Component component)
	{
		if (component == null)
		{
			Debug.LogError("[OPTIMIZERS] Given component is null instead of MonoBehaviour!");
		}
		MonoBehaviour monoBehaviour = component as MonoBehaviour;
		if (Version == 0)
		{
			_ = monoBehaviour != null;
		}
	}

	public override void InterpolateBetween(FLOD_Base lodA, FLOD_Base lodB, float transitionToB)
	{
		base.InterpolateBetween(lodA, lodB, transitionToB);
		if (Version == 1)
		{
			return;
		}
		FLOD_MonoBehaviour fLOD_MonoBehaviour = lodA as FLOD_MonoBehaviour;
		FLOD_MonoBehaviour fLOD_MonoBehaviour2 = lodB as FLOD_MonoBehaviour;
		BaseLOD = fLOD_MonoBehaviour2.BaseLOD;
		if (Parameters == null)
		{
			return;
		}
		for (int i = 0; i < Parameters.Count; i++)
		{
			if (fLOD_MonoBehaviour2.Parameters[i].Change)
			{
				Parameters[i].Change = true;
			}
			if (!fLOD_MonoBehaviour.BaseLOD && !fLOD_MonoBehaviour.Parameters[i].Change)
			{
				Parameters[i].SetValue(Parameters[i].TypeID, fLOD_MonoBehaviour2.Parameters[i].GetValue(Parameters[i].TypeID));
			}
			else if (Parameters[i].TypeID == intId)
			{
				Parameters[i].Int = (int)Mathf.Lerp(fLOD_MonoBehaviour.Parameters[i].Int, fLOD_MonoBehaviour2.Parameters[i].Int, transitionToB);
			}
			else if (Parameters[i].TypeID == floatId)
			{
				Parameters[i].Float = Mathf.Lerp(fLOD_MonoBehaviour.Parameters[i].Float, fLOD_MonoBehaviour2.Parameters[i].Float, transitionToB);
			}
			else if (Parameters[i].TypeID == boolId)
			{
				if (transitionToB > 0.5f)
				{
					Parameters[i].Bool = fLOD_MonoBehaviour2.Parameters[i].Bool;
				}
				else
				{
					Parameters[i].Bool = fLOD_MonoBehaviour.Parameters[i].Bool;
				}
			}
			else if (Parameters[i].TypeID == colorId)
			{
				Parameters[i].Color = Color.Lerp(fLOD_MonoBehaviour.Parameters[i].Color, fLOD_MonoBehaviour2.Parameters[i].Color, transitionToB);
			}
		}
	}

	public override void ApplySettingsToComponent(Component component, FLOD_Base initialSettingsReference)
	{
		if (Version == 0)
		{
			if (initialSettingsReference as FLOD_MonoBehaviour == null)
			{
				Debug.Log("[OPTIMIZERS] Target LOD is not MonoBehaviour LOD or is null");
				return;
			}
			if (Parameters != null)
			{
				for (int i = 0; i < Parameters.Count; i++)
				{
					if (!Parameters[i].Change && !BaseLOD)
					{
						continue;
					}
					FieldInfo field = component.GetType().GetField(Parameters[i].ParamName);
					if (field != null)
					{
						if (Parameters[i].TypeID == intId)
						{
							field.SetValue(component, Parameters[i].Int);
						}
						else if (Parameters[i].TypeID == floatId)
						{
							field.SetValue(component, Parameters[i].Float);
						}
						else if (Parameters[i].TypeID == boolId)
						{
							field.SetValue(component, Parameters[i].Bool);
						}
						else if (Parameters[i].TypeID == colorId)
						{
							field.SetValue(component, Parameters[i].Color);
						}
					}
					else
					{
						Debug.LogError(string.Concat("[OPTIMIZERS] Not found field with name ", Parameters[i].ParamName, " in ", component.GetType(), " of ", component, " ", component.name));
					}
				}
			}
		}
		if (Event != null)
		{
			Event.Invoke();
		}
		base.ApplySettingsToComponent(component, initialSettingsReference);
	}

	public override void SetAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component source)
	{
		if (source as MonoBehaviour == null)
		{
			Debug.LogError("[OPTIMIZERS] Given component for reference values is null or is not MonoBehaviour Component!");
		}
		SetSameValuesAsComponent(source);
		base.name = "LOD" + (lodIndex + 2);
	}

	public override void SetSettingsAsForCulled(Component component)
	{
		base.SetSettingsAsForCulled(component);
		SetSameValuesAsComponent(component);
	}

	public override void SetSettingsAsForHidden(Component component)
	{
		base.SetSettingsAsForHidden(component);
		Disable = true;
	}

	public override void SetSettingsAsForNearest(Component component)
	{
		base.SetSettingsAsForNearest(component);
		SetSameValuesAsComponent(component);
		if (Parameters != null)
		{
			for (int i = 0; i < Parameters.Count; i++)
			{
				Parameters[i].Change = true;
			}
		}
	}

	public override void EditorWindow()
	{
		if (Parameters == null)
		{
			Parameters = new List<ParameterHelper>();
		}
		if (Parameters.Count != 0)
		{
			Version = 0;
		}
	}

	public override FComponentLODsController GenerateLODController(Component target, FOptimizer_Base optimizer)
	{
		MonoBehaviour monoBehaviour = target as MonoBehaviour;
		if (!monoBehaviour)
		{
			monoBehaviour = target.GetComponentInChildren<MonoBehaviour>();
		}
		if ((bool)monoBehaviour && !optimizer.ContainsComponent(monoBehaviour))
		{
			return new FComponentLODsController(optimizer, monoBehaviour, "Custom Component", this);
		}
		return null;
	}
}
