using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using Crosstales.RTVoice.Util;
using UnityEngine;

namespace Crosstales.RTVoice.Provider;

public class VoiceProviderLinux : BaseVoiceProvider<VoiceProviderLinux>
{
	private const int defaultRate = 160;

	private const int defaultVolume = 100;

	private const int defaultPitch = 50;

	private readonly List<Voice> voices = new List<Voice>(100);

	private bool isLoading;

	public override string AudioFileExtension => ".wav";

	public override AudioType AudioFileType => AudioType.WAV;

	public override bool isWorkingInEditor => true;

	public override bool isWorkingInPlaymode => true;

	public override int MaxTextLength => 32000;

	public override bool isSpeakNativeSupported => true;

	public override bool isSpeakSupported => true;

	public override bool isPlatformSupported => isSupported;

	public static bool isSupported
	{
		get
		{
			if (!BaseHelper.isWindowsPlatform && !BaseHelper.isMacOSPlatform)
			{
				return BaseHelper.isLinuxPlatform;
			}
			return true;
		}
	}

	public override bool isSSMLSupported => true;

	public override bool isOnlineService => false;

	public override bool hasCoRoutines => true;

	public override bool isIL2CPPSupported => true;

	public override bool hasVoicesInEditor => true;

	public override int MaxSimultaneousSpeeches => 0;

	public override void Load(bool forceReload = false)
	{
		List<Voice> list = cachedVoices;
		if ((list != null && list.Count == 0) || forceReload)
		{
			if (!BaseHelper.isEditorMode && !isLoading)
			{
				isLoading = true;
				Singleton<Speaker>.Instance.StartCoroutine(getVoices());
			}
		}
		else
		{
			onVoicesReady();
		}
	}

	public override IEnumerator SpeakNative(Wrapper wrapper)
	{
		if (wrapper == null)
		{
			UnityEngine.Debug.LogWarning("'wrapper' is null!");
			yield break;
		}
		if (string.IsNullOrEmpty(wrapper.Text))
		{
			UnityEngine.Debug.LogWarning("'wrapper.Text' is null or empty: " + wrapper);
			yield break;
		}
		yield return null;
		string voiceName = getVoiceName(wrapper);
		int num = calculateRate(wrapper.Rate);
		int num2 = calculateVolume(wrapper.Volume);
		int num3 = calculatePitch(wrapper.Pitch);
		string text = (string.IsNullOrEmpty(voiceName) ? string.Empty : ("-v \"" + voiceName.Replace('"', '\'') + "\"")) + ((num != 160) ? (" -s " + num + " ") : string.Empty) + ((num2 != 100) ? (" -a " + num2 + " ") : string.Empty) + ((num3 != 50) ? (" -p " + num3 + " ") : string.Empty) + " -z  -m \"" + wrapper.Text.Replace('"', '\'') + "\"" + (string.IsNullOrEmpty(Singleton<Speaker>.Instance.ESpeakDataPath) ? string.Empty : (" --path=\"" + Singleton<Speaker>.Instance.ESpeakDataPath + "\""));
		if (Config.DEBUG)
		{
			UnityEngine.Debug.Log("Process arguments: " + text);
		}
		Process process = new Process();
		try
		{
			process.StartInfo.FileName = Singleton<Speaker>.Instance.ESpeakApplication;
			process.StartInfo.Arguments = text;
			Thread worker = new Thread((ThreadStart)delegate
			{
				MainVoiceProvider.startProcess(process, 0, eventOutputData: false, eventErrorData: false, redirectOutputData: false);
			})
			{
				Name = wrapper.Uid
			};
			worker.Start();
			silence = false;
			processes.Add(wrapper.Uid, process);
			onSpeakStart(wrapper);
			do
			{
				yield return null;
			}
			while (worker.IsAlive || !process.HasExited);
			if (process.ExitCode == 0 || process.ExitCode == -1)
			{
				if (Config.DEBUG)
				{
					UnityEngine.Debug.Log("Text spoken: " + wrapper.Text);
				}
				onSpeakComplete(wrapper);
			}
			else
			{
				using StreamReader streamReader = process.StandardError;
				string text2 = string.Concat("Could not speak the text: ", wrapper, Environment.NewLine, "Exit code: ", process.ExitCode, Environment.NewLine, streamReader.ReadToEnd());
				UnityEngine.Debug.LogError(text2);
				onErrorInfo(wrapper, text2);
			}
			processes.Remove(wrapper.Uid);
		}
		finally
		{
			if (process != null)
			{
				((IDisposable)process).Dispose();
			}
		}
	}

