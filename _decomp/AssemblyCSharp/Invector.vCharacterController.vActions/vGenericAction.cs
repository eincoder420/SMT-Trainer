using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.vActions;

[vClassHeader("GENERIC ACTION", "Use the vTriggerGenericAction to trigger a simple animation.\n<b><size=12>You can use <color=red>vGenericActionReceiver</color> component to filter events by action name</size></b>", iconName = "triggerIcon")]
public class vGenericAction : vActionListener
{
	internal class ActionStorage
	{
		internal vTriggerGenericAction action;

		internal bool isValid;

		internal ActionStorage()
		{
		}

		internal ActionStorage(vTriggerGenericAction action)
		{
			this.action = action;
			action.OnValidate.AddListener(delegate
			{
				isValid = true;
			});
			action.OnInvalidate.AddListener(delegate
			{
				isValid = false;
			});
		}

		public static implicit operator vTriggerGenericAction(ActionStorage storage)
		{
			return storage.action;
		}

		public static implicit operator ActionStorage(vTriggerGenericAction action)
		{
			return new ActionStorage(action);
		}
	}

	[vEditorToolbar("Settings", false, "", false, false)]
	[Tooltip("Tag of the object you want to access")]
	public string actionTag = "Action";

	[Tooltip("Use root motion of the animation")]
	public bool useRootMotion = true;

	[vEditorToolbar("Debug", false, "", false, false)]
	[Header("--- Debug Only ---")]
	[Tooltip("Check this to enter the debug mode")]
	public bool debugMode;

	[vReadOnly(true)]
	public vTriggerGenericAction triggerAction;

	[vReadOnly(true)]
	[SerializeField]
	protected bool _playingAnimation;

	[vReadOnly(true)]
	[SerializeField]
	protected bool actionStarted;

	[vReadOnly(true)]
	public bool isLockTriggerEvents;

	[vReadOnly(true)]
	[SerializeField]
	protected List<Collider> colliders = new List<Collider>();

	[vEditorToolbar("Events", false, "", false, false)]
	public vOnActionHandle OnEnterTriggerAction;

	public vOnActionHandle OnExitTriggerAction;

	public vOnActionHandle OnStartAction;

	public vOnActionHandle OnCancelAction;

	public vOnActionHandle OnEndAction;

	internal Camera mainCamera;

	internal vThirdPersonInput tpInput;

	protected float _currentInputDelay;

	protected Vector3 _screenCenter;

	protected float timeInTrigger;

	protected float animationBehaviourDelay;

	protected bool finishRotationMatch;

	protected bool finishPositionXZMatch;

	protected bool finishPositionYMatch;

	internal Dictionary<Collider, ActionStorage> actions;

	protected virtual Vector3 screenCenter
	{
		get
		{
			_screenCenter.x = (float)Screen.width * 0.5f;
			_screenCenter.y = (float)Screen.height * 0.5f;
			_screenCenter.z = 0f;
			return _screenCenter;
		}
	}

	protected virtual bool inActionAnimation
	{
		get
		{
			if (!string.IsNullOrEmpty(triggerAction.playAnimation))
			{
				return tpInput.cc.animatorStateInfos.stateInfos[triggerAction.animatorLayer].shortPathHash.Equals(Animator.StringToHash(triggerAction.playAnimation));
			}
			return false;
		}
	}

	public virtual bool playingAnimation
	{
		get
		{
			if (triggerAction == null || !base.doingAction)
			{
				return _playingAnimation = false;
			}
			if (!_playingAnimation && inActionAnimation)
			{
				_playingAnimation = true;
				triggerAction.OnStartAnimation.Invoke();
				DisablePlayerGravityAndCollision();
			}
			else if (_playingAnimation && !inActionAnimation)
			{
				_playingAnimation = false;
			}
			return _playingAnimation;
		}
		protected set
		{
			_playingAnimation = true;
		}
	}

	public virtual bool actionConditions
	{
		get
		{
			if (!base.doingAction && !playingAnimation && !tpInput.cc.isJumping && !tpInput.cc.customAction)
			{
				return !tpInput.cc.animator.IsInTransition(triggerAction.animatorLayer);
			}
			return false;
		}
	}

