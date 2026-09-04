using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FIMSpace.FOptimizing;

[AddComponentMenu("FImpossible Creations/Optimizers/System/Optimizers Manager")]
public class FOptimizers_Manager : MonoBehaviour, IDropHandler, IEventSystemHandler, IFHierarchyIcon
{
	[Tooltip("(DontDestroyOnLoad - untoggled just for package examples purpose!)\n\nWith this option enabled, manager will be never destroyed, even during changing scenes. This one manager can be used as only manager in whole game time")]
	[FPD_Width(135)]
	public bool ExistThroughScenes = true;

	public static bool DrawGizmos;

	private static FOptimizers_Manager _get;

	[Tooltip("Main rendering camera reference")]
	public Camera TargetCamera;

	private static Camera _mainCam;

	private Vector3 previousCameraPositionMoveTrigger;

	private List<List<FOptimizer_Base>> dynamicLists;

	private bool existThroughScenes;

	private bool initialized;

	public static bool AppIsQuitting;

	public List<FOptimizer_Base> notContainedStaticOptimizers = new List<FOptimizer_Base>();

	public List<FOptimizer_Base> notContainedDynamicOptimizers = new List<FOptimizer_Base>();

	public List<FOptimizer_Base> notContainedEffectiveOptimizers = new List<FOptimizer_Base>();

	public List<FOptimizer_Base> notContainedTriggerOptimizers = new List<FOptimizer_Base>();

	[Header("Dynamic Optimization Parameters")]
	public bool Advanced;

	public bool Debugging;

	[Tooltip("If camera is not moving or not rotating there will be ignored some of calculations")]
	public bool DetectCameraFreeze;

	internal static int RaycastsInThisFrame;

	internal static int HiddenObjects;

	[Tooltip("When you adding this component, algorithm is adapting this value as MainCamera Far Clipping planes are setted*\n\nAutomatic optimization distance values basing on main character size - Check human scale gizmo in scene view next to camera (It can need other adjustement anyway - depends of project needs)")]
	public float WorldScale = 2f;

	[Tooltip("What amount of units should move camera/optimized object in previous frame to trigger checking LOD state (if camera and object doesn't move checking LOD state will be ignored - optimization for system)")]
	public float MoveTreshold;

	[Tooltip("If you want to object checking be even quicker (in some cases can affect a little performance but will reponse much quicker)")]
	[Range(0f, 1f)]
	public float UpdateBoost;

	[Tooltip("You can define in which distances optimized objects should be prioritized lower for checking LOD state")]
	public float[] Distances;

	private FOptimizers_DynamicClock[] clocks;

	private long totalTimeConsumption;

	private readonly List<FOptimizers_Transitioning> transitioning = new List<FOptimizers_Transitioning>();

	public string EditorIconPath => "FIMSpace/FOptimizing/Optimizers Manager Icon";

	public static FOptimizers_Manager Get
	{
		get
		{
			if (_get == null)
			{
				GenerateOptimizersManager();
			}
			if (_get == null)
			{
				return UnityEngine.Object.FindObjectOfType<FOptimizers_Manager>();
			}
			return _get;
		}
		private set
		{
			_get = value;
		}
	}

	public static bool Exists
	{
		get
		{
			if (_get == null)
			{
				UnityEngine.Object.FindObjectOfType<FOptimizers_Manager>().SetGet();
			}
			return _get != null;
		}
	}

	public static Camera MainCamera
	{
		get
		{
			if (_mainCam == null)
			{
				GetMainCamera();
			}
			return _mainCam;
		}
		private set
		{
			_mainCam = value;
		}
	}

	public Dictionary<int, FOptimizers_CullingContainersList> CullingContainersIDSpecific { get; private set; }

	public Plane[] CurrentFrustumPlanes { get; private set; }

	public void OnDrop(PointerEventData data)
	{
	}

