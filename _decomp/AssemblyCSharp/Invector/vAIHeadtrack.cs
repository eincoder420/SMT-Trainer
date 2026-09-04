using System;
using System.Collections.Generic;
using Invector.vCharacterController.AI;
using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

[vClassHeader("AI HEADTRACK", true, "icon_v2", false, "", helpBoxText = "If the bone hips don't have the same orientation of the character,\n you can add a custom hips to override the original (Transforms)", useHelpBox = true)]
public class vAIHeadtrack : vMonoBehaviour
{
	[vEditorToolbar("Settings", false, "", false, false)]
	public bool canLook = true;

	public bool _freezeLookPoint;

	[vHelpBox("Check this option to continue to look and ignore the limitAngle. ex: something is pursuing you", vHelpBoxAttribute.MessageType.None)]
	public bool keepLookingOutAngle = true;

	[Range(0f, 1f)]
	public float strafeHeadWeight = 0.8f;

	[Range(0f, 1f)]
	public float strafeBodyWeight = 0.8f;

	[Range(0f, 1f)]
	public float freeHeadWeight = 1f;

	[Range(0f, 1f)]
	public float freeBodyWeight = 0.4f;

	[vMinMax(minLimit = -180f, maxLimit = 180f)]
	public Vector2 limitAngleX = new Vector2(-90f, 90f);

	[vMinMax(minLimit = -90f, maxLimit = 90f)]
	public Vector2 limitAngleY = new Vector2(-90f, 90f);

	[Tooltip("Apply offset Y to look point")]
	public float defaultOffSetLookHeight = 1.5f;

	[SerializeField]
	[vReadOnly(true)]
	protected float currentOffSetLookHeight;

	public float smooth = 12f;

	[vHelpBox("Add a AnimatorTag here to ignore the Headtrack and play the animation instead", vHelpBoxAttribute.MessageType.None)]
	public List<string> animatorTags = new List<string> { "Attack", "LockMovement", "CustomAction", "IgnoreHeadtrack" };

	public Vector2 offsetSpine;

	public Vector2 offsetHead;

	public Transform mainLookTarget;

	public Transform eyes;

	public float timeToExitLookPoint = 1f;

	public float timeToExitLookTarget = 1f;

	[vHelpBox("Use it with the FSM Action LookAround to simulate an look around animation", vHelpBoxAttribute.MessageType.None)]
	[SerializeField]
	protected float lookAroundAngle = 60f;

	[SerializeField]
	protected AnimationCurve lookAroundCurve;

	[SerializeField]
	protected float lookAroundSpeed = 0.1f;

	[vEditorToolbar("Transforms", false, "", false, false)]
	[Tooltip("If the bone hips don't have the same orientation of the character, you can add a custom hips to override the original (Transforms)")]
	public Transform hips;

	[Header("Just for Debug")]
	public Transform head;

	public List<Transform> spine;

	private vIControlAI character;

	private Animator animator;

	private Transform temporaryLookTarget;

	private Vector3 temporaryLookPoint;

	private Vector3 targetLookPoint;

	private bool inLockPoint;

	private bool inLockTarget;

	private bool isInSmoothValues;

	private bool updateIK;

	private float targetOffsetHeight;

	private float exitLookPointTime;

	private float exitLookTargetTime;

	private float headHeight;

	private float yAngle;

	private float xAngle;

	private float _yAngle;

	private float _xAngle;

	private float yRotation;

	private float xRotation;

	private float _currentHeadWeight;

	private float _currentbodyWeight;

	private float lookAroundProgress;

	private vIAnimatorStateInfoController animatorStateInfos;

	[vEditorToolbar("Events", false, "", false, false)]
	public UnityEvent onPreUpdateSpineIK;

	[vEditorToolbar("Events", false, "", false, false)]
	public UnityEvent onPosUpdateSpineIK;

	protected Vector3 _currentLocalLookPosition;

	protected Vector3 _lastLocalLookPosition;

	public Vector3 currentLookPoint { get; set; }

	public Vector3 currentLookDirection { get; set; }

	public virtual bool freezeLookPoint
	{
		get
		{
			return _freezeLookPoint;
		}
		set
		{
			_freezeLookPoint = value;
		}
	}

	public virtual Vector3 currentLookPosition
	{
		get
		{
			if (!freezeLookPoint)
			{
				return base.transform.TransformPoint(_currentLocalLookPosition);
			}
			return base.transform.TransformPoint(_lastLocalLookPosition);
		}
		protected set
		{
			_currentLocalLookPosition = base.transform.InverseTransformPoint(value);
			if (!freezeLookPoint)
			{
				_lastLocalLookPosition = _currentLocalLookPosition;
			}
		}
	}

