using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using Crosstales.RTVoice.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crosstales.RTVoice.Tool;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_tool_1_1_speech_text.html")]
public class SpeechText : MonoBehaviour
{
	[Header("Configuration")]
	[FormerlySerializedAs("Text")]
	[Tooltip("Text to speak.")]
	[TextArea(5, 15)]
	[SerializeField]
	private string text = string.Empty;

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
	[Tooltip("Enable speaking of the text on start (default: false).")]
	[SerializeField]
	private bool playOnStart;

	[FormerlySerializedAs("Delay")]
	[Tooltip("Delay in seconds until the speech for this text starts (default: 0.1).")]
	[SerializeField]
	private float delay = 0.1f;

	[FormerlySerializedAs("GenerateAudioFile")]
	[Header("Output File Settings")]
	[Tooltip("Generate audio file on/off (default: false).")]
	[SerializeField]
	private bool generateAudioFile;

	[FormerlySerializedAs("FileName")]
	[Tooltip("File name (incl. path) for the generated audio.")]
	[SerializeField]
	private string fileName = "_generatedAudio/Speech01";

	[FormerlySerializedAs("FileInsideAssets")]
	[Tooltip("Is the generated file path inside the Assets-folder (current project)? If this option is enabled, it prefixes the path with 'Application.dataPath'.")]
	[SerializeField]
	private bool fileInsideAssets = true;

	private string uid;

	private bool played;

	private float lastSpeaktime = float.MinValue;

	[Header("Events")]
	public SpeechTextStartEvent OnStarted;

	public SpeechTextStartEvent OnCompleted;

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
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

	public bool GenerateAudioFile
	{
		get
		{
			return generateAudioFile;
		}
		set
		{
			generateAudioFile = value;
		}
	}

	public string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
			fileName = value;
		}
	}

	public bool FileInsideAssets
	{
		get
		{
			return fileInsideAssets;
		}
		set
		{
			fileInsideAssets = value;
		}
	}

	public event SpeechTextStart OnSpeechTextStart;

	public event SpeechTextComplete OnSpeechTextComplete;

	private void Start()
	{
		Singleton<Speaker>.Instance.OnVoicesReady += onVoicesReady;
		Singleton<Speaker>.Instance.OnSpeakStart += onSpeakStart;
		Singleton<Speaker>.Instance.OnSpeakComplete += onSpeakComplete;
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

	private void OnValidate()
	{
		if (delay < 0f)
		{
			delay = 0f;
		}
		rate = Mathf.Clamp(rate, 0f, 3f);
		pitch = Mathf.Clamp(pitch, 0f, 2f);
		volume = Mathf.Clamp01(volume);
		if (!string.IsNullOrEmpty(fileName))
		{
			fileName = FileHelper.ValidateFile(fileName);
		}
	}

	public void Speak()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (lastSpeaktime + Constants.SPEAK_CALL_SPEED < realtimeSinceStartup)
		{
			lastSpeaktime = realtimeSinceStartup;
			Silence();
			string outputFile = null;
			if (generateAudioFile)
			{
				if (!string.IsNullOrEmpty(fileName))
				{
					outputFile = (fileInsideAssets ? FileHelper.ValidateFile(Application.dataPath + "/" + fileName) : FileHelper.ValidateFile(fileName));
				}
				else
				{
					Debug.LogWarning("'FileName' is null or empty! Can't generate audio file.", this);
				}
			}
			if (!BaseHelper.isEditorMode)
			{
				uid = ((mode == SpeakMode.Speak) ? Singleton<Speaker>.Instance.Speak(text, source, voices.Voice, speakImmediately: true, rate, pitch, volume, outputFile) : Singleton<Speaker>.Instance.SpeakNative(text, voices.Voice, rate, pitch, volume));
			}
		}
		else
		{
			Debug.LogWarning("'Speak' called too fast - please slow down!", this);
		}
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

	private void play()
	{
		if (playOnStart && !played && Singleton<Speaker>.Instance.Voices.Count > 0)
		{
			played = true;
			Invoke("Speak", delay);
		}
	}

	private void onVoicesReady()
	{
		play();
	}

	private void onSpeakStart(Wrapper wrapper)
	{
		if (wrapper.Uid.Equals(uid))
		{
			onStart();
		}
	}

	private void onSpeakComplete(Wrapper wrapper)
	{
		if (wrapper.Uid.Equals(uid))
		{
			onComplete();
		}
	}

	private void onStart()
	{
		if (Config.DEBUG)
		{
			Debug.Log("onStart", this);
		}
		if (!BaseHelper.isEditorMode)
		{
			OnStarted?.Invoke();
		}
		this.OnSpeechTextStart?.Invoke();
	}

	private void onComplete()
	{
		if (Config.DEBUG)
		{
			Debug.Log("onComplete", this);
		}
		if (!BaseHelper.isEditorMode)
		{
			OnCompleted?.Invoke();
		}
		this.OnSpeechTextComplete?.Invoke();
	}
}
