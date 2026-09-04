using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FIMSpace.FOptimizing;

[AddComponentMenu("FImpossible Creations/Optimizers/Optimizer Wall Detection")]
public class FOptimizer_ObstacleDetection : FOptimizer_Base, IDropHandler, IEventSystemHandler, IFHierarchyIcon
{
	[HideInInspector]
	[Range(-1f, 5f)]
	[Tooltip("Allowing component to do more raycasts to detect obstacles covering it")]
	public int CoveragePrecision = 1;

	[HideInInspector]
	[Range(0f, 1.5f)]
	[Tooltip("If you want to avoid casting some raycasts from below ground")]
	public float CoverageScale = 1f;

	[HideInInspector]
	[Tooltip("Layer mask for raycasts checking obstacles in front of object in direction to camera")]
	public LayerMask CoverageMask = 1;

	[HideInInspector]
	[Tooltip("Draw menu for customized raycasting points")]
	public bool CustomCoveragePoints;

	[HideInInspector]
	public List<Vector3> CoverageOffsets;

	private int currentCoveragePrecision = -1;

	public string EditorIconPath => "FIMSpace/FOptimizing/Optimizers Wall Icon";

	public void OnDrop(PointerEventData data)
	{
	}

	protected override void Start()
	{
		RefreshCoverageOffsets();
		base.Start();
	}

	public override void DynamicLODUpdate(FEOptimizingDistance category, float distance)
	{
		base.DynamicLODUpdate(category, distance);
		if (CoveragePrecision != -1 && !base.OutOfCameraView && !base.OutOfDistance)
		{
			ObstacleCheck();
		}
	}

	private void ObstacleCheck()
	{
		Vector3[] coverageDetectionPoints = GetCoverageDetectionPoints(CoverageOffsets, base.PreviousPosition);
		for (int i = 0; i < coverageDetectionPoints.Length; i++)
		{
			Physics.Linecast(base.TargetCamera.position, coverageDetectionPoints[i], out var hitInfo, CoverageMask, QueryTriggerInteraction.Ignore);
			if (!hitInfo.transform)
			{
				SetHidden(hide: false);
				return;
			}
		}
		SetHidden(hide: true);
	}

	public override void CullingGroupStateChanged(CullingGroupEvent cullingEvent)
	{
		base.CullingGroupStateChanged(cullingEvent);
		if (CullIfNotSee && !base.OutOfCameraView && !base.OutOfDistance && CoveragePrecision > -1)
		{
			ObstacleCheck();
		}
	}

	public override void OnValidate()
	{
		CullIfNotSee = true;
		if (OptimizingMethod == FEOptimizingMethod.Static)
		{
			Debug.LogError(string.Concat("[OPTIMIZERS] ", OptimizingMethod, " method is not supported for FOptimizer_ObstacleDetection component!"));
			OptimizingMethod = FEOptimizingMethod.Effective;
		}
		base.OnValidate();
	}

