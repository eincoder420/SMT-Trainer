using Invector;
using Invector.vCamera;
using Invector.vCharacterController;
using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

[vClassHeader(" First Person Camera ", "Assign the child camera gameobject in vThirdPersonCamera into mainCamera.", iconName = "FPCameraSwapIcon")]
public class vFirstPersonCamera : vMonoBehaviour
{
	[vEditorToolbar("Camera Settings", false, "", false, false)]
	[vSeparator("Camera Postion Settings", "")]
	[Space(5f)]
	[Tooltip("Set the camera inside vThirdPersonCamera into here. If empty, the MainCamera will be used")]
	public Camera mainCamera;

	[Tooltip("Set the Camera Near Plane")]
	public float cameraNearClip = 0.01f;

	[Tooltip("Set the Camera Y offset from the head bone")]
	public float cameraYOffset = 0.1f;

	[Tooltip("Set the Camera Z offset from the head bone")]
	public float cameraZOffset = 0.02f;

	[Space(5f)]
	[vSeparator("Head Collider Settings", "")]
	[Tooltip("Enable head collision to prevent the camera from clipping into objects")]
	public bool enableHeadCollider = true;

	[vHideInInspector("enableHeadCollider", false)]
	[Tooltip("Show head collision Gizmos")]
	public bool showGizmos = true;

	[vHideInInspector("enableHeadCollider", false)]
	[Tooltip("Head collision radius")]
	public float colliderRadius = 0.12f;

	[vHideInInspector("enableHeadCollider", false)]
	[Tooltip("Head collision center")]
	public Vector3 colliderCenter = new Vector3(0f, 0.1f, 0.04f);

	[Space(20f)]
	[vSeparator("Action Angle Limit", "")]
	[Space(5f)]
	[Tooltip("The 'Horizontal' clamp angle for head look during actions")]
	[Range(0f, 90f)]
	public float actionHAngleLimit = 90f;

	[vSeparator("Body Rotation Settings", "")]
	[Space(5f)]
	[Tooltip("Set the default Animator Update Mode")]
	public AnimatorUpdateMode animatorUpdateMode = AnimatorUpdateMode.AnimatePhysics;

	[Space(5f)]
	[Tooltip("Set the strafe body IK reactivity respect head rotation")]
	[Range(0f, 2f)]
	public float strafeBodyIKWeight = 1.25f;

	[Space(5f)]
	[Tooltip("Set the free body IK reactivity respect head rotation")]
	[Range(0f, 2f)]
	public float freeBodyIKWeight = 1.25f;

	[Space(5f)]
	[Tooltip("The threshold angle between player head and player body; beyond which the rotation begins")]
	[Range(0f, 70f)]
	public float RotationThld = 55f;

	[Space(5f)]
	[vSeparator("Controller Settings", "")]
	[Space(5f)]
	[Tooltip("use cinematic camera during DEFAULT actions")]
	public bool cinematicOnActions;

	[Tooltip("use cinematic camera by external calls")]
	public bool cinematicOnRequest = true;

	[Tooltip("add Crosshair UI prefab at start")]
	public bool addCrosshair = true;

	[vEditorToolbar("Camera Mode", false, "", false, false)]
	[vSeparator("Camera Mode Settings", "")]
	[Space(5f)]
	[Tooltip("Assign keyboard button for camera mode")]
	public KeyCode cameraModeKey = KeyCode.F;

	[Space(5f)]
	[Tooltip("Set Third Person as default mode on start")]
	public bool isThirdPerson;

	[Space(5f)]
	[Header("Third Person Settings")]
	[Tooltip("Set default loomotion type")]
	public vThirdPersonMotor.LocomotionType defaultThirdLocomotion;

	[Space(5f)]
	[Tooltip("Force strafe mode in FreeWithStrafe")]
	public bool thrdCameraDefaultStrafe;

	[Space(5f)]
	[vSeparator("Events", "")]
	[Space(5f)]
	public UnityEvent FirstPersonMode;

	[Space(5f)]
	public UnityEvent ThirdPersonMode;

