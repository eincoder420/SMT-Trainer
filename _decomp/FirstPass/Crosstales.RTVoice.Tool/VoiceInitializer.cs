using System.Collections;
using System.Linq;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using Crosstales.RTVoice.Util;
using UnityEngine;

namespace Crosstales.RTVoice.Tool;

[HelpURL("https://crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_tool_1_1_voice_initializer.html")]
public class VoiceInitializer : MonoBehaviour
{
	[Header("Configuration")]
	[Tooltip("Selected provider to initialize the voices (default: Any).")]
	public ProviderType Provider = ProviderType.Any;

	[Tooltip("Initialize voices by name.")]
	public string[] VoiceNames;

	[Tooltip("Initialize all voices (default: false).")]
	public bool AllVoices;

	[Header("Behaviour Settings")]
	[Tooltip("Destroy the gameobject after initialize (default: true).")]
	public bool DestroyWhenFinished = true;

	private string activeUid = string.Empty;

	private string completedUid = string.Empty;

	private const string text = "crosstales";

	private void Start()
	{
		Singleton<Speaker>.Instance.OnVoicesReady += onVoicesReady;
		Singleton<Speaker>.Instance.OnSpeakComplete += onSpeakComplete;
		if (!BaseHelper.isEditorMode)
		{
			Object.DontDestroyOnLoad(base.transform.root.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnVoicesReady -= onVoicesReady;
			Singleton<Speaker>.Instance.OnSpeakComplete -= onSpeakComplete;
		}
	}

	private IEnumerator initializeVoices()
	{
		if (AllVoices)
		{
			foreach (Voice voice in Singleton<Speaker>.Instance.Voices)
			{
				activeUid = Singleton<Speaker>.Instance.SpeakNative("crosstales", voice, 3f, 1f, 0f);
				do
				{
					yield return null;
				}
				while (!activeUid.Equals(completedUid));
			}
		}
		else
		{
			foreach (Voice item in from voiceName in VoiceNames
				where !string.IsNullOrEmpty(voiceName)
				where Singleton<Speaker>.Instance.isVoiceForNameAvailable(voiceName)
				select Singleton<Speaker>.Instance.VoiceForName(voiceName))
			{
				activeUid = Singleton<Speaker>.Instance.SpeakNative("crosstales", item, 3f, 1f, 0f);
				do
				{
					yield return null;
				}
				while (!activeUid.Equals(completedUid));
			}
		}
		if (DestroyWhenFinished)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void onVoicesReady()
	{
		if (Provider == ProviderType.Any || Provider == Helper.CurrentProviderType)
		{
			StopAllCoroutines();
			StartCoroutine(initializeVoices());
		}
		else if (DestroyWhenFinished)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void onSpeakComplete(Wrapper wrapper)
	{
		completedUid = wrapper.Uid;
	}
}
