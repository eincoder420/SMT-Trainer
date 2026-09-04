using System.Collections;
using Invector.vCharacterController;
using UnityEngine;

namespace Invector.vCamera;

public class vThirdPersonCamera : MonoBehaviour
{
	private static vThirdPersonCamera _instance;

	public Transform mainTarget;

	[Tooltip("Lerp speed between Camera States")]
	public float smoothBetweenState = 6f;

	public float smoothCameraRotation = 6f;

	public float smoothSwitchSide = 2f;

	public float scrollSpeed = 10f;

	[Tooltip("Multiplier of Mouse x and y when using joystick")]
	public float joystickSensitivity = 1f;

	[Tooltip("What layer will be culled")]
	public LayerMask cullingLayer = 1;

	[Tooltip("Change this value If the camera pass through the wall")]
	public float clipPlaneMargin;

	public float checkHeightRadius;

	public bool showGizmos;

	public bool startUsingTargetRotation = true;

	public bool startSmooth;

	[Tooltip("Returns to behind the target automatically after 'behindTargetDelay' period")]
	public bool autoBehindTarget;

	[vHideInInspector("autoBehindTarget", false)]
	public float behindTargetDelay = 2f;

	[vHideInInspector("autoBehindTarget", false)]
	public float behindTargetSmoothRotation = 1f;

	[Tooltip("Debug purposes, lock the camera behind the character for better align the states")]
	[SerializeField]
	protected bool lockCamera;

	private WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

	public Vector2 offsetMouse;

	[HideInInspector]
	public int indexList;

	[HideInInspector]
	public int indexLookPoint;

	public float offSetPlayerPivot;

	[HideInInspector]
	public float distance = 5f;

	[HideInInspector]
	public string currentStateName;

	[HideInInspector]
	public Transform currentTarget;

	[HideInInspector]
	public vThirdPersonCameraState currentState;

	[HideInInspector]
	public vThirdPersonCameraListData CameraStateList;

	[HideInInspector]
	public Transform lockTarget;

	[HideInInspector]
	public Vector2 movementSpeed;

	[HideInInspector]
	public vThirdPersonCameraState lerpState;

	protected float lockTargetSpeed;

	protected float lockTargetWeight;

	protected float initialCameraRotation;

	protected bool cameraIsRotating;

	protected Quaternion lastCameraRotation;

	protected float lastRotationTimer;

	protected Vector3 currentTargetPos;

	protected Vector3 lookPoint;

	protected Vector3 current_cPos;

	protected Vector3 desired_cPos;

	protected Vector3 lookTargetAdjust;

	internal float mouseY;

	internal float mouseX;

	protected float currentHeight;

	protected float currentZoom;

	protected float cullingHeight;

	protected float cullingDistance;

	internal float switchRight;

	protected float currentSwitchRight;

	protected float heightOffset;

	internal bool isInit;

	protected bool useSmooth;

	protected bool isNewTarget;

	protected bool firstStateIsInit;

	protected Quaternion fixedRotation;

	internal Camera targetCamera;

	protected float transformWeight;

	protected float mouseXStart;

	protected float mouseYStart;

	protected Vector3 startPosition;

	protected Quaternion startRotation;

	private protected Vector3 cameraVelocityDamp;

	private protected bool firstUpdated;

	protected Transform _lookAtTarget;

	protected Vector3 lastLookAtPosition;

	protected Vector3 lastLookAtForward;

	public bool isFreezed;

	protected Rigidbody _selfRigidbody;

