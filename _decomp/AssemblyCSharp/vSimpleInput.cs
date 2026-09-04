using Invector;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;

[vClassHeader("Simple Input", true, "icon_v2", false, "", openClose = false)]
public class vSimpleInput : vMonoBehaviour
{
	[Tooltip("Input to press")]
	public GenericInput input = new GenericInput("Escape", "B", "B");

	[Tooltip("This Gameobject will turn off after the input is pressed")]
	public bool disableThisObjectAfterInput = true;

	public UnityEvent OnPressInput;

	private void Update()
	{
		if (input.GetButtonDown() && base.gameObject.activeSelf)
		{
			if (disableThisObjectAfterInput)
			{
				base.gameObject.SetActive(value: false);
			}
			OnPressInput.Invoke();
		}
	}
}
