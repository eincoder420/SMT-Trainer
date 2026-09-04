using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECE;

public class EasyColliderCreator
{
	private struct BestFitSphere
	{
		public Vector3 Center;

		public float Radius;

		public BestFitSphere(Vector3 center, float radius)
		{
			Center = center;
			Radius = radius;
		}
	}

	private BestFitSphere CalculateBestFitSphere(List<Vector3> localVertices)
	{
		int count = localVertices.Count;
		float num2;
		float num;
		float num3 = (num2 = (num = 0f));
		foreach (Vector3 localVertex in localVertices)
		{
			num2 += localVertex.x;
			num += localVertex.y;
			num3 += localVertex.z;
		}
		num2 *= 1f / (float)count;
		num *= 1f / (float)count;
		num3 *= 1f / (float)count;
		Vector3 zero = Vector3.zero;
		Matrix4x4 zero2 = Matrix4x4.zero;
		zero2.m33 = 1f;
		float num4;
		float num5 = (num4 = 0f);
		foreach (Vector3 localVertex2 in localVertices)
		{
			zero2[0, 0] += 2f * (localVertex2.x * (localVertex2.x - num2)) / (float)count;
			zero2[0, 1] += 2f * (localVertex2.x * (localVertex2.y - num)) / (float)count;
			zero2[0, 2] += 2f * (localVertex2.x * (localVertex2.z - num3)) / (float)count;
			zero2[1, 0] += 2f * (localVertex2.y * (localVertex2.x - num2)) / (float)count;
			zero2[1, 1] += 2f * (localVertex2.y * (localVertex2.y - num)) / (float)count;
			zero2[1, 2] += 2f * (localVertex2.y * (localVertex2.z - num3)) / (float)count;
			zero2[2, 0] += 2f * (localVertex2.z * (localVertex2.x - num2)) / (float)count;
			zero2[2, 1] += 2f * (localVertex2.z * (localVertex2.y - num)) / (float)count;
			zero2[2, 2] += 2f * (localVertex2.z * (localVertex2.z - num3)) / (float)count;
			num5 = localVertex2.x * localVertex2.x;
			num4 = localVertex2.y * localVertex2.y;
			float num6 = localVertex2.z * localVertex2.z;
			zero.x += (num5 + num4 + num6) * (localVertex2.x - num2) / (float)count;
			zero.y += (num5 + num4 + num6) * (localVertex2.y - num) / (float)count;
			zero.z += (num5 + num4 + num6) * (localVertex2.z - num3) / (float)count;
		}
		Vector3 center = (zero2.transpose * zero2).inverse * zero2.transpose * zero;
		float num7 = 0f;
		foreach (Vector3 localVertex3 in localVertices)
		{
			num7 += Mathf.Pow(localVertex3.x - center.x, 2f) + Mathf.Pow(localVertex3.y - center.y, 2f) + Mathf.Pow(localVertex3.z - center.z, 2f);
		}
		num7 = Mathf.Sqrt(num7 / (float)localVertices.Count);
		return new BestFitSphere(center, num7);
	}

	public BoxColliderData CalculateBox(List<Vector3> worldVertices, Transform attachTo, bool isRotated = false)
	{
		if (isRotated && worldVertices.Count < 3)
		{
			return new BoxColliderData();
		}
		if (worldVertices.Count < 2)
		{
			return new BoxColliderData();
		}
		Quaternion identity = Quaternion.identity;
		List<Vector3> list = new List<Vector3>();
		Matrix4x4 matrix;
		if (isRotated && worldVertices.Count >= 3)
		{
			Vector3 vector = worldVertices[1] - worldVertices[0];
			Vector3 upwards = Vector3.Cross(vector, worldVertices[2] - worldVertices[1]);
			identity = Quaternion.LookRotation(vector, upwards);
			matrix = Matrix4x4.TRS(attachTo.position, identity, Vector3.one);
			for (int i = 0; i < worldVertices.Count; i++)
			{
				list.Add(matrix.inverse.MultiplyPoint3x4(worldVertices[i]));
			}
		}
		else
		{
			list = ToLocalVerts(attachTo, worldVertices);
			matrix = attachTo.localToWorldMatrix;
		}
		BoxColliderData boxColliderData = CalculateBoxLocal(list);
		boxColliderData.ColliderType = (isRotated ? CREATE_COLLIDER_TYPE.ROTATED_BOX : CREATE_COLLIDER_TYPE.BOX);
		boxColliderData.Matrix = matrix;
		return boxColliderData;
	}

