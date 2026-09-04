using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using UnityEngine;

namespace Crosstales.RTVoice.Tool;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_tool_1_1_change_gender.html")]
[ExecuteInEditMode]
public class ChangeGender : MonoBehaviour
{
	[Tooltip("The new gender for all voices.")]
	public Gender NewGender;

	[Tooltip("Change voices only when eSpeak is used (default: true).")]
	public bool ESpeakOnly = true;

	private void Start()
	{
		Singleton<Speaker>.Instance.OnVoicesReady += Change;
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnVoicesReady -= Change;
		}
	}

	public void GenderChanged(int index)
	{
		NewGender = (Gender)index;
		Change();
	}

	public void Change()
	{
		if (ESpeakOnly && (!ESpeakOnly || !Singleton<Speaker>.Instance.ESpeakMode))
		{
			return;
		}
		foreach (Voice voice in Singleton<Speaker>.Instance.Voices)
		{
			voice.Gender = NewGender;
		}
	}
}
