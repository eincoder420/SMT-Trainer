using System.Collections.Generic;
using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController;

[vClassHeader("HEAD TRACK", true, "icon_v2", false, "", iconName = "headTrackIcon")]
public class vHeadTrack : vMonoBehaviour
{
	[vEditorToolbar("Settings", false, "", false, false)]
	[vHelpBox("If your character is not looking up/down, try changing the axis", vHelpBoxAttribute.MessageType.Info)]
	public Vector3 upDownAxis = Vector3.right;

	[Header("Head & Body Weight")]
	public float strafeHeadWeight = 0.6f;

	public float strafeBodyWeight = 0.6f;

	public float aimingHeadWeight = 0.8f;

	public float aimingBodyWeight = 0.8f;

	public float freeHeadWeight = 0.6f;

	public float freeBodyWeight = 0.6f;

	[SerializeField]
	protected float smooth = 10f;

	[Header("Default Offsets ")]
	[SerializeField]
	protected Vector2 defaultOffsetSpine;

	[SerializeField]
	protected Vector2 defaultOffsetHead;

	[vReadOnly(true)]
	public Vector2 offsetSpine;

	[vReadOnly(true)]
	public Vector2 offsetHead;

	[Header("Tracking")]
	[Tooltip("Follow the Camera Forward")]
	public bool followCamera = true;

	public bool _freezeLookPoint;

	[vHideInInspector("followCamera", false)]
	[Tooltip("Force to follow camera")]
	public bool alwaysFollowCamera;

	[Tooltip("Ignore the Limits and continue to follow the camera")]
	public bool cancelTrackOutOfAngle = true;

	[Tooltip("Considerer the head animation forward while tracking, try it to see different results")]
	public bool considerHeadAnimationForward;

	[Header("Limits")]
	[vMinMax(minLimit = -180f, maxLimit = 180f)]
	public Vector2 horizontalAngleLimit = new Vector2(-100f, 100f);

	[vMinMax(minLimit = -90f, maxLimit = 90f)]
	public Vector2 verticalAngleLimit = new Vector2(-80f, 80f);

	[vHelpBox("Animations with vAnimatorTag Behavior will ignore the HeadTrack while is being played", vHelpBoxAttribute.MessageType.None)]
	[Header("Ignore AnimatorTags")]
	public List<string> animatorIgnoreTags = new List<string> { "Attack", "LockMovement", "CustomAction", "IsEquipping", "IgnoreHeadtrack" };

	[vEditorToolbar("Bones", false, "", false, false)]
	[vHelpBox("Auto Find Bones using Humanoid", vHelpBoxAttribute.MessageType.None)]
	public bool autoFindBones = true;

	public Transform head;

	public List<Transform> spine = new List<Transform>();

	[vEditorToolbar("Detection", false, "", false, false)]
	public float updateTargetInteration = 1f;

	public float distanceToDetect = 10f;

	public LayerMask obstacleLayer = 1;

	[vHelpBox("Gameobjects Tags to detect", vHelpBoxAttribute.MessageType.None)]
	public List<string> tagsToDetect = new List<string> { "LookAt" };

	internal UnityEvent onInitUpdate = new UnityEvent();

	internal UnityEvent onFinishUpdate = new UnityEvent();

	internal Camera cameraMain;

	internal vLookTarget currentLookTarget;

	internal vLookTarget lastLookTarget;

	internal Quaternion currentLookRotation;

	internal List<vLookTarget> targetsInArea = new List<vLookTarget>();

	internal bool ignoreSmooth;

	private float yRotation;

	private float xRotation;

	private float _currentHeadWeight;

	private float _currentbodyWeight;

	private Animator animator;

	private vIAnimatorStateInfoController animatorStateInfo;

	private float headHeight;

	private Transform simpleTarget;

	private Vector3 temporaryLookPoint;

	private float temporaryLookTime;

	private vHeadTrackSensor sensor;

	private float interation;

	private vICharacter vchar;

	private float yAngle;

	private float xAngle;

	private float _yAngle;

	private float _xAngle;

	private Transform forwardReference;

	protected Vector3 _currentLocalLookPosition;

	protected Vector3 _lastLocalLookPosition;

	public float Smooth
	{
		get
		{
			if (!ignoreSmooth)
			{
				return smooth * Time.deltaTime;
			}
			return 1f;
		}
	}

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

	private Vector3 headPoint => base.transform.position + base.transform.up * headHeight;

	private bool lookConditions
	{
		get
		{
			if (!cameraMain)
			{
				cameraMain = Camera.main;
			}
			if ((!(head != null) || !followCamera || !(cameraMain != null)) && (followCamera || (!currentLookTarget && !simpleTarget)))
			{
				return temporaryLookTime > 0f;
			}
			return true;
		}
	}

