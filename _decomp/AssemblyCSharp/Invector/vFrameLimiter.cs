using UnityEngine;

namespace Invector;

[vClassHeader("Frame Limiter", false, "icon_v2", false, "")]
public class vFrameLimiter : vMonoBehaviour
{
	public int desiredFPS = 60;

	private void Awake()
	{
		Application.targetFrameRate = desiredFPS;
		QualitySettings.vSyncCount = 0;
	}
}
