using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ScreenshotHandler : MonoBehaviour
{
	private Roxanne_Control roxanne_Control;

	private Start_Menu menu;

	public string FileName;

	public Game_Data data;

	public RenderTexture RT;

	public Transform Photos_Folder;

	public GameObject Photo_Prefab;

	public Transform Gallery_Object;

	public RawImage[] Raws;

	public AudioSource photo_audio;

	public AudioClip Photo_sound;

	public Camera cam;

	public bool IsTransparent;

	public bool OpenFileDirecoty = true;

	private TextureFormat transp = TextureFormat.ARGB32;

	private TextureFormat nonTransp = TextureFormat.RGB24;

	public Resolution[] Resolutions;

	private string main_path;

	private void Start()
	{
		menu = UnityEngine.Object.FindObjectOfType<Start_Menu>();
		Gallery_Object.gameObject.SetActive(value: false);
		main_path = Application.streamingAssetsPath + "/../StreamingAssets/Photos/";
		if (!Directory.Exists(main_path))
		{
			Directory.CreateDirectory(main_path);
		}
		data.Photo_id = new DirectoryInfo(main_path).GetFiles().Length;
		Raws = new RawImage[data.Photo_id];
		if (!menu.Menu)
		{
			roxanne_Control = UnityEngine.Object.FindObjectOfType<Roxanne_Control>();
		}
	}

	public void Turn_Gallery()
	{
		setImage();
		Gallery_Object.gameObject.SetActive(!Gallery_Object.gameObject.activeInHierarchy);
	}

	public void Delete_File(int id)
	{
		string path = main_path + FileName + id + ".png";
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public void setImage()
	{
		for (int i = 0; i < data.Photo_id; i++)
		{
			Texture2D texture2D = new Texture2D(RT.width, RT.height);
			string path = main_path + FileName + i + ".png";
			if (File.Exists(path) && !Raws[i])
			{
				byte[] array = File.ReadAllBytes(path);
				texture2D.LoadImage(array);
				texture2D.Apply();
				GameObject gameObject = UnityEngine.Object.Instantiate(Photo_Prefab, Photos_Folder);
				gameObject.GetComponent<Photo>().id = i;
				Raws[i] = gameObject.GetComponent<RawImage>();
				gameObject.GetComponent<RawImage>().texture = texture2D;
			}
		}
	}

	private void LateUpdate()
	{
		if (!roxanne_Control || !Input.GetMouseButtonDown(2) || !roxanne_Control.Photo_Mode)
		{
			return;
		}
		photo_audio.PlayOneShot(Photo_sound);
		if (Resolutions.Length == 0)
		{
			UnityEngine.Debug.LogWarning("no resolution found !");
			return;
		}
		for (int i = 0; i < Resolutions.Length; i++)
		{
			if (Resolutions[i].Width == 0 || Resolutions[i].Height == 0)
			{
				UnityEngine.Debug.LogWarning("Resolution can't be 0 !");
				return;
			}
			Capture(Resolutions[i].Width, Resolutions[i].Height, 1);
		}
		data.Photo_id++;
		Raws = new RawImage[data.Photo_id];
		RawImage[] componentsInChildren = Photos_Folder.GetComponentsInChildren<RawImage>();
		for (int j = 0; j < Raws.Length; j++)
		{
			if (j < componentsInChildren.Length)
			{
				Raws[j] = componentsInChildren[j];
			}
		}
		setImage();
		if (!roxanne_Control.Photo_Making)
		{
			roxanne_Control.Remain_Photo_Counter_Time = 2f;
			roxanne_Control.Photo_Making = true;
		}
		roxanne_Control.mission_Explorer.Photo_Task_Start();
		roxanne_Control.mission_Explorer.Complete_Photo_Mission();
	}

	public void Open_Photo_Folder()
	{
		if (!Directory.Exists(main_path))
		{
			Directory.CreateDirectory(main_path);
		}
		Process.Start(main_path);
	}

	private void Capture(int width, int height, int enlargeCOEF)
	{
		TextureFormat textureFormat = nonTransp;
		if (IsTransparent)
		{
			textureFormat = transp;
		}
		RenderTexture renderTexture = new RenderTexture(width * enlargeCOEF, height * enlargeCOEF, 24);
		cam.targetTexture = renderTexture;
		Texture2D texture2D = new Texture2D(width * enlargeCOEF, height * enlargeCOEF, textureFormat, mipChain: false);
		cam.Render();
		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, width * enlargeCOEF, height * enlargeCOEF), 0, 0);
		cam.targetTexture = null;
		RenderTexture.active = null;
		UnityEngine.Object.Destroy(renderTexture);
		byte[] bytes = texture2D.EncodeToPNG();
		string path = ScreenshotName(FileName, (width * enlargeCOEF).ToString(), (height * enlargeCOEF).ToString());
		if (!Directory.Exists(main_path))
		{
			Directory.CreateDirectory(main_path);
		}
		File.WriteAllBytes(path, bytes);
		if (OpenFileDirecoty)
		{
			Process.Start(main_path);
		}
	}

	private string ScreenshotName(string name, string width, string height)
	{
		return string.Format("{0}/../StreamingAssets/Photos/" + name + data.Photo_id + ".png", Application.streamingAssetsPath, width, height, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}
}
