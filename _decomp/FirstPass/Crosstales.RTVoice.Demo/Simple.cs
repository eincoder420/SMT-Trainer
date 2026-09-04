using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using Crosstales.RTVoice.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.RTVoice.Demo;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_simple.html")]
public class Simple : MonoBehaviour
{
	[Header("Configuration")]
	public AudioSource SourceA;

	public AudioSource SourceB;

	[Range(0f, 3f)]
	public float RateSpeakerA = 1.25f;

	[Range(0f, 3f)]
	public float RateSpeakerB = 1.75f;

	public bool PlayOnStart;

	[Header("UI Objects")]
	public Text TextSpeakerA;

	public Text TextSpeakerB;

	public Text PhonemeSpeakerA;

	public Text PhonemeSpeakerB;

	public Text VisemeSpeakerA;

	public Text VisemeSpeakerB;

	private string uidSpeakerA;

	private string uidSpeakerB;

	private string textA = "Text A";

	private string textB = "Text B";

	private Wrapper currentWrapper;

	private bool silent = true;

	public void Start()
	{
		Singleton<Speaker>.Instance.OnSpeakAudioGenerationStart += speakAudioGenerationStartMethod;
		Singleton<Speaker>.Instance.OnSpeakAudioGenerationComplete += speakAudioGenerationCompleteMethod;
		Singleton<Speaker>.Instance.OnSpeakCurrentWord += speakCurrentWordMethod;
		Singleton<Speaker>.Instance.OnSpeakCurrentPhoneme += speakCurrentPhonemeMethod;
		Singleton<Speaker>.Instance.OnSpeakCurrentViseme += speakCurrentVisemeMethod;
		Singleton<Speaker>.Instance.OnSpeakStart += speakStartMethod;
		Singleton<Speaker>.Instance.OnSpeakComplete += speakCompleteMethod;
		if (TextSpeakerA != null)
		{
			textA = TextSpeakerA.text;
		}
		if (TextSpeakerB != null)
		{
			textB = TextSpeakerB.text;
		}
		if (PlayOnStart)
		{
			Play();
		}
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnSpeakAudioGenerationStart -= speakAudioGenerationStartMethod;
			Singleton<Speaker>.Instance.OnSpeakAudioGenerationComplete -= speakAudioGenerationCompleteMethod;
			Singleton<Speaker>.Instance.OnSpeakCurrentWord -= speakCurrentWordMethod;
			Singleton<Speaker>.Instance.OnSpeakCurrentPhoneme -= speakCurrentPhonemeMethod;
			Singleton<Speaker>.Instance.OnSpeakCurrentViseme -= speakCurrentVisemeMethod;
			Singleton<Speaker>.Instance.OnSpeakStart -= speakStartMethod;
			Singleton<Speaker>.Instance.OnSpeakComplete -= speakCompleteMethod;
		}
	}

	public void Play()
	{
		silent = false;
		if (TextSpeakerA != null)
		{
			TextSpeakerA.text = textA;
		}
		if (TextSpeakerB != null)
		{
			TextSpeakerB.text = textB;
		}
		SpeakerA();
	}

	public void SpeakerA()
	{
		uidSpeakerA = Singleton<Speaker>.Instance.Speak(textA, SourceA, Singleton<Speaker>.Instance.VoiceForGender(Gender.MALE, "en"), speakImmediately: false, RateSpeakerA);
	}

	public void SpeakerB()
	{
		uidSpeakerB = Singleton<Speaker>.Instance.Speak(textB, SourceB, Singleton<Speaker>.Instance.VoiceForGender(Gender.FEMALE, "en"), speakImmediately: false, RateSpeakerB);
	}

	public void Silence()
	{
		silent = true;
		Singleton<Speaker>.Instance.Silence();
		if (SourceA != null)
		{
			SourceA.Stop();
		}
		if (SourceB != null)
		{
			SourceB.Stop();
		}
		if (TextSpeakerA != null)
		{
			TextSpeakerA.text = textA;
		}
		if (TextSpeakerB != null)
		{
			TextSpeakerB.text = textB;
		}
		Text visemeSpeakerB = VisemeSpeakerB;
		Text phonemeSpeakerB = PhonemeSpeakerB;
		Text visemeSpeakerA = VisemeSpeakerA;
		string text2 = (PhonemeSpeakerA.text = "-");
		string text4 = (visemeSpeakerA.text = text2);
		string text6 = (phonemeSpeakerB.text = text4);
		visemeSpeakerB.text = text6;
	}

	private void speakAudio()
	{
		Singleton<Speaker>.Instance.SpeakMarkedWordsWithUID(currentWrapper);
	}

	private static void speakAudioGenerationStartMethod(Wrapper wrapper)
	{
		if (Config.DEBUG)
		{
			Debug.Log("speakAudioGenerationStartMethod: " + wrapper);
		}
	}

	private void speakAudioGenerationCompleteMethod(Wrapper wrapper)
	{
		if (Config.DEBUG)
		{
			Debug.Log("speakAudioGenerationCompleteMethod: " + wrapper);
		}
		if (wrapper.Uid.Equals(uidSpeakerA) || wrapper.Uid.Equals(uidSpeakerB))
		{
			currentWrapper = wrapper;
			Invoke("speakAudio", 0.1f);
		}
	}

	private void speakStartMethod(Wrapper wrapper)
	{
		if (wrapper.Uid.Equals(uidSpeakerA))
		{
			if (Config.DEBUG)
			{
				Debug.Log("Speaker A - Speech start: " + wrapper, this);
			}
		}
		else if (wrapper.Uid.Equals(uidSpeakerB) && Config.DEBUG)
		{
			Debug.Log("Speaker B - Speech start: " + wrapper, this);
		}
	}

	private void speakCompleteMethod(Wrapper wrapper)
	{
		if (wrapper.Uid.Equals(uidSpeakerA))
		{
			if (Config.DEBUG)
			{
				Debug.Log("Speaker A - Speech complete: " + wrapper, this);
			}
			if (TextSpeakerA != null)
			{
				TextSpeakerA.text = wrapper.Text;
			}
			if (VisemeSpeakerA != null)
			{
				Text visemeSpeakerA = VisemeSpeakerA;
				string text2 = (PhonemeSpeakerA.text = "-");
				visemeSpeakerA.text = text2;
			}
			if (!silent)
			{
				Invoke("SpeakerB", 0.1f);
			}
		}
		else if (wrapper.Uid.Equals(uidSpeakerB))
		{
			if (Config.DEBUG)
			{
				Debug.Log("Speaker B - Speech complete: " + wrapper, this);
			}
			if (TextSpeakerB != null)
			{
				TextSpeakerB.text = wrapper.Text;
			}
			if (VisemeSpeakerB != null)
			{
				Text visemeSpeakerB = VisemeSpeakerB;
				string text2 = (PhonemeSpeakerB.text = "-");
				visemeSpeakerB.text = text2;
			}
			if (!silent)
			{
				Invoke("SpeakerA", 0.1f);
			}
		}
	}

	private void speakCurrentWordMethod(Wrapper wrapper, string[] speechTextArray, int wordIndex)
	{
		if (wrapper.Uid.Equals(uidSpeakerA))
		{
			if (TextSpeakerA != null)
			{
				TextSpeakerA.text = Helper.MarkSpokenText(speechTextArray, wordIndex);
			}
		}
		else if (wrapper.Uid.Equals(uidSpeakerB) && TextSpeakerB != null)
		{
			TextSpeakerB.text = Helper.MarkSpokenText(speechTextArray, wordIndex);
		}
	}

	private void speakCurrentPhonemeMethod(Wrapper wrapper, string phoneme)
	{
		if (wrapper.Uid.Equals(uidSpeakerA))
		{
			if (PhonemeSpeakerA != null)
			{
				PhonemeSpeakerA.text = phoneme;
			}
		}
		else if (wrapper.Uid.Equals(uidSpeakerB) && PhonemeSpeakerB != null)
		{
			PhonemeSpeakerB.text = phoneme;
		}
	}

	private void speakCurrentVisemeMethod(Wrapper wrapper, string viseme)
	{
		if (wrapper.Uid.Equals(uidSpeakerA))
		{
			if (VisemeSpeakerA != null)
			{
				VisemeSpeakerA.text = viseme;
			}
		}
		else if (wrapper.Uid.Equals(uidSpeakerB) && VisemeSpeakerB != null)
		{
			VisemeSpeakerB.text = viseme;
		}
	}
}
