using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using Crosstales.RTVoice.Provider;
using Crosstales.RTVoice.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crosstales.RTVoice;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(LiveSpeaker))]
[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_speaker.html")]
public class Speaker : Singleton<Speaker>
{
	[FormerlySerializedAs("CustomProvider")]
	[Header("Custom Provider")]
	[Tooltip("Custom provider for RT-Voice.")]
	[SerializeField]
	private BaseCustomVoiceProvider customProvider;

	[FormerlySerializedAs("CustomMode")]
	[Tooltip("Enable or disable the custom provider (default: false).")]
	[SerializeField]
	private bool customMode;

	[FormerlySerializedAs("ESpeakMode")]
	[Header("eSpeak Settings")]
	[Tooltip("Enable or disable eSpeak for standalone platforms (default: false).")]
	[SerializeField]
	private bool eSpeakMode;

	[Tooltip("eSpeak application name/path (default: 'espeak').")]
	[SerializeField]
	private string eSpeakApplication = "espeak";

	[Tooltip("eSpeak application data path (default: empty).")]
	[SerializeField]
	private string eSpeakDataPath = string.Empty;

	[FormerlySerializedAs("ESpeakModifier")]
	[Tooltip("Active modifier for all eSpeak voices (default: none, m1-m6 = male, f1-f4 = female).")]
	[SerializeField]
	private ESpeakModifiers eSpeakModifier;

	[FormerlySerializedAs("AndroidEngine")]
	[Header("Android Settings")]
	[Tooltip("Active speech engine under Android (default: empty).")]
	[SerializeField]
	private string androidEngine = string.Empty;

	[Header("Windows Settings")]
	[Tooltip("Force 32bit under Windows standalone (default: false).")]
	[SerializeField]
	private bool windowsForce32bit;

	[FormerlySerializedAs("AutoClearTags")]
	[Header("Advanced Settings")]
	[Tooltip("Automatically clear tags from speeches depending on the capabilities of the current TTS-system (default: false).")]
	[SerializeField]
	private bool autoClearTags;

	[FormerlySerializedAs("Caching")]
	[Tooltip("Enable or disable the caching of generated speeches (default: true).")]
	[SerializeField]
	private bool caching = true;

	[FormerlySerializedAs("SilenceOnDisable")]
	[Header("Behaviour Settings")]
	[Tooltip("Silence any speeches if this component gets disabled (default: false).")]
	[SerializeField]
	private bool silenceOnDisable;

	[FormerlySerializedAs("SilenceOnFocusLost")]
	[FormerlySerializedAs("SilenceOnFocustLost")]
	[Tooltip("Silence any speeches if the application loses the focus. Otherwise the speeches are paused and unpaused (default: false).")]
	[SerializeField]
	private bool silenceOnFocusLost;

	[Tooltip("Starts and stops the Speaker depending on the focus and running state (default: true).")]
	[SerializeField]
	private bool handleFocus = true;

	private float cleanUpTimer;

	private IVoiceProvider voiceProvider;

	private MainVoiceProvider mainVoiceProvider;

	private BaseCustomVoiceProvider customVoiceProvider;

	private readonly Dictionary<string, AudioSource> genericSources = new Dictionary<string, AudioSource>();

	private readonly Dictionary<string, AudioSource> providedSources = new Dictionary<string, AudioSource>();

	private int speechCount;

	private int busyCount;

	private int realSpeechCount;

	private bool deleted;

	private static readonly char[] splitCharWords = new char[1] { ' ' };

	private const float cleanUpTime = 5f;

	private static bool loggedVPIsNull;

	private Thread deleteWorker;

	public VoicesReadyEvent OnReady;

	public SpeakStartEvent OnSpeakStarted;

	public SpeakCompleteEvent OnSpeakCompleted;

	public ProviderChangeEvent OnProviderChanged;

	public ErrorEvent OnError;

	public BaseCustomVoiceProvider CustomProvider
	{
		get
		{
			return customProvider;
		}
		set
		{
			if (!(customProvider == value))
			{
				customProvider = value;
				ReloadProvider();
			}
		}
	}

	public bool CustomMode
	{
		get
		{
			return customMode;
		}
		set
		{
			if (customMode != value)
			{
				customMode = value;
				ReloadProvider();
			}
		}
	}

	public bool ESpeakMode
	{
		get
		{
			return eSpeakMode;
		}
		set
		{
			if (eSpeakMode != value)
			{
				eSpeakMode = value;
				ReloadProvider();
			}
		}
	}

	public string ESpeakApplication
	{
		get
		{
			return eSpeakApplication;
		}
		set
		{
			eSpeakApplication = value;
		}
	}

	public string ESpeakDataPath
	{
		get
		{
			return eSpeakDataPath;
		}
		set
		{
			eSpeakDataPath = value;
		}
	}

	public ESpeakModifiers ESpeakModifier
	{
		get
		{
			return eSpeakModifier;
		}
		set
		{
			eSpeakModifier = value;
		}
	}

	public string AndroidEngine
	{
		get
		{
			return androidEngine;
		}
		set
		{
			if (!(androidEngine == value) && BaseHelper.isAndroidPlatform)
			{
				androidEngine = value;
				ReloadProvider();
			}
		}
	}

	public bool WindowsForce32bit
	{
		get
		{
			return windowsForce32bit;
		}
		set
		{
			if (windowsForce32bit != value && BaseHelper.isWindowsPlatform)
			{
				windowsForce32bit = value;
				ReloadProvider();
			}
		}
	}

	public bool AutoClearTags
	{
		get
		{
			return autoClearTags;
		}
		set
		{
			autoClearTags = value;
		}
	}

	public bool Caching
	{
		get
		{
			return caching;
		}
		set
		{
			caching = value;
		}
	}

	public bool SilenceOnDisable
	{
		get
		{
			return silenceOnDisable;
		}
		set
		{
			silenceOnDisable = value;
		}
	}

	public bool SilenceOnFocusLost
	{
		get
		{
			return silenceOnFocusLost;
		}
		set
		{
			silenceOnFocusLost = value;
		}
	}

	public bool HandleFocus
	{
		get
		{
			return handleFocus;
		}
		set
		{
			handleFocus = value;
		}
	}

	public int SpeechCount
	{
		get
		{
			return speechCount;
		}
		private set
		{
			speechCount = ((value >= 0) ? value : 0);
		}
	}

	public int BusyCount
	{
		get
		{
			return busyCount;
		}
		private set
		{
			busyCount = ((value >= 0) ? value : 0);
		}
	}

	public bool areVoicesReady { get; private set; }

	public bool isTTSAvailable
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.Voices.Count > 0;
			}
			logVPIsNull();
			return false;
		}
	}

	public bool isSpeaking => SpeechCount > 0;

	public bool isBusy => BusyCount > 0;

	public bool enforcedStandaloneTTS { get; private set; }

	public bool isPaused { get; private set; }

	public bool isMuted { get; private set; }

	public string AudioFileExtension
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.AudioFileExtension;
			}
			logVPIsNull();
			return ".wav";
		}
	}

	public string DefaultVoiceName
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.DefaultVoiceName;
			}
			logVPIsNull();
			return string.Empty;
		}
	}

	public List<Voice> Voices
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.Voices;
			}
			logVPIsNull();
			return new List<Voice>();
		}
	}

	public bool isWorkingInEditor
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.isWorkingInEditor;
			}
			logVPIsNull();
			return false;
		}
	}

	public bool isWorkingInPlaymode
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.isWorkingInPlaymode;
			}
			logVPIsNull();
			return false;
		}
	}

	public int MaxTextLength
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.MaxTextLength;
			}
			logVPIsNull();
			return 3999;
		}
	}

	public bool isSpeakNativeSupported
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.isSpeakNativeSupported;
			}
			logVPIsNull();
			return false;
		}
	}

	public bool isSpeakSupported
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.isSpeakSupported;
			}
			logVPIsNull();
			return false;
		}
	}

	public bool isPlatformSupported => voiceProvider?.isPlatformSupported ?? false;

	public bool isSSMLSupported
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.isSSMLSupported;
			}
			logVPIsNull();
			return false;
		}
	}

	public bool isOnlineService
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.isOnlineService;
			}
			logVPIsNull();
			return false;
		}
	}

	public bool hasCoRoutines
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.hasCoRoutines;
			}
			logVPIsNull();
			return true;
		}
	}

	public bool isIL2CPPSupported
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.isIL2CPPSupported;
			}
			logVPIsNull();
			return true;
		}
	}

	public bool hasVoicesInEditor
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.hasVoicesInEditor;
			}
			logVPIsNull();
			return false;
		}
	}

	public int MaxSimultaneousSpeeches
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.MaxSimultaneousSpeeches;
			}
			logVPIsNull();
			return 0;
		}
	}

	public List<string> Cultures
	{
		get
		{
			if (voiceProvider != null)
			{
				return voiceProvider.Cultures;
			}
			logVPIsNull();
			return new List<string>();
		}
	}

	public List<SystemLanguage> Languages
	{
		get
		{
			List<SystemLanguage> list = new List<SystemLanguage>();
			if (voiceProvider != null)
			{
				foreach (string culture in voiceProvider.Cultures)
				{
					SystemLanguage item = BaseHelper.ISO639ToLanguage(culture);
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			else
			{
				logVPIsNull();
			}
			return list;
		}
	}

	public List<string> Engines => new List<string>();

	public event VoicesReady OnVoicesReady;

	public event SpeakStart OnSpeakStart;

	public event SpeakComplete OnSpeakComplete;

	public event SpeakCurrentWord OnSpeakCurrentWord;

	public event SpeakCurrentWordString OnSpeakCurrentWordString;

	public event SpeakCurrentPhoneme OnSpeakCurrentPhoneme;

	public event SpeakCurrentViseme OnSpeakCurrentViseme;

	public event SpeakAudioGenerationStart OnSpeakAudioGenerationStart;

	public event SpeakAudioGenerationComplete OnSpeakAudioGenerationComplete;

	public event ProviderChange OnProviderChange;

	public event ErrorInfo OnErrorInfo;

	protected override void Awake()
	{
		base.Awake();
		if (!(Singleton<Speaker>.instance == this))
		{
			return;
		}
		if (!deleted)
		{
			deleted = true;
			if (Config.AUDIOFILE_AUTOMATIC_DELETE)
			{
				DeleteAudioFiles();
			}
		}
		if (BaseHelper.isLinuxPlatform)
		{
			eSpeakMode = true;
		}
		initProvider();
	}

	private void Update()
	{
		cleanUpTimer += Time.deltaTime;
		if (!(cleanUpTimer > 5f))
		{
			return;
		}
		cleanUpTimer = 0f;
		if (genericSources.Count > 0)
		{
			KeyValuePair<string, AudioSource>[] array = genericSources.Where((KeyValuePair<string, AudioSource> source) => source.Value != null && source.Value.clip != null && !source.Value.CTHasActiveClip()).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<string, AudioSource> keyValuePair = array[i];
				genericSources.Remove(keyValuePair.Key);
				UnityEngine.Object.Destroy(keyValuePair.Value);
			}
		}
		if (providedSources.Count > 0)
		{
			KeyValuePair<string, AudioSource>[] array = providedSources.Where((KeyValuePair<string, AudioSource> source) => source.Value != null && source.Value.clip != null && !source.Value.CTHasActiveClip()).ToArray();
			foreach (KeyValuePair<string, AudioSource> keyValuePair2 in array)
			{
				providedSources.Remove(keyValuePair2.Key);
			}
		}
	}

	private void OnDisable()
	{
		if (silenceOnDisable)
		{
			Silence();
		}
	}

	protected override void OnDestroy()
	{
		Silence();
		if (Singleton<Speaker>.instance == this)
		{
			unsubscribeEvents();
			unsubscribeCustomEvents();
		}
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		Silence();
		deleteWorker.CTAbort();
		base.OnApplicationQuit();
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!BaseHelper.isMobilePlatform && Application.runInBackground)
		{
			return;
		}
		if (silenceOnFocusLost)
		{
			if (!hasFocus)
			{
				Silence();
			}
		}
		else if (handleFocus)
		{
			if (hasFocus)
			{
				UnPause();
			}
			else
			{
				Pause();
			}
		}
	}

	public static void ResetObject()
	{
		Singleton<Speaker>.DeleteInstance();
		loggedVPIsNull = false;
	}

	public float ApproximateSpeechLength(string text, float rate = 1f, float wordsPerMinute = 175f, float timeFactor = 0.9f)
	{
		float num = text.Split(splitCharWords, StringSplitOptions.RemoveEmptyEntries).Length;
		float num2 = ((float)text.Length - num + 1f) / num;
		if (BaseHelper.isWindowsPlatform && !ESpeakMode && !CustomMode && Mathf.Abs(rate - 1f) > 0.0001f)
		{
			if (rate > 1f)
			{
				if (rate >= 2.75f)
				{
					rate = 2.78f;
				}
				else if (rate >= 2.6f && rate < 2.75f)
				{
					rate = 2.6f;
				}
				else if (rate >= 2.35f && rate < 2.6f)
				{
					rate = 2.39f;
				}
				else if (rate >= 2.2f && rate < 2.35f)
				{
					rate = 2.2f;
				}
				else if (rate >= 2f && rate < 2.2f)
				{
					rate = 2f;
				}
				else if (rate >= 1.8f && rate < 2f)
				{
					rate = 1.8f;
				}
				else if (rate >= 1.6f && rate < 1.8f)
				{
					rate = 1.6f;
				}
				else if (rate >= 1.4f && rate < 1.6f)
				{
					rate = 1.45f;
				}
				else if (rate >= 1.2f && rate < 1.4f)
				{
					rate = 1.28f;
				}
				else if (rate > 1f && rate < 1.2f)
				{
					rate = 1.14f;
				}
			}
			else if (rate <= 0.3f)
			{
				rate = 0.33f;
			}
			else if ((double)rate > 0.3 && rate <= 0.4f)
			{
				rate = 0.375f;
			}
			else if ((double)rate > 0.4 && rate <= 0.45f)
			{
				rate = 0.42f;
			}
			else if ((double)rate > 0.45 && rate <= 0.5f)
			{
				rate = 0.47f;
			}
			else if ((double)rate > 0.5 && rate <= 0.55f)
			{
				rate = 0.525f;
			}
			else if ((double)rate > 0.55 && rate <= 0.6f)
			{
				rate = 0.585f;
			}
			else if ((double)rate > 0.6 && rate <= 0.7f)
			{
				rate = 0.655f;
			}
			else if ((double)rate > 0.7 && rate <= 0.8f)
			{
				rate = 0.732f;
			}
			else if ((double)rate > 0.8 && rate <= 0.9f)
			{
				rate = 0.82f;
			}
			else if ((double)rate > 0.9 && rate < 1f)
			{
				rate = 0.92f;
			}
		}
		float num3 = num / (wordsPerMinute / 60f * rate);
		num3 = ((num2 < 2f) ? (num3 * 1f) : ((num2 >= 2f && num2 < 3f) ? (num3 * 1.05f) : ((num2 >= 3f && num2 < 3.5f) ? (num3 * 1.15f) : ((num2 >= 3.5f && num2 < 4f) ? (num3 * 1.2f) : ((num2 >= 4f && num2 < 4.5f) ? (num3 * 1.25f) : ((num2 >= 4.5f && num2 < 5f) ? (num3 * 1.3f) : ((num2 >= 5f && num2 < 5.5f) ? (num3 * 1.4f) : ((num2 >= 5.5f && num2 < 6f) ? (num3 * 1.45f) : ((num2 >= 6f && num2 < 6.5f) ? (num3 * 1.5f) : ((num2 >= 6.5f && num2 < 7f) ? (num3 * 1.6f) : ((num2 >= 7f && num2 < 8f) ? (num3 * 1.7f) : ((!(num2 >= 8f) || !(num2 < 9f)) ? (num3 * (num2 * (num2 / 100f + 0.02f) + 1f)) : (num3 * 1.8f)))))))))))));
		if (num3 < 0.8f)
		{
			num3 += 0.6f;
		}
		return num3 * timeFactor;
	}

	public bool isVoiceForGenderAvailable(Gender gender, string culture = "")
	{
		return VoicesForGender(gender, culture).Count > 0;
	}

	public bool isVoiceForGenderAvailable(Gender gender, SystemLanguage language)
	{
		return isVoiceForGenderAvailable(gender, BaseHelper.LanguageToISO639(language));
	}

	public List<Voice> VoicesForGender(Gender gender, string culture = "", bool isFuzzy = false)
	{
		List<Voice> list = new List<Voice>(Voices.Count);
		if (string.IsNullOrEmpty(culture))
		{
			if (Gender.UNKNOWN == gender)
			{
				return Voices;
			}
			list.AddRange(Voices.Where((Voice voice) => voice.Gender == gender));
		}
		else
		{
			if (Gender.UNKNOWN == gender)
			{
				return VoicesForCulture(culture, isFuzzy);
			}
			list.AddRange(from voice in VoicesForCulture(culture, isFuzzy)
				where voice.Gender == gender
				select voice);
			if (list.Count == 0)
			{
				return VoicesForCulture(culture, isFuzzy);
			}
		}
		return list;
	}

	public List<Voice> VoicesForGender(Gender gender, SystemLanguage language, bool isFuzzy = false)
	{
		return VoicesForGender(gender, BaseHelper.LanguageToISO639(language), isFuzzy);
	}

	public Voice VoiceForGender(Gender gender, string culture = "", int index = 0, string fallbackCulture = "en", bool isFuzzy = false)
	{
		Voice result = null;
		List<Voice> list = VoicesForGender(gender, culture, isFuzzy);
		if (list.Count > 0)
		{
			if (list.Count - 1 >= index && index >= 0)
			{
				result = list[index];
			}
			else
			{
				Debug.LogWarning($"No voice for gender '{gender}' and culture '{culture}' with index {index} found! Speaking with the default voice!", this);
			}
		}
		else
		{
			list = VoicesForGender(gender, fallbackCulture, isFuzzy);
			if (list.Count > 0)
			{
				result = list[0];
				Debug.LogWarning($"No voice for gender '{gender}' and culture '{culture}' found! Speaking with the fallback culture: '{fallbackCulture}'", this);
			}
			else
			{
				Debug.LogWarning($"No voice for gender '{gender}' and culture '{culture}' found! Speaking with the default voice!", this);
			}
		}
		return result;
	}

	public Voice VoiceForGender(Gender gender, SystemLanguage language, int index = 0, bool isFuzzy = false)
	{
		return VoiceForGender(gender, BaseHelper.LanguageToISO639(language), index, "en", isFuzzy);
	}

	public bool isVoiceForCultureAvailable(string culture)
	{
		return VoicesForCulture(culture).Count > 0;
	}

	public bool isVoiceForLanguageAvailable(SystemLanguage language)
	{
		return isVoiceForCultureAvailable(BaseHelper.LanguageToISO639(language));
	}

	public List<Voice> VoicesForCulture(string culture, bool isFuzzy = false)
	{
		if (string.IsNullOrEmpty(culture))
		{
			if (Config.DEBUG)
			{
				Debug.LogWarning("The given 'culture' is null or empty! Returning all available voices.", this);
			}
			return Voices;
		}
		string _culture = culture.Trim().Replace(" ", string.Empty).Replace("_", string.Empty)
			.Replace("-", string.Empty);
		List<Voice> list = (from s in Voices
			where s.SimplifiedCulture.StartsWith(_culture, StringComparison.InvariantCultureIgnoreCase)
			orderby s.Name
			select s).ToList();
		if (list.Count == 0 && isFuzzy)
		{
			return Voices;
		}
		return list;
	}

	public List<Voice> VoicesForLanguage(SystemLanguage language, bool isFuzzy = false)
	{
		return VoicesForCulture(BaseHelper.LanguageToISO639(language), isFuzzy);
	}

	public Voice VoiceForCulture(string culture, int index = 0, string fallbackCulture = "en", bool isFuzzy = false)
	{
		Voice result = null;
		if (!string.IsNullOrEmpty(culture))
		{
			List<Voice> list = VoicesForCulture(culture, isFuzzy);
			if (list.Count > 0)
			{
				if (list.Count - 1 >= index && index >= 0)
				{
					result = list[index];
				}
				else
				{
					Debug.LogWarning($"No voices for culture '{culture}' with index {index} found! Speaking with the default voice!", this);
				}
			}
			else
			{
				list = VoicesForCulture(fallbackCulture, isFuzzy);
				if (list.Count > 0)
				{
					result = list[0];
					Debug.LogWarning("No voices for culture '" + culture + "' found! Speaking with the fallback culture: '" + fallbackCulture + "'", this);
				}
				else
				{
					Debug.LogWarning("No voices for culture '" + culture + "' found! Speaking with the default voice!", this);
				}
			}
		}
		return result;
	}

	public Voice VoiceForLanguage(SystemLanguage language, int index = 0, bool isFuzzy = false)
	{
		return VoiceForCulture(BaseHelper.LanguageToISO639(language), index, "en", isFuzzy);
	}

	public bool isVoiceForNameAvailable(string _name, bool isExact = false)
	{
		return VoiceForName(_name, isExact) != null;
	}

	public Voice VoiceForName(string _name, bool isExact = false)
	{
		Voice voice2 = null;
		if (string.IsNullOrEmpty(_name))
		{
			if (Config.DEBUG)
			{
				Debug.LogWarning("The given 'name' is null or empty! Returning null.", this);
			}
		}
		else
		{
			voice2 = (isExact ? Voices.FirstOrDefault((Voice voice) => voice.Name.CTEquals(_name)) : Voices.FirstOrDefault((Voice voice) => voice.Name.CTContains(_name)));
			if (voice2 == null)
			{
				Debug.LogWarning("No voice for name '" + _name + "' found! Speaking with the default voice!", this);
			}
		}
		return voice2;
	}

	public void SpeakNativeWithUID(Wrapper wrapper)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return;
		}
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.LogWarning($"SpeakNativeWithUID called: {wrapper}", this);
		}
		if (wrapper != null)
		{
			if (BaseHelper.isEditorMode)
			{
				return;
			}
			if (voiceProvider != null)
			{
				if (string.IsNullOrEmpty(wrapper.Text))
				{
					Debug.LogWarning("'wrapper.Text' is null or empty!", this);
					return;
				}
				BusyCount++;
				if (!voiceProvider.isSpeakNativeSupported)
				{
					if (wrapper.Source == null)
					{
						wrapper.Source = base.gameObject.AddComponent<AudioSource>();
						genericSources.Add(wrapper.Uid, wrapper.Source);
					}
					else if (!providedSources.ContainsKey(wrapper.Uid))
					{
						providedSources.Add(wrapper.Uid, wrapper.Source);
					}
					wrapper.SpeakImmediately = true;
				}
				if (SpeechCount <= 1)
				{
					realSpeechCount++;
					StartCoroutine(voiceProvider.SpeakNative(wrapper));
				}
				else
				{
					Debug.LogWarning("Maximum one native speech per time! Please wait for the speech to complete or stop it.");
				}
			}
			else
			{
				logVPIsNull();
			}
		}
		else
		{
			logWrapperIsNull();
		}
	}

	public string SpeakNative(string text, Voice voice = null, float rate = 1f, float pitch = 1f, float volume = 1f, bool forceSSML = true)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return "disabled";
		}
		Wrapper wrapper = new Wrapper(text, voice, rate, pitch, volume, forceSSML);
		SpeakNativeWithUID(wrapper);
		return wrapper.Uid;
	}

	public string SpeakNative(Wrapper wrapper)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return "disabled";
		}
		if (wrapper != null)
		{
			SpeakNativeWithUID(wrapper);
			return wrapper.Uid;
		}
		logWrapperIsNull();
		return string.Empty;
	}

	public void SpeakWithUID(Wrapper wrapper)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return;
		}
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.LogWarning($"SpeakWithUID called: {wrapper}", this);
		}
		if (wrapper != null)
		{
			if (BaseHelper.isEditorMode)
			{
				return;
			}
			if (voiceProvider != null)
			{
				if (string.IsNullOrEmpty(wrapper.Text))
				{
					Debug.LogWarning("'wrapper.Text' is null or empty!", this);
					return;
				}
				BusyCount++;
				if (voiceProvider.isSpeakSupported)
				{
					if (wrapper.Source == null)
					{
						wrapper.Source = base.gameObject.AddComponent<AudioSource>();
						genericSources.Add(wrapper.Uid, wrapper.Source);
						if (string.IsNullOrEmpty(wrapper.OutputFile))
						{
							wrapper.SpeakImmediately = true;
						}
					}
					else if (!providedSources.ContainsKey(wrapper.Uid))
					{
						providedSources.Add(wrapper.Uid, wrapper.Source);
					}
					wrapper.Source.mute = isMuted;
				}
				if (Caching && Singleton<GlobalCache>.Instance.Clips.ContainsKey(wrapper))
				{
					if (Config.DEBUG)
					{
						Debug.Log($"Wrapper CACHED: {wrapper}", this);
					}
					Context.NumberOfCachedSpeeches++;
					StartCoroutine(voiceProvider.SpeakWithClip(wrapper, Singleton<GlobalCache>.Instance.GetClip(wrapper)));
				}
				else if (MaxSimultaneousSpeeches == 0 || realSpeechCount <= MaxSimultaneousSpeeches)
				{
					realSpeechCount++;
					if (Config.DEBUG)
					{
						Debug.Log($"Wrapper NOT cached: {wrapper}", this);
					}
					Context.NumberOfNonCachedSpeeches++;
					StartCoroutine(voiceProvider.Speak(wrapper));
				}
				else
				{
					Debug.LogWarning($"Maximum number of simultaneous speeches ({MaxSimultaneousSpeeches}) exceeded! Please wait for the speeches to complete or stop at least one of them.");
				}
			}
			else
			{
				logVPIsNull();
			}
		}
		else
		{
			logWrapperIsNull();
		}
	}

	public string Speak(string text, AudioSource source = null, Voice voice = null, bool speakImmediately = true, float rate = 1f, float pitch = 1f, float volume = 1f, string outputFile = "", bool forceSSML = true)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return "disabled";
		}
		Wrapper wrapper = new Wrapper(text, voice, rate, pitch, volume, source, speakImmediately, outputFile, forceSSML);
		SpeakWithUID(wrapper);
		return wrapper.Uid;
	}

	public string Speak(Wrapper wrapper)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return "disabled";
		}
		if (wrapper != null)
		{
			SpeakWithUID(wrapper);
			return wrapper.Uid;
		}
		logWrapperIsNull();
		return string.Empty;
	}

	public void SpeakMarkedWordsWithUID(Wrapper wrapper)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return;
		}
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.LogWarning($"SpeakMarkedWordsWithUID called: {wrapper}", this);
		}
		if (voiceProvider != null)
		{
			if (string.IsNullOrEmpty(wrapper.Text))
			{
				Debug.LogWarning("'wrapper.Text' is null or empty!", this);
				return;
			}
			if (wrapper.Source == null || wrapper.Source.clip == null)
			{
				Debug.LogError("'wrapper.Source' must be a valid AudioSource with a clip! Use 'Speak()' before!", this);
				return;
			}
			BusyCount++;
			wrapper.SpeakImmediately = true;
			if (!BaseHelper.isMacOSPlatform && !BaseHelper.isWSABasedPlatform && !CustomMode)
			{
				wrapper.Volume = 0f;
				wrapper.Source.PlayDelayed(0.1f);
			}
			SpeakNativeWithUID(wrapper);
		}
		else
		{
			logVPIsNull();
		}
	}

	public void SpeakMarkedWordsWithUID(string uid, string text, AudioSource source, Voice voice = null, float rate = 1f, float pitch = 1f, bool forceSSML = true)
	{
		SpeakMarkedWordsWithUID(new Wrapper(uid, text, voice, rate, pitch, 0f, source, speakImmediately: true, "", forceSSML));
	}

	public string Generate(Wrapper wrapper)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return "disabled";
		}
		if (wrapper != null)
		{
			if (!BaseHelper.isEditorMode)
			{
				if (voiceProvider != null)
				{
					if (string.IsNullOrEmpty(wrapper.Text))
					{
						Debug.LogWarning("'wrapper.Text' is null or empty! Can't generate audio file.", this);
					}
					else if (string.IsNullOrEmpty(wrapper.OutputFile))
					{
						Debug.LogWarning("'wrapper.OutputFile' is null or empty! Can't generate audio file.", this);
					}
					else if (MaxSimultaneousSpeeches == 0 || realSpeechCount <= MaxSimultaneousSpeeches)
					{
						realSpeechCount++;
						StartCoroutine(voiceProvider.Generate(wrapper));
					}
					else
					{
						Debug.LogWarning($"Maximum number of simultaneous speeches ({MaxSimultaneousSpeeches}) exceeded! Please wait for the speeches to complete or stop at least one of them.");
					}
					return wrapper.Uid;
				}
				logVPIsNull();
			}
		}
		else
		{
			logWrapperIsNull();
		}
		return string.Empty;
	}

	public string Generate(string text, string outputFile, Voice voice = null, float rate = 1f, float pitch = 1f, float volume = 1f, bool forceSSML = true)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return "disabled";
		}
		Wrapper wrapper = new Wrapper(text, voice, rate, pitch, volume, null, speakImmediately: false, outputFile, forceSSML);
		return Generate(wrapper);
	}

	public void Silence(string uid = null)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return;
		}
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.Log("Silence called: " + uid, this);
		}
		if (voiceProvider != null)
		{
			if (string.IsNullOrEmpty(uid))
			{
				silence();
			}
			else if (genericSources.ContainsKey(uid))
			{
				if (genericSources.TryGetValue(uid, out var value))
				{
					value.Stop();
				}
			}
			else if (providedSources.ContainsKey(uid))
			{
				if (providedSources.TryGetValue(uid, out var value2))
				{
					value2.Stop();
				}
			}
			else
			{
				voiceProvider.Silence(uid);
			}
		}
		else
		{
			logVPIsNull();
		}
	}

	public void Pause(string uid = null)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return;
		}
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.LogWarning("Pause called: " + uid, this);
		}
		isPaused = true;
		if (voiceProvider != null)
		{
			if (string.IsNullOrEmpty(uid))
			{
				foreach (KeyValuePair<string, AudioSource> item in genericSources.Where((KeyValuePair<string, AudioSource> source) => source.Value != null))
				{
					item.Value.Pause();
				}
				{
					foreach (KeyValuePair<string, AudioSource> item2 in providedSources.Where((KeyValuePair<string, AudioSource> source) => source.Value != null))
					{
						item2.Value.Pause();
					}
					return;
				}
			}
			if (genericSources.ContainsKey(uid))
			{
				if (genericSources.TryGetValue(uid, out var value))
				{
					value.Pause();
				}
			}
			else if (providedSources.ContainsKey(uid))
			{
				if (providedSources.TryGetValue(uid, out var value2))
				{
					value2.Pause();
				}
			}
			else
			{
				Debug.Log("No AudioSource for uid found: " + uid, this);
			}
		}
		else
		{
			logVPIsNull();
		}
	}

	public void UnPause(string uid = null)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return;
		}
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.LogWarning("UnPause called: " + uid, this);
		}
		isPaused = false;
		if (voiceProvider != null)
		{
			if (string.IsNullOrEmpty(uid))
			{
				foreach (KeyValuePair<string, AudioSource> item in genericSources.Where((KeyValuePair<string, AudioSource> source) => source.Value != null))
				{
					item.Value.UnPause();
				}
				{
					foreach (KeyValuePair<string, AudioSource> item2 in providedSources.Where((KeyValuePair<string, AudioSource> source) => source.Value != null))
					{
						item2.Value.UnPause();
					}
					return;
				}
			}
			if (genericSources.ContainsKey(uid))
			{
				if (genericSources.TryGetValue(uid, out var value))
				{
					value.UnPause();
				}
			}
			else if (providedSources.ContainsKey(uid))
			{
				if (providedSources.TryGetValue(uid, out var value2))
				{
					value2.UnPause();
				}
			}
			else
			{
				Debug.Log("No AudioSource for uid found: " + uid, this);
			}
		}
		else
		{
			logVPIsNull();
		}
	}

	public void PauseOrUnPause(string uid = null)
	{
		if (!(this != null) || base.isActiveAndEnabled)
		{
			if (isPaused)
			{
				UnPause(uid);
			}
			else
			{
				Pause(uid);
			}
		}
	}

	public void Mute(string uid = null)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return;
		}
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.LogWarning("Mute called: " + uid, this);
		}
		isMuted = true;
		if (voiceProvider != null)
		{
			if (string.IsNullOrEmpty(uid))
			{
				foreach (KeyValuePair<string, AudioSource> item in genericSources.Where((KeyValuePair<string, AudioSource> source) => source.Value != null))
				{
					item.Value.mute = true;
				}
				{
					foreach (KeyValuePair<string, AudioSource> item2 in providedSources.Where((KeyValuePair<string, AudioSource> source) => source.Value != null))
					{
						item2.Value.mute = true;
					}
					return;
				}
			}
			if (genericSources.ContainsKey(uid))
			{
				if (genericSources.TryGetValue(uid, out var value))
				{
					value.mute = true;
				}
			}
			else if (providedSources.ContainsKey(uid))
			{
				if (providedSources.TryGetValue(uid, out var value2))
				{
					value2.mute = true;
				}
			}
			else
			{
				Debug.Log("No AudioSource for uid found: " + uid, this);
			}
		}
		else
		{
			logVPIsNull();
		}
	}

	public void UnMute(string uid = null)
	{
		if (this != null && !base.isActiveAndEnabled)
		{
			return;
		}
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.LogWarning("UnMute called: " + uid, this);
		}
		isMuted = false;
		if (voiceProvider != null)
		{
			if (string.IsNullOrEmpty(uid))
			{
				foreach (KeyValuePair<string, AudioSource> item in genericSources.Where((KeyValuePair<string, AudioSource> source) => source.Value != null))
				{
					item.Value.mute = false;
				}
				{
					foreach (KeyValuePair<string, AudioSource> item2 in providedSources.Where((KeyValuePair<string, AudioSource> source) => source.Value != null))
					{
						item2.Value.mute = false;
					}
					return;
				}
			}
			if (genericSources.ContainsKey(uid))
			{
				if (genericSources.TryGetValue(uid, out var value))
				{
					value.mute = false;
				}
			}
			else if (providedSources.ContainsKey(uid))
			{
				if (providedSources.TryGetValue(uid, out var value2))
				{
					value2.mute = false;
				}
			}
			else
			{
				Debug.Log("No AudioSource for uid found: " + uid, this);
			}
		}
		else
		{
			logVPIsNull();
		}
	}

	public void MuteOrUnMute(string uid = null)
	{
		if (isMuted)
		{
			UnMute(uid);
		}
		else
		{
			Mute(uid);
		}
	}

	public void ReloadProvider()
	{
		if (!(this != null) || base.isActiveAndEnabled)
		{
			Silence();
			initProvider();
		}
	}

	public void DeleteAudioFiles()
	{
		string path = Application.temporaryCachePath;
		deleteWorker.CTAbort();
		deleteWorker = new Thread((ThreadStart)delegate
		{
			deleteAudioFiles(path);
		});
		deleteWorker.Start();
	}

	private void silence()
	{
		if (BaseConstants.DEV_DEBUG)
		{
			Debug.Log("Silence called", this);
		}
		if (voiceProvider != null)
		{
			voiceProvider.Silence();
			foreach (KeyValuePair<string, AudioSource> item in genericSources.Where((KeyValuePair<string, AudioSource> source) => source.Value != null))
			{
				item.Value.Stop();
				UnityEngine.Object.Destroy(item.Value, 0.1f);
			}
			genericSources.Clear();
			foreach (KeyValuePair<string, AudioSource> item2 in providedSources.Where((KeyValuePair<string, AudioSource> source) => source.Value != null))
			{
				item2.Value.Stop();
			}
		}
		else
		{
			providedSources.Clear();
			if (!BaseHelper.isEditorMode)
			{
				logVPIsNull();
			}
		}
		SpeechCount = 0;
		BusyCount = 0;
		realSpeechCount = 0;
	}

	private void deleteAudioFiles(string audioDataPath)
	{
		try
		{
			System.Random random = new System.Random();
			string searchPattern = Constants.AUDIOFILE_PREFIX + "*";
			string[] files = Directory.GetFiles((BaseHelper.isAndroidPlatform || BaseHelper.isWSABasedPlatform) ? FileHelper.ValidatePath(audioDataPath) : Config.AUDIOFILE_PATH, searchPattern);
			foreach (string text in files)
			{
				try
				{
					if (BaseHelper.isWindowsPlatform)
					{
						Thread.Sleep(random.Next(1200, 1800));
					}
					File.Delete(text);
				}
				catch (Exception arg)
				{
					if (!BaseHelper.isEditor)
					{
						Debug.LogWarning($"Could not delete the file '{text}': {arg}", this);
					}
				}
			}
		}
		catch (Exception arg2)
		{
			if (!BaseHelper.isEditor)
			{
				Debug.LogWarning($"Could not scan the path for files: {arg2}", this);
			}
		}
	}

	private void initProvider()
	{
		unsubscribeEvents();
		areVoicesReady = false;
		enforcedStandaloneTTS = false;
		bool flag = CustomProvider != null && CustomMode && CustomProvider.enabled;
		if (flag)
		{
			if (CustomProvider.isPlatformSupported)
			{
				subscribeCustomEvents();
				voiceProvider = (customVoiceProvider = CustomProvider);
				mainVoiceProvider = null;
				CustomProvider.Load();
			}
			else
			{
				Debug.LogWarning("'Custom Provider' does not support the current platform!", this);
				flag = false;
			}
		}
		if (!flag)
		{
			unsubscribeCustomEvents();
			customVoiceProvider = null;
			initOSProvider();
			subscribeEvents();
			voiceProvider?.Load();
			onProviderChange();
		}
	}

	private void initOSProvider()
	{
		if ((!BaseHelper.isMacOSEditor && !BaseHelper.isLinuxEditor && BaseHelper.isWindowsPlatform && !eSpeakMode) || (BaseHelper.isWindowsEditor && Config.ENFORCE_STANDALONE_TTS && !eSpeakMode))
		{
			enforcedStandaloneTTS = !BaseHelper.isWindowsPlatform && BaseHelper.isWindowsEditor && Config.ENFORCE_STANDALONE_TTS;
			voiceProvider = (mainVoiceProvider = BaseVoiceProvider<VoiceProviderWindows>.Instance);
		}
		else if ((!BaseHelper.isWindowsEditor && !BaseHelper.isLinuxEditor && BaseHelper.isMacOSPlatform && !eSpeakMode) || (BaseHelper.isMacOSEditor && Config.ENFORCE_STANDALONE_TTS && !eSpeakMode))
		{
			enforcedStandaloneTTS = !BaseHelper.isMacOSPlatform && BaseHelper.isMacOSEditor && Config.ENFORCE_STANDALONE_TTS;
		}
		else if (eSpeakMode && VoiceProviderLinux.isSupported)
		{
			voiceProvider = (mainVoiceProvider = BaseVoiceProvider<VoiceProviderLinux>.Instance);
		}
		else if (!BaseHelper.isAndroidPlatform && !BaseHelper.isIOSBasedPlatform)
		{
			Debug.LogError("No valid TTS provider found!", this);
			voiceProvider = (mainVoiceProvider = null);
		}
	}

	private void logWrapperIsNull()
	{
		onErrorInfo(null, "'wrapper' is null!");
		Debug.LogError("'wrapper' is null!", this);
	}

	private void logVPIsNull()
	{
		string text = "'voiceProvider' is null!" + Environment.NewLine + "Did you add the 'RTVoice'-prefab to the current scene?";
		onErrorInfo(null, text);
		if (!loggedVPIsNull && !BaseHelper.isEditorMode)
		{
			Debug.LogWarning(text, this);
			loggedVPIsNull = true;
		}
	}

	private void subscribeCustomEvents()
	{
		if (CustomProvider != null)
		{
			CustomProvider.isActive = true;
			CustomProvider.OnVoicesReady += onVoicesReady;
			CustomProvider.OnSpeakStart += onSpeakStart;
			CustomProvider.OnSpeakComplete += onSpeakComplete;
			CustomProvider.OnSpeakCurrentWord += onSpeakCurrentWord;
			CustomProvider.OnSpeakCurrentWordString += onSpeakCurrentWordString;
			CustomProvider.OnSpeakCurrentPhoneme += onSpeakCurrentPhoneme;
			CustomProvider.OnSpeakCurrentViseme += onSpeakCurrentViseme;
			CustomProvider.OnSpeakAudioGenerationStart += onSpeakAudioGenerationStart;
			CustomProvider.OnSpeakAudioGenerationComplete += onSpeakAudioGenerationComplete;
			CustomProvider.OnErrorInfo += onErrorInfo;
		}
	}

	private void unsubscribeCustomEvents()
	{
		if (CustomProvider != null)
		{
			CustomProvider.isActive = false;
			CustomProvider.OnVoicesReady -= onVoicesReady;
			CustomProvider.OnSpeakStart -= onSpeakStart;
			CustomProvider.OnSpeakComplete -= onSpeakComplete;
			CustomProvider.OnSpeakCurrentWord -= onSpeakCurrentWord;
			CustomProvider.OnSpeakCurrentWordString -= onSpeakCurrentWordString;
			CustomProvider.OnSpeakCurrentPhoneme -= onSpeakCurrentPhoneme;
			CustomProvider.OnSpeakCurrentViseme -= onSpeakCurrentViseme;
			CustomProvider.OnSpeakAudioGenerationStart -= onSpeakAudioGenerationStart;
			CustomProvider.OnSpeakAudioGenerationComplete -= onSpeakAudioGenerationComplete;
			CustomProvider.OnErrorInfo -= onErrorInfo;
		}
	}

	private void subscribeEvents()
	{
		if (mainVoiceProvider != null)
		{
			mainVoiceProvider.OnVoicesReady += onVoicesReady;
			mainVoiceProvider.OnSpeakStart += onSpeakStart;
			mainVoiceProvider.OnSpeakComplete += onSpeakComplete;
			mainVoiceProvider.OnSpeakCurrentWord += onSpeakCurrentWord;
			mainVoiceProvider.OnSpeakCurrentWordString += onSpeakCurrentWordString;
			mainVoiceProvider.OnSpeakCurrentPhoneme += onSpeakCurrentPhoneme;
			mainVoiceProvider.OnSpeakCurrentViseme += onSpeakCurrentViseme;
			mainVoiceProvider.OnSpeakAudioGenerationStart += onSpeakAudioGenerationStart;
			mainVoiceProvider.OnSpeakAudioGenerationComplete += onSpeakAudioGenerationComplete;
			mainVoiceProvider.OnErrorInfo += onErrorInfo;
		}
		if (customVoiceProvider != null)
		{
			customVoiceProvider.OnVoicesReady += onVoicesReady;
			customVoiceProvider.OnSpeakStart += onSpeakStart;
			customVoiceProvider.OnSpeakComplete += onSpeakComplete;
			customVoiceProvider.OnSpeakCurrentWord += onSpeakCurrentWord;
			customVoiceProvider.OnSpeakCurrentWordString += onSpeakCurrentWordString;
			customVoiceProvider.OnSpeakCurrentPhoneme += onSpeakCurrentPhoneme;
			customVoiceProvider.OnSpeakCurrentViseme += onSpeakCurrentViseme;
			customVoiceProvider.OnSpeakAudioGenerationStart += onSpeakAudioGenerationStart;
			customVoiceProvider.OnSpeakAudioGenerationComplete += onSpeakAudioGenerationComplete;
			customVoiceProvider.OnErrorInfo += onErrorInfo;
		}
	}

	private void unsubscribeEvents()
	{
		if (mainVoiceProvider != null)
		{
			mainVoiceProvider.OnVoicesReady -= onVoicesReady;
			mainVoiceProvider.OnSpeakStart -= onSpeakStart;
			mainVoiceProvider.OnSpeakComplete -= onSpeakComplete;
			mainVoiceProvider.OnSpeakCurrentWord -= onSpeakCurrentWord;
			mainVoiceProvider.OnSpeakCurrentWordString -= onSpeakCurrentWordString;
			mainVoiceProvider.OnSpeakCurrentPhoneme -= onSpeakCurrentPhoneme;
			mainVoiceProvider.OnSpeakCurrentViseme -= onSpeakCurrentViseme;
			mainVoiceProvider.OnSpeakAudioGenerationStart -= onSpeakAudioGenerationStart;
			mainVoiceProvider.OnSpeakAudioGenerationComplete -= onSpeakAudioGenerationComplete;
			mainVoiceProvider.OnErrorInfo -= onErrorInfo;
		}
		if (customVoiceProvider != null)
		{
			customVoiceProvider.OnVoicesReady -= onVoicesReady;
			customVoiceProvider.OnSpeakStart -= onSpeakStart;
			customVoiceProvider.OnSpeakComplete -= onSpeakComplete;
			customVoiceProvider.OnSpeakCurrentWord -= onSpeakCurrentWord;
			customVoiceProvider.OnSpeakCurrentWordString -= onSpeakCurrentWordString;
			customVoiceProvider.OnSpeakCurrentPhoneme -= onSpeakCurrentPhoneme;
			customVoiceProvider.OnSpeakCurrentViseme -= onSpeakCurrentViseme;
			customVoiceProvider.OnSpeakAudioGenerationStart -= onSpeakAudioGenerationStart;
			customVoiceProvider.OnSpeakAudioGenerationComplete -= onSpeakAudioGenerationComplete;
			customVoiceProvider.OnErrorInfo -= onErrorInfo;
		}
	}

	private void onVoicesReady()
	{
		areVoicesReady = true;
		if (!BaseHelper.isEditorMode)
		{
			OnReady?.Invoke();
		}
		this.OnVoicesReady?.Invoke();
	}

	private void onProviderChange()
	{
		if (!BaseHelper.isEditorMode)
		{
			OnProviderChanged?.Invoke(voiceProvider?.GetType().ToString());
		}
		this.OnProviderChange?.Invoke(voiceProvider?.GetType().ToString());
	}

	private void onSpeakStart(Wrapper wrapper)
	{
		if (!BaseHelper.isEditorMode)
		{
			OnSpeakStarted?.Invoke(wrapper?.Uid);
		}
		this.OnSpeakStart?.Invoke(wrapper);
		SpeechCount++;
	}

	private void onSpeakComplete(Wrapper wrapper)
	{
		if (!BaseHelper.isEditorMode)
		{
			OnSpeakCompleted?.Invoke(wrapper?.Uid);
		}
		this.OnSpeakComplete?.Invoke(wrapper);
		SpeechCount--;
		BusyCount--;
		realSpeechCount--;
		Context.NumberOfSpeeches++;
		Context.TotalSpeechLength += wrapper.SpeechTime;
		Context.NumberOfCharacters += wrapper.Text.Length;
	}

	private void onSpeakCurrentWord(Wrapper wrapper, string[] speechTextArray, int wordIndex)
	{
		this.OnSpeakCurrentWord?.Invoke(wrapper, speechTextArray, wordIndex);
	}

	private void onSpeakCurrentWordString(Wrapper wrapper, string word)
	{
		this.OnSpeakCurrentWordString?.Invoke(wrapper, word);
	}

	private void onSpeakCurrentPhoneme(Wrapper wrapper, string phoneme)
	{
		this.OnSpeakCurrentPhoneme?.Invoke(wrapper, phoneme);
	}

	private void onSpeakCurrentViseme(Wrapper wrapper, string viseme)
	{
		this.OnSpeakCurrentViseme?.Invoke(wrapper, viseme);
	}

	private void onSpeakAudioGenerationStart(Wrapper wrapper)
	{
		this.OnSpeakAudioGenerationStart?.Invoke(wrapper);
	}

	private void onSpeakAudioGenerationComplete(Wrapper wrapper)
	{
		this.OnSpeakAudioGenerationComplete?.Invoke(wrapper);
		Context.NumberOfAudioFiles++;
		Context.TotalSpeechLength += wrapper.SpeechTime;
		Context.NumberOfCharacters += wrapper.Text.Length;
	}

	private void onErrorInfo(Wrapper wrapper, string errorInfo)
	{
		if (!BaseHelper.isEditorMode)
		{
			OnError?.Invoke(wrapper?.Uid, errorInfo);
		}
		this.OnErrorInfo?.Invoke(wrapper, errorInfo);
	}
}
