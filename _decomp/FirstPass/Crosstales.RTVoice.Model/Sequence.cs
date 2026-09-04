using System;
using System.Text;
using System.Xml.Serialization;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model.Enum;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crosstales.RTVoice.Model;

[Serializable]
public class Sequence
{
	[FormerlySerializedAs("Text")]
	[Tooltip("Text to speak.")]
	[TextArea(1, 5)]
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

	[FormerlySerializedAs("Source")]
	[Header("Optional Settings")]
	[Tooltip("AudioSource for the output (optional).")]
	[SerializeField]
	[XmlIgnore]
	private AudioSource source;

	[FormerlySerializedAs("Rate")]
	[Tooltip("Speech rate of the speaker in percent (1 = 100%, default: 1, optional).")]
	[Range(0.01f, 3f)]
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

	private bool initialized;

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
			rate = Mathf.Clamp(value, 0.01f, 3f);
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

	public bool Initialized
	{
		get
		{
			return initialized;
		}
		set
		{
			initialized = value;
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GetType().Name);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_START);
		stringBuilder.Append("Text='");
		stringBuilder.Append(text);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Voices='");
		stringBuilder.Append(voices);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Source='");
		stringBuilder.Append(source);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Rate='");
		stringBuilder.Append(rate);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Pitch='");
		stringBuilder.Append(pitch);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Volume='");
		stringBuilder.Append(volume);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER_END);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_END);
		return stringBuilder.ToString();
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		Sequence sequence = (Sequence)obj;
		if (Text == sequence.Text && (Voices == null || Voices.Equals(sequence.Voices)) && Math.Abs(Rate - sequence.Rate) < 0.0001f && Math.Abs(Pitch - sequence.Pitch) < 0.0001f)
		{
			return Math.Abs(Volume - sequence.Volume) < 0.0001f;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
