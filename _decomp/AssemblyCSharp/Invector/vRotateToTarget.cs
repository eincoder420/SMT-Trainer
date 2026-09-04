using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Invector;

[vClassHeader("Rotate To Target", true, "icon_v2", false, "")]
public class vRotateToTarget : vMonoBehaviour
{
	public Transform targetTransform;

	[FormerlySerializedAs("angleRoot")]
	public Transform angleRootH;

	public Transform rotatorH;

	public Transform angleRootV;

	[FormerlySerializedAs("rotator")]
	public Transform rotatorV;

	public bool rotateH;

	public bool rotateV;

	public float rotationSpeedInAngle;

	public float rotationSpeedOutAngle;

	[Range(0f, 180f)]
	public float maxAngleVertical = 60f;

	[Range(0f, 180f)]
	public float maxAngleHorizontal = 60f;

	[Range(0f, 180f)]
	public float angleToReachTarget = 45f;

	public UnityEvent onEnterAngle;

	public UnityEvent onStayAngle;

	public UnityEvent onExitAngle;

	protected float angleH;

	protected float angleV;

	public bool targetIsInAngleRange { get; protected set; }

	protected virtual void Start()
	{
		if (angleRootH == null)
		{
			angleRootH = base.transform;
		}
		if (angleRootV == null)
		{
			angleRootV = base.transform;
		}
	}

	protected virtual void Update()
	{
		if (!angleRootH || !rotatorV)
		{
			return;
		}
		Transform transform = targetTransform;
		if ((bool)rotatorV)
		{
			angleV = rotatorV.localEulerAngles.x;
		}
		if ((bool)rotatorH)
		{
			angleH = rotatorH.localEulerAngles.y;
		}
		if ((bool)transform)
		{
			Vector3 vector = transform.position - angleRootV.position;
			Vector3 vector2 = transform.position - angleRootH.position;
			float x = angleRootV.forward.AngleFormOtherDirection(vector.normalized).x;
			float y = angleRootH.forward.AngleFormOtherDirection(vector2.normalized).y;
			bool flag = Mathf.Abs(x) <= maxAngleVertical && Mathf.Abs(y) <= maxAngleHorizontal;
			if (flag != targetIsInAngleRange)
			{
				if (flag)
				{
					onEnterAngle.Invoke();
				}
				else
				{
					onExitAngle.Invoke();
				}
				targetIsInAngleRange = flag;
			}
			if (targetIsInAngleRange)
			{
				onStayAngle.Invoke();
				angleV = Mathf.LerpAngle(angleV, x, rotationSpeedInAngle * Time.deltaTime);
				angleH = Mathf.LerpAngle(angleH, y, rotationSpeedInAngle * Time.deltaTime);
			}
			else
			{
				angleV = Mathf.LerpAngle(angleV, 0f, rotationSpeedOutAngle * Time.deltaTime);
				angleH = Mathf.LerpAngle(angleH, 0f, rotationSpeedOutAngle * Time.deltaTime);
			}
		}
		else
		{
			if (targetIsInAngleRange)
			{
				onExitAngle.Invoke();
				targetIsInAngleRange = false;
			}
			if (rotatorV.localEulerAngles.magnitude > 0f)
			{
				angleV = Mathf.LerpAngle(angleV, 0f, rotationSpeedOutAngle * Time.deltaTime);
				angleH = Mathf.LerpAngle(angleH, 0f, rotationSpeedOutAngle * Time.deltaTime);
			}
		}
		if (rotateV && rotateV)
		{
			Vector3 localEulerAngles = rotatorV.localEulerAngles;
			localEulerAngles.x = angleV;
			rotatorV.localEulerAngles = localEulerAngles;
		}
		if (rotateH && (bool)rotatorH)
		{
			Vector3 localEulerAngles2 = rotatorH.localEulerAngles;
			localEulerAngles2.y = angleH;
			rotatorH.localEulerAngles = localEulerAngles2;
		}
	}

	public void SetTarget(Transform target)
	{
		targetTransform = target;
	}

	public void ClearTarget()
	{
		targetTransform = null;
	}
}