	protected override void SetUpListener()
	{
		base.actionEnter = true;
		base.actionStay = true;
		base.actionExit = true;
		actions = new Dictionary<Collider, ActionStorage>();
	}

	protected override void Start()
	{
		base.Start();
		tpInput = GetComponent<vThirdPersonInput>();
		if (tpInput != null)
		{
			tpInput.onUpdate -= CheckForTriggerAction;
			tpInput.onUpdate += CheckForTriggerAction;
			tpInput.onLateUpdate -= UpdateGenericAction;
			tpInput.onLateUpdate += UpdateGenericAction;
		}
		if (!mainCamera)
		{
			mainCamera = Camera.main;
		}
	}

	protected virtual void UpdateGenericAction()
	{
		if (!mainCamera)
		{
			mainCamera = Camera.main;
		}
		if ((bool)mainCamera)
		{
			AnimationBehaviour();
			HandleColliders();
		}
	}

	private void HandleColliders()
	{
		colliders.Clear();
		foreach (Collider key in actions.Keys)
		{
			colliders.Add(key);
		}
		if (!base.doingAction && (bool)triggerAction && !isLockTriggerEvents)
		{
			if (timeInTrigger <= 0f)
			{
				actions.Clear();
				triggerAction = null;
			}
			else
			{
				timeInTrigger -= Time.deltaTime;
			}
		}
	}

	protected virtual void CheckForTriggerAction()
	{
		if ((actions.Count == 0 && !triggerAction) || isLockTriggerEvents)
		{
			return;
		}
		vTriggerGenericAction nearAction = GetNearAction();
		if (!base.doingAction && triggerAction != nearAction)
		{
			triggerAction = nearAction;
			if ((bool)triggerAction)
			{
				triggerAction.OnValidate.Invoke(base.gameObject);
				OnEnterTriggerAction.Invoke(triggerAction);
			}
		}
		TriggerActionInput();
	}

	protected vTriggerGenericAction GetNearAction()
	{
		if (isLockTriggerEvents || base.doingAction || playingAnimation)
		{
			return null;
		}
		float num = float.PositiveInfinity;
		vTriggerGenericAction vTriggerGenericAction2 = null;
		foreach (Collider key in actions.Keys)
		{
			if ((bool)key)
			{
				try
				{
					vTriggerGenericAction vTriggerGenericAction3 = actions[key];
					Vector3 vector = (mainCamera ? mainCamera.WorldToScreenPoint(key.transform.position) : screenCenter);
					if ((bool)mainCamera)
					{
						if (vTriggerGenericAction3.enabled && vTriggerGenericAction3.gameObject.activeInHierarchy && ((!vTriggerGenericAction3.activeFromForward && (vector - screenCenter).magnitude < num) || (IsInForward(vTriggerGenericAction3.transform, vTriggerGenericAction3.forwardAngle) && (vector - screenCenter).magnitude < num)))
						{
							num = (vector - screenCenter).magnitude;
							if ((bool)vTriggerGenericAction2 && vTriggerGenericAction2 != vTriggerGenericAction3)
							{
								if (actions[vTriggerGenericAction2._collider].isValid)
								{
									vTriggerGenericAction2.OnInvalidate.Invoke(base.gameObject);
								}
								vTriggerGenericAction2 = vTriggerGenericAction3;
							}
							else if (vTriggerGenericAction2 == null)
							{
								vTriggerGenericAction2 = vTriggerGenericAction3;
							}
						}
						else
						{
							if (actions[vTriggerGenericAction3._collider].isValid)
							{
								vTriggerGenericAction3.OnInvalidate.Invoke(base.gameObject);
							}
							OnExitTriggerAction.Invoke(triggerAction);
						}
					}
					else if (!vTriggerGenericAction2)
					{
						vTriggerGenericAction2 = vTriggerGenericAction3;
					}
					else
					{
						if (actions[vTriggerGenericAction3._collider].isValid)
						{
							vTriggerGenericAction3.OnInvalidate.Invoke(base.gameObject);
						}
						OnExitTriggerAction.Invoke(triggerAction);
					}
				}
				catch
				{
					break;
				}
				continue;
			}
			actions.Remove(key);
			return null;
		}
		return vTriggerGenericAction2;
	}