	protected virtual Vector3 headPoint => base.transform.position + base.transform.up * headHeight;

	protected Vector3 defaultLookPoint => headPoint + base.transform.forward * 100f;

	protected virtual float offsetHeightResult => currentOffSetLookHeight = Mathf.Lerp(currentOffSetLookHeight, targetOffsetHeight, smooth * 2f);

	public virtual bool isLookingForSomething
	{
		get
		{
			if (!(defaultLookPoint != targetLookPoint) || !canLook)
			{
				return isInSmoothValues;
			}
			return true;
		}
	}

	protected virtual void Start()
	{
		character = GetComponent<vIControlAI>();
		animator = GetComponent<Animator>();
		animatorStateInfos = GetComponent<vIAnimatorStateInfoController>();
		if (animator.isHuman)
		{
			head = animator.GetBoneTransform(HumanBodyBones.Head);
			Transform boneTransform = animator.GetBoneTransform(HumanBodyBones.Spine);
			Transform boneTransform2 = animator.GetBoneTransform(HumanBodyBones.Chest);
			spine = new List<Transform>();
			if ((bool)boneTransform)
			{
				spine.Add(boneTransform);
			}
			if ((bool)boneTransform2)
			{
				spine.Add(boneTransform2);
			}
			Transform boneTransform3 = animator.GetBoneTransform(HumanBodyBones.Neck);
			if (!hips)
			{
				hips = animator.GetBoneTransform(HumanBodyBones.Hips);
			}
			if ((bool)boneTransform3 && (bool)boneTransform2 && (bool)boneTransform3.parent && boneTransform3.parent != boneTransform2)
			{
				spine.Add(boneTransform3.parent);
			}
		}
		if ((bool)head)
		{
			headHeight = Vector3.Distance(base.transform.position, head.position);
		}
		ResetOffseLookHeight();
		currentLookPosition = GetLookPoint();
		_lastLocalLookPosition = _currentLocalLookPosition;
		lookAroundProgress = 0.5f;
	}

	protected virtual void FixedUpdate()
	{
		updateIK = true;
	}

	protected virtual void LateUpdate()
	{
		if (!(animator == null) && (character == null || (!(character.currentHealth <= 0f) && !character.isDead && !character.ragdolled)) && animator.enabled && (updateIK || animator.updateMode != AnimatorUpdateMode.AnimatePhysics))
		{
			updateIK = false;
			if (onPreUpdateSpineIK != null)
			{
				onPreUpdateSpineIK.Invoke();
			}
			if (!freezeLookPoint)
			{
				currentLookPoint = GetLookPoint();
			}
			LookAtIK(currentLookPoint, _currentHeadWeight, _currentbodyWeight);
			if (onPosUpdateSpineIK != null && !IgnoreHeadTrackFromAnimator())
			{
				onPosUpdateSpineIK.Invoke();
			}
		}
	}

	protected virtual void LookAtIK(Vector3 point, float headWeight, float spineWeight)
	{
		Vector3 vector = Quaternion.LookRotation(point).eulerAngles - base.transform.rotation.eulerAngles;
		float b = NormalizeAngle(vector.y);
		float b2 = NormalizeAngle(vector.x);
		xAngle = Mathf.Clamp(Mathf.Lerp(xAngle, b2, smooth * Time.deltaTime), limitAngleX.x, limitAngleX.y);
		yAngle = Mathf.Clamp(Mathf.Lerp(yAngle, b, smooth * Time.deltaTime), limitAngleY.x, limitAngleY.y);
		foreach (Transform item in spine)
		{
			float num = NormalizeAngle(yAngle + Quaternion.Euler(offsetSpine).eulerAngles.y);
			Quaternion quaternion = Quaternion.AngleAxis(NormalizeAngle(xAngle + Quaternion.Euler(offsetSpine).eulerAngles.x) * spineWeight / (float)spine.Count, item.InverseTransformDirection(base.transform.right));
			Quaternion quaternion2 = Quaternion.AngleAxis(num * spineWeight / (float)spine.Count, item.InverseTransformDirection(base.transform.up));
			item.rotation *= quaternion * quaternion2;
		}
		Vector3 vector2 = Quaternion.Euler(offsetHead).eulerAngles.NormalizeAngle();
		_yAngle = Mathf.Lerp(_yAngle, yAngle - yAngle * spineWeight + vector2.y, smooth * Time.deltaTime);
		_xAngle = Mathf.Lerp(_xAngle, xAngle - xAngle * spineWeight + vector2.x, smooth * Time.deltaTime);
		Quaternion quaternion3 = Quaternion.AngleAxis(_xAngle * headWeight, head.InverseTransformDirection(base.transform.right));
		Quaternion quaternion4 = Quaternion.AngleAxis(_yAngle * headWeight, head.InverseTransformDirection(base.transform.up));
		head.rotation *= quaternion3 * quaternion4;
	}

