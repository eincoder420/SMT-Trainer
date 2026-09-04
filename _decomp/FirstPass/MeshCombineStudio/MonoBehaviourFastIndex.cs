using UnityEngine;

namespace MeshCombineStudio;

public class MonoBehaviourFastIndex : MonoBehaviour, IFastIndex
{
	public IFastIndexList List { get; set; }

	public int ListIndex { get; set; }

	public MonoBehaviourFastIndex()
	{
		ListIndex = -1;
	}

	public void RemoveFromList()
	{
		if (List != null)
		{
			List.Remove(this);
		}
	}
}
