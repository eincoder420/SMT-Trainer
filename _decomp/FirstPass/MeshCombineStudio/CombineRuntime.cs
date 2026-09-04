using UnityEngine;

namespace MeshCombineStudio;

public class CombineRuntime : MonoBehaviour
{
	public MeshCombiner meshCombiner;

	public bool useSearchConditions = true;

	public GameObject[] gos;

	private void Start()
	{
		Combine();
	}

	private void Combine()
	{
		meshCombiner.searchOptions.parentGOs = gos;
		meshCombiner.CombineAll(useSearchConditions);
	}
}