	public BoxColliderData CalculateBoxLocal(List<Vector3> vertices)
	{
		float num2;
		float num;
		float num3 = (num2 = (num = float.PositiveInfinity));
		float num5;
		float num4;
		float num6 = (num5 = (num4 = float.NegativeInfinity));
		foreach (Vector3 vertex in vertices)
		{
			num2 = ((vertex.x < num2) ? vertex.x : num2);
			num5 = ((vertex.x > num5) ? vertex.x : num5);
			num = ((vertex.y < num) ? vertex.y : num);
			num4 = ((vertex.y > num4) ? vertex.y : num4);
			num3 = ((vertex.z < num3) ? vertex.z : num3);
			num6 = ((vertex.z > num6) ? vertex.z : num6);
		}
		Vector3 vector = new Vector3(num5, num4, num6);
		Vector3 vector2 = new Vector3(num2, num, num3);
		Vector3 size = vector - vector2;
		Vector3 center = (vector + vector2) / 2f;
		return new BoxColliderData
		{
			Center = center,
			ColliderType = CREATE_COLLIDER_TYPE.BOX,
			IsValid = true,
			Size = size
		};
	}

	public CapsuleColliderData CalculateCapsuleBestFit(List<Vector3> worldVertices, Transform attachTo, bool isRotated)
	{
		if (worldVertices.Count >= 3)
		{
			Quaternion identity = Quaternion.identity;
			List<Vector3> list = new List<Vector3>();
			Matrix4x4 matrix;
			if (isRotated)
			{
				Vector3 vector = worldVertices[1] - worldVertices[0];
				Vector3 upwards = Vector3.Cross(vector, worldVertices[2] - worldVertices[1]);
				identity = Quaternion.LookRotation(vector, upwards);
				matrix = Matrix4x4.TRS(attachTo.position, identity, Vector3.one);
				for (int i = 0; i < worldVertices.Count; i++)
				{
					list.Add(matrix.inverse.MultiplyPoint3x4(worldVertices[i]));
				}
			}
			else
			{
				list = ToLocalVerts(attachTo, worldVertices);
				matrix = attachTo.localToWorldMatrix;
			}
			CapsuleColliderData capsuleColliderData = CalculateCapsuleBestFitLocal(list);
			capsuleColliderData.ColliderType = (isRotated ? CREATE_COLLIDER_TYPE.ROTATED_CAPSULE : CREATE_COLLIDER_TYPE.CAPSULE);
			capsuleColliderData.Matrix = matrix;
			return capsuleColliderData;
		}
		return new CapsuleColliderData();
	}

	public CapsuleColliderData CalculateCapsuleBestFitLocal(List<Vector3> localVertices)
	{
		if (localVertices.Count < 3)
		{
			Debug.LogWarning("EasyColliderCreator: Too few vertices passed to calculate a best fit capsule collider.");
			return new CapsuleColliderData();
		}
		Vector3 a = localVertices[0];
		Vector3 b = localVertices[1];
		float height = Vector3.Distance(a, b);
		float num = Mathf.Abs(b.x - a.x);
		float num2 = Mathf.Abs(b.y - a.y);
		float num3 = Mathf.Abs(b.z - a.z);
		localVertices.RemoveAt(1);
		localVertices.RemoveAt(0);
		BestFitSphere bestFitSphere = CalculateBestFitSphere(localVertices);
		Vector3 center = bestFitSphere.Center;
		int num4 = 0;
		if (num > num2 && num > num3)
		{
			num4 = 0;
			center.x = (b.x + a.x) / 2f;
		}
		else if (num2 > num && num2 > num3)
		{
			num4 = 1;
			center.y = (b.y + a.y) / 2f;
		}
		else
		{
			num4 = 2;
			center.z = (b.z + a.z) / 2f;
		}
		return new CapsuleColliderData
		{
			Center = center,
			ColliderType = CREATE_COLLIDER_TYPE.CAPSULE,
			Direction = num4,
			Height = height,
			IsValid = true,
			Radius = bestFitSphere.Radius
		};
	}

