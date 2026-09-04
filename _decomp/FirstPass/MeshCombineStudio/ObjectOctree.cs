using System.Collections.Generic;
using UnityEngine;

namespace MeshCombineStudio;

public class ObjectOctree
{
	public class LODParent
	{
		public GameObject cellGO;

		public Transform cellT;

		public LODGroup lodGroup;

		public LODLevel[] lodLevels;

		public bool hasChanged;

		public int jobsPending;

		public LODParent(int lodCount)
		{
			lodLevels = new LODLevel[lodCount];
			for (int i = 0; i < lodLevels.Length; i++)
			{
				lodLevels[i] = new LODLevel();
			}
		}

		public void AssignLODGroup(MeshCombiner meshCombiner)
		{
			LOD[] array = new LOD[lodLevels.Length];
			int num = array.Length - 1;
			for (int i = 0; i < lodLevels.Length; i++)
			{
				LODLevel lODLevel = lodLevels[i];
				int num2 = i;
				float screenRelativeTransitionHeight = meshCombiner.lodGroupsSettings[num].lodSettings[i].screenRelativeTransitionHeight;
				Renderer[] renderers = lODLevel.newMeshRenderers.ToArray();
				array[num2] = new LOD(screenRelativeTransitionHeight, renderers);
			}
			lodGroup.SetLODs(array);
			lodGroup.size = meshCombiner.cellSize;
			meshCombiner.lodGroupsSettings[num].CopyToLodGroup(lodGroup, array);
		}

		public void ApplyChanges(MeshCombiner meshCombiner)
		{
			for (int i = 0; i < lodLevels.Length; i++)
			{
				lodLevels[i].ApplyChanges(meshCombiner);
			}
			hasChanged = false;
		}
	}

	public class LODLevel
	{
		public FastList<CachedGameObject> cachedGOs = new FastList<CachedGameObject>();

		public Dictionary<CombineCondition, MeshObjectsHolder> meshObjectsHoldersLookup;

		public FastList<MeshObjectsHolder> changedMeshObjectsHolders;

		public FastList<MeshRenderer> newMeshRenderers = new FastList<MeshRenderer>();

		public int vertCount;

		public int objectCount;

		public void ApplyChanges(MeshCombiner meshCombiner)
		{
			for (int i = 0; i < changedMeshObjectsHolders.Count; i++)
			{
				changedMeshObjectsHolders.items[i].hasChanged = false;
			}
			changedMeshObjectsHolders.Clear();
		}
	}

	public class MaxCell : Cell
	{
		public static int maxCellCount;

		public LODParent[] lodParents;

		public List<LODParent> changedLodParents;

		public bool hasChanged;

		public void ApplyChanges(MeshCombiner meshCombiner)
		{
			for (int i = 0; i < changedLodParents.Count; i++)
			{
				changedLodParents[i].ApplyChanges(meshCombiner);
			}
			changedLodParents.Clear();
			hasChanged = false;
		}
	}

	public class Cell : BaseOctree.Cell
	{
		public Cell[] cells;

		public Cell()
		{
		}

		public Cell(Vector3 position, Vector3 size, int maxLevels)
			: base(position, size, maxLevels)
		{
		}

		public MaxCell GetCell(Vector3 position)
		{
			if (!InsideBounds(position))
			{
				return null;
			}
			return GetCellInternal(position);
		}

		private MaxCell GetCellInternal(Vector3 position)
		{
			if (level == maxLevels)
			{
				return (MaxCell)this;
			}
			return GetCell(cells, position)?.GetCellInternal(position);
		}

		public CachedGameObject AddObject(Vector3 position, MeshCombiner meshCombiner, CachedGameObject cachedGO, int lodParentIndex, int lodLevel, bool isChangeMode = false)
		{
			if (InsideBounds(position))
			{
				AddObjectInternal(meshCombiner, cachedGO, position, lodParentIndex, lodLevel, isChangeMode);
				return cachedGO;
			}
			return null;
		}

