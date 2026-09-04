using UnityEngine;
using UnityEngine.EventSystems;

namespace FIMSpace.FOptimizing;

[AddComponentMenu("FImpossible Creations/Optimizers/Basic Optimizer")]
public class FOptimizer : FOptimizer_Base, IDropHandler, IEventSystemHandler, IFHierarchyIcon
{
	public string EditorIconPath => "FIMSpace/FOptimizing/Optimizers Icon";

	public void OnDrop(PointerEventData data)
	{
	}
}