	public CapsuleColliderData CalculateCapsuleMinMax(List<Vector3> worldVertices, Transform attachTo, CAPSULE_COLLIDER_METHOD method, bool isRotated)
	{
		if (isRotated && worldVertices.Count < 3)
		{
			return new CapsuleColliderData();
		}
		if (worldVertices.Count < 2)
		{
			return new CapsuleColliderData();
		}
		List<Vector3> list = new List<Vector3>();
		Matrix4x4 matrix;
		if (isRotated && worldVertices.Count >= 3)
		{
			Vector3 vector = worldVertices[1] - worldVertices[0];
			Vector3 upwards = Vector3.Cross(vector, worldVertices[2] - worldVertices[1]);
			Quaternion q = Quaternion.LookRotation(vector, upwards);
			matrix = Matrix4x4.TRS(attachTo.position, q, Vector3.one);
			for (int i = 0; i < worldVertices.Count; i++)
			{
				list.Add(matrix.inverse.MultiplyPoint3x4(worldVertices[i]));
			}
		}
		else
		{
			list = ToLocalVerts(attachTo.transform, worldVertices);
			matrix = attachTo.localToWorldMatrix;
		}
		CapsuleColliderData capsuleColliderData = CalculateCapsuleMinMaxLocal(list, method);
		capsuleColliderData.ColliderType = (isRotated ? CREATE_COLLIDER_TYPE.ROTATED_CAPSULE : CREATE_COLLIDER_TYPE.CAPSULE);
		capsuleColliderData.Matrix = matrix;
		return capsuleColliderData;
	}

	public CapsuleColliderData CalculateCapsuleMinMaxLocal(List<Vector3> localVertices, CAPSULE_COLLIDER_METHOD method)
	{
		Vector3 vector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		Vector3 vector2 = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		foreach (Vector3 localVertex in localVertices)
		{
			vector.x = ((localVertex.x < vector.x) ? localVertex.x : vector.x);
			vector.y = ((localVertex.y < vector.y) ? localVertex.y : vector.y);
			vector.z = ((localVertex.z < vector.z) ? localVertex.z : vector.z);
			vector2.x = ((localVertex.x > vector2.x) ? localVertex.x : vector2.x);
			vector2.y = ((localVertex.y > vector2.y) ? localVertex.y : vector2.y);
			vector2.z = ((localVertex.z > vector2.z) ? localVertex.z : vector2.z);
		}
		float num = vector2.x - vector.x;
		float num2 = vector2.y - vector.y;
		float num3 = vector2.z - vector.z;
		Vector3 vector3 = (vector2 + vector) / 2f;
		int num4 = 0;
		float num5 = 0f;
		if (num > num2 && num > num3)
		{
			num4 = 0;
			num5 = num;
		}
		else if (num2 > num && num2 > num3)
		{
			num4 = 1;
			num5 = num2;
		}
		else
		{
			num4 = 2;
			num5 = num3;
		}
		float num6 = float.NegativeInfinity;
		Vector3 zero = Vector3.zero;
		foreach (Vector3 localVertex2 in localVertices)
		{
			zero = localVertex2;
			switch (num4)
			{
			case 0:
				zero.x = vector3.x;
				break;
			case 1:
				zero.y = vector3.y;
				break;
			case 2:
				zero.z = vector3.z;
				break;
			}
			float num7 = Vector3.Distance(zero, vector3);
			if (num7 > num6)
			{
				num6 = num7;
			}
		}
		switch (method)
		{
		case CAPSULE_COLLIDER_METHOD.MinMaxPlusRadius:
			num5 += num6;
			break;
		case CAPSULE_COLLIDER_METHOD.MinMaxPlusDiameter:
			num5 += num6 * 2f;
			break;
		}
		return new CapsuleColliderData
		{
			Center = vector3,
			ColliderType = CREATE_COLLIDER_TYPE.CAPSULE,
			Direction = num4,
			Height = num5,
			IsValid = true,
			Radius = num6
		};
	}

