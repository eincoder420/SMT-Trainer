using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing;

public abstract class FOptimizer_Base : MonoBehaviour
{
	[HideInInspector]
	public List<FComponentLODsController> ToOptimize;

	[Range(1f, 8f)]
	[Tooltip("Level of detail (LOD) steps to configure optimization levels")]
	public int LODLevels = 2;

	[SerializeField]
	[HideInInspector]
	protected int preLODLevels = 1;

	[Tooltip("Max distance from main camera.\nWhen exceed object will be culled")]
	public float MaxDistance = 100f;

	[Tooltip("[Static] - For models which aren't moving far from initial position or just stays in one place (method is using only CullingGroups - Very Effective for 'Cull if not see')\n\n[Dynamic] - For objects which are moving in scene's world. If object is moving very fast, use 'UpdateBost' slider in Optimizers Manager but using EFFECTIVE method more recommended in such situtation. Dynamic method can response with some delay when there are thousands of active objects to optimize.\n\n[EFFECTIVE] - Connecting features of static method and dynamic, the most resposible method when you have very mobile objects and you need quick detection if object is seen by camera\n\n[Trigger Based] Using trigger colliders to define distance levels (experimental)")]
	public FEOptimizingMethod OptimizingMethod = FEOptimizingMethod.Effective;

	[FPD_DrawTexture("FIMSpace/FOptimizing/Opt_CullHelp", 128f, 20f, 120f, 165f)]
	[Tooltip("[Toggled] Changing LOD state to cull (or hidden) if camera is looking away from detection sphere/bounds\n\n[Untoggled] Only max distance will cull this object")]
	public bool CullIfNotSee = true;

	[Space(2f)]
	[FPD_Indent(1, 138, 5)]
	[Tooltip("CullIfNotSee: Radius of detecting object visibility for camera view (frustum - CullingGroups)")]
	public float DetectionRadius = 3f;

	[Space(2f)]
	[FPD_Indent(1, 138, 5)]
	[Tooltip("CullIfNotSee: Bounding Box for detecting object visibility for camera view (frustum)")]
	public Vector3 DetectionBounds = Vector3.one;

	[HideInInspector]
	public bool Hideable;

	[FD_HR(1, 10, 0.5f, 0.5f, 0.5f, 0.5f)]
	[Tooltip("Offsetting center of detection sphere/bounds")]
	public Vector3 DetectionOffset = Vector3.zero;

	[Range(0f, 1f)]
	[Tooltip("Alpha for debug spheres etc. visible in scene view when object with Optimizer is selected and Optimizer is unfolded")]
	public float GizmosAlpha = 1f;

	[Range(0f, 3f)]
	[Tooltip("How long (in seconds) should take transition between LOD levels (if transitioning for optimized component is supported)")]
	public float FadeDuration;

	[Tooltip("Displaying options to assign shared settings files to components LODs.\n(Untoggling will not disable using shared settings, just viewing them)")]
	public bool DrawSharedSettingsOptions;

	[Tooltip("If at 'Culled' LOD state game object should be deactivated (after transition)\n\nWARNING: Deactivating whole game object is highly time comsuming for unity when you do it on multiple objects during one game frame\nif you use optimizers on many objects and experience lags during rotating camera then try not deactivating game object but just components inside 'To Optimize' list!")]
	public bool DeactivateObject;

	[HideInInspector]
	public Vector2 MinMaxDistance = new Vector2(0f, 1000f);

	[HideInInspector]
	public List<float> LODPercent;

	protected Vector3 distancePoint = Vector3.zero;

	[HideInInspector]
	public bool AutoDistance;

	[HideInInspector]
	public bool DrawAutoDistanceToggle = true;

	[HideInInspector]
	public int HiddenCullAt = -1;

	[HideInInspector]
	public int LimitLODLevels;

	protected bool drawDetectionSphere = true;

	protected float moveTreshold;

	[HideInInspector]
	public bool UnlockFirstLOD;

	protected bool WasOutOfCameraView;

	protected bool WasHidden;

	protected bool doFirstCull = true;

	[HideInInspector]
	public bool DrawGeneratedPrefabInfo;

	[HideInInspector]
	public bool DrawDeactivateToggle = true;

	[Tooltip("Adding optimizer to culling container - when used a lot of objects with same distance levels and LOD levels count it can boost performance a lot.")]
	public bool AddToContainer = true;

	protected BoundingSphere[] visibilitySpheres;

	protected BoundingSphere mainVisibilitySphere;

	protected CullingGroupEvent lastEvent;

	private Bounds optimizerBounds;

	private float lastDynamicDistance;

	private bool isQuitting;

	internal bool Editor_WasSaving;

	[HideInInspector]
	public bool Editor_InIsolatedScene;

	[HideInInspector]
	public bool Editor_JustCreated = true;

	protected bool wasDisabled;

	internal bool WasAskingForStatic;

