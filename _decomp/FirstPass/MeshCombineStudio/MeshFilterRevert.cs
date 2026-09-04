using UnityEngine;

namespace MeshCombineStudio;

public class MeshFilterRevert : MonoBehaviour
{
	public string guid = string.Empty;

	public string meshName;

	public bool DestroyAndReferenceMeshFilter(MeshFilter mf)
	{
		return true;
	}

	public void RevertMeshFilter(MeshFilter mf)
	{
	}
}
