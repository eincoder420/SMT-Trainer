using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using UnityEngine;

namespace Crosstales.RTVoice.Demo;

[ExecuteInEditMode]
[HelpURL("https://crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_event_tester.html")]
public class EventTester : MonoBehaviour
{
	public bool ShowUnityEvents = true;

	public bool ShowCSharpEvents;

	private void Start()
	{
		Singleton<Speaker>.Instance.OnVoicesReady += onVoicesReady;
		Singleton<Speaker>.Instance.OnSpeakStart += onSpeakStart;
		Singleton<Speaker>.Instance.OnSpeakComplete += onSpeakComplete;
		Singleton<Speaker>.Instance.OnProviderChange += onProviderChange;
		Singleton<Speaker>.Instance.OnErrorInfo += onErrorInfo;
	}

	private void OnDestroy()
	{
		Singleton<Speaker>.Instance.OnVoicesReady -= onVoicesReady;
		Singleton<Speaker>.Instance.OnSpeakStart -= onSpeakStart;
		Singleton<Speaker>.Instance.OnSpeakComplete -= onSpeakComplete;
		Singleton<Speaker>.Instance.OnProviderChange -= onProviderChange;
		Singleton<Speaker>.Instance.OnErrorInfo -= onErrorInfo;
	}

	public void OnReady()
	{
		if (ShowUnityEvents)
		{
			Debug.Log("OnReady");
		}
	}

	public void OnSpeakStarted(string uid)
	{
		if (ShowUnityEvents)
		{
			Debug.Log("OnSpeakStarted: " + uid);
		}
	}

	public void OnSpeakCompleted(string uid)
	{
		if (ShowUnityEvents)
		{
			Debug.Log("OnSpeakCompleted: " + uid);
		}
	}

	public void OnProviderChanged(string provider)
	{
		if (ShowUnityEvents)
		{
			Debug.Log("OnProviderChanged: " + provider);
		}
	}

	public void OnError(string uid, string info)
	{
		if (ShowUnityEvents)
		{
			Debug.LogWarning("OnError: " + uid + " - " + info);
		}
	}

	public void AudioFileGeneratorStarted()
	{
		Debug.Log("AudioFileGeneratorStarted");
	}

	public void AudioFileGeneratorCompleted()
	{
		Debug.Log("AudioFileGeneratorCompleted");
	}

	public void ParalanguageStarted()
	{
		Debug.Log("ParalanguageStarted");
	}

	public void ParalanguageCompleted()
	{
		Debug.Log("ParalanguageCompleted");
	}

	public void SpeechTextStarted()
	{
		Debug.Log("SpeechTextStarted");
	}

	public void SpeechTextCompleted()
	{
		Debug.Log("SpeechTextCompleted");
	}

	public void TextFileSpeakerStarted()
	{
		Debug.Log("TextFileSpeakerStarted");
	}

	public void TextFileSpeakerCompleted()
	{
		Debug.Log("TextFileSpeakerCompleted");
	}

	private void onVoicesReady()
	{
		if (ShowCSharpEvents)
		{
			Debug.Log("C# - OnVoicesReady");
		}
	}

	private void onSpeakStart(Wrapper wrapper)
	{
		if (ShowCSharpEvents)
		{
			Debug.Log("C# - OnSpeakStart: " + wrapper);
		}
	}

	private void onSpeakComplete(Wrapper wrapper)
	{
		if (ShowCSharpEvents)
		{
			Debug.Log("C# - onSpeakComplete: " + wrapper);
		}
	}

	public void onProviderChange(string provider)
	{
		if (ShowCSharpEvents)
		{
			Debug.Log("C# - OnProviderChange: " + provider);
		}
	}

	private void onErrorInfo(Wrapper wrapper, string info)
	{
		if (ShowCSharpEvents)
		{
			Debug.LogWarning(string.Concat("C# - OnErrorInfo: ", wrapper, " - ", info));
		}
	}
}
