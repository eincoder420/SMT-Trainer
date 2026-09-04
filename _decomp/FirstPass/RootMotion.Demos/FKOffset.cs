using System;
using UnityEngine;

namespace RootMotion.Demos;

public class FKOffset : MonoBehaviour
{
	[Serializable]
	public class Offset
	{
		[HideInInspector]
		public string name;

		public HumanBodyBones bone;

		public Vector3 rotationOffset;

		private Transform t;

		public void Apply(Animator animator)
		{
			if (t == null)
			{
				t = animator.GetBoneTransform(bone);
			}
			if (!(t == null))
			{
				t.localRotation *= Quaternion.Euler(rotationOffset);
			}
		}
	}

	public Offset[] offsets;

	private Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	private void LateUpdate()
	{
		Offset[] array = offsets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Apply(animator);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Offset[] array = offsets;
		foreach (Offset obj in array)
		{
			obj.name = obj.bone.ToString();
		}
	}
}