	public static readonly Color[] lODColors = new Color[8]
	{
		new Color(0.2231376f, 0.8011768f, 0.1619608f, 1f),
		new Color(0.2070592f, 0.6333336f, 0.7556864f, 1f),
		new Color(0.159216f, 0.5578432f, 0.3435296f, 1f),
		new Color(0.1333336f, 0.4f, 0.7982352f, 1f),
		new Color(0.3827448f, 0.2886272f, 0.5239216f, 1f),
		new Color(0.8f, 0.4423528f, 0f, 1f),
		new Color(0.4886272f, 0.1078432f, 0.80196f, 1f),
		new Color(0.7749016f, 0.6368624f, 0.0250984f, 1f)
	};

	public static readonly Color culledLODColor = new Color(0.4f, 0f, 0f, 0.5f);

	protected int isSelected = -1;

	protected int isResizing = -1;

	private Transform triggersContainer;

	[HideInInspector]
	[Tooltip("Layer for triggers container to detect intersections only with Camera layer\n(camera and containers can have the same layer but change collision matrix)")]
	public LayerMask OnlyCamCollLayer;

	protected int triggerDistanceState = -1;

	protected int preTriggerDistanceState = -1;

	protected List<int> triggersEntered;

	public bool OutOfDistance { get; protected set; }

	public bool OutOfCameraView { get; protected set; }

	public float[] DistanceLevels { get; protected set; }

	public int CurrentLODLevel { get; protected set; }

	public int CurrentDistanceLODLevel { get; protected set; }

	public bool IsCulled { get; protected set; }

	public bool IsHidden { get; protected set; }

	public bool FarAway { get; protected set; }

	public Transform TargetCamera { get; protected set; }

	public int TransitionNextLOD { get; internal set; }

	public float TransitionPercent { get; internal set; }

	public int ContainerGeneratedID { get; private set; }

	public FOptimizers_CullingContainer OwnerContainer { get; private set; }

	public int ContainerSphereId { get; private set; }

	public CullingGroup CullingGroup { get; protected set; }

	public float CullSize { get; protected set; }

	public FEOptimizingDistance? CurrentDynamicDistanceCategory { get; protected set; }

	public int DynamicListIndex { get; protected set; }

	public Vector3 PreviousPosition { get; protected set; }

	public Vector3 LastDynamicCheckCameraPosition { get; protected set; }

	public Vector3 LastTresholdCheckPos { get; protected set; }

	public Vector3 LastTresholdCheckCamPos { get; protected set; }

	public Quaternion LastTresholdCheckCamRot { get; protected set; }

	protected FOptimizers_Manager manager { get; private set; }

	protected virtual void Start()
	{
		bool flag = false;
		for (int num = ToOptimize.Count - 1; num >= 0; num--)
		{
			if (ToOptimize[num].Component == null)
			{
				ToOptimize.RemoveAt(num);
				flag = true;
			}
		}
		if (flag)
		{
			Debug.LogWarning("[OPTIMIZERS] Optimizer had saved objects to optimize which are not existing anymore!");
		}
		StartVariablesRefresh();
		RefreshInitialSettingsForOptimized();
		switch (OptimizingMethod)
		{
		case FEOptimizingMethod.Static:
			InitStaticOptimizer();
			break;
		case FEOptimizingMethod.Dynamic:
			InitDynamicOptimizer(justDynamic: true);
			break;
		case FEOptimizingMethod.Effective:
			InitEffectiveOptimizer();
			break;
		case FEOptimizingMethod.TriggerBased:
			InitTriggerOptimizer();
			break;
		}
		moveTreshold = DetectionRadius * base.transform.lossyScale.x / 100f;
		if ((bool)FOptimizers_Manager.Get)
		{
			moveTreshold *= 1f - FOptimizers_Manager.Get.UpdateBoost * 0.999f;
		}
	}

	protected virtual void StartVariablesRefresh()
	{
		manager = null;
		CurrentDynamicDistanceCategory = null;
		DynamicListIndex = 0;
		TransitionNextLOD = 0;
		TransitionPercent = -1f;
		ContainerGeneratedID = FOptimizers_CullingContainer.GetId(GetDistanceMeasures());
		IsCulled = false;
		IsHidden = false;
	}

	public virtual float[] GetDistanceMeasures()
	{
		EditorResetLODValues();
		float[] array = new float[LODPercent.Count];
		for (int i = 0; i < LODPercent.Count; i++)
		{
			array[i] = Mathf.Lerp(MinMaxDistance.x, MinMaxDistance.y, LODPercent[i]);
		}
		return array;
	}

	protected virtual void InitBaseCullingVariables(Camera targetCamera)
	{
		OutOfDistance = true;
		OutOfCameraView = true;
		WasOutOfCameraView = false;
		IsHidden = false;
		WasHidden = false;
		CurrentLODLevel = 0;
		CurrentDistanceLODLevel = 0;
		if (targetCamera == null)
		{
			targetCamera = Camera.main;
		}
		if (targetCamera == null)
		{
			if (FEditor_OneShotLog.CanDrawLog("optC", 16))
			{
				Debug.LogWarning("[OPTIMIZERS] There is no main camera on scene!");
			}
		}
		else
		{
			TargetCamera = targetCamera.transform;
		}
	}