	public MeshColliderData CalculateCylinderCollider(List<Vector3> worldVertices, Transform attachTo, int numberOfSides = 12, CYLINDER_ORIENTATION orientation = CYLINDER_ORIENTATION.Automatic, float cylinderOffset = 0f)
	{
		MeshColliderData meshColliderData = new MeshColliderData();
		List<Vector3> vertices = ToLocalVerts(attachTo, worldVertices);
		EasyColliderQuickHull easyColliderQuickHull = EasyColliderQuickHull.CalculateHull(CalculateCylinderPointsLocal(vertices, attachTo, numberOfSides, orientation, cylinderOffset));
		meshColliderData.ColliderType = CREATE_COLLIDER_TYPE.CONVEX_MESH;
		meshColliderData.ConvexMesh = easyColliderQuickHull.Result;
		if (easyColliderQuickHull.Result != null)
		{
			meshColliderData.IsValid = true;
		}
		meshColliderData.Matrix = attachTo.transform.localToWorldMatrix;
		return meshColliderData;
	}

	public MeshColliderData CalculateCylinderColliderLocal(List<Vector3> vertices, int numberOfSides = 12, CYLINDER_ORIENTATION orientation = CYLINDER_ORIENTATION.Automatic, float cylinderOffset = 0f)
	{
		MeshColliderData meshColliderData = new MeshColliderData();
		EasyColliderQuickHull easyColliderQuickHull = EasyColliderQuickHull.CalculateHull(CalculateCylinderPointsLocal(vertices, null, numberOfSides, orientation, cylinderOffset));
		meshColliderData.ColliderType = CREATE_COLLIDER_TYPE.CONVEX_MESH;
		meshColliderData.ConvexMesh = easyColliderQuickHull.Result;
		if (easyColliderQuickHull.Result != null)
		{
			meshColliderData.IsValid = true;
		}
		meshColliderData.Matrix = default(Matrix4x4);
		return meshColliderData;
	}

	public MeshColliderData CalculateMeshColliderQuickHullLocal(List<Vector3> localVertices)
	{
		MeshColliderData meshColliderData = new MeshColliderData();
		EasyColliderQuickHull easyColliderQuickHull = EasyColliderQuickHull.CalculateHull(localVertices);
		meshColliderData.ConvexMesh = easyColliderQuickHull.Result;
		if (easyColliderQuickHull.Result != null)
		{
			meshColliderData.ColliderType = CREATE_COLLIDER_TYPE.CONVEX_MESH;
			meshColliderData.IsValid = true;
		}
		return meshColliderData;
	}

	public SphereColliderData CalculateSphereBestFit(List<Vector3> worldVertices, Transform attachTo)
	{
		if (worldVertices.Count < 2)
		{
			return new SphereColliderData();
		}
		List<Vector3> localVertices = ToLocalVerts(attachTo, worldVertices);
		SphereColliderData sphereColliderData = CalculateSphereBestFitLocal(localVertices);
		sphereColliderData.Matrix = attachTo.localToWorldMatrix;
		return sphereColliderData;
	}

