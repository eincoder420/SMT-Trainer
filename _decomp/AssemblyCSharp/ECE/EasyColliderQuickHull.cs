using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ECE;

public class EasyColliderQuickHull
{
	private class Face
	{
		public int F0;

		public int F1;

		public int F2;

		public Vector3 Normal;

		public bool OnConvexHull;

		public List<int> OutsideVertices;

		public int V0;

		public int V1;

		public int V2;

		public Face(int v0, int v1, int v2, Vector3 normal, int f0, int f1, int f2)
		{
			V0 = v0;
			V1 = v1;
			V2 = v2;
			Normal = normal;
			OutsideVertices = new List<int>();
			F0 = f0;
			F1 = f1;
			F2 = f2;
			OnConvexHull = true;
		}
	}

	private class Horizon
	{
		public int Face;

		public int From;

		public bool OnConvexHull;

		public int V0;

		public int V1;

		public Horizon(int v0, int v1, int face, int from)
		{
			V0 = v0;
			V1 = v1;
			Face = face;
			From = from;
			OnConvexHull = true;
		}
	}

	protected bool DebugHorizon;

	protected Color DebugHorizonColor = new Color(1f, 0.5f, 0f, 1f);

	protected int DebugLoopNumber;

	protected int DebugMaxLoopNumber;

	protected bool DebugNewFaces;

	protected bool DebugNormals;

	protected bool DebugOutsideSet;

	protected Color DebugNormalColor = new Color(0.5f, 0f, 0.5f, 1f);

	protected float DrawTime = 2f;

	private HashSet<int> AssignedVertices = new HashSet<int>();

	private HashSet<int> ClosedVertices = new HashSet<int>();

	private List<Horizon> CurrentHorizon = new List<Horizon>();

	private float Epsilon = 1E-06f;

	private List<Face> Faces = new List<Face>();

	private List<int> NewFaces = new List<int>();

	public Mesh Result;

	private HashSet<int> UnAssignedVertices = new HashSet<int>();

	private List<Vector3> VerticesList = new List<Vector3>();

	public bool isFinished => Result != null;

	public static EasyColliderQuickHull CalculateHull(List<Vector3> points)
	{
		EasyColliderQuickHull easyColliderQuickHull = new EasyColliderQuickHull();
		easyColliderQuickHull.GenerateHull(points);
		return easyColliderQuickHull;
	}

	public static EasyColliderQuickHull CalculateHullWorld(List<Vector3> points, Transform attachTo)
	{
		List<Vector3> list = new List<Vector3>();
		foreach (Vector3 point in points)
		{
			list.Add(attachTo.InverseTransformPoint(point));
		}
		EasyColliderQuickHull easyColliderQuickHull = new EasyColliderQuickHull();
		easyColliderQuickHull.GenerateHull(list);
		return easyColliderQuickHull;
	}

	public static MeshColliderData CalculateHullData(List<Vector3> points, Transform attachTo)
	{
		if (points == null || points.Count < 4)
		{
			return new MeshColliderData();
		}
		EasyColliderQuickHull easyColliderQuickHull = CalculateHullWorld(points, attachTo);
		return new MeshColliderData
		{
			ConvexMesh = easyColliderQuickHull.Result,
			IsValid = true,
			Matrix = attachTo.localToWorldMatrix,
			ColliderType = CREATE_COLLIDER_TYPE.CONVEX_MESH
		};
	}

	public static MeshColliderData CalculateHullData(List<Vector3> points)
	{
		EasyColliderQuickHull easyColliderQuickHull = CalculateHull(points);
		return new MeshColliderData
		{
			ConvexMesh = easyColliderQuickHull.Result,
			IsValid = true,
			ColliderType = CREATE_COLLIDER_TYPE.CONVEX_MESH
		};
	}

