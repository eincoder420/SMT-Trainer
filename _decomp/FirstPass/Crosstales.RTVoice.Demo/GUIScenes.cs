using Crosstales.Common.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Crosstales.RTVoice.Demo;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_g_u_i_scenes.html")]
public class GUIScenes : MonoBehaviour
{
	[Tooltip("Name of the previous scene.")]
	public string PreviousScene;

	[Tooltip("Name of the previous scene (WebGL only).")]
	public string PreviousSceneWebGL;

	[Tooltip("Name of the next scene.")]
	public string NextScene;

	[Tooltip("Name of the next scene (WebGL only).")]
	public string NextSceneWebGL;

	public void LoadPreviousScene()
	{
		Singleton<Speaker>.Instance.Silence();
		SceneManager.LoadScene(BaseHelper.isWebGLPlatform ? PreviousSceneWebGL : PreviousScene);
	}

	public void LoadNextScene()
	{
		Singleton<Speaker>.Instance.Silence();
		SceneManager.LoadScene(BaseHelper.isWebGLPlatform ? NextSceneWebGL : NextScene);
	}
}