	protected void RefreshVisibilityState(int targetLODLevel)
	{
		if (!base.enabled)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		CurrentDistanceLODLevel = targetLODLevel;
		if (OutOfDistance)
		{
			flag = true;
		}
		else
		{
			if (CullIfNotSee && OutOfCameraView)
			{
				flag2 = true;
			}
			if (!flag2 && IsHidden)
			{
				flag2 = true;
			}
			if (flag2)
			{
				if (HiddenCullAt < 0)
				{
					flag = true;
				}
				else if (targetLODLevel < HiddenCullAt + 1)
				{
					targetLODLevel = LODLevels + 1;
					flag3 = true;
				}
				else
				{
					flag = true;
				}
			}
			else if (WasOutOfCameraView)
			{
				flag2 = true;
			}
		}
		if (!flag && !IsHidden && WasHidden)
		{
			flag2 = true;
		}
		if (flag2 && TransitionPercent >= 0f)
		{
			FOptimizers_Manager.Get.EndTransition(this);
		}
		if (!(IsCulled && flag))
		{
			if (doFirstCull)
			{
				if (flag)
				{
					ChangeLODLevelTo(LODLevels);
				}
				else
				{
					ChangeLODLevelTo(targetLODLevel);
				}
				doFirstCull = false;
			}
			else if (CullIfNotSee)
			{
				if (flag2)
				{
					if (flag)
					{
						SetCulled();
					}
					else
					{
						if (TransitionPercent < 0f || flag3)
						{
							ChangeLODLevelTo(targetLODLevel);
						}
						if (!OutOfDistance)
						{
							SetCulled(culled: false);
						}
					}
				}
				else if (flag)
				{
					if (FadeDuration > 0f)
					{
						if (!OutOfDistance)
						{
							TransitionOrSetLODLevel(targetLODLevel);
						}
						else
						{
							TransitionOrSetLODLevel(LODLevels);
						}
					}
					else
					{
						TransitionOrSetLODLevel(LODLevels);
					}
				}
				else if (FadeDuration <= 0f)
				{
					SetLODLevel(targetLODLevel);
					SetCulled(culled: false);
				}
				else
				{
					TransitionOrSetLODLevel(targetLODLevel);
					SetCulled(culled: false, apply: false);
				}
			}
			else if (flag)
			{
				TransitionOrSetLODLevel(LODLevels);
			}
			else
			{
				TransitionOrSetLODLevel(targetLODLevel);
				SetCulled(culled: false);
			}
		}
		WasOutOfCameraView = OutOfCameraView;
		WasHidden = IsHidden;
	}

	protected virtual void TransitionOrSetLODLevel(int lodLevel)
	{
		if (FadeDuration <= 0f)
		{
			SetLODLevel(lodLevel);
		}
		else if (lodLevel != CurrentLODLevel || IsCulled || TransitionPercent != -1f)
		{
			if (lodLevel > LODLevels)
			{
				FOptimizers_Manager.Get.TransitionTo(this, LODLevels, FadeDuration);
			}
			else
			{
				FOptimizers_Manager.Get.TransitionTo(this, lodLevel, FadeDuration);
			}
		}
	}

	public void SetHidden(bool hide)
	{
		if (hide != IsHidden)
		{
			IsHidden = hide;
			RefreshVisibilityState(CurrentDistanceLODLevel);
		}
	}

	internal virtual void SetCulled(bool culled = true, bool apply = true)
	{
		if (culled && IsCulled == culled)
		{
			return;
		}
		IsCulled = culled;
		if (culled)
		{
			for (int i = 0; i < ToOptimize.Count; i++)
			{
				ToOptimize[i].ApplyLODLevelSettings(ToOptimize[i].GetCullingLOD());
			}
			if (DeactivateObject)
			{
				OnActivationChange(active: false);
				base.gameObject.SetActive(value: false);
			}
			return;
		}
		if (DeactivateObject && !base.gameObject.activeInHierarchy)
		{
			OnActivationChange(active: true);
			base.gameObject.SetActive(value: true);
		}
		if (apply)
		{
			for (int j = 0; j < ToOptimize.Count; j++)
			{
				ToOptimize[j].ApplyLODLevelSettings(ToOptimize[j].GetCurrentLOD());
			}
		}
	}

	internal virtual void SetLODLevel(int lodLevel)
	{
		if (lodLevel == LODLevels)
		{
			SetCulled();
			return;
		}
		CurrentLODLevel = lodLevel;
		for (int i = 0; i < ToOptimize.Count; i++)
		{
			ToOptimize[i].SetCurrentLODLevel(CurrentLODLevel);
		}
	}

