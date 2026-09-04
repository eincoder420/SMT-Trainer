using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.RTVoice.Demo.Util;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_util_1_1_platform_controller.html")]
public class PlatformController : Crosstales.Common.Util.PlatformController
{
	private void Start()
	{
		Singleton<Speaker>.Instance.OnProviderChange += onProviderChange;
		onProviderChange(string.Empty);
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnProviderChange -= onProviderChange;
		}
	}

	private void onProviderChange(string provider)
	{
		selectPlatform();
	}
}
