using UnityEngine;

public class MenuOptions : MonoBehaviour
{
	public Transform Window;

	private Start_Menu menu;

	private void Start()
	{
		menu = Object.FindObjectOfType<Start_Menu>();
	}

	public void OnClick()
	{
		menu.Close_All_Except(Window);
	}
}
