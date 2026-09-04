using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.RTVoice.Util;

public class SetupProject
{
	static SetupProject()
	{
		setup();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void setup()
	{
		Singleton<Speaker>.PrefabPath = "Prefabs/RTVoice";
		Singleton<Speaker>.GameObjectName = "RTVoice";
		Singleton<GlobalCache>.PrefabPath = "Prefabs/GlobalCache";
	}
}
