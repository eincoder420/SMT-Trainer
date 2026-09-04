using System;
using UnityEngine;

namespace MeshCombineStudio;

[Serializable]
public class CachedGameObject
{
	public Transform searchParentT;

	public GameObject go;

	public Transform t;

	public MeshRenderer mr;

	public MeshFilterRevert mfr;

	public MeshFilter mf;

	public Mesh mesh;

	public Matrix4x4 mt;

	public Matrix4x4 mtNormals;

	public Transform rootT;

	public Vector3 rootTLossyScale;

	public int rootInstanceId = -1;

	public bool excludeCombine;

	public bool mrEnabled;

	public CachedGameObject(Transform searchParentT, GameObject go, Transform t, MeshRenderer mr, MeshFilter mf, Mesh mesh)
	{
		this.searchParentT = searchParentT;
		this.go = go;
		this.t = t;
		this.mr = mr;
		this.mf = mf;
		this.mesh = mesh;
		mt = t.localToWorldMatrix;
		mrEnabled = mr.enabled;
		mtNormals = mt.inverse.transpose;
	}

	public CachedGameObject(CachedComponents cachedComponent)
	{
		go = cachedComponent.go;
		t = cachedComponent.t;
		mr = cachedComponent.mr;
		mf = cachedComponent.mf;
		mesh = cachedComponent.mf.sharedMesh;
		mt = t.localToWorldMatrix;
		mtNormals = mt.inverse.transpose;
	}

	public void GetRoot()
	{
		rootT = Methods.GetChildRootTransform(t, searchParentT);
		rootInstanceId = rootT.GetInstanceID();
		rootTLossyScale = rootT.lossyScale;
	}
}
