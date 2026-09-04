using UnityEngine;

namespace MeshCombineStudio;

public class NavigationCamera : MonoBehaviour
{
	public static float fov;

	public SO_NavigationCamera data;

	private Quaternion rot;

	private Vector3 currentSpeed;

	private float tStamp;

	private float deltaTime;

	private Vector3 startPosition;

	private Vector3 position;

	private Quaternion startRotation;

	private float scrollWheel;

	private void Awake()
	{
		tStamp = Time.realtimeSinceStartup;
		startPosition = (position = base.transform.position);
		startRotation = (rot = base.transform.rotation);
	}

	private void OnDestroy()
	{
		RestoreCam();
	}

	private void Update()
	{
		scrollWheel = Input.mouseScrollDelta.y * data.mouseScrollWheelMulti;
	}

	private void LateUpdate()
	{
		deltaTime = Time.realtimeSinceStartup - tStamp;
		tStamp = Time.realtimeSinceStartup;
		Vector2 vector = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		if (Input.GetMouseButtonDown(1))
		{
			rot = base.transform.rotation;
			vector = Vector2.zero;
		}
		Vector3 zero = Vector3.zero;
		if (Input.GetMouseButton(1))
		{
			Quaternion rotation = base.transform.rotation;
			base.transform.Rotate(0f, vector.x * data.mouseSensitity * 1.66f, 0f, Space.World);
			base.transform.Rotate((0f - vector.y) * data.mouseSensitity * 1.66f, 0f, 0f, Space.Self);
			rot = base.transform.rotation;
			base.transform.rotation = rotation;
			if (Input.GetKey(KeyCode.W))
			{
				zero.z = 1f;
			}
			else if (Input.GetKey(KeyCode.S))
			{
				zero.z = -1f;
			}
			if (Input.GetKey(KeyCode.D))
			{
				zero.x = 1f;
			}
			else if (Input.GetKey(KeyCode.A))
			{
				zero.x = -1f;
			}
			if (Input.GetKey(KeyCode.E))
			{
				zero.y = 1f;
			}
			else if (Input.GetKey(KeyCode.Q))
			{
				zero.y = -1f;
			}
			zero *= GetSpeedMulti();
		}
		if (Input.GetMouseButton(2))
		{
			zero.x = 0f - vector.x;
			zero.y = 0f - vector.y;
			zero *= GetSpeedMulti();
			currentSpeed = zero;
		}
		else
		{
			Lerp2Way(ref currentSpeed, zero, data.speedUpLerpMulti, data.speedDownLerpMulti);
		}
		position += base.transform.TransformDirection(currentSpeed * deltaTime) + base.transform.forward * scrollWheel * deltaTime;
		base.transform.rotation = rot;
		base.transform.position = position;
	}

	public void SetCam()
	{
		base.transform.rotation = rot;
		base.transform.position = position;
	}

	public void RestoreCam()
	{
		base.transform.position = startPosition;
		base.transform.rotation = startRotation;
	}

	private float GetSpeedMulti()
	{
		if (Input.GetKey(KeyCode.LeftShift))
		{
			return data.speedFast;
		}
		if (Input.GetKey(KeyCode.LeftControl))
		{
			return data.speedSlow;
		}
		return data.speedNormal;
	}

	private void Lerp2Way(ref Vector3 v, Vector3 targetV, float upMulti, float downMulti)
	{
		Lerp2Way(ref v.x, targetV.x, upMulti, downMulti);
		Lerp2Way(ref v.y, targetV.y, upMulti, downMulti);
		Lerp2Way(ref v.z, targetV.z, upMulti, downMulti);
	}

	private void Lerp2Way(ref float v, float targetV, float upMulti, float downMulti)
	{
		v = Mathf.Lerp(t: ((!(Mathf.Abs(v) < Mathf.Abs(targetV))) ? downMulti : upMulti) * deltaTime, a: v, b: targetV);
	}
}
