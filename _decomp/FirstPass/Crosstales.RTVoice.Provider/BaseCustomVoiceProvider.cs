using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Util;
using UnityEngine;
using UnityEngine.Networking;

namespace Crosstales.RTVoice.Provider;

public abstract class BaseCustomVoiceProvider : MonoBehaviour, IVoiceProvider
{
	protected List<Voice> cachedVoices = new List<Voice>();

	private List<string> cachedCultures;

	protected bool silence;

	private bool isActive1;

	public bool isActive
	{
		get
		{
			return isActive1;
		}
		set
		{
			isActive1 = value;
		}
	}

	public abstract string AudioFileExtension { get; }

	public abstract AudioType AudioFileType { get; }

	public virtual string DefaultVoiceName => string.Empty;

	public virtual List<Voice> Voices => cachedVoices;

	public abstract bool isWorkingInEditor { get; }

	public abstract bool isWorkingInPlaymode { get; }

	public abstract int MaxTextLength { get; }

	public abstract bool isSpeakNativeSupported { get; }

	public abstract bool isSpeakSupported { get; }

	public abstract bool isPlatformSupported { get; }

	public abstract bool isSSMLSupported { get; }

	public abstract bool isOnlineService { get; }

	public abstract bool hasCoRoutines { get; }

	public abstract bool isIL2CPPSupported { get; }

	public abstract bool hasVoicesInEditor { get; }

	public List<string> Cultures
	{
		get
		{
			if (cachedCultures == null || cachedCultures.Count == 0)
			{
				cachedCultures = new List<string>();
				foreach (Voice item in (IEnumerable<Voice>)(from cul in Voices
					group cul by cul.Culture into grp
					select grp.First() into s
					orderby s.Culture
					select s).ToList())
				{
					cachedCultures.Add(item.Culture);
				}
			}
			return cachedCultures;
		}
	}

	public abstract int MaxSimultaneousSpeeches { get; }

	public event VoicesReady OnVoicesReady;

	public event SpeakStart OnSpeakStart;

	public event SpeakComplete OnSpeakComplete;

	public event SpeakCurrentWord OnSpeakCurrentWord;

	public event SpeakCurrentWordString OnSpeakCurrentWordString;

	public event SpeakCurrentPhoneme OnSpeakCurrentPhoneme;

	public event SpeakCurrentViseme OnSpeakCurrentViseme;

	public event SpeakAudioGenerationStart OnSpeakAudioGenerationStart;

	public event SpeakAudioGenerationComplete OnSpeakAudioGenerationComplete;

	public event ErrorInfo OnErrorInfo;

	protected virtual void Start()
	{
		if (isPlatformSupported)
		{
			if (isOnlineService && !NetworkHelper.isInternetAvailable)
			{
				Singleton<Speaker>.Instance.CustomMode = false;
				Debug.LogWarning("'" + GetType().Name + "' needs an Internet connection. Falling back to the default provider. If you want to automatically re-enable this provider as soon as a connection is available again, please consider installing 'Online Check': https://assetstore.unity.com/packages/slug/74688?aid=1011lNGT", this);
			}
		}
		else
		{
			Singleton<Speaker>.Instance.CustomMode = false;
			Debug.LogWarning("'" + GetType().Name + "' is not supported under the current build platform or Unity Editor. Falling back to the default provider.", this);
		}
	}

	protected virtual void OnDestroy()
	{
	}

	public virtual void Silence()
	{
		silence = true;
	}

	public virtual void Silence(string uid)
	{
	}

	public abstract IEnumerator SpeakNative(Wrapper wrapper);

	public abstract IEnumerator Speak(Wrapper wrapper);

	public abstract IEnumerator Generate(Wrapper wrapper);

