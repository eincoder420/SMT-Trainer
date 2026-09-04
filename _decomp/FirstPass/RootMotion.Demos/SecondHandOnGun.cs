using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class SecondHandOnGun : MonoBehaviour
{
	public AimIK aim;

	public LimbIK leftArmIK;

	public Transform leftHand;

	public Transform rightHand;

	public GrounderFBBIK grounder;

	public Vector3 leftHandPositionOffset;

	public Vector3 leftHandRotationOffset;

	private Vector3 leftHandPosRelToRight;

	private Quaternion leftHandRotRelToRight;

	private void Start()
	{
		aim.enabled = false;
		leftArmIK.enabled = false;
		if (grounder != null)
		{
			grounder.ik.enabled = false;
		}
	}

	private void LateUpdate()
	{
		leftHandPosRelToRight = rightHand.InverseTransformPoint(leftHand.position);
		leftHandRotRelToRight = Quaternion.Inverse(rightHand.rotation) * leftHand.rotation;
		if (grounder != null)
		{
			grounder.ik.solver.Update();
		}
		aim.solver.Update();
		leftArmIK.solver.IKPosition = rightHand.TransformPoint(leftHandPosRelToRight + leftHandPositionOffset);
		leftArmIK.solver.IKRotation = rightHand.rotation * Quaternion.Euler(leftHandRotationOffset) * leftHandRotRelToRight;
		leftArmIK.solver.Update();
	}
}
