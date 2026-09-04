using UnityEngine;

namespace MeshCombineStudio;

public struct GameObjectLayer
{
	public GameObject go;

	public int layer;

	public GameObjectLayer(GameObject go)
	{
		this.go = go;
		layer = go.layer;
	}

	public void RestoreLayer()
	{
		go.layer = layer;
	}
}
