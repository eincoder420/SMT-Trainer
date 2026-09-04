using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace MeshCombineStudio;

[ExecuteInEditMode]
public class MeshCombiner : MonoBehaviour
{
	public enum ObjectType
	{
		Normal,
		LodGroup,
		LodRenderer
	}

	public enum HandleComponent
	{
		Disable,
		Destroy
	}

	public enum ObjectCenter
	{
		BoundsCenter,
		TransformPosition
	}

	public enum BackFaceTriangleMode
	{
		Transform,
		Box,
		Direction,
		EulerAngles
	}

	public delegate void EventMethod(MeshCombiner meshCombiner);

	public enum RebakeLightingMode
	{
		CopyLightmapUvs,
		RegenarateLightmapUvs
	}

	[Serializable]
	public class SearchOptions
	{
		public enum ComponentCondition
		{
			And,
			Or,
			Not
		}

		public enum LODGroupSearchMode
		{
			LodGroup,
			LodRenderers
		}

		public bool foldoutSearchParents = true;

		public bool foldoutSearchConditions = true;

		public GameObject parent;

		public GameObject[] parentGOs;

		public ObjectCenter objectCenter;

		public LODGroupSearchMode lodGroupSearchMode;

		public bool useSearchBox;

		public Bounds searchBoxBounds;

		public bool searchBoxSquare;

		public Vector3 searchBoxPivot;

		public Vector3 searchBoxSize = new Vector3(25f, 25f, 25f);

		public bool useMaxBoundsFactor = true;

		public float maxBoundsFactor = 1.5f;

		public bool useVertexInputLimit = true;

		public int vertexInputLimit = 5000;

		public bool useLayerMask;

		public LayerMask layerMask = -1;

		public bool useTag;

		public string tag;

		public bool useNameContains;

		public List<string> nameContainList = new List<string>();

		public bool onlyActive = true;

		public bool onlyStatic = true;

		public bool onlyActiveMeshRenderers = true;

		public bool useComponentsFilter;

		public ComponentCondition componentCondition;

		public List<string> componentNameList = new List<string>();

		public void GetSearchBoxBounds()
		{
			searchBoxBounds = new Bounds(searchBoxPivot + new Vector3(0f, searchBoxSize.y * 0.5f, 0f), searchBoxSize);
		}
	}

	[Serializable]
	public class LODGroupSettings
	{
		public bool animateCrossFading;

		public LODFadeMode fadeMode;

		public LODSettings[] lodSettings;

		public LODGroupSettings(int lodParentIndex)
		{
			int num = lodParentIndex + 1;
			lodSettings = new LODSettings[num];
			float num2 = 1f / (float)num;
			for (int i = 0; i < lodSettings.Length; i++)
			{
				lodSettings[i] = new LODSettings(1f - num2 * (float)(i + 1));
			}
		}

		public void CopyFromLodGroup(LODGroup lodGroup, LOD[] lods)
		{
			animateCrossFading = lodGroup.animateCrossFading;
			fadeMode = lodGroup.fadeMode;
			for (int i = 0; i < lods.Length; i++)
			{
				lodSettings[i].fadeTransitionWidth = lods[i].fadeTransitionWidth;
			}
		}

		public void CopyToLodGroup(LODGroup lodGroup, LOD[] lods)
		{
			lodGroup.animateCrossFading = animateCrossFading;
			lodGroup.fadeMode = fadeMode;
			for (int i = 0; i < lods.Length; i++)
			{
				lods[i].fadeTransitionWidth = lodSettings[i].fadeTransitionWidth;
			}
		}
	}

	[Serializable]
	public class LODSettings
	{
		public float screenRelativeTransitionHeight;

		public float fadeTransitionWidth;

		public LODSettings(float screenRelativeTransitionHeight)
		{
			this.screenRelativeTransitionHeight = screenRelativeTransitionHeight;
		}
	}

	[Serializable]
	public class LodParentHolder
	{
		public GameObject go;

		public Transform t;

		public bool found;

		public int[] lods;

		public void Init(int lodCount)
		{
			lods = new int[lodCount];
		}

		public void Create(MeshCombiner meshCombiner, int lodParentIndex)
		{
			if (meshCombiner.data.foundLodGroups.Count == 0)
			{
				go = new GameObject((meshCombiner.combineMode == CombineMode.StaticObjects) ? "Cells" : "Combine Parent");
			}
			else
			{
				go = new GameObject("LODGroup " + (lodParentIndex + 1));
				go.AddComponent<LODGroupSetup>().Init(meshCombiner, lodParentIndex);
			}
			t = go.transform;
			t.transform.parent = meshCombiner.transform;
		}

		public void Reset()
		{
			found = false;
			Array.Clear(lods, 0, lods.Length);
		}
	}

	public static EventMethod onInit;

	public static List<MeshCombiner> instances = new List<MeshCombiner>();

	public MeshCombineJobManager.JobSettings jobSettings = new MeshCombineJobManager.JobSettings();

	public LODGroupSettings[] lodGroupsSettings;

	public ComputeShader computeDepthToArray;

	public GameObject instantiatePrefab;

	public const int maxLodCount = 8;

	public string saveMeshesFolder;

	public ObjectOctree.Cell octree;

	public List<ObjectOctree.MaxCell> changedCells;

	[NonSerialized]
	public bool octreeContainsObjects;

	public bool unitySettingsFoldout = true;

	public SearchOptions searchOptions;

	public CombineConditionSettings combineConditionSettings;

	public bool outputSettingsFoldout = true;

	public CombineMode combineMode;

	public int cellSize = 32;

