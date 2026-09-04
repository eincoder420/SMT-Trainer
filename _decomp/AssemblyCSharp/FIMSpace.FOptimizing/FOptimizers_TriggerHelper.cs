using UnityEngine;

namespace FIMSpace.FOptimizing;

[AddComponentMenu("FImpossible Creations/Hidden/Trigger Helper")]
public class FOptimizers_TriggerHelper : MonoBehaviour
{
	public FOptimizer_Base Optimizer;

	public int TriggerIndex = -1;

	public FOptimizers_TriggerHelper Initialize(FOptimizer_Base optimizer, int index)
	{
		Optimizer = optimizer;
		TriggerIndex = index;
		return this;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (Optimizer == null)
		{
			Object.Destroy(base.gameObject);
		}
		else if (!(other.transform != Optimizer.TargetCamera))
		{
			Optimizer.OnTriggerChange(this, exit: false);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (Optimizer == null)
		{
			Object.Destroy(base.gameObject);
		}
		else if (!(other.transform != Optimizer.TargetCamera))
		{
			Optimizer.OnTriggerChange(this, exit: true);
		}
	}
}
