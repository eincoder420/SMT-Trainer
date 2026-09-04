using UnityEngine;

public class CharacterControl : MonoBehaviour
{
	public float speed = 6f;

	private float xRotation;

	private Transform cam;

	private CharacterController charController;

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		charController = GetComponent<CharacterController>();
		cam = base.transform.Find("Camera");
	}

	private void Update()
	{
		CameraMovement();
		Vector3 vector = base.transform.right * Input.GetAxis("Horizontal") + base.transform.forward * Input.GetAxis("Vertical");
		charController.SimpleMove(Vector3.ClampMagnitude(vector, 1f) * (Input.GetKey(KeyCode.LeftShift) ? (speed * 1.4f) : speed));
	}

	private void CameraMovement()
	{
		Vector2 vector = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
		xRotation -= vector.y;
		cam.localRotation = Quaternion.Euler(Mathf.Clamp(xRotation, -70f, 70f), 0f, 0f);
		base.transform.transform.Rotate(Vector3.up * vector.x);
	}
}