	protected virtual bool IsInForward(Transform target, float angleToCompare)
	{
		return Vector3.Angle(base.transform.forward, target.forward) <= angleToCompare;
	}

	protected virtual void AnimationBehaviour()
	{
		if (animationBehaviourDelay > 0f && !playingAnimation)
		{
			animationBehaviourDelay -= Time.deltaTime;
		}
		else if (playingAnimation)
		{
			if (triggerAction.matchTarget != null)
			{
				if (debugMode)
				{
					Debug.Log("<b>GenericAction: </b><color=blue>Match Target...</color> ");
				}
				EvaluateToTargetPosition();
			}
			if (triggerAction.useTriggerRotation)
			{
				if (debugMode)
				{
					Debug.Log("<b>GenericAction: </b><color=blue>Rotate to Target...</color> ");
				}
				EvaluateToTargetRotation();
			}
			if (actionStarted && !triggerAction.endActionManualy && (triggerAction.inputType != vTriggerGenericAction.InputType.GetButtonTimer || !triggerAction.playAnimationWhileHoldingButton) && tpInput.cc.animatorStateInfos.GetCurrentNormalizedTime(triggerAction.animatorLayer) >= triggerAction.endExitTimeAnimation)
			{
				if (debugMode)
				{
					Debug.Log("<b>GenericAction: </b>Finish Animation ");
				}
				EndAction();
			}
		}
		else if (base.doingAction && actionStarted && (triggerAction == null || !triggerAction.endActionManualy) && (!(triggerAction != null) || triggerAction.inputType != vTriggerGenericAction.InputType.GetButtonTimer || !triggerAction.playAnimationWhileHoldingButton))
		{
			if (debugMode)
			{
				Debug.Log("<b>GenericAction: </b>Force ResetTriggerSettings ");
			}
			EndAction();
		}
	}

