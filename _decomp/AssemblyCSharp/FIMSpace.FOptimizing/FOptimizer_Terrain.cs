using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FIMSpace.FOptimizing;

[AddComponentMenu("FImpossible Creations/Optimizers/Terrain Optimizer")]
public class FOptimizer_Terrain : FOptimizer_Base, IDropHandler, IEventSystemHandler, IFHierarchyIcon
{
	[Tooltip("Target terrain component to be optimized")]
	public Terrain Terrain;

	public TerrainCollider TerrainCollider;

	[Tooltip("If you have island type terrain you can feel free to untoggle this, but if you're using continous terrain it should be toggled on")]
	public bool SafeBorders = true;

	[Range(0f, 3f)]
	[Tooltip("Amount of used raycast for checking distance to terrain if character is high above terrain (very small performance change)")]
	public int CheckQuality = 2;

	private int nearestDistanceLevel;

	private int preNearestDistanceLevel;

	private int[] sphereState;

	private int spheresVisible;

	private int spheresInvisible;

	private int[] spheresWithLOD;

	public string EditorIconPath => "FIMSpace/FOptimizing/Optimizers Icon Terrain";

	public void OnDrop(PointerEventData data)
	{
	}

	protected override void Reset()
	{
		if (ToOptimize == null)
		{
			ToOptimize = new List<FComponentLODsController>();
		}
		AddTerrainToOptimize();
		DrawAutoDistanceToggle = false;
		base.Reset();
		DrawDeactivateToggle = false;
		if ((bool)Terrain)
		{
			DetectionRadius = Terrain.terrainData.size.x / 10f;
		}
		else
		{
			DetectionRadius = 80f;
		}
		if ((bool)Terrain)
		{
			MaxDistance = Terrain.terrainData.size.x * 1.5f;
		}
		else
		{
			MaxDistance = 1000f;
		}
	}

	protected override void RefreshInitialSettingsForOptimized()
	{
		base.RefreshInitialSettingsForOptimized();
		AddToContainer = false;
	}

