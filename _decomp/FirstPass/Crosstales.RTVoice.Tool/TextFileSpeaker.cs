using System;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using Crosstales.RTVoice.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crosstales.RTVoice.Tool;

[ExecuteInEditMode]
[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_tool_1_1_text_file_speaker.html")]
public class TextFileSpeaker : MonoBehaviour
{
	[Header("Configuration")]
	[FormerlySerializedAs("TextFiles")]
	[Tooltip("Text files to speak.")]
	[SerializeField]
	private TextAsset[] textFiles;

	[FormerlySerializedAs("Voices")]
	[Tooltip("Voices for the speech.")]
	[SerializeField]
	private VoiceAlias voices;

	[FormerlySerializedAs("Mode")]
	[Tooltip("Speak mode (default: 'Speak').")]
	[SerializeField]
	private SpeakMode mode;

	[FormerlySerializedAs("Source")]
	[Header("Optional Settings")]
	[Tooltip("AudioSource for the output (optional).")]
	[SerializeField]
	private AudioSource source;

	[FormerlySerializedAs("Rate")]
	[Tooltip("Speech rate of the speaker in percent (1 = 100%, default: 1, optional).")]
	[Range(0f, 3f)]
	[SerializeField]
	private float rate = 1f;

	[FormerlySerializedAs("Pitch")]
	[Tooltip("Speech pitch of the speaker in percent (1 = 100%, default: 1, optional, mobile only).")]
	[Range(0f, 2f)]
	[SerializeField]
	private float pitch = 1f;

	[FormerlySerializedAs("Volume")]
	[Tooltip("Volume of the speaker in percent (1 = 100%, default: 1, optional, Windows only).")]
	[Range(0f, 1f)]
	[SerializeField]
	private float volume = 1f;

	[FormerlySerializedAs("PlayOnStart")]
	[Header("Behaviour Settings")]
	[Tooltip("Enable speaking of a random text file on start (default: false).")]
	[SerializeField]
	private bool playOnStart;

	[FormerlySerializedAs("PlayAllOnStart")]
	[Tooltip("Enable speaking of a random text file on start (default: false).")]
	[SerializeField]
	private bool playAllOnStart;

	[FormerlySerializedAs("SpeakRandom")]
	[Tooltip("Speaks the text files in random order (default: false).")]
	[SerializeField]
	private bool speakRandom;

	[FormerlySerializedAs("Delay")]
	[Tooltip("Delay in seconds until the speech for this text starts (default: 0.1).")]
	[SerializeField]
	private float delay = 0.1f;

	private string[] texts;

	private string[] randomTexts;

	private int textIndex = -1;

	private int randomTextIndex = -1;

	private static readonly System.Random rnd = new System.Random();

	private string uid = string.Empty;

	private bool played;

	private bool playAll;

	private float lastSpeaktime = float.MinValue;

	private int lastNumberOfTextfiles = -1;

	[Header("Events")]
	public TextFileSpeakerStartEvent OnStarted;

	public TextFileSpeakerCompleteEvent OnCompleted;

	public TextAsset[] TextFiles
	{
		get
		{
			return textFiles;
		}
		set
		{
			textFiles = value;
		}
	}

	public VoiceAlias Voices
	{
		get
		{
			return voices;
		}
		set
		{
			voices = value;
		}
	}

	public SpeakMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			mode = value;
		}
	}

	public AudioSource Source
	{
		get
		{
			return source;
		}
		set
		{
			source = value;
		}
	}

	public float Rate
	{
		get
		{
			return rate;
		}
		set
		{
			rate = Mathf.Clamp(value, 0f, 3f);
		}
	}

	public float Pitch
	{
		get
		{
			return pitch;
		}
		set
		{
			pitch = Mathf.Clamp(value, 0f, 2f);
		}
	}

	public float Volume
	{
		get
		{
			return volume;
		}
		set
		{
			volume = Mathf.Clamp01(value);
		}
	}

	public bool PlayOnStart
	{
		get
		{
			return playOnStart;
		}
		set
		{
			playOnStart = value;
		}
	}

	public bool PlayAllOnStart
	{
		get
		{
			return playAllOnStart;
		}
		set
		{
			playAllOnStart = value;
		}
	}

	public bool SpeakRandom
	{
		get
		{
			return speakRandom;
		}
		set
		{
			speakRandom = value;
		}
	}

	public float Delay
	{
		get
		{
			return delay;
		}
		set
		{
			delay = Mathf.Abs(value);
		}
	}

	public event TextFileSpeakerStart OnTextFileSpeakerStart;

	public event TextFileSpeakerComplete OnTextFileSpeakerComplete;

	private void Start()
	{
		Singleton<Speaker>.Instance.OnVoicesReady += onVoicesReady;
		Singleton<Speaker>.Instance.OnSpeakStart += onSpeakStart;
		Singleton<Speaker>.Instance.OnSpeakComplete += onSpeakComplete;
		Reload();
		play();
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnVoicesReady -= onVoicesReady;
			Singleton<Speaker>.Instance.OnSpeakStart -= onSpeakStart;
			Singleton<Speaker>.Instance.OnSpeakComplete -= onSpeakComplete;
		}
	}

	private void Update()
	{
		if (textFiles.Length != lastNumberOfTextfiles)
		{
			Reload();
		}
	}

	private void OnValidate()
	{
		if (delay < 0f)
		{
			delay = 0f;
		}
		rate = Mathf.Clamp(rate, 0f, 3f);
		pitch = Mathf.Clamp(pitch, 0f, 2f);
		volume = Mathf.Clamp01(volume);
	}

	public void SpeakAll()
	{
		playAll = true;
		Next();
	}

	public void StopAll()
	{
		playAll = false;
		Silence();
	}

	public void Next()
	{
		Next(speakRandom);
	}

	public void Next(bool random)
	{
		int index;
		if (random)
		{
			if (randomTextIndex > -1 && randomTextIndex + 1 < randomTexts.Length)
			{
				randomTextIndex++;
			}
			else
			{
				randomTextIndex = 0;
			}
			index = randomTextIndex;
		}
		else
		{
			if (textIndex > -1 && textIndex + 1 < texts.Length)
			{
				textIndex++;
			}
			else
			{
				textIndex = 0;
			}
			index = textIndex;
		}
		SpeakText(index, random);
	}

	public void Previous()
	{
		Previous(speakRandom);
	}

	public void Previous(bool random)
	{
		int index;
		if (random)
		{
			if (randomTextIndex > 0 && randomTextIndex < randomTexts.Length)
			{
				randomTextIndex--;
			}
			else
			{
				randomTextIndex = randomTexts.Length - 1;
			}
			index = randomTextIndex;
		}
		else
		{
			if (textIndex > 0 && textIndex < texts.Length)
			{
				textIndex--;
			}
			else
			{
				textIndex = texts.Length - 1;
			}
			index = textIndex;
		}
		SpeakText(index, random);
	}

	public void Speak()
	{
		Next();
	}

	public string SpeakText(int index = -1, bool random = false)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (lastSpeaktime + Constants.SPEAK_CALL_SPEED < realtimeSinceStartup)
		{
			lastSpeaktime = realtimeSinceStartup;
			Silence();
			string text = string.Empty;
			if (texts.Length != 0)
			{
				if (random)
				{
					if (index < 0)
					{
						text = speak(randomTexts[rnd.Next(randomTexts.Length)]);
					}
					else if (index < texts.Length)
					{
						text = speak(randomTexts[index]);
					}
					else
					{
						Debug.LogWarning("Text file index is out of bounds: " + index + " - maximal index is: " + (randomTexts.Length - 1), this);
						text = speak(randomTexts[randomTexts.Length - 1]);
					}
				}
				else if (index < 0)
				{
					text = speak(texts[rnd.Next(texts.Length)]);
				}
				else if (index < texts.Length)
				{
					text = speak(texts[index]);
				}
				else
				{
					Debug.LogWarning("Text file index is out of bounds: " + index + " - maximal index is: " + (texts.Length - 1), this);
					text = speak(texts[texts.Length - 1]);
				}
			}
			else
			{
				Debug.LogError("No text files added - speak cancelled!", this);
			}
			uid = text;
		}
		else
		{
			Debug.LogWarning("'SpeakText' called too fast - please slow down!", this);
		}
		return uid;
	}

	public void Silence()
	{
		if (BaseHelper.isEditorMode)
		{
			Singleton<Speaker>.Instance.Silence();
		}
		else if (!string.IsNullOrEmpty(uid))
		{
			Singleton<Speaker>.Instance.Silence(uid);
		}
	}

	public void Reload()
	{
		if (textFiles.Length == 0)
		{
			return;
		}
		texts = new string[textFiles.Length];
		randomTexts = new string[textFiles.Length];
		lastNumberOfTextfiles = textFiles.Length;
		for (int i = 0; i < textFiles.Length; i++)
		{
			if (textFiles[i] != null)
			{
				randomTexts[i] = (texts[i] = textFiles[i].text);
			}
			else
			{
				randomTexts[i] = (texts[i] = string.Empty);
			}
		}
		randomTexts.CTShuffle();
		textIndex = -1;
		randomTextIndex = -1;
	}

	private void play()
	{
		if (!BaseHelper.isEditorMode && !played && Singleton<Speaker>.Instance.Voices.Count > 0)
		{
			played = true;
			if (playOnStart)
			{
				Invoke("Next", delay);
			}
			else if (playAllOnStart)
			{
				Invoke("SpeakAll", delay);
			}
		}
	}

	private string speak(string text)
	{
		if (mode != 0)
		{
			return Singleton<Speaker>.Instance.SpeakNative(text, voices.Voice, rate, pitch, volume);
		}
		return Singleton<Speaker>.Instance.Speak(text, source, voices.Voice, speakImmediately: true, rate, pitch, volume);
	}

	private void onVoicesReady()
	{
		play();
	}

	private void onSpeakStart(Wrapper wrapper)
	{
		if (wrapper.Uid.Equals(uid))
		{
			if (!BaseHelper.isEditorMode)
			{
				OnStarted?.Invoke();
			}
			this.OnTextFileSpeakerStart?.Invoke();
		}
	}

	private void onSpeakComplete(Wrapper wrapper)
	{
		if (wrapper.Uid.Equals(uid))
		{
			if (!BaseHelper.isEditorMode)
			{
				OnCompleted?.Invoke();
			}
			this.OnTextFileSpeakerComplete?.Invoke();
			if (playAll)
			{
				Invoke("Next", delay);
			}
		}
	}
}