	public Vector3 cellOffset;

	public int cellCount;

	public bool removeOriginalMeshReference;

	public bool useVertexOutputLimit;

	public int vertexOutputLimit = 64000;

	public RebakeLightingMode rebakeLightingMode;

	public bool copyBakedLighting;

	public bool validCopyBakedLighting;

	public bool rebakeLighting;

	public bool validRebakeLighting;

	public float scaleInLightmap = 1f;

	public bool addMeshColliders;

	public PhysicMaterial physicsMaterial;

	public bool addMeshCollidersInRange;

	public Bounds addMeshCollidersBounds;

	public bool makeMeshesUnreadable = true;

	public bool excludeSingleMeshes;

	public bool removeTrianglesBelowSurface;

	public bool noColliders;

	public LayerMask surfaceLayerMask;

	public float maxSurfaceHeight = 1000f;

	public bool removeOverlappingTriangles;

	public bool removeSamePositionTriangles;

	public bool reportFoundObjectsNotOnOverlapLayerMask = true;

	public GameObject overlappingCollidersGO;

	public LayerMask overlapLayerMask;

	public int voxelizeLayer;

	public int lodGroupLayer;

	public GameObject overlappingNonCombineGO;

	public bool disableOverlappingNonCombineGO;

	public bool removeBackFaceTriangles;

	public BackFaceTriangleMode backFaceTriangleMode;

	public Transform backFaceT;

	public Vector3 backFaceDirection;

	public Vector3 backFaceRotation;

	public Bounds backFaceBounds;

	public bool weldVertices;

	public bool weldSnapVertices;

	public float weldSnapSize = 0.025f;

	public bool weldIncludeNormals;

	public bool jobSettingsFoldout = true;

	public bool runtimeSettingsFoldout = true;

	public bool combineInRuntime;

	public bool combineOnStart = true;

	public bool useCombineSwapKey;

	public KeyCode combineSwapKey = KeyCode.Tab;

	public HandleComponent originalMeshRenderers;

	public HandleComponent originalLODGroups;

	public bool meshSaveSettingsFoldout = true;

	public bool deleteFilesFromSaveFolder;

	public Vector3 oldPosition;

	public Vector3 oldScale;

	public LodParentHolder[] lodParentHolders = new LodParentHolder[8];

	[HideInInspector]
	public List<GameObject> combinedGameObjects = new List<GameObject>();

	[HideInInspector]
	public List<CachedGameObject> foundObjects = new List<CachedGameObject>();

	[HideInInspector]
	public List<CachedLodGameObject> foundLodObjects = new List<CachedLodGameObject>();

	[HideInInspector]
	public List<LODGroup> foundLodGroups = new List<LODGroup>();

	[HideInInspector]
	public List<Collider> foundColliders = new List<Collider>();

	public HashSet<LODGroup> uniqueFoundLodGroups = new HashSet<LODGroup>();

	public HashSet<Mesh> unreadableMeshes = new HashSet<Mesh>();

	public HashSet<Mesh> selectImportSettingsMeshes = new HashSet<Mesh>();

	public FoundCombineConditions foundCombineConditions = new FoundCombineConditions();

	public HashSet<MeshCombineJobManager.MeshCombineJob> meshCombineJobs = new HashSet<MeshCombineJobManager.MeshCombineJob>();

	public int totalMeshCombineJobs;

	public int mrDisabledCount;

	public bool combined;

	public bool isCombining;

	public bool activeOriginal = true;

	public bool combinedActive;

	public bool drawGizmos = true;

	public bool drawMeshBounds = true;

	public int originalTotalVertices;

	public int originalTotalTriangles;

	public int newTotalVertices;

	public int newTotalTriangles;

	public int originalDrawCalls;

	public int newDrawCalls;

	public int originalTotalNormalChannels;

	public int originalTotalTangentChannels;

	public int originalTotalUvChannels;

	public int originalTotalUv2Channels;

	public int originalTotalUv3Channels;

	public int originalTotalUv4Channels;

	public int originalTotalColorChannels;

	public int newTotalNormalChannels;

	public int newTotalTangentChannels;

	public int newTotalUvChannels;

	public int newTotalUv2Channels;

	public int newTotalUv3Channels;

	public int newTotalUv4Channels;

	public int newTotalColorChannels;

	public float combineTime;

	[NonSerialized]
	public MeshCombinerData data;

	public FastList<MeshColliderAdd> addMeshCollidersList = new FastList<MeshColliderAdd>();

	private HashSet<Transform> uniqueLodObjects = new HashSet<Transform>();

	[NonSerialized]
	private MeshCombiner thisInstance;

	private bool hasFoundFirstObject;

	private Bounds bounds;

	private Stopwatch stopwatch = new Stopwatch();

	public event EventMethod onCombiningStart;

	public event EventMethod onCombiningAbort;

	public event EventMethod onCombiningReady;

