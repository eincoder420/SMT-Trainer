using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Invector.vCamera;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController;

[vClassHeader("Input Manager", true, "icon_v2", false, "", iconName = "inputIcon")]
public class vThirdPersonInput : vMonoBehaviour, vIAnimatorMoveReceiver
{
	public delegate void OnUpdateEvent();

	private Roxanne_Control player;

	private Inventory_Script inventory;

	[vEditorToolbar("Inputs", false, "", false, false)]
	[vHelpBox("Check these options if you need to use the mouse cursor, ex: <b>2.5D, Topdown or Mobile</b>", vHelpBoxAttribute.MessageType.Info)]
	public bool unlockCursorOnStart;

	public bool showCursorOnStart;

	[vHelpBox("PC only - use it to toggle between run/walk", vHelpBoxAttribute.MessageType.Info)]
	public KeyCode toggleWalk = KeyCode.CapsLock;

	[Header("Movement Input")]
	public GenericInput horizontalInput = new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal");

	public GenericInput verticallInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");

	public GenericInput sprintInput = new GenericInput("LeftShift", "LeftStickClick", "LeftStickClick");

	public GenericInput crouchInput = new GenericInput("C", "Y", "Y");

	public GenericInput strafeInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");

	public GenericInput jumpInput = new GenericInput("Space", "X", "X");

	public GenericInput rollInput = new GenericInput("Q", "B", "B");

	[HideInInspector]
	public bool lockInput;

	[vEditorToolbar("Camera Settings", false, "", false, false)]
	public bool lockCameraInput;

	public bool invertCameraInputVertical;

	public bool invertCameraInputHorizontal;

	[vEditorToolbar("Inputs", false, "", false, false)]
	[Header("Camera Input")]
	public GenericInput rotateCameraXInput = new GenericInput("Mouse X", "RightAnalogHorizontal", "Mouse X");

	public GenericInput rotateCameraYInput = new GenericInput("Mouse Y", "RightAnalogVertical", "Mouse Y");

	public GenericInput cameraZoomInput = new GenericInput("Mouse ScrollWheel", "", "");

	[vEditorToolbar("Events", false, "", false, false)]
	public UnityEvent OnLockCamera;

	public UnityEvent OnUnlockCamera;

	public UnityEvent onEnableAnimatorMove = new UnityEvent();

	public UnityEvent onDisableDisableAnimatorMove = new UnityEvent();

	[HideInInspector]
	public vThirdPersonCamera tpCamera;

	[HideInInspector]
	public bool ignoreTpCamera;

	[HideInInspector]
	public string customCameraState;

	[HideInInspector]
	public string customlookAtPoint;

	[HideInInspector]
	public bool changeCameraState;

	[HideInInspector]
	public bool smoothCameraState;

	[HideInInspector]
	public vThirdPersonController cc;

	[HideInInspector]
	public vHUDController hud;

	protected bool updateIK;

	protected bool isInit;

	[HideInInspector]
	public bool lockMoveInput;

	protected Camera _cameraMain;

	protected bool withoutMainCamera;

	internal bool lockUpdateMoveDirection;

	protected InputDevice inputDevice => vInput.instance.inputDevice;

	public Camera cameraMain
	{
		get
		{
			if (!_cameraMain && !withoutMainCamera)
			{
				if (!Camera.main)
				{
					Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
					withoutMainCamera = true;
				}
				else
				{
					_cameraMain = Camera.main;
					cc.rotateTarget = _cameraMain.transform;
				}
			}
			return _cameraMain;
		}
		set
		{
			_cameraMain = value;
		}
	}

	public Animator animator
	{
		get
		{
			if (cc == null)
			{
				cc = GetComponent<vThirdPersonController>();
			}
			if (cc.animator == null)
			{
				return GetComponent<Animator>();
			}
			return cc.animator;
		}
	}

	internal virtual vAnimatorMoveSender animatorMoveSender { get; set; }

	protected bool _useAnimatorMove { get; set; }

	public virtual bool UseAnimatorMove
	{
		get
		{
			return _useAnimatorMove;
		}
		set
		{
			if (_useAnimatorMove != value)
			{
				if (value)
				{
					animatorMoveSender = base.gameObject.AddComponent<vAnimatorMoveSender>();
					onEnableAnimatorMove?.Invoke();
				}
				else
				{
					if ((bool)animatorMoveSender)
					{
						UnityEngine.Object.Destroy(animatorMoveSender);
					}
					onEnableAnimatorMove?.Invoke();
				}
			}
			_useAnimatorMove = value;
		}
	}

	public event OnUpdateEvent onUpdate;

	public event OnUpdateEvent onLateUpdate;

	public event OnUpdateEvent onFixedUpdate;

	public event OnUpdateEvent onAnimatorMove;