		private void AddObjectInternal(MeshCombiner meshCombiner, CachedGameObject cachedGO, Vector3 position, int lodParentIndex, int lodLevel, bool isChangeMode)
		{
			if (level == maxLevels)
			{
				MaxCell maxCell = (MaxCell)this;
				if (maxCell.lodParents == null)
				{
					maxCell.lodParents = new LODParent[10];
				}
				if (maxCell.lodParents[lodParentIndex] == null)
				{
					maxCell.lodParents[lodParentIndex] = new LODParent(lodParentIndex + 1);
				}
				LODParent lODParent = maxCell.lodParents[lodParentIndex];
				LODLevel lODLevel = lODParent.lodLevels[lodLevel];
				lODLevel.cachedGOs.Add(cachedGO);
				if (isChangeMode && SortObject(meshCombiner, lODLevel, cachedGO))
				{
					if (!maxCell.hasChanged)
					{
						maxCell.hasChanged = true;
						if (meshCombiner.changedCells == null)
						{
							meshCombiner.changedCells = new List<MaxCell>();
						}
						meshCombiner.changedCells.Add(maxCell);
					}
					if (!lODParent.hasChanged)
					{
						lODParent.hasChanged = true;
						maxCell.changedLodParents.Add(lODParent);
					}
				}
				lODLevel.objectCount++;
				lODLevel.vertCount += cachedGO.mesh.vertexCount;
			}
			else
			{
				bool maxCellCreated;
				int num = AddCell<Cell, MaxCell>(ref cells, position, out maxCellCreated);
				if (maxCellCreated)
				{
					MaxCell.maxCellCount++;
				}
				cells[num].AddObjectInternal(meshCombiner, cachedGO, position, lodParentIndex, lodLevel, isChangeMode);
			}
		}

		public void SortObjects(MeshCombiner meshCombiner)
		{
			if (level == maxLevels)
			{
				LODParent[] lodParents = ((MaxCell)this).lodParents;
				foreach (LODParent lODParent in lodParents)
				{
					if (lODParent == null)
					{
						continue;
					}
					for (int j = 0; j < lODParent.lodLevels.Length; j++)
					{
						LODLevel lODLevel = lODParent.lodLevels[j];
						if (lODLevel == null || lODLevel.cachedGOs.Count == 0)
						{
							return;
						}
						for (int k = 0; k < lODLevel.cachedGOs.Count; k++)
						{
							CachedGameObject cachedGO = lODLevel.cachedGOs.items[k];
							if (!SortObject(meshCombiner, lODLevel, cachedGO))
							{
								lODLevel.cachedGOs.RemoveAt(k--);
							}
						}
					}
				}
				return;
			}
			for (int l = 0; l < 8; l++)
			{
				if (cellsUsed[l])
				{
					cells[l].SortObjects(meshCombiner);
				}
			}
		}

		public bool SortObject(MeshCombiner meshCombiner, LODLevel lod, CachedGameObject cachedGO, bool isChangeMode = false)
		{
			if (cachedGO.mr == null)
			{
				return false;
			}
			if (lod.meshObjectsHoldersLookup == null)
			{
				lod.meshObjectsHoldersLookup = new Dictionary<CombineCondition, MeshObjectsHolder>();
			}
			CombineConditionSettings combineConditionSettings = meshCombiner.combineConditionSettings;
			Material[] sharedMaterials = cachedGO.mr.sharedMaterials;
			int num = Mathf.Min(cachedGO.mesh.subMeshCount, sharedMaterials.Length);
			int num2 = -1;
			if (meshCombiner.combineMode == CombineMode.DynamicObjects)
			{
				num2 = cachedGO.rootInstanceId;
				if (num2 == -1)
				{
					cachedGO.GetRoot();
					num2 = cachedGO.rootInstanceId;
				}
			}
			for (int i = 0; i < num; i++)
			{
				Material material;
				if (combineConditionSettings.sameMaterial)
				{
					material = sharedMaterials[i];
					if (material == null)
					{
						continue;
					}
				}
				else
				{
					material = combineConditionSettings.material;
				}
				CombineCondition combineCondition = default(CombineCondition);
				combineCondition.ReadFromGameObject(num2, combineConditionSettings, meshCombiner.copyBakedLighting && meshCombiner.validCopyBakedLighting, cachedGO.go, cachedGO.t, cachedGO.mr, material);
				if (!lod.meshObjectsHoldersLookup.TryGetValue(combineCondition, out var value))
				{
					meshCombiner.foundCombineConditions.combineConditions.Add(combineCondition);
					value = new MeshObjectsHolder(ref combineCondition, material);
					lod.meshObjectsHoldersLookup.Add(combineCondition, value);
				}
				value.meshObjects.Add(new MeshObject(cachedGO, i));
				if (isChangeMode && !value.hasChanged)
				{
					value.hasChanged = true;
					lod.changedMeshObjectsHolders.Add(value);
				}
			}
			return true;
		}

