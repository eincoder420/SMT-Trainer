using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController;

[RequireComponent(typeof(LineRenderer))]
[vClassHeader("THROW MANAGER", true, "icon_v2", false, "")]
public class vThrowManager : vMonoBehaviour
{
	public enum CameraStyle
	{
		ThirdPerson,
		TopDown,
		SideScroll
	}

	[Serializable]
	public class ThrowObject
	{
		private Rigidbody objectToThrow;

		private int id;

		private int count;
	}

	[vEditorToolbar("Settings", false, "", false, false)]
	public CameraStyle cameraStyle;

	public bool canUseThrow;

	public Transform throwStartPoint;

	public GameObject throwEnd;

	public Rigidbody objectToThrow;

	public LayerMask obstacles = 1;

	public float throwMaxForce = 15f;

	public float throwDelayTime = 0.25f;

	public float lineStepPerTime = 0.1f;

	public float lineMaxTime = 10f;

	public int maxThrowObjects = 6;

	public int currentThrowObject;

	public float exitThrowModeDelay = 0.5f;

	[Tooltip("Set ignore collision to the grenade to not collide with the Player")]
	public bool setIgnoreCollision;

	public bool debug;

	[vSeparator("Only for ThirdPerson Camera Style", "")]
	[Tooltip("The Third person camera right will be applied as offset to throw start point")]
	public bool useCameraRightAsOffset;

	[Tooltip("Increase or decrease the Offset right value")]
	[vHideInInspector("useCameraRightAsOffset", false)]
	public float cameraRightOffsetMultiplier = 1f;

	[Tooltip("Rotate to aim point while aiming")]
	public bool rotateWhileAiming = true;

	public bool strafeWhileAiming = true;

	[vEditorToolbar("Inputs", false, "", false, false)]
	public GenericInput throwInput = new GenericInput("Mouse0", "RB", "RB");

	public GenericInput aimThrowInput = new GenericInput("G", "LB", "LB");

	public bool aimHoldingButton = true;

	[vEditorToolbar("Animations", false, "", false, false)]
	[Tooltip("Delay to exit the Throw Aiming Mode and get back to default locomotion")]
	public string throwAnimation = "ThrowObject";

	public string holdingAnimation = "HoldingObject";

	public string cancelAnimation = "CancelThrow";

	[vEditorToolbar("Events", false, "", false, false)]
	public UnityEvent onEnableAim;

	public UnityEvent onCancelAim;

	public UnityEvent onThrowObject;

	public UnityEvent onCollectObject;

	public UnityEvent onFinishThrow;

	public Collider[] selfColliders;

	protected bool isAiming;

	protected bool inThrow;

	protected bool isThrowInput;

	protected Transform rightUpperArm;

	protected LineRenderer lineRenderer;

	protected vThrowUI _ui;

	protected vThirdPersonInput tpInput;

	protected RaycastHit hit;

	protected GameObject lastThrowable;

	protected vExplosive explosive;

	public virtual vThrowUI ui
	{
		get
		{
			if (!_ui)
			{
				_ui = UnityEngine.Object.FindObjectOfType<vThrowUI>();
				if ((bool)_ui)
				{
					_ui.UpdateCount(this);
				}
			}
			return _ui;
		}
	}

	protected virtual Vector3 thirdPersonAimPoint => startPoint + tpInput.cameraMain.transform.forward * throwMaxForce;

	protected virtual Vector3 topdownAimPoint
	{
		get
		{
			Vector3 result = vMousePositionHandler.Instance.WorldMousePosition(obstacles);
			result.y = base.transform.position.y;
			return result;
		}
	}

	protected virtual Vector3 sideScrollAimPoint
	{
		get
		{
			Vector3 position = base.transform.InverseTransformPoint(vMousePositionHandler.Instance.WorldMousePosition(obstacles));
			position.x = 0f;
			return base.transform.TransformPoint(position);
		}
	}

	protected virtual Vector3 startPoint
	{
		get
		{
			Vector3 position = throwStartPoint.position;
			if (useCameraRightAsOffset && (bool)tpInput && (bool)tpInput.tpCamera && tpInput.tpCamera.lerpState != null)
			{
				position += tpInput.tpCamera.transform.right * tpInput.tpCamera.lerpState.right * cameraRightOffsetMultiplier * tpInput.tpCamera.switchRight;
			}
			return position;
		}
	}

	protected virtual Vector3 StartVelocity
	{
		get
		{
			float value = Vector3.Distance(startPoint, aimPoint);
			if (debug)
			{
				Debug.DrawLine(startPoint, aimPoint);
			}
			if (cameraStyle == CameraStyle.ThirdPerson && Physics.Raycast(startPoint, aimDirection.normalized, out var hitInfo, aimDirection.magnitude, obstacles))
			{
				value = hitInfo.distance;
			}
			if (cameraStyle != CameraStyle.SideScroll)
			{
				float num = Mathf.Clamp(value, 0f, throwMaxForce);
				return aimDirection.normalized * num;
			}
			float num2 = Mathf.Clamp(value, 0f, throwMaxForce);
			return aimDirection.normalized * num2;
		}
	}