	private void Start()
	{
		if (!sensor)
		{
			GameObject gameObject = new GameObject("HeadTrackSensor");
			sensor = gameObject.AddComponent<vHeadTrackSensor>();
		}
		vThirdPersonInput component = GetComponent<vThirdPersonInput>();
		if ((bool)component)
		{
			component.onLateUpdate -= UpdateHeadTrack;
			component.onLateUpdate += UpdateHeadTrack;
		}
		vchar = GetComponent<vICharacter>();
		sensor.headTrack = this;
		cameraMain = Camera.main;
		int layer = LayerMask.NameToLayer("HeadTrack");
		sensor.transform.parent = base.transform;
		sensor.gameObject.layer = layer;
		sensor.gameObject.tag = base.transform.tag;
		animatorStateInfo = GetComponent<vIAnimatorStateInfoController>();
		Init();
	}

	public void Init()
	{
		currentLookPosition = GetLookPoint();
		_lastLocalLookPosition = _currentLocalLookPosition;
		if (animator == null)
		{
			animator = GetComponentInChildren<Animator>();
		}
		if (autoFindBones)
		{
			spine.Clear();
			head = animator.GetBoneTransform(HumanBodyBones.Head);
			if ((bool)head)
			{
				if (!forwardReference)
				{
					forwardReference = new GameObject("FWRF").transform;
				}
				forwardReference.SetParent(head);
				forwardReference.transform.localPosition = Vector3.zero;
				forwardReference.transform.rotation = base.transform.rotation;
				Transform boneTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
				if ((bool)boneTransform)
				{
					Transform parent = head;
					for (int i = 0; i < 4; i++)
					{
						if (!parent.parent)
						{
							break;
						}
						if (!(parent.parent.gameObject != boneTransform.gameObject))
						{
							break;
						}
						spine.Add(parent.parent);
						parent = parent.parent;
					}
				}
			}
		}
		if ((bool)head)
		{
			headHeight = Vector3.Distance(base.transform.position, head.position);
			sensor.transform.position = head.transform.position;
		}
		else
		{
			headHeight = 1f;
			sensor.transform.position = base.transform.position;
		}
		if (spine.Count == 0)
		{
			Debug.Log("Headtrack Spines missing");
		}
		spine.Reverse();
	}

	public virtual void UpdateHeadTrack()
	{
		if (!(animator == null) && animator.enabled && vchar != null && vchar.currentHealth > 0f && animator != null && !vchar.ragdolled)
		{
			onInitUpdate.Invoke();
			if (!freezeLookPoint)
			{
				currentLookPosition = GetLookPoint();
			}
			SetLookAtPosition(currentLookPosition, _currentHeadWeight, _currentbodyWeight);
			onFinishUpdate.Invoke();
		}
	}

	public virtual void SetLookAtPosition(Vector3 point, float headWeight, float spineWeight)
	{
		Quaternion quaternion = (currentLookRotation = Quaternion.LookRotation(point - headPoint));
		Vector3 vector = quaternion.eulerAngles - base.transform.rotation.eulerAngles;
		float num = NormalizeAngle(vector.y);
		float num2 = NormalizeAngle(vector.x);
		Vector3 eulerAngle = (considerHeadAnimationForward ? (forwardReference.eulerAngles - base.transform.eulerAngles) : Vector3.zero);
		xAngle = Mathf.Clamp(Mathf.Lerp(xAngle, num2 - eulerAngle.NormalizeAngle().x + Quaternion.Euler(offsetSpine + defaultOffsetSpine).eulerAngles.NormalizeAngle().x, Smooth), verticalAngleLimit.x, verticalAngleLimit.y);
		yAngle = Mathf.Clamp(Mathf.Lerp(yAngle, num - eulerAngle.NormalizeAngle().y + Quaternion.Euler(offsetSpine + defaultOffsetSpine).eulerAngles.NormalizeAngle().y, Smooth), horizontalAngleLimit.x, horizontalAngleLimit.y);
		float num3 = NormalizeAngle(xAngle);
		float num4 = NormalizeAngle(yAngle);
		foreach (Transform item in spine)
		{
			Quaternion quaternion2 = Quaternion.AngleAxis(num4 * spineWeight / (float)spine.Count, item.InverseTransformDirection(base.transform.up));
			item.rotation *= quaternion2;
			Quaternion quaternion3 = Quaternion.AngleAxis(num3 * spineWeight / (float)spine.Count, item.InverseTransformDirection(base.transform.TransformDirection(upDownAxis)));
			item.rotation *= quaternion3;
		}
		if ((bool)head)
		{
			float num5 = NormalizeAngle(xAngle - num3 * spineWeight + Quaternion.Euler(offsetHead + defaultOffsetHead).eulerAngles.NormalizeAngle().x);
			Quaternion quaternion4 = Quaternion.AngleAxis(NormalizeAngle(yAngle - num4 * spineWeight + Quaternion.Euler(offsetHead + defaultOffsetHead).eulerAngles.NormalizeAngle().y) * headWeight, head.InverseTransformDirection(base.transform.up));
			head.rotation *= quaternion4;
			Quaternion quaternion5 = Quaternion.AngleAxis(num5 * headWeight, head.InverseTransformDirection(base.transform.TransformDirection(upDownAxis)));
			head.rotation *= quaternion5;
		}
	}