	public SphereColliderData CalculateSphereBestFitLocal(List<Vector3> localVertices)
	{
		BestFitSphere bestFitSphere = CalculateBestFitSphere(localVertices);
		return new SphereColliderData
		{
			Center = bestFitSphere.Center,
			ColliderType = CREATE_COLLIDER_TYPE.SPHERE,
			IsValid = true,
			Radius = bestFitSphere.Radius
		};
	}

	public SphereColliderData CalculateSphereDistance(List<Vector3> worldVertices, Transform attachTo)
	{
		if (worldVertices.Count < 2)
		{
			return new SphereColliderData();
		}
		List<Vector3> localVertices = ToLocalVerts(attachTo, worldVertices);
		SphereColliderData sphereColliderData = CalculateSphereDistanceLocal(localVertices);
		sphereColliderData.Matrix = attachTo.localToWorldMatrix;
		return sphereColliderData;
	}

	public SphereColliderData CalculateSphereDistanceLocal(List<Vector3> localVertices)
	{
		bool flag = false;
		double num = Time.realtimeSinceStartup;
		double num2 = 0.10000000149011612;
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		float num3 = float.NegativeInfinity;
		float num4 = 0f;
		for (int i = 0; i < localVertices.Count; i++)
		{
			for (int j = i + 1; j < localVertices.Count; j++)
			{
				num4 = Vector3.Distance(localVertices[i], localVertices[j]);
				if (num4 > num3)
				{
					num3 = num4;
					vector = localVertices[i];
					vector2 = localVertices[j];
				}
			}
			if ((double)Time.realtimeSinceStartup - num > num2)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			Vector3 zero = Vector3.zero;
			foreach (Vector3 localVertex in localVertices)
			{
				zero += localVertex;
			}
			zero /= (float)localVertices.Count;
			foreach (Vector3 localVertex2 in localVertices)
			{
				num4 = Vector3.Distance(localVertex2, zero);
				if (num4 > num3)
				{
					vector = localVertex2;
					num3 = num4;
				}
			}
			num3 = float.NegativeInfinity;
			foreach (Vector3 localVertex3 in localVertices)
			{
				num4 = Vector3.Distance(localVertex3, vector);
				if (num4 > num3)
				{
					num3 = num4;
					vector2 = localVertex3;
				}
			}
		}
		return new SphereColliderData
		{
			Center = (vector + vector2) / 2f,
			ColliderType = CREATE_COLLIDER_TYPE.SPHERE,
			IsValid = true,
			Radius = num3 / 2f
		};
	}

	public SphereColliderData CalculateSphereMinMax(List<Vector3> worldVertices, Transform attachTo)
	{
		if (worldVertices.Count < 2)
		{
			return new SphereColliderData();
		}
		List<Vector3> localVertices = ToLocalVerts(attachTo, worldVertices);
		SphereColliderData sphereColliderData = CalculateSphereMinMaxLocal(localVertices);
		sphereColliderData.Matrix = attachTo.localToWorldMatrix;
		return sphereColliderData;
	}

	public SphereColliderData CalculateSphereMinMaxLocal(List<Vector3> localVertices)
	{
		float num2;
		float num;
		float num3 = (num2 = (num = float.PositiveInfinity));
		float num5;
		float num4;
		float num6 = (num5 = (num4 = float.NegativeInfinity));
		for (int i = 0; i < localVertices.Count; i++)
		{
			num2 = ((localVertices[i].x < num2) ? localVertices[i].x : num2);
			num5 = ((localVertices[i].x > num5) ? localVertices[i].x : num5);
			num = ((localVertices[i].y < num) ? localVertices[i].y : num);
			num4 = ((localVertices[i].y > num4) ? localVertices[i].y : num4);
			num3 = ((localVertices[i].z < num3) ? localVertices[i].z : num3);
			num6 = ((localVertices[i].z > num6) ? localVertices[i].z : num6);
		}
		Vector3 vector = (new Vector3(num2, num, num3) + new Vector3(num5, num4, num6)) / 2f;
		float num7 = 0f;
		float num8 = 0f;
		foreach (Vector3 localVertex in localVertices)
		{
			num8 = Vector3.Distance(localVertex, vector);
			if (num8 > num7)
			{
				num7 = num8;
			}
		}
		return new SphereColliderData
		{
			Center = vector,
			ColliderType = CREATE_COLLIDER_TYPE.SPHERE,
			IsValid = true,
			Radius = num7
		};
	}

