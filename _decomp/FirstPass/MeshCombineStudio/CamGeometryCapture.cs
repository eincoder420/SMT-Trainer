using System;
using UnityEngine;

namespace MeshCombineStudio;

public class CamGeometryCapture : MonoBehaviour
{
	public ComputeShader computeDepthToArray;

	public Int2 resolution = new Int2(1024, 1024);

	public Camera cam;

	public Transform t;

	public RenderTexture rtCapture;

	private float[] heights;

	private Bounds bounds;

	private float maxSize;

	public void Init()
	{
		if (!(t != null))
		{
			t = base.transform;
			cam = GetComponent<Camera>();
			cam.aspect = 1f;
			cam.orthographic = true;
		}
	}

	private void OnDestroy()
	{
		DisposeRTCapture();
	}

	private void DisposeRenderTexture(ref RenderTexture rt)
	{
		if (!(rt == null))
		{
			rt.Release();
			UnityEngine.Object.Destroy(rt);
			rt = null;
		}
	}

	public void DisposeRTCapture()
	{
		cam.targetTexture = null;
		DisposeRenderTexture(ref rtCapture);
	}

	public void RemoveTrianglesBelowSurface(Transform t, MeshCombineJobManager.MeshCombineJob meshCombineJob, MeshCache.SubMeshCache newMeshCache, ref byte[] vertexIsBelow)
	{
		if (vertexIsBelow == null)
		{
			vertexIsBelow = new byte[65534];
		}
		Vector3 zero = Vector3.zero;
		int collisionMask = meshCombineJob.meshCombiner.surfaceLayerMask;
		Vector3[] vertices = newMeshCache.vertices;
		int[] triangles = newMeshCache.triangles;
		FastList<MeshObject> meshObjects = meshCombineJob.meshObjectsHolder.meshObjects;
		int startIndex = meshCombineJob.startIndex;
		int endIndex = meshCombineJob.endIndex;
		for (int i = startIndex; i < endIndex; i++)
		{
			MeshObject meshObject = meshObjects.items[i];
			Capture(meshObject.cachedGO.mr.bounds, collisionMask, new Vector3(0f, -1f, 0f), new Int2(1024, 1024));
			int startNewTriangleIndex = meshObject.startNewTriangleIndex;
			int num = meshObject.newTriangleCount + startNewTriangleIndex;
			for (int j = startNewTriangleIndex; j < num; j += 3)
			{
				bool flag = false;
				for (int k = 0; k < 3; k++)
				{
					int num2 = triangles[j + k];
					if (num2 != -1)
					{
						byte b = vertexIsBelow[num2];
						if (b == 0)
						{
							zero = t.TransformPoint(vertices[num2]);
							float height = GetHeight(zero);
							b = (byte)((zero.y < height) ? 1 : 2);
							vertexIsBelow[num2] = b;
							b = ((zero.y < height) ? (vertexIsBelow[num2] = 1) : (vertexIsBelow[num2] = 2));
						}
						if (b != 1)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					meshCombineJob.trianglesRemoved += 3;
					triangles[j] = -1;
				}
			}
		}
		Array.Clear(vertexIsBelow, 0, vertices.Length);
	}

	public void Capture(Bounds bounds, int collisionMask, Vector3 direction, Int2 resolution)
	{
		if (rtCapture == null || rtCapture.width != resolution.x || rtCapture.height != resolution.y)
		{
			if (rtCapture != null)
			{
				DisposeRTCapture();
			}
			rtCapture = new RenderTexture(resolution.x, resolution.y, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
		}
		bounds.size *= 1.1f;
		this.bounds = bounds;
		cam.targetTexture = rtCapture;
		cam.cullingMask = collisionMask;
		SetCamera(direction);
		cam.Render();
		int num = resolution.x * resolution.y;
		ComputeBuffer computeBuffer = new ComputeBuffer(num, 4);
		computeDepthToArray.SetTexture(0, "rtDepth", rtCapture);
		computeDepthToArray.SetBuffer(0, "heightBuffer", computeBuffer);
		computeDepthToArray.SetInt("resolution", resolution.x);
		computeDepthToArray.SetFloat("captureHeight", t.position.y);
		computeDepthToArray.SetFloat("distance", bounds.size.y + 256f);
		computeDepthToArray.SetInt("direction", (direction.y == 1f) ? 1 : (-1));
		computeDepthToArray.Dispatch(0, Mathf.CeilToInt(resolution.x / 8), Mathf.CeilToInt(resolution.y / 8), 1);
		if (heights == null || heights.Length != num)
		{
			heights = new float[num];
		}
		computeBuffer.GetData(heights);
		computeBuffer.Dispose();
	}

	public void SetCamera(Vector3 direction)
	{
		if (direction == new Vector3(0f, 1f, 0f))
		{
			t.position = bounds.center - new Vector3(0f, bounds.extents.y + 256f, 0f);
		}
		else if (direction == new Vector3(0f, -1f, 0f))
		{
			t.position = bounds.center + new Vector3(0f, bounds.extents.y + 256f, 0f);
		}
		t.forward = direction;
		maxSize = bounds.size.x;
		if (bounds.size.z > maxSize)
		{
			maxSize = bounds.size.z;
		}
		cam.orthographicSize = maxSize / 2f;
		cam.nearClipPlane = 0f;
		cam.farClipPlane = bounds.size.y + 256f;
	}

	public float GetHeight(Vector3 pos)
	{
		pos -= bounds.min;
		pos.x += (maxSize - bounds.size.x) / 2f;
		pos.z += (maxSize - bounds.size.z) / 2f;
		float num = maxSize / (float)resolution.x;
		float num2 = maxSize / (float)resolution.y;
		float num3 = (int)(pos.x / num);
		float num4 = (int)(pos.z / num2);
		if (num3 > (float)(resolution.x - 2) || num3 < 0f || num4 > (float)(resolution.y - 2) || num4 < 0f)
		{
			Debug.Log("Out of bounds " + num3 + " " + num4);
			return 0f;
		}
		int num5 = (int)num3;
		int num6 = (int)num4;
		float num7 = num3 - (float)num5;
		float a = heights[num5 + num6 * resolution.y];
		float b = heights[num5 + 1 + num6 * resolution.y];
		float a2 = Mathf.Lerp(a, b, num7);
		float a3 = heights[num5 + (num6 + 1) * resolution.y];
		b = heights[num5 + 1 + (num6 + 1) * resolution.y];
		float b2 = Mathf.Lerp(a3, b, num7);
		num7 = num4 - (float)num6;
		return Mathf.Lerp(a2, b2, num7);
	}
}
