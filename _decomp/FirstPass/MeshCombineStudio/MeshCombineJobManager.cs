using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MeshCombineStudio;

[DefaultExecutionOrder(-94000000)]
[ExecuteInEditMode]
public class MeshCombineJobManager : MonoBehaviour
{
	[Serializable]
	public class JobSettings
	{
		public CombineJobMode combineJobMode;

		public ThreadAmountMode threadAmountMode;

		public int combineMeshesPerFrame = 4;

		public bool useMultiThreading = true;

		public bool useMainThread = true;

		public int customThreadAmount = 1;

		public bool showStats;

		public void CopySettings(JobSettings source)
		{
			combineJobMode = source.combineJobMode;
			threadAmountMode = source.threadAmountMode;
			combineMeshesPerFrame = source.combineMeshesPerFrame;
			useMultiThreading = source.useMultiThreading;
			useMainThread = source.useMainThread;
			customThreadAmount = source.customThreadAmount;
		}

		public void ReportStatus()
		{
			Debug.Log("---------------------");
			Debug.Log("combineJobMode " + combineJobMode);
			Debug.Log("threadAmountMode " + threadAmountMode);
			Debug.Log("combineMeshesPerFrame " + combineMeshesPerFrame);
			Debug.Log("useMultiThreading " + useMultiThreading);
			Debug.Log("useMainThread " + useMainThread);
			Debug.Log("customThreadAmount " + customThreadAmount);
		}
	}

	public enum CombineJobMode
	{
		CombineAtOnce,
		CombinePerFrame
	}

	public enum ThreadAmountMode
	{
		AllThreads,
		HalfThreads,
		Custom
	}

	public enum ThreadState
	{
		isFree,
		isReady,
		isRunning,
		hasError
	}

	public class MeshCombineJobsThread
	{
		public int threadId;

		public ThreadState threadState;

		public Queue<MeshCombineJob> meshCombineJobs = new Queue<MeshCombineJob>();

		public Queue<NewMeshObject> newMeshObjectsDone = new Queue<NewMeshObject>();

		public MeshCombineJobsThread(int threadId)
		{
			this.threadId = threadId;
		}

		public void ExecuteJobsThread(object state)
		{
			NewMeshObject newMeshObject = null;
			try
			{
				newMeshObject = null;
				MeshCombineJob meshCombineJob;
				lock (meshCombineJobs)
				{
					meshCombineJob = meshCombineJobs.Dequeue();
				}
				Interlocked.Increment(ref instance.totalNewMeshObjects);
				lock (instance.newMeshObjectsPool)
				{
					newMeshObject = ((instance.newMeshObjectsPool.Count != 0) ? instance.newMeshObjectsPool.Dequeue() : new NewMeshObject());
				}
				newMeshObject.newPosition = meshCombineJob.position;
				newMeshObject.Combine(meshCombineJob);
				lock (newMeshObjectsDone)
				{
					newMeshObjectsDone.Enqueue(newMeshObject);
				}
				threadState = ThreadState.isReady;
			}
			catch (Exception ex)
			{
				if (newMeshObject != null)
				{
					lock (instance.newMeshObjectsPool)
					{
						instance.newMeshObjectsPool.Add(newMeshObject);
					}
					Interlocked.Decrement(ref instance.totalNewMeshObjects);
				}
				lock (meshCombineJobs)
				{
					meshCombineJobs.Clear();
				}
				Debug.LogError("(MeshCombineStudio) => Mesh Combine Studio thread error -> " + ex.ToString());
				threadState = ThreadState.hasError;
			}
		}
	}

	public class MeshCombineJob
	{
		public MeshCombiner meshCombiner;

		public MeshObjectsHolder meshObjectsHolder;

		public Transform parent;

		public Vector3 position;

		public int startIndex;

		public int endIndex;

		public bool firstMesh;

		public bool intersectsSurface;

		public int backFaceTrianglesRemoved;

		public int trianglesRemoved;

		public bool abort;

		public string name;

		public MeshCombineJob(MeshCombiner meshCombiner, MeshObjectsHolder meshObjectsHolder, Transform parent, Vector3 position, int startIndex, int length, bool firstMesh, bool intersectsSurface)
		{
			this.meshCombiner = meshCombiner;
			this.meshObjectsHolder = meshObjectsHolder;
			this.parent = parent;
			this.position = position;
			this.startIndex = startIndex;
			this.firstMesh = firstMesh;
			this.intersectsSurface = intersectsSurface;
			endIndex = startIndex + length;
			meshObjectsHolder.lodParent.jobsPending++;
			name = GetHashCode().ToString();
		}
	}

	public class NewMeshObject
	{
		public static FastList<Vector3> weldVertices;

		public MeshCombineJob meshCombineJob;

		public MeshCache.SubMeshCache newMeshCache = new MeshCache.SubMeshCache();

		public bool allSkipped;

		public Vector3 newPosition;

		private byte[] vertexIsBelow;

		private const byte belowSurface = 1;

		private const byte aboveSurface = 2;

		public NewMeshObject()
		{
			newMeshCache.Init();
		}