	private void RefreshCoverageOffsets()
	{
		if (CustomCoveragePoints || currentCoveragePrecision == CoveragePrecision || CoveragePrecision == -1)
		{
			return;
		}
		currentCoveragePrecision = CoveragePrecision;
		CoverageOffsets = new List<Vector3>();
		Vector3[] array = new Vector3[0];
		if (OptimizingMethod == FEOptimizingMethod.Effective)
		{
			if (CoveragePrecision == 0)
			{
				array = new Vector3[1]
				{
					new Vector3(0f, 0f, 1f)
				};
			}
			else if (CoveragePrecision == 4)
			{
				array = new Vector3[13];
				array[0] = new Vector3(0f, 0f, 1f);
				array[1] = new Vector3(-1f, 0f, 0f);
				array[2] = new Vector3(1f, 0f, 0f);
				array[3] = new Vector3(0f, 1f, 0f);
				array[4] = new Vector3(0f, -1f, 0f);
				array[5] = new Vector3(-0.5f, 0.5f, 0.85f);
				array[6] = new Vector3(0.5f, 0.5f, 0.85f);
				array[7] = new Vector3(0.5f, -0.5f, 0.85f);
				array[8] = new Vector3(-0.5f, -0.5f, 0.85f);
				array[9] = new Vector3(0.5f, 0.5f, 0f);
				array[11] = new Vector3(-0.5f, 0.5f, 0f);
				array[10] = new Vector3(-0.5f, -0.5f, 0f);
				array[12] = new Vector3(0.5f, -0.5f, 0f);
			}
			else if (CoveragePrecision == 5)
			{
				array = new Vector3[25];
				array[0] = new Vector3(0f, 0f, 1f);
				array[1] = new Vector3(-1f, 0f, 0f);
				array[2] = new Vector3(1f, 0f, 0f);
				array[3] = new Vector3(0f, 1f, 0f);
				array[4] = new Vector3(0f, -1f, 0f);
				array[5] = new Vector3(-0.5f, 0.5f, 0.85f);
				array[6] = new Vector3(0.5f, 0.5f, 0.85f);
				array[7] = new Vector3(0.5f, -0.5f, 0.85f);
				array[8] = new Vector3(-0.5f, -0.5f, 0.85f);
				array[9] = new Vector3(0.5f, 0.5f, 0f);
				array[11] = new Vector3(-0.5f, 0.5f, 0f);
				array[10] = new Vector3(-0.5f, -0.5f, 0f);
				array[12] = new Vector3(0.5f, -0.5f, 0f);
				for (int i = 13; i < array.Length; i++)
				{
					array[i] = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
				}
			}
			else if (CoveragePrecision == 3)
			{
				array = new Vector3[9]
				{
					new Vector3(0f, 0f, 1f),
					new Vector3(-1f, 0f, 0f),
					new Vector3(1f, 0f, 0f),
					new Vector3(0f, 1f, 0f),
					new Vector3(0f, -1f, 0f),
					new Vector3(-0.7f, 0.7f, 0.85f),
					new Vector3(0.7f, 0.7f, 0.85f),
					new Vector3(0.7f, -0.7f, 0.85f),
					new Vector3(-0.7f, -0.7f, 0.85f)
				};
			}
			else if (CoveragePrecision == 2)
			{
				array = new Vector3[5]
				{
					new Vector3(0f, 0f, 1f),
					new Vector3(-1f, 1f, 0.4f),
					new Vector3(1f, -1f, 0.4f),
					new Vector3(1f, 1f, 0.4f),
					new Vector3(-1f, -1f, 0.4f)
				};
			}
			else if (CoveragePrecision == 1)
			{
				array = new Vector3[4]
				{
					new Vector3(0f, 0f, 1f),
					new Vector3(0f, 0.4f, 0.1f),
					new Vector3(-0.6f, -0.3f, 0.15f),
					new Vector3(0.6f, -0.3f, 0.15f)
				};
			}
		}
		else if (CoveragePrecision == 0)
		{
			array = new Vector3[1]
			{
				new Vector3(0f, 0f, 1f)
			};
		}
		else if (CoveragePrecision == 4)
		{
			array = new Vector3[13]
			{
				new Vector3(0f, 0f, 1f),
				new Vector3(-1f, 1f, 0.4f),
				new Vector3(1f, -1f, 0.4f),
				new Vector3(1f, 1f, 0.4f),
				new Vector3(-1f, -1f, 0.4f),
				new Vector3(-0.7f, 0.4f, 0.85f),
				new Vector3(0.7f, 0.4f, 0.85f),
				new Vector3(0.7f, -0.4f, 0.85f),
				new Vector3(-0.7f, -0.4f, 0.85f),
				new Vector3(-1f, 0f, 0f),
				new Vector3(1f, 0f, 0f),
				new Vector3(0f, 1f, 0f),
				new Vector3(0f, -1f, 0f)
			};
		}
		else if (CoveragePrecision == 5)
		{
			array = new Vector3[25]
			{
				new Vector3(0f, 0f, 1f),
				new Vector3(-1f, 1f, 0.4f),
				new Vector3(1f, -1f, 0.4f),
				new Vector3(1f, 1f, 0.4f),
				new Vector3(-1f, -1f, 0.4f),
				new Vector3(-0.7f, 0.4f, 0.85f),
				new Vector3(0.7f, 0.4f, 0.85f),
				new Vector3(0.7f, -0.4f, 0.85f),
				new Vector3(-0.7f, -0.4f, 0.85f),
				new Vector3(-1f, 0f, 0f),
				new Vector3(1f, 0f, 0f),
				new Vector3(0f, 1f, 0f),
				new Vector3(0f, -1f, 0f),
				default(Vector3),
				default(Vector3),
				default(Vector3),
				default(Vector3),
				default(Vector3),
				default(Vector3),
				default(Vector3),
				default(Vector3),
				default(Vector3),
				default(Vector3),
				default(Vector3),
				default(Vector3)
			};
			for (int j = 13; j < array.Length; j++)
			{
				array[j] = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(0f, 1f));
			}
		}
		else if (CoveragePrecision == 3)
		{
			array = new Vector3[9]
			{
				new Vector3(0f, 0f, 1f),
				new Vector3(-1f, 1f, 0.4f),
				new Vector3(1f, -1f, 0.4f),
				new Vector3(1f, 1f, 0.4f),
				new Vector3(-1f, -1f, 0.4f),
				new Vector3(-0.7f, 0.4f, 0.85f),
				new Vector3(0.7f, 0.4f, 0.85f),
				new Vector3(0.7f, -0.4f, 0.85f),
				new Vector3(-0.7f, -0.4f, 0.85f)
			};
		}
		else if (CoveragePrecision == 2)
		{
			array = new Vector3[5]
			{
				new Vector3(0f, 0f, 1f),
				new Vector3(-1f, 1f, 0.4f),
				new Vector3(1f, -1f, 0.4f),
				new Vector3(1f, 1f, 0.4f),
				new Vector3(-1f, -1f, 0.4f)
			};
		}
		else if (CoveragePrecision == 1)
		{
			array = new Vector3[4]
			{
				new Vector3(0f, 0f, 1f),
				new Vector3(0f, 0.8f, 0.1f),
				new Vector3(-1f, -0.85f, 0.15f),
				new Vector3(1f, -0.85f, 0.15f)
			};
		}
		CoverageOffsets.Clear();
		for (int k = 0; k < array.Length; k++)
		{
			CoverageOffsets.Add(array[k]);
		}
	}

	public Vector3[] GetCoverageDetectionPoints(List<Vector3> coverageOffsets, Vector3 origin)
	{
		Vector3[] array = new Vector3[coverageOffsets.Count];
		float num = CoverageScale * 0.7f;
		if (OptimizingMethod == FEOptimizingMethod.Effective)
		{
			if (CustomCoveragePoints)
			{
				Quaternion quaternion = Quaternion.LookRotation(Camera.main.transform.position - origin);
				for (int i = 0; i < coverageOffsets.Count; i++)
				{
					array[i] = origin;
					array[i] += quaternion * Vector3.Scale(coverageOffsets[i] * num, Vector3.one * DetectionRadius);
				}
			}
			else
			{
				Quaternion quaternion2 = Quaternion.LookRotation(Camera.main.transform.position - origin);
				for (int j = 0; j < coverageOffsets.Count; j++)
				{
					array[j] = origin;
					array[j] += quaternion2 * coverageOffsets[j].normalized * DetectionRadius * num;
				}
			}
		}
		else
		{
			Quaternion quaternion3 = Quaternion.LookRotation(Camera.main.transform.position - origin);
			for (int k = 0; k < coverageOffsets.Count; k++)
			{
				array[k] = origin;
				array[k] += quaternion3 * Vector3.Scale(coverageOffsets[k] * num, DetectionBounds / 2f);
			}
		}
		return array;
	}
}