	private Vector3 GetLookPoint()
	{
		if (animator == null)
		{
			return Vector3.zero;
		}
		int num = 100;
		if (lookConditions && !IgnoreHeadTrack())
		{
			Vector3 forward = base.transform.forward;
			if (temporaryLookTime <= 0f)
			{
				Vector3 vector = headPoint + base.transform.forward * num;
				if (followCamera)
				{
					vector = cameraMain.transform.position + cameraMain.transform.forward * num;
				}
				forward = vector - headPoint;
				if ((followCamera && !alwaysFollowCamera) || !followCamera)
				{
					if (simpleTarget != null)
					{
						forward = simpleTarget.position - headPoint;
						if ((bool)currentLookTarget && currentLookTarget == lastLookTarget)
						{
							currentLookTarget.ExitLook(this);
							lastLookTarget = null;
						}
					}
					else if (currentLookTarget != null && (currentLookTarget.ignoreHeadTrackAngle || TargetIsOnRange(currentLookTarget.lookPoint - headPoint)) && currentLookTarget.IsVisible(headPoint, obstacleLayer))
					{
						forward = currentLookTarget.lookPoint - headPoint;
						if (currentLookTarget != lastLookTarget)
						{
							currentLookTarget.EnterLook(this);
							lastLookTarget = currentLookTarget;
						}
					}
					else if ((bool)currentLookTarget && currentLookTarget == lastLookTarget)
					{
						currentLookTarget.ExitLook(this);
						lastLookTarget = null;
					}
				}
			}
			else
			{
				forward = temporaryLookPoint - headPoint;
				temporaryLookTime -= Time.deltaTime;
				if ((bool)currentLookTarget && currentLookTarget == lastLookTarget)
				{
					currentLookTarget.ExitLook(this);
					lastLookTarget = null;
				}
			}
			Vector2 targetAngle = GetTargetAngle(forward);
			if (cancelTrackOutOfAngle && (lastLookTarget == null || !lastLookTarget.ignoreHeadTrackAngle))
			{
				if (TargetIsOnRange(forward))
				{
					if (animator.GetBool("IsStrafing") && !IsAnimatorTag("Upperbody Pose"))
					{
						SmoothValues(strafeHeadWeight, strafeBodyWeight, targetAngle.x, targetAngle.y);
					}
					else if (animator.GetBool("IsStrafing") && IsAnimatorTag("Upperbody Pose"))
					{
						SmoothValues(aimingHeadWeight, aimingBodyWeight, targetAngle.x, targetAngle.y);
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
			else if (animator.GetBool("IsStrafing") && !IsAnimatorTag("Upperbody Pose"))
			{
				SmoothValues(strafeHeadWeight, strafeBodyWeight, targetAngle.x, targetAngle.y);
			}
			else if (animator.GetBool("IsStrafing") && IsAnimatorTag("Upperbody Pose"))
			{
				SmoothValues(aimingHeadWeight, aimingBodyWeight, targetAngle.x, targetAngle.y);
			}
			else
			{
				SmoothValues(freeHeadWeight, freeBodyWeight, targetAngle.x, targetAngle.y);
			}
			if (targetsInArea.Count > 1)
			{
				SortTargets();
			}
		}
		else
		{
			SmoothValues();
			if (targetsInArea.Count > 1)
			{
				SortTargets();
			}
		}
		Quaternion quaternion = Quaternion.AngleAxis(yRotation, base.transform.up);
		Quaternion quaternion2 = Quaternion.AngleAxis(xRotation, base.transform.right);
		Vector3 vector2 = quaternion * quaternion2 * base.transform.forward;
		return headPoint + vector2 * num;
	}

	private Vector2 GetTargetAngle(Vector3 direction)
	{
		Vector3 eulerAngle = Quaternion.LookRotation(direction, base.transform.up).eulerAngles - base.transform.eulerAngles;
		return new Vector2(eulerAngle.NormalizeAngle().x, eulerAngle.NormalizeAngle().y);
	}

	private bool TargetIsOnRange(Vector3 direction)
	{
		Vector2 targetAngle = GetTargetAngle(direction);
		if (targetAngle.x >= verticalAngleLimit.x && targetAngle.x <= verticalAngleLimit.y && targetAngle.y >= horizontalAngleLimit.x)
		{
			return targetAngle.y <= horizontalAngleLimit.y;
		}
		return false;
	}

	public virtual void SetAlwaysFollowCamera(bool value)
	{
		alwaysFollowCamera = value;
	}

	public virtual void SetLookTarget(vLookTarget target, bool priority = false)
	{
		if (!targetsInArea.Contains(target))
		{
			targetsInArea.Add(target);
		}
		if (priority)
		{
			currentLookTarget = target;
		}
	}

	public virtual void SetLookTarget(Transform target)
	{
		simpleTarget = target;
	}

	public virtual void SetTemporaryLookPoint(Vector3 point, float time = 1f)
	{
		temporaryLookPoint = point;
		temporaryLookTime = time;
	}

	public virtual void RemoveLookTarget(vLookTarget target)
	{
		if (targetsInArea.Contains(target))
		{
			targetsInArea.Remove(target);
		}
		if (currentLookTarget == target)
		{
			currentLookTarget = null;
		}
	}

	public virtual void RemoveLookTarget(Transform target)
	{
		if (simpleTarget == target)
		{
			simpleTarget = null;
		}
	}

	private float NormalizeAngle(float angle)
	{
		if (angle > 180f)
		{
			angle -= 360f;
		}
		else if (angle < -180f)
		{
			angle += 360f;
		}
		return angle;
	}

	private void ResetValues()
	{
		_currentHeadWeight = 0f;
		_currentbodyWeight = 0f;
		yRotation = 0f;
		xRotation = 0f;
	}

	private void SmoothValues(float _headWeight = 0f, float _bodyWeight = 0f, float _x = 0f, float _y = 0f)
	{
		_currentHeadWeight = Mathf.Lerp(_currentHeadWeight, _headWeight, Smooth);
		_currentbodyWeight = Mathf.Lerp(_currentbodyWeight, _bodyWeight, Smooth);
		yRotation = Mathf.Lerp(yRotation, _y, Smooth);
		xRotation = Mathf.Lerp(xRotation, _x, Smooth);
		yRotation = Mathf.Clamp(yRotation, horizontalAngleLimit.x, horizontalAngleLimit.y);
		xRotation = Mathf.Clamp(xRotation, verticalAngleLimit.x, verticalAngleLimit.y);
	}

	private void SortTargets()
	{
		interation += Time.deltaTime;
		if (!(interation > updateTargetInteration))
		{
			return;
		}
		interation -= updateTargetInteration;
		if (targetsInArea == null || targetsInArea.Count < 2)
		{
			if (targetsInArea != null && targetsInArea.Count > 0)
			{
				currentLookTarget = targetsInArea[0];
			}
			return;
		}
		for (int num = targetsInArea.Count - 1; num >= 0; num--)
		{
			if (targetsInArea[num] == null)
			{
				targetsInArea.RemoveAt(num);
			}
		}
		targetsInArea.Sort((vLookTarget c1, vLookTarget c2) => Vector3.Distance(base.transform.position, (c1 != null) ? c1.transform.position : (Vector3.one * float.PositiveInfinity)).CompareTo(Vector3.Distance(base.transform.position, (c2 != null) ? c2.transform.position : (Vector3.one * float.PositiveInfinity))));
		if (targetsInArea.Count > 0)
		{
			currentLookTarget = targetsInArea[0];
		}
	}

	public virtual void OnDetect(Collider other)
	{
		if (tagsToDetect.Contains(other.gameObject.tag) && other.GetComponent<vLookTarget>() != null)
		{
			currentLookTarget = other.GetComponent<vLookTarget>();
			vHeadTrack componentInParent = other.GetComponentInParent<vHeadTrack>();
			if (!targetsInArea.Contains(currentLookTarget) && (componentInParent == null || componentInParent != this))
			{
				targetsInArea.Add(currentLookTarget);
				SortTargets();
				currentLookTarget = targetsInArea[0];
			}
		}
	}

	public virtual void OnLost(Collider other)
	{
		if (!tagsToDetect.Contains(other.gameObject.tag) || !(other.GetComponentInParent<vLookTarget>() != null))
		{
			return;
		}
		vLookTarget componentInParent = other.GetComponentInParent<vLookTarget>();
		if (targetsInArea.Contains(componentInParent))
		{
			targetsInArea.Remove(componentInParent);
			if (componentInParent == lastLookTarget)
			{
				componentInParent.ExitLook(this);
			}
		}
		SortTargets();
		if (targetsInArea.Count > 0)
		{
			currentLookTarget = targetsInArea[0];
		}
		else
		{
			currentLookTarget = null;
		}
	}

	public virtual bool IgnoreHeadTrack()
	{
		if (animatorIgnoreTags.Exists((string tag) => IsAnimatorTag(tag)))
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
		if (animatorStateInfo.isValid() && animatorStateInfo.animatorStateInfos.HasTag(tag))
		{
			return true;
		}
		return false;
	}
}
