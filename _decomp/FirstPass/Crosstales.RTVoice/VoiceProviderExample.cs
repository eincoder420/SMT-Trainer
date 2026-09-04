using System.Collections;
using System.Collections.Generic;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using Crosstales.RTVoice.Provider;
using UnityEngine;

namespace Crosstales.RTVoice;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_voice_provider_example.html")]
public class VoiceProviderExample : BaseCustomVoiceProvider
{
	public override string AudioFileExtension => ".wav";

	public override AudioType AudioFileType => AudioType.WAV;

	public override string DefaultVoiceName => "Marisa";

	public override bool isWorkingInEditor => true;

	public override bool isWorkingInPlaymode => true;

	public override bool isPlatformSupported => true;

	public override int MaxTextLength => 32000;

	public override bool isSpeakNativeSupported => false;

	public override bool isSpeakSupported => true;

	public override bool isSSMLSupported => true;

	public override bool isOnlineService => false;

	public override bool hasCoRoutines => true;

	public override bool isIL2CPPSupported => true;

	public override bool hasVoicesInEditor => true;

	public override int MaxSimultaneousSpeeches => 0;

	public override void Load(bool forceReload = false)
	{
		Debug.Log("Load", this);
		List<Voice> list = cachedVoices;
		if ((list != null && list.Count == 0) || forceReload)
		{
			Debug.Log("Reload all voices", this);
			cachedVoices = new List<Voice>
			{
				new Voice("Marisa", "RTV custom provider test -> female", Gender.FEMALE, "adult", "en-US"),
				new Voice("Stefan", "RTV custom provider test -> male", Gender.MALE, "adult", "en-US")
			};
		}
		onVoicesReady();
	}

	public override IEnumerator Generate(Wrapper wrapper)
	{
		Debug.Log("Generate: " + wrapper, this);
		if (wrapper == null)
		{
			Debug.LogWarning("'wrapper' is null!", this);
			yield break;
		}
		if (string.IsNullOrEmpty(wrapper.Text))
		{
			Debug.LogWarning("'wrapper.Text' is null or empty!", this);
			yield break;
		}
		yield return null;
		silence = false;
		onSpeakAudioGenerationStart(wrapper);
		string text = wrapper.Text;
		for (int i = 0; i < text.Length; i++)
		{
			Debug.Log(text[i], this);
			yield return null;
		}
		onSpeakAudioGenerationComplete(wrapper);
	}

	public override IEnumerator Speak(Wrapper wrapper)
	{
		Debug.Log("Speak: " + wrapper, this);
		if (wrapper == null)
		{
			Debug.LogWarning("'wrapper' is null!", this);
			yield break;
		}
		if (string.IsNullOrEmpty(wrapper.Text))
		{
			Debug.LogWarning("'wrapper.Text' is null or empty!", this);
			yield break;
		}
		yield return null;
		silence = false;
		onSpeakAudioGenerationStart(wrapper);
		onSpeakAudioGenerationComplete(wrapper);
		if (wrapper.SpeakImmediately)
		{
			onSpeakStart(wrapper);
			string text = wrapper.Text;
			for (int i = 0; i < text.Length; i++)
			{
				Debug.Log(text[i], this);
				yield return null;
			}
			onSpeakComplete(wrapper);
		}
	}

	public override IEnumerator SpeakNative(Wrapper wrapper)
	{
		Debug.Log("SpeakNative: " + wrapper, this);
		if (wrapper == null)
		{
			Debug.LogWarning("'wrapper' is null!", this);
			yield break;
		}
		if (string.IsNullOrEmpty(wrapper.Text))
		{
			Debug.LogWarning("'wrapper.Text' is null or empty!", this);
			yield break;
		}
		yield return null;
		silence = false;
		onSpeakStart(wrapper);
		string text = wrapper.Text;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			onSpeakCurrentPhoneme(wrapper, c.ToString());
			onSpeakCurrentViseme(wrapper, c.ToString());
			Debug.Log(c, this);
			yield return null;
		}
		onSpeakComplete(wrapper);
	}
}
