using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Util;
using UnityEngine;
using UnityEngine.Networking;

namespace Crosstales.RTVoice.Provider;

public abstract class MainVoiceProvider : IVoiceProvider
{
	protected List<Voice> cachedVoices = new List<Voice>();

	private List<string> cachedCultures;

	protected readonly Dictionary<string, Process> processes = new Dictionary<string, Process>();

	protected bool silence;

	protected static readonly char[] splitCharWords = new char[1] { ' ' };

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

	public virtual void Silence()
	{
		silence = true;
		foreach (KeyValuePair<string, Process> item in processes.Where(delegate(KeyValuePair<string, Process> kvp)
		{
			Process value = kvp.Value;
			return value != null && !value.HasExited;
		}))
		{
			item.Value.Kill();
		}
		processes.Clear();
	}

	public virtual void Silence(string uid)
	{
		if (!string.IsNullOrEmpty(uid) && processes.ContainsKey(uid))
		{
			if (processes[uid] != null && !processes[uid].HasExited)
			{
				processes[uid].Kill();
			}
			processes.Remove(uid);
		}
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
				UnityEngine.Debug.Log("Text generated: " + wrapper.Text);
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
					UnityEngine.Debug.Log("Text spoken: " + wrapper.Text);
				}
				onSpeakComplete(wrapper);
			}
		}
		else
		{
			string text = "'Source' is null: " + wrapper;
			UnityEngine.Debug.LogError(text);
			onErrorInfo(wrapper, text);
		}
	}

	public abstract void Load(bool forceReload = false);

	protected static void startProcess(Process process, int timeout = 0, bool eventOutputData = false, bool eventErrorData = false, bool redirectOutputData = true, bool redirectErrorData = true)
	{
		try
		{
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.RedirectStandardOutput = redirectOutputData;
			process.StartInfo.RedirectStandardError = redirectErrorData;
			process.StartInfo.UseShellExecute = false;
			if (redirectOutputData)
			{
				process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
			}
			if (redirectErrorData)
			{
				process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
			}
			process.Start();
			if (eventOutputData)
			{
				process.BeginOutputReadLine();
			}
			if (eventErrorData)
			{
				process.BeginErrorReadLine();
			}
			if (timeout > 0)
			{
				process.WaitForExit(timeout);
			}
			else
			{
				process.WaitForExit();
			}
		}
		catch (ThreadAbortException)
		{
		}
		catch (Exception ex2)
		{
			UnityEngine.Debug.LogError("Could not start process: " + ex2);
		}
	}

	protected virtual string getOutputFile(string uid, bool isPersistentData = false)
	{
		string text = Constants.AUDIOFILE_PREFIX + uid + AudioFileExtension;
		if (isPersistentData)
		{
			return FileHelper.ValidatePath(Application.temporaryCachePath) + text;
		}
		return Config.AUDIOFILE_PATH + text;
	}

	protected virtual IEnumerator playAudioFile(Wrapper wrapper, string url, string outputFile, AudioType type = AudioType.WAV, bool isNative = false, bool isLocalFile = true, Dictionary<string, string> headers = null)
	{
		if (wrapper != null && wrapper.Source != null)
		{
			if (!isLocalFile || (isLocalFile && File.Exists(outputFile) && new FileInfo(outputFile).Length > 1024))
			{
				if (BaseHelper.isStandalonePlatform && type == AudioType.MPEG)
				{
					UnityEngine.Debug.LogWarning("MP3 is not supported under the current platform!");
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
							UnityEngine.Debug.Log("Text generated: " + wrapper.Text);
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
								UnityEngine.Debug.Log($"Adding wrapper to clips-cache: {wrapper}");
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
								UnityEngine.Debug.Log("Text spoken: " + wrapper.Text);
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
						UnityEngine.Debug.LogError(text);
						onErrorInfo(wrapper, text);
					}
				}
				else
				{
					string text2 = $"Could not generate the speech: {wrapper} ({www.error})";
					UnityEngine.Debug.LogError(text2);
					onErrorInfo(wrapper, text2);
				}
			}
			else
			{
				string text3 = $"The generated audio file is invalid: {wrapper}";
				UnityEngine.Debug.LogError(text3);
				onErrorInfo(wrapper, text3);
			}
		}
		else
		{
			string text4 = $"'Source' is null: {wrapper}";
			UnityEngine.Debug.LogError(text4);
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
						UnityEngine.Debug.LogError("Could not write audio file!" + Environment.NewLine + ex);
					}
				}
			}
			if (Config.AUDIOFILE_AUTOMATIC_DELETE && !BaseHelper.isWindowsPlatform)
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
					UnityEngine.Debug.LogError(text);
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
			UnityEngine.Debug.LogError("'wrapper' is null!");
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
					UnityEngine.Debug.Log("Text generated: " + wrapper.Text);
				}
				copyAudioFile(wrapper, outputFile, isLocalFile, data);
				onSpeakAudioGenerationComplete(wrapper);
			}
			else
			{
				UnityEngine.Debug.LogError("The generated audio file is invalid!");
				onErrorInfo(wrapper, "The generated audio file is invalid!");
			}
		}
		else
		{
			UnityEngine.Debug.LogError("'wrapper' is null!");
			onErrorInfo(null, "'wrapper' is null!");
		}
	}

	protected virtual string getVoiceName(Wrapper wrapper)
	{
		if (wrapper != null && string.IsNullOrEmpty(wrapper.Voice?.Name))
		{
			if (Config.DEBUG)
			{
				UnityEngine.Debug.LogWarning("'wrapper.Voice' or 'wrapper.Voice.Name' is null! Using the providers 'default' voice.");
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
			UnityEngine.Debug.Log("onVoicesReady");
		}
		cachedCultures = null;
		this.OnVoicesReady?.Invoke();
	}

	protected void onSpeakStart(Wrapper wrapper)
	{
		if (Config.DEBUG)
		{
			UnityEngine.Debug.Log("onSpeakStart: " + wrapper);
		}
		this.OnSpeakStart?.Invoke(wrapper);
	}

	protected void onSpeakComplete(Wrapper wrapper)
	{
		if (Config.DEBUG)
		{
			UnityEngine.Debug.Log("onSpeakComplete: " + wrapper);
		}
		this.OnSpeakComplete?.Invoke(wrapper);
	}

	protected void onSpeakCurrentWord(Wrapper wrapper, string[] speechTextArray, int wordIndex)
	{
		if (wordIndex < speechTextArray.Length)
		{
			if (Config.DEBUG)
			{
				UnityEngine.Debug.Log("onSpeakCurrentWord: " + speechTextArray[wordIndex] + Environment.NewLine + wrapper);
			}
			this.OnSpeakCurrentWord?.Invoke(wrapper, speechTextArray, wordIndex);
		}
		else
		{
			UnityEngine.Debug.LogWarning("Word index is larger than the speech text word count: " + wordIndex + "/" + speechTextArray.Length);
		}
	}

	protected void onSpeakCurrentWord(Wrapper wrapper, string word)
	{
		if (Config.DEBUG)
		{
			UnityEngine.Debug.Log("onSpeakCurrentWord: " + word + Environment.NewLine + wrapper);
		}
		this.OnSpeakCurrentWordString?.Invoke(wrapper, word);
	}

	protected void onSpeakCurrentPhoneme(Wrapper wrapper, string phoneme)
	{
		if (Config.DEBUG)
		{
			UnityEngine.Debug.Log("onSpeakCurrentPhoneme: " + phoneme + Environment.NewLine + wrapper);
		}
		this.OnSpeakCurrentPhoneme?.Invoke(wrapper, phoneme);
	}

	protected void onSpeakCurrentViseme(Wrapper wrapper, string viseme)
	{
		if (Config.DEBUG)
		{
			UnityEngine.Debug.Log("onSpeakCurrentViseme: " + viseme + Environment.NewLine + wrapper);
		}
		this.OnSpeakCurrentViseme?.Invoke(wrapper, viseme);
	}

	protected void onSpeakAudioGenerationStart(Wrapper wrapper)
	{
		if (Config.DEBUG)
		{
			UnityEngine.Debug.Log("onSpeakAudioGenerationStart: " + wrapper);
		}
		this.OnSpeakAudioGenerationStart?.Invoke(wrapper);
	}

	protected void onSpeakAudioGenerationComplete(Wrapper wrapper)
	{
		if (Config.DEBUG)
		{
			UnityEngine.Debug.Log("onSpeakAudioGenerationComplete: " + wrapper);
		}
		this.OnSpeakAudioGenerationComplete?.Invoke(wrapper);
	}

	protected void onErrorInfo(Wrapper wrapper, string info)
	{
		if (Config.DEBUG)
		{
			UnityEngine.Debug.Log("onErrorInfo: " + info);
		}
		this.OnErrorInfo?.Invoke(wrapper, info);
	}
}