	public override IEnumerator Speak(Wrapper wrapper)
	{
		if (wrapper == null)
		{
			UnityEngine.Debug.LogWarning("'wrapper' is null!");
			yield break;
		}
		if (string.IsNullOrEmpty(wrapper.Text))
		{
			UnityEngine.Debug.LogWarning("'wrapper.Text' is null or empty: " + wrapper);
			yield break;
		}
		if (wrapper.Source == null)
		{
			UnityEngine.Debug.LogWarning("'wrapper.Source' is null: " + wrapper);
			yield break;
		}
		yield return null;
		string voiceName = getVoiceName(wrapper);
		int num = calculateRate(wrapper.Rate);
		int num2 = calculateVolume(wrapper.Volume);
		int num3 = calculatePitch(wrapper.Pitch);
		string outputFile = getOutputFile(wrapper.Uid);
		string text = (string.IsNullOrEmpty(voiceName) ? string.Empty : ("-v \"" + voiceName.Replace('"', '\'') + "\"")) + ((num != 160) ? (" -s " + num + " ") : string.Empty) + ((num2 != 100) ? (" -a " + num2 + " ") : string.Empty) + ((num3 != 50) ? (" -p " + num3 + " ") : string.Empty) + " -w \"" + outputFile.Replace('"', '\'') + "\" -z  -m \"" + wrapper.Text.Replace('"', '\'') + "\"" + (string.IsNullOrEmpty(Singleton<Speaker>.Instance.ESpeakDataPath) ? string.Empty : (" --path=\"" + Singleton<Speaker>.Instance.ESpeakDataPath + "\""));
		if (Config.DEBUG)
		{
			UnityEngine.Debug.Log("Process arguments: " + text);
		}
		Process process = new Process();
		try
		{
			process.StartInfo.FileName = Singleton<Speaker>.Instance.ESpeakApplication;
			process.StartInfo.Arguments = text;
			Thread worker = new Thread((ThreadStart)delegate
			{
				MainVoiceProvider.startProcess(process, 0, eventOutputData: false, eventErrorData: false, redirectOutputData: false);
			})
			{
				Name = wrapper.Uid
			};
			worker.Start();
			silence = false;
			processes.Add(wrapper.Uid, process);
			onSpeakAudioGenerationStart(wrapper);
			do
			{
				yield return null;
			}
			while (worker.IsAlive || !process.HasExited);
			if (process.ExitCode == 0)
			{
				yield return playAudioFile(wrapper, NetworkHelper.GetURLFromFile(outputFile), outputFile);
			}
			else
			{
				using StreamReader streamReader = process.StandardError;
				string text2 = string.Concat("Could not speak the text: ", wrapper, Environment.NewLine, "Exit code: ", process.ExitCode, Environment.NewLine, streamReader.ReadToEnd());
				UnityEngine.Debug.LogError(text2);
				onErrorInfo(wrapper, text2);
			}
			processes.Remove(wrapper.Uid);
		}
		finally
		{
			if (process != null)
			{
				((IDisposable)process).Dispose();
			}
		}
	}

	public override IEnumerator Generate(Wrapper wrapper)
	{
		if (wrapper == null)
		{
			UnityEngine.Debug.LogWarning("'wrapper' is null!");
			yield break;
		}
		if (string.IsNullOrEmpty(wrapper.Text))
		{
			UnityEngine.Debug.LogWarning("'wrapper.Text' is null or empty: " + wrapper);
			yield break;
		}
		yield return null;
		string voiceName = getVoiceName(wrapper);
		int num = calculateRate(wrapper.Rate);
		int num2 = calculateVolume(wrapper.Volume);
		int num3 = calculatePitch(wrapper.Pitch);
		string outputFile = getOutputFile(wrapper.Uid);
		string text = (string.IsNullOrEmpty(voiceName) ? string.Empty : ("-v \"" + voiceName.Replace('"', '\'') + "\"")) + ((num != 160) ? (" -s " + num + " ") : string.Empty) + ((num2 != 100) ? (" -a " + num2 + " ") : string.Empty) + ((num3 != 50) ? (" -p " + num3 + " ") : string.Empty) + " -w \"" + outputFile.Replace('"', '\'') + "\" -z  -m \"" + wrapper.Text.Replace('"', '\'') + "\"" + (string.IsNullOrEmpty(Singleton<Speaker>.Instance.ESpeakDataPath) ? string.Empty : (" --path=\"" + Singleton<Speaker>.Instance.ESpeakDataPath + "\""));
		if (Config.DEBUG)
		{
			UnityEngine.Debug.Log("Process arguments: " + text);
		}
		Process process = new Process();
		try
		{
			process.StartInfo.FileName = Singleton<Speaker>.Instance.ESpeakApplication;
			process.StartInfo.Arguments = text;
			Thread worker = new Thread((ThreadStart)delegate
			{
				MainVoiceProvider.startProcess(process, 0, eventOutputData: false, eventErrorData: false, redirectOutputData: false);
			})
			{
				Name = wrapper.Uid
			};
			worker.Start();
			silence = false;
			processes.Add(wrapper.Uid, process);
			onSpeakAudioGenerationStart(wrapper);
			do
			{
				yield return null;
			}
			while (worker.IsAlive || !process.HasExited);
			if (process.ExitCode == 0)
			{
				processAudioFile(wrapper, outputFile);
			}
			else
			{
				using StreamReader streamReader = process.StandardError;
				string text2 = string.Concat("Could not generate the text: ", wrapper, Environment.NewLine, "Exit code: ", process.ExitCode, Environment.NewLine, streamReader.ReadToEnd());
				UnityEngine.Debug.LogError(text2);
				onErrorInfo(wrapper, text2);
			}
			processes.Remove(wrapper.Uid);
		}
		finally
		{
			if (process != null)
			{
				((IDisposable)process).Dispose();
			}
		}
	}