	public virtual IEnumerator SpeakWithClip(Wrapper wrapper, AudioClip clip)
	{
		if (wrapper != null && wrapper.Source != null)
		{
			silence = false;
			onSpeakAudioGenerationStart(wrapper);
			wrapper.Source.clip = clip;
			yield return null;
			if (Config.DEBUG)
			{
				Debug.Log("Text generated: " + wrapper.Text);
			}
			onSpeakAudioGenerationComplete(wrapper);
			yield return null;
			if (wrapper.SpeakImmediately && wrapper.Source != null)
			{
				wrapper.Source.Play();
				onSpeakStart(wrapper);
				do
				{
					yield return null;
				}
				while (!silence && wrapper.Source.CTHasActiveClip());
				if (Config.DEBUG)
				{
					Debug.Log("Text spoken: " + wrapper.Text);
				}
				onSpeakComplete(wrapper);
			}
		}
		else
		{
			string text = "'Source' is null: " + wrapper;
			Debug.LogError(text);
			onErrorInfo(wrapper, text);
		}
	}

	public abstract void Load(bool forceReload = false);

	protected virtual string getOutputFile(string uid, bool isPersistentData = false)
	{
		string text = Constants.AUDIOFILE_PREFIX + uid + AudioFileExtension;
		if (isPersistentData)
		{
			return FileHelper.ValidatePath(Application.temporaryCachePath) + text;
		}
		return Config.AUDIOFILE_PATH + text;
	}

	protected virtual IEnumerator playAudioFile(Wrapper wrapper, AudioClip ac, bool isNative = false)
	{
		if (wrapper != null && wrapper.Source != null)
		{
			if (ac != null)
			{
				wrapper.Source.clip = ac;
				if (!isNative)
				{
					onSpeakAudioGenerationComplete(wrapper);
				}
				if ((isNative || wrapper.SpeakImmediately) && wrapper.Source != null)
				{
					wrapper.Source.Play();
					onSpeakStart(wrapper);
					do
					{
						yield return null;
					}
					while (!silence && wrapper.Source.CTHasActiveClip());
					if (Config.DEBUG)
					{
						Debug.Log("Text spoken: " + wrapper.Text, this);
					}
					onSpeakComplete(wrapper);
					if (ac != null && !Singleton<Speaker>.Instance.Caching)
					{
						UnityEngine.Object.Destroy(ac);
					}
				}
				if (ac != null && Singleton<Speaker>.Instance.Caching)
				{
					if (Config.DEBUG)
					{
						Debug.Log("Adding wrapper to clips-cache: " + wrapper);
					}
					Singleton<GlobalCache>.Instance.AddClip(wrapper, ac);
				}
			}
			else
			{
				string text = "The attached AudioClip is invalid: " + wrapper;
				Debug.LogError(text, this);
				onErrorInfo(wrapper, text);
			}
		}
		else
		{
			string text2 = "'Source' is null: " + wrapper;
			Debug.LogError(text2, this);
			onErrorInfo(wrapper, text2);
		}
	}

