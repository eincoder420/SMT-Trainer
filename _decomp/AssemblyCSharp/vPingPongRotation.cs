using UnityEngine;

public class vPingPongRotation : MonoBehaviour
{
	[vHelpBox("This Component needs to be child of a root transform", vHelpBoxAttribute.MessageType.None)]
	[vMinMax(-180f, 180f)]
	public Vector2 angleX;

	[vMinMax(-180f, 180f)]
	public Vector2 angleY;

	[vMinMax(-180f, 180f)]
	public Vector2 angleZ;

	public Vector3 speed = Vector3.one;

	private Vector3 pingPongTime;

	private Vector3 euler;

	private float evaluateToDirection;

	private Vector3 defaultLocalForward;

	public Transform targetTransform;

	private Vector3 evaluate;

	private void Start()
	{
		if (targetTransform == null)
		{
			targetTransform = base.transform;
		}
		defaultLocalForward = targetTransform.parent.InverseTransformDirection(targetTransform.forward);
	}

	private void OnEnable()
	{
		evaluateToDirection = 0f;
	}

	public void Reset()
	{
		evaluateToDirection = 0f;
	}

	private void Update()
	{
		Vector3 vector = targetTransform.parent.TransformDirection(defaultLocalForward);
		if (angleX.magnitude > 0f)
		{
			pingPongTime.x = Time.time * speed.x;
		}
		if (angleY.magnitude > 0f)
		{
			pingPongTime.y = Time.time * speed.y;
		}
		if (angleZ.magnitude > 0f)
		{
			pingPongTime.z = Time.time * speed.z;
		}
		if (evaluateToDirection < 1f)
		{
			evaluateToDirection += Time.deltaTime * speed.magnitude;
		}
		else
		{
			evaluateToDirection = 1f;
		}
		if (angleX.magnitude > 0f)
		{
			evaluate.x = Mathf.PingPong(pingPongTime.x, 1f);
		}
		if (angleY.magnitude > 0f)
		{
			evaluate.y = Mathf.PingPong(pingPongTime.y, 1f);
		}
		if (angleZ.magnitude > 0f)
		{
			pingPongTime.z = Time.time * speed.z;
		}
		evaluate.z = Mathf.PingPong(pingPongTime.z, 1f);
		if (angleX.magnitude > 0f)
		{
			euler.x = Mathf.Lerp(angleX.x, angleX.y, evaluate.x);
		}
		if (angleY.magnitude > 0f)
		{
			euler.y = Mathf.Lerp(angleY.x, angleY.y, evaluate.y);
		}
		if (angleZ.magnitude > 0f)
		{
			pingPongTime.z = Time.time * speed.z;
		}
		euler.z = Mathf.Lerp(angleZ.x, angleZ.y, evaluate.z);
		targetTransform.forward = Vector3.Lerp(vector, Quaternion.Euler(euler) * vector, evaluateToDirection);
	}
}
