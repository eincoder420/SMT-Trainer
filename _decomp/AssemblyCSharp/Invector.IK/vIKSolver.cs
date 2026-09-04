using System;
using UnityEngine;

namespace Invector.IK;

[Serializable]
public class vIKSolver
{
	public Transform rootTransform;

	public Transform rootBone;

	public Transform middleBone;

	public Transform endBone;

	[Header("Optional")]
	public Transform endBoneRef;

	public Transform middleBoneRef;

	public Transform endBoneOffset;

	public Transform middleBoneOffset;

	private string middleTag;

	private string endTag;

	private float _weight;

	private Vector3? hintPosition;

	public bool isValidBones
	{
		get
		{
			if ((bool)rootBone && (bool)middleBone && (bool)endBone && (bool)endBoneRef && (bool)middleBoneRef && (bool)endBoneOffset)
			{
				return middleBoneOffset;
			}
			return false;
		}
	}

	public virtual float ikWeight => _weight;

	public vIKSolver(Transform rootTransform, Transform rootBone, Transform middleBone, Transform endBone)
	{
		this.rootTransform = rootTransform;
		this.rootBone = rootBone;
		this.middleBone = middleBone;
		this.endBone = endBone;
	}

	public vIKSolver(Animator animator, AvatarIKGoal ikGoal)
	{
		if (animator == null)
		{
			return;
		}
		rootTransform = animator.transform;
		if (animator.isHuman)
		{
			switch (ikGoal)
			{
			case AvatarIKGoal.LeftHand:
				rootBone = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
				middleBone = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
				endBone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
				endTag = "LeftHand";
				middleTag = "LeftHint";
				break;
			case AvatarIKGoal.RightHand:
				rootBone = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
				middleBone = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
				endBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
				endTag = "RightHand";
				middleTag = "RightHint";
				break;
			case AvatarIKGoal.LeftFoot:
				rootBone = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
				middleBone = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
				endBone = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
				endTag = "LeftFoot";
				middleTag = "LeftHint";
				break;
			case AvatarIKGoal.RightFoot:
				rootBone = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
				middleBone = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
				endBone = animator.GetBoneTransform(HumanBodyBones.RightFoot);
				endTag = "RightFoot";
				middleTag = "RightHint";
				break;
			}
		}
		CreateBones();
	}

	private void CreateBones()
	{
		if ((bool)rootTransform && (bool)rootBone && (bool)middleBone && (bool)endBone)
		{
			if (!endBoneRef)
			{
				endBoneRef = new GameObject(endTag + "Ref").transform;
				endBoneRef.hideFlags = HideFlags.HideInHierarchy;
				endBoneRef.SetParent(rootTransform);
			}
			if (!middleBoneRef)
			{
				middleBoneRef = new GameObject(middleTag + "Ref").transform;
				middleBoneRef.hideFlags = HideFlags.HideInHierarchy;
				middleBoneRef.SetParent(rootTransform);
			}
			if (!endBoneOffset)
			{
				endBoneOffset = new GameObject(endTag + "Offset").transform;
				endBoneOffset.SetParent(endBoneRef);
				endBoneOffset.localPosition = Vector3.zero;
				endBoneOffset.localEulerAngles = Vector3.zero;
			}
			if (!middleBoneOffset)
			{
				middleBoneOffset = new GameObject(middleTag + "Offset").transform;
				middleBoneOffset.SetParent(middleBoneRef);
				middleBoneOffset.localPosition = Vector3.zero;
				middleBoneOffset.localEulerAngles = Vector3.zero;
			}
		}
	}

	public virtual void SetIKWeight(float weight)
	{
		_weight = weight;
	}

	public void UpdateIK()
	{
		if ((bool)endBoneRef)
		{
			endBoneRef.position = endBone.position;
			endBoneRef.rotation = endBone.rotation;
		}
		if ((bool)middleBoneRef)
		{
			middleBoneRef.position = middleBone.position;
			middleBoneRef.rotation = middleBone.rotation;
		}
	}

	public virtual void AnimationToIK()
	{
		if (!isValidBones)
		{
			CreateBones();
			return;
		}
		UpdateIK();
		SetIKHintPosition(middleBoneOffset.position);
		SetIKPosition(endBoneOffset.position);
		SetIKRotation(endBoneOffset.rotation);
	}

	public virtual void SetIKPosition(Vector3 ikPosition)
	{
		if (!(ikWeight <= 0f))
		{
			Vector3 zero = Vector3.zero;
			zero = ((!hintPosition.HasValue) ? Vector3.Cross(endBone.position - rootBone.position, Vector3.Cross(endBone.position - rootBone.position, endBone.position - middleBone.position)) : (hintPosition.Value - rootBone.position));
			float magnitude = (middleBone.position - rootBone.position).magnitude;
			float magnitude2 = (endBone.position - middleBone.position).magnitude;
			Vector3 vector = GetHintPosition(rootBone.position, ikPosition, magnitude, magnitude2, zero);
			Quaternion b = Quaternion.FromToRotation(middleBone.position - rootBone.position, vector - rootBone.position) * rootBone.rotation;
			if (!float.IsNaN(b.x) && !float.IsNaN(b.y) && !float.IsNaN(b.z))
			{
				rootBone.rotation = Quaternion.Slerp(rootBone.rotation, b, ikWeight);
				Quaternion b2 = Quaternion.FromToRotation(endBone.position - middleBone.position, ikPosition - vector) * middleBone.rotation;
				middleBone.rotation = Quaternion.Slerp(middleBone.rotation, b2, ikWeight);
			}
			hintPosition = null;
		}
	}

	public virtual void SetIKRotation(Quaternion rotation)
	{
		if ((bool)rootBone && (bool)middleBone && (bool)endBone && !(ikWeight <= 0f))
		{
			endBone.rotation = Quaternion.Slerp(endBone.rotation, rotation, ikWeight);
		}
	}

	public virtual void SetIKHintPosition(Vector3 hintPosition)
	{
		this.hintPosition = hintPosition;
	}

	protected virtual Vector3 GetHintPosition(Vector3 rootPos, Vector3 endPos, float rootBoneLength, float middleBoneLength, Vector3 middleBoneDirection)
	{
		Vector3 vector = endPos - rootPos;
		float num = vector.magnitude;
		float num2 = (rootBoneLength + middleBoneLength) * 0.999f;
		if (num > num2)
		{
			endPos = rootPos + vector.normalized * num2;
			vector = endPos - rootPos;
			num = num2;
		}
		float num3 = Mathf.Abs(rootBoneLength - middleBoneLength) * 1.001f;
		if (num < num3)
		{
			endPos = rootPos + vector.normalized * num3;
			vector = endPos - rootPos;
			num = num3;
		}
		float num4 = (num * num + rootBoneLength * rootBoneLength - middleBoneLength * middleBoneLength) * 0.5f / num;
		float num5 = Mathf.Sqrt(rootBoneLength * rootBoneLength - num4 * num4);
		Vector3 vector2 = Vector3.Cross(vector, Vector3.Cross(middleBoneDirection, vector));
		return rootPos + num4 * vector.normalized + num5 * vector2.normalized;
	}
}
