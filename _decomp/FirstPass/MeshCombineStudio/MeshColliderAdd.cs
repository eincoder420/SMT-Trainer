using UnityEngine;

namespace MeshCombineStudio;

public struct MeshColliderAdd
{
	public GameObject go;

	public Mesh mesh;

	public MeshColliderAdd(GameObject go, Mesh mesh)
	{
		this.go = go;
		this.mesh = mesh;
	}
}
