using Crosstales.Common.Util;
using Crosstales.RTVoice;
using Crosstales.RTVoice.Model;
using UnityEngine;

public class SimpleRTVoiceExample : MonoBehaviour
{
	public string Text = "Hello world, I am RT-Voice!";

	public string Culture = "en";

	public bool UseDefaultVoice;

	public bool SpeakWhenReady;

	public AudioSource Audio;

	public bool UseNative;

	private string uid;

	private void OnEnable()
	{
		Singleton<Speaker>.Instance.OnVoicesReady += voicesReady;
		Singleton<Speaker>.Instance.OnSpeakStart += speakStart;
		Singleton<Speaker>.Instance.OnSpeakComplete += speakComplete;
	}

	private void OnDisable()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnVoicesReady -= voicesReady;
			Singleton<Speaker>.Instance.OnSpeakStart -= speakStart;
			Singleton<Speaker>.Instance.OnSpeakComplete -= speakComplete;
		}
	}

	public void Speak()
	{
		if (UseNative)
		{
			uid = Singleton<Speaker>.Instance.SpeakNative(Text, UseDefaultVoice ? null : Singleton<Speaker>.Instance.VoiceForCulture(Culture));
		}
		else
		{
			uid = Singleton<Speaker>.Instance.Speak(Text, Audio, UseDefaultVoice ? null : Singleton<Speaker>.Instance.VoiceForCulture(Culture));
		}
	}

	private void voicesReady()
	{
		Debug.Log($"RT-Voice: {Singleton<Speaker>.Instance.Voices.Count} voices are ready to use!");
		if (SpeakWhenReady)
		{
			Speak();
		}
	}

	private void speakStart(Wrapper wrapper)
	{
		if (wrapper.Uid == uid)
		{
			Debug.Log($"RT-Voice: speak started: {wrapper}");
		}
	}

	private void speakComplete(Wrapper wrapper)
	{
		if (wrapper.Uid == uid)
		{
			Debug.Log($"RT-Voice: speak completed: {wrapper}");
		}
	}
}
