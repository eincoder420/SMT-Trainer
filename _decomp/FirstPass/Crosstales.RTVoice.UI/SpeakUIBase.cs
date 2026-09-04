using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Crosstales.RTVoice.UI;

[DisallowMultipleComponent]
public abstract class SpeakUIBase : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Configuration")]
	[Tooltip("Voices for the speech.")]
	[SerializeField]
	private VoiceAlias voices;

	[Tooltip("Speak mode (default: 'Speak').")]
	[SerializeField]
	private SpeakMode mode;

	[Tooltip("Delay in seconds before the speech starts (default: 1.5).")]
	[Range(0f, 10f)]
	[SerializeField]
	private float delay = 1.5f;

	[Tooltip("Always speak the text if the content changed (default: false).")]
	[SerializeField]
	private bool speakIfChanged;

	[Tooltip("Speak the text only once the user hovered over the component (default: true).")]
	[SerializeField]
	private bool speakOnce = true;

	[Tooltip("Silence the speech once exit (default: true).")]
	[SerializeField]
	private bool silenceOnExit = true;

	[Header("Optional Settings")]
	[Tooltip("AudioSource for the output (optional).")]
	[SerializeField]
	private AudioSource source;

	[Tooltip("Speech rate of the speaker in percent (1 = 100%, default: 1, optional).")]
	[Range(0f, 3f)]
	[SerializeField]
	private float rate = 1f;

	[Tooltip("Speech pitch of the speaker in percent (1 = 100%, default: 1, optional, mobile only).")]
	[Range(0f, 2f)]
	[SerializeField]
	private float pitch = 1f;

	[Tooltip("Volume of the speaker in percent (1 = 100%, default: 1, optional, Windows only).")]
	[Range(0f, 1f)]
	[SerializeField]
	private float volume = 1f;

	protected float elapsedTime;

	protected string uid;

	protected bool isInside;

	protected bool spoken;

	protected bool isSpeaking;

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

	public bool SpeakIfChanged
	{
		get
		{
			return speakIfChanged;
		}
		set
		{
			speakIfChanged = value;
		}
	}

	public bool SpeakOnlyOnce
	{
		get
		{
			return speakOnce;
		}
		set
		{
			speakOnce = value;
		}
	}

	public bool SilenceOnExit
	{
		get
		{
			return silenceOnExit;
		}
		set
		{
			silenceOnExit = value;
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

	protected virtual void Start()
	{
		Singleton<Speaker>.Instance.OnSpeakAudioGenerationStart += onSpeakStart;
		Singleton<Speaker>.Instance.OnSpeakComplete += onSpeakComplete;
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnSpeakAudioGenerationStart -= onSpeakStart;
			Singleton<Speaker>.Instance.OnSpeakComplete -= onSpeakComplete;
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		isInside = true;
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		isInside = false;
		if (SilenceOnExit && uid != null)
		{
			if (Mode == SpeakMode.Speak)
			{
				Singleton<Speaker>.Instance.Silence(uid);
			}
			else
			{
				Singleton<Speaker>.Instance.Silence();
			}
		}
	}

	protected virtual string speak(string text)
	{
		if (Mode != 0)
		{
			return Singleton<Speaker>.Instance.SpeakNative(text, Voices.Voice, Rate, Pitch, Volume);
		}
		return Singleton<Speaker>.Instance.Speak(text, Source, Voices.Voice, speakImmediately: true, Rate, Pitch, Volume);
	}

	protected virtual void onSpeakStart(Wrapper wrapper)
	{
		if (wrapper.Uid == uid)
		{
			isSpeaking = true;
		}
	}

	protected virtual void onSpeakComplete(Wrapper wrapper)
	{
		if (wrapper.Uid == uid)
		{
			isInside = false;
			spoken = true;
			elapsedTime = 0f;
			uid = null;
			isSpeaking = false;
		}
	}
}
