using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using UnityEngine;

namespace Crosstales.RTVoice.Demo;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_pre_generated_audio.html")]
public class PreGeneratedAudio : MonoBehaviour
{
	public string SpeechText = "This is an example with pre-generated audio for exact timing (e.g. animations).";

	public bool PlayOnStart;

	private AudioSource audioSource;

	private bool isPlayed;

	private string uid;

	private Wrapper wrapper;

	private void Start()
	{
		Singleton<Speaker>.Instance.OnSpeakAudioGenerationComplete += speakAudioGenerationCompleteMethod;
		audioSource = base.gameObject.AddComponent<AudioSource>();
		uid = Singleton<Speaker>.Instance.Speak(SpeechText, audioSource, Singleton<Speaker>.Instance.VoiceForCulture("en", 1), speakImmediately: false);
	}

	private void Update()
	{
		if (!audioSource.CTHasActiveClip() && isPlayed)
		{
			Stop();
		}
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnSpeakAudioGenerationComplete -= speakAudioGenerationCompleteMethod;
		}
	}

	public void Play()
	{
		Debug.Log("Play your animations: " + wrapper, this);
		isPlayed = true;
		audioSource.Play();
	}

	public void Silence()
	{
		audioSource.Stop();
	}

	public void Stop()
	{
		Debug.Log("Stop your animations: " + wrapper, this);
		isPlayed = false;
	}

	private void speakAudioGenerationCompleteMethod(Wrapper wrapper)
	{
		if (uid == wrapper.Uid)
		{
			this.wrapper = wrapper;
			if (PlayOnStart)
			{
				Play();
			}
		}
	}
}
