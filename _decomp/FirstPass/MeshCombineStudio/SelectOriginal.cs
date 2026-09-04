using UnityEngine;

namespace MeshCombineStudio;

public class SelectOriginal : MonoBehaviour
{
	public MeshCombiner meshCombiner;

	public Camera mainCamera;

	public Material matSelect;

	private Material oldMat;

	private Vector3 oldPos;

	private MeshRenderer oldMr;

	private void Update()
	{
		Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			Deselect();
		}
		if (Physics.Raycast(ray, out var hitInfo))
		{
			Transform transform = hitInfo.transform;
			MeshRenderer component = transform.GetComponent<MeshRenderer>();
			if (component != null && !(component == oldMr))
			{
				Deselect();
				oldMr = component;
				if (meshCombiner.searchOptions.objectCenter == MeshCombiner.ObjectCenter.BoundsCenter)
				{
					oldPos = oldMr.bounds.center;
				}
				else
				{
					oldPos = transform.position;
				}
				oldMat = oldMr.sharedMaterial;
				SelectOrDeselect(oldPos, oldMr, matSelect, select: true);
			}
		}
		else
		{
			Deselect();
		}
	}

	private void Deselect()
	{
		if (oldMr != null)
		{
			SelectOrDeselect(oldPos, oldMr, oldMat, select: false);
		}
	}

	private void SelectOrDeselect(Vector3 position, MeshRenderer mr, Material mat, bool select)
	{
		ObjectOctree.Cell octree = meshCombiner.octree;
		if (octree == null)
		{
			return;
		}
		ObjectOctree.MaxCell cell = octree.GetCell(position);
		if (cell == null)
		{
			return;
		}
		mr.sharedMaterial = mat;
		if (meshCombiner.activeOriginal)
		{
			return;
		}
		ObjectOctree.LODParent[] lodParents = cell.lodParents;
		foreach (ObjectOctree.LODParent lODParent in lodParents)
		{
			if (lODParent == null)
			{
				continue;
			}
			ObjectOctree.LODLevel[] lodLevels = lODParent.lodLevels;
			foreach (ObjectOctree.LODLevel lODLevel in lodLevels)
			{
				if (lODLevel != null)
				{
					Methods.SetMeshRenderersActive(lODLevel.newMeshRenderers, !select);
					Methods.SetCachedGOSActive(lODLevel.cachedGOs, select);
				}
			}
		}
	}
}