	protected virtual void SmoothValues(float _headWeight = 0f, float _bodyWeight = 0f, float _x = 0f, float _y = 0f)
	{
		_currentHeadWeight = Mathf.Lerp(_currentHeadWeight, _headWeight, smooth * Time.deltaTime);
		_currentbodyWeight = Mathf.Lerp(_currentbodyWeight, _bodyWeight, smooth * Time.deltaTime);
		yRotation = Mathf.Lerp(yRotation, _y, smooth * Time.deltaTime);
		xRotation = Mathf.Lerp(xRotation, _x, smooth * Time.deltaTime);
		yRotation = Mathf.Clamp(yRotation, limitAngleY.x, limitAngleY.y);
		xRotation = Mathf.Clamp(xRotation, limitAngleX.x, limitAngleX.y);
		bool flag = Mathf.Abs(yRotation - Mathf.Clamp(_y, limitAngleY.x, limitAngleY.y)) < 0.01f;
		bool flag2 = Mathf.Abs(yRotation - Mathf.Clamp(_x, limitAngleX.x, limitAngleX.y)) < 0.01f;
		isInSmoothValues = !(flag && flag2);
	}

	protected virtual Vector3 GetLookPoint()
	{
		if (!IgnoreHeadTrackFromAnimator() && canLook)
		{
			Vector3 vector = (mainLookTarget ? (mainLookTarget.position + Vector3.up * offsetHeightResult) : defaultLookPoint);
			targetLookPoint = vector;
			if (exitLookTargetTime > 0f || inLockTarget)
			{
				if ((bool)temporaryLookTarget)
				{
					targetLookPoint = temporaryLookTarget.position + Vector3.up * offsetHeightResult;
				}
				else
				{
					exitLookTargetTime = 0f;
				}
				if (!inLockTarget)
				{
					exitLookTargetTime -= Time.deltaTime;
				}
			}
			if (exitLookPointTime > 0f || inLockPoint)
			{
				targetLookPoint = temporaryLookPoint + Vector3.up * offsetHeightResult;
				if (!inLockPoint)
				{
					exitLookPointTime -= Time.deltaTime;
				}
			}
			Vector3 vector2 = defaultLookPoint - headPoint;
			vector2 = targetLookPoint - headPoint;
			Vector2 targetAngle = GetTargetAngle(vector2);
			if (!keepLookingOutAngle)
			{
				if (LookDirectionIsOnRange(vector2))
				{
					if (character != null && character.isStrafing)
					{
						SmoothValues(strafeHeadWeight, strafeBodyWeight, targetAngle.x, targetAngle.y);
					}
					else
					{
						SmoothValues(freeHeadWeight, freeBodyWeight, targetAngle.x, targetAngle.y);
					}
				}
				else
				{
					SmoothValues();
				}
			}
			else if (character != null && character.isStrafing)
			{
				SmoothValues(strafeHeadWeight, strafeBodyWeight, targetAngle.x, targetAngle.y);
			}
			else
			{
				SmoothValues(freeHeadWeight, freeBodyWeight, targetAngle.x, targetAngle.y);
			}
		}
		else
		{
			SmoothValues();
		}
		Quaternion quaternion = Quaternion.AngleAxis(yRotation, base.transform.up);
		Quaternion quaternion2 = Quaternion.AngleAxis(xRotation, base.transform.right);
		Vector3 vector3 = quaternion * quaternion2 * base.transform.forward;
		currentLookPoint = headPoint + vector3;
		return vector3;
	}

	protected virtual Vector2 GetTargetAngle(Vector3 direction)
	{
		Quaternion quaternion = Quaternion.Euler(Quaternion.LookRotation(direction, base.transform.up).eulerAngles - base.transform.eulerAngles);
		float x = (float)Math.Round(NormalizeAngle(quaternion.eulerAngles.x), 2);
		float y = (float)Math.Round(NormalizeAngle(quaternion.eulerAngles.y), 2);
		return new Vector2(x, y);
	}

	protected virtual bool IgnoreHeadTrackFromAnimator()
	{
		if (animatorTags.Exists((string tag) => IsAnimatorTag(tag)))
		{
			return true;
		}
		return false;
	}