	private bool isAction;

	private bool isCinematic;

	private bool isUpdateModeNormal;

	private bool stateDone;

	private vThirdPersonInput vInput;

	private vHeadTrack vHeadT;

	private Animator animator;

	private Transform headBone;

	private GameObject headBoneRef;

	private GameObject headBoneRotCorrection;

	public vAnimatorStateInfos animatorStateInfos;

	private bool isCustomAction;

	private bool lateUpdateSync;

	private GameObject headCollider;

	private bool headColliderStatus = true;

	private vThirdPersonCamera tpCamera;

	private bool cameraModeLast;

	private bool startJumpandRotate;

	private void Start()
	{
		tpCamera = Object.FindObjectOfType<vThirdPersonCamera>();
		if (mainCamera == null)
		{
			mainCamera = Camera.main.gameObject.GetComponent<Camera>();
		}
		mainCamera.GetComponent<Camera>().nearClipPlane = cameraNearClip;
		vInput = GetComponent<vThirdPersonInput>();
		startJumpandRotate = vInput.cc.jumpAndRotate;
		if (!isThirdPerson)
		{
			vInput.cc.locomotionType = vThirdPersonMotor.LocomotionType.OnlyStrafe;
			vInput.cc.sprintOnlyFree = false;
			vInput.cc.strafeSpeed.rotateWithCamera = false;
		}
		vHeadT = GetComponent<vHeadTrack>();
		vHeadT.strafeBodyWeight = strafeBodyIKWeight;
		vHeadT.freeBodyWeight = freeBodyIKWeight;
		animator = GetComponent<Animator>();
		headBone = animator.GetBoneTransform(HumanBodyBones.Head);
		animatorStateInfos = new vAnimatorStateInfos(animator);
		animatorStateInfos.RegisterListener();
		animator.updateMode = animatorUpdateMode;
		if (animator.updateMode != AnimatorUpdateMode.AnimatePhysics)
		{
			isUpdateModeNormal = true;
		}
		else
		{
			isUpdateModeNormal = false;
		}
		headColliderStatus = enableHeadCollider;
		if (enableHeadCollider)
		{
			headCollider = new GameObject("HeadCollision");
			headCollider.AddComponent<vFPCameraHeadCollider>();
			headCollider.layer = 15;
			headCollider.tag = "Player";
			headCollider.AddComponent<SphereCollider>();
			headCollider.GetComponent<SphereCollider>().radius = colliderRadius;
			headCollider.GetComponent<SphereCollider>().center = colliderCenter;
			headCollider.transform.parent = base.transform;
			headCollider.transform.localRotation = Quaternion.identity;
		}
		headBoneRef = new GameObject("HeadRef");
		headBoneRotCorrection = new GameObject("FPCameraRoot");
		Vector3 vector = base.transform.root.forward * cameraZOffset + base.transform.root.up * cameraYOffset;
		headBoneRef.transform.position = headBone.transform.position + vector;
		headBoneRotCorrection.transform.position = headBone.transform.position;
		headBoneRotCorrection.transform.rotation = headBone.transform.root.rotation;
		headBoneRef.transform.rotation = headBone.transform.rotation;
		headBoneRef.transform.parent = headBoneRotCorrection.transform;
		for (int i = 0; i < animator.layerCount; i++)
		{
			if (animator.GetLayerName(i) == "UnderBody")
			{
				animator.SetLayerWeight(i, 0f);
			}
		}
	}

	private void FixedUpdate()
	{
		lateUpdateSync = true;
		if (!vInput.cc.ragdolled && Time.timeScale != 0f && !isCinematic && !isAction)
		{
			CameraLook();
		}
	}

