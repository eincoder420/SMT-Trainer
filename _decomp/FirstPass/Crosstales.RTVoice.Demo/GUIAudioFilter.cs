using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.RTVoice.Demo;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_g_u_i_audio_filter.html")]
public class GUIAudioFilter : MonoBehaviour
{
	[Header("Audio Source")]
	public AudioSource Source;

	[Header("Filters")]
	public AudioReverbFilter ReverbFilter;

	public AudioChorusFilter ChorusFilter;

	public AudioEchoFilter EchoFilter;

	public AudioDistortionFilter DistortionFilter;

	public AudioLowPassFilter LowPassFilter;

	public AudioHighPassFilter HighPassFilter;

	[Header("UI Objects")]
	public Text Distortion;

	public Text Lowpass;

	public Text Highpass;

	public Text Volume;

	public Text Pitch;

	public Dropdown ReverbFilterDropdown;

	private readonly List<AudioReverbPreset> reverbPresets = new List<AudioReverbPreset>();

	private void Start()
	{
		List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
		foreach (AudioReverbPreset value in Enum.GetValues(typeof(AudioReverbPreset)))
		{
			list.Add(new Dropdown.OptionData(value.ToString()));
			reverbPresets.Add(value);
		}
		if (ReverbFilterDropdown != null)
		{
			ReverbFilterDropdown.ClearOptions();
			ReverbFilterDropdown.AddOptions(list);
		}
		ResetFilters();
	}

	public void ResetFilters()
	{
		Source.pitch = 1f;
		Source.volume = 1f;
		ReverbFilter.reverbPreset = reverbPresets[0];
		ChorusFilter.enabled = false;
		EchoFilter.enabled = false;
		DistortionFilter.distortionLevel = 0.5f;
		DistortionFilter.enabled = false;
		LowPassFilter.cutoffFrequency = 5000f;
		LowPassFilter.enabled = false;
		HighPassFilter.cutoffFrequency = 5000f;
		HighPassFilter.enabled = false;
	}

	public void ReverbFilterDropdownChanged(int index)
	{
		ReverbFilter.reverbPreset = reverbPresets[index];
	}

	public void ChorusFilterEnabled(bool isEnabled)
	{
		ChorusFilter.enabled = isEnabled;
	}

	public void EchoFilterEnabled(bool isEnabled)
	{
		EchoFilter.enabled = isEnabled;
	}

	public void DistortionFilterEnabled(bool isEnabled)
	{
		DistortionFilter.enabled = isEnabled;
	}

	public void DistortionFilterChanged(float value)
	{
		DistortionFilter.distortionLevel = value;
		Distortion.text = value.ToString("0.00");
	}

	public void LowPassFilterEnabled(bool isEnabled)
	{
		LowPassFilter.enabled = isEnabled;
	}

	public void LowPassFilterChanged(float value)
	{
		LowPassFilter.cutoffFrequency = value;
		Lowpass.text = value.ToString();
	}

	public void HighPassFilterEnabled(bool isEnabled)
	{
		HighPassFilter.enabled = isEnabled;
	}

	public void HighPassFilterChanged(float value)
	{
		HighPassFilter.cutoffFrequency = value;
		Highpass.text = value.ToString();
	}

	public void VolumeChanged(float value)
	{
		Source.volume = value;
		Volume.text = value.ToString("0.00");
	}

	public void PitchChanged(float value)
	{
		Source.pitch = value;
		Pitch.text = value.ToString("0.00");
	}
}
