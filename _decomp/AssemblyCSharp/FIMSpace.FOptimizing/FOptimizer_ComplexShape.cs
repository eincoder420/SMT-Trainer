using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FIMSpace.FOptimizing;

[AddComponentMenu("FImpossible Creations/Optimizers/Optimizer Complex Shape")]
public class FOptimizer_ComplexShape : FOptimizer_Base, IDropHandler, IEventSystemHandler, IFHierarchyIcon
{
	[Serializable]
	public class FOptComplex_DetectionSphere
	{
		public Vector3 position;

		public float radius = 1f;

		public Transform transform;
	}

	[HideInInspector]
	[Range(0f, 1f)]
	[Tooltip("How many spheres should be created in auto detection process")]
	public float AutoPrecision = 0.25f;

	[HideInInspector]
	[Tooltip("[Optional] Mesh to create detection spheres on it's structure")]
	public Mesh AutoReferenceMesh;

	[HideInInspector]
	public bool DrawPositionHandles = true;

	[HideInInspector]
	public bool ScalingHandles = true;

	[HideInInspector]
	public List<FOptComplex_DetectionSphere> Shapes;

	[HideInInspector]
	public List<Vector3> ShapePos;

	[HideInInspector]
	public List<float> ShapeRadius;

	private int nearestDistanceLevel;

	private int preNearestDistanceLevel;

	private int[] sphereState;

	private int spheresVisible;

	private int[] spheresWithLOD;

	public string EditorIconPath => "FIMSpace/FOptimizing/Optimizers Icon Complex";

	public void OnDrop(PointerEventData data)
	{
	}

	protected override void RefreshInitialSettingsForOptimized()
	{
		base.RefreshInitialSettingsForOptimized();
		AddToContainer = false;
	}

	protected override void InitCullingGroups(float[] distances, float detectionSphereRadius = 2.5f, Camera targetCamera = null)
	{
		if (Shapes != null && Shapes.Count != 0)
		{
			InitBaseCullingVariables(targetCamera);
			base.DistanceLevels = new float[distances.Length + 2];
			base.DistanceLevels[0] = 0.001f;
			for (int i = 1; i < distances.Length + 1; i++)
			{
				base.DistanceLevels[i] = distances[i - 1];
			}
			base.DistanceLevels[base.DistanceLevels.Length - 1] = distances[distances.Length - 1] * 2f;
			distancePoint = base.transform.position;
			base.CullingGroup = new CullingGroup
			{
				targetCamera = targetCamera
			};
			visibilitySpheres = GetBoundingSpheres();
			sphereState = new int[visibilitySpheres.Length];
			mainVisibilitySphere = visibilitySpheres[0];
			for (int j = 0; j < sphereState.Length; j++)
			{
				sphereState[j] = 0;
			}
			spheresWithLOD = new int[LODLevels + 2];
			spheresWithLOD[1] = visibilitySpheres.Length;
			base.CullingGroup.SetBoundingSpheres(visibilitySpheres);
			base.CullingGroup.SetBoundingSphereCount(visibilitySpheres.Length);
			base.CullingGroup.onStateChanged = CullingGroupStateChanged;
			base.CullingGroup.SetBoundingDistances(base.DistanceLevels);
			base.CullingGroup.SetDistanceReferencePoint(targetCamera.transform);
			spheresVisible = 0;
			float[] centerPosAndFarthest = GetCenterPosAndFarthest();
			distancePoint = new Vector3(centerPosAndFarthest[0], centerPosAndFarthest[1], centerPosAndFarthest[2]);
		}
	}

	public override void CullingGroupStateChanged(CullingGroupEvent cullingEvent)
	{
		int num = cullingEvent.currentDistance;
		if (num == 0)
		{
			num = 1;
		}
		if (num >= spheresWithLOD.Length)
		{
			num = spheresWithLOD.Length - 1;
		}
		sphereState[cullingEvent.index] = num;
		int num2 = cullingEvent.previousDistance;
		if (num2 == 0)
		{
			num2 = 1;
		}
		if (num2 >= spheresWithLOD.Length)
		{
			num2 = spheresWithLOD.Length - 1;
		}
		spheresWithLOD[num2]--;
		spheresWithLOD[num]++;
		if (cullingEvent.hasBecomeInvisible)
		{
			spheresVisible--;
		}
		if (cullingEvent.hasBecomeVisible)
		{
			spheresVisible++;
		}
		int num3 = 0;
		for (int num4 = spheresWithLOD.Length - 1; num4 >= 0; num4--)
		{
			if (spheresWithLOD[num4] > 0)
			{
				num3 = num4;
			}
		}
		if (num3 == 0)
		{
			num3 = 1;
		}
		nearestDistanceLevel = num3;
		if (nearestDistanceLevel > base.DistanceLevels.Length - 2)
		{
			base.OutOfDistance = true;
			if (nearestDistanceLevel > base.DistanceLevels.Length - 1)
			{
				base.FarAway = true;
			}
			else
			{
				base.FarAway = false;
			}
		}
		else
		{
			base.OutOfDistance = false;
			base.FarAway = false;
		}
		if (spheresVisible == 0)
		{
			base.OutOfCameraView = true;
		}
		else
		{
			base.OutOfCameraView = false;
		}
		bool flag = false;
		if (preNearestDistanceLevel != nearestDistanceLevel)
		{
			flag = true;
		}
		else if (WasOutOfCameraView != base.OutOfCameraView)
		{
			flag = true;
		}
		else if (WasHidden != base.IsHidden)
		{
			flag = true;
		}
		if (flag)
		{
			RefreshVisibilityState(Mathf.Max(0, nearestDistanceLevel - 1));
			preNearestDistanceLevel = nearestDistanceLevel;
		}
	}

