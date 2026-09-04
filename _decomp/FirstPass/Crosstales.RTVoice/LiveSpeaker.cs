using System;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using UnityEngine;

namespace Crosstales.RTVoice;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_live_speaker.html")]
public class LiveSpeaker : MonoBehaviour
{
	private static readonly char[] splitChar = new char[1] { ';' };

	public void SpeakNativeLive(Wrapper wrapper)
	{
		Singleton<Speaker>.Instance.SpeakNative(wrapper);
	}

	public void SpeakNativeLive(string args)
	{
		if (!string.IsNullOrEmpty(args))
		{
			SpeakNativeLive(args.Split(splitChar, StringSplitOptions.RemoveEmptyEntries));
		}
		else
		{
			Debug.LogWarning("'args' is null or empty!", this);
		}
	}

	public void SpeakNativeLive(string[] args)
	{
		if (args != null && args.Length >= 1)
		{
			string text = args[0];
			string culture = null;
			if (args.Length >= 2)
			{
				culture = args[1];
			}
			Voice voice = null;
			if (args.Length >= 3)
			{
				voice = Singleton<Speaker>.Instance.VoiceForName(args[2]);
			}
			float result = 1f;
			if (args.Length >= 4 && !float.TryParse(args[3], out result))
			{
				Debug.LogWarning("Argument 3 (= rate) is not a number: '" + args[3] + "'", this);
				result = 1f;
			}
			float result2 = 1f;
			if (args.Length >= 5 && !float.TryParse(args[4], out result2))
			{
				Debug.LogWarning("Argument 4 (= pitch) is not a number: '" + args[4] + "'", this);
				result2 = 1f;
			}
			float result3 = 1f;
			if (args.Length >= 6 && !float.TryParse(args[5], out result3))
			{
				Debug.LogWarning("Argument 5 (= volume) is not a number: '" + args[5] + "'", this);
				result3 = 1f;
			}
			if (voice == null)
			{
				voice = Singleton<Speaker>.Instance.VoiceForCulture(culture);
			}
			SpeakNativeLive(new Wrapper(text, voice, result, result2, result3, forceSSML: true));
		}
		else
		{
			Debug.LogError("'args' is null or wrong number of arguments given!" + Environment.NewLine + "Please verify that you pass a string-array with at least one argument (text).", this);
		}
	}

	public void SpeakLive(Wrapper wrapper)
	{
		Singleton<Speaker>.Instance.Speak(wrapper);
	}

	public void SpeakLive(string args)
	{
		if (!string.IsNullOrEmpty(args))
		{
			SpeakLive(args.Split(splitChar, StringSplitOptions.RemoveEmptyEntries));
		}
		else
		{
			Debug.LogWarning("'args' is null or empty!", this);
		}
	}

	public void SpeakLive(string[] args)
	{
		if (args != null && args.Length >= 1)
		{
			string text = args[0];
			string culture = null;
			if (args.Length >= 2)
			{
				culture = args[1];
			}
			Voice voice = null;
			if (args.Length >= 3)
			{
				voice = Singleton<Speaker>.Instance.VoiceForName(args[2]);
			}
			float result = 1f;
			if (args.Length >= 4 && !float.TryParse(args[3], out result))
			{
				Debug.LogWarning("Argument 3 (= rate) is not a number: '" + args[3] + "'", this);
				result = 1f;
			}
			float result2 = 1f;
			if (args.Length >= 5 && !float.TryParse(args[4], out result2))
			{
				Debug.LogWarning("Argument 5 (= pitch) is not a number: '" + args[4] + "'", this);
				result2 = 1f;
			}
			float result3 = 1f;
			if (args.Length >= 6 && !float.TryParse(args[5], out result3))
			{
				Debug.LogWarning("Argument 4 (= volume) is not a number: '" + args[5] + "'", this);
				result3 = 1f;
			}
			if (voice == null)
			{
				voice = Singleton<Speaker>.Instance.VoiceForCulture(culture);
			}
			SpeakLive(new Wrapper(text, voice, result, result2, result3));
		}
		else
		{
			Debug.LogError("'args' is null or wrong number of arguments given!" + Environment.NewLine + "Please verify that you pass a string-array with at least one argument (text).", this);
		}
	}

	public void SilenceLive()
	{
		Singleton<Speaker>.Instance.Silence();
	}
}
