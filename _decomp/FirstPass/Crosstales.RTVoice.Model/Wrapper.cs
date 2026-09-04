using System;
using System.Text;
using System.Xml.Serialization;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Util;
using UnityEngine;

namespace Crosstales.RTVoice.Model;

[Serializable]
public class Wrapper
{
	[Tooltip("Text for the speech.")]
	[TextArea(1, 5)]
	[SerializeField]
	private string text = string.Empty;

	[Tooltip("AudioSource for the speech.")]
	[SerializeField]
	private AudioSource source;

	[Tooltip("Voice for the speech.")]
	[SerializeField]
	private Voice voice;

	[Tooltip("Speak immediately after the audio generation. Only works if 'Source' is not null.")]
	[SerializeField]
	private bool speakImmediately = true;

	[Tooltip("Speech rate of the speaker in percent (1 = 100%, default: 1, optional).")]
	[Range(0.01f, 3f)]
	[SerializeField]
	private float rate = 1f;

	[Tooltip("Speech pitch of the speaker in percent (1 = 100%, default: 1, optional).")]
	[Range(0f, 2f)]
	[SerializeField]
	private float pitch = 1f;

	[Tooltip("Volume of the speaker in percent (1 = 100%, default: 1, optional).")]
	[Range(0f, 1f)]
	[SerializeField]
	private float volume = 1f;

	[Tooltip("Output file (without extension) for the generated audio.")]
	[SerializeField]
	private string outputFile;

	[Tooltip("Force SSML on supported platforms.")]
	[SerializeField]
	private bool forceSSML = true;

	[Tooltip("Is the current wrapper just a part of a speech (only used in iOS).")]
	[SerializeField]
	private bool _isPartial;

	private string uid;

	private readonly DateTime created = DateTime.Now;

	public string Text
	{
		get
		{
			string text = Helper.CleanText(this.text, Singleton<Speaker>.Instance.AutoClearTags || !ForceSSML);
			if (text.Length > Singleton<Speaker>.Instance.MaxTextLength)
			{
				Debug.LogWarning("Text is too long! It will be shortened to " + Singleton<Speaker>.Instance.MaxTextLength + " characters: " + this);
				return text.Substring(0, Singleton<Speaker>.Instance.MaxTextLength);
			}
			return text;
		}
		set
		{
			text = value;
		}
	}

	[XmlIgnore]
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

	public Voice Voice
	{
		get
		{
			return voice;
		}
		set
		{
			voice = value;
		}
	}

	public bool SpeakImmediately
	{
		get
		{
			return speakImmediately;
		}
		set
		{
			speakImmediately = value;
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
			volume = Mathf.Clamp(value, 0.01f, 1f);
		}
	}

	public string OutputFile
	{
		get
		{
			return outputFile;
		}
		set
		{
			outputFile = value;
		}
	}

	public bool ForceSSML
	{
		get
		{
			return forceSSML;
		}
		set
		{
			forceSSML = value;
		}
	}

	public bool isPartial
	{
		get
		{
			return _isPartial;
		}
		set
		{
			_isPartial = value;
		}
	}

	public string Uid
	{
		get
		{
			return uid;
		}
		set
		{
			uid = value;
		}
	}

	public DateTime Created => created;

	public float SpeechTime
	{
		get
		{
			if (!BaseHelper.isEditorMode && source != null && source.clip != null)
			{
				return source.clip.length;
			}
			return 0f;
		}
	}

	public Wrapper()
	{
		uid = Guid.NewGuid().ToString();
	}

	public Wrapper(string text, Voice voice = null, float rate = 1f, float pitch = 1f, float volume = 1f, bool forceSSML = true)
	{
		uid = Guid.NewGuid().ToString();
		Text = text;
		this.voice = voice;
		Rate = rate;
		Pitch = pitch;
		Volume = volume;
		this.forceSSML = forceSSML;
	}

	public Wrapper(string text, Voice voice = null, float rate = 1f, float pitch = 1f, float volume = 1f, AudioSource source = null, bool speakImmediately = true, string outputFile = "", bool forceSSML = true)
	{
		uid = Guid.NewGuid().ToString();
		Text = text;
		this.source = source;
		this.voice = voice;
		this.speakImmediately = speakImmediately;
		Rate = rate;
		Pitch = pitch;
		Volume = volume;
		this.outputFile = outputFile;
		this.forceSSML = forceSSML;
	}

	public Wrapper(string uid, string text, Voice voice = null, float rate = 1f, float pitch = 1f, float volume = 1f, AudioSource source = null, bool speakImmediately = true, string outputFile = "", bool forceSSML = true)
		: this(text, voice, rate, pitch, volume, source, speakImmediately, outputFile, forceSSML)
	{
		this.uid = uid;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GetType().Name);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_START);
		stringBuilder.Append("Uid='");
		stringBuilder.Append(uid);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Text='");
		stringBuilder.Append(text);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Source='");
		stringBuilder.Append(source);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Voice='");
		stringBuilder.Append(voice);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("SpeakImmediately='");
		stringBuilder.Append(speakImmediately);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Rate='");
		stringBuilder.Append(rate);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Pitch='");
		stringBuilder.Append(pitch);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Volume='");
		stringBuilder.Append(volume);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("OutputFile='");
		stringBuilder.Append(outputFile);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("ForceSSML='");
		stringBuilder.Append(forceSSML);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("isPartial='");
		stringBuilder.Append(isPartial);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Created='");
		stringBuilder.Append(Created);
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
		Wrapper wrapper = (Wrapper)obj;
		if (Text == wrapper.Text && (Voice == null || Voice.Equals(wrapper.Voice)) && Math.Abs(Rate - wrapper.Rate) < 0.0001f && Math.Abs(Pitch - wrapper.Pitch) < 0.0001f)
		{
			return Math.Abs(Volume - wrapper.Volume) < 0.0001f;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 0;
		if (Text != null)
		{
			num += Text.GetHashCode();
		}
		if (Voice != null)
		{
			num += Voice.GetHashCode();
		}
		num += (int)(Rate * 100f) * 17;
		num += (int)(Pitch * 100f) * 17;
		return num + (int)(Volume * 100f) * 17;
	}
}