	private static void GenerateOptimizersManager()
	{
		FOptimizers_Manager fOptimizers_Manager = UnityEngine.Object.FindObjectOfType<FOptimizers_Manager>();
		if (!fOptimizers_Manager)
		{
			GameObject obj = new GameObject("Generated Optimizers Manager");
			obj.transform.SetAsFirstSibling();
			fOptimizers_Manager = obj.AddComponent<FOptimizers_Manager>();
		}
		_get = fOptimizers_Manager;
		Get = fOptimizers_Manager;
		Get.Init();
	}

	private static void GetMainCamera()
	{
		bool num = _mainCam != null;
		Camera camera = Camera.main;
		if (camera == null)
		{
			camera = UnityEngine.Object.FindObjectOfType<Camera>();
			if ((bool)camera)
			{
				Debug.LogWarning("[OPTIMIZERS] There is no object with 'MainCamera' Tag!");
			}
			else if (FEditor_OneShotLog.CanDrawLog("OptNoCamera", 10))
			{
				Debug.LogWarning("[OPTIMIZERS] There is no camera on the scene!");
			}
		}
		_mainCam = camera;
		Get.TargetCamera = camera;
		if (!num)
		{
			SetNewMainCamera(camera);
		}
	}

	public void SetGet()
	{
		FOptimizers_Manager fOptimizers_Manager = UnityEngine.Object.FindObjectOfType<FOptimizers_Manager>();
		bool flag = false;
		if ((bool)fOptimizers_Manager && fOptimizers_Manager != this)
		{
			if (Application.isPlaying)
			{
				Debug.LogError("[OPTIMIZERS] There can't be two Optimizers Managers at the same time! I'm removing new one!");
				UnityEngine.Object.Destroy(this);
				flag = true;
			}
			else
			{
				Debug.LogError("[OPTIMIZERS EDITOR] There can't be two Optimizers Managers at the same time! I'm removing previous one!");
				UnityEngine.Object.DestroyImmediate(fOptimizers_Manager);
				flag = true;
			}
		}
		if (flag)
		{
			return;
		}
		if (_get != null && _get != this)
		{
			if (Application.isPlaying)
			{
				Debug.LogError("[OPTIMIZERS] There can't be two Optimizers Managers at the same time! I'm removing new one!");
				UnityEngine.Object.Destroy(this);
			}
			else
			{
				Debug.LogError("[OPTIMIZERS EDITOR] There can't be two Optimizers Managers at the same time! I'm removing previous one!");
				UnityEngine.Object.DestroyImmediate(_get);
			}
		}
		else
		{
			Get = this;
		}
	}

	public static void SetNewMainCamera(Camera camera)
	{
		if (camera == null)
		{
			return;
		}
		MainCamera = camera;
		foreach (FOptimizer_Base notContainedStaticOptimizer in Get.notContainedStaticOptimizers)
		{
			notContainedStaticOptimizer.RefreshCamera(camera);
		}
		foreach (FOptimizer_Base notContainedDynamicOptimizer in Get.notContainedDynamicOptimizers)
		{
			notContainedDynamicOptimizer.RefreshCamera(camera);
		}
		foreach (FOptimizer_Base notContainedEffectiveOptimizer in Get.notContainedEffectiveOptimizers)
		{
			notContainedEffectiveOptimizer.RefreshCamera(camera);
		}
		foreach (FOptimizer_Base notContainedTriggerOptimizer in Get.notContainedTriggerOptimizers)
		{
			notContainedTriggerOptimizer.RefreshCamera(camera);
		}
		SetNewMainCameraForContainers(camera);
	}

	public static void SetNewMainCameraForContainers(Camera camera)
	{
		MainCamera = camera;
		if (Get.CullingContainersIDSpecific == null)
		{
			return;
		}
		foreach (KeyValuePair<int, FOptimizers_CullingContainersList> item in Get.CullingContainersIDSpecific)
		{
			for (int i = 0; i < item.Value.Count; i++)
			{
				item.Value[i].SetNewCamera(camera);
			}
		}
	}