		public void Combine(MeshCombineJob meshCombineJob)
		{
			this.meshCombineJob = meshCombineJob;
			if (meshCombineJob.abort)
			{
				return;
			}
			int startIndex = meshCombineJob.startIndex;
			int endIndex = meshCombineJob.endIndex;
			FastList<MeshObject> meshObjects = meshCombineJob.meshObjectsHolder.meshObjects;
			newMeshCache.ResetHasBooleans();
			int num = 0;
			int num2 = 0;
			int num3 = endIndex - startIndex;
			MeshCombiner meshCombiner = meshCombineJob.meshCombiner;
			CombineMode combineMode = meshCombiner.combineMode;
			bool validCopyBakedLighting = meshCombiner.validCopyBakedLighting;
			bool validRebakeLighting = meshCombiner.validRebakeLighting;
			bool flag = meshCombiner.rebakeLightingMode == MeshCombiner.RebakeLightingMode.RegenarateLightmapUvs;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			float num7 = 0f;
			if (validRebakeLighting)
			{
				num4 = Mathf.CeilToInt(Mathf.Sqrt(num3));
				num7 = 1f / (float)num4;
			}
			allSkipped = true;
			for (int i = startIndex; i < endIndex; i++)
			{
				MeshObject obj = meshObjects.items[i];
				int subMeshIndex = obj.subMeshIndex;
				MeshCache.SubMeshCache subMeshCache = obj.meshCache.subMeshCache[subMeshIndex];
				int vertexCount = subMeshCache.vertexCount;
				HasArray(ref newMeshCache.hasNormals, subMeshCache.hasNormals, ref newMeshCache.normals, subMeshCache.normals, vertexCount, num);
				HasArray(ref newMeshCache.hasTangents, subMeshCache.hasTangents, ref newMeshCache.tangents, subMeshCache.tangents, vertexCount, num, useDefaultValue: true, new Vector4(1f, 1f, 1f, 1f));
				HasArray(ref newMeshCache.hasUv, subMeshCache.hasUv, ref newMeshCache.uv, subMeshCache.uv, vertexCount, num);
				HasArray(ref newMeshCache.hasUv2, subMeshCache.hasUv2, ref newMeshCache.uv2, subMeshCache.uv2, vertexCount, num);
				HasArray(ref newMeshCache.hasUv3, subMeshCache.hasUv3, ref newMeshCache.uv3, subMeshCache.uv3, vertexCount, num);
				HasArray(ref newMeshCache.hasUv4, subMeshCache.hasUv4, ref newMeshCache.uv4, subMeshCache.uv4, vertexCount, num);
				HasArray(ref newMeshCache.hasColors, subMeshCache.hasColors, ref newMeshCache.colors32, subMeshCache.colors32, vertexCount, num, useDefaultValue: true, new Color32(1, 1, 1, 1));
				num += vertexCount;
			}
			num = 0;
			for (int j = startIndex; j < endIndex; j++)
			{
				MeshObject meshObject = meshObjects.items[j];
				if (meshObject.skip)
				{
					continue;
				}
				allSkipped = false;
				MeshCache meshCache = meshObject.meshCache;
				int subMeshIndex2 = meshObject.subMeshIndex;
				MeshCache.SubMeshCache subMeshCache2 = meshCache.subMeshCache[subMeshIndex2];
				Vector3 scale = meshObject.scale;
				bool flag2 = false;
				if (scale.x < 0f)
				{
					flag2 = !flag2;
				}
				if (scale.y < 0f)
				{
					flag2 = !flag2;
				}
				if (scale.z < 0f)
				{
					flag2 = !flag2;
				}
				int num8 = 1;
				if (flag2)
				{
					num8 = -1;
				}
				Vector3[] vertices = subMeshCache2.vertices;
				Vector3[] normals = subMeshCache2.normals;
				Vector4[] tangents = subMeshCache2.tangents;
				Vector2[] uv = subMeshCache2.uv;
				Vector2[] uv2 = subMeshCache2.uv2;
				Vector2[] uv3 = subMeshCache2.uv3;
				Vector2[] uv4 = subMeshCache2.uv4;
				Color32[] colors = subMeshCache2.colors32;
				int[] triangles = subMeshCache2.triangles;
				int vertexCount2 = subMeshCache2.vertexCount;
				int[] triangles2 = newMeshCache.triangles;
				Vector3[] vertices2 = newMeshCache.vertices;
				Vector3[] normals2 = newMeshCache.normals;
				Vector4[] tangents2 = newMeshCache.tangents;
				Vector2[] uv5 = newMeshCache.uv;
				Vector2[] uv6 = newMeshCache.uv2;
				Vector2[] uv7 = newMeshCache.uv3;
				Vector2[] uv8 = newMeshCache.uv4;
				Color32[] colors2 = newMeshCache.colors32;
				bool hasNormals = subMeshCache2.hasNormals;
				bool hasTangents = subMeshCache2.hasTangents;
				Vector3 position = meshCombineJob.position;
				Matrix4x4 mt = meshObject.cachedGO.mt;
				Matrix4x4 mtNormals = meshObject.cachedGO.mtNormals;
				if (combineMode == CombineMode.DynamicObjects)
				{
					Vector3 rootTLossyScale = meshObject.cachedGO.rootTLossyScale;
					rootTLossyScale.x = 1f / rootTLossyScale.x;
					rootTLossyScale.y = 1f / rootTLossyScale.y;
					rootTLossyScale.z = 1f / rootTLossyScale.z;
					for (int k = 0; k < vertices.Length; k++)
					{
						int num9 = k + num;
						vertices2[num9] = Vector3.Scale(mt.MultiplyPoint3x4(vertices[k]) - position, rootTLossyScale);
					}
				}
				else
				{
					for (int l = 0; l < vertices.Length; l++)
					{
						int num10 = l + num;
						vertices2[num10] = mt.MultiplyPoint3x4(vertices[l]) - position;
					}
				}
				if (hasNormals)
				{
					meshCombiner.originalTotalNormalChannels++;
					for (int m = 0; m < vertices.Length; m++)
					{
						int num11 = m + num;
						normals2[num11] = mtNormals.MultiplyVector(normals[m]);
					}
				}
				if (hasTangents)
				{
					meshCombiner.originalTotalTangentChannels++;
					for (int n = 0; n < vertices.Length; n++)
					{
						int num12 = n + num;
						tangents2[num12] = mt.MultiplyVector(tangents[n]);
						tangents2[num12].w = tangents[n].w * (float)num8;
					}
				}
				if (subMeshCache2.hasUv)
				{
					meshCombiner.originalTotalUvChannels++;
					Array.Copy(uv, 0, uv5, num, vertexCount2);
				}
				if (subMeshCache2.hasUv2)
				{
					meshCombiner.originalTotalUv2Channels++;
					if (validCopyBakedLighting)
					{
						Vector4 lightmapScaleOffset = meshObject.lightmapScaleOffset;
						Vector2 vector = new Vector2(lightmapScaleOffset.z, lightmapScaleOffset.w);
						Vector2 vector2 = new Vector2(lightmapScaleOffset.x, lightmapScaleOffset.y);
						for (int num13 = 0; num13 < vertices.Length; num13++)
						{
							int num14 = num13 + num;
							uv6[num14] = new Vector2(uv2[num13].x * vector2.x, uv2[num13].y * vector2.y) + vector;
						}
					}
					else if (validRebakeLighting)
					{
						if (!flag)
						{
							Vector2 vector3 = new Vector2(num7 * (float)num5, num7 * (float)num6);
							for (int num15 = 0; num15 < vertices.Length; num15++)
							{
								int num16 = num15 + num;
								uv6[num16] = uv2[num15] * num7 + vector3;
							}
						}
					}
					else
					{
						Array.Copy(uv2, 0, uv6, num, vertexCount2);
					}
				}
				if (subMeshCache2.hasUv3)
				{
					meshCombiner.originalTotalUv3Channels++;
					Array.Copy(uv3, 0, uv7, num, vertexCount2);
				}
				if (subMeshCache2.hasUv4)
				{
					meshCombiner.originalTotalUv4Channels++;
					Array.Copy(uv4, 0, uv8, num, vertexCount2);
				}
				if (subMeshCache2.hasColors)
				{
					meshCombiner.originalTotalColorChannels++;
					Array.Copy(colors, 0, colors2, num, vertexCount2);
				}
				else if (newMeshCache.hasColors)
				{
					int num17 = num + vertexCount2;
					for (int num18 = num; num18 < num17; num18++)
					{
						colors2[num18] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
					}
				}
				if (flag2)
				{
					for (int num19 = 0; num19 < triangles.Length; num19 += 3)
					{
						triangles2[num19 + num2] = triangles[num19 + 2] + num;
						triangles2[num19 + num2 + 1] = triangles[num19 + 1] + num;
						triangles2[num19 + num2 + 2] = triangles[num19] + num;
					}
				}
				else
				{
					for (int num20 = 0; num20 < triangles.Length; num20++)
					{
						triangles2[num20 + num2] = triangles[num20] + num;
					}
				}
				num += vertexCount2;
				num2 += triangles.Length;
				if (++num5 >= num4)
				{
					num5 = 0;
					num6++;
				}
			}
			newMeshCache.vertexCount = num;
			newMeshCache.triangleCount = num2;
			if (meshCombiner.removeBackFaceTriangles)
			{
				RemoveBackFaceTriangles();
			}
		}