	public void AddMeshColliders()
	{
		try
		{
			for (int i = 0; i < addMeshCollidersList.Count; i++)
			{
				MeshColliderAdd meshColliderAdd = addMeshCollidersList.items[i];
				MeshCollider meshCollider = meshColliderAdd.go.AddComponent<MeshCollider>();
				meshCollider.sharedMesh = meshColliderAdd.mesh;
				meshCollider.material = physicsMaterial;
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		finally
		{
			addMeshCollidersList.Clear();
		}
	}

	public void ExecuteOnCombiningReady()
	{
		totalMeshCombineJobs = 0;
		stopwatch.Stop();
		combineTime = (float)stopwatch.ElapsedMilliseconds / 1000f;
		combinedActive = true;
		combined = true;
		isCombining = false;
		if (this.onCombiningReady != null)
		{
			this.onCombiningReady(this);
		}
	}

	private void Awake()
	{
		Init();
	}

	private void OnEnable()
	{
		Init();
	}

	private void Init()
	{
		if (thisInstance == null)
		{
			instances.Add(this);
			thisInstance = this;
			if (onInit != null)
			{
				onInit(this);
			}
		}
	}

	private void OnDisable()
	{
		thisInstance = null;
		instances.Remove(this);
	}

	public void InitData()
	{
		if ((searchOptions.parentGOs == null || searchOptions.parentGOs.Length == 0) && (bool)searchOptions.parent)
		{
			searchOptions.parentGOs = new GameObject[1] { searchOptions.parent };
		}
		if (data == null)
		{
			data = GetComponent<MeshCombinerData>();
			if (data == null)
			{
				data = base.gameObject.AddComponent<MeshCombinerData>();
				data.combinedGameObjects = new List<GameObject>(combinedGameObjects);
				data.foundObjects = new List<CachedGameObject>(foundObjects);
				data.foundLodObjects = new List<CachedLodGameObject>(foundLodObjects);
				data.foundLodGroups = new List<LODGroup>(foundLodGroups);
				data.foundColliders = new List<Collider>(foundColliders);
				combinedGameObjects.Clear();
				foundObjects.Clear();
				foundLodObjects.Clear();
				foundLodGroups.Clear();
				foundColliders.Clear();
			}
		}
	}

	private void Start()
	{
		if (!Application.isPlaying || combineInRuntime)
		{
			InitMeshCombineJobManager();
			if (instances[0] == this)
			{
				MeshCombineJobManager.instance.SetJobMode(jobSettings);
			}
			if (Application.isPlaying || !Application.isEditor)
			{
				StartRuntime();
			}
		}
	}

	private void OnDestroy()
	{
		RestoreOriginalRenderersAndLODGroups(onDestroy: true);
		thisInstance = null;
		instances.Remove(this);
		if (instances.Count == 0 && MeshCombineJobManager.instance != null)
		{
			Methods.Destroy(MeshCombineJobManager.instance.gameObject);
			MeshCombineJobManager.instance = null;
		}
	}

	public static MeshCombiner GetInstance(string name)
	{
		for (int i = 0; i < instances.Count; i++)
		{
			if (instances[i].gameObject.name == name)
			{
				return instances[i];
			}
		}
		return null;
	}

	public void CopyJobSettingsToAllInstances()
	{
		for (int i = 0; i < instances.Count; i++)
		{
			instances[i].jobSettings.CopySettings(jobSettings);
		}
	}

	public void InitMeshCombineJobManager()
	{
		if (MeshCombineJobManager.instance == null)
		{
			MeshCombineJobManager.CreateInstance(this, instantiatePrefab);
		}
	}

	public void CreateLodGroupsSettings()
	{
		lodGroupsSettings = new LODGroupSettings[8];
		for (int i = 0; i < lodGroupsSettings.Length; i++)
		{
			lodGroupsSettings[i] = new LODGroupSettings(i);
		}
	}

	private void StartRuntime()
	{
		if (!combineInRuntime)
		{
			return;
		}
		if (combineOnStart)
		{
			CombineAll();
		}
		if (useCombineSwapKey && originalMeshRenderers == HandleComponent.Disable && originalLODGroups == HandleComponent.Disable)
		{
			if (SwapCombineKey.instance == null)
			{
				base.gameObject.AddComponent<SwapCombineKey>();
			}
			else
			{
				SwapCombineKey.instance.meshCombinerList.Add(this);
			}
		}
	}

	public void DestroyCombinedObjects()
	{
		AbortAndClearMeshCombineJobs(executeAbortEvent: false);
		RestoreOriginalRenderersAndLODGroups(onDestroy: false);
		Methods.DestroyChildren(base.transform);
		List<GameObject> list = data.combinedGameObjects;
		for (int i = 0; i < list.Count; i++)
		{
			Methods.Destroy(list[i]);
		}
		list.Clear();
		data.ClearAll();
		combinedActive = false;
		combined = false;
	}

	public void Reset()
	{
		DestroyCombinedObjects();
		uniqueLodObjects.Clear();
		uniqueFoundLodGroups.Clear();
		unreadableMeshes.Clear();
		foundCombineConditions.combineConditions.Clear();
		ResetOctree();
		hasFoundFirstObject = false;
		ref Bounds reference = ref bounds;
		Vector3 center = (bounds.size = Vector3.zero);
		reference.center = center;
		if (searchOptions.useSearchBox)
		{
			searchOptions.GetSearchBoxBounds();
		}
		InitAndResetLodParentsCount();
	}

	public void AbortAndClearMeshCombineJobs(bool executeAbortEvent = true)
	{
		foreach (MeshCombineJobManager.MeshCombineJob meshCombineJob in meshCombineJobs)
		{
			meshCombineJob.abort = true;
			meshCombineJob.meshCombiner.isCombining = false;
		}
		ClearMeshCombineJobs(executeAbortEvent);
	}

	public void ClearMeshCombineJobs(bool executeAbortEvent = true)
	{
		meshCombineJobs.Clear();
		totalMeshCombineJobs = 0;
		if (executeAbortEvent && this.onCombiningAbort != null)
		{
			this.onCombiningAbort(this);
		}
	}

	public void AddObjects(Transform rootT, List<Transform> transforms, bool useSearchOptions, bool checkForLODGroups = true)
	{
		List<LODGroup> list = new List<LODGroup>();
		if (checkForLODGroups)
		{
			for (int i = 0; i < transforms.Count; i++)
			{
				LODGroup component = transforms[i].GetComponent<LODGroup>();
				if (component != null)
				{
					list.Add(component);
				}
			}
			if (list.Count > 0)
			{
				AddLodGroups(rootT, list.ToArray(), useSearchOptions);
			}
		}
		AddTransforms(rootT, transforms.ToArray(), useSearchOptions);
	}

	public void AddObjectsAutomatically(bool useSearchConditions = true)
	{
		InitData();
		Reset();
		AddObjectsFromSearchParent(useSearchConditions);
		if (combineMode == CombineMode.DynamicObjects && data.foundLodObjects.Count > 0)
		{
			UnityEngine.Debug.Log("(MeshCombineStudio) => Lod Groups don't work yet for dynamic objects (they only work on static objects), this feature will be added in the next update.");
			data.foundLodObjects.Clear();
			return;
		}
		AddFoundObjectsToOctree();
		if (octreeContainsObjects)
		{
			octree.SortObjects(this);
			CombineCondition.MakeFoundReport(foundCombineConditions);
			cellCount = ObjectOctree.MaxCell.maxCellCount;
		}
		if (Console.instance != null)
		{
			LogOctreeInfo();
		}
	}

	public void AddFoundObjectsToOctree()
	{
		List<CachedGameObject> list = data.foundObjects;
		List<CachedLodGameObject> list2 = data.foundLodObjects;
		if (list.Count > 0 || list2.Count > 0)
		{
			octreeContainsObjects = true;
			CalcOctreeSize(bounds);
			ObjectOctree.MaxCell.maxCellCount = 0;
			for (int i = 0; i < list.Count; i++)
			{
				CachedGameObject cachedGameObject = list[i];
				Vector3 position = ((searchOptions.objectCenter == ObjectCenter.TransformPosition) ? cachedGameObject.t.position : cachedGameObject.mr.bounds.center);
				octree.AddObject(position, this, cachedGameObject, 0, 0);
			}
			for (int j = 0; j < list2.Count; j++)
			{
				CachedLodGameObject cachedLodGameObject = list2[j];
				octree.AddObject(cachedLodGameObject.center, this, cachedLodGameObject, cachedLodGameObject.lodCount, cachedLodGameObject.lodLevel);
			}
		}
		else
		{
			UnityEngine.Debug.Log("(MeshCombineStudio) => No matching GameObjects with chosen search options are found for combining.");
		}
	}

	public void ResetOctree()
	{
		octreeContainsObjects = false;
		if (octree == null)
		{
			octree = new ObjectOctree.Cell();
			return;
		}
		BaseOctree.Cell[] cells = octree.cells;
		BaseOctree.Cell[] cells2 = cells;
		octree.Reset(ref cells2);
	}

	public void CalcOctreeSize(Bounds bounds)
	{
		Methods.SnapBoundsAndPreserveArea(ref bounds, cellSize, (combineMode == CombineMode.StaticObjects) ? cellOffset : Vector3.zero);
		int num;
		float num2;
		if (combineMode == CombineMode.StaticObjects)
		{
			num = Mathf.CeilToInt(Mathf.Log(Mathf.Max(Mathw.GetMax(bounds.size), cellSize) / (float)cellSize, 2f));
			num2 = (int)Mathf.Pow(2f, num) * cellSize;
		}
		else
		{
			num2 = Mathw.GetMax(bounds.size);
			num = 0;
		}
		if (num == 0 && octree != null)
		{
			octree = new ObjectOctree.MaxCell();
		}
		else if (num > 0 && octree is ObjectOctree.MaxCell)
		{
			octree = new ObjectOctree.Cell();
		}
		octree.maxLevels = num;
		octree.bounds = new Bounds(bounds.center, new Vector3(num2, num2, num2));
	}

	public void ApplyChanges()
	{
		validRebakeLighting = rebakeLighting && !validCopyBakedLighting && !Application.isPlaying && Application.isEditor;
		for (int i = 0; i < changedCells.Count; i++)
		{
			ObjectOctree.MaxCell maxCell = changedCells[i];
			maxCell.hasChanged = false;
			maxCell.ApplyChanges(this);
		}
		changedCells.Clear();
	}

	public void CombineAll(bool useSearchConditions = true)
	{
		if (this.onCombiningStart != null)
		{
			this.onCombiningStart(this);
		}
		if (!combineConditionSettings.sameMaterial && combineConditionSettings.material == null)
		{
			UnityEngine.Debug.LogError("(MeshCombineStudio) => You need to assign an output material in 'Combine Conditions' => 'Change Materials'. Keep in mind with this setting you ignore the source materials and combine all meshes into 1 output material.");
			return;
		}
		if (removeBackFaceTriangles && backFaceTriangleMode == BackFaceTriangleMode.Transform)
		{
			if (backFaceT == null)
			{
				UnityEngine.Debug.LogError("(MeshCombineStudio) => You need to assign the BackFace Transform in 'Output Settings'.");
				return;
			}
			backFaceDirection = backFaceT.forward;
		}
		InitMeshCombineJobManager();
		isCombining = true;
		stopwatch.Reset();
		stopwatch.Start();
		addMeshCollidersList.Clear();
		unreadableMeshes.Clear();
		selectImportSettingsMeshes.Clear();
		AddObjectsAutomatically(useSearchConditions);
		if (!octreeContainsObjects)
		{
			return;
		}
		validRebakeLighting = rebakeLighting && !validCopyBakedLighting && !Application.isPlaying && Application.isEditor;
		newTotalVertices = (newTotalTriangles = (originalTotalVertices = (originalTotalTriangles = (originalDrawCalls = (newDrawCalls = 0)))));
		originalTotalNormalChannels = (originalTotalTangentChannels = (originalTotalUvChannels = (originalTotalUv2Channels = (originalTotalUv3Channels = (originalTotalUv4Channels = (originalTotalColorChannels = 0))))));
		newTotalNormalChannels = (newTotalTangentChannels = (newTotalUvChannels = (newTotalUv2Channels = (newTotalUv3Channels = (newTotalUv4Channels = (newTotalColorChannels = 0))))));
		for (int i = 0; i < lodParentHolders.Length; i++)
		{
			LodParentHolder lodParentHolder = lodParentHolders[i];
			if (lodParentHolder.found)
			{
				if (lodParentHolder.go == null && combineMode != CombineMode.DynamicObjects)
				{
					lodParentHolder.Create(this, i);
				}
				octree.CombineMeshes(this, i);
			}
		}
		if (MeshCombineJobManager.instance.jobSettings.combineJobMode == MeshCombineJobManager.CombineJobMode.CombineAtOnce)
		{
			MeshCombineJobManager.instance.ExecuteJobs();
		}
		ExecuteHandleObjects(active: false, HandleComponent.Disable, HandleComponent.Disable);
	}

	private void InitAndResetLodParentsCount()
	{
		for (int i = 0; i < lodParentHolders.Length; i++)
		{
			if (lodParentHolders[i].lods == null || lodParentHolders[i].lods.Length != i + 1)
			{
				lodParentHolders[i].Init(i + 1);
			}
			else
			{
				lodParentHolders[i].Reset();
			}
		}
	}

	public void AddObjectsFromSearchParent(bool useSearchConditions)
	{
		if (searchOptions.parentGOs == null || searchOptions.parentGOs.Length == 0)
		{
			UnityEngine.Debug.Log("(MeshCombineStudio) => You need to assign at least one Parent GameObject to 'Search Parents' in which meshes will be searched");
			return;
		}
		GameObject[] parentGOs = searchOptions.parentGOs;
		foreach (GameObject gameObject in parentGOs)
		{
			if (!(gameObject == null))
			{
				Transform searchParentT = gameObject.transform;
				LODGroup[] componentsInChildren = gameObject.GetComponentsInChildren<LODGroup>(includeInactive: true);
				AddLodGroups(searchParentT, componentsInChildren);
				Transform[] componentsInChildren2 = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
				AddTransforms(searchParentT, componentsInChildren2, useSearchConditions);
			}
		}
		List<CachedGameObject> list = data.foundObjects;
		List<LODGroup> list2 = data.foundLodGroups;
		List<Collider> list3 = data.foundColliders;
		Dictionary<Collider, CachedGameObject> colliderLookup = data.colliderLookup;
		Dictionary<LODGroup, CachedGameObject> lodGroupLookup = data.lodGroupLookup;
		if (!addMeshColliders)
		{
			return;
		}
		for (int j = 0; j < list.Count; j++)
		{
			Collider[] componentsInChildren3 = list[j].go.GetComponentsInChildren<Collider>(includeInactive: false);
			foreach (Collider collider in componentsInChildren3)
			{
				if (!colliderLookup.ContainsKey(collider))
				{
					list3.Add(collider);
					colliderLookup.Add(collider, list[j]);
				}
			}
		}
		for (int l = 0; l < list2.Count; l++)
		{
			LODGroup key = list2[l];
			Collider[] componentsInChildren4 = list2[l].gameObject.GetComponentsInChildren<Collider>(includeInactive: false);
			foreach (Collider collider2 in componentsInChildren4)
			{
				if (!colliderLookup.ContainsKey(collider2))
				{
					list3.Add(collider2);
					colliderLookup.Add(collider2, lodGroupLookup[key]);
				}
			}
		}
	}

	private void CheckForFoundObjectNotOnOverlapLayerMask(GameObject go)
	{
		if (!Methods.IsLayerInLayerMask(overlapLayerMask, go.layer))
		{
			UnityEngine.Debug.LogError("(MeshCombineStudio) => " + go.name + " on layer " + LayerMask.LayerToName(go.layer) + " is not part of the Overlap LayerMask", go);
		}
	}

	private void AddLodGroups(Transform searchParentT, LODGroup[] lodGroups, bool useSearchOptions = true)
	{
		List<CachedLodGameObject> list = new List<CachedLodGameObject>();
		CachedGameObject cachedGameObject = null;
		for (int i = 0; i < lodGroups.Length; i++)
		{
			LODGroup lODGroup = lodGroups[i];
			bool flag;
			if (searchOptions.lodGroupSearchMode == SearchOptions.LODGroupSearchMode.LodGroup)
			{
				flag = ValidObject(searchParentT, lODGroup.transform, ObjectType.LodGroup, useSearchOptions, ref cachedGameObject) == 1;
			}
			else
			{
				if (searchOptions.onlyActive && !lODGroup.gameObject.activeInHierarchy)
				{
					continue;
				}
				flag = true;
			}
			LOD[] lODs = lODGroup.GetLODs();
			int num = lODs.Length - 1;
			if (num <= 0)
			{
				continue;
			}
			if (i == 0)
			{
				lodGroupsSettings[num].CopyFromLodGroup(lODGroup, lODs);
			}
			Vector3 center = Vector3.zero;
			int num2 = 0;
			for (int j = 0; j < lODs.Length; j++)
			{
				LOD lOD = lODs[j];
				for (int k = 0; k < lOD.renderers.Length; k++)
				{
					Renderer renderer = lOD.renderers[k];
					if (!renderer)
					{
						continue;
					}
					if (flag)
					{
						CachedGameObject cachedGameObject2 = null;
						int num3 = ValidObject(searchParentT, renderer.transform, ObjectType.LodRenderer, useSearchOptions, ref cachedGameObject2);
						if (num3 == -1)
						{
							continue;
						}
						if (num3 == -2)
						{
							goto IL_00ed;
						}
						if (removeOverlappingTriangles && reportFoundObjectsNotOnOverlapLayerMask)
						{
							CheckForFoundObjectNotOnOverlapLayerMask(cachedGameObject2.go);
						}
						list.Add(new CachedLodGameObject(cachedGameObject2, num, j));
						if (searchOptions.objectCenter == ObjectCenter.BoundsCenter)
						{
							center += cachedGameObject2.mr.bounds.center;
							num2++;
						}
					}
					uniqueLodObjects.Add(renderer.transform);
				}
				continue;
				IL_00ed:
				list.Clear();
				break;
			}
			if (list.Count <= 0)
			{
				continue;
			}
			if (searchOptions.objectCenter == ObjectCenter.BoundsCenter)
			{
				center /= (float)num2;
			}
			else
			{
				center = lODGroup.transform.position;
			}
			List<CachedLodGameObject> list2 = data.foundLodObjects;
			for (int l = 0; l < list.Count; l++)
			{
				CachedLodGameObject cachedLodGameObject = list[l];
				if (l == 0)
				{
					data.lodGroupLookup[lODGroup] = cachedLodGameObject;
				}
				cachedLodGameObject.center = center;
				if (!hasFoundFirstObject)
				{
					bounds.center = cachedLodGameObject.mr.bounds.center;
					hasFoundFirstObject = true;
				}
				bounds.Encapsulate(cachedLodGameObject.mr.bounds);
				list2.Add(cachedLodGameObject);
				lodParentHolders[num].found = true;
				lodParentHolders[num].lods[cachedLodGameObject.lodLevel]++;
			}
			uniqueFoundLodGroups.Add(lODGroup);
			list.Clear();
		}
		data.foundLodGroups = new List<LODGroup>(uniqueFoundLodGroups);
	}

	private void AddTransforms(Transform searchParentT, Transform[] transforms, bool useSearchConditions = true)
	{
		int count = uniqueLodObjects.Count;
		List<CachedGameObject> list = data.foundObjects;
		foreach (Transform transform in transforms)
		{
			if (count > 0 && uniqueLodObjects.Contains(transform))
			{
				continue;
			}
			CachedGameObject cachedGameObject = null;
			if (ValidObject(searchParentT, transform, ObjectType.Normal, useSearchConditions, ref cachedGameObject) == 1)
			{
				if (removeOverlappingTriangles && reportFoundObjectsNotOnOverlapLayerMask)
				{
					CheckForFoundObjectNotOnOverlapLayerMask(cachedGameObject.go);
				}
				if (!hasFoundFirstObject)
				{
					bounds.center = cachedGameObject.mr.bounds.center;
					hasFoundFirstObject = true;
				}
				bounds.Encapsulate(cachedGameObject.mr.bounds);
				list.Add(cachedGameObject);
				lodParentHolders[0].lods[0]++;
			}
		}
		if (list.Count > 0)
		{
			lodParentHolders[0].found = true;
		}
	}

	private int ValidObject(Transform searchParentT, Transform t, ObjectType objectType, bool useSearchOptions, ref CachedGameObject cachedGameObject)
	{
		if (t == null)
		{
			return -1;
		}
		GameObject gameObject = t.gameObject;
		MeshRenderer meshRenderer = null;
		MeshFilter meshFilter = null;
		Mesh mesh = null;
		if (objectType != ObjectType.LodGroup || searchOptions.lodGroupSearchMode == SearchOptions.LODGroupSearchMode.LodRenderers)
		{
			meshRenderer = t.GetComponent<MeshRenderer>();
			if (meshRenderer == null || (!meshRenderer.enabled && searchOptions.onlyActiveMeshRenderers))
			{
				return -1;
			}
			meshFilter = t.GetComponent<MeshFilter>();
			if (meshFilter == null)
			{
				return -1;
			}
			mesh = meshFilter.sharedMesh;
			if (mesh == null)
			{
				return -1;
			}
			if (mesh.vertexCount > 65534)
			{
				return -2;
			}
		}
		if (useSearchOptions)
		{
			if (searchOptions.onlyActive && !gameObject.activeInHierarchy)
			{
				return -1;
			}
			if (objectType != ObjectType.LodRenderer || searchOptions.lodGroupSearchMode == SearchOptions.LODGroupSearchMode.LodRenderers)
			{
				if (searchOptions.useLayerMask)
				{
					int num = 1 << t.gameObject.layer;
					if ((searchOptions.layerMask.value & num) != num)
					{
						return -1;
					}
				}
				if (searchOptions.onlyStatic && !gameObject.isStatic)
				{
					return -1;
				}
				if (searchOptions.useTag && !t.CompareTag(searchOptions.tag))
				{
					return -1;
				}
				if (searchOptions.useComponentsFilter)
				{
					if (searchOptions.componentCondition == SearchOptions.ComponentCondition.And)
					{
						bool flag = true;
						for (int i = 0; i < searchOptions.componentNameList.Count; i++)
						{
							if (t.GetComponent(searchOptions.componentNameList[i]) == null)
							{
								flag = false;
								break;
							}
						}
						if (!flag)
						{
							return -1;
						}
					}
					else if (searchOptions.componentCondition == SearchOptions.ComponentCondition.Or)
					{
						bool flag2 = false;
						for (int j = 0; j < searchOptions.componentNameList.Count; j++)
						{
							if (t.GetComponent(searchOptions.componentNameList[j]) != null)
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							return -1;
						}
					}
					else
					{
						bool flag3 = true;
						for (int k = 0; k < searchOptions.componentNameList.Count; k++)
						{
							if (t.GetComponent(searchOptions.componentNameList[k]) != null)
							{
								flag3 = false;
								break;
							}
						}
						if (!flag3)
						{
							return -1;
						}
					}
				}
				if (searchOptions.useNameContains)
				{
					bool flag4 = false;
					for (int l = 0; l < searchOptions.nameContainList.Count; l++)
					{
						if (Methods.Contains(t.name, searchOptions.nameContainList[l]))
						{
							flag4 = true;
							break;
						}
					}
					if (!flag4)
					{
						return -1;
					}
				}
				if (searchOptions.useSearchBox)
				{
					if (searchOptions.objectCenter == ObjectCenter.BoundsCenter)
					{
						if (!searchOptions.searchBoxBounds.Contains(meshRenderer.bounds.center))
						{
							return -2;
						}
					}
					else if (!searchOptions.searchBoxBounds.Contains(t.position))
					{
						return -2;
					}
				}
			}
			if (objectType != ObjectType.LodGroup)
			{
				if (searchOptions.useVertexInputLimit && mesh.vertexCount > searchOptions.vertexInputLimit)
				{
					return -2;
				}
				if (useVertexOutputLimit && mesh.vertexCount > vertexOutputLimit)
				{
					return -2;
				}
				if (searchOptions.useMaxBoundsFactor && combineMode == CombineMode.StaticObjects && Mathw.GetMax(meshRenderer.bounds.size) > (float)cellSize * searchOptions.maxBoundsFactor)
				{
					return -2;
				}
			}
		}
		if ((objectType != ObjectType.LodGroup || searchOptions.lodGroupSearchMode == SearchOptions.LODGroupSearchMode.LodRenderers) && !mesh.isReadable)
		{
			if (unreadableMeshes.Add(mesh))
			{
				UnityEngine.Debug.LogError("(MeshCombineStudio) => Read/Write is disabled on the mesh on GameObject " + gameObject.name + " and can't be combined. Click the 'Make Meshes Readable' in the MCS Inspector to make it automatically readable in the mesh import settings.");
			}
			return -1;
		}
		if (objectType != ObjectType.LodGroup)
		{
			cachedGameObject = new CachedGameObject(searchParentT, gameObject, t, meshRenderer, meshFilter, mesh);
		}
		return 1;
	}

	public void RestoreOriginalRenderersAndLODGroups(bool onDestroy)
	{
		if (!activeOriginal)
		{
			ExecuteHandleObjects(active: true, HandleComponent.Disable, HandleComponent.Disable, includeColliders: true, onDestroy);
		}
	}

	public void SwapCombine()
	{
		if (!combined)
		{
			CombineAll();
			return;
		}
		combinedActive = !combinedActive;
		ExecuteHandleObjects(!combinedActive, originalMeshRenderers, originalLODGroups);
	}

	private void SetOriginalCollidersActive(bool active, bool onDestroy)
	{
		if (data == null && !onDestroy)
		{
			InitData();
		}
		if (data == null)
		{
			return;
		}
		List<Collider> list = data.foundColliders;
		for (int i = 0; i < list.Count; i++)
		{
			Collider collider = list[i];
			if ((bool)collider)
			{
				data.colliderLookup.TryGetValue(collider, out var value);
				if (value == null || !value.excludeCombine)
				{
					collider.enabled = active;
				}
				else
				{
					Methods.ListRemoveAt(list, i--);
				}
			}
			else
			{
				Methods.ListRemoveAt(list, i--);
			}
		}
	}

	private void ExecuteMeshFilter(bool active, CachedGameObject cachedGO)
	{
		if (active)
		{
			if ((bool)cachedGO.mfr)
			{
				cachedGO.mfr.RevertMeshFilter(cachedGO.mf);
			}
			return;
		}
		MeshFilterRevert meshFilterRevert = cachedGO.go.AddComponent<MeshFilterRevert>();
		if (meshFilterRevert.DestroyAndReferenceMeshFilter(cachedGO.mf))
		{
			cachedGO.mfr = meshFilterRevert;
		}
		else
		{
			Methods.Destroy(meshFilterRevert);
		}
	}

	public void ExecuteHandleObjects(bool active, HandleComponent handleOriginalObjects, HandleComponent handleOriginalLodGroups, bool includeColliders = true, bool onDestroy = false)
	{
		activeOriginal = active;
		Methods.SetChildrenActive(base.transform, !active);
		bool flag = !Application.isPlaying && removeOriginalMeshReference;
		List<CachedGameObject> list;
		List<CachedLodGameObject> list2;
		List<LODGroup> list3;
		List<Collider> list4;
		if (onDestroy)
		{
			list = foundObjects;
			list2 = foundLodObjects;
			list3 = foundLodGroups;
			list4 = foundColliders;
		}
		else
		{
			InitData();
			if (data == null)
			{
				return;
			}
			list = data.foundObjects;
			list2 = data.foundLodObjects;
			list3 = data.foundLodGroups;
			list4 = data.foundColliders;
		}
		if (handleOriginalObjects == HandleComponent.Disable)
		{
			if (includeColliders)
			{
				SetOriginalCollidersActive(active, onDestroy);
			}
			for (int i = 0; i < list.Count; i++)
			{
				CachedGameObject cachedGameObject = list[i];
				if ((bool)cachedGameObject.mr && !cachedGameObject.excludeCombine)
				{
					cachedGameObject.mr.enabled = cachedGameObject.mrEnabled && active;
					if (flag)
					{
						ExecuteMeshFilter(active, cachedGameObject);
					}
				}
				else
				{
					Methods.ListRemoveAt(list, i--);
				}
			}
			for (int j = 0; j < list2.Count; j++)
			{
				CachedLodGameObject cachedLodGameObject = list2[j];
				if ((bool)cachedLodGameObject.mr && !cachedLodGameObject.excludeCombine)
				{
					cachedLodGameObject.mr.enabled = cachedLodGameObject.mrEnabled && active;
					if (flag)
					{
						ExecuteMeshFilter(active, cachedLodGameObject);
					}
				}
				else
				{
					Methods.ListRemoveAt(list2, j--);
				}
			}
		}
		if (handleOriginalObjects == HandleComponent.Destroy)
		{
			for (int k = 0; k < list4.Count; k++)
			{
				Collider collider = list4[k];
				if ((bool)collider)
				{
					data.colliderLookup.TryGetValue(collider, out var value);
					if (value == null || !value.excludeCombine)
					{
						UnityEngine.Object.Destroy(collider);
					}
					else
					{
						Methods.ListRemoveAt(list4, k--);
					}
				}
				else
				{
					Methods.ListRemoveAt(list4, k--);
				}
			}
			for (int l = 0; l < list.Count; l++)
			{
				bool flag2 = false;
				CachedGameObject cachedGameObject2 = list[l];
				if (!cachedGameObject2.excludeCombine)
				{
					if ((bool)cachedGameObject2.mf)
					{
						UnityEngine.Object.Destroy(cachedGameObject2.mf);
					}
					else
					{
						flag2 = true;
					}
					if ((bool)cachedGameObject2.mr)
					{
						UnityEngine.Object.Destroy(cachedGameObject2.mr);
					}
					else
					{
						flag2 = true;
					}
				}
				else
				{
					flag2 = true;
				}
				if (flag2)
				{
					Methods.ListRemoveAt(list, l--);
				}
			}
			for (int m = 0; m < list2.Count; m++)
			{
				bool flag3 = false;
				CachedGameObject cachedGameObject3 = list2[m];
				if (!cachedGameObject3.excludeCombine)
				{
					if ((bool)cachedGameObject3.mf)
					{
						UnityEngine.Object.Destroy(cachedGameObject3.mf);
					}
					else
					{
						flag3 = true;
					}
					if ((bool)cachedGameObject3.mr)
					{
						UnityEngine.Object.Destroy(cachedGameObject3.mr);
					}
					else
					{
						flag3 = true;
					}
				}
				else
				{
					flag3 = true;
				}
				if (flag3)
				{
					Methods.ListRemoveAt(list2, m--);
				}
			}
		}
		for (int n = 0; n < list3.Count; n++)
		{
			LODGroup lODGroup = list3[n];
			if ((bool)lODGroup)
			{
				data.lodGroupLookup.TryGetValue(lODGroup, out var value2);
				if (value2 == null || !value2.excludeCombine)
				{
					if (handleOriginalLodGroups == HandleComponent.Disable)
					{
						lODGroup.enabled = active;
					}
					else
					{
						UnityEngine.Object.Destroy(lODGroup);
					}
				}
				else
				{
					Methods.ListRemoveAt(list3, n--);
				}
			}
			else
			{
				Methods.ListRemoveAt(list3, n--);
			}
		}
	}

	private void DrawGizmosCube(Bounds bounds, Color color)
	{
		Gizmos.color = color;
		Gizmos.DrawWireCube(bounds.center, bounds.size);
		Gizmos.color = new Color(color.r, color.g, color.b, 0.5f);
		Gizmos.DrawCube(bounds.center, bounds.size);
		Gizmos.color = Color.white;
	}

	private void OnDrawGizmosSelected()
	{
		if (addMeshColliders && addMeshCollidersInRange)
		{
			DrawGizmosCube(addMeshCollidersBounds, Color.green);
		}
		if (removeBackFaceTriangles && backFaceTriangleMode == BackFaceTriangleMode.Box)
		{
			DrawGizmosCube(backFaceBounds, Color.blue);
		}
		if (drawGizmos)
		{
			if (octree != null && octreeContainsObjects)
			{
				octree.Draw(this, onlyMaxLevel: true, !searchOptions.useSearchBox);
			}
			if (searchOptions.useSearchBox)
			{
				searchOptions.GetSearchBoxBounds();
				Gizmos.color = Color.green;
				Gizmos.DrawWireCube(searchOptions.searchBoxBounds.center, searchOptions.searchBoxBounds.size);
				Gizmos.color = Color.white;
			}
		}
	}

	private void LogOctreeInfo()
	{
		Console.Log("Cells " + ObjectOctree.MaxCell.maxCellCount + " -> Found Objects: ");
		LodParentHolder[] array = lodParentHolders;
		if (array == null || array.Length == 0)
		{
			return;
		}
		for (int i = 0; i < array.Length; i++)
		{
			LodParentHolder lodParentHolder = array[i];
			if (lodParentHolder.found)
			{
				string text = "";
				text = "LOD Group " + (i + 1) + " |";
				int[] lods = lodParentHolder.lods;
				for (int j = 0; j < lods.Length; j++)
				{
					text = text + " " + lods[j] + " |";
				}
				Console.Log(text);
			}
		}
	}
}