	private BoxCollider CreateBoxCollider(BoxColliderData data, EasyColliderProperties properties, bool postProcess = true)
	{
		BoxCollider boxCollider = properties.AttachTo.AddComponent<BoxCollider>();
		boxCollider.size = data.Size;
		boxCollider.center = data.Center;
		PostColliderCreation(boxCollider, properties, postProcess);
		return boxCollider;
	}

	public BoxCollider CreateBoxCollider(List<Vector3> vertices, EasyColliderProperties properties, bool isLocal = false)
	{
		if (vertices.Count >= 2)
		{
			BoxColliderData data;
			if (properties.Orientation != COLLIDER_ORIENTATION.ROTATED)
			{
				data = (isLocal ? CalculateBoxLocal(vertices) : CalculateBox(vertices, properties.AttachTo.transform));
			}
			else
			{
				if (vertices.Count < 3)
				{
					Debug.LogWarning("Easy Collider Editor: Creating a Rotated Box Collider requires at least 3 points to be selected.");
					return null;
				}
				GameObject gameObject = CreateGameObjectOrientation(vertices, properties.AttachTo, "Rotated Box Collider");
				if (gameObject != null)
				{
					gameObject.layer = properties.Layer;
					properties.AttachTo = gameObject;
				}
				data = CalculateBox(vertices, properties.AttachTo.transform, isRotated: true);
			}
			return CreateBoxCollider(data, properties);
		}
		return null;
	}

	private CapsuleCollider CreateCapsuleCollider(CapsuleColliderData data, EasyColliderProperties properties, bool postProcess = true)
	{
		CapsuleCollider capsuleCollider = properties.AttachTo.AddComponent<CapsuleCollider>();
		capsuleCollider.direction = data.Direction;
		capsuleCollider.height = data.Height;
		capsuleCollider.center = data.Center;
		capsuleCollider.radius = data.Radius;
		PostColliderCreation(capsuleCollider, properties);
		return capsuleCollider;
	}

	public CapsuleCollider CreateCapsuleCollider_BestFit(List<Vector3> worldVertices, EasyColliderProperties properties)
	{
		if (worldVertices.Count >= 3)
		{
			CapsuleColliderData capsuleColliderData = new CapsuleColliderData();
			if (properties.Orientation == COLLIDER_ORIENTATION.ROTATED)
			{
				GameObject gameObject = CreateGameObjectOrientation(worldVertices, properties.AttachTo, "Rotated Capsule Collider");
				if (gameObject != null)
				{
					properties.AttachTo = gameObject;
					gameObject.layer = properties.Layer;
				}
				capsuleColliderData = CalculateCapsuleBestFit(worldVertices, properties.AttachTo.transform, isRotated: true);
			}
			else
			{
				capsuleColliderData = CalculateCapsuleBestFit(worldVertices, properties.AttachTo.transform, isRotated: false);
			}
			return CreateCapsuleCollider(capsuleColliderData, properties);
		}
		return null;
	}

	public CapsuleCollider CreateCapsuleCollider_MinMax(List<Vector3> worldVertices, EasyColliderProperties properties, CAPSULE_COLLIDER_METHOD method, bool isLocal = false)
	{
		CapsuleColliderData data;
		if (properties.Orientation != COLLIDER_ORIENTATION.ROTATED || worldVertices.Count < 3)
		{
			data = (isLocal ? CalculateCapsuleMinMaxLocal(worldVertices, method) : CalculateCapsuleMinMax(worldVertices, properties.AttachTo.transform, method, isRotated: false));
		}
		else
		{
			GameObject gameObject = CreateGameObjectOrientation(worldVertices, properties.AttachTo, "Rotated Capsule Collider");
			if (gameObject != null)
			{
				properties.AttachTo = gameObject;
				gameObject.layer = properties.AttachTo.layer;
			}
			data = CalculateCapsuleMinMax(worldVertices, properties.AttachTo.transform, method, isRotated: true);
		}
		return CreateCapsuleCollider(data, properties);
	}

