using UnityEngine;

namespace FIMSpace.FOptimizing;

public class FOptimizers_CullingContainer
{
	public const int MaxSlots = 1000;

	internal bool Destroying;

	private int highestIndex;

	private int lastRemovedIndex;

	public int ID { get; private set; }

	public bool HaveFreeSlots => highestIndex < 999;

	public CullingGroup CullingGroup { get; private set; }

	public FOptimizer_Base[] Optimizers { get; private set; }

	public BoundingSphere[] CullingSpheres { get; private set; }

	public int BoundingCount { get; private set; }

	public float[] DistanceLevels { get; private set; }

	public FOptimizers_CullingContainer()
	{
		Optimizers = new FOptimizer_Base[1000];
	}

	public void InitializeContainer(int id, float[] distances, Camera targetCamera)
	{
		ID = id;
		DistanceLevels = new float[distances.Length + 2];
		DistanceLevels[0] = 0.001f;
		for (int i = 1; i < distances.Length + 1; i++)
		{
			DistanceLevels[i] = distances[i - 1];
		}
		DistanceLevels[DistanceLevels.Length - 1] = distances[distances.Length - 1] * 1.5f;
		CullingGroup = new CullingGroup
		{
			targetCamera = targetCamera
		};
		CullingSpheres = new BoundingSphere[1000];
		CullingGroup.SetBoundingSpheres(CullingSpheres);
		BoundingCount = 0;
		highestIndex = -1;
		lastRemovedIndex = -1;
		CullingGroup.SetBoundingSphereCount(BoundingCount);
		CullingGroup.onStateChanged = CullingGroupStateChanged;
		CullingGroup.SetBoundingDistances(DistanceLevels);
		if ((bool)targetCamera)
		{
			CullingGroup.SetDistanceReferencePoint(targetCamera.transform);
		}
	}

	public void SetNewCamera(Camera cam)
	{
		if (!(cam == null))
		{
			CullingGroup.targetCamera = cam;
			CullingGroup.SetDistanceReferencePoint(cam.transform);
		}
	}

	public bool AddOptimizer(FOptimizer_Base optimizer)
	{
		if (!HaveFreeSlots)
		{
			return false;
		}
		int num = highestIndex + 1;
		CullingSpheres[num].position = optimizer.GetReferencePosition();
		CullingSpheres[num].radius = optimizer.DetectionRadius * FOptimizer_Base.GetScaler(optimizer.transform);
		Optimizers[num] = optimizer;
		optimizer.AssignToContainer(this, num, ref CullingSpheres[num]);
		highestIndex++;
		BoundingCount++;
		CullingGroup.SetBoundingSphereCount(BoundingCount);
		return true;
	}

	public void RemoveOptimizer(FOptimizer_Base optimizer)
	{
		if (Optimizers != null)
		{
			lastRemovedIndex = optimizer.ContainerSphereId;
			Optimizers[lastRemovedIndex] = null;
			MoveStackOptimizerToFreeSlot();
		}
	}

	private void MoveStackOptimizerToFreeSlot()
	{
		FOptimizer_Base fOptimizer_Base = Optimizers[highestIndex];
		Optimizers[highestIndex] = null;
		highestIndex--;
		BoundingCount--;
		if (!(fOptimizer_Base == null))
		{
			int num = lastRemovedIndex;
			lastRemovedIndex = highestIndex + 1;
			CullingSpheres[num].position = fOptimizer_Base.GetReferencePosition();
			CullingSpheres[num].radius = fOptimizer_Base.DetectionRadius * FOptimizer_Base.GetScaler(fOptimizer_Base.transform);
			Optimizers[num] = fOptimizer_Base;
			fOptimizer_Base.AssignToContainer(this, num, ref CullingSpheres[num]);
		}
	}

	private void CullingGroupStateChanged(CullingGroupEvent cullingEvent)
	{
		if (Optimizers[cullingEvent.index] != null)
		{
			Optimizers[cullingEvent.index].CullingGroupStateChanged(cullingEvent);
		}
	}

	public void Dispose()
	{
		CullingGroup.Dispose();
		CullingGroup = null;
		Optimizers = null;
	}

	public static int GetId(float[] distances)
	{
		int num = distances.Length * 179;
		for (int i = 0; i < distances.Length; i++)
		{
			num += (int)distances[i] / 2;
		}
		return num;
	}
}
