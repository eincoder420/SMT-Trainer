using System;
using Invector;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;

[vClassHeader("Simple Trigger Input", true, "icon_v2", false, "")]
public class vSimpleTriggerWithInput : vSimpleTrigger
{
	public enum InputType
	{
		GetButtonDown,
		GetDoubleButton,
		GetButtonTimer
	}

	[Serializable]
	public class OnUpdateValue : UnityEvent<float>
	{
	}

	public InputType inputType;

	[Tooltip("Input to make the action")]
	public GenericInput actionInput = new GenericInput("E", "A", "A");

	[vHelpBox("Time you have to hold the button *Only for GetButtonTimer*", vHelpBoxAttribute.MessageType.None)]
	public float buttonTimer = 3f;

	[vHelpBox("Add delay to start the input count *Only for GetButtonTimer*", vHelpBoxAttribute.MessageType.None)]
	public float inputDelay = 0.1f;

	[vHelpBox("Time to press the button twice *Only for GetDoubleButton*", vHelpBoxAttribute.MessageType.None)]
	public float doubleButtomTime = 0.25f;

	public float _currentInputDelay;

	public float currentButtonTimer;

	public UnityEvent OnPressButton;

	public UnityEvent OnCancelButtonTimer;

	public OnUpdateValue OnUpdateButtonTimer;

	private void Update()
	{
		if (!other)
		{
			_currentInputDelay = inputDelay;
		}
		else if (inputType == InputType.GetButtonDown)
		{
			if (actionInput.GetButtonDown())
			{
				OnPressButton.Invoke();
			}
		}
		else if (inputType == InputType.GetDoubleButton)
		{
			if (actionInput.GetDoubleButtonDown(doubleButtomTime))
			{
				OnPressButton.Invoke();
			}
		}
		else
		{
			if (inputType != InputType.GetButtonTimer)
			{
				return;
			}
			if (_currentInputDelay <= 0f)
			{
				bool upAfterPressed = false;
				float currentTimer = 0f;
				if (actionInput.GetButtonTimer(ref currentTimer, ref upAfterPressed, buttonTimer))
				{
					_currentInputDelay = inputDelay;
					OnPressButton.Invoke();
				}
				if (actionInput.inButtomTimer)
				{
					UpdateButtonTimer(currentTimer);
				}
				if (upAfterPressed)
				{
					CancelButtonTimer();
				}
			}
			else
			{
				_currentInputDelay -= Time.deltaTime;
			}
		}
	}

	public void UpdateButtonTimer(float value)
	{
		if (value != currentButtonTimer)
		{
			currentButtonTimer = value;
			OnUpdateButtonTimer.Invoke(value);
		}
	}

	private void CancelButtonTimer()
	{
		OnCancelButtonTimer.Invoke();
		_currentInputDelay = inputDelay;
		UpdateButtonTimer(0f);
	}
}