	public MeshCollider CreateConvexMeshCollider(Mesh mesh, GameObject attachToObject, EasyColliderProperties properties, bool postProcess = true)
	{
		MeshCollider meshCollider = attachToObject.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = mesh;
		meshCollider.cookingOptions = (MeshColliderCookingOptions)(-1);
		meshCollider.convex = true;
		PostColliderCreation(meshCollider, properties, postProcess);
		return meshCollider;
	}

	private SphereCollider CreateSphereCollider(SphereColliderData data, EasyColliderProperties properties, bool postProcess = true)
	{
		SphereCollider sphereCollider = properties.AttachTo.AddComponent<SphereCollider>();
		sphereCollider.radius = data.Radius;
		sphereCollider.center = data.Center;
		PostColliderCreation(sphereCollider, properties, postProcess);
		return sphereCollider;
	}

	public SphereCollider CreateSphereCollider_BestFit(List<Vector3> worldVertices, EasyColliderProperties properties)
	{
		if (worldVertices.Count >= 2)
		{
			SphereColliderData data = CalculateSphereBestFit(worldVertices, properties.AttachTo.transform);
			return CreateSphereCollider(data, properties);
		}
		return null;
	}

	public SphereCollider CreateSphereCollider_Distance(List<Vector3> worldVertices, EasyColliderProperties properties)
	{
		if (worldVertices.Count >= 2)
		{
			SphereColliderData data = CalculateSphereDistance(worldVertices, properties.AttachTo.transform);
			return CreateSphereCollider(data, properties);
		}
		return null;
	}

	public SphereCollider CreateSphereCollider_MinMax(List<Vector3> worldVertices, EasyColliderProperties properties, bool isLocal = false)
	{
		if (worldVertices.Count >= 2)
		{
			if (!isLocal)
			{
				SphereColliderData data = CalculateSphereMinMax(worldVertices, properties.AttachTo.transform);
				return CreateSphereCollider(data, properties);
			}
			SphereColliderData data2 = CalculateSphereMinMaxLocal(worldVertices);
			return CreateSphereCollider(data2, properties);
		}
		return null;
	}

	public void PostColliderCreationProcess(Collider createdCollider, EasyColliderProperties properties)
	{
	}

