using System;
using UnityEngine;

namespace Invector.vCharacterController;

[Serializable]
public class GenericInput
{
	public bool useInput = true;

	[SerializeField]
	private bool isAxisInUse;

	[SerializeField]
	private bool isUnityInput;

	[SerializeField]
	public string keyboard;

	[SerializeField]
	public bool keyboardAxis;

	[SerializeField]
	public string joystick;

	[SerializeField]
	public bool joystickAxis;

	[SerializeField]
	public string mobile;

	[SerializeField]
	public bool mobileAxis;

	[SerializeField]
	public bool joystickAxisInvert;

	[SerializeField]
	public bool keyboardAxisInvert;

	[SerializeField]
	public bool mobileAxisInvert;

	public float timeButtonWasPressed;

	public float lastTimeTheButtonWasPressed;

	public bool inButtomTimer;

	private float multTapTimer;

	private int multTapCounter;

	protected InputDevice inputDevice => vInput.instance.inputDevice;

	public bool isAxis
	{
		get
		{
			bool result = false;
			switch (inputDevice)
			{
			case InputDevice.Joystick:
				result = joystickAxis;
				break;
			case InputDevice.MouseKeyboard:
				result = keyboardAxis;
				break;
			case InputDevice.Mobile:
				result = mobileAxis;
				break;
			}
			return result;
		}
	}

	public bool isAxisInvert
	{
		get
		{
			bool result = false;
			switch (inputDevice)
			{
			case InputDevice.Joystick:
				result = joystickAxisInvert;
				break;
			case InputDevice.MouseKeyboard:
				result = keyboardAxisInvert;
				break;
			case InputDevice.Mobile:
				result = mobileAxisInvert;
				break;
			}
			return result;
		}
	}

	public string buttonName
	{
		get
		{
			if (vInput.instance != null)
			{
				if (vInput.instance.inputDevice == InputDevice.MouseKeyboard)
				{
					return keyboard.ToString();
				}
				if (vInput.instance.inputDevice == InputDevice.Joystick)
				{
					return joystick;
				}
				return mobile;
			}
			return string.Empty;
		}
	}

	public bool isKey
	{
		get
		{
			if (vInput.instance != null && !isUnityInput && Enum.IsDefined(typeof(KeyCode), buttonName))
			{
				return true;
			}
			return false;
		}
	}

	public KeyCode key => (KeyCode)Enum.Parse(typeof(KeyCode), buttonName);

	public GenericInput(string keyboard, string joystick, string mobile)
	{
		this.keyboard = keyboard;
		this.joystick = joystick;
		this.mobile = mobile;
	}

	public GenericInput(string keyboard, bool keyboardAxis, string joystick, bool joystickAxis, string mobile, bool mobileAxis)
	{
		this.keyboard = keyboard;
		this.keyboardAxis = keyboardAxis;
		this.joystick = joystick;
		this.joystickAxis = joystickAxis;
		this.mobile = mobile;
		this.mobileAxis = mobileAxis;
	}

	public GenericInput(string keyboard, bool keyboardAxis, bool keyboardInvert, string joystick, bool joystickAxis, bool joystickInvert, string mobile, bool mobileAxis, bool mobileInvert)
	{
		this.keyboard = keyboard;
		this.keyboardAxis = keyboardAxis;
		keyboardAxisInvert = keyboardInvert;
		this.joystick = joystick;
		this.joystickAxis = joystickAxis;
		joystickAxisInvert = joystickInvert;
		this.mobile = mobile;
		this.mobileAxis = mobileAxis;
		mobileAxisInvert = mobileInvert;
	}

