using UnityEngine;

namespace BeautifyEffect;

public class FreeCameraMove : MonoBehaviour
{
	public float cameraSensitivity = 150f;

	public float climbSpeed = 20f;

	public float normalMoveSpeed = 20f;

	public float slowMoveFactor = 0.25f;

	public float fastMoveFactor = 3f;

	private float rotationX;

	private float rotationY;

	private Quaternion originalRotation;

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		originalRotation = Camera.main.transform.rotation;
	}

	private void Update()
	{
		Vector2 vector = Input.mousePosition;
		if (!(vector.x < 0f) && !(vector.x > (float)Screen.width) && !(vector.y < 0f) && !(vector.y > (float)Screen.height))
		{
			rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
			rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
			rotationY = Mathf.Clamp(rotationY, -90f, 90f);
			base.transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
			base.transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
			base.transform.localRotation *= originalRotation;
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				base.transform.position += base.transform.forward * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
				base.transform.position += base.transform.right * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
			}
			else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				base.transform.position += base.transform.forward * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
				base.transform.position += base.transform.right * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
			}
			else
			{
				base.transform.position += base.transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
				base.transform.position += base.transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.Q))
			{
				base.transform.position -= base.transform.up * climbSpeed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.E))
			{
				base.transform.position += base.transform.up * climbSpeed * Time.deltaTime;
			}
			if (base.transform.position.y < 1f)
			{
				base.transform.position += Vector3.up * (1f - base.transform.position.y);
			}
		}
	}
}
