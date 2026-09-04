using Invector.vCharacterController;
using UnityEngine;

public class vEnableDisableByInputDevice : MonoBehaviour
{
	public enum CheckMethod
	{
		Equals,
		Different
	}

	public InputDevice inputDevice;

	public CheckMethod methodToCheck;

	private void Start()
	{
		vInput.instance.onChangeInputType -= OnChangeInput;
		vInput.instance.onChangeInputType += OnChangeInput;
		OnChangeInput(vInput.instance.inputDevice);
	}

	public void OnChangeInput(InputDevice type)
	{
		bool flag = ((methodToCheck == CheckMethod.Different) ? (type != inputDevice) : (type == inputDevice));
		if (base.gameObject.activeSelf != flag)
		{
			base.gameObject.SetActive(flag);
		}
	}
}
