using Crosstales.Common.Util;
using Crosstales.RTVoice.Provider;
using UnityEngine;

namespace Crosstales.RTVoice.Demo.Util;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_util_1_1_custom_provider_controller.html")]
public class CustomProviderController : MonoBehaviour
{
	public BaseCustomVoiceProvider Provider;

	public bool ParentProvider;

	private bool isCustom;

	private void Start()
	{
		isCustom = Singleton<Speaker>.Instance.CustomMode;
		if (Provider != null)
		{
			Singleton<Speaker>.Instance.CustomProvider = Provider;
			Singleton<Speaker>.Instance.CustomMode = true;
			if (ParentProvider)
			{
				Provider.transform.SetParent(Singleton<Speaker>.Instance.transform);
			}
		}
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.CustomMode = isCustom;
		}
	}
}