	internal virtual void ChangeLODLevelTo(int lodLevel)
	{
		CurrentLODLevel = Mathf.Min(lodLevel, LODLevels + 2);
		for (int i = 0; i < ToOptimize.Count; i++)
		{
			ToOptimize[i].SetCurrentLODLevel(CurrentLODLevel);
			ToOptimize[i].ApplyLODLevelSettings(ToOptimize[i].GetCurrentLOD());
		}
		bool flag = false;
		if (lodLevel >= LODLevels)
		{
			flag = ((lodLevel != LODLevels + 1) ? true : false);
		}
		if (flag)
		{
			CullOrUncullObject();
		}
		else
		{
			CullOrUncullObject(cull: false);
		}
	}

	internal virtual void CullOrUncullObject(bool cull = true)
	{
		if (IsCulled == cull)
		{
			return;
		}
		IsCulled = cull;
		if (cull)
		{
			if (DeactivateObject && base.gameObject.activeInHierarchy)
			{
				OnActivationChange(active: false);
				base.gameObject.SetActive(value: false);
			}
		}
		else if (DeactivateObject && !base.gameObject.activeInHierarchy)
		{
			OnActivationChange(active: true);
			base.gameObject.SetActive(value: true);
		}
	}

	public void RefreshCamera(Camera camera)
	{
		if (!(camera == null))
		{
			TargetCamera = camera.transform;
			if (OwnerContainer == null && CullingGroup != null)
			{
				CullingGroup.targetCamera = camera;
				CullingGroup.SetDistanceReferencePoint(TargetCamera);
			}
		}
	}

	public virtual Vector3 GetReferencePosition()
	{
		if (OptimizingMethod == FEOptimizingMethod.Static && visibilitySpheres != null)
		{
			return visibilitySpheres[0].position;
		}
		return base.transform.position + base.transform.TransformVector(DetectionOffset);
	}

	public virtual float GetReferenceDistance()
	{
		if (OptimizingMethod == FEOptimizingMethod.Static || OptimizingMethod == FEOptimizingMethod.Effective)
		{
			float num = Vector3.Distance(GetReferencePosition(), TargetCamera.position);
			if (num < mainVisibilitySphere.radius)
			{
				return 0f;
			}
			return num - mainVisibilitySphere.radius;
		}
		return Vector3.Distance(PreviousPosition, LastDynamicCheckCameraPosition);
	}

	public float GetAddRadius()
	{
		if (OptimizingMethod == FEOptimizingMethod.Static || OptimizingMethod == FEOptimizingMethod.Effective)
		{
			return DetectionRadius * base.transform.lossyScale.x;
		}
		return 0f;
	}

	public virtual void OnValidate()
	{
	}

	protected virtual void Reset()
	{
	}

	internal void AssignToContainer(FOptimizers_CullingContainer container, int sphereId, ref BoundingSphere sphere)
	{
		OwnerContainer = container;
		ContainerSphereId = sphereId;
		mainVisibilitySphere = sphere;
	}

	protected void InitStaticOptimizer()
	{
		if (!AddToContainer)
		{
			FOptimizers_Manager.Get.RegisterNotContainedStaticOptimizer(this, init: true);
		}
		InitCullingGroups(GetDistanceMeasures(), DetectionRadius, FOptimizers_Manager.MainCamera);
	}

	protected virtual void InitCullingGroups(float[] distances, float detectionSphereRadius = 2.5f, Camera targetCamera = null)
	{
		InitBaseCullingVariables(targetCamera);
		if (!AddToContainer)
		{
			SetDistanceLevels(distances);
			CullingGroup = new CullingGroup
			{
				targetCamera = targetCamera
			};
			visibilitySpheres = new BoundingSphere[1];
			visibilitySpheres[0] = new BoundingSphere(base.transform.position + base.transform.TransformVector(DetectionOffset), detectionSphereRadius * GetScaler(base.transform));
			mainVisibilitySphere = visibilitySpheres[0];
			CullingGroup.SetBoundingSpheres(visibilitySpheres);
			CullingGroup.SetBoundingSphereCount(1);
			CullingGroup.onStateChanged = CullingGroupStateChanged;
			CullingGroup.SetBoundingDistances(DistanceLevels);
			if ((bool)targetCamera)
			{
				CullingGroup.SetDistanceReferencePoint(targetCamera.transform);
			}
		}
		else
		{
			SetDistanceLevels(distances);
			FOptimizers_Manager.Get.AddToContainer(this);
		}
		distancePoint = GetReferencePosition();
		PreviousPosition = distancePoint;
	}

