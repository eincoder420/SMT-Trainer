using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.vActions;

[vClassHeader("Ladder Action", true, "icon_v2", false, "", iconName = "ladderIcon")]
public class vLadderAction : vActionListener
{
	[vEditorToolbar("Settings", false, "", false, true, order = 0)]
	[Tooltip("Tag of the object you want to access")]
	public string actionTag = "LadderTrigger";

	[Tooltip("Speed multiplier for the climb ladder animations")]
	public float climbSpeed = 1.5f;

	[Tooltip("Speed multiplier for the climb ladder animations when the fastClimbInput is pressed")]
	public float fastClimbSpeed = 3f;

	[Tooltip("How much Stamina will be consumed when climbing faster")]
	public float fastClimbStamina = 30f;

	[Tooltip("Input to use the ladder going up or down")]
	public GenericInput verticallInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");

	[Tooltip("Input to enter the ladder")]
	public GenericInput enterInput = new GenericInput("E", "A", "A");

	[Tooltip("Input to exit the ladder")]
	public GenericInput exitInput = new GenericInput("Space", "B", "B");

	[Tooltip("Input to climb faster")]
	public GenericInput fastClimbInput = new GenericInput("LeftShift", "LeftStickClick", "LeftStickClick");

	[Tooltip("Input to climb faster")]
	public GenericInput slideDownInput = new GenericInput("Q", "X", "X");

	[vEditorToolbar("Events", false, "", false, false)]
	public UnityEvent OnEnterLadder;

	public UnityEvent OnExitLadder;

	public UnityEvent OnEnterTriggerLadder;

	public UnityEvent OnExitTriggerLadder;

	[vEditorToolbar("Debug", false, "", false, false)]
	public bool debugMode;

	[vReadOnly(false)]
	[SerializeField]
	protected vTriggerLadderAction targetLadderAction;

	[vReadOnly(false)]
	[SerializeField]
	protected vTriggerLadderAction currentLadderAction;

	protected List<vTriggerLadderAction> actionTriggers = new List<vTriggerLadderAction>();

	[vReadOnly(false)]
	[SerializeField]
	protected float speed;

	[vReadOnly(false)]
	[SerializeField]
	protected float currentClimbSpeed;

	[vReadOnly(false)]
	[SerializeField]
	protected bool isUsingLadder;

	[vReadOnly(false)]
	[SerializeField]
	protected bool enterLadderStarted;

	[vReadOnly(false)]
	[SerializeField]
	protected bool inEnterLadderAnimation;

	[vReadOnly(false)]
	[SerializeField]
	protected bool inExitingLadderAnimation;

	[vReadOnly(false)]
	[SerializeField]
	protected bool triggerEnterOnce;

	[vReadOnly(false)]
	[SerializeField]
	protected bool triggerExitOnce;

	protected vThirdPersonInput tpInput;

	protected override void SetUpListener()
	{
		base.actionEnter = false;
		base.actionStay = true;
		base.actionExit = true;
	}

	protected override void Start()
	{
		base.Start();
		tpInput = GetComponent<vThirdPersonInput>();
		if ((bool)tpInput)
		{
			tpInput.onUpdate -= UpdateLadderBehavior;
			tpInput.onUpdate += UpdateLadderBehavior;
			tpInput.onAnimatorMove -= UsingLadder;
			tpInput.onAnimatorMove += UsingLadder;
		}
	}

	protected virtual void UpdateLadderBehavior()
	{
		AutoEnterLadder();
		EnterLadderInput();
		ExitLadderInput();
	}

	protected virtual void EnterLadderInput()
	{
		if (!(targetLadderAction == null) && !tpInput.cc.customAction && !tpInput.cc.isJumping && tpInput.cc.isGrounded && !tpInput.cc.isRolling && enterInput.GetButtonDown() && !enterLadderStarted && !isUsingLadder && !targetLadderAction.autoAction)
		{
			TriggerEnterLadder();
		}
	}

