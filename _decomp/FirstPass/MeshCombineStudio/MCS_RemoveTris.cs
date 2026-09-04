using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeshCombineStudio;

[DefaultExecutionOrder(-99999999)]
[ExecuteInEditMode]
public abstract class MCS_RemoveTris : MonoBehaviour
{
	private HashSet<GameObjectLayer> gos = new HashSet<GameObjectLayer>();

	private bool hasRegistered;

	private void Awake()
	{
		Register(first: true);
	}

	private void OnEnable()
	{
		Register(first: false);
	}

	private void Register(bool first)
	{
		if (hasRegistered)
		{
			return;
		}
		if (first)
		{
			if (MeshCombiner.instances.Count == 0)
			{
				return;
			}
			for (int i = 0; i < MeshCombiner.instances.Count; i++)
			{
				Init(MeshCombiner.instances[i]);
			}
		}
		else
		{
			MeshCombiner.onInit = (MeshCombiner.EventMethod)Delegate.Combine(MeshCombiner.onInit, new MeshCombiner.EventMethod(Init));
		}
		hasRegistered = true;
	}

	private void Init(MeshCombiner meshCombiner)
	{
		meshCombiner.onCombiningStart += OnCombine;
		meshCombiner.onCombiningAbort += OnCombineReady;
		meshCombiner.onCombiningReady += OnCombineReady;
	}

	private void OnDisable()
	{
		Unregister();
	}

	private void OnDestroy()
	{
		Unregister();
	}

	private void Unregister()
	{
		if (hasRegistered)
		{
			hasRegistered = false;
			OnCombineReady(null);
			MeshCombiner.onInit = (MeshCombiner.EventMethod)Delegate.Remove(MeshCombiner.onInit, new MeshCombiner.EventMethod(Init));
			for (int i = 0; i < MeshCombiner.instances.Count; i++)
			{
				MeshCombiner meshCombiner = MeshCombiner.instances[i];
				meshCombiner.onCombiningStart -= OnCombine;
				meshCombiner.onCombiningAbort -= OnCombineReady;
				meshCombiner.onCombiningReady -= OnCombineReady;
			}
		}
	}

	private void OnCombine(MeshCombiner meshCombiner)
	{
		if (gos.Count > 0)
		{
			OnCombineReady(null);
		}
		int num = ((!(this is MCS_RemoveTrisBelowSurface)) ? Methods.GetFirstLayerInLayerMask(meshCombiner.overlapLayerMask) : Methods.GetFirstLayerInLayerMask(meshCombiner.surfaceLayerMask));
		if (num != -1)
		{
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				GameObject gameObject = componentsInChildren[i].gameObject;
				gos.Add(new GameObjectLayer(gameObject));
				gameObject.layer = num;
			}
		}
	}

	private void OnCombineReady(MeshCombiner meshCombiner)
	{
		foreach (GameObjectLayer go in gos)
		{
			go.RestoreLayer();
		}
		gos.Clear();
	}
}
