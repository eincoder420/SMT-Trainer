using UnityEngine;

namespace Crosstales.RTVoice.Demo;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_demo_1_1_sequence_caller.html")]
public class SequenceCaller : MonoBehaviour
{
	public GameObject receiver;

	public int NumberOfSequences;

	public float SequenceDelay = 1f;

	private void Start()
	{
		for (int i = 0; i < NumberOfSequences; i++)
		{
			Invoke("playNextSequence", (float)i * SequenceDelay);
		}
	}

	private void playNextSequence()
	{
		receiver.SendMessage("PlayNextSequence");
	}
}
