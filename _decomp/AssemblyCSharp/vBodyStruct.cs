using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Invector/SnapBody/New Body Struct")]
public class vBodyStruct : ScriptableObject
{
	[Serializable]
	public class Bone
	{
		public string name;

		public HumanBodyBones humanBone;

		public string genericBone;

		public bool isHuman = true;
	}

	public List<Bone> bones = new List<Bone>();

	private static string[] ignoreBones => new string[10] { "Thumb", "Distal", "Little", "Middle", "Index", "Ring", "Eye", "Toes", "Jaw", "LastBone" };

	protected virtual void Reset()
	{
		bones.Clear();
		bones = GetHumanBones();
	}

	public static List<Bone> GetHumanBones()
	{
		List<Bone> list = new List<Bone>();
		string[] names = Enum.GetNames(typeof(HumanBodyBones));
		for (int i = 0; i < names.Length; i++)
		{
			if (!IsIgnoredBone(names[i]))
			{
				HumanBodyBones enumTarget = HumanBodyBones.Chest;
				if (names[i].ToEnum(ref enumTarget))
				{
					Bone bone = new Bone();
					bone.isHuman = true;
					bone.name = names[i];
					bone.genericBone = names[i];
					bone.humanBone = enumTarget;
					list.Add(bone);
				}
			}
		}
		return (from x in list
			orderby x.name.ToUpper().Contains("LEFT"), x.name.ToUpper().Contains("RIGHT")
			select x).ToList();
	}

	private static bool IsIgnoredBone(string bone)
	{
		bool result = false;
		for (int i = 0; i < ignoreBones.Length; i++)
		{
			if (bone.Contains(ignoreBones[i]))
			{
				result = true;
				break;
			}
		}
		return result;
	}
}
