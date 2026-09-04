using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class VRIKCalibrationBasic : MonoBehaviour
{
	[Tooltip("The VRIK component.")]
	public VRIK ik;

	[Header("Head")]
	[Tooltip("HMD.")]
	public Transform centerEyeAnchor;

	[Tooltip("Position offset of the camera from the head bone (root space).")]
	public Vector3 headAnchorPositionOffset;

	[Tooltip("Rotation offset of the camera from the head bone (root space).")]
	public Vector3 headAnchorRotationOffset;

	[Header("Hands")]
	[Tooltip("Left Hand Controller")]
	public Transform leftHandAnchor;

	[Tooltip("Right Hand Controller")]
	public Transform rightHandAnchor;

	[Tooltip("Position offset of the hand controller from the hand bone (controller space).")]
	public Vector3 handAnchorPositionOffset;

	[Tooltip("Rotation offset of the hand controller from the hand bone (controller space).")]
	public Vector3 handAnchorRotationOffset;

	[Header("Scale")]
	[Tooltip("Multiplies the scale of the root.")]
	public float scaleMlp = 1f;

	[Header("Data stored by Calibration")]
	public VRIKCalibrator.CalibrationData data = new VRIKCalibrator.CalibrationData();

	private void LateUpdate()
	{
		if (Input.GetKeyDown(KeyCode.C))
		{
			data = VRIKCalibrator.Calibrate(ik, centerEyeAnchor, leftHandAnchor, rightHandAnchor, headAnchorPositionOffset, headAnchorRotationOffset, handAnchorPositionOffset, handAnchorRotationOffset, scaleMlp);
		}
		if (Input.GetKeyDown(KeyCode.D))
		{
			if (data.scale == 0f)
			{
				Debug.LogError("No Calibration Data to calibrate to, please calibrate with 'C' first.");
			}
			else
			{
				VRIKCalibrator.Calibrate(ik, data, centerEyeAnchor, null, leftHandAnchor, rightHandAnchor);
			}
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			if (data.scale == 0f)
			{
				Debug.LogError("Avatar needs to be calibrated before RecalibrateScale is called.");
			}
			VRIKCalibrator.RecalibrateScale(ik, data, scaleMlp);
		}
	}
}
