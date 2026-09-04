using Crosstales.Common.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.RTVoice.UI;

[RequireComponent(typeof(Text))]
[HelpURL("https://crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_u_i_1_1_speak_u_i_text_voice.html")]
public class SpeakUITextVoice : SpeakUIText
{
	protected override string speak(string text)
	{
		if (base.Mode != 0)
		{
			return Singleton<Speaker>.Instance.SpeakNative(text, Singleton<Speaker>.Instance.VoiceForName(TextComponent.text), base.Rate, base.Pitch, base.Volume);
		}
		return Singleton<Speaker>.Instance.Speak(text, base.Source, Singleton<Speaker>.Instance.VoiceForName(TextComponent.text), speakImmediately: true, base.Rate, base.Pitch, base.Volume);
	}
}