	public static void SwitchOptimizersOnOrOff(bool on = true, bool unhideAll = true)
	{
		if (!Get)
		{
			return;
		}
		Get.enabled = on;
		FOptimizer_Base[] array;
		if (unhideAll)
		{
			array = UnityEngine.Object.FindObjectsOfType<FOptimizer_Base>();
			foreach (FOptimizer_Base fOptimizer_Base in array)
			{
				if (fOptimizer_Base.CullingGroup != null)
				{
					fOptimizer_Base.CullingGroup.enabled = on;
				}
				fOptimizer_Base.SetLODLevel(0);
			}
			return;
		}
		array = UnityEngine.Object.FindObjectsOfType<FOptimizer_Base>();
		foreach (FOptimizer_Base fOptimizer_Base2 in array)
		{
			if (fOptimizer_Base2.CullingGroup != null)
			{
				fOptimizer_Base2.CullingGroup.enabled = on;
			}
		}
	}

	private static int GetDistanceTypesCount()
	{
		return Enum.GetValues(typeof(FEOptimizingDistance)).Length;
	}

	private void Awake()
	{
		if (!Application.isPlaying)
		{
			SetGet();
		}
		else
		{
			Init();
		}
	}

	private void Start()
	{
		Init();
	}

	private void Reset()
	{
		GetMainCamera();
		if ((bool)MainCamera)
		{
			WorldScale = (float)Math.Round(MainCamera.farClipPlane / 520f, 2);
		}
	}

	public void Init()
	{
		if (initialized)
		{
			return;
		}
		SetGet();
		if (Application.isPlaying)
		{
			if (ExistThroughScenes)
			{
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
				existThroughScenes = true;
			}
			dynamicLists = new List<List<FOptimizer_Base>>();
			CullingContainersIDSpecific = new Dictionary<int, FOptimizers_CullingContainersList>();
			initialized = true;
			GenerateClocks();
			RefreshDistances();
			RunDynamicClocks();
		}
	}

	private void Update()
	{
		if (!existThroughScenes && ExistThroughScenes)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		if (TargetCamera == null)
		{
			GetMainCamera();
			SetNewMainCamera(TargetCamera);
			if (TargetCamera != null)
			{
				Debug.Log("[OPTIMIZERS] New Camera detected and assigned! " + TargetCamera.name);
			}
			return;
		}
		if (TargetCamera != MainCamera)
		{
			SetNewMainCamera(TargetCamera);
			Debug.Log("[OPTIMIZERS] New Camera detected and assigned! " + TargetCamera.name);
		}
		TransitionsUpdate();
		DynamicUpdate();
	}

	public void OnValidate()
	{
		if (TargetCamera != null && TargetCamera != MainCamera)
		{
			MainCamera = TargetCamera;
		}
		if (WorldScale <= 0f)
		{
			WorldScale = 0.1f;
		}
		if (!Advanced)
		{
			MoveTreshold = WorldScale / (150f * (1f + UpdateBoost));
		}
		RefreshDistances();
		if (!Advanced)
		{
			Debugging = false;
		}
		TargetCamera = MainCamera;
	}

	private void OnApplicationQuit()
	{
		AppIsQuitting = true;
	}

