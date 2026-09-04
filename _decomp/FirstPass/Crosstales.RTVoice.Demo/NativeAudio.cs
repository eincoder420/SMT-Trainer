using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using UnityEngine;

namespace Crosstales.RTVoice.Demo;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_native_audio.html")]
public class NativeAudio : MonoBehaviour
{
	public string SpeechText = "This is an example with native audio for exact timing (e.g. animations).";

	public bool PlayOnStart;

	public float Delay = 1f;

	private void Start()
	{
		Singleton<Speaker>.Instance.OnSpeakStart += play;
		Singleton<Speaker>.Instance.OnSpeakComplete += stop;
		if (PlayOnStart)
		{
			Invoke("StartTTS", Delay);
		}
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnSpeakStart -= play;
			Singleton<Speaker>.Instance.OnSpeakComplete -= stop;
		}
	}

	public void StartTTS()
	{
		Singleton<Speaker>.Instance.SpeakNative(SpeechText, Singleton<Speaker>.Instance.VoiceForCulture("en", 1));
	}

	public void Silence()
	{
		Singleton<Speaker>.Instance.Silence();
	}

	private void play(Wrapper wrapper)
	{
		Debug.Log("Play your animations to the event: " + wrapper, this);
	}

	private void stop(Wrapper wrapper)
	{
		Debug.Log("Stop your animations from the event: " + wrapper, this);
	}
}
