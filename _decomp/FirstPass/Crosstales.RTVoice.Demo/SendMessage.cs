using System.Collections;
using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.RTVoice.Demo;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_send_message.html")]
public class SendMessage : MonoBehaviour
{
	[TextArea]
	public string TextA = "RT-Voice works great with PlayMaker, SALSA, Localized Dialogs/Cutscenes, Dialogue System for Unity and THE Dialogue Engine - that's awesome!";

	[TextArea]
	public string TextB = "Absolutely true! RT-Voice is fantastic.";

	public float DelayTextB = 12.2f;

	public bool PlayOnStart;

	private GameObject receiver;

	private void Start()
	{
		receiver = Singleton<Speaker>.Instance.gameObject;
		if (PlayOnStart)
		{
			Play();
		}
	}

	public void Play()
	{
		SpeakerA();
		StartCoroutine(SpeakerB());
	}

	public void SpeakerA()
	{
		receiver.SendMessage("SpeakLive", TextA + ";en;" + (BaseHelper.isWindowsPlatform ? "Microsoft David Desktop" : "Alex"));
	}

	public IEnumerator SpeakerB()
	{
		yield return new WaitForSeconds(DelayTextB);
		receiver.SendMessage("SpeakLive", new string[3]
		{
			TextB,
			"en",
			BaseHelper.isWindowsPlatform ? "Microsoft Zira Desktop" : "Vicki"
		});
	}

	public void Silence()
	{
		StopAllCoroutines();
		receiver.SendMessage("SilenceLive");
	}
}