	public virtual Vector3 aimPoint => cameraStyle switch
	{
		CameraStyle.ThirdPerson => thirdPersonAimPoint, 
		CameraStyle.TopDown => topdownAimPoint, 
		CameraStyle.SideScroll => sideScrollAimPoint, 
		_ => startPoint + tpInput.cameraMain.transform.forward * throwMaxForce, 
	};

	public virtual Vector3 aimDirection => aimPoint - startPoint;

	public virtual void CanUseThrow(bool value)
	{
		canUseThrow = value;
	}

	protected virtual IEnumerator Start()
	{
		yield return new WaitForEndOfFrame();
		if (ui != null)
		{
			ui.UpdateCount(this);
		}
		lineRenderer = GetComponent<LineRenderer>();
		if ((bool)lineRenderer)
		{
			lineRenderer.useWorldSpace = true;
		}
		canUseThrow = true;
		tpInput = GetComponentInParent<vThirdPersonInput>();
		if (currentThrowObject > maxThrowObjects)
		{
			currentThrowObject = maxThrowObjects;
		}
		if ((bool)tpInput)
		{
			selfColliders = tpInput.GetComponentsInChildren<Collider>(includeInactive: true);
			tpInput.onUpdate -= UpdateThrowInput;
			tpInput.onUpdate += UpdateThrowInput;
			tpInput.onFixedUpdate -= UpdateThrowBehavior;
			tpInput.onFixedUpdate += UpdateThrowBehavior;
			rightUpperArm = tpInput.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			if (cameraStyle == CameraStyle.SideScroll)
			{
				tpInput.cc.strafeSpeed.rotateWithCamera = true;
			}
		}
		if (cameraStyle != 0)
		{
			useCameraRightAsOffset = false;
			rotateWhileAiming = true;
			strafeWhileAiming = true;
		}
	}

	protected virtual void UpdateThrowBehavior()
	{
		UpdateThrow();
		if (objectToThrow == null || !tpInput.enabled || tpInput.cc.customAction || !canUseThrow || tpInput.cc.isDead)
		{
			isAiming = false;
			inThrow = false;
			isThrowInput = false;
		}
		else
		{
			MoveAndRotate();
		}
	}

	protected virtual void UpdateThrowInput()
	{
		if (objectToThrow == null || !tpInput.enabled || tpInput.cc.customAction || !canUseThrow || tpInput.cc.isDead)
		{
			isAiming = false;
			inThrow = false;
			isThrowInput = false;
			return;
		}
		if (aimThrowInput.GetButtonDown() && !isAiming && !inThrow)
		{
			PrepareControllerToThrow(value: true);
			tpInput.animator.CrossFadeInFixedTime(holdingAnimation, 0.2f);
			onEnableAim.Invoke();
			return;
		}
		if (aimThrowInput.GetButtonUp() && aimHoldingButton && isAiming)
		{
			PrepareControllerToThrow(value: false);
			tpInput.animator.CrossFadeInFixedTime(cancelAnimation, 0.2f);
			onCancelAim.Invoke();
			onFinishThrow.Invoke();
		}
		if (throwInput.GetButtonDown() && isAiming && !inThrow)
		{
			isAiming = false;
			isThrowInput = true;
		}
		else if (!aimHoldingButton && aimThrowInput.GetButtonDown() && !isThrowInput && isAiming)
		{
			PrepareControllerToThrow(value: false);
			tpInput.animator.CrossFadeInFixedTime(cancelAnimation, 0.2f);
			onCancelAim.Invoke();
			onFinishThrow.Invoke();
		}
	}

	protected virtual void MoveAndRotate()
	{
		if (!isAiming && !inThrow)
		{
			return;
		}
		tpInput.MoveInput();
		switch (cameraStyle)
		{
		case CameraStyle.ThirdPerson:
			if (inThrow || rotateWhileAiming)
			{
				tpInput.cc.RotateToDirection(tpInput.cameraMain.transform.forward);
			}
			break;
		case CameraStyle.TopDown:
		{
			Vector3 direction = aimDirection;
			direction.y = 0f;
			if (inThrow || rotateWhileAiming)
			{
				tpInput.cc.RotateToDirection(direction);
			}
			break;
		}
		case CameraStyle.SideScroll:
			break;
		}
	}

	protected virtual void LaunchObject(Rigidbody projectily)
	{
		projectily.AddForce(StartVelocity, ForceMode.VelocityChange);
	}

