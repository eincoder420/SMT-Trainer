using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.RTVoice.Demo;

[RequireComponent(typeof(AudioSource))]
[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_speak_wrapper.html")]
public class SpeakWrapper : MonoBehaviour
{
	public Voice SpeakerVoice;

	public InputField Input;

	public Text Label;

	public AudioSource Audio;

	private string uid = string.Empty;

	private void Start()
	{
		Audio = GetComponent<AudioSource>();
	}

	public void Speak()
	{
		if (!string.IsNullOrEmpty(uid))
		{
			Singleton<Speaker>.Instance.Silence(uid);
		}
		uid = (GUISpeech.isNative ? Singleton<Speaker>.Instance.SpeakNative(Input.text, SpeakerVoice, GUISpeech.Rate, GUISpeech.Pitch, GUISpeech.Volume) : Singleton<Speaker>.Instance.Speak(Input.text, Audio, SpeakerVoice, speakImmediately: true, GUISpeech.Rate, GUISpeech.Pitch, GUISpeech.Volume));
	}
}