	private void LateUpdate()
	{
		if (isUpdateModeNormal)
		{
			lateUpdateSync = true;
		}
		if (!lateUpdateSync)
		{
			return;
		}
		lateUpdateSync = false;
		if (!vInput.cc.ragdolled && Time.timeScale != 0f)
		{
			if (cinematicOnActions)
			{
				if (vInput.cc.customAction)
				{
					isCinematic = true;
					stateDone = false;
				}
				else if (!vInput.cc.customAction && !stateDone)
				{
					isCinematic = false;
					stateDone = true;
				}
			}
			FaceToCamera();
			CameraHeadBonePosition();
			if (isCinematic)
			{
				CinematicCam();
				return;
			}
			CharacterRotation();
			if (isAction)
			{
				CameraLook();
			}
			CameraHeadBoneRotation();
		}
		else
		{
			CinematicCam();
		}
	}

	private void Update()
	{
		if (animatorStateInfos.HasTag("CustomAction") || animatorStateInfos.HasTag("LockMovement"))
		{
			isCustomAction = true;
		}
		else
		{
			isCustomAction = false;
		}
		if (animator.updateMode != AnimatorUpdateMode.AnimatePhysics)
		{
			isUpdateModeNormal = true;
		}
		else
		{
			isUpdateModeNormal = false;
		}
		if (Input.GetKeyDown(cameraModeKey))
		{
			FpcSwap();
		}
		if (cameraModeLast != isThirdPerson)
		{
			if (isThirdPerson)
			{
				ThirdPersonEvent();
				if (headColliderStatus)
				{
					enableHeadCollider = false;
					headCollider.SetActive(value: false);
				}
				vInput.cc.locomotionType = defaultThirdLocomotion;
				if (thrdCameraDefaultStrafe)
				{
					vInput.cc.isStrafing = true;
				}
				else
				{
					vInput.cc.isStrafing = false;
				}
				cameraModeLast = isThirdPerson;
			}
			else if (!isThirdPerson)
			{
				FirstPersonEvent();
				headCollider.SetActive(headColliderStatus);
				enableHeadCollider = true;
				vInput.cc.locomotionType = vThirdPersonMotor.LocomotionType.OnlyStrafe;
				vInput.cc.sprintOnlyFree = false;
				vInput.cc.strafeSpeed.rotateWithCamera = false;
				cameraModeLast = isThirdPerson;
			}
		}
		if (!isThirdPerson)
		{
			float min = base.transform.eulerAngles.y + (0f - actionHAngleLimit);
			float max = base.transform.eulerAngles.y + actionHAngleLimit;
			float num = Mathf.Clamp(tpCamera.mouseX, min, max);
			if (tpCamera.mouseX > num + 100f)
			{
				tpCamera.mouseX -= 360f;
			}
			else if (tpCamera.mouseX < num - 100f)
			{
				tpCamera.mouseX += 360f;
			}
			if (vInput.cc.customAction || isAction || isCustomAction)
			{
				tpCamera.mouseX = Mathf.Clamp(tpCamera.mouseX, min, max);
				tpCamera.mouseY = Mathf.Clamp(tpCamera.mouseY, tpCamera.lerpState.yMinLimit, tpCamera.lerpState.yMaxLimit);
			}
		}
	}

	private void CameraHeadBonePosition()
	{
		if (!isThirdPerson)
		{
			headBoneRotCorrection.transform.position = headBone.transform.position;
			mainCamera.transform.position = headBoneRef.transform.position;
			if (enableHeadCollider)
			{
				headCollider.transform.position = headBone.transform.position;
			}
		}
		else
		{
			mainCamera.transform.position = tpCamera.transform.position;
		}
	}

	private void CameraHeadBoneRotation()
	{
		if (!isThirdPerson)
		{
			headBone.rotation = headBoneRef.transform.rotation;
			mainCamera.transform.rotation = tpCamera.transform.rotation;
			if (enableHeadCollider)
			{
				headCollider.transform.rotation = headBone.transform.rotation;
			}
		}
	}

	private void CinematicCam()
	{
		if (!isThirdPerson && Time.timeScale != 0f)
		{
			tpCamera.mouseY = base.transform.eulerAngles.NormalizeAngle().x;
			tpCamera.mouseX = base.transform.eulerAngles.NormalizeAngle().y;
			headBoneRotCorrection.transform.position = headBone.transform.position;
			headBoneRotCorrection.transform.rotation = headBone.transform.rotation;
			mainCamera.transform.position = headBoneRef.transform.position;
			mainCamera.transform.rotation = headBone.transform.rotation;
		}
	}

