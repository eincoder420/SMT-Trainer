using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class TwoHandedProp : MonoBehaviour
{
	[Range(0f, 1f)]
	public float weight = 1f;

	[Tooltip("The left hand target parented to the right hand.")]
	public Transform leftHandTarget;

	[Tooltip("Left hand poser (poses fingers to match the left hand target).")]
	public Poser leftHandPoser;

	[Tooltip("The weight of pinning the left hand to the prop.")]
	[Range(0f, 1f)]
	public float leftHandWeight = 1f;

	private FullBodyBipedIK ik;

	private Vector3 targetPosRelativeToRight;

	private Quaternion targetRotRelativeToRight;

	private void Start()
	{
		ik = GetComponent<FullBodyBipedIK>();
		IKSolverFullBodyBiped solver = ik.solver;
		solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterFBBIK));
		if (ik.solver.rightHandEffector.target == null)
		{
			Debug.LogError("Right Hand Effector needs a Target in this demo.");
		}
	}

	private void LateUpdate()
	{
		targetPosRelativeToRight = ik.references.rightHand.InverseTransformPoint(leftHandTarget.position);
		targetRotRelativeToRight = Quaternion.Inverse(ik.references.rightHand.rotation) * leftHandTarget.rotation;
		ik.solver.leftHandEffector.position = ik.solver.rightHandEffector.target.position + ik.solver.rightHandEffector.target.rotation * targetPosRelativeToRight;
		ik.solver.leftHandEffector.rotation = ik.solver.rightHandEffector.target.rotation * targetRotRelativeToRight;
		ik.solver.rightHandEffector.positionWeight = weight;
		float positionWeight = leftHandWeight * weight;
		ik.solver.leftHandEffector.positionWeight = positionWeight;
		leftHandPoser.weight = positionWeight;
	}

	private void AfterFBBIK()
	{
		ik.solver.leftHandEffector.bone.rotation = Quaternion.Slerp(ik.solver.leftHandEffector.bone.rotation, ik.solver.leftHandEffector.rotation, leftHandWeight * weight);
		ik.solver.rightHandEffector.bone.rotation = Quaternion.Slerp(ik.solver.rightHandEffector.bone.rotation, ik.solver.rightHandEffector.rotation, weight);
	}

	private void OnDestroy()
	{
		if (ik != null)
		{
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPostUpdate, new IKSolver.UpdateDelegate(AfterFBBIK));
		}
	}
}