		public void CombineMeshes(MeshCombiner meshCombiner, int lodParentIndex)
		{
			if (level == maxLevels)
			{
				LODParent lODParent = ((MaxCell)this).lodParents[lodParentIndex];
				if (lODParent == null)
				{
					return;
				}
				CombineMode combineMode = meshCombiner.combineMode;
				if (combineMode != CombineMode.DynamicObjects)
				{
					lODParent.cellGO = new GameObject((meshCombiner.combineMode == CombineMode.StaticObjects) ? ("Cell " + bounds.center) : "Combined Objects");
					lODParent.cellT = lODParent.cellGO.transform;
					lODParent.cellT.position = bounds.center;
					lODParent.cellT.parent = meshCombiner.lodParentHolders[lodParentIndex].t;
				}
				if (lodParentIndex > 0)
				{
					lODParent.lodGroup = lODParent.cellGO.AddComponent<LODGroup>();
					LODGroup lodGroup = lODParent.lodGroup;
					Vector3 localReferencePoint = (lODParent.cellT.position = bounds.center);
					lodGroup.localReferencePoint = localReferencePoint;
				}
				LODLevel[] lodLevels = lODParent.lodLevels;
				for (int i = 0; i < lodLevels.Length; i++)
				{
					LODLevel lODLevel = lODParent.lodLevels[i];
					if (lODLevel == null || lODLevel.meshObjectsHoldersLookup == null)
					{
						break;
					}
					Transform transform = null;
					if (lodParentIndex > 0)
					{
						transform = new GameObject("LOD" + i).transform;
						transform.parent = lODParent.cellT;
					}
					foreach (MeshObjectsHolder value in lODLevel.meshObjectsHoldersLookup.Values)
					{
						value.lodParent = lODParent;
						value.lodLevel = i;
						Vector3 position = ((combineMode == CombineMode.DynamicObjects) ? value.meshObjects.items[0].cachedGO.rootT.position : bounds.center);
						MeshCombineJobManager.instance.AddJob(meshCombiner, value, (lodParentIndex > 0) ? transform : lODParent.cellT, position);
					}
				}
				return;
			}
			for (int j = 0; j < 8; j++)
			{
				if (cellsUsed[j])
				{
					cells[j].CombineMeshes(meshCombiner, lodParentIndex);
				}
			}
		}

		public void Draw(MeshCombiner meshCombiner, bool onlyMaxLevel, bool drawLevel0)
		{
			if (!onlyMaxLevel || level == maxLevels || (drawLevel0 && level == 0))
			{
				Gizmos.DrawWireCube(base.bounds.center, base.bounds.size);
				if (level == maxLevels && meshCombiner.drawMeshBounds)
				{
					LODParent[] lodParents = ((MaxCell)this).lodParents;
					for (int i = 0; i < lodParents.Length; i++)
					{
						if (lodParents[i] == null)
						{
							continue;
						}
						LODLevel[] lodLevels = lodParents[i].lodLevels;
						Gizmos.color = (meshCombiner.activeOriginal ? Color.blue : Color.green);
						for (int j = 0; j < lodLevels.Length; j++)
						{
							for (int k = 0; k < lodLevels[j].cachedGOs.Count; k++)
							{
								if (!(lodLevels[j].cachedGOs.items[k].mr == null))
								{
									Bounds bounds = lodLevels[j].cachedGOs.items[k].mr.bounds;
									Gizmos.DrawWireCube(bounds.center, bounds.size);
								}
							}
						}
						Gizmos.color = Color.white;
					}
					return;
				}
			}
			if (cells == null || cellsUsed == null)
			{
				return;
			}
			for (int l = 0; l < 8; l++)
			{
				if (cellsUsed[l])
				{
					cells[l].Draw(meshCombiner, onlyMaxLevel, drawLevel0);
				}
			}
		}
	}
}