	private void CameraLook()
	{
		headBoneRotCorrection.transform.rotation = tpCamera.transform.rotation;
	}

	private void CharacterRotation()
	{
		if (isThirdPerson)
		{
			return;
		}
		Quaternion identity = Quaternion.identity;
		if (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f)
		{
			return;
		}
		Vector3 vector = base.transform.InverseTransformDirection(mainCamera.transform.forward);
		float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		float num2 = 0f;
		if (Mathf.Abs(num) > Mathf.Abs(RotationThld))
		{
			num2 = num - RotationThld;
			if (num < 0f)
			{
				num2 = num + RotationThld;
			}
			if (!isAction && !isCustomAction && !vInput.cc.customAction)
			{
				identity = Quaternion.AngleAxis(num2, base.transform.up) * base.transform.rotation;
				if (!isUpdateModeNormal)
				{
					base.transform.rotation = Quaternion.Lerp(base.transform.rotation, identity, Time.fixedDeltaTime * vInput.cc.strafeSpeed.rotationSpeed);
				}
				else
				{
					base.transform.rotation = Quaternion.Lerp(base.transform.rotation, identity, Time.deltaTime * vInput.cc.strafeSpeed.rotationSpeed);
				}
			}
		}
		else
		{
			identity = base.transform.rotation;
		}
	}

	private void FaceToCamera()
	{
		if (!isThirdPerson)
		{
			if (vInput.cc.input.z < 0f && (!vInput.cc.isGrounded || vInput.cc.isInAirborne))
			{
				vInput.cc.jumpAndRotate = false;
				Quaternion b = Quaternion.Euler(0f, tpCamera.mouseX, 0f);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, Time.fixedDeltaTime * vInput.cc.strafeSpeed.rotationSpeed);
			}
			else
			{
				vInput.cc.jumpAndRotate = startJumpandRotate;
			}
			if (animatorStateInfos.HasTag("Attack") || animatorStateInfos.HasTag("isBlocking"))
			{
				Quaternion b2 = Quaternion.Euler(0f, tpCamera.mouseX, 0f);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b2, Time.fixedDeltaTime * vInput.cc.strafeSpeed.rotationSpeed);
			}
			if (vInput.cc.IsAnimatorTag("isSwimming"))
			{
				headCollider.SetActive(value: false);
				Quaternion b3 = Quaternion.Euler(0f, tpCamera.mouseX, 0f);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b3, Time.fixedDeltaTime * vInput.cc.strafeSpeed.rotationSpeed);
			}
			else
			{
				headCollider.SetActive(headColliderStatus);
			}
		}
	}

	public void IsAction(bool status)
	{
		isAction = status;
		if (isAction)
		{
			headBoneRotCorrection.transform.parent = base.transform.root;
		}
		else
		{
			headBoneRotCorrection.transform.parent = null;
		}
	}

	public void IsCinematic(bool state)
	{
		if (cinematicOnRequest)
		{
			isCinematic = state;
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (mainCamera != null && showGizmos && enableHeadCollider)
		{
			animator = GetComponent<Animator>();
			headBone = animator.GetBoneTransform(HumanBodyBones.Head);
			Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
			Gizmos.DrawSphere(headBone.transform.position + headBone.transform.forward * colliderCenter.z + headBone.transform.up * colliderCenter.y + headBone.transform.right * colliderCenter.x, colliderRadius);
		}
	}

	private void FirstPersonEvent()
	{
		FirstPersonMode.Invoke();
	}

	private void ThirdPersonEvent()
	{
		ThirdPersonMode.Invoke();
	}

	public void FpcSwap()
	{
		isThirdPerson = !isThirdPerson;
	}

	public void FpcSetThirdMode(bool value)
	{
		isThirdPerson = value;
	}
}
