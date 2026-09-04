using UnityEngine;

namespace Invector.vCharacterController;

public class vInput : MonoBehaviour
{
	public delegate void OnChangeInputType(InputDevice type);

	private static vInput _instance;

	public vHUDController hud;

	private InputDevice _inputType;

	public static vInput instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindObjectOfType<vInput>();
				if (_instance == null)
				{
					_instance = new GameObject("vInputType").AddComponent<vInput>();
					return _instance;
				}
			}
			return _instance;
		}
	}

	[HideInInspector]
	public InputDevice inputDevice
	{
		get
		{
			return _inputType;
		}
		set
		{
			_inputType = value;
			OnChangeInput();
		}
	}

	public event OnChangeInputType onChangeInputType;

	private void Start()
	{
		if (hud == null)
		{
			hud = vHUDController.instance;
		}
	}

	private void OnGUI()
	{
		switch (inputDevice)
		{
		case InputDevice.MouseKeyboard:
			if (isJoystickInput())
			{
				inputDevice = InputDevice.Joystick;
				if (hud != null)
				{
					hud.controllerInput = true;
					hud.ShowText("Control scheme changed to Controller", 2f, 0.5f);
				}
			}
			else if (isMobileInput())
			{
				inputDevice = InputDevice.Mobile;
				if (hud != null)
				{
					hud.controllerInput = true;
					hud.ShowText("Control scheme changed to Mobile", 2f, 0.5f);
				}
			}
			break;
		case InputDevice.Joystick:
			if (isMouseKeyboard())
			{
				inputDevice = InputDevice.MouseKeyboard;
				if (hud != null)
				{
					hud.controllerInput = false;
					hud.ShowText("Control scheme changed to Keyboard/Mouse", 2f, 0.5f);
				}
			}
			else if (isMobileInput())
			{
				inputDevice = InputDevice.Mobile;
				if (hud != null)
				{
					hud.controllerInput = true;
					hud.ShowText("Control scheme changed to Mobile", 2f, 0.5f);
				}
			}
			break;
		case InputDevice.Mobile:
			if (isMouseKeyboard())
			{
				inputDevice = InputDevice.MouseKeyboard;
				if (hud != null)
				{
					hud.controllerInput = false;
					hud.ShowText("Control scheme changed to Keyboard/Mouse", 2f, 0.5f);
				}
			}
			else if (isJoystickInput())
			{
				inputDevice = InputDevice.Joystick;
				if (hud != null)
				{
					hud.controllerInput = true;
					hud.ShowText("Control scheme changed to Controller", 2f, 0.5f);
				}
			}
			break;
		}
	}

	private bool isMobileInput()
	{
		return false;
	}

	private bool isMouseKeyboard()
	{
		if (Event.current.isKey || Event.current.isMouse)
		{
			return true;
		}
		if (Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f)
		{
			return true;
		}
		return false;
	}

	private bool isJoystickInput()
	{
		if (Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Joystick1Button1) || Input.GetKey(KeyCode.Joystick1Button2) || Input.GetKey(KeyCode.Joystick1Button3) || Input.GetKey(KeyCode.Joystick1Button4) || Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.Joystick1Button6) || Input.GetKey(KeyCode.Joystick1Button7) || Input.GetKey(KeyCode.Joystick1Button8) || Input.GetKey(KeyCode.Joystick1Button9) || Input.GetKey(KeyCode.Joystick1Button10) || Input.GetKey(KeyCode.Joystick1Button11) || Input.GetKey(KeyCode.Joystick1Button12) || Input.GetKey(KeyCode.Joystick1Button13) || Input.GetKey(KeyCode.Joystick1Button14) || Input.GetKey(KeyCode.Joystick1Button15) || Input.GetKey(KeyCode.Joystick1Button16) || Input.GetKey(KeyCode.Joystick1Button17) || Input.GetKey(KeyCode.Joystick1Button18) || Input.GetKey(KeyCode.Joystick1Button19))
		{
			return true;
		}
		if (Input.GetAxis("LeftAnalogHorizontal") != 0f || Input.GetAxis("LeftAnalogVertical") != 0f || Input.GetAxis("RightAnalogHorizontal") != 0f || Input.GetAxis("RightAnalogVertical") != 0f || Input.GetAxis("LT") != 0f || Input.GetAxis("RT") != 0f || Input.GetAxis("D-Pad Horizontal") != 0f || Input.GetAxis("D-Pad Vertical") != 0f)
		{
			return true;
		}
		return false;
	}

	private void OnChangeInput()
	{
		if (this.onChangeInputType != null)
		{
			this.onChangeInputType(inputDevice);
		}
	}
}
