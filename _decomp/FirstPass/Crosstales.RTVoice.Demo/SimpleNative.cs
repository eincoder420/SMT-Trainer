using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using Crosstales.RTVoice.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.RTVoice.Demo;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_simple_native.html")]
public class SimpleNative : MonoBehaviour
{
	[Header("Configuration")]
	[Range(0f, 3f)]
	public float RateSpeakerA = 1.25f;

	[Range(0f, 3f)]
	public float RateSpeakerB = 1.75f;

	[Range(0f, 3f)]
	public float RateSpeakerC = 2.5f;

	public bool PlayOnStart;

	[Header("UI Objects")]
	public Text TextSpeakerA;

	public Text TextSpeakerB;

	public Text TextSpeakerC;

	public Text PhonemeSpeakerA;

	public Text PhonemeSpeakerB;

	public Text PhonemeSpeakerC;

	public Text VisemeSpeakerA;

	public Text VisemeSpeakerB;

	public Text VisemeSpeakerC;

	private string uidSpeakerA;

	private string uidSpeakerB;

	private string uidSpeakerC;

	private string textA = "Text A";

	private string textB = "Text B";

	private string textC = "Text C";

	private bool silent = true;

	private void Start()
	{
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
		if (TextSpeakerC != null)
		{
			textC = TextSpeakerC.text;
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
		if (TextSpeakerC != null)
		{
			TextSpeakerC.text = textC;
		}
		SpeakerA();
	}

	public void SpeakerA()
	{
		uidSpeakerA = Singleton<Speaker>.Instance.SpeakNative(textA, Singleton<Speaker>.Instance.VoiceForGender(Gender.MALE, "en"), RateSpeakerA);
	}

	public void SpeakerB()
	{
		uidSpeakerB = Singleton<Speaker>.Instance.SpeakNative(textB, Singleton<Speaker>.Instance.VoiceForGender(Gender.FEMALE, "en"), RateSpeakerB);
	}

	public void SpeakerC()
	{
		uidSpeakerC = Singleton<Speaker>.Instance.SpeakNative(textC, Singleton<Speaker>.Instance.VoiceForGender(Gender.MALE, "en", 1), RateSpeakerC);
	}

	public void Silence()
	{
		silent = true;
		Singleton<Speaker>.Instance.Silence();
		if (TextSpeakerA != null)
		{
			TextSpeakerA.text = textA;
		}
		if (TextSpeakerB != null)
		{
			TextSpeakerB.text = textB;
		}
		if (TextSpeakerC != null)
		{
			TextSpeakerC.text = textC;
		}
		Text visemeSpeakerC = VisemeSpeakerC;
		Text phonemeSpeakerC = PhonemeSpeakerC;
		Text visemeSpeakerB = VisemeSpeakerB;
		Text phonemeSpeakerB = PhonemeSpeakerB;
		Text visemeSpeakerA = VisemeSpeakerA;
		string text2 = (PhonemeSpeakerA.text = "-");
		string text4 = (visemeSpeakerA.text = text2);
		string text6 = (phonemeSpeakerB.text = text4);
		string text8 = (visemeSpeakerB.text = text6);
		string text10 = (phonemeSpeakerC.text = text8);
		visemeSpeakerC.text = text10;
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
		else if (wrapper.Uid.Equals(uidSpeakerB))
		{
			if (Config.DEBUG)
			{
				Debug.Log("Speaker B - Speech start: " + wrapper, this);
			}
		}
		else if (wrapper.Uid.Equals(uidSpeakerC) && Config.DEBUG)
		{
			Debug.Log("Speaker C - Speech start: " + wrapper, this);
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
				SpeakerB();
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
				SpeakerC();
			}
		}
		else if (wrapper.Uid.Equals(uidSpeakerC))
		{
			if (Config.DEBUG)
			{
				Debug.Log("Speaker C - Speech complete: " + wrapper, this);
			}
			if (TextSpeakerC != null)
			{
				TextSpeakerC.text = wrapper.Text;
			}
			if (VisemeSpeakerC != null)
			{
				Text visemeSpeakerC = VisemeSpeakerC;
				string text2 = (PhonemeSpeakerC.text = "-");
				visemeSpeakerC.text = text2;
			}
			if (!silent)
			{
				SpeakerA();
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
		else if (wrapper.Uid.Equals(uidSpeakerB))
		{
			if (TextSpeakerB != null)
			{
				TextSpeakerB.text = Helper.MarkSpokenText(speechTextArray, wordIndex);
			}
		}
		else if (wrapper.Uid.Equals(uidSpeakerC) && TextSpeakerC != null)
		{
			TextSpeakerC.text = Helper.MarkSpokenText(speechTextArray, wordIndex);
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
		else if (wrapper.Uid.Equals(uidSpeakerB))
		{
			if (PhonemeSpeakerB != null)
			{
				PhonemeSpeakerB.text = phoneme;
			}
		}
		else if (wrapper.Uid.Equals(uidSpeakerC) && PhonemeSpeakerC != null)
		{
			PhonemeSpeakerC.text = phoneme;
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
		else if (wrapper.Uid.Equals(uidSpeakerB))
		{
			if (VisemeSpeakerB != null)
			{
				VisemeSpeakerB.text = viseme;
			}
		}
		else if (wrapper.Uid.Equals(uidSpeakerC) && VisemeSpeakerC != null)
		{
			VisemeSpeakerC.text = viseme;
		}
	}
}