	private void AddToOutsideSet(Face face, HashSet<int> vertices)
	{
		float num = 0f;
		foreach (int vertex in vertices)
		{
			if (AssignedVertices.Contains(vertex) || ClosedVertices.Contains(vertex))
			{
				continue;
			}
			num = DistanceFromPlane(VerticesList[vertex], face.Normal, VerticesList[face.V0]);
			if (IsApproxZero(num))
			{
				if (IsVertOnFace(vertex, face))
				{
					ClosedVertices.Add(vertex);
				}
			}
			else if (num > 0f)
			{
				AssignedVertices.Add(vertex);
				face.OutsideVertices.Add(vertex);
			}
		}
	}

	private bool AreVertsCoincident(Vector3 a, Vector3 b)
	{
		if (Mathf.Abs(a.x - b.x) > Epsilon || Mathf.Abs(a.y - b.y) > Epsilon || Mathf.Abs(a.z - b.z) > Epsilon)
		{
			return false;
		}
		return true;
	}

	private bool AreVertsCoincident(int a, int b)
	{
		if (Mathf.Abs(VerticesList[a].x - VerticesList[b].x) > Epsilon || Mathf.Abs(VerticesList[a].y - VerticesList[b].y) > Epsilon || Mathf.Abs(VerticesList[a].z - VerticesList[b].z) > Epsilon)
		{
			return false;
		}
		return true;
	}