	protected BoundingSphere[] GetBoundingSpheres()
	{
		BoundingSphere[] array = new BoundingSphere[Shapes.Count];
		_ = base.transform;
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		for (int i = 0; i < Shapes.Count; i++)
		{
			if (Shapes[i].transform == null)
			{
				array[i] = new BoundingSphere(localToWorldMatrix.MultiplyPoint(Shapes[i].position), DetectionRadius * Shapes[i].radius);
			}
			else
			{
				array[i] = new BoundingSphere(Shapes[i].transform.localToWorldMatrix.MultiplyPoint(Shapes[i].position), DetectionRadius * Shapes[i].radius);
			}
		}
		return array;
	}

	public override Vector3 GetReferencePosition()
	{
		return distancePoint;
	}

	public override void OnValidate()
	{
		if (OptimizingMethod == FEOptimizingMethod.Dynamic || OptimizingMethod == FEOptimizingMethod.TriggerBased)
		{
			Debug.LogError(string.Concat("[OPTIMIZERS] Optimization Method ", OptimizingMethod, " is not supported by Complex Shape Component!"));
			OptimizingMethod = FEOptimizingMethod.Effective;
		}
		base.OnValidate();
		CullIfNotSee = true;
		Hideable = true;
		if (!AutoReferenceMesh)
		{
			MeshFilter componentInChildren = GetComponentInChildren<MeshFilter>();
			if ((bool)componentInChildren)
			{
				AutoReferenceMesh = componentInChildren.sharedMesh;
			}
			if (!AutoReferenceMesh)
			{
				SkinnedMeshRenderer componentInChildren2 = base.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
				if ((bool)componentInChildren2)
				{
					AutoReferenceMesh = componentInChildren2.sharedMesh;
				}
			}
		}
		if (ShapePos.Count > 0)
		{
			for (int i = 0; i < ShapePos.Count; i++)
			{
				Shapes.Add(new FOptComplex_DetectionSphere());
				Shapes[i].position = ShapePos[i];
				Shapes[i].radius = ShapeRadius[i];
			}
			ShapePos.Clear();
			ShapeRadius.Clear();
		}
	}

	public override void DynamicLODUpdate(FEOptimizingDistance category, float distance)
	{
		base.PreviousPosition = visibilitySpheres[0].position + Vector3.right * moveTreshold * 2f;
		base.DynamicLODUpdate(category, distance);
	}

	protected override void RefreshEffectiveCullingGroups()
	{
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		for (int i = 0; i < Shapes.Count; i++)
		{
			if (Shapes[i].transform == null)
			{
				visibilitySpheres[i].position = localToWorldMatrix.MultiplyPoint(Shapes[i].position);
			}
			else
			{
				visibilitySpheres[i].position = Shapes[i].transform.localToWorldMatrix.MultiplyPoint(Shapes[i].position);
			}
		}
	}

	protected float[] GetCenterPosAndFarthest()
	{
		float[] array = new float[5];
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < visibilitySpheres.Length; i++)
		{
			zero += visibilitySpheres[i].position;
		}
		zero /= (float)Shapes.Count;
		float num = 0f;
		float num2 = 0f;
		for (int j = 0; j < visibilitySpheres.Length; j++)
		{
			float num3 = Vector3.Distance(visibilitySpheres[j].position, zero);
			if (num3 > num)
			{
				num = num3;
			}
			if (visibilitySpheres[j].radius > num2)
			{
				num2 = visibilitySpheres[j].radius;
			}
		}
		array[0] = zero.x;
		array[1] = zero.y;
		array[2] = zero.z;
		array[3] = num;
		array[4] = num2;
		return array;
	}

	public void GenerateAutoShape()
	{
		if ((bool)AutoReferenceMesh)
		{
			List<Vector3> pointsFromMesh = GetPointsFromMesh(AutoReferenceMesh, AutoPrecision);
			Shapes = new List<FOptComplex_DetectionSphere>();
			for (int i = 0; i < pointsFromMesh.Count; i++)
			{
				Shapes.Add(new FOptComplex_DetectionSphere());
				Shapes[i].position = pointsFromMesh[i];
			}
		}
		else
		{
			Debug.LogError("[OPTIMIZERS] No mesh to reference from");
		}
	}

	protected List<Vector3> GetPointsFromMesh(Mesh mesh, float precision)
	{
		try
		{
			List<Vector3> list = new List<Vector3>();
			float num = (DetectionRadius = mesh.bounds.size.magnitude / Mathf.Lerp(2f, 10f, precision));
			list.Add(mesh.vertices[0]);
			for (int i = 0; i < 100; i++)
			{
				float num2 = float.MaxValue;
				int num3 = -1;
				for (int j = 0; j < mesh.vertices.Length; j++)
				{
					bool flag = true;
					for (int k = 0; k < list.Count; k++)
					{
						float num4 = Vector3.Distance(mesh.vertices[j], list[k]);
						if (num4 < num)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						float num4 = Vector3.Distance(mesh.vertices[j], list[i]);
						if (num4 < num2)
						{
							num2 = num4;
							num3 = j;
						}
					}
				}
				if (num3 == -1)
				{
					break;
				}
				list.Add(mesh.vertices[num3]);
			}
			return list;
		}
		catch (Exception)
		{
		}
		return null;
	}
}
