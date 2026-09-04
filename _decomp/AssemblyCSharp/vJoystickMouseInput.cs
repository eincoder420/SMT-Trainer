using System;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.EventSystems;

public class vJoystickMouseInput : BaseInput
{
	[Serializable]
	public class JoystickAxisInput
	{
		public string vertical = "LeftAnalogVertical";

		public string horizontal = "LeftAnalogHorizontal";

		public float horizontalAxis => Input.GetAxis(horizontal);

		public float verticalAxis => Input.GetAxis(vertical);
	}

	public StandaloneInputModule inputModule;

	public RectTransform cursor;

	public JoystickAxisInput joystickAxisInput;

	public BaseInput oldOverride;

	protected Vector2 CursorPosition = Vector2.zero;

	public float mouseSpeed = 4f;

	public override Vector2 mousePosition
	{
		get
		{
			if (vInput.instance.inputDevice == InputDevice.Joystick)
			{
				if ((bool)cursor && (!cursor.gameObject.activeSelf || Cursor.visible))
				{
					Cursor.visible = false;
					CursorPosition = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
					cursor.gameObject.SetActive(value: true);
					EventSystem.current.SetSelectedGameObject(null);
				}
				CursorPosition.x += joystickAxisInput.horizontalAxis * mouseSpeed;
				CursorPosition.x = Mathf.Clamp(CursorPosition.x, 0f, Screen.width);
				CursorPosition.y += joystickAxisInput.verticalAxis * mouseSpeed;
				CursorPosition.y = Mathf.Clamp(CursorPosition.y, 0f, Screen.height);
			}
			else
			{
				if ((bool)cursor && cursor.gameObject.activeSelf)
				{
					Cursor.visible = true;
					cursor.gameObject.SetActive(value: false);
				}
				CursorPosition = base.mousePosition;
			}
			if ((bool)cursor)
			{
				cursor.position = CursorPosition;
			}
			return CursorPosition;
		}
	}

	public virtual string submitButton => inputModule.submitButton;

	protected override void OnEnable()
	{
		if ((bool)inputModule)
		{
			inputModule.inputOverride = this;
		}
	}

	protected override void OnDisable()
	{
		if ((bool)inputModule)
		{
			inputModule.inputOverride = oldOverride;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (!inputModule)
		{
			inputModule = UnityEngine.Object.FindObjectOfType<StandaloneInputModule>();
		}
		if ((bool)inputModule)
		{
			oldOverride = inputModule.inputOverride;
			inputModule.inputOverride = this;
		}
	}

	public override bool GetMouseButton(int button)
	{
		InputDevice inputDevice = vInput.instance.inputDevice;
		if (inputDevice == InputDevice.Joystick)
		{
			if (button == 0)
			{
				return Input.GetButton(submitButton);
			}
			return base.GetMouseButton(button);
		}
		return base.GetMouseButton(button);
	}

	public override bool GetMouseButtonUp(int button)
	{
		InputDevice inputDevice = vInput.instance.inputDevice;
		if (inputDevice == InputDevice.Joystick)
		{
			if (button == 0)
			{
				return Input.GetButtonUp(submitButton);
			}
			return base.GetMouseButtonUp(button);
		}
		return base.GetMouseButtonUp(button);
	}

	public override bool GetMouseButtonDown(int button)
	{
		InputDevice inputDevice = vInput.instance.inputDevice;
		if (inputDevice == InputDevice.Joystick)
		{
			if (button == 0)
			{
				return Input.GetButtonDown(submitButton);
			}
			return base.GetMouseButtonDown(button);
		}
		return base.GetMouseButtonDown(button);
	}
}