	protected virtual IEnumerator playAudioFile(Wrapper wrapper, string url, string outputFile, AudioType type = AudioType.WAV, bool isNative = false, bool isLocalFile = true, Dictionary<string, string> headers = null)
	{
		if (wrapper != null && wrapper.Source != null)
		{
			if (!isLocalFile || (isLocalFile && File.Exists(outputFile) && new FileInfo(outputFile).Length > 1024))
			{
				if (BaseHelper.isStandalonePlatform && type == AudioType.MPEG)
				{
					Debug.LogWarning("MP3 is not supported under the current platform!");
					yield break;
				}
				using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url.Trim(), type);
				if (headers != null)
				{
					foreach (KeyValuePair<string, string> header in headers)
					{
						www.SetRequestHeader(header.Key, header.Value);
					}
				}
				yield return www.SendWebRequest();
				if (!www.isHttpError && !www.isNetworkError)
				{
					AudioClip ac = DownloadHandlerAudioClip.GetContent(www);
					do
					{
						yield return ac;
					}
					while (ac != null && ac.loadState == AudioDataLoadState.Loading);
					if (ac != null && ac.loadState == AudioDataLoadState.Loaded)
					{
						wrapper.Source.clip = ac;
						if (Config.DEBUG)
						{
							Debug.Log("Text generated: " + wrapper.Text, this);
						}
						copyAudioFile(wrapper, outputFile, isLocalFile, www.downloadHandler.data);
						if (!isNative)
						{
							onSpeakAudioGenerationComplete(wrapper);
						}
						if (ac != null && Singleton<Speaker>.Instance.Caching)
						{
							if (Config.DEBUG)
							{
								Debug.Log($"Adding wrapper to clips-cache: {wrapper}", this);
							}
							Singleton<GlobalCache>.Instance.AddClip(wrapper, ac);
						}
						if ((isNative || wrapper.SpeakImmediately) && wrapper.Source != null)
						{
							wrapper.Source.Play();
							onSpeakStart(wrapper);
							do
							{
								yield return null;
							}
							while (!silence && wrapper.Source.CTHasActiveClip());
							if (Config.DEBUG)
							{
								Debug.Log("Text spoken: " + wrapper.Text, this);
							}
							onSpeakComplete(wrapper);
							if (ac != null && !Singleton<Speaker>.Instance.Caching)
							{
								UnityEngine.Object.Destroy(ac);
							}
						}
					}
					else
					{
						string text = $"Could not load the audio file from the speech: {wrapper}";
						Debug.LogError(text, this);
						onErrorInfo(wrapper, text);
					}
				}
				else
				{
					string text2 = $"Could not generate the speech: {wrapper} ({www.error})";
					Debug.LogError(text2, this);
					onErrorInfo(wrapper, text2);
				}
			}
			else
			{
				string text3 = $"The generated audio file is invalid: {wrapper}";
				Debug.LogError(text3, this);
				onErrorInfo(wrapper, text3);
			}
		}
		else
		{
			string text4 = $"'Source' is null: {wrapper}";
			Debug.LogError(text4, this);
			onErrorInfo(wrapper, text4);
		}
	}

	protected virtual void copyAudioFile(Wrapper wrapper, string outputFile, bool isLocalFile = true, byte[] data = null)
	{
		if (wrapper != null)
		{
			if (!string.IsNullOrEmpty(wrapper.OutputFile))
			{
				wrapper.OutputFile += AudioFileExtension;
				if (isLocalFile)
				{
					FileHelper.CopyFile(outputFile, wrapper.OutputFile, Config.AUDIOFILE_AUTOMATIC_DELETE);
				}
				else if (data != null)
				{
					try
					{
						File.WriteAllBytes(wrapper.OutputFile, data);
					}
					catch (Exception ex)
					{
						Debug.LogError("Could not write audio file!" + Environment.NewLine + ex, this);
					}
				}
			}
			if (Config.AUDIOFILE_AUTOMATIC_DELETE)
			{
				try
				{
					if (File.Exists(outputFile))
					{
						File.Delete(outputFile);
					}
					return;
				}
				catch (Exception ex2)
				{
					string text = "Could not delete file '" + outputFile + "'!" + Environment.NewLine + ex2;
					Debug.LogError(text, this);
					onErrorInfo(wrapper, text);
					return;
				}
			}
			if (string.IsNullOrEmpty(wrapper.OutputFile))
			{
				wrapper.OutputFile = outputFile;
			}
		}
		else
		{
			Debug.LogError("'wrapper' is null!", this);
			onErrorInfo(null, "'wrapper' is null!");
		}
	}

	protected virtual void processAudioFile(Wrapper wrapper, string outputFile, bool isLocalFile = true, byte[] data = null)
	{
		if (wrapper != null)
		{
			if (!isLocalFile || (isLocalFile && File.Exists(outputFile) && new FileInfo(outputFile).Length > 1024))
			{
				if (Config.DEBUG)
				{
					Debug.Log("Text generated: " + wrapper.Text, this);
				}
				copyAudioFile(wrapper, outputFile, isLocalFile, data);
				onSpeakAudioGenerationComplete(wrapper);
			}
			else
			{
				Debug.LogError("The generated audio file is invalid!", this);
				onErrorInfo(wrapper, "The generated audio file is invalid!");
			}
		}
		else
		{
			Debug.LogError("'wrapper' is null!", this);
			onErrorInfo(null, "'wrapper' is null!");
		}
	}

	protected virtual string getVoiceName(Wrapper wrapper)
	{
		if (wrapper != null && string.IsNullOrEmpty(wrapper.Voice?.Name))
		{
			if (Config.DEBUG)
			{
				Debug.LogWarning("'wrapper.Voice' or 'wrapper.Voice.Name' is null! Using the providers 'default' voice.", this);
			}
			return DefaultVoiceName;
		}
		if (wrapper == null)
		{
			return DefaultVoiceName;
		}
		return wrapper.Voice?.Name;
	}

	protected static string getValidXML(string xml)
	{
		if (string.IsNullOrEmpty(xml))
		{
			return xml;
		}
		return xml.Replace(" & ", " &amp; ").Replace(" < ", " &lt; ").Replace(" > ", " &gt; ");
	}

	protected void onVoicesReady()
	{
		if (Config.DEBUG)
		{
			Debug.Log("onVoicesReady", this);
		}
		cachedCultures = null;
		this.OnVoicesReady?.Invoke();
	}

	protected void onSpeakStart(Wrapper wrapper)
	{
		if (Config.DEBUG)
		{
			Debug.Log("onSpeakStart: " + wrapper, this);
		}
		this.OnSpeakStart?.Invoke(wrapper);
	}

	protected void onSpeakComplete(Wrapper wrapper)
	{
		if (Config.DEBUG)
		{
			Debug.Log("onSpeakComplete: " + wrapper, this);
		}
		this.OnSpeakComplete?.Invoke(wrapper);
	}

	protected void onSpeakCurrentWord(Wrapper wrapper, string[] speechTextArray, int wordIndex)
	{
		if (wordIndex < speechTextArray.Length)
		{
			if (Config.DEBUG)
			{
				Debug.Log("onSpeakCurrentWord: " + speechTextArray[wordIndex] + Environment.NewLine + wrapper, this);
			}
			this.OnSpeakCurrentWord?.Invoke(wrapper, speechTextArray, wordIndex);
		}
		else
		{
			Debug.LogWarning("Word index is larger than the speech text word count: " + wordIndex + "/" + speechTextArray.Length, this);
		}
	}

	protected void onSpeakCurrentWord(Wrapper wrapper, string word)
	{
		if (Config.DEBUG)
		{
			Debug.Log("onSpeakCurrentWord: " + word + Environment.NewLine + wrapper);
		}
		this.OnSpeakCurrentWordString?.Invoke(wrapper, word);
	}

	protected void onSpeakCurrentPhoneme(Wrapper wrapper, string phoneme)
	{
		if (Config.DEBUG)
		{
			Debug.Log("onSpeakCurrentPhoneme: " + phoneme + Environment.NewLine + wrapper, this);
		}
		this.OnSpeakCurrentPhoneme?.Invoke(wrapper, phoneme);
	}

	protected void onSpeakCurrentViseme(Wrapper wrapper, string viseme)
	{
		if (Config.DEBUG)
		{
			Debug.Log("onSpeakCurrentViseme: " + viseme + Environment.NewLine + wrapper, this);
		}
		this.OnSpeakCurrentViseme?.Invoke(wrapper, viseme);
	}

	protected void onSpeakAudioGenerationStart(Wrapper wrapper)
	{
		if (Config.DEBUG)
		{
			Debug.Log("onSpeakAudioGenerationStart: " + wrapper, this);
		}
		this.OnSpeakAudioGenerationStart?.Invoke(wrapper);
	}

	protected void onSpeakAudioGenerationComplete(Wrapper wrapper)
	{
		if (Config.DEBUG)
		{
			Debug.Log("onSpeakAudioGenerationComplete: " + wrapper, this);
		}
		this.OnSpeakAudioGenerationComplete?.Invoke(wrapper);
	}

	protected void onErrorInfo(Wrapper wrapper, string info)
	{
		if (Config.DEBUG)
		{
			Debug.Log("onErrorInfo: " + info, this);
		}
		this.OnErrorInfo?.Invoke(wrapper, info);
	}
}