	public override void Silence()
	{
		base.Silence();
	}

	public override void Silence(string uid)
	{
		base.Silence(uid);
	}

	protected override string getVoiceName(Wrapper wrapper)
	{
		if (wrapper != null && string.IsNullOrEmpty(wrapper.Voice?.Name))
		{
			if (Config.DEBUG)
			{
				UnityEngine.Debug.LogWarning("'wrapper.Voice' or 'wrapper.Voice.Name' is null! Using the OS 'default' voice.");
			}
			return DefaultVoiceName;
		}
		if (wrapper == null)
		{
			return DefaultVoiceName;
		}
		if (Singleton<Speaker>.Instance.ESpeakModifier == ESpeakModifiers.none)
		{
			if (wrapper.Voice.Gender == Gender.FEMALE)
			{
				return wrapper.Voice?.Name + Constants.ESPEAK_FEMALE_MODIFIER;
			}
			return wrapper.Voice?.Name;
		}
		return wrapper.Voice?.Name + "+" + Singleton<Speaker>.Instance.ESpeakModifier;
	}

	private IEnumerator getVoices()
	{
		voices.Clear();
		string arguments = "--voices" + (string.IsNullOrEmpty(Singleton<Speaker>.Instance.ESpeakDataPath) ? string.Empty : (" --path=\"" + Singleton<Speaker>.Instance.ESpeakDataPath + "\""));
		Process process = new Process();
		try
		{
			process.StartInfo.FileName = Singleton<Speaker>.Instance.ESpeakApplication;
			process.StartInfo.Arguments = arguments;
			process.OutputDataReceived += process_OutputDataReceived;
			Thread worker = new Thread((ThreadStart)delegate
			{
				MainVoiceProvider.startProcess(process, 7000, eventOutputData: true);
			});
			worker.Start();
			do
			{
				yield return null;
			}
			while (worker.IsAlive || !process.HasExited);
			if (process.ExitCode != 0)
			{
				using StreamReader streamReader = process.StandardError;
				string text = "Could not get any voices: " + process.ExitCode + Environment.NewLine + streamReader.ReadToEnd();
				UnityEngine.Debug.LogError(text);
				onErrorInfo(null, text);
			}
		}
		finally
		{
			if (process != null)
			{
				((IDisposable)process).Dispose();
			}
		}
		cachedVoices = voices.OrderBy((Voice s) => s.Name).ToList();
		if (BaseConstants.DEV_DEBUG)
		{
			UnityEngine.Debug.Log("Voices read: " + cachedVoices.CTDump());
		}
		isLoading = false;
		onVoicesReady();
	}

	private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
	{
		string data = e.Data;
		if (!string.IsNullOrEmpty(data) && !data.CTStartsWith("Pty"))
		{
			voices.Add(Singleton<Speaker>.Instance.ESpeakApplication.CTContains("espeak-ng") ? new Voice(data.Substring(30, 19).Trim().Replace("_", " "), data.Substring(50).Trim(), Helper.StringToGender(data.Substring(23, 1)), Constants.VOICE_AGE_UNKNOWN, data.Substring(4, 15).Trim(), "", "espeak-ng") : new Voice(data.Substring(22, 20).Trim(), data.Substring(43).Trim(), Helper.StringToGender(data.Substring(19, 1)), Constants.VOICE_AGE_UNKNOWN, data.Substring(4, 15).Trim(), "", "espeak"));
		}
	}

	private static int calculateRate(float rate)
	{
		int num = Mathf.Clamp((Mathf.Abs(rate - 1f) > 0.0001f) ? ((int)(160f * rate)) : 160, 1, 480);
		if (BaseConstants.DEV_DEBUG)
		{
			UnityEngine.Debug.Log("calculateRate: " + num + " - " + rate);
		}
		return num;
	}

	private static int calculateVolume(float volume)
	{
		return Mathf.Clamp((int)(100f * volume), 0, 200);
	}

	private static int calculatePitch(float pitch)
	{
		return Mathf.Clamp((int)(50f * pitch), 0, 99);
	}
}
