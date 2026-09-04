using Crosstales.Common.Model.Enum;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Provider;
using UnityEngine;

namespace Crosstales.RTVoice.Tool;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_tool_1_1_platform_provider.html")]
public class PlatformProvider : MonoBehaviour
{
	[Header("Configuration Settings")]
	[Tooltip("Platform specific provider for the app (empty provider = default of the OS).")]
	public PlatformProviderTuple[] Configuration;

	[Header("Default")]
	[Tooltip("Default provider of the app (empty = default of the OS).")]
	public BaseCustomVoiceProvider DefaultVoiceProvider;

	[Header("Parenting")]
	[Tooltip("Set the provider as child of the RTVoice parent object (default: true).")]
	public bool SetAsChild = true;

	[Header("Editor")]
	[Tooltip("Use the default provider inside the Editor (default: false).")]
	public bool UseDefault;

	private void Start()
	{
		bool flag = false;
		if (!BaseHelper.isEditor || !UseDefault)
		{
			Platform currentPlatform = BaseHelper.CurrentPlatform;
			PlatformProviderTuple[] configuration = Configuration;
			foreach (PlatformProviderTuple platformProviderTuple in configuration)
			{
				if (platformProviderTuple.Platform != currentPlatform)
				{
					continue;
				}
				if (platformProviderTuple.CustomVoiceProvider == null)
				{
					Singleton<Speaker>.Instance.CustomMode = false;
				}
				else
				{
					Singleton<Speaker>.Instance.CustomProvider = platformProviderTuple.CustomVoiceProvider;
					Singleton<Speaker>.Instance.CustomMode = true;
					if (SetAsChild)
					{
						for (int num = Singleton<Speaker>.Instance.transform.childCount - 1; num >= 0; num--)
						{
							Transform child = Singleton<Speaker>.Instance.transform.GetChild(num);
							if (child != platformProviderTuple.CustomVoiceProvider.transform)
							{
								Object.Destroy(child.gameObject);
							}
						}
						platformProviderTuple.CustomVoiceProvider.transform.SetParent(Singleton<Speaker>.Instance.transform);
					}
				}
				flag = true;
				break;
			}
		}
		if (flag)
		{
			return;
		}
		if (DefaultVoiceProvider == null)
		{
			Singleton<Speaker>.Instance.CustomMode = false;
			return;
		}
		Singleton<Speaker>.Instance.CustomProvider = DefaultVoiceProvider;
		Singleton<Speaker>.Instance.CustomMode = true;
		if (SetAsChild)
		{
			for (int num2 = Singleton<Speaker>.Instance.transform.childCount - 1; num2 >= 0; num2--)
			{
				Object.Destroy(Singleton<Speaker>.Instance.transform.GetChild(num2).gameObject);
			}
			DefaultVoiceProvider.transform.SetParent(Singleton<Speaker>.Instance.transform);
		}
	}
}