	public virtual void CullingGroupStateChanged(CullingGroupEvent cullingEvent)
	{
		lastEvent = cullingEvent;
		if (!base.enabled)
		{
			wasDisabled = true;
			return;
		}
		int num = cullingEvent.currentDistance;
		if (num == 0)
		{
			num = 1;
		}
		int num2 = cullingEvent.previousDistance;
		if (num2 == 0)
		{
			num2 = 1;
		}
		if (num > DistanceLevels.Length - 2)
		{
			OutOfDistance = true;
			if (num > DistanceLevels.Length - 1)
			{
				FarAway = true;
			}
			else
			{
				FarAway = false;
			}
		}
		else
		{
			OutOfDistance = false;
			FarAway = false;
		}
		if (CullIfNotSee)
		{
			bool flag = false;
			if (num2 == DistanceLevels.Length - 2 && num == DistanceLevels.Length - 1)
			{
				flag = true;
			}
			if (cullingEvent.hasBecomeVisible)
			{
				OutOfCameraView = false;
			}
			else if (cullingEvent.hasBecomeInvisible && !flag)
			{
				OutOfCameraView = true;
			}
		}
		else if (cullingEvent.hasBecomeVisible)
		{
			OutOfCameraView = false;
		}
		else if (cullingEvent.hasBecomeInvisible)
		{
			OutOfCameraView = true;
		}
		bool flag2 = false;
		int num3 = num - 1;
		if (num3 != CurrentDistanceLODLevel)
		{
			flag2 = true;
		}
		else if (WasOutOfCameraView != OutOfCameraView)
		{
			flag2 = true;
		}
		else if (WasHidden != IsHidden)
		{
			flag2 = true;
		}
		if (!doFirstCull)
		{
			if (flag2)
			{
				RefreshVisibilityState(num3);
			}
		}
		else
		{
			RefreshVisibilityState(num3);
		}
		distancePoint = GetReferencePosition();
	}

	private void SetDistanceLevels(float[] distances)
	{
		DistanceLevels = new float[distances.Length + 2];
		DistanceLevels[0] = 0.001f;
		for (int i = 1; i < distances.Length + 1; i++)
		{
			DistanceLevels[i] = distances[i - 1];
		}
		DistanceLevels[DistanceLevels.Length - 1] = distances[distances.Length - 1] * 1.5f;
	}

	protected void CleanCullingGroup()
	{
		if (CullingGroup != null)
		{
			CullingGroup.Dispose();
			CullingGroup = null;
		}
		if (OwnerContainer != null)
		{
			OwnerContainer.RemoveOptimizer(this);
		}
	}

	public static float GetScaler(Transform transform)
	{
		float num = 1f;
		if (transform.lossyScale.x > transform.lossyScale.y)
		{
			if (transform.lossyScale.y > transform.lossyScale.z)
			{
				return transform.lossyScale.y;
			}
			return transform.lossyScale.z;
		}
		return transform.lossyScale.x;
	}

	private void InitDynamicOptimizer(bool justDynamic)
	{
		PreviousPosition = GetReferencePosition();
		if (manager == null)
		{
			manager = FOptimizers_Manager.Get;
			if ((bool)FOptimizers_Manager.MainCamera)
			{
				TargetCamera = FOptimizers_Manager.MainCamera.transform;
			}
		}
		if (justDynamic)
		{
			FOptimizers_Manager.Get.RegisterNotContainedDynamicOptimizer(this, init: true);
		}
		if ((bool)TargetCamera)
		{
			LastTresholdCheckPos = base.transform.position + Vector3.forward * 100f;
			LastTresholdCheckCamPos = TargetCamera.position + Vector3.forward * 100f;
			LastTresholdCheckCamRot = TargetCamera.rotation * Quaternion.Euler(180f, 0f, 0f);
		}
		DynamicListIndex = manager.AddToDynamic(this);
		if (OptimizingMethod != FEOptimizingMethod.Effective)
		{
			optimizerBounds = new Bounds(GetReferencePosition(), Vector3.Scale(DetectionBounds, base.transform.lossyScale));
		}
	}

	private void RefreshDistances()
	{
		float[] distanceMeasures = GetDistanceMeasures();
		DistanceLevels = new float[distanceMeasures.Length];
		for (int i = 0; i < distanceMeasures.Length; i++)
		{
			DistanceLevels[i] = distanceMeasures[i];
		}
	}

	private void DisposeDynamicOptimizer()
	{
		if (!isQuitting && (bool)manager)
		{
			manager.RemoveFromDynamic(this);
		}
	}

	public virtual void DynamicLODUpdate(FEOptimizingDistance category, float distance)
	{
		lastDynamicDistance = distance;
		CurrentDynamicDistanceCategory = category;
		if (!base.enabled)
		{
			wasDisabled = true;
			return;
		}
		Vector3 referencePosition = GetReferencePosition();
		int lODForDistance = GetLODForDistance(distance);
		if (OptimizingMethod == FEOptimizingMethod.Dynamic)
		{
			if (distance > DistanceLevels[DistanceLevels.Length - 1])
			{
				OutOfDistance = true;
				FarAway = true;
			}
			else
			{
				OutOfDistance = false;
				FarAway = false;
			}
			if (CullIfNotSee)
			{
				optimizerBounds.center = referencePosition;
				OutOfCameraView = !GeometryUtility.TestPlanesAABB(manager.CurrentFrustumPlanes, optimizerBounds);
			}
			else
			{
				OutOfCameraView = false;
			}
			bool flag = false;
			if (lODForDistance != CurrentDistanceLODLevel)
			{
				flag = true;
			}
			else if (WasOutOfCameraView != OutOfCameraView)
			{
				flag = true;
			}
			else if (WasHidden != IsHidden)
			{
				flag = true;
			}
			if (flag)
			{
				RefreshVisibilityState(lODForDistance);
			}
		}
		else if (OptimizingMethod == FEOptimizingMethod.Effective)
		{
			EffectiveLODUpdate();
		}
		else
		{
			TriggerLODUpdate();
		}
		PreviousPosition = referencePosition;
		LastDynamicCheckCameraPosition = TargetCamera.position;
		distancePoint = PreviousPosition;
	}