	protected virtual void EvaluateToTargetPosition()
	{
		Vector3 position = triggerAction.matchTarget.position;
		switch (triggerAction.avatarTarget)
		{
		case AvatarTarget.LeftHand:
			position = triggerAction.matchTarget.position - base.transform.rotation * base.transform.InverseTransformPoint(tpInput.animator.GetBoneTransform(HumanBodyBones.LeftHand).position);
			break;
		case AvatarTarget.RightHand:
			position = triggerAction.matchTarget.position - base.transform.rotation * base.transform.InverseTransformPoint(tpInput.animator.GetBoneTransform(HumanBodyBones.RightHand).position);
			break;
		case AvatarTarget.LeftFoot:
			position = triggerAction.matchTarget.position - base.transform.rotation * base.transform.InverseTransformPoint(tpInput.animator.GetBoneTransform(HumanBodyBones.LeftFoot).position);
			break;
		case AvatarTarget.RightFoot:
			position = triggerAction.matchTarget.position - base.transform.rotation * base.transform.InverseTransformPoint(tpInput.animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
			break;
		}
		AnimationCurve matchPositionXZCurve = triggerAction.matchPositionXZCurve;
		AnimationCurve matchPositionYCurve = triggerAction.matchPositionYCurve;
		float time = Mathf.Clamp(tpInput.cc.animatorStateInfos.GetCurrentNormalizedTime(triggerAction.animatorLayer), 0f, 1f);
		Vector3 position2 = triggerAction.matchTarget.InverseTransformPoint(position);
		if (!triggerAction.useLocalX)
		{
			position2.x = triggerAction.matchTarget.InverseTransformPoint(base.transform.position).x;
		}
		if (!triggerAction.useLocalZ)
		{
			position2.z = triggerAction.matchTarget.InverseTransformPoint(base.transform.position).z;
		}
		position = triggerAction.matchTarget.TransformPoint(position2);
		Vector3 rootPosition = tpInput.cc.animator.rootPosition;
		float num = matchPositionXZCurve.Evaluate(time);
		float num2 = matchPositionYCurve.Evaluate(time);
		if (num < 1f)
		{
			rootPosition.x = Mathf.Lerp(rootPosition.x, position.x, num);
			rootPosition.z = Mathf.Lerp(rootPosition.z, position.z, num);
			finishPositionXZMatch = true;
		}
		else if (finishPositionXZMatch)
		{
			finishPositionXZMatch = false;
			rootPosition.x = position.x;
			rootPosition.z = position.z;
		}
		if (num2 < 1f)
		{
			rootPosition.y = Mathf.Lerp(rootPosition.y, position.y, num2);
			finishPositionYMatch = true;
		}
		else if (finishPositionYMatch)
		{
			finishPositionYMatch = false;
			rootPosition.y = position.y;
		}
		base.transform.position = rootPosition;
	}

	protected virtual void EvaluateToTargetRotation()
	{
		Quaternion quaternion = Quaternion.Euler(new Vector3(base.transform.eulerAngles.x, triggerAction.transform.eulerAngles.y, base.transform.eulerAngles.z));
		Quaternion quaternion2 = tpInput.cc.animator.rootRotation;
		AnimationCurve matchRotationCurve = triggerAction.matchRotationCurve;
		float currentNormalizedTime = tpInput.cc.animatorStateInfos.GetCurrentNormalizedTime(triggerAction.animatorLayer);
		float num = matchRotationCurve.Evaluate(currentNormalizedTime);
		if (num < 1f)
		{
			quaternion2 = Quaternion.Lerp(quaternion2, quaternion, num);
			finishRotationMatch = true;
		}
		else if (finishRotationMatch)
		{
			finishRotationMatch = false;
			quaternion2 = quaternion;
		}
		base.transform.rotation = quaternion2;
	}

	protected virtual void EndAction()
	{
		OnEndAction.Invoke(triggerAction);
		vTriggerGenericAction vTriggerGenericAction2 = triggerAction;
		vTriggerGenericAction2.OnEndAnimation.Invoke();
		OnExitTriggerAction.Invoke(triggerAction);
		ResetTriggerSettings();
		if (vTriggerGenericAction2.destroyAfter)
		{
			StartCoroutine(DestroyActionDelay(vTriggerGenericAction2));
		}
		if (debugMode)
		{
			Debug.Log("<b>GenericAction: </b>End Action ");
		}
	}

	public override void OnActionEnter(Collider other)
	{
		if (isLockTriggerEvents || !(other != null) || !other.gameObject.CompareTag(actionTag) || actions.ContainsKey(other))
		{
			return;
		}
		vTriggerGenericAction component = other.GetComponent<vTriggerGenericAction>();
		if ((bool)component && component.enabled)
		{
			actions.Add(other, component);
			component.OnPlayerEnter.Invoke(base.gameObject);
			if (debugMode)
			{
				Debug.Log("<color=green>Enter in Trigger </color>" + other.gameObject, other.gameObject);
			}
		}
	}

	public override void OnActionExit(Collider other)
	{
		if (!isLockTriggerEvents && other.gameObject.CompareTag(actionTag) && actions.ContainsKey(other) && (!base.doingAction || other != triggerAction._collider))
		{
			vTriggerGenericAction vTriggerGenericAction2 = actions[other];
			actions.Remove(other);
			vTriggerGenericAction2.OnPlayerExit.Invoke(base.gameObject);
			vTriggerGenericAction2.OnInvalidate.Invoke(base.gameObject);
			OnExitTriggerAction.Invoke(vTriggerGenericAction2);
			if (debugMode)
			{
				Debug.Log("<color=red>Exit of Trigger </color> " + other.gameObject, other.gameObject);
			}
		}
	}

	public override void OnActionStay(Collider other)
	{
		if (!isLockTriggerEvents && other != null && actions.ContainsKey(other))
		{
			actions[other].action.OnPlayerStay.Invoke(base.gameObject);
			timeInTrigger = 0.5f;
			if (debugMode)
			{
				Debug.Log("<color=yellow>Stay in Trigger </color>" + other.gameObject, other.gameObject);
			}
		}
	}

	public virtual void FinishAction()
	{
		if ((bool)triggerAction && actionStarted && triggerAction.endActionManualy)
		{
			EndAction();
		}
	}

	public virtual void TriggerActionInput()
	{
		if (triggerAction == null || !triggerAction.gameObject.activeInHierarchy)
		{
			return;
		}
		if (triggerAction.inputType == vTriggerGenericAction.InputType.AutoAction && actionConditions)
		{
			TriggerActionEvents();
			TriggerAnimation();
		}
		else if (triggerAction.inputType == vTriggerGenericAction.InputType.GetButtonDown && actionConditions)
		{
			if (triggerAction.actionInput.GetButtonDown())
			{
				TriggerActionEvents();
				TriggerAnimation();
			}
		}
		else if (triggerAction.inputType == vTriggerGenericAction.InputType.GetDoubleButton && actionConditions)
		{
			if (triggerAction.actionInput.GetDoubleButtonDown(triggerAction.doubleButtomTime))
			{
				TriggerActionEvents();
				TriggerAnimation();
			}
		}
		else
		{
			if (triggerAction.inputType != vTriggerGenericAction.InputType.GetButtonTimer)
			{
				return;
			}
			if (_currentInputDelay <= 0f)
			{
				bool upAfterPressed = false;
				float currentTimer = 0f;
				if (triggerAction.playAnimationWhileHoldingButton)
				{
					TriggerActionEventsInput();
					if (triggerAction.actionInput.GetButtonTimer(ref currentTimer, ref upAfterPressed, triggerAction.buttonTimer))
					{
						if (debugMode)
						{
							Debug.Log("<b>GenericAction: </b>Finish Action Input ");
						}
						triggerAction.UpdateButtonTimer(0f);
						triggerAction.OnFinishActionInput.Invoke();
						ResetActionState();
						EndAction();
					}
					if ((bool)triggerAction && triggerAction.actionInput.inButtomTimer)
					{
						if (debugMode)
						{
							Debug.Log("<b>GenericAction: </b><color=blue>Holding Input</color>  ");
						}
						triggerAction.UpdateButtonTimer(currentTimer);
						TriggerAnimation();
					}
					if (upAfterPressed && (bool)triggerAction)
					{
						CancelButtonTimer();
					}
					return;
				}
				TriggerActionEventsInput();
				if (triggerAction.actionInput.GetButtonTimer(ref currentTimer, ref upAfterPressed, triggerAction.buttonTimer))
				{
					if (debugMode)
					{
						Debug.Log("<b>GenericAction: </b>Finish Action Input ");
					}
					triggerAction.UpdateButtonTimer(0f);
					triggerAction.OnFinishActionInput.Invoke();
					TriggerAnimation();
				}
				if ((bool)triggerAction && triggerAction.actionInput.inButtomTimer)
				{
					if (debugMode)
					{
						Debug.Log("<b>GenericAction: </b><color=blue>Holding Input</color>");
					}
					triggerAction.UpdateButtonTimer(currentTimer);
				}
				if (upAfterPressed && (bool)triggerAction)
				{
					CancelButtonTimer();
				}
			}
			else
			{
				_currentInputDelay -= Time.deltaTime;
			}
		}
	}

	private void CancelButtonTimer()
	{
		if (debugMode)
		{
			Debug.Log("<b>GenericAction: </b>Cancel Action ");
		}
		triggerAction.OnCancelActionInput.Invoke();
		_currentInputDelay = triggerAction.inputDelay;
		triggerAction.UpdateButtonTimer(0f);
		OnCancelAction.Invoke(triggerAction);
		ResetActionState();
		ResetTriggerSettings(removeTrigger: false);
	}

	private void TriggerActionEventsInput()
	{
		if ((bool)triggerAction && triggerAction.actionInput.GetButtonDown())
		{
			TriggerActionEvents();
		}
	}

	public virtual void TriggerActionEvents()
	{
		if (debugMode)
		{
			Debug.Log("<b>GenericAction: </b>TriggerAction Events ", base.gameObject);
		}
		base.doingAction = true;
		OnStartAction.Invoke(triggerAction);
		OnDoAction.Invoke(triggerAction);
		StartCoroutine(triggerAction.OnPressActionDelay(base.gameObject));
	}

	public virtual void TriggerAnimation()
	{
		if (playingAnimation || actionStarted)
		{
			return;
		}
		if (debugMode)
		{
			Debug.Log("<b>GenericAction: </b>TriggerAnimation ", base.gameObject);
		}
		if (triggerAction.animatorActionState != 0)
		{
			if (debugMode)
			{
				Debug.Log("<b>GenericAction: </b>Applied ActionState: " + triggerAction.animatorActionState + " ", base.gameObject);
			}
			tpInput.cc.SetActionState(triggerAction.animatorActionState);
		}
		if (!string.IsNullOrEmpty(triggerAction.playAnimation))
		{
			if (!actionStarted)
			{
				if (debugMode)
				{
					Debug.Log("<b>GenericAction: </b>PlayAnimation: " + triggerAction.playAnimation + " ", base.gameObject);
				}
				actionStarted = true;
				playingAnimation = true;
				tpInput.cc.animator.CrossFadeInFixedTime(triggerAction.playAnimation, triggerAction.crossFadeTransition);
				if (!string.IsNullOrEmpty(triggerAction.customCameraState))
				{
					tpInput.ChangeCameraState(triggerAction.customCameraState);
				}
			}
		}
		else
		{
			actionStarted = true;
		}
		animationBehaviourDelay = triggerAction.crossFadeTransition + 0.1f;
	}

	public virtual void ResetActionState()
	{
		if ((bool)triggerAction && triggerAction.resetAnimatorActionState)
		{
			tpInput.cc.SetActionState(0);
		}
	}

	public virtual void ResetTriggerSettings(bool removeTrigger = true)
	{
		if (debugMode)
		{
			Debug.Log("<b>GenericAction: </b>Reset Trigger Settings ");
		}
		EnablePlayerGravityAndCollision();
		ResetActionState();
		if (triggerAction != null && !string.IsNullOrEmpty(triggerAction.customCameraState))
		{
			tpInput.ResetCameraState();
		}
		if (triggerAction != null && actions.ContainsKey(triggerAction._collider) && removeTrigger)
		{
			actions.Remove(triggerAction._collider);
		}
		triggerAction = null;
		base.doingAction = false;
		actionStarted = false;
	}

	public virtual void DisablePlayerGravityAndCollision()
	{
		if ((bool)triggerAction && triggerAction.disableGravity)
		{
			if (debugMode)
			{
				Debug.Log("<b>GenericAction: </b><color=red>Disable Player's Gravity</color> ");
			}
			tpInput.cc._rigidbody.useGravity = false;
			tpInput.cc._rigidbody.isKinematic = true;
			tpInput.cc._rigidbody.velocity = Vector3.zero;
		}
		if ((bool)triggerAction && triggerAction.disableCollision)
		{
			if (debugMode)
			{
				Debug.Log("<b>GenericAction: </b><color=red>Disable Player's Collision</color> ");
			}
			tpInput.cc._capsuleCollider.isTrigger = true;
		}
	}

	public virtual void EnablePlayerGravityAndCollision()
	{
		if ((bool)triggerAction && triggerAction.disableGravity)
		{
			if (debugMode)
			{
				Debug.Log("<b>GenericAction: </b><color=red>Enable Player's Gravity</color> ");
			}
			tpInput.cc._rigidbody.useGravity = true;
			tpInput.cc._rigidbody.isKinematic = false;
		}
		if ((bool)triggerAction && triggerAction.disableCollision)
		{
			if (debugMode)
			{
				Debug.Log("<b>GenericAction: </b><color=red>Enable Player's Collision</color> ");
			}
			tpInput.cc._capsuleCollider.isTrigger = false;
		}
	}

	public virtual IEnumerator DestroyActionDelay(vTriggerGenericAction triggerAction)
	{
		yield return new WaitForSeconds(triggerAction.destroyDelay);
		if (triggerAction != null && triggerAction.gameObject != null)
		{
			OnExitTriggerAction.Invoke(triggerAction);
			Object.Destroy(triggerAction.gameObject);
		}
		if (debugMode)
		{
			Debug.Log("<b>GenericAction: </b>Destroy Trigger ");
		}
	}

	public virtual void SetLockTriggerEvents(bool value)
	{
		foreach (Collider key in actions.Keys)
		{
			if ((bool)key)
			{
				actions[key].action.OnPlayerExit.Invoke(base.gameObject);
				actions[key].action.OnInvalidate.Invoke(base.gameObject);
			}
		}
		actions.Clear();
		isLockTriggerEvents = value;
	}
}
