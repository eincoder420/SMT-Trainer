using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Crosstales.RTVoice.Demo;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_g_u_i_main.html")]
public class GUIMain : MonoBehaviour
{
	[Header("UI Objects")]
	public Text Name;

	public Text Version;

	public Text Scene;

	public GameObject NoVoices;

	public Text Errors;

	private void Start()
	{
		Singleton<Speaker>.Instance.OnVoicesReady += onVoicesReady;
		Singleton<Speaker>.Instance.OnErrorInfo += onErrorInfo;
		Singleton<Speaker>.Instance.OnSpeakStart += onSpeakStart;
		if (Name != null)
		{
			Name.text = "RT-Voice PRO";
		}
		if (Version != null)
		{
			Version.text = "2024.1.1";
		}
		if (Scene != null)
		{
			Scene.text = SceneManager.GetActiveScene().name;
		}
		if (NoVoices != null)
		{
			NoVoices.SetActive(Singleton<Speaker>.Instance.Voices.Count <= 0);
		}
		if (Errors != null)
		{
			Errors.gameObject.SetActive(value: false);
			Errors.text = string.Empty;
		}
	}

	private void Update()
	{
		Cursor.visible = true;
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnVoicesReady -= onVoicesReady;
			Singleton<Speaker>.Instance.OnErrorInfo -= onErrorInfo;
			Singleton<Speaker>.Instance.OnSpeakStart -= onSpeakStart;
		}
	}

	public void OpenAssetURL()
	{
		NetworkHelper.OpenURL("https://assetstore.unity.com/lists/crosstales-42213?aid=1011lNGT");
	}

	public void OpenCTURL()
	{
		NetworkHelper.OpenURL("https://www.crosstales.com");
	}

	public void Silence()
	{
		Singleton<Speaker>.Instance.Silence();
	}

	public void Quit()
	{
		if (!Application.isEditor)
		{
			Application.Quit();
		}
	}

	private void onVoicesReady()
	{
		if (NoVoices != null)
		{
			NoVoices.SetActive(Singleton<Speaker>.Instance.Voices.Count <= 0);
		}
		if (Errors != null)
		{
			Errors.gameObject.SetActive(value: false);
			Errors.text = string.Empty;
		}
	}

	private void onErrorInfo(Wrapper wrapper, string errorInfo)
	{
		if (Errors != null)
		{
			Errors.gameObject.SetActive(value: true);
			Errors.text = errorInfo;
		}
	}

	private void onSpeakStart(Wrapper wrapper)
	{
		if (Errors != null)
		{
			Errors.gameObject.SetActive(value: false);
			Errors.text = string.Empty;
		}
	}
}
