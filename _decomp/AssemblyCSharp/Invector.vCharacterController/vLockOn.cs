using System;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController;

[vClassHeader("MELEE LOCK-ON", true, "icon_v2", false, "")]
public class vLockOn : vLockOnBehaviour
{
	[Serializable]
	public class LockOnEvent : UnityEvent<Transform>
	{
	}

	[Tooltip("Make sure to disable or change the StrafeInput to a different key at the Player Input component")]
	public bool strafeWhileLockOn = true;

	[Tooltip("Create a Image inside the UI and assign here")]
	public RectTransform aimImagePrefab;

	public Canvas aimImageContainer;

	public Vector2 aimImageSize = new Vector2(30f, 30f);

	[Tooltip("True: Hide the sprite when not Lock On, False: Always show the Sprite")]
	public bool hideSprite = true;

	[Tooltip("Create a offset for the sprite based at the center of the target")]
	[Range(-0.5f, 0.5f)]
	public float spriteHeight = 0.25f;

	[Tooltip("Offset for the camera height")]
	public float cameraHeightOffset;

	[Tooltip("Transition Speed for the Camera")]
	public float lockSpeed = 0.5f;

	[Header("LockOn Inputs")]
	public GenericInput lockOnInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");

	public GenericInput nexTargetInput = new GenericInput("X", keyboardAxis: false, keyboardInvert: false, "RightAnalogHorizontal", joystickAxis: true, joystickInvert: false, "X", mobileAxis: false, mobileInvert: false);

	public GenericInput previousTargetInput = new GenericInput("Z", keyboardAxis: false, keyboardInvert: false, "RightAnalogHorizontal", joystickAxis: true, joystickInvert: true, "Z", mobileAxis: false, mobileInvert: false);

	internal bool isLockingOn;

	public LockOnEvent onLockOnTarget;

	public LockOnEvent onUnLockOnTarget;

	private Canvas _aimCanvas;

	private RectTransform _aimImage;

	protected bool inTarget;

	protected vMeleeCombatInput tpInput;

	public RectTransform aimImage
	{
		get
		{
			if ((bool)_aimImage)
			{
				return _aimImage;
			}
			if ((bool)aimImageContainer)
			{
				_aimImage = UnityEngine.Object.Instantiate(aimImagePrefab, Vector2.zero, Quaternion.identity);
				_aimImage.SetParent(aimImageContainer.transform);
				return _aimImage;
			}
			Debug.LogWarning("Missing UI Canvas in the scene, please add one");
			return null;
		}
	}

	protected virtual void Start()
	{
		Init();
		tpInput = GetComponent<vMeleeCombatInput>();
		if ((bool)tpInput)
		{
			tpInput.onUpdate -= UpdateLockOn;
			tpInput.onUpdate += UpdateLockOn;
			GetComponent<vHealthController>().onDead.AddListener(delegate
			{
				isLockingOn = false;
				LockOn(value: false);
				UpdateLockOn();
			});
		}
		if (!aimImageContainer)
		{
			aimImageContainer = base.gameObject.GetComponentInChildren<Canvas>(includeInactive: true);
		}
	}

	protected virtual void UpdateLockOn()
	{
		if (!(tpInput == null))
		{
			LockOnInput();
			SwitchTargetsInput();
			CheckForCharacterAlive();
			UpdateAimImage();
		}
	}

	protected virtual void LockOnInput()
	{
		if (tpInput.tpCamera == null || tpInput.cc == null)
		{
			return;
		}
		if (lockOnInput.GetButtonDown() && !tpInput.cc.customAction)
		{
			isLockingOn = !isLockingOn;
			LockOn(isLockingOn);
		}
		else if (isLockingOn && tpInput.tpCamera.lockTarget == null)
		{
			isLockingOn = false;
			LockOn(value: false);
		}
		if (strafeWhileLockOn && !tpInput.cc.locomotionType.Equals(vThirdPersonMotor.LocomotionType.OnlyStrafe))
		{
			if (isLockingOn && tpInput.tpCamera.lockTarget != null)
			{
				tpInput.cc.lockInStrafe = true;
				tpInput.cc.isStrafing = true;
			}
			else
			{
				tpInput.cc.lockInStrafe = false;
				tpInput.cc.isStrafing = false;
			}
		}
	}

	protected override void SetTarget()
	{
		if (tpInput.tpCamera != null)
		{
			tpInput.tpCamera.SetLockTarget(currentTarget.transform, cameraHeightOffset, lockSpeed);
			onLockOnTarget.Invoke(currentTarget);
		}
	}

	protected virtual void SwitchTargetsInput()
	{
		if (!(tpInput.tpCamera == null) && (bool)tpInput.tpCamera.lockTarget)
		{
			if (previousTargetInput.GetButtonDown())
			{
				PreviousTarget();
			}
			else if (nexTargetInput.GetButtonDown())
			{
				NextTarget();
			}
		}
	}

	protected virtual void CheckForCharacterAlive()
	{
		if (((bool)currentTarget && !isCharacterAlive() && inTarget) || (inTarget && !isCharacterAlive()))
		{
			ResetLockOn();
			inTarget = false;
			LockOn(value: true);
			StopLockOn();
		}
	}

	protected virtual void LockOn(bool value)
	{
		base.UpdateLockOn(value);
		if (!inTarget && (bool)currentTarget)
		{
			inTarget = true;
			SetTarget();
		}
		else if (inTarget && !currentTarget)
		{
			inTarget = false;
			StopLockOn();
		}
	}

	protected virtual void UpdateAimImage()
	{
		if (!aimImageContainer || !aimImage)
		{
			return;
		}
		if (hideSprite)
		{
			aimImage.sizeDelta = aimImageSize;
			if ((bool)currentTarget && !aimImage.transform.gameObject.activeSelf && isCharacterAlive())
			{
				aimImage.transform.gameObject.SetActive(value: true);
			}
			else if (!currentTarget && aimImage.transform.gameObject.activeSelf)
			{
				aimImage.transform.gameObject.SetActive(value: false);
			}
			else if (_aimImage.transform.gameObject.activeSelf && !isCharacterAlive())
			{
				aimImage.transform.gameObject.SetActive(value: false);
			}
		}
		if ((bool)currentTarget && (bool)aimImage && (bool)aimImageContainer)
		{
			aimImage.anchoredPosition = currentTarget.GetScreenPointOffBoundsCenter(aimImageContainer, tpCamera.targetCamera, spriteHeight);
		}
		else if ((bool)aimImageContainer)
		{
			aimImage.anchoredPosition = Vector2.zero;
		}
	}

	public virtual void StopLockOn()
	{
		if (currentTarget == null && tpInput.tpCamera != null)
		{
			onUnLockOnTarget.Invoke(tpInput.tpCamera.lockTarget);
			tpInput.tpCamera.RemoveLockTarget();
			isLockingOn = false;
			inTarget = false;
		}
	}

	public virtual void NextTarget()
	{
		base.ChangeTarget(1);
	}

	public virtual void PreviousTarget()
	{
		base.ChangeTarget(-1);
	}
}
