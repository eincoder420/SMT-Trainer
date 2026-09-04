using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.UI.Audio;

public class AudioFilterController : MonoBehaviour
{
	[Header("Audio Filters")]
	[Tooltip("Searches for all audio filters in the whole scene (default: true).")]
	public bool FindAllAudioFiltersOnStart = true;

	public AudioReverbFilter[] ReverbFilters;

	public AudioChorusFilter[] ChorusFilters;

	public AudioEchoFilter[] EchoFilters;

	public AudioDistortionFilter[] DistortionFilters;

	public AudioLowPassFilter[] LowPassFilters;

	public AudioHighPassFilter[] HighPassFilters;

	[Header("Settings")]
	[Tooltip("Resets all active audio filters (default: on).")]
	public bool ResetAudioFiltersOnStart = true;

	public bool ChorusFilter;

	public bool EchoFilter;

	public bool DistortionFilter;

	public float DistortionFilterValue = 0.5f;

	public bool LowpassFilter;

	public float LowpassFilterValue = 5000f;

	public bool HighpassFilter;

	public float HighpassFilterValue = 5000f;

	[Header("UI Objects")]
	public Dropdown ReverbFilterDropdown;

	public Text DistortionText;

	public Text LowpassText;

	public Text HighpassText;

	private readonly List<AudioReverbPreset> _reverbPresets = new List<AudioReverbPreset>();

	private bool _initialized;

	private void Start()
	{
		List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
		foreach (AudioReverbPreset value in Enum.GetValues(typeof(AudioReverbPreset)))
		{
			list.Add(new Dropdown.OptionData(value.ToString()));
			_reverbPresets.Add(value);
		}
		if (ReverbFilterDropdown != null)
		{
			ReverbFilterDropdown.ClearOptions();
			ReverbFilterDropdown.AddOptions(list);
		}
	}

	private void Update()
	{
		if (!_initialized && Time.frameCount % 30 == 0)
		{
			_initialized = true;
			if (FindAllAudioFiltersOnStart)
			{
				FindAllAudioFilters();
			}
			if (ResetAudioFiltersOnStart)
			{
				ResetAudioFilters();
			}
		}
	}

	public void FindAllAudioFilters()
	{
		ReverbFilters = UnityEngine.Object.FindObjectsOfType(typeof(AudioReverbFilter)) as AudioReverbFilter[];
		ChorusFilters = UnityEngine.Object.FindObjectsOfType(typeof(AudioChorusFilter)) as AudioChorusFilter[];
		EchoFilters = UnityEngine.Object.FindObjectsOfType(typeof(AudioEchoFilter)) as AudioEchoFilter[];
		DistortionFilters = UnityEngine.Object.FindObjectsOfType(typeof(AudioDistortionFilter)) as AudioDistortionFilter[];
		LowPassFilters = UnityEngine.Object.FindObjectsOfType(typeof(AudioLowPassFilter)) as AudioLowPassFilter[];
		HighPassFilters = UnityEngine.Object.FindObjectsOfType(typeof(AudioHighPassFilter)) as AudioHighPassFilter[];
	}

	public void ResetAudioFilters()
	{
		ReverbFilterDropdownChanged(0);
		ChorusFilterEnabled(ChorusFilter);
		EchoFilterEnabled(EchoFilter);
		DistortionFilterEnabled(DistortionFilter);
		DistortionFilterChanged(DistortionFilterValue);
		LowPassFilterEnabled(LowpassFilter);
		LowPassFilterChanged(LowpassFilterValue);
		HighPassFilterEnabled(HighpassFilter);
		HighPassFilterChanged(HighpassFilterValue);
	}

	public void ReverbFilterDropdownChanged(int index)
	{
		AudioReverbFilter[] reverbFilters = ReverbFilters;
		for (int i = 0; i < reverbFilters.Length; i++)
		{
			reverbFilters[i].reverbPreset = _reverbPresets[index];
		}
	}

	public void ChorusFilterEnabled(bool isEnabled)
	{
		AudioChorusFilter[] chorusFilters = ChorusFilters;
		for (int i = 0; i < chorusFilters.Length; i++)
		{
			chorusFilters[i].enabled = isEnabled;
		}
	}

	public void EchoFilterEnabled(bool isEnabled)
	{
		AudioEchoFilter[] echoFilters = EchoFilters;
		for (int i = 0; i < echoFilters.Length; i++)
		{
			echoFilters[i].enabled = isEnabled;
		}
	}

	public void DistortionFilterEnabled(bool isEnabled)
	{
		AudioDistortionFilter[] distortionFilters = DistortionFilters;
		for (int i = 0; i < distortionFilters.Length; i++)
		{
			distortionFilters[i].enabled = isEnabled;
		}
	}

	public void DistortionFilterChanged(float value)
	{
		AudioDistortionFilter[] distortionFilters = DistortionFilters;
		for (int i = 0; i < distortionFilters.Length; i++)
		{
			distortionFilters[i].distortionLevel = value;
		}
		if (DistortionText != null)
		{
			DistortionText.text = value.ToString("0.00");
		}
	}

	public void LowPassFilterEnabled(bool isEnabled)
	{
		AudioLowPassFilter[] lowPassFilters = LowPassFilters;
		for (int i = 0; i < lowPassFilters.Length; i++)
		{
			lowPassFilters[i].enabled = isEnabled;
		}
	}

	public void LowPassFilterChanged(float value)
	{
		AudioLowPassFilter[] lowPassFilters = LowPassFilters;
		for (int i = 0; i < lowPassFilters.Length; i++)
		{
			lowPassFilters[i].cutoffFrequency = value;
		}
		if (LowpassText != null)
		{
			LowpassText.text = value.ToString("0");
		}
	}

	public void HighPassFilterEnabled(bool isEnabled)
	{
		AudioHighPassFilter[] highPassFilters = HighPassFilters;
		for (int i = 0; i < highPassFilters.Length; i++)
		{
			highPassFilters[i].enabled = isEnabled;
		}
	}

	public void HighPassFilterChanged(float value)
	{
		AudioHighPassFilter[] highPassFilters = HighPassFilters;
		for (int i = 0; i < highPassFilters.Length; i++)
		{
			highPassFilters[i].cutoffFrequency = value;
		}
		if (HighpassText != null)
		{
			HighpassText.text = value.ToString("0");
		}
	}
}
