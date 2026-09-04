using UnityEngine;

namespace Invector.vCharacterController;

public class vMousePositionHandler : MonoBehaviour
{
	public Camera mainCamera;

	protected static vMousePositionHandler _instance;

	public string joystickHorizontalAxis = "RightAnalogHorizontal";

	public string joystickVerticalAxis = "RightAnalogVertical";

	public float joystickSensitivity = 25f;

	public bool clampScreen = true;

	public Vector2 joystickMousePos;

	public static vMousePositionHandler Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindObjectOfType<vMousePositionHandler>();
			}
			if (_instance == null)
			{
				_instance = new GameObject("MousePositionHandler").AddComponent<vMousePositionHandler>();
				_instance.mainCamera = Camera.main;
			}
			return _instance;
		}
	}

	public virtual Vector2 mousePosition
	{
		get
		{
			switch (vInput.instance.inputDevice)
			{
			case InputDevice.MouseKeyboard:
				return Input.mousePosition;
			case InputDevice.Joystick:
			{
				joystickMousePos.x += Input.GetAxis("RightAnalogHorizontal") * joystickSensitivity;
				joystickMousePos.x = Mathf.Clamp(joystickMousePos.x, 0f - (float)Screen.width * 0.5f, (float)Screen.width * 0.5f);
				joystickMousePos.y += Input.GetAxis("RightAnalogVertical") * joystickSensitivity;
				joystickMousePos.y = Mathf.Clamp(joystickMousePos.y, 0f - (float)Screen.height * 0.5f, (float)Screen.height * 0.5f);
				Vector2 vector = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
				Vector2 result = joystickMousePos + vector;
				result.x = Mathf.Clamp(result.x, 0f, Screen.width);
				result.y = Mathf.Clamp(result.y, 0f, Screen.height);
				return result;
			}
			case InputDevice.Mobile:
				if (clampScreen)
				{
					return Input.GetTouch(0).deltaPosition;
				}
				return joystickMousePos;
			default:
				return Input.mousePosition;
			}
		}
	}

	public virtual void SetMousePosition(Vector2 pos)
	{
		joystickMousePos = pos;
	}

	public virtual Vector3 WorldMousePosition(LayerMask castLayer, out Collider collider)
	{
		if (!mainCamera)
		{
			if (!Camera.main)
			{
				Debug.LogWarning("Trying to get the world mouse position but a MainCamera is missing from the scene");
				collider = null;
				return Vector3.zero;
			}
			mainCamera = Camera.main;
			collider = null;
			return Vector3.zero;
		}
		Ray ray = mainCamera.ScreenPointToRay(mousePosition);
		if (Physics.Raycast(ray, out var hitInfo, mainCamera.farClipPlane, castLayer))
		{
			collider = hitInfo.collider;
			return hitInfo.point;
		}
		collider = null;
		return ray.GetPoint(mainCamera.farClipPlane);
	}

	public virtual Vector3 WorldMousePosition(LayerMask castLayer)
	{
		if (!mainCamera)
		{
			if (!Camera.main)
			{
				Debug.LogWarning("Trying to get the world mouse position but a MainCamera is missing from the scene");
				return Vector3.zero;
			}
			mainCamera = Camera.main;
			return Vector3.zero;
		}
		Ray ray = mainCamera.ScreenPointToRay(mousePosition);
		if (Physics.Raycast(ray, out var hitInfo, mainCamera.farClipPlane, castLayer))
		{
			return hitInfo.point;
		}
		return ray.GetPoint(mainCamera.farClipPlane);
	}
}
