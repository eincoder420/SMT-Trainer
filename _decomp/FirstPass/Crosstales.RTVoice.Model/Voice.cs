using System;
using System.Xml.Serialization;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model.Enum;
using UnityEngine;

namespace Crosstales.RTVoice.Model;

[Serializable]
public class Voice
{
	[Tooltip("Name of the voice.")]
	public string Name;

	[Tooltip("Culture of the voice voice.")]
	[SerializeField]
	private string culture;

	[Tooltip("Description of the voice.")]
	public string Description;

	[Tooltip("Gender of the voice.")]
	public Gender Gender;

	[Tooltip("Age of the voice.")]
	public string Age;

	[Tooltip("Identifier of the voice.")]
	public string Identifier = string.Empty;

	[Tooltip("Vendor of the voice.")]
	public string Vendor = string.Empty;

	[Tooltip("Sample rate in Hz of the voice.")]
	public int SampleRate;

	[Tooltip("Is the voice neural?.")]
	public bool isNeural;

	public string Culture
	{
		get
		{
			return culture;
		}
		set
		{
			if (value != null)
			{
				culture = value.Trim().Replace('_', '-');
			}
		}
	}

	public SystemLanguage Language => BaseHelper.ISO639ToLanguage(Culture);

	[XmlIgnore]
	public string SimplifiedCulture => culture.Replace("-", string.Empty);

	public Voice()
	{
	}

	public Voice(string name, string description, Gender gender, string age, string culture, string id = "", string vendor = "unknown", int sampleRate = 0, bool neural = false)
	{
		Name = name;
		Description = description;
		Gender = gender;
		Age = age;
		Culture = culture;
		Identifier = id;
		Vendor = vendor;
		SampleRate = sampleRate;
		isNeural = neural;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		Voice voice = (Voice)obj;
		if (Name == voice.Name && Culture == voice.Culture && Description == voice.Description && Gender == voice.Gender && Age == voice.Age && Identifier == voice.Identifier && Vendor == voice.Vendor && SampleRate == voice.SampleRate)
		{
			return isNeural == voice.isNeural;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 0;
		if (Name != null)
		{
			num += Name.GetHashCode();
		}
		if (Culture != null)
		{
			num += Culture.GetHashCode();
		}
		if (Description != null)
		{
			num += Description.GetHashCode();
		}
		num += (int)Gender * 17;
		if (Age != null)
		{
			num += Age.GetHashCode();
		}
		if (Identifier != null)
		{
			num += Identifier.GetHashCode();
		}
		if (Vendor != null)
		{
			num += Vendor.GetHashCode();
		}
		return num + SampleRate * 17;
	}

	public override string ToString()
	{
		return $"{Name} ({Culture}, {Gender})";
	}
}