	public virtual bool IsAnimatorTag(string tag)
	{
		if (animator == null)
		{
			return false;
		}
		if (animatorStateInfos.isValid() && animatorStateInfos.animatorStateInfos.HasTag(tag))
		{
			return true;
		}
		return false;
	}

	protected virtual float NormalizeAngle(float angle)
	{
		if (angle < -180f)
		{
			return angle + 360f;
		}
		if (angle > 180f)
		{
			return angle - 360f;
		}
		return angle;
	}

	protected virtual bool LookDirectionIsOnRange(Vector3 direction)
	{
		Vector2 targetAngle = GetTargetAngle(direction);
		if (targetAngle.x >= limitAngleX.x && targetAngle.x <= limitAngleX.y && targetAngle.y >= limitAngleY.x)
		{
			return targetAngle.y <= limitAngleY.y;
		}
		return false;
	}

	public virtual void SetMainLookTarget(Transform target)
	{
		mainLookTarget = target;
	}

	public virtual void RemoveMainLookTarget()
	{
		mainLookTarget = null;
	}

	public virtual void LookAround()
	{
		lookAroundProgress += Time.deltaTime * lookAroundSpeed;
		float time = Mathf.PingPong(lookAroundProgress, 1f);
		Vector3 vector = Quaternion.AngleAxis(Mathf.Lerp(0f - lookAroundAngle, lookAroundAngle, lookAroundCurve.Evaluate(time)), base.transform.up) * base.transform.forward;
		Vector3 point = (eyes ? eyes.position : (base.transform.position + Vector3.up * headHeight)) + vector * 100f;
		LookAtPoint(point, 0.1f);
	}

	public virtual void LookAtPoint(Vector3 point, float offsetLookHeight = -1f)
	{
		if (!inLockPoint)
		{
			if (offsetLookHeight != -1f)
			{
				SetOffsetLookHeight(offsetLookHeight);
			}
			else
			{
				ResetOffseLookHeight();
			}
			temporaryLookPoint = point;
			exitLookPointTime = timeToExitLookPoint;
		}
	}

	public virtual void LookAtPoint(Vector3 point, float timeToExitLookPoint, float offsetLookHeight = -1f)
	{
		if (!inLockPoint)
		{
			if (offsetLookHeight != -1f)
			{
				SetOffsetLookHeight(offsetLookHeight);
			}
			else
			{
				ResetOffseLookHeight();
			}
			temporaryLookPoint = point;
			exitLookPointTime = timeToExitLookPoint;
		}
	}

	public virtual void LookAtTarget(Transform target, float offsetLookHeight = -1f)
	{
		if (!inLockTarget)
		{
			if (offsetLookHeight != -1f)
			{
				SetOffsetLookHeight(offsetLookHeight);
			}
			else
			{
				ResetOffseLookHeight();
			}
			temporaryLookTarget = target;
			exitLookTargetTime = timeToExitLookPoint;
		}
	}

	public virtual void LookAtTarget(Transform target, float timeToExitLookTarget, float offsetLookHeight = -1f)
	{
		if (!inLockTarget)
		{
			if (offsetLookHeight != -1f)
			{
				SetOffsetLookHeight(offsetLookHeight);
			}
			else
			{
				ResetOffseLookHeight();
			}
			temporaryLookTarget = target;
			exitLookTargetTime = timeToExitLookTarget;
		}
	}

	public virtual void LockLookAtPoint()
	{
		inLockPoint = true;
		inLockTarget = false;
	}

	public virtual void UnlockLookAtPoint()
	{
		inLockPoint = false;
	}

	public virtual void LockLookAtTarget()
	{
		inLockTarget = true;
		inLockPoint = false;
	}

	public virtual void UnlockLookAtTarget()
	{
		inLockTarget = false;
	}

	public virtual void ResetLookPoint()
	{
		exitLookPointTime = 0f;
		inLockPoint = false;
	}

	public virtual void ResetLookTarget()
	{
		exitLookTargetTime = 0f;
		temporaryLookTarget = null;
		inLockTarget = false;
	}

	public virtual void ResetLook()
	{
		ResetLookPoint();
		ResetLookTarget();
	}

	public virtual void SetOffsetLookHeight(float value)
	{
		targetOffsetHeight = value;
	}

	public virtual void ResetOffseLookHeight()
	{
		if (targetOffsetHeight != defaultOffSetLookHeight)
		{
			targetOffsetHeight = defaultOffSetLookHeight;
		}
	}
}