	private int GetLODForDistance(float distance)
	{
		if (DistanceLevels == null)
		{
			Debug.LogWarning("[OPTIMIZERS] There was something wrong with distance ranges of this object (" + base.name + ")");
			RefreshDistances();
		}
		for (int i = 0; i < DistanceLevels.Length; i++)
		{
			if (distance < DistanceLevels[i])
			{
				return i;
			}
		}
		return LODLevels;
	}

	internal bool TresholdTrigger()
	{
		bool num = manager.CameraMoved(LastTresholdCheckCamPos, LastTresholdCheckCamRot);
		LastTresholdCheckCamPos = TargetCamera.position;
		LastTresholdCheckCamRot = TargetCamera.rotation;
		if (num)
		{
			LastTresholdCheckPos = base.transform.position;
			return true;
		}
		float magnitude = (LastTresholdCheckPos - base.transform.position).magnitude;
		LastTresholdCheckPos = base.transform.position;
		if (magnitude >= manager.MoveTreshold)
		{
			return true;
		}
		LastTresholdCheckPos = base.transform.position;
		return false;
	}

	public virtual void AssignComponentsToOptimizeFrom(Component target)
	{
	}

	protected void TryAddLODControllerFor(FLOD_Base lod, Component target, List<FOptimizer_Base> childOptims)
	{
	}

	public bool CheckIfAlreadyInUse(FComponentLODsController generatedController, List<FOptimizer_Base> childOptims)
	{
		bool flag = false;
		if (childOptims != null)
		{
			for (int i = 0; i < childOptims.Count; i++)
			{
				if (flag)
				{
					break;
				}
				if (!(childOptims[i] != this) || childOptims[i].ToOptimize == null)
				{
					continue;
				}
				for (int j = 0; j < childOptims[i].ToOptimize.Count; j++)
				{
					if (childOptims[i].ToOptimize[j].Component == generatedController.Component)
					{
						flag = true;
						break;
					}
				}
			}
		}
		return flag;
	}

	public virtual void AssignCustomComponentToOptimize(MonoBehaviour target)
	{
	}

	public FLOD_Base LoadLODReference(string resourcesPath)
	{
		FLOD_Base fLOD_Base = Resources.Load<FLOD_Base>(resourcesPath);
		if (fLOD_Base == null)
		{
			Debug.LogError("[OPTIMIZERS CRITICAL ERROR] There are no references for base LOD Types, you removed them from resources folder???");
		}
		return fLOD_Base;
	}

