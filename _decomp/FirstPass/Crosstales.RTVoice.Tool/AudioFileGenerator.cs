using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Crosstales.Common.Model.Enum;
using Crosstales.Common.Util;
using Crosstales.NAudio.Wave;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crosstales.RTVoice.Tool;

[ExecuteInEditMode]
[HelpURL("https://crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_tool_1_1_audio_file_generator.html")]
public class AudioFileGenerator : MonoBehaviour
{
	[Header("Configuration")]
	[FormerlySerializedAs("TextFiles")]
	[Tooltip("Text files to generate.")]
	[SerializeField]
	private TextAsset[] textFiles;

	[FormerlySerializedAs("FileInsideAssets")]
	[Tooltip("Are the specified file paths inside the Assets-folder (current project)? If this option is enabled, it prefixes the path with 'Application.dataPath' (default: true).")]
	[SerializeField]
	private bool fileInsideAssets = true;

	[FormerlySerializedAs("SampleRate")]
	[Header("Windows Settings")]
	[Tooltip("Set the sample rate of the WAV files (default: 48000). Note: this works only under Windows standalone.")]
	[SerializeField]
	private SampleRate sampleRate = SampleRate._48000Hz;

	[FormerlySerializedAs("BitsPerSample")]
	[HideInInspector]
	[Tooltip("Set the bits per sample of the WAV files (default: 16). Note: this works only under Windows standalone.")]
	[SerializeField]
	private int bitsPerSample = 16;

	[FormerlySerializedAs("Channels")]
	[Tooltip("Set the channels of the WAV files (default: 1). Note: this works only under Windows standalone.")]
	[Range(1f, 2f)]
	[SerializeField]
	private int channels = 2;

	[FormerlySerializedAs("CreateCopy")]
	[Tooltip("Creates a copy of the downsampled WAV file and leaves the original intact (default: false). Note: this works only under Windows standalone.")]
	[SerializeField]
	private bool createCopy;

	[FormerlySerializedAs("isNormalize")]
	[Tooltip("Normalize the volume of the WAV files (default: false). Note: this works only under Windows standalone.")]
	[SerializeField]
	private bool _isNormalize;

	[FormerlySerializedAs("GenerateOnStart")]
	[Header("Behaviour Settings")]
	[Tooltip("Enable generating of the texts on start (default: false).")]
	[SerializeField]
	private bool generateOnStart;

	private static readonly char[] splitChar = new char[1] { ';' };

	private string lastUid = "crosstales";

	private bool isGenerate;

	[Header("Events")]
	public AudioFileGeneratorStartEvent OnStarted;

	public AudioFileGeneratorCompleteEvent OnCompleted;

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

	public SampleRate SampleRate
	{
		get
		{
			return sampleRate;
		}
		set
		{
			sampleRate = value;
		}
	}

	public int Channels
	{
		get
		{
			return channels;
		}
		set
		{
			channels = Mathf.Clamp(value, 1, 2);
		}
	}

	public bool CreateCopy
	{
		get
		{
			return createCopy;
		}
		set
		{
			createCopy = value;
		}
	}

	public bool isNormalize
	{
		get
		{
			return _isNormalize;
		}
		set
		{
			_isNormalize = value;
		}
	}

	public bool GenerateOnStart
	{
		get
		{
			return generateOnStart;
		}
		set
		{
			generateOnStart = value;
		}
	}

	public event AudioFileGeneratorStart OnAudioFileGeneratorStart;

	public event AudioFileGeneratorComplete OnAudioFileGeneratorComplete;

