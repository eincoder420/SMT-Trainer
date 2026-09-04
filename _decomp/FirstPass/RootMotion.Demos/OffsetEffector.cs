using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class OffsetEffector : OffsetModifier
{
	[Serializable]
	public class EffectorLink
	{
		public FullBodyBipedEffector effectorType;

		public float weightMultiplier = 1f;

		[HideInInspector]
		public Vector3 localPosition;
	}

	[Tooltip("Optional. Assign the bone Transform that is closest to this OffsetEffector to be able to call OffsetEffector.Anchor() in LateUpdate to match its position and rotation to animation.")]
	public Transform anchor;

	public EffectorLink[] effectorLinks;

	private Vector3 posRelToAnchor;

	private Quaternion rotRelToAnchor = Quaternion.identity;

	protected override void Start()
	{
		base.Start();
		if (anchor != null)
		{
			posRelToAnchor = anchor.InverseTransformPoint(base.transform.position);
			rotRelToAnchor = Quaternion.Inverse(anchor.rotation) * base.transform.rotation;
		}
		EffectorLink[] array = effectorLinks;
		foreach (EffectorLink effectorLink in array)
		{
			Transform bone = ik.solver.GetEffector(effectorLink.effectorType).bone;
			effectorLink.localPosition = base.transform.InverseTransformPoint(bone.position);
			if (effectorLink.effectorType == FullBodyBipedEffector.Body)
			{
				ik.solver.bodyEffector.effectChildNodes = false;
			}
		}
	}

	protected override void OnModifyOffset()
	{
		EffectorLink[] array = effectorLinks;
		foreach (EffectorLink effectorLink in array)
		{
			Vector3 vector = base.transform.TransformPoint(effectorLink.localPosition);
			ik.solver.GetEffector(effectorLink.effectorType).positionOffset += (vector - (ik.solver.GetEffector(effectorLink.effectorType).bone.position + ik.solver.GetEffector(effectorLink.effectorType).positionOffset)) * weight * effectorLink.weightMultiplier;
		}
	}

	public void Anchor()
	{
		if (!(anchor == null))
		{
			base.transform.position = anchor.TransformPoint(posRelToAnchor);
			base.transform.rotation = anchor.rotation * rotRelToAnchor;
		}
	}
}