	public virtual void AssignComponentsToBeOptimizedFromAllChildren(GameObject target, bool searchForCustom = false)
	{
		RefreshToOptimizeList();
		Transform[] componentsInChildren;
		if (!searchForCustom)
		{
			componentsInChildren = target.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform target2 in componentsInChildren)
			{
				AssignComponentsToOptimizeFrom(target2);
			}
			return;
		}
		componentsInChildren = target.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MonoBehaviour[] components = componentsInChildren[i].gameObject.GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour target3 in components)
			{
				AssignCustomComponentToOptimize(target3);
			}
		}
	}

	public bool ContainsComponent(Component component)
	{
		for (int num = ToOptimize.Count - 1; num >= 0; num--)
		{
			if (ToOptimize == null)
			{
				ToOptimize.RemoveAt(num);
			}
			else if (ToOptimize[num].Component == component)
			{
				return true;
			}
		}
		return false;
	}

	public void RefreshToOptimizeList()
	{
		for (int num = ToOptimize.Count - 1; num >= 0; num--)
		{
			if (ToOptimize[num] == null)
			{
				ToOptimize.RemoveAt(num);
			}
		}
	}

	public bool IsPrefabed()
	{
		return false;
	}

	protected virtual void RefreshInitialSettingsForOptimized()
	{
		RefreshDistances();
		for (int num = ToOptimize.Count - 1; num >= 0; num--)
		{
			if (ToOptimize == null)
			{
				ToOptimize.RemoveAt(num);
			}
			else
			{
				ToOptimize[num].OnStart();
			}
		}
	}

	public void RemoveFromToOptimizeAt(int i)
	{
	}

	public void RemoveAllComponentsFromToOptimize()
	{
	}

	private FComponentLODsController AddToOptimize(FComponentLODsController lod)
	{
		return null;
	}

	protected virtual void ResetLODs()
	{
	}

	protected virtual void OnActivationChange(bool active)
	{
		if (OptimizingMethod != FEOptimizingMethod.TriggerBased)
		{
			return;
		}
		if (!active)
		{
			if (triggersContainer.transform.parent != null)
			{
				triggersContainer.transform.SetParent(null, worldPositionStays: true);
			}
		}
		else if (triggersContainer.transform.parent == null)
		{
			triggersContainer.transform.SetParent(base.transform, worldPositionStays: true);
		}
	}

	public void CheckForNullsToOptimize()
	{
		if (ToOptimize == null)
		{
			return;
		}
		for (int num = ToOptimize.Count - 1; num >= 0; num--)
		{
			if (ToOptimize[num] == null)
			{
				ToOptimize.RemoveAt(num);
			}
			else if (ToOptimize[num].Component == null)
			{
				ToOptimize.RemoveAt(num);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		DisposeDynamicOptimizer();
		CleanCullingGroup();
		if (!isQuitting && !FOptimizers_Manager.AppIsQuitting)
		{
			FOptimizers_Manager.Get.UnRegisterOptimizer(this);
		}
	}

	private void OnApplicationQuit()
	{
		isQuitting = true;
		CleanCullingGroup();
	}

	public void CleanAsset()
	{
	}

	private List<T> FindComponentsInAllChildren<T>(Transform transformToSearchIn) where T : Component
	{
		List<T> list = new List<T>();
		Transform[] componentsInChildren = transformToSearchIn.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			T component = componentsInChildren[i].GetComponent<T>();
			if ((bool)(Object)component)
			{
				list.Add(component);
			}
		}
		return list;
	}

	protected void OptimizerReset()
	{
	}

	protected virtual void OnEnable()
	{
		if (wasDisabled)
		{
			ApplyLastEvent();
			wasDisabled = false;
		}
	}

	private void ApplyLastEvent()
	{
		if (OptimizingMethod == FEOptimizingMethod.Dynamic)
		{
			OutOfCameraView = false;
			DynamicLODUpdate(CurrentDynamicDistanceCategory.Value, lastDynamicDistance);
			return;
		}
		if (OptimizingMethod == FEOptimizingMethod.Effective && CurrentDynamicDistanceCategory.HasValue)
		{
			DynamicLODUpdate(CurrentDynamicDistanceCategory.Value, lastDynamicDistance);
		}
		CullingGroupStateChanged(lastEvent);
	}

	public void OptimizerOnValidate()
	{
	}

	protected void OnValidateStart()
	{
		if (LODLevels <= 0)
		{
			LODLevels = 2;
		}
		if (LODLevels > 8)
		{
			LODLevels = 8;
		}
		if (DetectionRadius < 0f)
		{
			DetectionRadius = 0f;
		}
	}

	protected void OnValidateRefreshComponents()
	{
		if (ToOptimize != null)
		{
			RefreshToOptimizeList();
		}
		else
		{
			AssignComponentsToOptimizeFrom(base.gameObject.transform);
		}
	}

	protected void OnValidateUpdateToOptimize()
	{
		if (preLODLevels != LODLevels)
		{
			ResetLODs();
		}
		preLODLevels = LODLevels;
	}

	public void SetAutoDistance(float multiplier = 1f)
	{
		switch (OptimizingMethod)
		{
		case FEOptimizingMethod.Static:
		case FEOptimizingMethod.Effective:
			MaxDistance = DetectionRadius * 550f;
			MaxDistance *= GetScaler(base.transform);
			if ((bool)FOptimizers_Manager.MainCamera && MaxDistance > FOptimizers_Manager.MainCamera.farClipPlane)
			{
				MaxDistance = FOptimizers_Manager.MainCamera.farClipPlane;
			}
			MaxDistance *= multiplier;
			break;
		case FEOptimizingMethod.Dynamic:
		case FEOptimizingMethod.TriggerBased:
			MaxDistance = DetectionBounds.magnitude * 166f;
			MaxDistance *= GetScaler(base.transform);
			if ((bool)FOptimizers_Manager.MainCamera && MaxDistance > FOptimizers_Manager.MainCamera.farClipPlane)
			{
				MaxDistance = FOptimizers_Manager.MainCamera.farClipPlane;
			}
			MaxDistance *= multiplier;
			break;
		}
	}

	protected void OnValidateCheckForStatic()
	{
	}

	public void SyncWithReferences()
	{
		if (ToOptimize.Count > 0 && ToOptimize[0].LODSet != null && ToOptimize[0].LODSet.LevelOfDetailSets != null && ToOptimize[0].LODSet.LevelOfDetailSets.Count > 0 && ToOptimize[0].LODSet.LevelOfDetailSets.Count - 2 != LODLevels)
		{
			LODLevels = ToOptimize[0].LODSet.LevelOfDetailSets.Count - 2;
			preLODLevels = LODLevels;
		}
	}

	public void EditorUpdate()
	{
	}

	public void EditorResetLODValues()
	{
	}

	private void InitEffectiveOptimizer()
	{
		if (!AddToContainer)
		{
			FOptimizers_Manager.Get.RegisterNotContainedEffectiveOptimizer(this, init: true);
		}
		InitCullingGroups(GetDistanceMeasures(), DetectionRadius, FOptimizers_Manager.MainCamera);
		InitDynamicOptimizer(justDynamic: false);
	}

	private void EffectiveLODUpdate()
	{
		if ((PreviousPosition - mainVisibilitySphere.position).magnitude > moveTreshold)
		{
			RefreshEffectiveCullingGroups();
		}
	}

	protected virtual void RefreshEffectiveCullingGroups()
	{
		if (OwnerContainer != null)
		{
			OwnerContainer.CullingSpheres[ContainerSphereId].position = GetReferencePosition();
		}
		else
		{
			mainVisibilitySphere.position = GetReferencePosition();
		}
	}

	public void Gizmos_IsResizingLOD(int lod)
	{
		isResizing = lod;
	}

	public void Gizmos_StopChanging()
	{
		isResizing = -1;
	}

	public void Gizmos_SelectLOD(int lod)
	{
		isSelected = lod;
	}

	protected virtual void OnDrawGizmos()
	{
		if (FOptimizers_Manager.DrawGizmos && !(GizmosAlpha <= 0f))
		{
			Gizmos.DrawIcon(base.transform.position, "FIMSpace/FOptimizing/Optimizers Gizmo Icon.png", allowScaling: true);
		}
	}

	private void InitTriggerOptimizer()
	{
		if (triggersEntered == null)
		{
			triggersEntered = new List<int>();
		}
		Transform transform = ((FOptimizers_Manager.MainCamera != null) ? FOptimizers_Manager.MainCamera.transform : null);
		if ((bool)transform)
		{
			OnlyCamCollLayer = transform.gameObject.layer;
		}
		TargetCamera = transform;
		float[] distanceMeasures = GetDistanceMeasures();
		DistanceLevels = new float[distanceMeasures.Length];
		for (int i = 0; i < distanceMeasures.Length; i++)
		{
			DistanceLevels[i] = distanceMeasures[i];
		}
		FOptimizers_Manager.Get.RegisterNotContainedTriggerOptimizer(this, init: true);
		if (CullIfNotSee)
		{
			InitDynamicOptimizer(justDynamic: false);
		}
		TriggerLODUpdate();
		GenerateTriggerHelpers();
		OutOfDistance = true;
		RefreshVisibilityState(CurrentDistanceLODLevel);
	}

	private void TriggerLODUpdate()
	{
		if (CullIfNotSee)
		{
			optimizerBounds.center = GetReferencePosition();
			OutOfCameraView = !GeometryUtility.TestPlanesAABB(manager.CurrentFrustumPlanes, optimizerBounds);
			if (WasOutOfCameraView != OutOfCameraView)
			{
				RefreshVisibilityState(CurrentDistanceLODLevel);
			}
		}
		else
		{
			OutOfCameraView = false;
		}
	}

	internal virtual void OnTriggerChange(FOptimizers_TriggerHelper helper, bool exit)
	{
		int num;
		if (!exit)
		{
			if (!triggersEntered.Contains(helper.TriggerIndex))
			{
				triggersEntered.Add(helper.TriggerIndex);
			}
			num = helper.TriggerIndex;
		}
		else
		{
			triggersEntered.Remove(helper.TriggerIndex);
			num = ((triggersEntered.Count != 0) ? triggersEntered[triggersEntered.Count - 1] : LODLevels);
		}
		if (num >= LODLevels + 1)
		{
			num = LODLevels;
		}
		triggerDistanceState = num;
		bool flag = false;
		if (preTriggerDistanceState != num)
		{
			flag = true;
		}
		if (triggersEntered.Count == 0)
		{
			OutOfDistance = true;
		}
		else
		{
			OutOfDistance = false;
		}
		if (flag)
		{
			RefreshVisibilityState(num);
			preTriggerDistanceState = num;
		}
	}

	protected void GenerateTriggerHelpers()
	{
		if (!(triggersContainer == null))
		{
			return;
		}
		GameObject gameObject = new GameObject("Optimizers-" + base.name + "-Triggers");
		triggersContainer = gameObject.transform;
		triggersContainer.SetParent(base.transform);
		triggersContainer.localPosition = DetectionOffset;
		triggersContainer.localRotation = Quaternion.identity;
		triggersContainer.localScale = Vector3.one;
		triggersContainer.gameObject.layer = OnlyCamCollLayer;
		for (int i = 0; i < DistanceLevels.Length; i++)
		{
			GameObject obj = new GameObject(i.ToString());
			Transform obj2 = obj.transform;
			obj2.SetParent(triggersContainer, worldPositionStays: false);
			obj2.localPosition = Vector3.zero;
			obj2.localRotation = Quaternion.identity;
			obj2.localScale = Vector3.one;
			SphereCollider sphereCollider = obj.AddComponent<SphereCollider>();
			sphereCollider.isTrigger = true;
			float num = base.transform.lossyScale.x;
			if (num == 0f)
			{
				num = 1f;
			}
			sphereCollider.radius = DistanceLevels[i] / num;
			obj.AddComponent<FOptimizers_TriggerHelper>().Initialize(this, i);
		}
	}
}
