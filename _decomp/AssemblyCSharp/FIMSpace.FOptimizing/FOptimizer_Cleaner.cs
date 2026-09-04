using UnityEngine;

namespace FIMSpace.FOptimizing;

[AddComponentMenu("FImpossible Creations/Optimizers/Utilities/Optimizers Sub-Assets Cleaner")]
public class FOptimizer_Cleaner : MonoBehaviour
{
	public GameObject PrefabWithOptimizers;

	private void Reset()
	{
		PrefabWithOptimizers = base.gameObject;
	}
}