	private void CloseUnAssignedVertsOnFaces()
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (Face face in Faces)
		{
			if (!face.OnConvexHull)
			{
				continue;
			}
			foreach (int unAssignedVertex in UnAssignedVertices)
			{
				if (!ClosedVertices.Contains(unAssignedVertex) && IsVertOnFace(unAssignedVertex, face))
				{
					hashSet.Add(unAssignedVertex);
					ClosedVertices.Add(unAssignedVertex);
				}
			}
		}
		UnAssignedVertices.ExceptWith(hashSet);
	}

	private bool IsVertOnFace(int i, Face face)
	{
		if (AreVertsCoincident(i, face.V0) || AreVertsCoincident(i, face.V1) || AreVertsCoincident(i, face.V2))
		{
			return true;
		}
		float a = CalcTriangleArea(face.V0, face.V1, face.V2);
		float num = CalcTriangleArea(i, face.V0, face.V1);
		float num2 = CalcTriangleArea(i, face.V1, face.V2);
		float num3 = CalcTriangleArea(i, face.V2, face.V0);
		if (isApproxEqual(a, num + num2 + num3))
		{
			return true;
		}
		return false;
	}

	private Vector3 CalcNormal(Vector3 a, Vector3 b, Vector3 c)
	{
		return Vector3.Cross(b - a, c - a).normalized;
	}

	private Vector3 CalcNormal(int a, int b, int c)
	{
		return Vector3.Cross(VerticesList[b] - VerticesList[a], VerticesList[c] - VerticesList[a]).normalized;
	}

	private float CalcTriangleArea(int v0, int v1, int v2)
	{
		return 0.5f * Vector3.Cross(VerticesList[v1] - VerticesList[v0], VerticesList[v2] - VerticesList[v1]).magnitude;
	}

	private void CalculateHorizon(int eyePoint, Horizon crossedEdge, int currFace, bool firstFace = true)
	{
		float num = DistanceFromPlane(VerticesList[eyePoint], Faces[currFace].Normal, VerticesList[Faces[currFace].V0]);
		if (!Faces[currFace].OnConvexHull)
		{
			crossedEdge.OnConvexHull = false;
		}
		else if (num > 0f)
		{
			Faces[currFace].OnConvexHull = false;
			UnAssignedVertices.UnionWith(Faces[currFace].OutsideVertices);
			Faces[currFace].OutsideVertices.Clear();
			if (!firstFace)
			{
				crossedEdge.OnConvexHull = false;
			}
			if (firstFace)
			{
				CurrentHorizon.Add(new Horizon(Faces[currFace].V0, Faces[currFace].V1, Faces[currFace].F0, currFace));
				CalculateHorizon(eyePoint, CurrentHorizon[CurrentHorizon.Count - 1], Faces[currFace].F0, firstFace: false);
				CurrentHorizon.Add(new Horizon(Faces[currFace].V1, Faces[currFace].V2, Faces[currFace].F1, currFace));
				CalculateHorizon(eyePoint, CurrentHorizon[CurrentHorizon.Count - 1], Faces[currFace].F1, firstFace: false);
				CurrentHorizon.Add(new Horizon(Faces[currFace].V2, Faces[currFace].V0, Faces[currFace].F2, currFace));
				CalculateHorizon(eyePoint, CurrentHorizon[CurrentHorizon.Count - 1], Faces[currFace].F2, firstFace: false);
			}
			else if (Faces[currFace].F0 == crossedEdge.From)
			{
				CurrentHorizon.Add(new Horizon(Faces[currFace].V1, Faces[currFace].V2, Faces[currFace].F1, currFace));
				CalculateHorizon(eyePoint, CurrentHorizon[CurrentHorizon.Count - 1], Faces[currFace].F1, firstFace: false);
				CurrentHorizon.Add(new Horizon(Faces[currFace].V2, Faces[currFace].V0, Faces[currFace].F2, currFace));
				CalculateHorizon(eyePoint, CurrentHorizon[CurrentHorizon.Count - 1], Faces[currFace].F2, firstFace: false);
			}
			else if (Faces[currFace].F1 == crossedEdge.From)
			{
				CurrentHorizon.Add(new Horizon(Faces[currFace].V2, Faces[currFace].V0, Faces[currFace].F2, currFace));
				CalculateHorizon(eyePoint, CurrentHorizon[CurrentHorizon.Count - 1], Faces[currFace].F2, firstFace: false);
				CurrentHorizon.Add(new Horizon(Faces[currFace].V0, Faces[currFace].V1, Faces[currFace].F0, currFace));
				CalculateHorizon(eyePoint, CurrentHorizon[CurrentHorizon.Count - 1], Faces[currFace].F0, firstFace: false);
			}
			else if (Faces[currFace].F2 == crossedEdge.From)
			{
				CurrentHorizon.Add(new Horizon(Faces[currFace].V0, Faces[currFace].V1, Faces[currFace].F0, currFace));
				CalculateHorizon(eyePoint, CurrentHorizon[CurrentHorizon.Count - 1], Faces[currFace].F0, firstFace: false);
				CurrentHorizon.Add(new Horizon(Faces[currFace].V1, Faces[currFace].V2, Faces[currFace].F1, currFace));
				CalculateHorizon(eyePoint, CurrentHorizon[CurrentHorizon.Count - 1], Faces[currFace].F1, firstFace: false);
			}
		}
	}

	private Mesh CreateMesh(List<Face> allFaces)
	{
		Mesh mesh = new Mesh();
		List<Vector3> list = new List<Vector3>();
		List<Face> list2 = allFaces.Where((Face face) => face.OnConvexHull).ToList();
		List<Vector3> list3 = new List<Vector3>();
		int[] array = new int[list2.Count * 3];
		int num;
		int num2 = (num = 0);
		for (int i = 0; i < list2.Count; i++)
		{
			num = list.IndexOf(VerticesList[list2[i].V0]);
			num2 = list.IndexOf(VerticesList[list2[i].V1]);
			int num3 = list.IndexOf(VerticesList[list2[i].V2]);
			if (num < 0)
			{
				list3.Add(list2[i].Normal);
				list.Add(VerticesList[list2[i].V0]);
				num = list.Count - 1;
			}
			else
			{
				list3[num] = (list3[num] + list2[i].Normal).normalized;
			}
			if (num2 < 0)
			{
				list3.Add(list2[i].Normal);
				list.Add(VerticesList[list2[i].V1]);
				num2 = list.Count - 1;
			}
			else
			{
				list3[num2] = (list3[num2] + list2[i].Normal).normalized;
			}
			if (num3 < 0)
			{
				list3.Add(list2[i].Normal);
				list.Add(VerticesList[list2[i].V2]);
				num3 = list.Count - 1;
			}
			else
			{
				list3[num3] = (list3[num3] + list2[i].Normal).normalized;
			}
			array[i * 3] = num;
			array[i * 3 + 1] = num2;
			array[i * 3 + 2] = num3;
		}
		mesh.SetVertices(list);
		mesh.SetTriangles(array, 0);
		mesh.SetNormals(list3);
		return mesh;
	}

	private float DistanceFromLine(Vector3 point, Vector3 line, Vector3 pointOnLine)
	{
		float num = Vector3.Dot(point - pointOnLine, line);
		return Vector3.Distance(pointOnLine + num * line, point);
	}

	private float DistanceFromPlane(Vector3 point, Plane p)
	{
		return p.GetDistanceToPoint(point);
	}

	private float DistanceFromPlane(Vector3 point, Vector3 normal, Vector3 pointOnPlane)
	{
		return Vector3.Dot(normal, point - pointOnPlane);
	}

	private bool FindInitialHull(List<Vector3> points)
	{
		bool flag = false;
		if (FindInitialPoints(points, out var initialPoints))
		{
			flag = true;
		}
		else if (FindInitialPointsFallBack(points, out initialPoints))
		{
			flag = true;
		}
		if (flag)
		{
			float b = float.NegativeInfinity;
			int num = 0;
			Vector3 normalized = (points[initialPoints[1]] - points[initialPoints[0]]).normalized;
			int index = 0;
			for (int i = 2; i < 6; i++)
			{
				float num2 = DistanceFromLine(points[initialPoints[i]], normalized, points[initialPoints[0]]);
				if (isAGreaterThanB(num2, b))
				{
					b = num2;
					num = initialPoints[i];
					index = i;
				}
			}
			initialPoints[index] = initialPoints[2];
			initialPoints[2] = num;
			b = float.NegativeInfinity;
			Plane p = new Plane(points[initialPoints[0]], points[initialPoints[1]], points[num]);
			int num3 = -1;
			for (int j = 2; j < 6; j++)
			{
				if (initialPoints[j] != num)
				{
					float num4 = DistanceFromPlane(points[initialPoints[j]], p);
					if (!IsApproxZero(num4) && isAGreaterThanB(Mathf.Abs(num4), b))
					{
						num3 = initialPoints[j];
						b = num4;
						index = j;
					}
				}
			}
			if (num3 == -1)
			{
				return false;
			}
			initialPoints[index] = initialPoints[3];
			initialPoints[3] = num3;
			if (DistanceFromPlane(points[num3], p) < 0f)
			{
				int value = initialPoints[2];
				initialPoints[2] = initialPoints[0];
				initialPoints[0] = value;
			}
			Faces.Add(new Face(initialPoints[0], initialPoints[2], initialPoints[1], CalcNormal(points[initialPoints[0]], points[initialPoints[2]], points[initialPoints[1]]), 2, 3, 1));
			Faces.Add(new Face(initialPoints[0], initialPoints[1], initialPoints[3], CalcNormal(points[initialPoints[0]], points[initialPoints[1]], points[initialPoints[3]]), 0, 3, 2));
			Faces.Add(new Face(initialPoints[0], initialPoints[3], initialPoints[2], CalcNormal(points[initialPoints[0]], points[initialPoints[3]], points[initialPoints[2]]), 1, 3, 0));
			Faces.Add(new Face(initialPoints[1], initialPoints[2], initialPoints[3], CalcNormal(points[initialPoints[1]], points[initialPoints[2]], points[initialPoints[3]]), 0, 2, 1));
			UnAssignedVertices.UnionWith(Enumerable.Range(0, points.Count));
			AssignedVertices = new HashSet<int>();
			foreach (Face face in Faces)
			{
				AddToOutsideSet(face, UnAssignedVertices);
			}
			ClosedVertices.UnionWith(UnAssignedVertices);
			ClosedVertices.ExceptWith(AssignedVertices);
			return true;
		}
		return false;
	}

	private bool FindInitialPointsFallBack(List<Vector3> points, out List<int> initialPoints)
	{
		List<int> ips = new List<int>(6) { -1, -1, -1, -1, -1, -1 };
		initialPoints = new List<int>(6) { -1, -1, -1, -1, -1, -1 };
		Vector3 vector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		Vector3 vector2;
		Vector3 vector3 = (vector2 = vector);
		Vector3 vector4 = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		Vector3 vector5;
		Vector3 vector6 = (vector5 = vector4);
		for (int i = 0; i < points.Count; i++)
		{
			if (isALessThanB(points[i].x, vector.x) || (isApproxEqual(points[i].x, vector.x) && initialPoints.FindAll((int element) => element == ips[0]).Count > 1))
			{
				initialPoints[0] = i;
				ips[0] = i;
				vector = points[i];
			}
			if (isAGreaterThanB(points[i].x, vector4.x) || (isApproxEqual(points[i].x, vector4.x) && initialPoints.FindAll((int element) => element == ips[1]).Count > 1))
			{
				initialPoints[1] = i;
				ips[1] = i;
				vector4 = points[i];
			}
			if (isALessThanB(points[i].y, vector2.y) || (isApproxEqual(points[i].y, vector2.y) && initialPoints.FindAll((int element) => element == ips[2]).Count > 1))
			{
				initialPoints[2] = i;
				ips[2] = i;
				vector2 = points[i];
			}
			if (isAGreaterThanB(points[i].y, vector5.y) || (isApproxEqual(points[i].y, vector5.y) && initialPoints.FindAll((int element) => element == ips[3]).Count > 1))
			{
				initialPoints[3] = i;
				ips[3] = i;
				vector5 = points[i];
			}
			if (isALessThanB(points[i].z, vector3.z) || (isApproxEqual(points[i].z, vector3.z) && initialPoints.FindAll((int element) => element == ips[4]).Count > 1))
			{
				initialPoints[4] = i;
				ips[4] = i;
				vector3 = points[i];
			}
			if (isAGreaterThanB(points[i].z, vector6.z) || (isApproxEqual(points[i].z, vector6.z) && initialPoints.FindAll((int element) => element == ips[5]).Count > 1))
			{
				initialPoints[5] = i;
				ips[5] = i;
				vector6 = points[i];
			}
		}
		if (!isApproxEqual(vector.x, vector4.x) && !isApproxEqual(vector2.y, vector5.y) && !isApproxEqual(vector3.z, vector6.z))
		{
			return true;
		}
		return false;
	}

	private bool FindInitialPoints(List<Vector3> points, out List<int> initialPoints)
	{
		initialPoints = new List<int>(6) { -1, -1, -1, -1, -1, -1 };
		Vector3 vector;
		Vector3 zero;
		Vector3 vector2 = (vector = (zero = Vector3.zero));
		for (int i = 0; i < points.Count; i++)
		{
			if (i + 3 >= points.Count || i + 2 >= points.Count || i + 1 >= points.Count)
			{
				continue;
			}
			vector2 = points[i];
			vector = points[i + 1];
			zero = points[i + 2];
			Vector3 vector3 = points[i + 3];
			float a = Mathf.Abs(Vector3.Dot(vector2 - vector3, Vector3.Cross(vector - vector3, zero - vector3))) / 6f;
			if (!IsApproxZero(a))
			{
				initialPoints[0] = i;
				initialPoints[1] = i + 1;
				initialPoints[2] = i + 2;
				initialPoints[3] = i + 3;
				if (i + 4 < points.Count)
				{
					initialPoints[4] = i + 4;
				}
				else
				{
					initialPoints[4] = i;
				}
				if (i + 5 < points.Count)
				{
					initialPoints[5] = i + 5;
				}
				else
				{
					initialPoints[5] = i;
				}
				return true;
			}
			for (int j = i + 4; j < points.Count; j++)
			{
				vector3 = points[j];
				a = Mathf.Abs(Vector3.Dot(vector2 - vector3, Vector3.Cross(vector - vector3, zero - vector3))) / 6f;
				if (!IsApproxZero(a))
				{
					initialPoints[0] = i;
					initialPoints[1] = i + 1;
					initialPoints[2] = i + 2;
					initialPoints[3] = j;
					if (i + 4 < points.Count)
					{
						initialPoints[4] = i + 4;
					}
					else
					{
						initialPoints[4] = i;
					}
					if (i + 5 < points.Count)
					{
						initialPoints[5] = i + 5;
					}
					else
					{
						initialPoints[5] = i;
					}
					return true;
				}
			}
		}
		return false;
	}

	private void CalculateEpsilon(List<Vector3> points)
	{
		Vector3 a = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		Vector3 b = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
		foreach (Vector3 point in points)
		{
			if (point.x < a.x)
			{
				a.x = point.x;
			}
			if (point.y < a.y)
			{
				a.y = point.y;
			}
			if (point.z < a.z)
			{
				a.z = point.z;
			}
			if (point.x > b.x)
			{
				b.x = point.x;
			}
			if (point.y > b.y)
			{
				b.y = point.y;
			}
			if (point.z > b.z)
			{
				b.z = point.z;
			}
		}
		Epsilon = Vector3.Distance(a, b) * 1E-06f;
	}

	public void GenerateHull(List<Vector3> points)
	{
		CalculateEpsilon(points);
		VerticesList = points;
		if (!FindInitialHull(points))
		{
			return;
		}
		while (HaveNonEmptyFaceSet())
		{
			UnAssignedVertices = new HashSet<int>();
			CurrentHorizon = new List<Horizon>();
			int nonEmptyFaceIndex = GetNonEmptyFaceIndex();
			int furthestPointFromFace = GetFurthestPointFromFace(nonEmptyFaceIndex);
			CalculateHorizon(furthestPointFromFace, null, nonEmptyFaceIndex);
			AssignedVertices.ExceptWith(UnAssignedVertices);
			int count = Faces.Count;
			int f = Faces.Count + CurrentHorizon.Where((Horizon item) => item.OnConvexHull).ToList().Count - 1;
			int count2 = CurrentHorizon.Where((Horizon item) => item.OnConvexHull).ToList().Count;
			NewFaces = new List<int>();
			int num = 0;
			for (int i = 0; i < CurrentHorizon.Count; i++)
			{
				Horizon horizon = CurrentHorizon[i];
				if (horizon.OnConvexHull)
				{
					if (num == 0)
					{
						Faces.Add(new Face(horizon.V0, horizon.V1, furthestPointFromFace, CalcNormal(horizon.V0, horizon.V1, furthestPointFromFace), horizon.Face, Faces.Count + 1, f));
					}
					else if (num == count2 - 1)
					{
						Faces.Add(new Face(horizon.V0, horizon.V1, furthestPointFromFace, CalcNormal(horizon.V0, horizon.V1, furthestPointFromFace), horizon.Face, count, Faces.Count - 1));
					}
					else
					{
						Faces.Add(new Face(horizon.V0, horizon.V1, furthestPointFromFace, CalcNormal(horizon.V0, horizon.V1, furthestPointFromFace), horizon.Face, Faces.Count + 1, Faces.Count - 1));
					}
					NewFaces.Add(Faces.Count - 1);
					UpdateFace(horizon, Faces.Count - 1);
					num++;
				}
			}
			CloseUnAssignedVertsOnFaces();
			for (int j = 0; j < NewFaces.Count; j++)
			{
				AddToOutsideSet(Faces[NewFaces[j]], UnAssignedVertices);
			}
			UnAssignedVertices.ExceptWith(AssignedVertices);
			ClosedVertices.UnionWith(UnAssignedVertices);
		}
		Result = CreateMesh(Faces);
	}

	private int GetFurthestPointFromFace(int faceIndex)
	{
		Face face = Faces[faceIndex];
		float num = float.NegativeInfinity;
		int result = -1;
		foreach (int outsideVertex in face.OutsideVertices)
		{
			float num2 = DistanceFromPlane(VerticesList[outsideVertex], face.Normal, VerticesList[face.V0]);
			if (num2 > num)
			{
				result = outsideVertex;
				num = num2;
			}
		}
		return result;
	}

	private int GetNonEmptyFaceIndex()
	{
		for (int i = 0; i < Faces.Count; i++)
		{
			if (Faces[i].OutsideVertices.Count > 0)
			{
				return i;
			}
		}
		return -1;
	}

	private bool HaveNonEmptyFaceSet()
	{
		foreach (Face face in Faces)
		{
			if (face.OutsideVertices.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	private bool isAGreaterThanB(float a, float b)
	{
		if (a - b > Epsilon)
		{
			return true;
		}
		return false;
	}

	private bool isALessThanB(float a, float b)
	{
		if (b - a > Epsilon)
		{
			return true;
		}
		return false;
	}

	private bool isApproxEqual(float a, float b)
	{
		return Mathf.Abs(a - b) < Epsilon;
	}

	private bool IsApproxZero(float a)
	{
		return Mathf.Abs(a) < Epsilon;
	}

	private void UpdateFace(Horizon horizon, int newFace)
	{
		if (Faces[horizon.Face].OnConvexHull)
		{
			if (Faces[horizon.Face].F0 == horizon.From)
			{
				Faces[horizon.Face].F0 = newFace;
			}
			else if (Faces[horizon.Face].F1 == horizon.From)
			{
				Faces[horizon.Face].F1 = newFace;
			}
			else if (Faces[horizon.Face].F2 == horizon.From)
			{
				Faces[horizon.Face].F2 = newFace;
			}
		}
	}

	private Vector3 CalcFaceCenter(Face face)
	{
		return (VerticesList[face.V0] + VerticesList[face.V1] + VerticesList[face.V2]) / 3f;
	}

	private void DebugInitialPoints(List<Vector3> points, List<int> initialPoints)
	{
		string text = "";
		string text2 = "";
		foreach (int initialPoint in initialPoints)
		{
			text = text + initialPoint + " : ";
			text2 = string.Concat(text2, points[initialPoint], " : ");
		}
	}

	private void DrawFace(int face, Color color, float size = 0.08f)
	{
		Face face2 = Faces[face];
		DrawPoint(VerticesList[face2.V0], color, size);
		DrawPoint(VerticesList[face2.V1], color, size);
		DrawPoint(VerticesList[face2.V2], color, size);
	}

	private void DrawFaceConnections(int face)
	{
		DrawFaceNormal(Faces[Faces[face].F0], Color.red, 1.025f);
		DrawFaceNormal(Faces[Faces[face].F1], Color.green, 1.05f);
		DrawFaceNormal(Faces[Faces[face].F2], Color.blue, 1.075f);
	}

	private void DrawFaceNormal(Face face, Color color, float distance = 1f)
	{
		Vector3 vector = CalcFaceCenter(face);
		Debug.DrawLine(vector, vector + face.Normal * distance, color, DrawTime);
	}

	private void ForceUpdateFace(int faceIndex)
	{
		if (1 == 0)
		{
			return;
		}
		Face face = Faces[faceIndex];
		Face face2 = null;
		for (int i = 0; i < Faces.Count; i++)
		{
			if (faceIndex != i && Faces[i].OnConvexHull)
			{
				face2 = Faces[i];
				if ((face.V0 == face2.V0 || face.V0 == face2.V1 || face.V0 == face2.V2) && (face.V1 == face2.V0 || face.V1 == face2.V1 || face.V1 == face2.V2))
				{
					face.F0 = i;
				}
				else if ((face.V2 == face2.V0 || face.V2 == face2.V1 || face.V2 == face2.V2) && (face.V1 == face2.V0 || face.V1 == face2.V1 || face.V1 == face2.V2))
				{
					face.F1 = i;
				}
				else if ((face.V0 == face2.V0 || face.V0 == face2.V1 || face.V0 == face2.V2) && (face.V2 == face2.V0 || face.V2 == face2.V1 || face.V2 == face2.V2))
				{
					face.F2 = i;
				}
			}
		}
	}

	private Color RandomColor()
	{
		return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
	}

	private void DrawPoint(Vector3 point, Color color, float size = 0.05f)
	{
		Debug.DrawLine(point - Vector3.up * size, point + Vector3.up * size, color, DrawTime);
		Debug.DrawLine(point - Vector3.left * size, point + Vector3.left * size, color, DrawTime);
		Debug.DrawLine(point - Vector3.forward * size, point + Vector3.forward * size, color, DrawTime);
	}
}
