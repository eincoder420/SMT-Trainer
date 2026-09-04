using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using Crosstales.RTVoice.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crosstales.RTVoice.Tool;

[RequireComponent(typeof(AudioSource))]
[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_tool_1_1_paralanguage.html")]
public class Paralanguage : MonoBehaviour
{
	[Header("Configuration")]
	[FormerlySerializedAs("Text")]
	[Tooltip("Text to speak.")]
	[TextArea(3, 15)]
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

	[FormerlySerializedAs("Clips")]
	[Tooltip("Audio clips to play.")]
	[SerializeField]
	private AudioClip[] clips;

	[FormerlySerializedAs("Rate")]
	[Header("Optional Settings")]
	[Tooltip("Speech rate of the speaker in percent (1 = 100%, default: 1, optional).")]
	[Range(0f, 3f)]
	[SerializeField]
	private float rate = 1f;

	[FormerlySerializedAs("Pitch")]
	[Tooltip("Speech pitch of the speaker in percent (1 = 100%, default: 1, optional).")]
	[Range(0f, 2f)]
	[SerializeField]
	private float pitch = 1f;

	[FormerlySerializedAs("Volume")]
	[Tooltip("Volume of the speaker in percent (1 = 100%, default: 1, optional).")]
	[Range(0f, 1f)]
	[SerializeField]
	private float volume = 1f;

	[FormerlySerializedAs("PlayOnStart")]
	[Header("Behaviour Settings")]
	[Tooltip("Enable speaking of the text on start (default: false).")]
	[SerializeField]
	private bool playOnStart;

	[FormerlySerializedAs("Delay")]
	[Tooltip("Delay until the speech for this text starts (default: 0.1")]
	[SerializeField]
	private float delay = 0.1f;

	private static readonly Regex splitRegex = new Regex("#.*?#");

	private string uid;

	private bool played;

	private readonly IDictionary<int, string> stack = new SortedDictionary<int, string>();

	private readonly IDictionary<string, AudioClip> clipDict = new Dictionary<string, AudioClip>();

	private AudioSource audioSource;

	private bool next;

	[Header("Events")]
	public ParalanguageStartEvent OnStarted;

	public ParalanguageCompleteEvent OnCompleted;

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

	public AudioClip[] Clips
	{
		get
		{
			return clips;
		}
		set
		{
			clips = value;
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

	public event ParalanguageStart OnParalanguageStart;

	public event ParalanguageComplete OnParalanguageComplete;

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnVoicesReady -= onVoicesReady;
			Singleton<Speaker>.Instance.OnSpeakComplete -= onSpeakComplete;
		}
	}

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.loop = false;
		audioSource.Stop();
	}

	private void Start()
	{
		Singleton<Speaker>.Instance.OnVoicesReady += onVoicesReady;
		Singleton<Speaker>.Instance.OnSpeakComplete += onSpeakComplete;
		play();
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

	public void Speak()
	{
		Silence();
		stack.Clear();
		clipDict.Clear();
		AudioClip[] array = clips;
		foreach (AudioClip audioClip in array)
		{
			clipDict.Add("#" + audioClip.name + "#", audioClip);
		}
		string[] array2 = (from s in splitRegex.Split(this.text)
			where s != string.Empty
			select s).ToArray();
		MatchCollection matchCollection = splitRegex.Matches(this.text);
		int startIndex = 0;
		foreach (Match item in matchCollection)
		{
			stack.Add(startIndex = this.text.CTIndexOf(item.ToString(), startIndex), item.ToString());
			startIndex++;
		}
		startIndex = 0;
		string[] array3 = array2;
		foreach (string text in array3)
		{
			stack.Add(startIndex = this.text.CTIndexOf(text, startIndex), text);
			startIndex++;
		}
		StartCoroutine(processStack());
	}

	public void Silence()
	{
		StopAllCoroutines();
		if (BaseHelper.isEditorMode)
		{
			Singleton<Speaker>.Instance.Silence();
		}
		else if (!string.IsNullOrEmpty(uid))
		{
			Singleton<Speaker>.Instance.Silence(uid);
		}
	}

	private IEnumerator processStack()
	{
		onStart();
		foreach (KeyValuePair<int, string> item in stack)
		{
			if (item.Value.CTStartsWith("#"))
			{
				clipDict.TryGetValue(item.Value, out var value);
				if (clipDict.TryGetValue(item.Value, out value))
				{
					audioSource.clip = value;
					audioSource.Play();
					do
					{
						yield return null;
					}
					while (audioSource.isPlaying);
				}
				else
				{
					Debug.LogWarning("Clip not found: " + item.Value, this);
				}
			}
			else
			{
				next = false;
				uid = ((mode == SpeakMode.Speak) ? Singleton<Speaker>.Instance.Speak(item.Value, audioSource, voices.Voice, speakImmediately: true, rate, pitch, volume) : Singleton<Speaker>.Instance.SpeakNative(item.Value, voices.Voice, rate, pitch, volume));
				do
				{
					yield return null;
				}
				while (!next);
			}
		}
		onComplete();
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

	private void onSpeakComplete(Wrapper wrapper)
	{
		if (wrapper.Uid.Equals(uid))
		{
			next = true;
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
		this.OnParalanguageStart?.Invoke();
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
		this.OnParalanguageComplete?.Invoke();
	}
}
