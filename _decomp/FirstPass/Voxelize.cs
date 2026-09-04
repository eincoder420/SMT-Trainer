using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Voxelize : MonoBehaviour
{
	public class VoxelizedMesh
	{
		public byte[,,] volume;

		public Bounds bounds;

		public Int3 voxels;
	}

	public struct Int3
	{
		public int x;

		public int y;

		public int z;

		public Int3(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static Int3 operator +(Int3 a, Int3 b)
		{
			Int3 result = default(Int3);
			result.x = a.x + b.x;
			result.y = a.y + b.y;
			result.z = a.z + b.z;
			return result;
		}

		public override string ToString()
		{
			return "(" + x + ", " + y + ", " + z + ")";
		}
	}

	private static readonly byte[] bits = new byte[8] { 1, 2, 4, 8, 16, 32, 64, 128 };

	private static Dictionary<Mesh, VoxelizedMesh> voxelizedLookup = new Dictionary<Mesh, VoxelizedMesh>();

	private static List<float> intersectList = new List<float>();

	private const byte insideVoxel = 1;

	private const byte outsideVoxel = 2;

	public int voxelizeLayer;

	public float voxelResolution;

	public bool voxelize;

	public void Update()
	{
		if (voxelize)
		{
			voxelize = false;
			VoxelizeMesh(base.transform, voxelResolution, voxelizeLayer);
		}
	}

	private VoxelizedMesh VoxelizeMesh(Transform t, float voxelResolution, int voxelizeLayer)
	{
		Physics.queriesHitBackfaces = false;
		MeshRenderer component = t.GetComponent<MeshRenderer>();
		if (component == null)
		{
			return null;
		}
		MeshFilter component2 = t.GetComponent<MeshFilter>();
		if (component2 == null)
		{
			return null;
		}
		Mesh sharedMesh = component2.sharedMesh;
		if (sharedMesh == null)
		{
			return null;
		}
		VoxelizedMesh voxelizedMesh = new VoxelizedMesh();
		voxelizedLookup[sharedMesh] = voxelizedMesh;
		Transform parent = t.parent;
		Vector3 position = t.position;
		Quaternion rotation = t.rotation;
		Vector3 localScale = t.localScale;
		t.parent = null;
		t.position = Vector3.zero;
		t.rotation = Quaternion.identity;
		t.localScale = Vector3.one;
		int layer = t.gameObject.layer;
		t.gameObject.layer = voxelizeLayer;
		LayerMask voxelizeLayerMask = 1 << voxelizeLayer;
		Bounds bounds = component.bounds;
		Vector3 size = bounds.size;
		Int3 voxels = new Int3(Mathf.CeilToInt(size.x / voxelResolution), Mathf.CeilToInt(size.y / voxelResolution), Mathf.CeilToInt(size.z / voxelResolution));
		voxels += new Int3(2, 2, 2);
		int num = Mathf.CeilToInt((float)voxels.x / 8f);
		voxelizedMesh.voxels = voxels;
		size = (bounds.size = new Vector3((float)voxels.x * voxelResolution, (float)voxels.y * voxelResolution, (float)voxels.z * voxelResolution));
		voxelizedMesh.bounds = bounds;
		byte[,,] array = new byte[num, voxels.y, voxels.z];
		Ray ray = default(Ray);
		Ray ray2 = default(Ray);
		ray.direction = Vector3.forward;
		ray2.direction = Vector3.back;
		Vector3 min = bounds.min;
		Vector3 vector2 = min;
		vector2.z = bounds.max.z;
		Debug.Log(PrintVector3(component.bounds.size) + " new size " + PrintVector3(size) + " voxels " + voxels.ToString());
		int num2 = 0;
		Vector3 vector3 = Vector3.one * voxelResolution * 0.5f;
		Vector3 vector4 = min + vector3;
		try
		{
			for (int i = 0; i < voxels.x; i++)
			{
				int num3 = i / 8;
				byte b = bits[i - num3 * 8];
				for (int j = 0; j < voxels.y; j++)
				{
					Vector3 origin = (ray.origin = min + new Vector3(((float)i + 0.5f) * voxelResolution, ((float)j + 0.5f) * voxelResolution, 0f));
					origin.z = vector2.z;
					ray2.origin = origin;
					intersectList.Clear();
					MultiCast(ray, intersectList, 0.001f, size.z, voxelizeLayerMask);
					MultiCast(ray2, intersectList, -0.001f, size.z, voxelizeLayerMask);
					intersectList.Sort();
					float num4 = (float)intersectList.Count / 2f;
					if (num4 != (float)(int)num4)
					{
						continue;
					}
					for (int k = 0; k < intersectList.Count; k += 2)
					{
						int num5 = Mathf.RoundToInt((intersectList[k] - min.z) / voxelResolution);
						int num6 = Mathf.RoundToInt((intersectList[k + 1] - min.z) / voxelResolution);
						for (int l = num5; l < num6; l++)
						{
							Vector3 position2 = new Vector3((float)i * voxelResolution, (float)j * voxelResolution, (float)l * voxelResolution) + vector4;
							position2 = t.TransformPoint(position2);
							array[num3, j, l] |= b;
							num2++;
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
		}
		Debug.Log(t.name + " voxels " + num2);
		voxelizedMesh.volume = array;
		t.gameObject.layer = layer;
		t.parent = parent;
		t.position = position;
		t.rotation = rotation;
		t.localScale = localScale;
		return voxelizedMesh;
	}

	private static string PrintVector3(Vector3 v)
	{
		return "(" + v.x + ", " + v.y + ", " + v.z + ")";
	}

	private static void MultiCast(Ray ray, List<float> points, float hitOffset, float maxDistance, LayerMask voxelizeLayerMask)
	{
		RaycastHit hitInfo;
		while (Physics.Raycast(ray, out hitInfo, maxDistance, voxelizeLayerMask))
		{
			points.Add(hitInfo.point.z);
			Vector3 origin = ray.origin;
			ray.origin = new Vector3(origin.x, origin.y, hitInfo.point.z + hitOffset);
		}
	}

	private static void Report(VoxelizedMesh vm, float voxelResolution)
	{
		int num = (int)voxelResolution / 8;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; (float)j < voxelResolution; j++)
			{
				for (int k = 0; (float)k < voxelResolution; k++)
				{
					Debug.Log(vm.volume[i, j, k]);
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		DrawVolume(base.transform, voxelResolution);
		Gizmos.color = Color.white;
	}

	public void DrawVolume(Transform t, float voxelResolution)
	{
		MeshRenderer component = t.GetComponent<MeshRenderer>();
		if (component == null)
		{
			return;
		}
		MeshFilter component2 = t.GetComponent<MeshFilter>();
		if (component2 == null)
		{
			return;
		}
		Mesh sharedMesh = component2.sharedMesh;
		if (sharedMesh == null)
		{
			return;
		}
		voxelizedLookup.TryGetValue(sharedMesh, out var value);
		if (value == null)
		{
			return;
		}
		byte[,,] volume = value.volume;
		if (volume == null)
		{
			return;
		}
		Vector3 min = value.bounds.min;
		Vector3 size = t.lossyScale * voxelResolution;
		Vector3 vector = Vector3.one * voxelResolution * 0.5f;
		Int3 voxels = value.voxels;
		Gizmos.DrawWireCube(component.bounds.center, component.bounds.size);
		for (int i = 0; i < voxels.x; i++)
		{
			int num = i / 8;
			int num2 = i - num * 8;
			for (int j = 0; j < voxels.y; j++)
			{
				for (int k = 0; k < voxels.z; k++)
				{
					if ((volume[num, j, k] & bits[num2]) > 0)
					{
						Vector3 position = new Vector3(min.x + (float)i * voxelResolution, min.y + (float)j * voxelResolution, min.z + (float)k * voxelResolution) + vector;
						Gizmos.DrawWireCube(t.TransformPoint(position), size);
					}
				}
			}
		}
	}
}
