using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.RTVoice.Demo;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_g_u_i_multi_audio_filter.html")]
public class GUIMultiAudioFilter : MonoBehaviour
{
	[Header("Audio Sources")]
	public List<AudioSource> Sources = new List<AudioSource>();

	[Header("Filters")]
	public List<AudioReverbFilter> ReverbFilters = new List<AudioReverbFilter>();

	public List<AudioChorusFilter> ChorusFilters = new List<AudioChorusFilter>();

	public List<AudioEchoFilter> EchoFilters = new List<AudioEchoFilter>();

	public List<AudioDistortionFilter> DistortionFilters = new List<AudioDistortionFilter>();

	public List<AudioLowPassFilter> LowPassFilters = new List<AudioLowPassFilter>();

	public List<AudioHighPassFilter> HighPassFilters = new List<AudioHighPassFilter>();

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
	}

	public void ResetFilters()
	{
		foreach (AudioSource source in Sources)
		{
			source.volume = 1f;
			source.pitch = 1f;
		}
		foreach (AudioReverbFilter reverbFilter in ReverbFilters)
		{
			reverbFilter.reverbPreset = reverbPresets[0];
		}
		foreach (AudioChorusFilter chorusFilter in ChorusFilters)
		{
			chorusFilter.enabled = false;
		}
		foreach (AudioEchoFilter echoFilter in EchoFilters)
		{
			echoFilter.enabled = false;
		}
		foreach (AudioDistortionFilter distortionFilter in DistortionFilters)
		{
			distortionFilter.distortionLevel = 0.5f;
			distortionFilter.enabled = false;
		}
		foreach (AudioLowPassFilter lowPassFilter in LowPassFilters)
		{
			lowPassFilter.cutoffFrequency = 5000f;
			lowPassFilter.enabled = false;
		}
		foreach (AudioHighPassFilter highPassFilter in HighPassFilters)
		{
			highPassFilter.cutoffFrequency = 5000f;
			highPassFilter.enabled = false;
		}
	}

	public void ClearFilters()
	{
		Sources.Clear();
		ReverbFilters.Clear();
		ChorusFilters.Clear();
		EchoFilters.Clear();
		DistortionFilters.Clear();
		LowPassFilters.Clear();
		HighPassFilters.Clear();
	}

	public void ReverbFilterDropdownChanged(int index)
	{
		foreach (AudioReverbFilter reverbFilter in ReverbFilters)
		{
			reverbFilter.reverbPreset = reverbPresets[index];
		}
	}

	public void ChorusFilterEnabled(bool isEnabled)
	{
		foreach (AudioChorusFilter chorusFilter in ChorusFilters)
		{
			chorusFilter.enabled = isEnabled;
		}
	}

	public void EchoFilterEnabled(bool isEnabled)
	{
		foreach (AudioEchoFilter echoFilter in EchoFilters)
		{
			echoFilter.enabled = isEnabled;
		}
	}

	public void DistortionFilterEnabled(bool isEnabled)
	{
		foreach (AudioDistortionFilter distortionFilter in DistortionFilters)
		{
			distortionFilter.enabled = isEnabled;
		}
	}

	public void DistortionFilterChanged(float value)
	{
		foreach (AudioDistortionFilter distortionFilter in DistortionFilters)
		{
			distortionFilter.distortionLevel = value;
		}
		Distortion.text = value.ToString("0.00");
	}

	public void LowPassFilterEnabled(bool isEnabled)
	{
		foreach (AudioLowPassFilter lowPassFilter in LowPassFilters)
		{
			lowPassFilter.enabled = isEnabled;
		}
	}

	public void LowPassFilterChanged(float value)
	{
		foreach (AudioLowPassFilter lowPassFilter in LowPassFilters)
		{
			lowPassFilter.cutoffFrequency = value;
		}
		Lowpass.text = value.ToString();
	}

	public void HighPassFilterEnabled(bool isEnabled)
	{
		foreach (AudioHighPassFilter highPassFilter in HighPassFilters)
		{
			highPassFilter.enabled = isEnabled;
		}
	}

	public void HighPassFilterChanged(float value)
	{
		foreach (AudioHighPassFilter highPassFilter in HighPassFilters)
		{
			highPassFilter.cutoffFrequency = value;
		}
		Highpass.text = value.ToString();
	}

	public void VolumeChanged(float value)
	{
		foreach (AudioSource source in Sources)
		{
			source.volume = value;
		}
		Volume.text = value.ToString("0.00");
	}

	public void PitchChanged(float value)
	{
		foreach (AudioSource source in Sources)
		{
			source.pitch = value;
		}
		Pitch.text = value.ToString("0.00");
	}
}