	public List<Vector3> CalculateCylinderPointsLocal(List<Vector3> vertices, Transform attachTo, int numberOfSides, CYLINDER_ORIENTATION orientation, float cylinderOffset)
	{
		BoxColliderData boxColliderData = CalculateBoxLocal(vertices);
		float num = 0f;
		int num2 = 0;
		if (orientation == CYLINDER_ORIENTATION.Automatic)
		{
			num = Mathf.Max(Mathf.Max(boxColliderData.Size.x, boxColliderData.Size.y), boxColliderData.Size.z);
			num2 = ((num != boxColliderData.Size.x) ? ((num == boxColliderData.Size.y) ? 1 : 2) : 0);
		}
		else
		{
			num = boxColliderData.Size[(int)(orientation - 1)];
			num2 = (int)(orientation - 1);
		}
		float num3 = 0f;
		float num4 = 0f;
		Vector3 zero = Vector3.zero;
		foreach (Vector3 vertex in vertices)
		{
			zero.x = ((num2 == 0) ? boxColliderData.Center.x : vertex.x);
			zero.y = ((num2 == 1) ? boxColliderData.Center.y : vertex.y);
			zero.z = ((num2 == 2) ? boxColliderData.Center.z : vertex.z);
			num3 = Vector3.Distance(zero, boxColliderData.Center);
			if (num3 > num4)
			{
				num4 = num3;
			}
		}
		float num5 = num / 2f;
		float num6 = 360f / (float)numberOfSides;
		Vector3 center;
		Vector3 item = (center = boxColliderData.Center);
		center.x = ((num2 == 0) ? (center.x + num5) : center.x);
		center.y = ((num2 == 1) ? (center.y + num5) : center.y);
		center.z = ((num2 == 2) ? (center.z + num5) : center.z);
		item.x = ((num2 == 0) ? (item.x - num5) : item.x);
		item.y = ((num2 == 1) ? (item.y - num5) : item.y);
		item.z = ((num2 == 2) ? (item.z - num5) : item.z);
		List<Vector3> list = new List<Vector3>();
		for (float num7 = 0f + cylinderOffset; num7 < 360f + cylinderOffset; num7 += num6)
		{
			float num8 = num4 * Mathf.Sin(num7 * ((float)Math.PI / 180f));
			float num9 = num4 * Mathf.Cos(num7 * ((float)Math.PI / 180f));
			switch (num2)
			{
			case 0:
				center.y = num8 + boxColliderData.Center.y;
				center.z = num9 + boxColliderData.Center.z;
				item.y = num8 + boxColliderData.Center.y;
				item.z = num9 + boxColliderData.Center.z;
				break;
			case 1:
				center.x = num8 + boxColliderData.Center.x;
				center.z = num9 + boxColliderData.Center.z;
				item.x = num8 + boxColliderData.Center.x;
				item.z = num9 + boxColliderData.Center.z;
				break;
			default:
				center.y = num8 + boxColliderData.Center.y;
				center.x = num9 + boxColliderData.Center.x;
				item.y = num8 + boxColliderData.Center.y;
				item.x = num9 + boxColliderData.Center.x;
				break;
			}
			list.Add(center);
			list.Add(item);
		}
		return list;
	}

	private GameObject CreateGameObjectOrientation(List<Vector3> worldVertices, GameObject parent, string name)
	{
		GameObject gameObject = new GameObject(name);
		if (worldVertices.Count >= 3)
		{
			Vector3 vector = worldVertices[1] - worldVertices[0];
			Vector3 upwards = Vector3.Cross(vector, worldVertices[2] - worldVertices[1]);
			gameObject.transform.rotation = Quaternion.LookRotation(vector, upwards);
			gameObject.transform.SetParent(parent.transform);
			gameObject.transform.localPosition = Vector3.zero;
			return gameObject;
		}
		return null;
	}

	private void DebugDrawPoint(Vector3 worldLoc, Color color, float dist = 0.01f)
	{
		Debug.DrawLine(worldLoc - Vector3.up * dist, worldLoc + Vector3.up * dist, color, 0.01f, depthTest: false);
		Debug.DrawLine(worldLoc - Vector3.left * dist, worldLoc + Vector3.left * dist, color, 0.01f, depthTest: false);
		Debug.DrawLine(worldLoc - Vector3.forward * dist, worldLoc + Vector3.forward * dist, color, 0.01f, depthTest: false);
	}

	private void PostColliderCreation(Collider collider, EasyColliderProperties properties, bool postProcess = true)
	{
		SetPropertiesOnCollider(collider, properties);
	}

	private void SetPropertiesOnCollider(Collider collider, EasyColliderProperties properties)
	{
		if (collider != null)
		{
			collider.isTrigger = properties.IsTrigger;
			collider.sharedMaterial = properties.PhysicMaterial;
		}
	}

	private List<Vector3> ToLocalVerts(Transform transform, List<Vector3> worldVertices)
	{
		List<Vector3> list = new List<Vector3>(worldVertices.Count);
		foreach (Vector3 worldVertex in worldVertices)
		{
			list.Add(transform.InverseTransformPoint(worldVertex));
		}
		return list;
	}
}
