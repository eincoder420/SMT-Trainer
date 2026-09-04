using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.UI;

public class StaticManager : MonoBehaviour
{
	public void Quit()
	{
		Application.Quit();
	}

	public void OpenCrosstales()
	{
		NetworkHelper.OpenURL("https://www.crosstales.com");
	}

	public void OpenAssetstore()
	{
		NetworkHelper.OpenURL("https://assetstore.unity.com/lists/crosstales-42213?aid=1011lNGT");
	}
}
