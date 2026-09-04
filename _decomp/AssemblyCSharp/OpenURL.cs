using UnityEngine;

public class OpenURL : MonoBehaviour
{
	public void OpenVRARTISTURL(string URLName)
	{
		Application.OpenURL(URLName);
	}
}
