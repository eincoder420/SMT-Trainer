using UnityEngine;

public class Photo : MonoBehaviour
{
	private ScreenshotHandler Screen_System;

	public int id;

	private void Start()
	{
		Screen_System = Object.FindObjectOfType<ScreenshotHandler>();
	}

	public void Delete_Photo_File()
	{
		Screen_System.Delete_File(id);
	}
}