	private void Start()
	{
		Singleton<Speaker>.Instance.OnSpeakAudioGenerationComplete += onSpeakAudioGenerationComplete;
		Singleton<Speaker>.Instance.OnVoicesReady += onVoicesReady;
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnSpeakAudioGenerationComplete -= onSpeakAudioGenerationComplete;
			Singleton<Speaker>.Instance.OnVoicesReady -= onVoicesReady;
		}
	}

	private void OnValidate()
	{
		if (bitsPerSample < 15)
		{
			bitsPerSample = 8;
		}
		else if (bitsPerSample < 31)
		{
			bitsPerSample = 16;
		}
		else
		{
			bitsPerSample = 32;
		}
		channels = ((channels <= 1) ? 1 : 2);
	}

	public void Generate()
	{
		if (!isGenerate)
		{
			isGenerate = true;
			if (!BaseHelper.isEditorMode)
			{
				StartCoroutine(generate());
			}
		}
	}

	private void convert(string outputFile)
	{
		string text = string.Concat(outputFile.Substring(0, outputFile.Length - 4), "_", sampleRate, Singleton<Speaker>.Instance.AudioFileExtension);
		bool flag = false;
		try
		{
			using WaveFileReader waveFileReader = new WaveFileReader(outputFile);
			if (waveFileReader.WaveFormat.SampleRate != (int)sampleRate)
			{
				using (WaveFormatConversionStream sourceProvider = new WaveFormatConversionStream(new WaveFormat((int)sampleRate, bitsPerSample, channels), waveFileReader))
				{
					WaveFileWriter.CreateWaveFile(text, sourceProvider);
				}
				flag = true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not convert audio file: " + ex, this);
		}
		if (!flag)
		{
			return;
		}
		try
		{
			if (!createCopy)
			{
				File.Delete(outputFile);
				File.Move(text, outputFile);
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("Could not delete and move audio files: " + ex2, this);
		}
	}

	private void normalizeWAV(string inputFile)
	{
		string filename = inputFile.Substring(0, inputFile.Length - 4) + "_normalized" + Singleton<Speaker>.Instance.AudioFileExtension;
		try
		{
			using AudioFileReader audioFileReader = new AudioFileReader(inputFile);
			float maxPeak = getMaxPeak(inputFile);
			if (Mathf.Abs(maxPeak) < 0.0001f || maxPeak > 1f)
			{
				Debug.LogWarning("File cannot be normalized!", this);
				return;
			}
			audioFileReader.Position = 0L;
			audioFileReader.Volume = 1f / maxPeak;
			WaveFileWriter.CreateWaveFile16(filename, audioFileReader);
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not normalize audio file: " + ex, this);
		}
	}

	private float getMaxPeak(string inputFile)
	{
		float num = 0f;
		try
		{
			using AudioFileReader audioFileReader = new AudioFileReader(inputFile);
			float[] array = new float[audioFileReader.WaveFormat.SampleRate];
			int num2;
			do
			{
				num2 = audioFileReader.Read(array, 0, array.Length);
				for (int i = 0; i < num2; i++)
				{
					float num3 = Mathf.Abs(array[i]);
					if (num3 > num)
					{
						num = num3;
					}
				}
			}
			while (num2 > 0);
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not find the max peak in audio file: " + ex, this);
		}
		return num;
	}

	private IEnumerator generate()
	{
		onStart();
		TextAsset[] array = textFiles;
		foreach (TextAsset textAsset in array)
		{
			if (!(textAsset != null))
			{
				continue;
			}
			List<string> list = BaseHelper.SplitStringToLines(textAsset.text);
			foreach (string item in list)
			{
				if (item.CTStartsWith("#"))
				{
					continue;
				}
				string[] array2 = item.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
				if (array2.Length >= 2)
				{
					Wrapper wrapper = prepare(array2, item);
					string uid = Singleton<Speaker>.Instance.Generate(wrapper);
					do
					{
						yield return null;
					}
					while (!uid.Equals(lastUid));
					convert(wrapper.OutputFile);
					if (_isNormalize)
					{
						normalizeWAV(wrapper.OutputFile);
					}
				}
				else
				{
					Debug.LogWarning("Invalid speech: " + item, this);
				}
			}
		}
		if (Config.DEBUG)
		{
			Debug.Log("Generate finished!", this);
		}
		onComplete();
		isGenerate = false;
	}

	private Wrapper prepare(string[] args, string speech)
	{
		Wrapper wrapper = new Wrapper
		{
			Text = args[0]
		};
		if (fileInsideAssets)
		{
			wrapper.OutputFile = Application.dataPath + "/" + args[1];
		}
		else
		{
			wrapper.OutputFile = args[1];
		}
		if (args.Length >= 3)
		{
			wrapper.Voice = Singleton<Speaker>.Instance.VoiceForName(args[2]);
		}
		if (args.Length >= 4)
		{
			if (!float.TryParse(args[3], out var result))
			{
				Debug.LogWarning("Rate was invalid: " + speech, this);
			}
			else
			{
				wrapper.Rate = result;
			}
		}
		if (args.Length >= 5)
		{
			if (!float.TryParse(args[4], out var result2))
			{
				Debug.LogWarning("Pitch was invalid: " + speech, this);
			}
			else
			{
				wrapper.Pitch = result2;
			}
		}
		if (args.Length >= 6)
		{
			if (!float.TryParse(args[5], out var result3))
			{
				Debug.LogWarning("Volume was invalid: " + speech, this);
			}
			else
			{
				wrapper.Volume = result3;
			}
		}
		return wrapper;
	}

	private void onVoicesReady()
	{
		if (generateOnStart)
		{
			Generate();
		}
	}

	private void onSpeakAudioGenerationComplete(Wrapper wrapper)
	{
		lastUid = wrapper.Uid;
		if (Config.DEBUG)
		{
			Debug.Log("Speech generated: " + wrapper, this);
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
		this.OnAudioFileGeneratorStart?.Invoke();
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
		this.OnAudioFileGeneratorComplete?.Invoke();
	}
}
