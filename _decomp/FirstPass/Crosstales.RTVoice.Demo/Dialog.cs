using System.Collections;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using Crosstales.RTVoice.Util;
using UnityEngine;

namespace Crosstales.RTVoice.Demo;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_dialog.html")]
public class Dialog : MonoBehaviour
{
	[Header("Configuration")]
	public string CultureA = "en";

	public string CultureB = "en";

	[Range(0f, 3f)]
	public float RateA = 1f;

	[Range(0f, 3f)]
	public float RateB = 1f;

	[Range(0f, 2f)]
	public float PitchA = 1f;

	[Range(0f, 2f)]
	public float PitchB = 1f;

	[Range(0f, 1f)]
	public float VolumeA = 1f;

	[Range(0f, 1f)]
	public float VolumeB = 1f;

	public Gender GenderA = Gender.UNKNOWN;

	public Gender GenderB = Gender.UNKNOWN;

	public AudioSource AudioPersonA;

	public AudioSource AudioPersonB;

	public SpeakMode ModeA;

	public SpeakMode ModeB;

	[Header("Dialogues")]
	public string[] DialogPersonA;

	public string[] DialogPersonB;

	public string CurrentDialogA = string.Empty;

	public string CurrentDialogB = string.Empty;

	public bool Running;

	private string uidSpeakerA;

	private string uidSpeakerB;

	private bool playingA;

	private bool playingB;

	private void Start()
	{
		Singleton<Speaker>.Instance.OnSpeakStart += speakStartMethod;
		Singleton<Speaker>.Instance.OnSpeakComplete += speakCompleteMethod;
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnSpeakStart -= speakStartMethod;
			Singleton<Speaker>.Instance.OnSpeakComplete -= speakCompleteMethod;
		}
	}

	public IEnumerator DialogSequence()
	{
		if (Running)
		{
			yield break;
		}
		Running = true;
		playingA = false;
		playingB = false;
		for (int index = 0; (Running && index < DialogPersonA?.Length) || index < DialogPersonB?.Length; index++)
		{
			yield return null;
			if (index < DialogPersonA?.Length)
			{
				CurrentDialogA = DialogPersonA[index];
			}
			uidSpeakerA = ((ModeA == SpeakMode.Speak) ? Singleton<Speaker>.Instance.Speak(CurrentDialogA, AudioPersonA, Singleton<Speaker>.Instance.VoiceForGender(GenderA, CultureA), speakImmediately: true, RateA, PitchA, VolumeA) : Singleton<Speaker>.Instance.SpeakNative(CurrentDialogA, Singleton<Speaker>.Instance.VoiceForGender(GenderA, CultureA), RateA, PitchA, VolumeA));
			do
			{
				yield return null;
			}
			while (!playingA && Running);
			do
			{
				yield return null;
			}
			while (playingA && Running);
			CurrentDialogA = string.Empty;
			yield return null;
			if (Running)
			{
				if (index < DialogPersonB?.Length)
				{
					CurrentDialogB = DialogPersonB[index];
				}
				uidSpeakerB = ((ModeB == SpeakMode.Speak) ? Singleton<Speaker>.Instance.Speak(CurrentDialogB, AudioPersonB, Singleton<Speaker>.Instance.VoiceForGender(GenderB, CultureB, 1), speakImmediately: true, RateB, PitchB, VolumeB) : Singleton<Speaker>.Instance.SpeakNative(CurrentDialogB, Singleton<Speaker>.Instance.VoiceForGender(GenderB, CultureB, 1), RateB, PitchB, VolumeB));
				do
				{
					yield return null;
				}
				while (!playingB && Running);
				do
				{
					yield return null;
				}
				while (playingB && Running);
				CurrentDialogB = string.Empty;
			}
		}
		Running = false;
	}

	private void speakStartMethod(Wrapper wrapper)
	{
		if (wrapper.Uid.Equals(uidSpeakerA))
		{
			if (Config.DEBUG)
			{
				Debug.Log("speakStartMethod - Speaker A: " + wrapper, this);
			}
			playingA = true;
		}
		else if (wrapper.Uid.Equals(uidSpeakerB))
		{
			if (Config.DEBUG)
			{
				Debug.Log("speakStartMethod - Speaker B: " + wrapper, this);
			}
			playingB = true;
		}
	}

	private void speakCompleteMethod(Wrapper wrapper)
	{
		if (wrapper.Uid.Equals(uidSpeakerA))
		{
			if (Config.DEBUG)
			{
				Debug.Log("speakCompleteMethod - Speaker A: " + wrapper, this);
			}
			playingA = false;
		}
		else if (wrapper.Uid.Equals(uidSpeakerB))
		{
			if (Config.DEBUG)
			{
				Debug.Log("speakCompleteMethod - Speaker B: " + wrapper, this);
			}
			playingB = false;
		}
	}
}
