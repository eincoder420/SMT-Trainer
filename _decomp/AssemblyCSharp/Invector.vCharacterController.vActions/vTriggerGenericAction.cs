using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Invector.vCharacterController.vActions;

[vClassHeader("Trigger Generic Action", false, "icon_v2", false, "", iconName = "triggerIcon")]
public class vTriggerGenericAction : vMonoBehaviour
{
	public enum InputType
	{
		GetButtonDown,
		GetDoubleButton,
		GetButtonTimer,
		AutoAction
	}

	[Serializable]
	public class OnUpdateValue : UnityEvent<float>
	{
	}

	[vEditorToolbar("Input", false, "", false, false, order = 1)]
	public InputType inputType;

	[Tooltip("Input to make the action")]
	public GenericInput actionInput = new GenericInput("E", "A", "A");

	[vHelpBox("Time you have to hold the button *Only for GetButtonTimer*", vHelpBoxAttribute.MessageType.None)]
	public float buttonTimer = 3f;

	[vHelpBox("Add delay to start the input count *Only for GetButtonTimer*", vHelpBoxAttribute.MessageType.None)]
	public float inputDelay = 0.1f;

	[vHelpBox("*Only for GetButtonTimer* \n\n<b>TRUE: </b> Play the animation while you're holding the button \n<b>FALSE: </b>Play the animation after you finish holding the button", vHelpBoxAttribute.MessageType.None)]
	public bool playAnimationWhileHoldingButton = true;

	[vHelpBox("Time to press the button twice *Only for GetDoubleButton*", vHelpBoxAttribute.MessageType.None)]
	public float doubleButtomTime = 0.25f;

	[vEditorToolbar("Trigger", false, "", false, false, order = 2)]
	public string actionName = "Action";

	public string actionTag = "Action";

	[vHelpBox("Disable this trigger OnStart", vHelpBoxAttribute.MessageType.None)]
	public bool disableOnStart;

	[vHelpBox("Disable the Player's Capsule Collider Collision, useful for animations with closer interactions", vHelpBoxAttribute.MessageType.None)]
	public bool disableCollision;

	[vHelpBox("Disable the Player's Rigidbody Gravity, useful for on air animations", vHelpBoxAttribute.MessageType.None)]
	public bool disableGravity;

	[vHelpBox("It will only use the trigger if the forward of the character is close to the forward of this transform", vHelpBoxAttribute.MessageType.None)]
	public bool activeFromForward;

	[vHelpBox("Max angle between character forward and trigger forward to active trigger", vHelpBoxAttribute.MessageType.None)]
	[Range(5f, 180f)]
	public float forwardAngle = 55f;

	[vHelpBox("Rotate Character to the Forward Rotation of this Trigger", vHelpBoxAttribute.MessageType.None)]
	public bool useTriggerRotation;

	[vHelpBox("Destroy this Trigger after pressing the Input or AutoAction or finishing the Action", vHelpBoxAttribute.MessageType.None)]
	public bool destroyAfter;

	[vHideInInspector("destroyAfter", false)]
	public float destroyDelay;

	[vHelpBox("Change your CameraState to a Custom State while playing the animation", vHelpBoxAttribute.MessageType.None)]
	public string customCameraState;

	[vEditorToolbar("Animation", false, "", false, false, order = 2)]
	[vHelpBox("Trigger a Animation - Use the exactly same name of the AnimationState you want to trigger, don't forget to add a vAnimatorTag to your State", vHelpBoxAttribute.MessageType.None)]
	public string playAnimation;

	public float crossFadeTransition = 0.25f;

	public int animatorLayer;

	[vHelpBox("Check the Exit Time of your animation (if it doesn't loop) and insert here. \n\nFor example if your Exit Time is 0.82 you need to insert 0.82\n\nAlways check with the Debug of the GenericAction if your animation is finishing correctly, otherwise the controller won't reset to the default physics and collision.", vHelpBoxAttribute.MessageType.Warning)]
	[Tooltip("You can use this to make a persistent action, and finish the action calling FinishAction method of the vGenericAction  component in your character")]
	public bool endActionManualy;

	[vHideInInspector("endActionManualy", false, invertValue = true)]
	public float endExitTimeAnimation = 0.8f;

	[vHelpBox("Use a ActionState value to apply special conditions for your AnimatorController transitions", vHelpBoxAttribute.MessageType.None)]
	public int animatorActionState;

	[vHelpBox("Reset the ActionState parameter to 0 after playing the animation", vHelpBoxAttribute.MessageType.None)]
	public bool resetAnimatorActionState = true;

	[vHelpBox("Use a empty transform as reference for the MatchTarget", vHelpBoxAttribute.MessageType.None)]
	public Transform matchTarget;

	[vHelpBox("Select the bone you want to use as reference to the Match Target", vHelpBoxAttribute.MessageType.None)]
	public AvatarTarget avatarTarget;

	[Header("Curve Match target system")]
	public bool useLocalX;

	public bool useLocalZ = true;

	public AnimationCurve matchPositionXZCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 1f));

	public AnimationCurve matchPositionYCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 1f));

	public AnimationCurve matchRotationCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 1f));

	[vEditorToolbar("Events", false, "", false, false, order = 3)]
	[Tooltip("Delay to run the OnDoAction Event")]
	[FormerlySerializedAs("onDoActionDelay")]
	public float onPressActionDelay;

	[Header("--- INPUT EVENTS ---")]
	[FormerlySerializedAs("OnDoAction")]
	public UnityEvent OnPressActionInput;

	public OnDoActionWithTarget onPressActionInputWithTarget;

	[Header("--- ONLY FOR GET BUTTON TIMER ---")]
	public UnityEvent OnCancelActionInput;

	public UnityEvent OnFinishActionInput;

	public OnUpdateValue OnUpdateButtonTimer;

	[Header("--- ANIMATION EVENTS ---")]
	public UnityEvent OnStartAnimation;

	public UnityEvent OnEndAnimation;

	[Header("--- PLAYER AND TRIGGER DETECTION ---")]
	public OnDoActionWithTarget OnPlayerEnter;

	public OnDoActionWithTarget OnPlayerStay;

	public OnDoActionWithTarget OnPlayerExit;

	[Header("--- ACTION VALIDATION  ---")]
	public OnDoActionWithTarget OnValidate;

	public OnDoActionWithTarget OnInvalidate;

	private float currentButtonTimer;

	internal Collider _collider;

	protected virtual void Start()
	{
		base.gameObject.tag = actionTag;
		base.gameObject.layer = LayerMask.NameToLayer("Triggers");
		_collider = GetComponent<Collider>();
		_collider.isTrigger = true;
		if (disableOnStart)
		{
			base.enabled = false;
		}
	}

	public virtual IEnumerator OnPressActionDelay(GameObject obj)
	{
		yield return new WaitForSeconds(onPressActionDelay);
		OnPressActionInput.Invoke();
		if ((bool)obj)
		{
			onPressActionInputWithTarget.Invoke(obj);
		}
	}

	public void UpdateButtonTimer(float value)
	{
		if (value != currentButtonTimer)
		{
			currentButtonTimer = value;
			OnUpdateButtonTimer.Invoke(value);
		}
	}
}
