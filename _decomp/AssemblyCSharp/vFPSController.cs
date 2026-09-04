using Invector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
[vClassHeader("vFPS Simple Controller", "Simple FPS Controller added as bonus just to see the AI in action.")]
public class vFPSController : vMonoBehaviour
{
	[vEditorToolbar("Controller Settings", false, "", false, false)]
	public float walkSpeed = 6f;

	public float runSpeed = 11f;

	public float jumpSpeed = 8f;

	public float gravity = 20f;

	private float antiBumpFactor = 0.75f;

	private int antiBunnyHopFactor = 1;

	private Vector3 moveDirection = Vector3.zero;

	private bool grounded;

	private CharacterController controller;

	private Transform myTransform;

	private float speed;

	private bool crouch;

	private int jumpTimer;

	[vEditorToolbar("Camera Settings", false, "", false, false)]
	public AnimationCurve cameraWalkCurve;

	public Camera _camera;

	public Transform cameraPivot;

	public float cameraHeight = 1f;

	public float cameraWalkEffectSpeed = 0.5f;

	public float cameraSensitivityX = 1f;

	public float cameraSensitivityY = 1f;

	[vEditorToolbar("Inputs", false, "", false, false)]
	public string horizontalInput = "Horizontal";

	public string verticalInput = "Vertical";

	[Header("Camera Input")]
	public string rotateCameraXInput = "Mouse X";

	public string rotateCameraYInput = "Mouse Y";

	private float inputX;

	private float inputY;

	private float cameraWalkNormalizedTime;

	private float cameraWalkProgress;

	[vEditorToolbar("Events", false, "", false, false)]
	public UnityEvent onAttack;

	public UnityEvent onStep;

	private void Start()
	{
		_camera = Camera.main;
		controller = GetComponent<CharacterController>();
		myTransform = base.transform;
		speed = walkSpeed;
		jumpTimer = antiBunnyHopFactor;
	}

	private void Update()
	{
		AttackInput();
		MoveController();
		RotationControl();
		CameraWalkEffect();
	}

	private void MoveController()
	{
		inputX = Input.GetAxis(horizontalInput);
		inputY = Input.GetAxis(verticalInput);
		float num = ((inputX != 0f && inputY != 0f) ? 0.7071f : 1f);
		if (grounded)
		{
			speed = (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed);
			crouch = Input.GetKey(KeyCode.C);
			if (crouch)
			{
				controller.height = 1f;
				controller.center = Vector3.up * 0.5f;
				cameraHeight = 0.95f;
			}
			else
			{
				controller.height = 2f;
				controller.center = Vector3.up;
				cameraHeight = 1.5f;
			}
			moveDirection = new Vector3(inputX * num, 0f - antiBumpFactor, inputY * num);
			moveDirection = myTransform.TransformDirection(moveDirection) * speed;
			if (!Input.GetKeyDown(KeyCode.Space))
			{
				jumpTimer++;
			}
			else if (jumpTimer >= antiBunnyHopFactor)
			{
				moveDirection.y = jumpSpeed;
				jumpTimer = 0;
			}
		}
		else
		{
			moveDirection.x = inputX * speed * num;
			moveDirection.z = inputY * speed * num;
			moveDirection = myTransform.TransformDirection(moveDirection);
		}
		moveDirection.y -= gravity * Time.deltaTime;
		grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
	}

	private void AttackInput()
	{
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			onAttack.Invoke();
		}
	}

	private void RotationControl()
	{
		Vector3 eulerAngles = base.transform.eulerAngles;
		Vector3 localEulerAngles = _camera.transform.localEulerAngles;
		eulerAngles.y += Input.GetAxis(rotateCameraXInput) * cameraSensitivityX;
		base.transform.eulerAngles = eulerAngles;
		localEulerAngles.x -= Input.GetAxis(rotateCameraYInput) * cameraSensitivityY;
		localEulerAngles = localEulerAngles.NormalizeAngle();
		localEulerAngles.x = Mathf.Clamp(localEulerAngles.x, -45f, 45f);
		_camera.transform.localEulerAngles = localEulerAngles;
	}

	private void CameraWalkEffect()
	{
		cameraWalkNormalizedTime += Time.deltaTime * Mathf.Abs(Mathf.Clamp(inputX + inputY, -1f, 1f)) * speed * cameraWalkEffectSpeed;
		cameraWalkProgress = cameraWalkNormalizedTime % 1f;
		float num = cameraHeight * (1f + cameraWalkCurve.Evaluate(cameraWalkProgress));
		Vector3 localPosition = new Vector3(cameraPivot.transform.localPosition.x, 0f, cameraPivot.transform.localPosition.z) + Vector3.up * num;
		cameraPivot.transform.localPosition = localPosition;
	}

	public void ReloadScene()
	{
		SceneManager.LoadScene("AI_Examples");
	}
}
