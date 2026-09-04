using System;
using System.Text;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model.Enum;
using UnityEngine;

namespace Crosstales.RTVoice.Model;

[Serializable]
public class VoiceAlias
{
	[Tooltip("Name of the voice under Windows.")]
	public string VoiceNameWindows;

	[Tooltip("Name of the voice under macOS.")]
	public string VoiceNameMac;

	[Tooltip("Name of the voice under Linux and for eSpeak.")]
	public string VoiceNameLinux;

	[Tooltip("Name of the voice under Android.")]
	public string VoiceNameAndroid;

	[Tooltip("Name of the voice under iOS.")]
	public string VoiceNameIOS;

	[Tooltip("Name of the voice under WSA.")]
	public string VoiceNameWSA;

	[Tooltip("Name of the voice for custom TTS-systems.")]
	public string VoiceNameCustom;

	[Tooltip("Fallback culture for the text (e.g. 'en', optional).")]
	public string Culture = "en";

	[Tooltip("Fallback gender for the text.")]
	public Gender Gender = Gender.UNKNOWN;

	public string VoiceName
	{
		get
		{
			if (Singleton<Speaker>.Instance.CustomProvider == null)
			{
				if (BaseHelper.isWindowsPlatform && !Singleton<Speaker>.Instance.ESpeakMode)
				{
					return VoiceNameWindows;
				}
				if (BaseHelper.isMacOSPlatform && !Singleton<Speaker>.Instance.ESpeakMode)
				{
					return VoiceNameMac;
				}
				if (BaseHelper.isAndroidPlatform)
				{
					return VoiceNameAndroid;
				}
				if (BaseHelper.isWSABasedPlatform)
				{
					return VoiceNameWSA;
				}
				if (BaseHelper.isIOSBasedPlatform)
				{
					return VoiceNameIOS;
				}
				return VoiceNameLinux;
			}
			return VoiceNameCustom;
		}
	}

	public Voice Voice => Singleton<Speaker>.Instance.VoiceForName(VoiceName) ?? Singleton<Speaker>.Instance.VoiceForGender(Gender, Culture);

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GetType().Name);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_START);
		stringBuilder.Append("VoiceNameWindows='");
		stringBuilder.Append(VoiceNameWindows);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("VoiceNameMac='");
		stringBuilder.Append(VoiceNameMac);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("VoiceNameLinux='");
		stringBuilder.Append(VoiceNameLinux);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("VoiceNameAndroid='");
		stringBuilder.Append(VoiceNameAndroid);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("VoiceNameIOS='");
		stringBuilder.Append(VoiceNameIOS);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("VoiceNameWSA='");
		stringBuilder.Append(VoiceNameWSA);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("VoiceNameCustom='");
		stringBuilder.Append(VoiceNameCustom);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Culture='");
		stringBuilder.Append(Culture);
		stringBuilder.Append(BaseConstants.TEXT_TOSTRING_DELIMITER);
		stringBuilder.Append("Gender='");
		stringBuilder.Append(Gender);
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
		VoiceAlias voiceAlias = (VoiceAlias)obj;
		if (VoiceNameWindows == voiceAlias.VoiceNameWindows && VoiceNameMac == voiceAlias.VoiceNameMac && VoiceNameLinux == voiceAlias.VoiceNameLinux && VoiceNameAndroid == voiceAlias.VoiceNameAndroid && VoiceNameIOS == voiceAlias.VoiceNameIOS && VoiceNameWSA == voiceAlias.VoiceNameWSA && VoiceNameCustom == voiceAlias.VoiceNameCustom && Culture == voiceAlias.Culture)
		{
			return Gender == voiceAlias.Gender;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
