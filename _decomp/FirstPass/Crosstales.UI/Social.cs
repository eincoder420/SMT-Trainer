using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.UI;

public class Social : MonoBehaviour
{
	public void Facebook()
	{
		NetworkHelper.OpenURL("https://www.facebook.com/crosstales/");
	}

	public void Twitter()
	{
		NetworkHelper.OpenURL("https://twitter.com/crosstales");
	}

	public void LinkedIn()
	{
		NetworkHelper.OpenURL("https://www.linkedin.com/company/crosstales");
	}

	public void Youtube()
	{
		NetworkHelper.OpenURL("https://www.youtube.com/c/Crosstales");
	}

	public void Discord()
	{
		NetworkHelper.OpenURL("https://discord.gg/ZbZ2sh4");
	}
}