	public bool GetButton()
	{
		if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName))
		{
			return false;
		}
		if (isAxis)
		{
			return GetAxisButton();
		}
		if (inputDevice == InputDevice.Mobile)
		{
			return true;
		}
		if (inputDevice == InputDevice.MouseKeyboard)
		{
			if (isKey)
			{
				if (Input.GetKey(key))
				{
					return true;
				}
			}
			else if (Input.GetButton(buttonName))
			{
				return true;
			}
		}
		else if (inputDevice == InputDevice.Joystick && Input.GetButton(buttonName))
		{
			return true;
		}
		return false;
	}

	public bool GetButtonDown()
	{
		if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName))
		{
			return false;
		}
		if (isAxis)
		{
			return GetAxisButtonDown();
		}
		if (inputDevice == InputDevice.Mobile)
		{
			return true;
		}
		if (inputDevice == InputDevice.MouseKeyboard)
		{
			if (isKey)
			{
				if (Input.GetKeyDown(key))
				{
					return true;
				}
			}
			else if (Input.GetButtonDown(buttonName))
			{
				return true;
			}
		}
		else if (inputDevice == InputDevice.Joystick && Input.GetButtonDown(buttonName))
		{
			return true;
		}
		return false;
	}

	public bool GetButtonUp()
	{
		if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName))
		{
			return false;
		}
		if (isAxis)
		{
			return GetAxisButtonUp();
		}
		if (inputDevice == InputDevice.Mobile)
		{
			return true;
		}
		if (inputDevice == InputDevice.MouseKeyboard)
		{
			if (isKey)
			{
				if (Input.GetKeyUp(key))
				{
					return true;
				}
			}
			else if (Input.GetButtonUp(buttonName))
			{
				return true;
			}
		}
		else if (inputDevice == InputDevice.Joystick && Input.GetButtonUp(buttonName))
		{
			return true;
		}
		return false;
	}

	public float GetAxis()
	{
		if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName) || isKey)
		{
			return 0f;
		}
		if (inputDevice != InputDevice.Mobile)
		{
			if (inputDevice == InputDevice.MouseKeyboard)
			{
				return Input.GetAxis(buttonName);
			}
			if (inputDevice == InputDevice.Joystick)
			{
				return Input.GetAxis(buttonName);
			}
		}
		return 0f;
	}

	public float GetAxisRaw()
	{
		if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName) || isKey)
		{
			return 0f;
		}
		if (inputDevice != InputDevice.Mobile)
		{
			if (inputDevice == InputDevice.MouseKeyboard)
			{
				return Input.GetAxisRaw(buttonName);
			}
			if (inputDevice == InputDevice.Joystick)
			{
				return Input.GetAxisRaw(buttonName);
			}
		}
		return 0f;
	}

	public bool GetDoubleButtonDown(float inputTime = 1f)
	{
		if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName))
		{
			return false;
		}
		if (multTapCounter == 0 && GetButtonDown())
		{
			multTapTimer = Time.time;
			multTapCounter = 1;
			return false;
		}
		if (multTapCounter == 1 && GetButtonDown())
		{
			float num = multTapTimer + inputTime;
			bool result = Time.time < num;
			multTapTimer = 0f;
			multTapCounter = 0;
			return result;
		}
		if (multTapCounter == 1 && multTapTimer + inputTime < Time.time)
		{
			multTapTimer = 0f;
			multTapCounter = 0;
		}
		return false;
	}

	public bool GetButtonTimer(float inputTime = 2f)
	{
		if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName))
		{
			return false;
		}
		if (GetButtonDown() && !inButtomTimer)
		{
			lastTimeTheButtonWasPressed = Time.time + 0.1f;
			timeButtonWasPressed = Time.time;
			inButtomTimer = true;
		}
		if (inButtomTimer)
		{
			bool flag = timeButtonWasPressed + inputTime - Time.time <= 0f;
			if (!GetButton() || lastTimeTheButtonWasPressed < Time.time)
			{
				inButtomTimer = false;
				return false;
			}
			lastTimeTheButtonWasPressed = Time.time + 0.1f;
			if (flag)
			{
				inButtomTimer = false;
			}
			return flag;
		}
		return false;
	}

	public bool GetButtonTimer(ref float currentTimer, float inputTime = 2f)
	{
		if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName))
		{
			return false;
		}
		if (GetButtonDown() && !inButtomTimer)
		{
			lastTimeTheButtonWasPressed = Time.time + 0.1f;
			timeButtonWasPressed = Time.time;
			inButtomTimer = true;
		}
		if (inButtomTimer)
		{
			float num = timeButtonWasPressed + inputTime;
			currentTimer = num - Time.time;
			bool flag = num - Time.time <= 0f;
			if (!GetButton() || lastTimeTheButtonWasPressed < Time.time)
			{
				inButtomTimer = false;
				return false;
			}
			lastTimeTheButtonWasPressed = Time.time + 0.1f;
			if (flag)
			{
				inButtomTimer = false;
			}
			return flag;
		}
		return false;
	}

	public bool GetButtonTimer(ref float currentTimer, ref bool upAfterPressed, float inputTime = 2f)
	{
		if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName))
		{
			return false;
		}
		if (GetButtonDown())
		{
			lastTimeTheButtonWasPressed = Time.time + 0.1f;
			timeButtonWasPressed = Time.time;
			inButtomTimer = true;
		}
		if (inButtomTimer)
		{
			float num = timeButtonWasPressed + inputTime;
			currentTimer = (inputTime - (num - Time.time)) / inputTime;
			bool flag = num - Time.time <= 0f;
			if (!GetButton() || lastTimeTheButtonWasPressed < Time.time)
			{
				inButtomTimer = false;
				upAfterPressed = true;
				return false;
			}
			upAfterPressed = false;
			lastTimeTheButtonWasPressed = Time.time + 0.1f;
			if (flag)
			{
				inButtomTimer = false;
			}
			return flag;
		}
		return false;
	}

	public bool GetAxisButton(float value = 0.5f)
	{
		if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName))
		{
			return false;
		}
		if (isAxisInvert)
		{
			value *= -1f;
		}
		if (value > 0f)
		{
			return GetAxisRaw() >= value;
		}
		if (value < 0f)
		{
			return GetAxisRaw() <= value;
		}
		return false;
	}

	public bool GetAxisButtonDown(float value = 0.5f)
	{
		if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName))
		{
			return false;
		}
		if (isAxisInvert)
		{
			value *= -1f;
		}
		if (value > 0f)
		{
			if (!isAxisInUse && GetAxisRaw() >= value)
			{
				isAxisInUse = true;
				return true;
			}
			if (isAxisInUse && GetAxisRaw() == 0f)
			{
				isAxisInUse = false;
			}
		}
		else if (value < 0f)
		{
			if (!isAxisInUse && GetAxisRaw() <= value)
			{
				isAxisInUse = true;
				return true;
			}
			if (isAxisInUse && GetAxisRaw() == 0f)
			{
				isAxisInUse = false;
			}
		}
		return false;
	}

	public bool GetAxisButtonUp()
	{
		if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName))
		{
			return false;
		}
		if (isAxisInUse && GetAxisRaw() == 0f)
		{
			isAxisInUse = false;
			return true;
		}
		if (!isAxisInUse && GetAxisRaw() != 0f)
		{
			isAxisInUse = true;
		}
		return false;
	}

	private bool IsButtonAvailable(string btnName)
	{
		if (!useInput)
		{
			return false;
		}
		try
		{
			if (isKey)
			{
				return true;
			}
			Input.GetButton(buttonName);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning(" Failure to try access button :" + buttonName + "\n" + ex.Message);
			return false;
		}
	}
}