	internal void AddToContainer(FOptimizer_Base optimizer)
	{
		if (optimizer == null)
		{
			return;
		}
		FOptimizers_CullingContainer fOptimizers_CullingContainer = null;
		if (CullingContainersIDSpecific.TryGetValue(optimizer.ContainerGeneratedID, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].HaveFreeSlots)
				{
					fOptimizers_CullingContainer = value[i];
					break;
				}
			}
			if (fOptimizers_CullingContainer == null)
			{
				fOptimizers_CullingContainer = GenerateNewContainer(optimizer);
				value.Add(fOptimizers_CullingContainer);
			}
		}
		else
		{
			value = new FOptimizers_CullingContainersList(optimizer.ContainerGeneratedID);
			fOptimizers_CullingContainer = GenerateNewContainer(optimizer);
			value.Add(fOptimizers_CullingContainer);
			CullingContainersIDSpecific.Add(optimizer.ContainerGeneratedID, value);
		}
		fOptimizers_CullingContainer.AddOptimizer(optimizer);
	}

	private FOptimizers_CullingContainer GenerateNewContainer(FOptimizer_Base optimizer)
	{
		FOptimizers_CullingContainer fOptimizers_CullingContainer = new FOptimizers_CullingContainer();
		fOptimizers_CullingContainer.InitializeContainer(optimizer.ContainerGeneratedID, optimizer.GetDistanceMeasures(), TargetCamera);
		return fOptimizers_CullingContainer;
	}

	internal void RemoveFromContainer(FOptimizer_Base optimizer)
	{
		if (!(optimizer == null))
		{
			optimizer.OwnerContainer.RemoveOptimizer(optimizer);
		}
	}

	private void OnDestroy()
	{
		ClearCullingContainers();
	}

	internal void ClearCullingContainers()
	{
		if (CullingContainersIDSpecific == null)
		{
			return;
		}
		foreach (KeyValuePair<int, FOptimizers_CullingContainersList> item in CullingContainersIDSpecific)
		{
			item.Value.Dispose();
		}
		CullingContainersIDSpecific.Clear();
	}

	public void RegisterNotContainedOptimizer(FOptimizer_Base optimizer, bool init = false)
	{
		switch (optimizer.OptimizingMethod)
		{
		case FEOptimizingMethod.Static:
			RegisterNotContainedStaticOptimizer(optimizer, init);
			break;
		case FEOptimizingMethod.Dynamic:
			RegisterNotContainedDynamicOptimizer(optimizer, init);
			break;
		case FEOptimizingMethod.Effective:
			RegisterNotContainedEffectiveOptimizer(optimizer, init);
			break;
		case FEOptimizingMethod.TriggerBased:
			RegisterNotContainedTriggerOptimizer(optimizer, init);
			break;
		}
	}

	public void RegisterNotContainedStaticOptimizer(FOptimizer_Base optimizer, bool init = false)
	{
		if (init)
		{
			notContainedStaticOptimizers.Add(optimizer);
		}
		else if (!notContainedStaticOptimizers.Contains(optimizer))
		{
			notContainedStaticOptimizers.Add(optimizer);
		}
	}

	public void RegisterNotContainedDynamicOptimizer(FOptimizer_Base optimizer, bool init = false)
	{
		if (init)
		{
			notContainedDynamicOptimizers.Add(optimizer);
		}
		else if (!notContainedDynamicOptimizers.Contains(optimizer))
		{
			notContainedDynamicOptimizers.Add(optimizer);
		}
	}

	public void RegisterNotContainedEffectiveOptimizer(FOptimizer_Base optimizer, bool init = false)
	{
		if (init)
		{
			notContainedEffectiveOptimizers.Add(optimizer);
		}
		else if (!notContainedEffectiveOptimizers.Contains(optimizer))
		{
			notContainedEffectiveOptimizers.Add(optimizer);
		}
	}

	public void RegisterNotContainedTriggerOptimizer(FOptimizer_Base optimizer, bool init = false)
	{
		if (init)
		{
			notContainedTriggerOptimizers.Add(optimizer);
		}
		else if (!notContainedTriggerOptimizers.Contains(optimizer))
		{
			notContainedTriggerOptimizers.Add(optimizer);
		}
	}

	public void UnRegisterOptimizer(FOptimizer_Base optimizer)
	{
		if (!optimizer.AddToContainer)
		{
			switch (optimizer.OptimizingMethod)
			{
			case FEOptimizingMethod.Static:
				UnRegisterStaticOptimizer(optimizer);
				break;
			case FEOptimizingMethod.Dynamic:
				UnRegisterDynamicOptimizer(optimizer);
				break;
			case FEOptimizingMethod.Effective:
				UnRegisterEffectiveOptimizer(optimizer);
				break;
			case FEOptimizingMethod.TriggerBased:
				UnRegisterTriggerOptimizer(optimizer);
				break;
			}
		}
	}

	public void UnRegisterStaticOptimizer(FOptimizer_Base optimizer)
	{
		if (notContainedStaticOptimizers.Contains(optimizer))
		{
			notContainedStaticOptimizers.Remove(optimizer);
		}
	}

	public void UnRegisterDynamicOptimizer(FOptimizer_Base optimizer)
	{
		if (!notContainedDynamicOptimizers.Contains(optimizer))
		{
			notContainedDynamicOptimizers.Remove(optimizer);
		}
	}

	public void UnRegisterEffectiveOptimizer(FOptimizer_Base optimizer)
	{
		if (!notContainedEffectiveOptimizers.Contains(optimizer))
		{
			notContainedEffectiveOptimizers.Remove(optimizer);
		}
	}

	public void UnRegisterTriggerOptimizer(FOptimizer_Base optimizer)
	{
		if (!notContainedTriggerOptimizers.Contains(optimizer))
		{
			notContainedTriggerOptimizers.Remove(optimizer);
		}
	}

	private void GenerateClocks()
	{
		if (clocks == null)
		{
			clocks = new FOptimizers_DynamicClock[GetDistanceTypesCount()];
			for (int i = 0; i < clocks.Length; i++)
			{
				dynamicLists.Add(new List<FOptimizer_Base>());
				clocks[i] = new FOptimizers_DynamicClock(this, (FEOptimizingDistance)i, dynamicLists[i]);
			}
		}
	}

	private void RunDynamicClocks()
	{
		StartCoroutine(InitialCall());
		for (int i = 0; i < clocks.Length; i++)
		{
			StartCoroutine(clocks[i].WatchUpdate());
		}
	}

	private void DynamicUpdate()
	{
		RaycastsInThisFrame = 0;
		CurrentFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(MainCamera);
		totalTimeConsumption = 0L;
		for (int i = 0; i < clocks.Length; i++)
		{
			totalTimeConsumption += clocks[i].FrameTicksConsumption;
		}
	}

	public static void CallUpdateAll()
	{
		if (!MainCamera)
		{
			return;
		}
		for (int i = 0; i < Get.dynamicLists.Count; i++)
		{
			for (int num = Get.dynamicLists[i].Count - 1; num >= 0; num--)
			{
				Get.CheckElement(Get.dynamicLists[i][num], num, full: false);
			}
		}
	}

	public int AddToDynamic(FOptimizer_Base optimizer)
	{
		float distance = float.MaxValue;
		if ((bool)MainCamera)
		{
			distance = (optimizer.GetReferencePosition() - MainCamera.transform.position).magnitude;
		}
		FEOptimizingDistance fEOptimizingDistance = QualifyDistance(distance);
		int num = -1;
		if (optimizer.CurrentDynamicDistanceCategory != fEOptimizingDistance)
		{
			if (optimizer.CurrentDynamicDistanceCategory.HasValue)
			{
				dynamicLists[(int)optimizer.CurrentDynamicDistanceCategory.Value].RemoveAt(optimizer.DynamicListIndex);
			}
			dynamicLists[(int)fEOptimizingDistance].Add(optimizer);
			num = dynamicLists[(int)fEOptimizingDistance].Count;
			if ((bool)MainCamera)
			{
				optimizer.DynamicLODUpdate(fEOptimizingDistance, distance);
			}
			return num;
		}
		return optimizer.DynamicListIndex;
	}

	public void RemoveFromDynamic(FOptimizer_Base optimizer)
	{
		if (optimizer.CurrentDynamicDistanceCategory.HasValue)
		{
			dynamicLists[(int)optimizer.CurrentDynamicDistanceCategory.Value].Remove(optimizer);
		}
	}

	public void CheckElement(FOptimizer_Base optimizer, int index, bool full = true)
	{
		if (full && !optimizer.TresholdTrigger())
		{
			return;
		}
		float distance = Vector3.Distance(optimizer.TargetCamera.position, optimizer.GetReferencePosition());
		FEOptimizingDistance fEOptimizingDistance = QualifyDistance(distance);
		if (fEOptimizingDistance != optimizer.CurrentDynamicDistanceCategory)
		{
			if (optimizer.CurrentDynamicDistanceCategory.HasValue)
			{
				dynamicLists[(int)optimizer.CurrentDynamicDistanceCategory.Value].RemoveAt(index);
			}
			dynamicLists[(int)fEOptimizingDistance].Add(optimizer);
		}
		optimizer.DynamicLODUpdate(fEOptimizingDistance, distance);
	}

	private IEnumerator InitialCall()
	{
		yield return null;
		CallUpdateAll();
	}

	private FEOptimizingDistance QualifyDistance(float distance)
	{
		for (int i = 0; i < Distances.Length; i++)
		{
			if (distance < Distances[i])
			{
				return (FEOptimizingDistance)i;
			}
		}
		return FEOptimizingDistance.Farthest;
	}

	public void RefreshDistances()
	{
		if (Advanced)
		{
			if (Distances != null)
			{
				for (int i = 1; i < Distances.Length; i++)
				{
					if (Distances[i] < Distances[i - 1] * 1.05f)
					{
						Distances[i] = Distances[i - 1] * 1.2f;
					}
				}
			}
			else
			{
				Distances = new float[GetDistanceTypesCount() - 1];
			}
		}
		else
		{
			Distances = new float[GetDistanceTypesCount() - 1];
			for (int j = 0; j < Distances.Length; j++)
			{
				Distances[j] = Mathf.Lerp(60f * WorldScale, 750f * WorldScale, (float)j / (float)Distances.Length);
			}
		}
	}

	public bool CameraMoved(Vector3 prePos, Quaternion preRot)
	{
		if (!DetectCameraFreeze)
		{
			return true;
		}
		bool flag = false;
		flag = (((MainCamera.transform.position - prePos).magnitude > Mathf.Max(1E-06f, Get.MoveTreshold)) ? true : false);
		if (!flag && Quaternion.Angle(preRot, MainCamera.transform.rotation) > 0.1f)
		{
			flag = true;
		}
		return flag;
	}

	public void TransitionTo(FOptimizer_Base optimizer, int targetLODLevel, float duration = 0f)
	{
		int instanceID = optimizer.GetInstanceID();
		FOptimizers_Transitioning fOptimizers_Transitioning = null;
		for (int i = 0; i < transitioning.Count; i++)
		{
			if (transitioning[i].Id == instanceID)
			{
				fOptimizers_Transitioning = transitioning[i];
				break;
			}
		}
		if (fOptimizers_Transitioning != null)
		{
			fOptimizers_Transitioning.BreakCurrentTransition(duration, targetLODLevel);
			return;
		}
		fOptimizers_Transitioning = new FOptimizers_Transitioning(instanceID, optimizer, targetLODLevel, duration, transitioning.Count);
		transitioning.Add(fOptimizers_Transitioning);
	}

	public void EndTransition(FOptimizer_Base optimizer)
	{
		int instanceID = optimizer.GetInstanceID();
		for (int i = 0; i < transitioning.Count; i++)
		{
			if (transitioning[i].Id == instanceID)
			{
				transitioning[i].Finish();
				transitioning.RemoveAt(i);
				break;
			}
		}
	}

	private void TransitionsUpdate()
	{
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		for (int num = transitioning.Count - 1; num >= 0; num--)
		{
			transitioning[num].Update(unscaledDeltaTime);
			if (transitioning[num].Finished)
			{
				transitioning.RemoveAt(num);
			}
		}
	}
}
