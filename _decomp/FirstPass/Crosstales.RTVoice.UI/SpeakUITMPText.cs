using UnityEngine;

namespace Crosstales.RTVoice.UI;

[HelpURL("https://crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_u_i_1_1_speak_u_i_t_m_p_text.html")]
public class SpeakUITMPText : SpeakUIBase
{
	public bool ChangeColor = true;

	public Color TextColor = Color.green;

	public bool ClearTags = true;

	private void Awake()
	{
		Debug.LogWarning("Is 'TextMesh Pro' installed? If so, please uncomment line 4 in 'SpeakUITMPText.cs'.");
	}
}
