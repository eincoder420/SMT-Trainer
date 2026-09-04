using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos;

public class VRController : MonoBehaviour
{
	[Serializable]
	public enum InputMode
	{
		Input,
		WASDOnly
	}

	public InputMode inputMode;

	public VRIK ik;

	public Transform centerEyeAnchor;

	public float walkSpeed = 1f;

	public float runSpeed = 3f;

	public float walkForwardSpeedMlp = 1f;

	public float runForwardSpeedMlp = 1f;

	private Vector3 smoothInput;

	private Vector3 smoothInputV;

	private void Update()
	{
		Vector3 input = GetInput();
		input *= ik.solver.scale;
		bool flag = Vector3.Dot(input, Vector3.forward) > 0f;
		float num = walkSpeed;
		if (Input.GetKey(KeyCode.LeftShift))
		{
			num = runSpeed;
			if (flag)
			{
				num *= runForwardSpeedMlp;
			}
		}
		else if (flag)
		{
			num *= walkForwardSpeedMlp;
		}
		smoothInput = Vector3.SmoothDamp(smoothInput, input * num, ref smoothInputV, 0.1f);
		Vector3 forward = centerEyeAnchor.forward;
		forward.y = 0f;
		Quaternion quaternion = Quaternion.LookRotation(forward);
		base.transform.position += quaternion * smoothInput * Time.deltaTime;
	}

	private Vector3 GetInput()
	{
		switch (inputMode)
		{
		case InputMode.Input:
		{
			Vector3 vector = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
			if (vector.sqrMagnitude < 0.3f)
			{
				return Vector3.zero;
			}
			return vector.normalized;
		}
		case InputMode.WASDOnly:
		{
			Vector3 zero = Vector3.zero;
			if (Input.GetKey(KeyCode.W))
			{
				zero += Vector3.forward;
			}
			if (Input.GetKey(KeyCode.S))
			{
				zero += Vector3.back;
			}
			if (Input.GetKey(KeyCode.A))
			{
				zero += Vector3.left;
			}
			if (Input.GetKey(KeyCode.D))
			{
				zero += Vector3.right;
			}
			return zero.normalized;
		}
		default:
			return Vector3.zero;
		}
	}
}
