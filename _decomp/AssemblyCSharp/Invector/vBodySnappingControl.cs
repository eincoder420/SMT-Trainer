using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

[vClassHeader("Body Snapping Control", true, "icon_v2", false, "", openClose = false)]
public class vBodySnappingControl : vMonoBehaviour
{
	[Serializable]
	public class vBoneTransformSnapping
	{
		public enum Orientation
		{
			Forward,
			Back,
			Right,
			Left,
			Up,
			Down
		}

		public string name;

		public Transform bone;

		public Transform target;

		public Orientation orientation;

		public UnityEvent onSnap;

		public Quaternion targetRotation
		{
			get
			{
				Quaternion result = Quaternion.LookRotation(Vector3.forward);
				Vector3 direction = Vector3.forward;
				if ((bool)bone && (bool)target && (bool)bone.parent)
				{
					switch (orientation)
					{
					case Orientation.Back:
						direction = Vector3.back;
						break;
					case Orientation.Right:
						direction = Vector3.right;
						break;
					case Orientation.Left:
						direction = Vector3.left;
						break;
					case Orientation.Up:
						direction = Vector3.up;
						break;
					case Orientation.Down:
						direction = Vector3.down;
						break;
					}
					result = Quaternion.LookRotation(bone.TransformDirection(direction), bone.up);
				}
				return result;
			}
		}

		public void Snap()
		{
			if ((bool)bone && (bool)target)
			{
				if (Application.isPlaying && target.parent != bone)
				{
					target.parent = bone;
					onSnap.Invoke();
				}
				target.rotation = targetRotation;
				target.position = bone.position;
			}
		}
	}

	[vButton("Create New BodyStruct", "NewBodyStruct", typeof(vBodySnappingControl), false)]
	[vButton("Load Bones", "LoadBones", typeof(vBodySnappingControl), false)]
	public vBodyStruct bodyStruct;

	public bool showLabels;

	[HideInInspector]
	public List<vBoneTransformSnapping> boneSnappingList = new List<vBoneTransformSnapping>();

	private bool bonesIsLoaded;

	protected virtual void Reset()
	{
		LoadBones();
	}

	public virtual void LoadBones()
	{
		Animator componentInParent = GetComponentInParent<Animator>();
		List<vBodyStruct.Bone> bones = (bodyStruct ? bodyStruct.bones : vBodyStruct.GetHumanBones());
		if ((bool)bodyStruct)
		{
			List<vBoneTransformSnapping> list = boneSnappingList.FindAll((vBoneTransformSnapping _b) => !bones.Exists((vBodyStruct.Bone _b2) => _b2.name.Equals(_b.name)));
			for (int j = 0; j < list.Count; j++)
			{
				boneSnappingList.Remove(list[j]);
			}
		}
		if (bones.Count > 0)
		{
			int i;
			for (i = 0; i < bones.Count; i++)
			{
				Transform transform = null;
				transform = ((!bones[i].isHuman || !componentInParent || !componentInParent.isHuman) ? GetBoneByName(bones[i].genericBone) : componentInParent.GetBoneTransform(bones[i].humanBone));
				vBoneTransformSnapping vBoneTransformSnapping = boneSnappingList.Find((vBoneTransformSnapping _b) => _b.name.Equals(bones[i].name));
				if (vBoneTransformSnapping == null)
				{
					vBoneTransformSnapping = new vBoneTransformSnapping();
					vBoneTransformSnapping.name = bones[i].name;
					vBoneTransformSnapping.bone = transform;
					boneSnappingList.Add(vBoneTransformSnapping);
				}
				else
				{
					vBoneTransformSnapping.bone = transform;
				}
			}
		}
		boneSnappingList = (from x in boneSnappingList
			orderby x.bone != null && x.name.ToUpper().Contains("LEFT"), x.bone != null && x.name.ToUpper().Contains("RIGHT")
			select x).ToList();
		if (!Application.isPlaying)
		{
			bonesIsLoaded = true;
		}
	}

	protected virtual void Awake()
	{
		LoadBones();
		SnapAll();
	}

	public virtual void SnapAll()
	{
		foreach (vBoneTransformSnapping boneSnapping in boneSnappingList)
		{
			boneSnapping.Snap();
		}
	}

	public virtual Transform GetBone(string name)
	{
		if (!bonesIsLoaded)
		{
			LoadBones();
		}
		return boneSnappingList.Find((vBoneTransformSnapping b) => b.name.Equals(name))?.bone;
	}

	protected virtual Transform GetBoneByName(string name)
	{
		Animator componentInParent = GetComponentInParent<Animator>();
		if (!componentInParent)
		{
			return null;
		}
		Transform boneTransform = componentInParent.GetBoneTransform(HumanBodyBones.Hips);
		if (boneTransform == null)
		{
			boneTransform = componentInParent.transform;
		}
		List<Transform> list = boneTransform.gameObject.GetComponentsInChildren<Transform>(includeInactive: true).vToList();
		Transform result = null;
		if (list.Count > 0 && !string.IsNullOrEmpty(name.Trim()))
		{
			string[] nameSplited = name.Trim().Split(';');
			result = list.Find((Transform child) => ContainsName(nameSplited, child.gameObject.name.Trim()));
		}
		return result;
	}

	protected virtual bool ContainsName(string[] nameSplited, string targetName)
	{
		bool result = false;
		for (int i = 0; i < nameSplited.Length; i++)
		{
			if (targetName.Contains(nameSplited[i]))
			{
				result = true;
				break;
			}
		}
		return result;
	}
}
