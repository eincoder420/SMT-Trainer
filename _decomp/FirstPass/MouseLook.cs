using UnityEngine;

public class MouseLook : MonoBehaviour
{
	public enum RotationAxes
	{
		MouseXAndY,
		MouseX,
		MouseY
	}

	public RotationAxes axes;

	public float sensitivityX = 3f;

	public float sensitivityY = 3f;

	public float minimumX = -360f;

	public float maximumX = 360f;

	public float minimumY = -80f;

	public float maximumY = 80f;

	public float rotationX;

	public float rotationY;

	private Quaternion originalRotation;

	private void Update()
	{
		if (axes == RotationAxes.MouseXAndY)
		{
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationX = ClampAngle(rotationX, minimumX, maximumX);
			rotationY = ClampAngle(rotationY, minimumY, maximumY);
			Quaternion quaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
			Quaternion quaternion2 = Quaternion.AngleAxis(rotationY, Vector3.left);
			base.transform.localRotation = originalRotation * quaternion * quaternion2;
		}
	}

	private void Start()
	{
		originalRotation = base.transform.localRotation;
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