		private void PrintMissingArrayWarning(MeshCombiner meshCombiner, GameObject go, Mesh mesh, string text)
		{
			Debug.Log("(MeshCombineStudio) => GameObject: " + go.name + " Mesh " + mesh.name + " has missing " + text + " while the other meshes have them. Click the 'Select Meshes in Project' button to change the import settings.");
			meshCombiner.selectImportSettingsMeshes.Add(mesh);
		}

		private void HasArray<T>(ref bool hasNewArray, bool hasArray, ref T[] newArray, Array array, int vertexCount, int totalVertices, bool useDefaultValue = false, T defaultValue = default(T))
		{
			if (hasArray)
			{
				if (!hasNewArray)
				{
					if (newArray == null)
					{
						newArray = new T[65534];
						if (useDefaultValue)
						{
							FillArray(newArray, 0, totalVertices, defaultValue);
						}
					}
					else if (useDefaultValue)
					{
						FillArray(newArray, 0, totalVertices, defaultValue);
					}
					else
					{
						Array.Clear(newArray, 0, totalVertices);
					}
				}
				hasNewArray = true;
			}
			else if (hasNewArray)
			{
				if (useDefaultValue)
				{
					FillArray(newArray, totalVertices, vertexCount, defaultValue);
				}
				else
				{
					Array.Clear(newArray, totalVertices, vertexCount);
				}
			}
		}

		private void FillArray<T>(T[] array, int offset, int length, T value)
		{
			length += offset;
			for (int i = offset; i < length; i++)
			{
				array[i] = value;
			}
		}