	protected virtual void UpdateThrow()
	{
		if (objectToThrow == null || !tpInput.enabled || tpInput.cc.customAction)
		{
			isAiming = false;
			inThrow = false;
			isThrowInput = false;
			if ((bool)lineRenderer && lineRenderer.enabled)
			{
				lineRenderer.enabled = false;
			}
			if ((bool)throwEnd && throwEnd.activeSelf)
			{
				throwEnd.SetActive(value: false);
			}
			return;
		}
		if (isAiming)
		{
			DrawTrajectory();
		}
		else
		{
			if ((bool)lineRenderer && lineRenderer.enabled)
			{
				lineRenderer.enabled = false;
			}
			if ((bool)throwEnd && throwEnd.activeSelf)
			{
				throwEnd.SetActive(value: false);
			}
		}
		if (isThrowInput)
		{
			inThrow = true;
			isThrowInput = false;
			tpInput.animator.CrossFadeInFixedTime(throwAnimation, 0.2f);
			currentThrowObject--;
			StartCoroutine(Launch());
		}
	}

	protected virtual void DrawTrajectory()
	{
		List<Vector3> trajectoryPoints = GetTrajectoryPoints(startPoint, StartVelocity, lineStepPerTime, lineMaxTime);
		if ((bool)lineRenderer)
		{
			if (!lineRenderer.enabled)
			{
				lineRenderer.enabled = true;
			}
			lineRenderer.positionCount = trajectoryPoints.Count;
			lineRenderer.SetPositions(trajectoryPoints.ToArray());
		}
		if ((bool)throwEnd)
		{
			if (!throwEnd.activeSelf)
			{
				throwEnd.SetActive(value: true);
			}
			if (objectToThrow.gameObject != lastThrowable)
			{
				lastThrowable = objectToThrow.gameObject;
				explosive = lastThrowable.GetComponentInChildren<vExplosive>();
			}
			if ((bool)explosive)
			{
				throwEnd.transform.localScale = Vector3.one * explosive.maxExplosionRadius;
			}
			if (trajectoryPoints.Count > 1)
			{
				throwEnd.transform.position = trajectoryPoints[trajectoryPoints.Count - 1];
			}
		}
	}

	protected virtual IEnumerator Launch()
	{
		yield return new WaitForSeconds(throwDelayTime);
		Rigidbody rigidbody = UnityEngine.Object.Instantiate(objectToThrow, startPoint, throwStartPoint.rotation);
		if (setIgnoreCollision)
		{
			Collider component = rigidbody.GetComponent<Collider>();
			if ((bool)component)
			{
				for (int i = 0; i < selfColliders.Length; i++)
				{
					Physics.IgnoreCollision(component, selfColliders[i], ignore: true);
				}
			}
		}
		rigidbody.isKinematic = false;
		LaunchObject(rigidbody);
		if ((bool)ui)
		{
			ui.UpdateCount(this);
		}
		onThrowObject.Invoke();
		yield return new WaitForSeconds(2f * lineStepPerTime);
		inThrow = false;
		if (currentThrowObject <= 0)
		{
			objectToThrow = null;
		}
		yield return new WaitForSeconds(exitThrowModeDelay);
		PrepareControllerToThrow(value: false);
		onFinishThrow.Invoke();
	}

	protected virtual void PrepareControllerToThrow(bool value)
	{
		isAiming = value;
		tpInput.SetLockAllInput(value);
		tpInput.SetStrafeLocomotion(value && strafeWhileAiming);
		if (cameraStyle == CameraStyle.SideScroll)
		{
			tpInput.cc.strafeSpeed.rotateWithCamera = true;
		}
	}

	protected virtual Vector3 PlotTrajectoryAtTime(Vector3 start, Vector3 startVelocity, float time)
	{
		return start + startVelocity * time + Physics.gravity * time * time * 0.5f;
	}

	protected virtual List<Vector3> GetTrajectoryPoints(Vector3 start, Vector3 startVelocity, float timestep, float maxTime)
	{
		Vector3 vector = start;
		List<Vector3> list = new List<Vector3>();
		list.Add(vector);
		int num = 1;
		while (true)
		{
			float num2 = timestep * (float)num;
			if (num2 > maxTime)
			{
				break;
			}
			Vector3 vector2 = PlotTrajectoryAtTime(start, startVelocity, num2);
			if (Physics.Linecast(vector, vector2, out var hitInfo, obstacles))
			{
				list.Add(hitInfo.point);
				break;
			}
			if (debug)
			{
				Debug.DrawLine(vector, vector2, Color.red);
			}
			list.Add(vector2);
			vector = vector2;
			num++;
		}
		return list;
	}

	public virtual void SetAmount(int value)
	{
		currentThrowObject += value;
		if ((bool)ui)
		{
			ui.UpdateCount(this);
		}
		onCollectObject.Invoke();
	}
}