	protected override void InitCullingGroups(float[] distances, float detectionSphereRadius = 2.5f, Camera targetCamera = null)
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
		mainVisibilitySphere = visibilitySpheres[0];
		sphereState = new int[visibilitySpheres.Length];
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
		spheresInvisible = visibilitySpheres.Length;
		distancePoint = GetTerrainCenter();
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
			spheresInvisible++;
			spheresVisible--;
		}
		if (cullingEvent.hasBecomeVisible)
		{
			spheresInvisible--;
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
			RefreshVisibilityState(nearestDistanceLevel - 1);
			preNearestDistanceLevel = nearestDistanceLevel;
		}
	}

	protected BoundingSphere[] GetBoundingSpheres()
	{
		List<BoundingSphere> list = new List<BoundingSphere>();
		float x = Terrain.terrainData.size.x;
		int num = Mathf.RoundToInt(x / (DetectionRadius * 2f));
		int num2 = Mathf.RoundToInt(Terrain.terrainData.size.z / (DetectionRadius * 2f));
		float num3 = (float)num * (DetectionRadius * 2f) / x;
		num3 = 1f - num3 + 1f;
		num3 *= DetectionRadius * 2f;
		float num4 = num * num2 + num2 + 1;
		float num5 = 0f;
		for (int i = 0; i <= num; i++)
		{
			for (int j = 0; j <= num2; j++)
			{
				Vector3 position = Terrain.GetPosition();
				position += Vector3.right * i * num3 + Vector3.right * DetectionRadius;
				position += Vector3.forward * j * num3 + Vector3.forward * DetectionRadius;
				position.y = Terrain.SampleHeight(position);
				Color color = Color.HSVToRGB(num5 / num4, 0.9f, 0.8f);
				color.a = GizmosAlpha;
				Gizmos.color = color;
				if (j != num2 && i != num)
				{
					list.Add(new BoundingSphere(position, DetectionRadius));
				}
				position -= Vector3.right * DetectionRadius * 1f;
				position -= Vector3.forward * DetectionRadius * 1f;
				position.y = Terrain.SampleHeight(position);
				if (SafeBorders)
				{
					list.Add(new BoundingSphere(position, DetectionRadius));
				}
				else if (j != num2 && i != num && i != 0 && j != 0)
				{
					list.Add(new BoundingSphere(position, DetectionRadius));
				}
				num5 += 1f;
			}
		}
		return list.ToArray();
	}

	public override Vector3 GetReferencePosition()
	{
		return distancePoint;
	}

	private bool IsTargetOutside()
	{
		return false;
	}

	private Vector3 GetNearestPointOnTerrain(Vector3 from)
	{
		return Vector3.zero;
	}

	private void RefreshTerrainComponents()
	{
		if (!Terrain)
		{
			Terrain = GetComponentInChildren<Terrain>();
		}
		if ((bool)Terrain)
		{
			TerrainCollider = Terrain.GetComponent<TerrainCollider>();
		}
	}

	private void AddTerrainToOptimize()
	{
		RefreshTerrainComponents();
		if (ToOptimize.Count == 0)
		{
			TryAddLODControllerFor(LoadLODReference("Optimizers/Base/FLOD_Terrain Reference"), base.gameObject.transform, null);
		}
		else if (ToOptimize[0] == null)
		{
			FComponentLODsController fComponentLODsController = LoadLODReference("Optimizers/Base/FLOD_Terrain Reference").GenerateLODController(base.transform, this);
			if (fComponentLODsController != null)
			{
				ToOptimize[0] = fComponentLODsController;
			}
		}
	}

	private bool HaveTerrain()
	{
		if (!Terrain)
		{
			RefreshTerrainComponents();
			if (!Terrain)
			{
				Debug.LogError("[OPTIMIZERS] No terrain attached to Optimizer component on object " + base.name);
				return false;
			}
			if (!TerrainCollider)
			{
				Debug.LogError("[OPTIMIZERS] Terrain don't have Terrain Collider! (" + base.name + ")");
				return false;
			}
		}
		if (!Terrain)
		{
			return false;
		}
		return true;
	}

	public float LimitRadius(float value)
	{
		if (HaveTerrain() && Terrain.terrainData != null && value < Terrain.terrainData.size.x / 40f)
		{
			value = Terrain.terrainData.size.x / 40f;
		}
		return value;
	}

	private Vector3 GetTerrainCenter()
	{
		return Terrain.GetPosition() + Vector3.right * (Terrain.terrainData.size.x / 2f) + Vector3.forward * (Terrain.terrainData.size.z / 2f);
	}

	private float GetMinRadius()
	{
		return Vector3.Distance(Terrain.GetPosition(), GetTerrainCenter()) + (SafeBorders ? DetectionRadius : 0f);
	}

	public override void OnValidate()
	{
		if (ToOptimize == null)
		{
			ToOptimize = new List<FComponentLODsController>();
		}
		AddTerrainToOptimize();
		DrawAutoDistanceToggle = false;
		CullIfNotSee = true;
		Hideable = true;
		HiddenCullAt = -1;
		LimitLODLevels = 4;
		DeactivateObject = false;
		DrawDeactivateToggle = false;
		base.OnValidate();
		DetectionRadius = LimitRadius(DetectionRadius);
	}

	private void DrawTerrainSphere(float radius, float inRadius = 0f)
	{
		int num = Mathf.RoundToInt(Terrain.terrainData.size.x / (DetectionRadius * 2f));
		int num2 = Mathf.RoundToInt(Terrain.terrainData.size.z / (DetectionRadius * 2f));
		Vector3 normalized = new Vector3(-1f, 0f, 1f).normalized;
		Vector3 normalized2 = new Vector3(1f, 0f, 1f).normalized;
		float num3 = 0f;
		if (SafeBorders)
		{
			num3 += DetectionRadius;
			num++;
			num2++;
			num /= 2;
			num2 /= 2;
		}
		List<Vector3> list = new List<Vector3>();
		list.Add(Terrain.GetPosition() + Vector3.left * num3 + Vector3.left * radius);
		list.Add(Terrain.GetPosition() + Vector3.left * num3 + Vector3.left * radius + Vector3.forward * Terrain.terrainData.size.z);
		list.Add(Terrain.GetPosition() + Vector3.forward * Terrain.terrainData.size.z + normalized * num3 + normalized * radius);
		list.Add(Terrain.GetPosition() + Vector3.forward * num3 + Vector3.forward * radius + Vector3.forward * Terrain.terrainData.size.z);
		list.Add(Terrain.GetPosition() + Vector3.forward * num3 + Vector3.forward * radius + Vector3.forward * Terrain.terrainData.size.z + Vector3.right * Terrain.terrainData.size.x);
		list.Add(Terrain.GetPosition() + Vector3.forward * Terrain.terrainData.size.z + normalized2 * num3 + normalized2 * radius + Vector3.right * Terrain.terrainData.size.x);
		list.Add(Terrain.GetPosition() + Vector3.right * num3 + Vector3.right * radius + Vector3.right * Terrain.terrainData.size.x + Vector3.forward * Terrain.terrainData.size.z);
		list.Add(Terrain.GetPosition() + Vector3.right * num3 + Vector3.right * radius + Vector3.right * Terrain.terrainData.size.x);
		list.Add(Terrain.GetPosition() - normalized * num3 - normalized * radius + Vector3.right * Terrain.terrainData.size.x);
		list.Add(Terrain.GetPosition() + Vector3.back * num3 + Vector3.back * radius + Vector3.right * Terrain.terrainData.size.x);
		list.Add(Terrain.GetPosition() + Vector3.back * num3 + Vector3.back * radius);
		list.Add(Terrain.GetPosition() - normalized2 * num3 - normalized2 * radius);
		DrawVertices(list, Vector3.zero);
		if (inRadius != 0f)
		{
			float num4 = Terrain.terrainData.size.z / (float)num2;
			Vector3 vector = Terrain.GetPosition() + Vector3.left * num3;
			for (int i = 0; i <= num2; i++)
			{
				Gizmos.DrawRay(vector + Vector3.forward * num4 * i + Vector3.left * radius, Vector3.left * inRadius);
			}
			vector = Terrain.GetPosition() + Vector3.right * (Terrain.terrainData.size.x + num3);
			for (int j = 0; j <= num2; j++)
			{
				Gizmos.DrawRay(vector + Vector3.forward * num4 * j + Vector3.right * radius, Vector3.right * inRadius);
			}
			num4 = Terrain.terrainData.size.x / (float)num;
			vector = Terrain.GetPosition() + Vector3.forward * (Terrain.terrainData.size.z + num3);
			for (int k = 0; k <= num; k++)
			{
				Gizmos.DrawRay(vector + Vector3.right * num4 * k + Vector3.forward * radius, Vector3.forward * inRadius);
			}
			vector = Terrain.GetPosition() + Vector3.back * num3;
			for (int l = 0; l <= num; l++)
			{
				Gizmos.DrawRay(vector + Vector3.right * num4 * l + Vector3.back * radius, Vector3.back * inRadius);
			}
			Gizmos.DrawRay(Terrain.GetPosition() - normalized2 * num3 - normalized2 * radius, normalized2 * (0f - inRadius));
			Gizmos.DrawRay(Terrain.GetPosition() + Vector3.forward * Terrain.terrainData.size.z + normalized * num3 + normalized * radius, normalized * inRadius);
			Gizmos.DrawRay(Terrain.GetPosition() + Vector3.forward * Terrain.terrainData.size.z + normalized2 * num3 + normalized2 * radius + Vector3.right * Terrain.terrainData.size.x, normalized2 * inRadius);
			Gizmos.DrawRay(Terrain.GetPosition() - normalized * num3 - normalized * radius + Vector3.right * Terrain.terrainData.size.x, normalized * (0f - inRadius));
			Color color = Gizmos.color;
			Gizmos.color = color * new Color(1f, 1f, 1f, 0.25f);
			DrawVertices(list, Vector3.up * radius + Vector3.up * num3 + Vector3.up * DetectionRadius);
			DrawVertices(list, Vector3.down * radius + Vector3.down * num3 + Vector3.down * DetectionRadius);
			DrawVerticesVert(list, Vector3.down * radius + Vector3.down * num3 + Vector3.down * DetectionRadius);
			Gizmos.color = color;
		}
	}

	private void DrawVertices(List<Vector3> v, Vector3 offset, bool vert = false)
	{
		for (int i = 1; i < v.Count; i++)
		{
			Gizmos.DrawLine(v[i - 1] + offset, v[i] + offset);
			if (vert)
			{
				Gizmos.DrawRay(v[i - 1] + offset + Vector3.down * (MaxDistance + DetectionRadius), Vector3.up * (MaxDistance + DetectionRadius) * 2f);
			}
		}
		Gizmos.DrawLine(v[v.Count - 1] + offset, v[0] + offset);
	}

	private void DrawVerticesVert(List<Vector3> v, Vector3 offset)
	{
		for (int i = 1; i < v.Count; i++)
		{
			Gizmos.DrawRay(v[i - 1] + offset, -offset * 2f);
		}
		Gizmos.DrawRay(v[v.Count - 1] + offset, -offset * 2f);
	}
}
