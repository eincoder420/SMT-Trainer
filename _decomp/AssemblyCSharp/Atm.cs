using UnityEngine;

public class Atm : MonoBehaviour
{
	private PauseMenuScript interface_script;

	public void Show_Money_Window()
	{
		if (!interface_script)
		{
			interface_script = Object.FindObjectOfType<PauseMenuScript>();
		}
		interface_script.Rox_Interface.Atm_Money_Window.gameObject.SetActive(!interface_script.Rox_Interface.Atm_Money_Window.gameObject.activeInHierarchy);
	}

	public void Hide_Money_Window()
	{
		if (!interface_script)
		{
			interface_script = Object.FindObjectOfType<PauseMenuScript>();
		}
		interface_script.Rox_Interface.Atm_Money_Window.gameObject.SetActive(value: false);
	}
}