	protected virtual void Start()
	{
		player = GetComponent<Roxanne_Control>();
		inventory = GetComponent<Inventory_Script>();
		cc = GetComponent<vThirdPersonController>();
		if (cc != null)
		{
			cc.Init();
		}
		StartCoroutine(CharacterInit());
		ShowCursor(showCursorOnStart);
		LockCursor(unlockCursorOnStart);
		EnableOnAnimatorMove();
	}

	protected virtual IEnumerator CharacterInit()
	{
		FindCamera();
		yield return new WaitForEndOfFrame();
		FindHUD();
	}

	public virtual void FindHUD()
	{
		if (hud == null && vHUDController.instance != null)
		{
			hud = vHUDController.instance;
			hud.Init(cc);
		}
	}

	public virtual void FindCamera()
	{
		vThirdPersonCamera[] array = UnityEngine.Object.FindObjectsOfType<vThirdPersonCamera>();
		if (array.Length > 1)
		{
			tpCamera = Array.Find(array, (vThirdPersonCamera tp) => !tp.isInit);
			if (tpCamera == null)
			{
				tpCamera = array[0];
			}
			if (tpCamera != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (tpCamera != array[i])
					{
						UnityEngine.Object.Destroy(array[i].gameObject);
					}
				}
			}
		}
		else if (array.Length == 1)
		{
			tpCamera = array[0];
		}
		if ((bool)tpCamera && tpCamera.mainTarget != base.transform)
		{
			tpCamera.SetMainTarget(base.transform);
		}
	}

	protected virtual void LateUpdate()
	{
		if (!(cc == null) && Time.timeScale != 0f && updateIK)
		{
			if (this.onLateUpdate != null)
			{
				this.onLateUpdate();
			}
			CameraInput();
			UpdateCameraStates();
			updateIK = false;
		}
	}

	protected virtual void FixedUpdate()
	{
		if (this.onFixedUpdate != null)
		{
			this.onFixedUpdate();
		}
		Physics.SyncTransforms();
		cc.UpdateMotor();
		cc.ControlLocomotionType();
		ControlRotation();
		cc.UpdateAnimator();
		updateIK = true;
	}

	protected virtual void Update()
	{
		if (!(cc == null) && Time.timeScale != 0f)
		{
			if (this.onUpdate != null)
			{
				this.onUpdate();
			}
			InputHandle();
			UpdateHUD();
		}
	}

	public virtual void OnAnimatorMoveEvent()
	{
		if (!(cc == null))
		{
			cc.ControlAnimatorRootMotion();
			if (this.onAnimatorMove != null)
			{
				this.onAnimatorMove();
			}
		}
	}

	public virtual void SetLockBasicInput(bool value)
	{
		lockInput = value;
	}

	public virtual void SetLockAllInput(bool value)
	{
		SetLockBasicInput(value);
	}

	public virtual void ShowCursor(bool value)
	{
		Cursor.visible = value;
	}

	public virtual void LockCursor(bool value)
	{
		if (!value)
		{
			Cursor.lockState = CursorLockMode.Locked;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
		}
	}

	public virtual void SetLockCameraInput(bool value)
	{
		lockCameraInput = value;
		if (lockCameraInput)
		{
			OnLockCamera.Invoke();
		}
		else
		{
			OnUnlockCamera.Invoke();
		}
	}

	public virtual void SetLockUpdateMoveDirection(bool value)
	{
		lockUpdateMoveDirection = value;
	}

	public virtual void SetWalkByDefault(bool value)
	{
		cc.freeSpeed.walkByDefault = value;
		cc.strafeSpeed.walkByDefault = value;
	}

	public virtual void SetStrafeLocomotion(bool value)
	{
		cc.lockInStrafe = value;
		cc.isStrafing = value;
	}

	public virtual void EnableOnAnimatorMove()
	{
		UseAnimatorMove = true;
	}

	public virtual void DisableOnAnimatorMove()
	{
		UseAnimatorMove = false;
	}

	protected virtual void InputHandle()
	{
		if (!lockInput && !cc.ragdolled)
		{
			MoveInput();
			SprintInput();
			CrouchInput();
			StrafeInput();
			JumpInput();
			RollInput();
		}
	}

	public virtual void MoveInput()
	{
		if (!lockMoveInput)
		{
			cc.input.x = horizontalInput.GetAxisRaw();
			cc.input.z = verticallInput.GetAxisRaw();
		}
		if (Input.GetKeyDown(toggleWalk))
		{
			cc.alwaysWalkByDefault = !cc.alwaysWalkByDefault;
		}
		cc.ControlKeepDirection();
	}

	public virtual void ControlRotation()
	{
		if ((bool)cameraMain && !lockUpdateMoveDirection && !cc.keepDirection)
		{
			cc.UpdateMoveDirection(cameraMain.transform);
		}
		if (tpCamera != null && (bool)tpCamera.lockTarget && cc.isStrafing)
		{
			cc.RotateToPosition(tpCamera.lockTarget.position);
		}
		else
		{
			cc.ControlRotationType();
		}
	}

	protected virtual void StrafeInput()
	{
		if (strafeInput.GetButtonDown())
		{
			cc.Strafe();
		}
	}

	protected virtual void SprintInput()
	{
		if (sprintInput.useInput)
		{
			cc.Sprint(cc.useContinuousSprint ? sprintInput.GetButtonDown() : sprintInput.GetButton());
		}
	}

	protected virtual void CrouchInput()
	{
		cc.AutoCrouch();
		if (crouchInput.useInput && crouchInput.GetButtonDown() && !player.Showing && !inventory.Wearing)
		{
			cc.Crouch();
		}
	}

	protected virtual bool JumpConditions()
	{
		if (!cc.customAction && !cc.isCrouching && cc.isGrounded && cc.GroundAngle() < cc.slopeLimit && cc.currentStamina >= cc.jumpStamina && !cc.isJumping)
		{
			return !cc.isRolling;
		}
		return false;
	}

	protected virtual void JumpInput()
	{
		if (jumpInput.GetButtonDown() && JumpConditions() && !player.Showing && !player.Masturbating && !inventory.Wearing)
		{
			cc.Jump(consumeStamina: true);
		}
	}

	protected virtual bool RollConditions()
	{
		if ((!cc.isRolling || cc.canRollAgain) && cc.isGrounded && cc.input != Vector3.zero && !cc.customAction && cc.currentStamina > cc.rollStamina && !cc.isJumping)
		{
			return !cc.isSliding;
		}
		return false;
	}

	protected virtual void RollInput()
	{
		if (rollInput.GetButtonDown() && RollConditions())
		{
			cc.Roll();
		}
	}

	public virtual void CameraInput()
	{
		if ((bool)cameraMain && !(tpCamera == null))
		{
			float num = (lockCameraInput ? 0f : rotateCameraYInput.GetAxis());
			float num2 = (lockCameraInput ? 0f : rotateCameraXInput.GetAxis());
			if (invertCameraInputHorizontal)
			{
				num2 *= -1f;
			}
			if (invertCameraInputVertical)
			{
				num *= -1f;
			}
			float axis = cameraZoomInput.GetAxis();
			if (!inventory.data.Display.Freeze_Mouse)
			{
				tpCamera.RotateCamera(num2, num);
			}
			else if (Input.GetMouseButton(2))
			{
				tpCamera.RotateCamera(num2, num);
			}
			if (!lockCameraInput)
			{
				tpCamera.Zoom(axis);
			}
		}
	}

	public virtual void UpdateCameraStates()
	{
		if (ignoreTpCamera)
		{
			return;
		}
		if (tpCamera == null)
		{
			tpCamera = UnityEngine.Object.FindObjectOfType<vThirdPersonCamera>();
			if (tpCamera == null)
			{
				return;
			}
			if ((bool)tpCamera)
			{
				tpCamera.SetMainTarget(base.transform);
				tpCamera.Init();
			}
		}
		if (changeCameraState)
		{
			tpCamera.ChangeState(customCameraState, customlookAtPoint, smoothCameraState);
		}
		else if (cc.isCrouching)
		{
			tpCamera.ChangeState("Crouch", hasSmooth: true);
		}
		else if (cc.isStrafing)
		{
			tpCamera.ChangeState("Strafing", hasSmooth: true);
		}
		else
		{
			tpCamera.ChangeState("Default", hasSmooth: true);
		}
	}

	public virtual void ChangeCameraState(string cameraState, bool useLerp = true)
	{
		if (useLerp)
		{
			ChangeCameraStateWithLerp(cameraState);
		}
		else
		{
			ChangeCameraStateNoLerp(cameraState);
		}
	}

	public virtual void ResetCameraAngle()
	{
		if ((bool)tpCamera)
		{
			tpCamera.ResetAngle();
		}
	}

	public virtual void ChangeCameraStateWithLerp(string cameraState)
	{
		changeCameraState = true;
		customCameraState = cameraState;
		smoothCameraState = true;
	}

	public virtual void ChangeCameraStateNoLerp(string cameraState)
	{
		changeCameraState = true;
		customCameraState = cameraState;
		smoothCameraState = false;
	}

	public virtual void ResetCameraState()
	{
		changeCameraState = false;
		customCameraState = string.Empty;
	}

	public virtual void UpdateHUD()
	{
		if (hud == null)
		{
			if (!(vHUDController.instance != null))
			{
				return;
			}
			hud = vHUDController.instance;
			hud.Init(cc);
		}
		hud.UpdateHUD(cc);
	}

	[SpecialName]
	bool vIAnimatorMoveReceiver.get_enabled()
	{
		return base.enabled;
	}

	[SpecialName]
	void vIAnimatorMoveReceiver.set_enabled(bool value)
	{
		base.enabled = value;
	}
}