	protected virtual void ExitLadderInput()
	{
		if (!isUsingLadder || tpInput.cc.baseLayerInfo.IsName("EnterLadderTop") || tpInput.cc.baseLayerInfo.IsName("EnterLadderBottom"))
		{
			return;
		}
		if (targetLadderAction == null)
		{
			if (!tpInput.cc.IsAnimatorTag("ClimbLadder"))
			{
				return;
			}
			if (slideDownInput.GetButtonDown() && !inExitingLadderAnimation)
			{
				tpInput.cc.animator.CrossFadeInFixedTime("Ladder_SlideDown", 0.2f);
			}
			if (exitInput.GetButtonDown())
			{
				if (debugMode)
				{
					Debug.Log("Quick Exit..." + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
				}
				tpInput.cc.animator.speed = 1f;
				tpInput.cc.animator.CrossFadeInFixedTime("QuickExitLadder", 0.1f);
				Invoke("ResetPlayerSettings", 0.5f);
			}
			return;
		}
		currentLadderAction = targetLadderAction;
		string exitAnimation = targetLadderAction.exitAnimation;
		if (exitAnimation == "ExitLadderBottom")
		{
			if ((exitInput.GetButtonDown() && !triggerExitOnce) || (speed <= -0.05f && !triggerExitOnce) || (tpInput.cc.IsAnimatorTag("LadderSlideDown") && targetLadderAction != null && !triggerExitOnce))
			{
				if (debugMode)
				{
					Debug.Log("Exit Bottom..." + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
				}
				triggerExitOnce = true;
				tpInput.cc.animator.CrossFadeInFixedTime(targetLadderAction.exitAnimation, 0.1f);
			}
		}
		else if (exitAnimation == "ExitLadderTop" && tpInput.cc.IsAnimatorTag("ClimbLadder") && speed >= 0.05f && !triggerExitOnce && !tpInput.cc.animator.IsInTransition(0))
		{
			if (debugMode)
			{
				Debug.Log("Exit Top..." + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
			}
			triggerExitOnce = true;
			tpInput.cc.animator.CrossFadeInFixedTime(targetLadderAction.exitAnimation, 0.1f);
		}
	}

	protected virtual void AutoEnterLadder()
	{
		if (!(targetLadderAction == null) && targetLadderAction.autoAction && !tpInput.cc.customAction && !isUsingLadder && !tpInput.cc.animator.IsInTransition(0) && targetLadderAction.autoAction && tpInput.cc.input != Vector3.zero && !tpInput.cc.customAction)
		{
			Vector3 vector = Camera.main.transform.TransformDirection(new Vector3(tpInput.cc.input.x, 0f, tpInput.cc.input.z));
			vector.y = 0f;
			if (Vector3.Distance(vector.normalized, targetLadderAction.transform.forward) < 0.8f)
			{
				TriggerEnterLadder();
			}
		}
	}

	protected virtual void TriggerEnterLadder()
	{
		if (debugMode)
		{
			Debug.Log("Enter Ladder");
		}
		OnExitTriggerLadder.Invoke();
		if ((bool)targetLadderAction.targetCharacterParent)
		{
			base.transform.parent = targetLadderAction.targetCharacterParent;
		}
		tpInput.cc.isCrouching = false;
		tpInput.cc.ControlCapsuleHeight();
		tpInput.UpdateCameraStates();
		tpInput.cc.UpdateAnimator();
		OnEnterLadder.Invoke();
		triggerEnterOnce = true;
		enterLadderStarted = true;
		tpInput.cc.animator.SetInteger(vAnimatorParameters.ActionState, 1);
		tpInput.SetLockAllInput(value: true);
		tpInput.cc.ResetInputAnimatorParameters();
		targetLadderAction.OnDoAction.Invoke();
		currentLadderAction = targetLadderAction;
		if (!string.IsNullOrEmpty(currentLadderAction.playAnimation))
		{
			if (debugMode)
			{
				Debug.Log("TriggerAnimation " + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
			}
			tpInput.cc.animator.CrossFadeInFixedTime(currentLadderAction.playAnimation, 0.25f);
			isUsingLadder = true;
			tpInput.cc.disableAnimations = true;
			tpInput.cc.StopCharacter();
		}
	}

	protected virtual void UsingLadder()
	{
		if (!isUsingLadder)
		{
			return;
		}
		tpInput.cc.AnimatorLayerControl();
		tpInput.cc.ActionsControl();
		tpInput.CameraInput();
		speed = verticallInput.GetAxis();
		tpInput.cc.animator.SetFloat(vAnimatorParameters.InputVertical, speed, 0.1f, Time.deltaTime);
		if (speed >= 0.05f || speed <= -0.05f)
		{
			tpInput.cc.animator.speed = Mathf.Lerp(tpInput.cc.animator.speed, currentClimbSpeed, 2f * Time.deltaTime);
		}
		else
		{
			tpInput.cc.animator.speed = Mathf.Lerp(tpInput.cc.animator.speed, 1f, 2f * Time.deltaTime);
		}
		if (fastClimbInput.GetButton() && tpInput.cc.currentStamina > 0f)
		{
			currentClimbSpeed = fastClimbSpeed;
			StaminaConsumption();
		}
		else
		{
			currentClimbSpeed = climbSpeed;
		}
		int num;
		if (!tpInput.cc.baseLayerInfo.IsName("EnterLadderTop"))
		{
			if (tpInput.cc.baseLayerInfo.IsName("EnterLadderBottom"))
			{
				num = ((!tpInput.cc.animator.IsInTransition(0)) ? 1 : 0);
				if (num != 0)
				{
					goto IL_01ab;
				}
			}
			else
			{
				num = 0;
			}
			goto IL_0334;
		}
		num = 1;
		goto IL_01ab;
		IL_01ab:
		inEnterLadderAnimation = true;
		tpInput.cc.DisableGravityAndCollision();
		if (currentLadderAction != null)
		{
			currentLadderAction.OnPlayerExit.Invoke();
		}
		if (currentLadderAction.useTriggerRotation)
		{
			if (debugMode)
			{
				Debug.Log("Rotating to target..." + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
			}
			EvaluateToRotation(currentLadderAction.enterRotationCurve, currentLadderAction.matchTarget.transform.rotation, tpInput.cc.baseLayerInfo.normalizedTime);
		}
		if (currentLadderAction.matchTarget != null)
		{
			if (base.transform.parent != currentLadderAction.targetCharacterParent)
			{
				base.transform.parent = currentLadderAction.targetCharacterParent;
			}
			if (debugMode)
			{
				Debug.Log("Match Target to Enter..." + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
			}
			EvaluateToPosition(currentLadderAction.enterPositionXZCurve, currentLadderAction.enterPositionYCurve, currentLadderAction.matchTarget.position, tpInput.cc.baseLayerInfo.normalizedTime);
		}
		goto IL_0334;
		IL_0334:
		if (num == 0 && inEnterLadderAnimation)
		{
			enterLadderStarted = false;
			inEnterLadderAnimation = false;
		}
		TriggerExitLadder();
	}

	protected virtual void TriggerExitLadder()
	{
		inExitingLadderAnimation = tpInput.cc.baseLayerInfo.IsName("ExitLadderTop") || tpInput.cc.baseLayerInfo.IsName("ExitLadderBottom") || tpInput.cc.baseLayerInfo.IsName("QuickExitLadder");
		if (!inExitingLadderAnimation)
		{
			return;
		}
		tpInput.cc.animator.speed = 1f;
		if (currentLadderAction.exitMatchTarget != null && !tpInput.cc.baseLayerInfo.IsName("QuickExitLadder"))
		{
			if (debugMode)
			{
				Debug.Log("Match Target to exit..." + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
			}
			EvaluateToPosition(currentLadderAction.exitPositionXZCurve, currentLadderAction.exitPositionYCurve, currentLadderAction.exitMatchTarget.position, tpInput.cc.baseLayerInfo.normalizedTime);
		}
		Vector3 euler = new Vector3(0f, tpInput.animator.rootRotation.eulerAngles.y, 0f);
		EvaluateToRotation(currentLadderAction.exitRotationCurve, Quaternion.Euler(euler), tpInput.cc.baseLayerInfo.normalizedTime);
		if (tpInput.cc.baseLayerInfo.normalizedTime >= 0.8f)
		{
			ResetPlayerSettings();
		}
	}

	protected virtual void EvaluateToPosition(AnimationCurve XZ, AnimationCurve Y, Vector3 targetPosition, float normalizedTime)
	{
		Vector3 rootPosition = tpInput.cc.animator.rootPosition;
		float num = XZ.Evaluate(normalizedTime);
		float num2 = Y.Evaluate(normalizedTime);
		if (num < 1f)
		{
			rootPosition.x = Mathf.Lerp(rootPosition.x, targetPosition.x, num);
			rootPosition.z = Mathf.Lerp(rootPosition.z, targetPosition.z, num);
		}
		if (num2 < 1f)
		{
			rootPosition.y = Mathf.Lerp(rootPosition.y, targetPosition.y, num2);
		}
		base.transform.position = rootPosition;
	}

	protected virtual void EvaluateToRotation(AnimationCurve curve, Quaternion targetRotation, float normalizedTime)
	{
		Quaternion quaternion = tpInput.cc.animator.rootRotation;
		float num = curve.Evaluate(normalizedTime);
		if (num < 1f)
		{
			quaternion = Quaternion.Lerp(quaternion, targetRotation, num);
		}
		base.transform.rotation = quaternion;
	}

	protected virtual void StaminaConsumption()
	{
		if (!(tpInput.cc.currentStamina <= 0f))
		{
			tpInput.cc.ReduceStamina(fastClimbStamina, accumulative: true);
			tpInput.cc.currentStaminaRecoveryDelay = 0.25f;
		}
	}

	protected virtual void AddLadderTrigger(vTriggerLadderAction _ladderAction)
	{
		if (targetLadderAction != _ladderAction)
		{
			targetLadderAction = _ladderAction;
			if (debugMode)
			{
				Debug.Log("TriggerStay " + targetLadderAction.name + "_" + targetLadderAction.transform.parent.gameObject.name);
			}
		}
		if (!actionTriggers.Contains(targetLadderAction))
		{
			actionTriggers.Add(targetLadderAction);
			targetLadderAction.OnPlayerEnter.Invoke();
		}
	}

	protected virtual void RemoveLadderTrigger(vTriggerLadderAction _ladderAction)
	{
		if (_ladderAction == targetLadderAction)
		{
			targetLadderAction = null;
		}
		if (actionTriggers.Contains(_ladderAction))
		{
			actionTriggers.Remove(_ladderAction);
			_ladderAction.OnPlayerExit.Invoke();
		}
	}

	protected virtual void CheckForTriggerAction(Collider other)
	{
		vTriggerLadderAction component = other.GetComponent<vTriggerLadderAction>();
		if (!component)
		{
			return;
		}
		float num = Vector3.Distance(base.transform.forward, component.transform.forward);
		if (isUsingLadder && component != null)
		{
			if (targetLadderAction != component)
			{
				targetLadderAction = component;
				if (!actionTriggers.Contains(targetLadderAction))
				{
					actionTriggers.Add(targetLadderAction);
				}
			}
		}
		else if ((!component.activeFromForward || num <= 0.8f) && !isUsingLadder)
		{
			AddLadderTrigger(component);
			OnEnterTriggerLadder.Invoke();
		}
		else
		{
			RemoveLadderTrigger(component);
		}
	}

	public virtual void ResetPlayerSettings()
	{
		if (debugMode)
		{
			Debug.Log("Reset Player Settings");
		}
		speed = 0f;
		targetLadderAction = null;
		isUsingLadder = false;
		OnExitLadder.Invoke();
		triggerExitOnce = false;
		triggerEnterOnce = false;
		inEnterLadderAnimation = false;
		enterLadderStarted = false;
		tpInput.cc.animator.SetInteger(vAnimatorParameters.ActionState, 0);
		tpInput.cc.EnableGravityAndCollision();
		tpInput.SetLockAllInput(value: false);
		tpInput.cc.StopCharacter();
		tpInput.cc.disableAnimations = false;
		if (base.transform.parent != null)
		{
			base.transform.parent = null;
		}
	}

	public override void OnActionStay(Collider other)
	{
		if (other.gameObject.CompareTag(actionTag) && !enterLadderStarted)
		{
			CheckForTriggerAction(other);
		}
	}

	public override void OnActionExit(Collider other)
	{
		if (!other.gameObject.CompareTag(actionTag))
		{
			return;
		}
		vTriggerLadderAction component = other.GetComponent<vTriggerLadderAction>();
		if ((bool)component)
		{
			RemoveLadderTrigger(component);
			if (debugMode)
			{
				Debug.Log("TriggerExit " + other.name + "_" + other.transform.parent.gameObject.name);
			}
			OnExitTriggerLadder.Invoke();
		}
	}
}