	public static vThirdPersonCamera instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindObjectOfType<vThirdPersonCamera>();
			}
			return _instance;
		}
	}

	protected Transform targetLookAt
	{
		get
		{
			if (!_lookAtTarget)
			{
				_lookAtTarget = new GameObject("targetLookAt").transform;
				_lookAtTarget.rotation = base.transform.rotation;
				_lookAtTarget.position = mainTarget.position;
			}
			return _lookAtTarget;
		}
	}

	public Rigidbody selfRigidbody
	{
		get
		{
			if (!_selfRigidbody)
			{
				_selfRigidbody = base.gameObject.AddComponent<Rigidbody>();
				_selfRigidbody.isKinematic = true;
				_selfRigidbody.interpolation = RigidbodyInterpolation.None;
			}
			return _selfRigidbody;
		}
	}

	public bool LockCamera
	{
		get
		{
			return lockCamera;
		}
		set
		{
			lockCamera = value;
		}
	}

	protected virtual bool isValidFixedPoint
	{
		get
		{
			if (currentState.lookPoints != null && currentState.cameraMode.Equals(TPCameraMode.FixedPoint))
			{
				if (indexLookPoint >= currentState.lookPoints.Count)
				{
					return currentState.lookPoints.Count > 0;
				}
				return true;
			}
			return false;
		}
	}

	protected virtual void OnDrawGizmos()
	{
		if (showGizmos && (bool)currentTarget)
		{
			Vector3 vector = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
			Gizmos.DrawWireSphere(vector + Vector3.up * cullingHeight, checkHeightRadius);
			Gizmos.DrawLine(vector, vector + Vector3.up * cullingHeight);
		}
	}

	protected virtual void Start()
	{
		Init();
	}

	public virtual void Init()
	{
		if (!(mainTarget == null))
		{
			firstUpdated = true;
			useSmooth = true;
			targetLookAt.rotation = (startUsingTargetRotation ? mainTarget.rotation : base.transform.rotation);
			targetLookAt.position = mainTarget.position;
			targetLookAt.hideFlags = HideFlags.HideInHierarchy;
			startPosition = selfRigidbody.position;
			startRotation = selfRigidbody.rotation;
			initialCameraRotation = smoothCameraRotation;
			if (!targetCamera)
			{
				targetCamera = Camera.main;
			}
			currentTarget = mainTarget;
			switchRight = 1f;
			currentSwitchRight = 1f;
			mouseXStart = base.transform.eulerAngles.NormalizeAngle().y;
			mouseYStart = base.transform.eulerAngles.NormalizeAngle().x;
			if (startSmooth)
			{
				distance = Vector3.Distance(targetLookAt.position, base.transform.position);
			}
			else
			{
				transformWeight = 1f;
			}
			if (startUsingTargetRotation)
			{
				mouseY = currentTarget.eulerAngles.NormalizeAngle().x;
				mouseX = currentTarget.eulerAngles.NormalizeAngle().y;
			}
			else
			{
				mouseY = base.transform.eulerAngles.NormalizeAngle().x;
				mouseX = base.transform.eulerAngles.NormalizeAngle().y;
			}
			ChangeState("Default", startSmooth);
			currentZoom = currentState.defaultDistance;
			currentHeight = currentState.height;
			currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z) + currentTarget.transform.up * lerpState.height;
			targetLookAt.position = currentTargetPos;
			isInit = true;
		}
	}

	public virtual void FixedUpdate()
	{
		if (!(mainTarget == null) && !(targetLookAt == null) && currentState != null && lerpState != null && isInit && !isFreezed)
		{
			switch (currentState.cameraMode)
			{
			case TPCameraMode.FreeDirectional:
				CameraMovement();
				break;
			case TPCameraMode.FixedAngle:
				CameraMovement();
				break;
			case TPCameraMode.FixedPoint:
				CameraFixed();
				break;
			}
		}
	}

	public virtual void SetLockTarget(Transform lockTarget)
	{
		if (!(this.lockTarget != null) || !(this.lockTarget == lockTarget))
		{
			isNewTarget = lockTarget != this.lockTarget;
			this.lockTarget = lockTarget;
			lockTargetWeight = 0f;
			lockTargetSpeed = 1f;
		}
	}

	public virtual void SetLockTarget(Transform lockTarget, float heightOffset, float lockSpeed = 1f)
	{
		if (!(this.lockTarget != null) || !(this.lockTarget == lockTarget))
		{
			isNewTarget = lockTarget != this.lockTarget;
			this.lockTarget = lockTarget;
			this.heightOffset = heightOffset;
			lockTargetWeight = 0f;
			lockTargetSpeed = lockSpeed;
		}
	}

	public virtual void RemoveLockTarget()
	{
		lockTargetWeight = 0f;
		lockTarget = null;
	}

	public virtual void SetTarget(Transform newTarget)
	{
		lockTargetWeight = 0f;
		currentTarget = (newTarget ? newTarget : mainTarget);
	}

	public virtual void SetMainTarget(Transform newTarget)
	{
		mainTarget = newTarget;
		currentTarget = newTarget;
		if (!isInit)
		{
			Init();
		}
	}

	public virtual void ResetTarget()
	{
		if (currentTarget != mainTarget)
		{
			currentTarget = mainTarget;
			if (!isInit)
			{
				Init();
			}
		}
	}

	public virtual void ResetAngle()
	{
		if ((bool)currentTarget)
		{
			mouseY = currentTarget.eulerAngles.NormalizeAngle().x;
			mouseX = currentTarget.eulerAngles.NormalizeAngle().y;
		}
		else
		{
			mouseY = 0f;
			mouseX = 0f;
		}
	}

	public virtual Ray ScreenPointToRay(Vector3 Point)
	{
		return GetComponent<Camera>().ScreenPointToRay(Point);
	}

	public virtual void ChangeState(string stateName)
	{
		ChangeState(stateName, hasSmooth: true);
	}

	public virtual void ChangeState(string stateName, bool hasSmooth)
	{
		if ((currentState != null && currentState.Name.Equals(stateName)) || (!isInit && firstStateIsInit))
		{
			if (firstStateIsInit)
			{
				useSmooth = hasSmooth;
			}
			return;
		}
		useSmooth = ((!firstStateIsInit) ? startSmooth : hasSmooth);
		vThirdPersonCameraState vThirdPersonCameraState = ((CameraStateList != null) ? CameraStateList.tpCameraStates.Find((vThirdPersonCameraState obj) => obj.Name.Equals(stateName)) : new vThirdPersonCameraState("Default"));
		if (vThirdPersonCameraState != null)
		{
			currentStateName = stateName;
			currentState.cameraMode = vThirdPersonCameraState.cameraMode;
			lerpState = vThirdPersonCameraState;
			if (!firstStateIsInit)
			{
				currentState.defaultDistance = Vector3.Distance(targetLookAt.position, base.transform.position);
				currentState.forward = lerpState.forward;
				currentState.height = vThirdPersonCameraState.height;
				currentState.fov = vThirdPersonCameraState.fov;
				if (useSmooth)
				{
					StartCoroutine(ResetFirstState());
				}
				else
				{
					distance = lerpState.defaultDistance;
					firstStateIsInit = true;
				}
			}
			if (currentState != null && !useSmooth)
			{
				currentState.CopyState(vThirdPersonCameraState);
			}
		}
		else if (CameraStateList != null && CameraStateList.tpCameraStates.Count > 0)
		{
			if (lerpState != null)
			{
				return;
			}
			vThirdPersonCameraState = CameraStateList.tpCameraStates[0];
			currentStateName = vThirdPersonCameraState.Name;
			currentState.cameraMode = vThirdPersonCameraState.cameraMode;
			lerpState = vThirdPersonCameraState;
			if (currentState != null && !useSmooth)
			{
				currentState.CopyState(vThirdPersonCameraState);
			}
		}
		if (currentState == null)
		{
			currentState = new vThirdPersonCameraState("Null");
			currentStateName = currentState.Name;
		}
		if (CameraStateList != null)
		{
			indexList = CameraStateList.tpCameraStates.IndexOf(vThirdPersonCameraState);
		}
		currentZoom = vThirdPersonCameraState.defaultDistance;
		if (currentState.cameraMode == TPCameraMode.FixedAngle)
		{
			mouseX = currentState.fixedAngle.x;
			mouseY = currentState.fixedAngle.y;
		}
		currentState.fixedAngle = new Vector3(mouseX, mouseY);
		indexLookPoint = 0;
		if (!isInit)
		{
			CameraMovement(forceUpdate: true);
		}
	}

	public virtual void ChangeState(string stateName, string pointName, bool hasSmooth)
	{
		useSmooth = hasSmooth;
		if (!currentState.Name.Equals(stateName))
		{
			vThirdPersonCameraState vThirdPersonCameraState = CameraStateList.tpCameraStates.Find((vThirdPersonCameraState obj) => obj.Name.Equals(stateName));
			if (vThirdPersonCameraState != null)
			{
				currentStateName = stateName;
				currentState.cameraMode = vThirdPersonCameraState.cameraMode;
				lerpState = vThirdPersonCameraState;
				if (currentState != null && !hasSmooth)
				{
					currentState.CopyState(vThirdPersonCameraState);
				}
			}
			else if (CameraStateList.tpCameraStates.Count > 0)
			{
				vThirdPersonCameraState = CameraStateList.tpCameraStates[0];
				currentStateName = vThirdPersonCameraState.Name;
				currentState.cameraMode = vThirdPersonCameraState.cameraMode;
				lerpState = vThirdPersonCameraState;
				if (currentState != null && !hasSmooth)
				{
					currentState.CopyState(vThirdPersonCameraState);
				}
			}
			if (currentState == null)
			{
				currentState = new vThirdPersonCameraState("Null");
				currentStateName = currentState.Name;
			}
			indexList = CameraStateList.tpCameraStates.IndexOf(vThirdPersonCameraState);
			currentZoom = vThirdPersonCameraState.defaultDistance;
			currentState.fixedAngle = new Vector3(mouseX, mouseY);
			indexLookPoint = 0;
		}
		if (currentState.cameraMode == TPCameraMode.FixedPoint)
		{
			LookPoint lookPoint = currentState.lookPoints.Find((LookPoint obj) => obj.pointName.Equals(pointName));
			if (lookPoint != null)
			{
				indexLookPoint = currentState.lookPoints.IndexOf(lookPoint);
			}
			else
			{
				indexLookPoint = 0;
			}
		}
	}

	protected virtual IEnumerator ResetFirstState()
	{
		yield return new WaitForEndOfFrame();
		firstStateIsInit = true;
	}

	public virtual void ChangePoint(string pointName)
	{
		if (currentState != null && currentState.cameraMode == TPCameraMode.FixedPoint && currentState.lookPoints != null)
		{
			LookPoint lookPoint = currentState.lookPoints.Find((LookPoint obj) => obj.pointName.Equals(pointName));
			if (lookPoint != null)
			{
				indexLookPoint = currentState.lookPoints.IndexOf(lookPoint);
			}
			else
			{
				indexLookPoint = 0;
			}
		}
	}

	public virtual void FreezeCamera()
	{
		isFreezed = true;
		if ((bool)mainTarget)
		{
			lastLookAtForward = mainTarget.InverseTransformDirection(targetLookAt.forward);
			lastLookAtPosition = mainTarget.InverseTransformPoint(targetLookAt.position);
			current_cPos = mainTarget.InverseTransformPoint(current_cPos);
			desired_cPos = mainTarget.InverseTransformPoint(desired_cPos);
		}
	}

	public virtual void UnFreezeCamera()
	{
		if ((bool)mainTarget)
		{
			targetLookAt.forward = mainTarget.TransformDirection(lastLookAtForward);
			targetLookAt.position = mainTarget.TransformPoint(lastLookAtPosition);
			current_cPos = mainTarget.TransformPoint(current_cPos);
			desired_cPos = mainTarget.TransformPoint(desired_cPos);
		}
		isFreezed = false;
	}

	public virtual void Zoom(float scroolValue)
	{
		currentZoom -= scroolValue * scrollSpeed;
	}

	public virtual void CheckCameraIsRotating()
	{
		cameraIsRotating = (double)(base.transform.eulerAngles - lastCameraRotation.eulerAngles).magnitude > 0.1;
		lastCameraRotation.eulerAngles = base.transform.eulerAngles;
	}

	public virtual void RotateCamera(float x, float y)
	{
		if (currentState.cameraMode.Equals(TPCameraMode.FixedPoint) || !isInit)
		{
			smoothCameraRotation = initialCameraRotation;
		}
		else if (!currentState.cameraMode.Equals(TPCameraMode.FixedAngle))
		{
			if (!lockTarget)
			{
				mouseX += x * ((vInput.instance.inputDevice == InputDevice.Joystick) ? (currentState.xMouseSensitivity * joystickSensitivity) : currentState.xMouseSensitivity);
				mouseY -= y * ((vInput.instance.inputDevice == InputDevice.Joystick) ? (currentState.yMouseSensitivity * joystickSensitivity) : currentState.yMouseSensitivity);
				movementSpeed.x = x;
				movementSpeed.y = 0f - y;
				CheckCameraIsRotating();
				bool flag = (base.transform.forward - currentTarget.forward).magnitude <= 0.5f;
				if (!LockCamera && cameraIsRotating)
				{
					lastRotationTimer = Time.time;
					if (movementSpeed.x != 0f || movementSpeed.y != 0f)
					{
						smoothCameraRotation = initialCameraRotation;
					}
					mouseY = vExtensions.ClampAngle(mouseY, lerpState.yMinLimit, lerpState.yMaxLimit);
					mouseX = vExtensions.ClampAngle(mouseX, lerpState.xMinLimit, lerpState.xMaxLimit);
				}
				else if (LockCamera || (!flag && autoBehindTarget))
				{
					if (autoBehindTarget)
					{
						smoothCameraRotation = Mathf.Lerp(smoothCameraRotation, behindTargetSmoothRotation, 6f * Time.fixedDeltaTime);
					}
					if (LockCamera || Time.time > lastRotationTimer + behindTargetDelay)
					{
						mouseY = currentTarget.root.eulerAngles.NormalizeAngle().x;
						mouseX = currentTarget.root.eulerAngles.NormalizeAngle().y;
					}
				}
			}
			else
			{
				smoothCameraRotation = initialCameraRotation;
			}
		}
		else
		{
			smoothCameraRotation = initialCameraRotation;
			float x2 = lerpState.fixedAngle.x;
			float y2 = lerpState.fixedAngle.y;
			mouseX = (useSmooth ? Mathf.LerpAngle(mouseX, x2, smoothBetweenState * Time.fixedDeltaTime) : x2);
			mouseY = (useSmooth ? Mathf.LerpAngle(mouseY, y2, smoothBetweenState * Time.fixedDeltaTime) : y2);
		}
	}

	public virtual void SwitchRight(bool value = false)
	{
		switchRight = ((!value) ? 1 : (-1));
	}

	protected virtual void CalculeLockOnPoint()
	{
		if (currentState.cameraMode.Equals(TPCameraMode.FixedAngle) && (bool)lockTarget)
		{
			return;
		}
		Collider component = lockTarget.GetComponent<Collider>();
		if (!(component == null))
		{
			Quaternion quaternion = Quaternion.LookRotation(component.bounds.center - desired_cPos);
			float num = 0f;
			float y = quaternion.eulerAngles.y;
			num = ((quaternion.eulerAngles.x < -180f) ? (quaternion.eulerAngles.x + 360f) : ((!(quaternion.eulerAngles.x > 180f)) ? quaternion.eulerAngles.x : (quaternion.eulerAngles.x - 360f)));
			if (lockTargetWeight < 1f)
			{
				lockTargetWeight += Time.fixedDeltaTime * lockTargetSpeed;
			}
			mouseY = Mathf.LerpAngle(mouseY, vExtensions.ClampAngle(num, currentState.yMinLimit, currentState.yMaxLimit), lockTargetWeight);
			mouseX = Mathf.LerpAngle(mouseX, vExtensions.ClampAngle(y, currentState.xMinLimit, currentState.xMaxLimit), lockTargetWeight);
		}
	}

	protected virtual void CameraMovement(bool forceUpdate = false)
	{
		if (currentTarget == null || targetCamera == null || (!firstStateIsInit && !forceUpdate))
		{
			return;
		}
		transformWeight = Mathf.Clamp(transformWeight += Time.fixedDeltaTime, 0f, 1f);
		if (useSmooth)
		{
			currentState.Slerp(lerpState, smoothBetweenState * Time.fixedDeltaTime);
		}
		else
		{
			currentState.CopyState(lerpState);
		}
		if (currentState.useZoom)
		{
			currentZoom = Mathf.Clamp(currentZoom, currentState.minDistance, currentState.maxDistance);
			distance = (useSmooth ? Mathf.Lerp(distance, currentZoom, lerpState.smooth * Time.fixedDeltaTime) : currentZoom);
		}
		else
		{
			distance = (useSmooth ? Mathf.Lerp(distance, currentState.defaultDistance, lerpState.smooth * Time.fixedDeltaTime) : currentState.defaultDistance);
			currentZoom = currentState.defaultDistance;
		}
		targetCamera.fieldOfView = currentState.fov;
		cullingDistance = Mathf.Lerp(cullingDistance, currentZoom, smoothBetweenState * Time.fixedDeltaTime);
		currentSwitchRight = Mathf.Lerp(currentSwitchRight, switchRight, smoothSwitchSide * Time.fixedDeltaTime);
		Vector3 normalized = (currentState.forward * targetLookAt.forward + currentState.right * currentSwitchRight * targetLookAt.right).normalized;
		Vector3 vector = (currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y, currentTarget.position.z) + currentTarget.transform.up * offSetPlayerPivot);
		desired_cPos = vector + currentTarget.transform.up * currentState.height;
		current_cPos = (firstUpdated ? (vector + currentTarget.transform.up * currentHeight) : Vector3.SmoothDamp(current_cPos, vector + currentTarget.transform.up * currentHeight, ref cameraVelocityDamp, lerpState.smoothDamp * Time.fixedDeltaTime));
		firstUpdated = false;
		ClipPlanePoints to = targetCamera.NearClipPlanePoints(current_cPos + normalized * distance, clipPlaneMargin);
		ClipPlanePoints to2 = targetCamera.NearClipPlanePoints(desired_cPos + normalized * currentZoom, clipPlaneMargin);
		if (Physics.SphereCast(vector, checkHeightRadius, currentTarget.transform.up, out var hitInfo, currentState.cullingHeight + 0.2f, cullingLayer))
		{
			float num = hitInfo.distance - 0.2f;
			num -= currentState.height;
			num /= currentState.cullingHeight - currentState.height;
			cullingHeight = Mathf.Lerp(currentState.height, currentState.cullingHeight, Mathf.Clamp(num, 0f, 1f));
		}
		else
		{
			cullingHeight = (useSmooth ? Mathf.Lerp(cullingHeight, currentState.cullingHeight, smoothBetweenState * Time.fixedDeltaTime) : currentState.cullingHeight);
		}
		if (CullingRayCast(desired_cPos, to2, out hitInfo, currentZoom + 0.2f, cullingLayer, Color.blue))
		{
			float num2 = hitInfo.distance;
			if (num2 < currentState.defaultDistance)
			{
				float num3 = num2;
				num3 -= currentState.cullingMinDist;
				num3 /= currentZoom - currentState.cullingMinDist;
				currentHeight = Mathf.Lerp(cullingHeight, currentState.height, Mathf.Clamp(num3, 0f, 1f));
				current_cPos = vector + currentTarget.transform.up * currentHeight;
			}
		}
		else
		{
			currentHeight = (useSmooth ? Mathf.Lerp(currentHeight, currentState.height, smoothBetweenState * Time.fixedDeltaTime) : currentState.height);
		}
		if (cullingDistance < distance)
		{
			distance = cullingDistance;
		}
		if (CullingRayCast(current_cPos, to, out hitInfo, distance, cullingLayer, Color.cyan))
		{
			distance = Mathf.Clamp(cullingDistance, 0f, currentState.defaultDistance);
		}
		Vector3 vector2 = current_cPos + targetLookAt.forward * targetCamera.farClipPlane + targetLookAt.right * Vector3.Dot(normalized * distance, targetLookAt.right);
		targetLookAt.position = current_cPos;
		float num4 = Mathf.LerpAngle(mouseYStart, mouseY, transformWeight);
		Quaternion quaternion = Quaternion.Euler(y: Mathf.LerpAngle(mouseXStart, mouseX, transformWeight) + offsetMouse.x, x: num4 + offsetMouse.y, z: 0f);
		targetLookAt.rotation = (useSmooth ? Quaternion.Lerp(targetLookAt.rotation, quaternion, smoothCameraRotation * Time.fixedDeltaTime) : quaternion);
		selfRigidbody.MovePosition(Vector3.Lerp(startPosition, current_cPos + normalized * distance, transformWeight));
		Quaternion quaternion2 = Quaternion.LookRotation(vector2 - selfRigidbody.position);
		if ((bool)lockTarget)
		{
			CalculeLockOnPoint();
			if (!currentState.cameraMode.Equals(TPCameraMode.FixedAngle))
			{
				Collider component = lockTarget.GetComponent<Collider>();
				if (component != null)
				{
					Vector3 b = Quaternion.LookRotation(component.bounds.center + Vector3.up * heightOffset - selfRigidbody.position).eulerAngles - quaternion2.eulerAngles;
					if (isNewTarget)
					{
						lookTargetAdjust.x = Mathf.LerpAngle(lookTargetAdjust.x, b.x, lockTargetWeight);
						lookTargetAdjust.y = Mathf.LerpAngle(lookTargetAdjust.y, b.y, lockTargetWeight);
						lookTargetAdjust.z = Mathf.LerpAngle(lookTargetAdjust.z, b.z, lockTargetWeight);
						if (Vector3.Distance(lookTargetAdjust, b) < 0.5f)
						{
							isNewTarget = false;
						}
					}
					else
					{
						lookTargetAdjust = b;
					}
				}
			}
		}
		else
		{
			lookTargetAdjust.x = Mathf.LerpAngle(lookTargetAdjust.x, 0f, currentState.smooth * Time.fixedDeltaTime);
			lookTargetAdjust.y = Mathf.LerpAngle(lookTargetAdjust.y, 0f, currentState.smooth * Time.fixedDeltaTime);
			lookTargetAdjust.z = Mathf.LerpAngle(lookTargetAdjust.z, 0f, currentState.smooth * Time.fixedDeltaTime);
		}
		Vector3 vector3 = quaternion2.eulerAngles + lookTargetAdjust;
		vector3.z = 0f;
		Quaternion b2 = Quaternion.Euler(vector3 + currentState.rotationOffSet);
		selfRigidbody.MoveRotation(Quaternion.Lerp(startRotation, b2, transformWeight));
		movementSpeed = Vector2.zero;
	}

	protected virtual void CameraFixed()
	{
		if (useSmooth)
		{
			currentState.Slerp(lerpState, smoothBetweenState);
		}
		else
		{
			currentState.CopyState(lerpState);
		}
		transformWeight = Mathf.Clamp(transformWeight += Time.fixedDeltaTime, 0f, 1f);
		Vector3 vector = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot + currentState.height, currentTarget.position.z);
		currentTargetPos = (useSmooth ? Vector3.MoveTowards(currentTargetPos, vector, currentState.smooth * Time.fixedDeltaTime) : vector);
		current_cPos = currentTargetPos;
		Vector3 vector2 = (isValidFixedPoint ? currentState.lookPoints[indexLookPoint].positionPoint : base.transform.position);
		base.transform.position = Vector3.Lerp(startPosition, useSmooth ? Vector3.Lerp(base.transform.position, vector2, currentState.smooth * Time.fixedDeltaTime) : vector2, transformWeight);
		targetLookAt.position = current_cPos;
		if (isValidFixedPoint && currentState.lookPoints[indexLookPoint].freeRotation)
		{
			Quaternion quaternion = Quaternion.Euler(currentState.lookPoints[indexLookPoint].eulerAngle);
			base.transform.rotation = Quaternion.Lerp(startRotation, useSmooth ? Quaternion.Slerp(base.transform.rotation, quaternion, currentState.smooth * 0.5f * Time.fixedDeltaTime) : quaternion, transformWeight);
		}
		else if (isValidFixedPoint)
		{
			Quaternion quaternion2 = Quaternion.LookRotation(currentTargetPos - base.transform.position);
			base.transform.rotation = Quaternion.Lerp(startRotation, useSmooth ? Quaternion.Slerp(base.transform.rotation, quaternion2, currentState.smooth * Time.fixedDeltaTime) : quaternion2, transformWeight);
		}
		targetCamera.fieldOfView = currentState.fov;
	}

	protected virtual bool CullingRayCast(Vector3 from, ClipPlanePoints _to, out RaycastHit hitInfo, float distance, LayerMask cullingLayer, Color color)
	{
		bool flag = false;
		if (showGizmos)
		{
			Debug.DrawRay(from, _to.LowerLeft - from, color);
			Debug.DrawLine(_to.LowerLeft, _to.LowerRight, color);
			Debug.DrawLine(_to.UpperLeft, _to.UpperRight, color);
			Debug.DrawLine(_to.UpperLeft, _to.LowerLeft, color);
			Debug.DrawLine(_to.UpperRight, _to.LowerRight, color);
			Debug.DrawRay(from, _to.LowerRight - from, color);
			Debug.DrawRay(from, _to.UpperLeft - from, color);
			Debug.DrawRay(from, _to.UpperRight - from, color);
		}
		if (Physics.Raycast(from, _to.LowerLeft - from, out hitInfo, distance, cullingLayer))
		{
			flag = true;
			cullingDistance = hitInfo.distance;
		}
		if (Physics.Raycast(from, _to.LowerRight - from, out hitInfo, distance, cullingLayer))
		{
			flag = true;
			if (cullingDistance > hitInfo.distance)
			{
				cullingDistance = hitInfo.distance;
			}
		}
		if (Physics.Raycast(from, _to.UpperLeft - from, out hitInfo, distance, cullingLayer))
		{
			flag = true;
			if (cullingDistance > hitInfo.distance)
			{
				cullingDistance = hitInfo.distance;
			}
		}
		if (Physics.Raycast(from, _to.UpperRight - from, out hitInfo, distance, cullingLayer))
		{
			flag = true;
			if (cullingDistance > hitInfo.distance)
			{
				cullingDistance = hitInfo.distance;
			}
		}
		return (bool)hitInfo.collider && flag;
	}
}
