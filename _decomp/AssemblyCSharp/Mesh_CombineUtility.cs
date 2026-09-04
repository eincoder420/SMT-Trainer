using UnityEngine;

public class Mesh_CombineUtility
{
	public struct MeshInstance
	{
		public Mesh mesh;

		public int subMeshIndex;

		public Matrix4x4 transform;
	}

	public static Mesh Combine(MeshInstance[] combines, bool generateStrips)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		MeshInstance[] array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance meshInstance = array[i];
			if (!meshInstance.mesh)
			{
				continue;
			}
			num += meshInstance.mesh.vertexCount;
			if (!generateStrips)
			{
				continue;
			}
			int num4 = meshInstance.mesh.GetTriangles(meshInstance.subMeshIndex).Length;
			if (num4 != 0)
			{
				if (num3 != 0)
				{
					num3 = (((num3 & 1) != 1) ? (num3 + 2) : (num3 + 3));
				}
				num3 += num4;
			}
			else
			{
				generateStrips = false;
			}
		}
		if (!generateStrips)
		{
			array = combines;
			for (int i = 0; i < array.Length; i++)
			{
				MeshInstance meshInstance2 = array[i];
				if ((bool)meshInstance2.mesh)
				{
					num2 += meshInstance2.mesh.GetTriangles(meshInstance2.subMeshIndex).Length;
				}
			}
		}
		Vector3[] array2 = new Vector3[num];
		Vector3[] array3 = new Vector3[num];
		Vector4[] array4 = new Vector4[num];
		Vector2[] array5 = new Vector2[num];
		Vector2[] array6 = new Vector2[num];
		Color[] array7 = new Color[num];
		int[] array8 = new int[num2];
		int[] array9 = new int[num3];
		int offset = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance meshInstance3 = array[i];
			if ((bool)meshInstance3.mesh)
			{
				Copy(meshInstance3.mesh.vertexCount, meshInstance3.mesh.vertices, array2, ref offset, meshInstance3.transform);
			}
		}
		offset = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance meshInstance4 = array[i];
			if ((bool)meshInstance4.mesh)
			{
				Matrix4x4 transform = meshInstance4.transform;
				transform = transform.inverse.transpose;
				CopyNormal(meshInstance4.mesh.vertexCount, meshInstance4.mesh.normals, array3, ref offset, transform);
			}
		}
		offset = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance meshInstance5 = array[i];
			if ((bool)meshInstance5.mesh)
			{
				Matrix4x4 transform2 = meshInstance5.transform;
				transform2 = transform2.inverse.transpose;
				CopyTangents(meshInstance5.mesh.vertexCount, meshInstance5.mesh.tangents, array4, ref offset, transform2);
			}
		}
		offset = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance meshInstance6 = array[i];
			if ((bool)meshInstance6.mesh)
			{
				Copy(meshInstance6.mesh.vertexCount, meshInstance6.mesh.uv, array5, ref offset);
			}
		}
		offset = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance meshInstance7 = array[i];
			if ((bool)meshInstance7.mesh)
			{
				Copy(meshInstance7.mesh.vertexCount, meshInstance7.mesh.uv2, array6, ref offset);
			}
		}
		offset = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance meshInstance8 = array[i];
			if ((bool)meshInstance8.mesh)
			{
				CopyColors(meshInstance8.mesh.vertexCount, meshInstance8.mesh.colors, array7, ref offset);
			}
		}
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		array = combines;
		for (int i = 0; i < array.Length; i++)
		{
			MeshInstance meshInstance9 = array[i];
			if (!meshInstance9.mesh)
			{
				continue;
			}
			if (generateStrips)
			{
				int[] triangles = meshInstance9.mesh.GetTriangles(meshInstance9.subMeshIndex);
				if (num6 != 0)
				{
					if ((num6 & 1) == 1)
					{
						array9[num6] = array9[num6 - 1];
						array9[num6 + 1] = triangles[0] + num7;
						array9[num6 + 2] = triangles[0] + num7;
						num6 += 3;
					}
					else
					{
						array9[num6] = array9[num6 - 1];
						array9[num6 + 1] = triangles[0] + num7;
						num6 += 2;
					}
				}
				for (int j = 0; j < triangles.Length; j++)
				{
					array9[j + num6] = triangles[j] + num7;
				}
				num6 += triangles.Length;
			}
			else
			{
				int[] triangles2 = meshInstance9.mesh.GetTriangles(meshInstance9.subMeshIndex);
				for (int k = 0; k < triangles2.Length; k++)
				{
					array8[k + num5] = triangles2[k] + num7;
				}
				num5 += triangles2.Length;
			}
			num7 += meshInstance9.mesh.vertexCount;
		}
		if (array2.Length > 65000)
		{
			return null;
		}
		Mesh mesh = new Mesh();
		mesh.name = "Combined Mesh";
		mesh.vertices = array2;
		mesh.normals = array3;
		mesh.colors = array7;
		mesh.uv = array5;
		mesh.uv2 = array6;
		mesh.tangents = array4;
		if (generateStrips)
		{
			mesh.SetTriangles(array9, 0);
		}
		else
		{
			mesh.triangles = array8;
		}
		return mesh;
	}

	private static void Copy(int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0; i < src.Length; i++)
		{
			dst[i + offset] = transform.MultiplyPoint(src[i]);
		}
		offset += vertexcount;
	}

	private static void CopyNormal(int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0; i < src.Length; i++)
		{
			dst[i + offset] = transform.MultiplyVector(src[i]).normalized;
		}
		offset += vertexcount;
	}

	private static void Copy(int vertexcount, Vector2[] src, Vector2[] dst, ref int offset)
	{
		for (int i = 0; i < src.Length; i++)
		{
			dst[i + offset] = src[i];
		}
		offset += vertexcount;
	}

	private static void CopyColors(int vertexcount, Color[] src, Color[] dst, ref int offset)
	{
		for (int i = 0; i < src.Length; i++)
		{
			dst[i + offset] = src[i];
		}
		offset += vertexcount;
	}

	private static void CopyTangents(int vertexcount, Vector4[] src, Vector4[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i = 0; i < src.Length; i++)
		{
			Vector4 vector = src[i];
			Vector3 vector2 = new Vector3(vector.x, vector.y, vector.z);
			vector2 = transform.MultiplyVector(vector2).normalized;
			dst[i + offset] = new Vector4(vector2.x, vector2.y, vector2.z, vector.w);
		}
		offset += vertexcount;
	}
}
