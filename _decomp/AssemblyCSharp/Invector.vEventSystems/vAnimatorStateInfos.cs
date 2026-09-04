using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vEventSystems;

[Serializable]
public class vAnimatorStateInfos
{
	[Serializable]
	public class vStateInfo
	{
		public int layer;

		public int shortPathHash;

		public float normalizedTime;

		public List<string> tags = new List<string>();

		public vStateInfo(int layer)
		{
			this.layer = layer;
		}
	}

	public bool debug;

	public Animator animator;

	public vStateInfo[] stateInfos = new vStateInfo[0];

	public vAnimatorStateInfos(Animator animator)
	{
		this.animator = animator;
		Init();
	}

	public void Init()
	{
		if ((bool)animator)
		{
			stateInfos = new vStateInfo[animator.layerCount];
			for (int i = 0; i < stateInfos.Length; i++)
			{
				stateInfos[i] = new vStateInfo(i);
			}
		}
	}

	public void RegisterListener()
	{
		vAnimatorTagBase[] behaviours = animator.GetBehaviours<vAnimatorTagBase>();
		for (int i = 0; i < behaviours.Length; i++)
		{
			behaviours[i].RemoveStateInfoListener(this);
			behaviours[i].AddStateInfoListener(this);
		}
		if (debug)
		{
			Debug.Log("Listeners Registered", animator);
		}
	}

	public void RemoveListener()
	{
		if ((bool)animator)
		{
			vAnimatorTagBase[] behaviours = animator.GetBehaviours<vAnimatorTagBase>();
			for (int i = 0; i < behaviours.Length; i++)
			{
				behaviours[i].RemoveStateInfoListener(this);
			}
			if (debug)
			{
				Debug.Log("Listeners Removed", animator);
			}
		}
	}

	public void AddStateInfo(string tag, int layer)
	{
		if (stateInfos.Length != 0 && layer < stateInfos.Length)
		{
			vStateInfo obj = stateInfos[layer];
			obj.tags.Add(tag);
			obj.shortPathHash = 0;
			obj.normalizedTime = 0f;
		}
		if (debug)
		{
			Debug.Log($"<color=green>Add tag : <b><i>{tag}</i></b></color>,in the animator layer :{layer}", animator);
		}
	}

	public void UpdateStateInfo(int layer, float normalizedTime, int fullPathHash)
	{
		if (stateInfos.Length != 0 && layer < stateInfos.Length)
		{
			vStateInfo obj = stateInfos[layer];
			obj.normalizedTime = normalizedTime;
			obj.shortPathHash = fullPathHash;
		}
	}

	public void RemoveStateInfo(string tag, int layer)
	{
		if (stateInfos.Length == 0 || layer >= stateInfos.Length)
		{
			return;
		}
		vStateInfo vStateInfo = stateInfos[layer];
		if (vStateInfo.tags.Contains(tag))
		{
			vStateInfo.tags.Remove(tag);
			if (vStateInfo.tags.Count == 0)
			{
				vStateInfo.shortPathHash = 0;
				vStateInfo.normalizedTime = 0f;
			}
			if (debug)
			{
				Debug.Log($"<color=red>Remove tag : <b><i>{tag}</i></b></color>, in the animator layer :{layer}", animator);
			}
		}
	}

	public bool HasTag(string tag)
	{
		return Array.Exists(stateInfos, (vStateInfo info) => info.tags.Contains(tag));
	}

	public bool HasAllTags(params string[] tags)
	{
		bool result = ((tags.Length != 0) ? true : false);
		for (int i = 0; i < tags.Length; i++)
		{
			if (!HasTag(tags[i]))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public bool HasAnyTag(params string[] tags)
	{
		bool result = false;
		for (int i = 0; i < tags.Length; i++)
		{
			if (HasTag(tags[i]))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public vStateInfo GetStateInfoUsingTag(string tag)
	{
		return Array.Find(stateInfos, (vStateInfo info) => info.tags.Contains(tag));
	}

	public float GetCurrentNormalizedTime(int layer)
	{
		if (stateInfos.Length != 0 && layer < stateInfos.Length)
		{
			return stateInfos[layer].normalizedTime;
		}
		return 0f;
	}
}
