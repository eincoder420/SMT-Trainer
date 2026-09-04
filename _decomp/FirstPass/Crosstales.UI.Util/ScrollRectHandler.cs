using Crosstales.Common.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.UI.Util;

public class ScrollRectHandler : MonoBehaviour
{
	public ScrollRect Scroll;

	public float WindowsSensitivity = 35f;

	public float MacSensitivity = 25f;

	private void Start()
	{
		if (BaseHelper.isWindowsPlatform)
		{
			Scroll.scrollSensitivity = WindowsSensitivity;
		}
		else if (BaseHelper.isMacOSPlatform)
		{
			Scroll.scrollSensitivity = MacSensitivity;
		}
	}
}