		public void RemoveTrianglesBelowSurface(Transform t, MeshCombineJob meshCombineJob)
		{
			if (vertexIsBelow == null)
			{
				vertexIsBelow = new byte[65534];
			}
			Ray ray = instance.ray;
			RaycastHit hitInfo = instance.hitInfo;
			Vector3 zero = Vector3.zero;
			int layerMask = meshCombineJob.meshCombiner.surfaceLayerMask;
			float maxSurfaceHeight = meshCombineJob.meshCombiner.maxSurfaceHeight;
			Vector3[] vertices = newMeshCache.vertices;
			int[] triangles = newMeshCache.triangles;
			FastList<MeshObject> meshObjects = meshCombineJob.meshObjectsHolder.meshObjects;
			int startIndex = meshCombineJob.startIndex;
			int endIndex = meshCombineJob.endIndex;
			for (int i = startIndex; i < endIndex; i++)
			{
				MeshObject meshObject = meshObjects.items[i];
				if (!meshObject.intersectsSurface)
				{
					continue;
				}
				int startNewTriangleIndex = meshObject.startNewTriangleIndex;
				int num = meshObject.newTriangleCount + startNewTriangleIndex;
				for (int j = startNewTriangleIndex; j < num; j += 3)
				{
					bool flag = false;
					for (int k = 0; k < 3; k++)
					{
						int num2 = triangles[j + k];
						if (num2 == -1)
						{
							continue;
						}
						byte b = vertexIsBelow[num2];
						if (b == 0)
						{
							zero = t.TransformPoint(vertices[num2]);
							ray.origin = new Vector3(zero.x, maxSurfaceHeight, zero.z);
							if (!Physics.Raycast(ray, out hitInfo, maxSurfaceHeight - zero.y, layerMask))
							{
								vertexIsBelow[num2] = 2;
								flag = true;
								break;
							}
							if (!(zero.y < hitInfo.point.y))
							{
								b = (vertexIsBelow[num2] = 2);
								break;
							}
							b = (vertexIsBelow[num2] = 1);
						}
						if (b != 1)
						{
							flag = true;
							break;
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

		public void RemoveBackFaceTriangles()
		{
			int[] triangles = newMeshCache.triangles;
			Vector3[] normals = newMeshCache.normals;
			int triangleCount = newMeshCache.triangleCount;
			MeshCombiner meshCombiner = meshCombineJob.meshCombiner;
			bool flag = meshCombiner.backFaceTriangleMode == MeshCombiner.BackFaceTriangleMode.Box;
			Bounds backFaceBounds = meshCombiner.backFaceBounds;
			Vector3 min = backFaceBounds.min;
			Vector3 max = backFaceBounds.max;
			Vector3[] vertices = newMeshCache.vertices;
			Vector3 lhs = ((meshCombiner.backFaceTriangleMode != MeshCombiner.BackFaceTriangleMode.EulerAngles) ? meshCombiner.backFaceDirection : (Quaternion.Euler(meshCombiner.backFaceRotation) * Vector3.forward));
			Vector3 vector = default(Vector3);
			for (int i = 0; i < triangleCount; i += 3)
			{
				Vector3 zero = Vector3.zero;
				Vector3 zero2 = Vector3.zero;
				for (int j = 0; j < 3; j++)
				{
					int num = triangles[i + j];
					zero2 += vertices[num];
					zero += normals[num];
				}
				zero2 /= 3f;
				zero /= 3f;
				if (flag)
				{
					vector.x = ((zero.x > 0f) ? max.x : min.x);
					vector.y = ((zero.y > 0f) ? max.y : min.y);
					vector.z = ((zero.z > 0f) ? max.z : min.z);
					lhs = newPosition + zero2 - vector;
				}
				if (Vector3.Dot(lhs, zero) >= 0f)
				{
					triangles[i] = -1;
					meshCombineJob.backFaceTrianglesRemoved += 3;
				}
			}
		}

		public void WeldVertices(MeshCombineJob meshCombineJob)
		{
			if (weldVertices == null)
			{
				weldVertices = new FastList<Vector3>(65534);
			}
			else
			{
				weldVertices.FastClear();
			}
			Vector3[] vertices = newMeshCache.vertices;
			int vertexCount = newMeshCache.vertexCount;
			int[] array = new int[vertexCount];
			Dictionary<Vector3, int> dictionary = new Dictionary<Vector3, int>();
			int value;
			if (meshCombineJob.meshCombiner.weldSnapVertices)
			{
				float num = meshCombineJob.meshCombiner.weldSnapSize;
				if (num < 1E-05f)
				{
					num = 1E-05f;
				}
				for (int i = 0; i < vertexCount; i++)
				{
					Vector3 vector = Mathw.SnapRound(vertices[i], num);
					if (dictionary.TryGetValue(vector, out value))
					{
						array[i] = value;
						continue;
					}
					dictionary[vector] = (array[i] = weldVertices.Count);
					weldVertices.Add(vector);
				}
			}
			else
			{
				for (int j = 0; j < vertexCount; j++)
				{
					Vector3 vector2 = vertices[j];
					if (dictionary.TryGetValue(vector2, out value))
					{
						array[j] = value;
						continue;
					}
					dictionary[vector2] = (array[j] = weldVertices.Count);
					weldVertices.Add(vector2);
				}
			}
			int[] triangles = newMeshCache.triangles;
			int triangleCount = newMeshCache.triangleCount;
			for (int k = 0; k < triangleCount; k++)
			{
				if (triangles[k] != -1)
				{
					triangles[k] = array[triangles[k]];
				}
			}
			Array.Copy(weldVertices.items, newMeshCache.vertices, weldVertices.Count);
			newMeshCache.vertexCount = weldVertices.Count;
		}

		private void ArrangeTriangles()
		{
			int num = newMeshCache.triangleCount;
			int[] triangles = newMeshCache.triangles;
			for (int i = 0; i < num; i += 3)
			{
				if (triangles[i] == -1)
				{
					triangles[i] = triangles[num - 3];
					triangles[i + 1] = triangles[num - 2];
					triangles[i + 2] = triangles[num - 1];
					i -= 3;
					num -= 3;
				}
			}
			newMeshCache.triangleCount = num;
		}

		public void CreateMesh()
		{
			MeshCombiner meshCombiner = meshCombineJob.meshCombiner;
			if (meshCombiner.instantiatePrefab == null)
			{
				Debug.LogError("(MeshCombineStudio) => Instantiate Prefab = null");
				return;
			}
			CombineMode combineMode = meshCombiner.combineMode;
			MeshObjectsHolder meshObjectsHolder = meshCombineJob.meshObjectsHolder;
			if (combineMode == CombineMode.DynamicObjects)
			{
				meshCombineJob.parent = meshCombineJob.meshObjectsHolder.meshObjects.items[0].cachedGO.rootT;
			}
			else if (meshCombineJob.parent == null)
			{
				meshCombineJob.parent = meshCombineJob.meshCombiner.transform;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(meshCombiner.instantiatePrefab, newPosition, Quaternion.identity, meshCombineJob.parent);
			meshCombiner.data.combinedGameObjects.Add(gameObject);
			CachedComponents component = gameObject.GetComponent<CachedComponents>();
			MeshRenderer mr = component.mr;
			MeshFilter mf = component.mf;
			string name = (gameObject.name = ((combineMode == CombineMode.DynamicObjects) ? "CombinedMesh" : meshObjectsHolder.mat.name));
			if (meshCombineJob.intersectsSurface)
			{
				if (meshCombiner.noColliders)
				{
					instance.camGeometryCapture.RemoveTrianglesBelowSurface(gameObject.transform, meshCombineJob, newMeshCache, ref vertexIsBelow);
				}
				else
				{
					RemoveTrianglesBelowSurface(gameObject.transform, meshCombineJob);
				}
			}
			if (meshCombiner.weldVertices)
			{
				WeldVertices(meshCombineJob);
			}
			if (meshCombineJob.trianglesRemoved > 0 || meshCombineJob.backFaceTrianglesRemoved > 0 || meshCombiner.weldVertices)
			{
				ArrangeTriangles();
				if (instance.tempMeshCache == null)
				{
					instance.tempMeshCache = new MeshCache.SubMeshCache();
					instance.tempMeshCache.Init(initTriangles: false);
				}
				instance.tempMeshCache.CopySubMeshCache(newMeshCache);
				newMeshCache.RebuildVertexBuffer(instance.tempMeshCache, resizeArrays: false);
			}
			int vertexCount = newMeshCache.vertexCount;
			int triangleCount = newMeshCache.triangleCount;
			if (vertexCount == 0)
			{
				Methods.Destroy(gameObject);
				return;
			}
			Mesh mesh = new Mesh();
			mesh.name = name;
			meshCombiner.newTotalVertices += vertexCount;
			meshCombiner.newTotalTriangles += triangleCount;
			MeshExtension.ApplyVertices(mesh, newMeshCache.vertices, vertexCount);
			MeshExtension.ApplyTriangles(mesh, newMeshCache.triangles, triangleCount);
			if (meshCombiner.weldVertices)
			{
				if (newMeshCache.hasNormals && meshCombiner.weldIncludeNormals)
				{
					mesh.RecalculateNormals();
				}
			}
			else
			{
				if (newMeshCache.hasNormals)
				{
					meshCombiner.newTotalNormalChannels++;
					MeshExtension.ApplyNormals(mesh, newMeshCache.normals, vertexCount);
				}
				if (newMeshCache.hasTangents)
				{
					meshCombiner.newTotalTangentChannels++;
					MeshExtension.ApplyTangents(mesh, newMeshCache.tangents, vertexCount);
				}
				if (newMeshCache.hasUv)
				{
					meshCombiner.newTotalUvChannels++;
					MeshExtension.ApplyUvs(mesh, newMeshCache.uv, 0, vertexCount);
				}
				if (newMeshCache.hasUv2)
				{
					meshCombiner.newTotalUv2Channels++;
					MeshExtension.ApplyUvs(mesh, newMeshCache.uv2, 1, vertexCount);
				}
				if (newMeshCache.hasUv3)
				{
					meshCombiner.newTotalUv3Channels++;
					MeshExtension.ApplyUvs(mesh, newMeshCache.uv3, 2, vertexCount);
				}
				if (newMeshCache.hasUv4)
				{
					meshCombiner.newTotalUv3Channels++;
					MeshExtension.ApplyUvs(mesh, newMeshCache.uv4, 3, vertexCount);
				}
				if (newMeshCache.hasColors)
				{
					meshCombiner.newTotalColorChannels++;
					MeshExtension.ApplyColors32(mesh, newMeshCache.colors32, vertexCount);
				}
			}
			if (meshCombiner.addMeshColliders)
			{
				bool flag = true;
				if (meshCombiner.addMeshCollidersInRange && !meshCombiner.addMeshCollidersBounds.Contains(gameObject.transform.position))
				{
					flag = false;
				}
				if (flag)
				{
					meshCombiner.addMeshCollidersList.Add(new MeshColliderAdd(gameObject, mesh));
				}
			}
			if (meshCombiner.makeMeshesUnreadable)
			{
				mesh.UploadMeshData(markNoLongerReadable: true);
			}
			meshCombiner.newDrawCalls++;
			mr.sharedMaterial = meshObjectsHolder.mat;
			mf.sharedMesh = mesh;
			component.garbageCollectMesh.mesh = mesh;
			meshObjectsHolder.combineCondition.WriteToGameObject(gameObject, mr);
			if (meshObjectsHolder.newCachedGOs == null)
			{
				meshObjectsHolder.newCachedGOs = new FastList<CachedGameObject>();
			}
			meshObjectsHolder.newCachedGOs.Add(new CachedGameObject(component));
			meshObjectsHolder.lodParent.lodLevels[meshObjectsHolder.lodLevel].newMeshRenderers.Add(mr);
			if (--meshObjectsHolder.lodParent.jobsPending == 0 && meshObjectsHolder.lodParent.lodLevels.Length > 1)
			{
				meshObjectsHolder.lodParent.AssignLODGroup(meshCombiner);
			}
		}
	}

	public static MeshCombineJobManager instance;

	public JobSettings jobSettings = new JobSettings();

	[NonSerialized]
	public FastList<NewMeshObject> newMeshObjectsPool = new FastList<NewMeshObject>();

	public Dictionary<Mesh, MeshCache> meshCacheDictionary = new Dictionary<Mesh, MeshCache>();

	[NonSerialized]
	public int totalNewMeshObjects;

	public Queue<MeshCombineJob> meshCombineJobs = new Queue<MeshCombineJob>();

	public MeshCombineJobsThread[] meshCombineJobsThreads;

	public CamGeometryCapture camGeometryCapture;

	public int cores;

	public int threadAmount;

	public int startThreadId;

	public int endThreadId;

	public bool abort;

	private MeshCache.SubMeshCache tempMeshCache;

	private Ray ray = new Ray(Vector3.zero, Vector3.down);

	private RaycastHit hitInfo;

	public static MeshCombineJobManager CreateInstance(MeshCombiner meshCombiner, GameObject instantiatePrefab)
	{
		if (instance != null)
		{
			instance.camGeometryCapture.computeDepthToArray = meshCombiner.computeDepthToArray;
			return instance;
		}
		GameObject gameObject = new GameObject("MCS Job Manager");
		instance = gameObject.AddComponent<MeshCombineJobManager>();
		instance.SetJobMode(meshCombiner.jobSettings);
		gameObject.AddComponent<Camera>().enabled = false;
		instance.camGeometryCapture = gameObject.AddComponent<CamGeometryCapture>();
		instance.camGeometryCapture.computeDepthToArray = meshCombiner.computeDepthToArray;
		instance.camGeometryCapture.Init();
		return instance;
	}

	public static void ResetMeshCache()
	{
		if ((bool)instance)
		{
			instance.meshCacheDictionary.Clear();
		}
	}

	private void Awake()
	{
		instance = this;
	}

	private void OnEnable()
	{
		instance = this;
		base.gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
		Init();
	}

	public void Init()
	{
		cores = Environment.ProcessorCount;
		if (meshCombineJobsThreads == null || meshCombineJobsThreads.Length != cores)
		{
			meshCombineJobsThreads = new MeshCombineJobsThread[cores];
			for (int i = 0; i < meshCombineJobsThreads.Length; i++)
			{
				meshCombineJobsThreads[i] = new MeshCombineJobsThread(i);
			}
		}
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
		AbortJobs();
		if (instance == this)
		{
			instance = null;
		}
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			MyUpdate();
		}
	}

	private void MyUpdate()
	{
		ExecuteJobs();
	}

	public void SetJobMode(JobSettings newJobSettings)
	{
		if (newJobSettings.combineMeshesPerFrame < 1)
		{
			Debug.LogError("(MeshCombineStudio) => CombineMeshesPerFrame is " + newJobSettings.combineMeshesPerFrame + " and should be 1 or higher.");
			return;
		}
		if (newJobSettings.combineMeshesPerFrame > 128)
		{
			Debug.LogError("(MeshCombineStudio) => CombineMeshesPerFrame is " + newJobSettings.combineMeshesPerFrame + " and should be 128 or lower.");
			return;
		}
		if (newJobSettings.customThreadAmount < 1)
		{
			Debug.LogError("(MeshCombineStudio) => customThreadAmount is " + newJobSettings.combineMeshesPerFrame + " and should be 1 or higher.");
			return;
		}
		if (newJobSettings.customThreadAmount > cores)
		{
			newJobSettings.customThreadAmount = cores;
		}
		jobSettings.CopySettings(newJobSettings);
		if (jobSettings.useMultiThreading)
		{
			startThreadId = ((!jobSettings.useMainThread) ? 1 : 0);
			if (jobSettings.threadAmountMode == ThreadAmountMode.Custom)
			{
				if (jobSettings.customThreadAmount > cores - startThreadId)
				{
					jobSettings.customThreadAmount = cores - startThreadId;
				}
				threadAmount = jobSettings.customThreadAmount;
			}
			else
			{
				if (jobSettings.threadAmountMode == ThreadAmountMode.AllThreads)
				{
					threadAmount = cores;
				}
				else
				{
					threadAmount = cores / 2;
				}
				threadAmount -= startThreadId;
			}
			endThreadId = startThreadId + threadAmount;
		}
		else
		{
			startThreadId = 0;
			endThreadId = 1;
			threadAmount = 1;
		}
		int num = ((jobSettings.combineJobMode != CombineJobMode.CombinePerFrame) ? threadAmount : jobSettings.combineMeshesPerFrame);
		while (newMeshObjectsPool.Count > num)
		{
			newMeshObjectsPool.RemoveLast();
		}
	}

	public void AddJob(MeshCombiner meshCombiner, MeshObjectsHolder meshObjectsHolder, Transform parent, Vector3 position)
	{
		FastList<MeshObject> meshObjects = meshObjectsHolder.meshObjects;
		if (meshObjects.Count == 0)
		{
			return;
		}
		if (meshObjects.Count < 2 && meshObjects.items[0].cachedGO.mr.sharedMaterials.Length == 1 && !meshCombiner.removeTrianglesBelowSurface && !meshCombiner.removeOverlappingTriangles && !meshCombiner.removeBackFaceTriangles)
		{
			if (meshCombiner.excludeSingleMeshes)
			{
				for (int i = 0; i < meshObjects.Count; i++)
				{
					meshObjects.items[i].cachedGO.excludeCombine = true;
				}
				meshCombiner.originalDrawCalls++;
				meshCombiner.newDrawCalls++;
			}
			else if (meshObjects.Count == 1)
			{
				MeshObject meshObject = meshObjects.items[0];
				GameObject obj = UnityEngine.Object.Instantiate(meshCombiner.instantiatePrefab, meshObject.position, meshObject.rotation, parent);
				obj.transform.localScale = meshObject.cachedGO.t.lossyScale;
				Mesh sharedMesh = meshObject.cachedGO.mf.sharedMesh;
				obj.name = "SingleMesh " + sharedMesh.name;
				CachedComponents component = obj.GetComponent<CachedComponents>();
				component.mf.sharedMesh = sharedMesh;
				MeshRenderer mr = meshObject.cachedGO.mr;
				MeshRenderer mr2 = component.mr;
				mr2.sharedMaterials = mr.sharedMaterials;
				mr2.lightmapScaleOffset = mr.lightmapScaleOffset;
				mr2.lightmapIndex = mr.lightmapIndex;
				LightmapSettings lightmapSettings = obj.AddComponent<LightmapSettings>();
				lightmapSettings.mr = mr2;
				lightmapSettings.lightmapIndex = mr.lightmapIndex;
				lightmapSettings.setLigthmapScaleOffset = true;
				lightmapSettings.lightmapScaleOffset = mr.lightmapScaleOffset;
			}
			return;
		}
		int num = 0;
		int num2 = 0;
		int startIndex = 0;
		int num3 = 0;
		bool firstMesh = true;
		bool intersectsSurface = false;
		Mesh mesh = null;
		MeshCache value = null;
		int num4 = (meshCombiner.useVertexOutputLimit ? meshCombiner.vertexOutputLimit : 64000);
		for (int j = 0; j < meshObjects.Count; j++)
		{
			MeshObject meshObject2 = meshObjects.items[j];
			meshObject2.skip = false;
			meshCombiner.originalDrawCalls++;
			Mesh mesh2 = meshObject2.cachedGO.mesh;
			if (mesh2 != mesh && !meshCacheDictionary.TryGetValue(mesh2, out value))
			{
				value = new MeshCache(mesh2);
				meshCacheDictionary.Add(mesh2, value);
			}
			mesh = mesh2;
			meshObject2.meshCache = value;
			int vertexCount = value.subMeshCache[meshObject2.subMeshIndex].vertexCount;
			int triangleCount = value.subMeshCache[meshObject2.subMeshIndex].triangleCount;
			meshCombiner.originalTotalVertices += vertexCount;
			meshCombiner.originalTotalTriangles += triangleCount;
			if (num + vertexCount > num4)
			{
				MeshCombineJob meshCombineJob = new MeshCombineJob(meshCombiner, meshObjectsHolder, parent, position, startIndex, num3, firstMesh, intersectsSurface);
				EnqueueJob(meshCombiner, meshCombineJob);
				firstMesh = (intersectsSurface = false);
				num = (num2 = (num3 = 0));
				startIndex = j;
			}
			if (meshCombiner.removeOverlappingTriangles)
			{
				meshObject2.startNewTriangleIndex = num2;
				meshObject2.newTriangleCount = triangleCount;
			}
			if (meshCombiner.removeTrianglesBelowSurface)
			{
				int num5 = 0;
				if (!meshCombiner.noColliders)
				{
					num5 = MeshIntersectsSurface(meshCombiner, meshObject2.cachedGO);
				}
				meshObject2.startNewTriangleIndex = num2;
				meshObject2.newTriangleCount = triangleCount;
				if (num5 == 0)
				{
					intersectsSurface = (meshObject2.intersectsSurface = true);
					meshObject2.skip = false;
				}
				else
				{
					meshObject2.intersectsSurface = false;
					if (num5 == -1)
					{
						meshObject2.skip = true;
						num3++;
						continue;
					}
					meshObject2.skip = false;
				}
			}
			num += vertexCount;
			num2 += triangleCount;
			num3++;
		}
		if (num > 0)
		{
			MeshCombineJob meshCombineJob2 = new MeshCombineJob(meshCombiner, meshObjectsHolder, parent, position, startIndex, num3, firstMesh, intersectsSurface);
			EnqueueJob(meshCombiner, meshCombineJob2);
		}
	}

	private void EnqueueJob(MeshCombiner meshCombiner, MeshCombineJob meshCombineJob)
	{
		meshCombiner.meshCombineJobs.Add(meshCombineJob);
		meshCombiner.totalMeshCombineJobs++;
		meshCombineJobs.Enqueue(meshCombineJob);
	}

	public int MeshIntersectsSurface(MeshCombiner meshCombiner, CachedGameObject cachedGO)
	{
		MeshRenderer mr = cachedGO.mr;
		LayerMask surfaceLayerMask = meshCombiner.surfaceLayerMask;
		float maxSurfaceHeight = meshCombiner.maxSurfaceHeight;
		if (Physics.CheckBox(mr.bounds.center, mr.bounds.extents, Quaternion.identity, surfaceLayerMask))
		{
			return 0;
		}
		Vector3 min = mr.bounds.min;
		float maxDistance = meshCombiner.maxSurfaceHeight - min.y;
		ray.origin = new Vector3(min.x, maxSurfaceHeight, min.z);
		if (Physics.Raycast(ray, out hitInfo, maxDistance, surfaceLayerMask) && min.y < hitInfo.point.y)
		{
			return -1;
		}
		return 1;
	}

	public void AbortJobs()
	{
		foreach (MeshCombineJob meshCombineJob in meshCombineJobs)
		{
			meshCombineJob.meshCombiner.ClearMeshCombineJobs();
		}
		meshCombineJobs.Clear();
		for (int i = 0; i < meshCombineJobsThreads.Length; i++)
		{
			MeshCombineJobsThread meshCombineJobsThread = meshCombineJobsThreads[i];
			lock (meshCombineJobsThread.meshCombineJobs)
			{
				foreach (MeshCombineJob meshCombineJob2 in meshCombineJobsThread.meshCombineJobs)
				{
					meshCombineJob2.meshCombiner.ClearMeshCombineJobs();
				}
				meshCombineJobsThread.meshCombineJobs.Clear();
			}
		}
		totalNewMeshObjects = 0;
		abort = true;
	}

	public void ExecuteJobs()
	{
		while (meshCombineJobs.Count > 0)
		{
			int num = 999999;
			int num2 = 0;
			for (int i = startThreadId; i < endThreadId; i++)
			{
				int count = meshCombineJobsThreads[i].meshCombineJobs.Count;
				if (count < num)
				{
					num2 = i;
					num = count;
					if (num == 0)
					{
						break;
					}
				}
			}
			lock (meshCombineJobsThreads[num2].meshCombineJobs)
			{
				MeshCombineJob meshCombineJob = meshCombineJobs.Dequeue();
				if (!meshCombineJob.abort)
				{
					meshCombineJobsThreads[num2].meshCombineJobs.Enqueue(meshCombineJob);
				}
			}
		}
		try
		{
			bool flag;
			do
			{
				flag = false;
				if (jobSettings.useMultiThreading)
				{
					for (int j = 1; j < endThreadId; j++)
					{
						MeshCombineJobsThread meshCombineJobsThread = meshCombineJobsThreads[j];
						if (meshCombineJobsThread.meshCombineJobs.Count <= 0)
						{
							continue;
						}
						flag = true;
						if (meshCombineJobsThread.threadState == ThreadState.isFree)
						{
							if (instance.jobSettings.combineJobMode == CombineJobMode.CombinePerFrame && instance.totalNewMeshObjects + 1 > instance.jobSettings.combineMeshesPerFrame)
							{
								break;
							}
							meshCombineJobsThread.threadState = ThreadState.isRunning;
							ThreadPool.QueueUserWorkItem(meshCombineJobsThread.ExecuteJobsThread);
						}
						if (meshCombineJobsThread.threadState == ThreadState.hasError)
						{
							AbortJobs();
							return;
						}
					}
					for (int k = 1; k < endThreadId; k++)
					{
						if (meshCombineJobsThreads[k].threadState == ThreadState.isReady)
						{
							CombineMeshesDone(meshCombineJobsThreads[k]);
						}
					}
				}
				if (jobSettings.useMultiThreading && !jobSettings.useMainThread)
				{
					continue;
				}
				MeshCombineJobsThread meshCombineJobsThread2 = meshCombineJobsThreads[0];
				if (meshCombineJobsThread2.meshCombineJobs.Count > 0)
				{
					flag = true;
					meshCombineJobsThread2.threadState = ThreadState.isRunning;
					meshCombineJobsThread2.ExecuteJobsThread(null);
					if (meshCombineJobsThread2.threadState == ThreadState.isReady)
					{
						CombineMeshesDone(meshCombineJobsThread2);
					}
				}
			}
			while (jobSettings.combineJobMode == CombineJobMode.CombineAtOnce && flag);
		}
		catch (Exception ex)
		{
			Debug.LogError("(MeshCombineStudio) => " + ex.ToString());
			AbortJobs();
		}
	}

	public void CombineMeshesDone(MeshCombineJobsThread meshCombineJobThread)
	{
		Queue<NewMeshObject> newMeshObjectsDone = meshCombineJobThread.newMeshObjectsDone;
		int num = 0;
		while (newMeshObjectsDone.Count > 0)
		{
			NewMeshObject newMeshObject = newMeshObjectsDone.Dequeue();
			MeshCombiner meshCombiner = newMeshObject.meshCombineJob.meshCombiner;
			if (!abort && !newMeshObject.meshCombineJob.abort)
			{
				meshCombiner.meshCombineJobs.Remove(newMeshObject.meshCombineJob);
				try
				{
					if (!newMeshObject.allSkipped)
					{
						newMeshObject.CreateMesh();
					}
					if (meshCombiner.meshCombineJobs.Count == 0)
					{
						if (meshCombiner.addMeshColliders)
						{
							meshCombiner.AddMeshColliders();
						}
						meshCombiner.ExecuteOnCombiningReady();
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("(MeshCombineStudio) => " + ex.ToString());
					instance.AbortJobs();
				}
			}
			lock (newMeshObjectsPool)
			{
				newMeshObjectsPool.Add(newMeshObject);
			}
			Interlocked.Decrement(ref totalNewMeshObjects);
			if (jobSettings.combineJobMode == CombineJobMode.CombinePerFrame && ++num > jobSettings.combineMeshesPerFrame && !abort)
			{
				break;
			}
		}
		meshCombineJobThread.threadState = ThreadState.isFree;
		abort = false;
	}
}
