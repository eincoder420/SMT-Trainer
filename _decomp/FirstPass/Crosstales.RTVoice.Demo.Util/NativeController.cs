using System.Linq;
using UnityEngine;

namespace Crosstales.RTVoice.Demo.Util;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_util_1_1_native_controller.html")]
public class NativeController : MonoBehaviour
{
	[Header("Configuration")]
	[Tooltip("Enable or disable the 'Objects' for native mode (default: true).")]
	public bool Active = true;

	[Header("Objects")]
	[Tooltip("Selected objects for the controller.")]
	public GameObject[] Objects;

	private void Update()
	{
		foreach (GameObject item in Objects.Where((GameObject go) => go != null))
		{
			if (GUISpeech.isNative)
			{
				item.SetActive(Active);
			}
			else
			{
				item.SetActive(!Active);
			}
		}
	}
}
