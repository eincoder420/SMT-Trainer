using System.Collections;
using System.Linq;
using Crosstales.Common.Util;
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice.Model.Enum;
using UnityEngine;

namespace Crosstales.RTVoice.Tool;

[HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_tool_1_1_sequencer.html")]
public class Sequencer : MonoBehaviour
{
	[Header("Configuration")]
	[Tooltip("All available sequences.")]
	public Sequence[] Sequences;

	[Tooltip("Delay in seconds before the Sequencer starts processing (default: 0).")]
	public float Delay;

	[Header("Behaviour Settings")]
	[Tooltip("Enable the Sequencer on start (default: false).")]
	public bool PlayOnStart;

	private int currentIndex;

	private string uidCurrentSpeaker;

	private bool playAllSequences;

	private bool played;

	public Sequence CurrentSequence => Sequences[currentIndex];

	private void Start()
	{
		Singleton<Speaker>.Instance.OnVoicesReady += onVoicesReady;
		Singleton<Speaker>.Instance.OnSpeakComplete += speakCompleteMethod;
		play();
	}

	private void OnDestroy()
	{
		if (Singleton<Speaker>.Instance != null)
		{
			Singleton<Speaker>.Instance.OnSpeakComplete -= speakCompleteMethod;
			Singleton<Speaker>.Instance.OnVoicesReady -= onVoicesReady;
		}
	}

	private void OnValidate()
	{
		if (Delay < 0f)
		{
			Delay = 0f;
		}
		foreach (Sequence item in Sequences.Where((Sequence seq) => !seq.Initialized))
		{
			item.Rate = 1f;
			item.Pitch = 1f;
			item.Volume = 1f;
			item.Initialized = true;
		}
	}

	public void PlaySequence(int index = 0)
	{
		if (Sequences != null)
		{
			if (index >= 0 && index < Sequences.Length)
			{
				StartCoroutine(playMe(Sequences[index]));
				currentIndex = index + 1;
			}
			else
			{
				Debug.LogWarning("The given index is outside the range of Sequences: " + index, this);
			}
		}
		else
		{
			Debug.LogWarning("Sequences is null!", this);
		}
	}

	public void PlayNextSequence()
	{
		PlaySequence(currentIndex);
	}

	public void PlayAllSequences()
	{
		StopAllSequences();
		playAllSequences = true;
		PlaySequence();
	}

	public void StopAllSequences()
	{
		StopAllCoroutines();
		Singleton<Speaker>.Instance.Silence();
		playAllSequences = false;
	}

	private void speakCompleteMethod(Wrapper wrapper)
	{
		if (playAllSequences)
		{
			if (wrapper.Uid.Equals(uidCurrentSpeaker) && currentIndex < Sequences.Length)
			{
				PlayNextSequence();
			}
			else
			{
				StopAllSequences();
			}
		}
	}

	private void onVoicesReady()
	{
		play();
	}

	private void play()
	{
		if (PlayOnStart && !played && Singleton<Speaker>.Instance.Voices.Count > 0)
		{
			played = true;
			PlayAllSequences();
		}
	}

	private IEnumerator playMe(Sequence seq)
	{
		yield return new WaitForSeconds(Delay);
		uidCurrentSpeaker = ((seq.Mode == SpeakMode.Speak) ? Singleton<Speaker>.Instance.Speak(seq.Text, seq.Source, seq.Voices.Voice, speakImmediately: true, seq.Rate, seq.Pitch, seq.Volume) : Singleton<Speaker>.Instance.SpeakNative(seq.Text, seq.Voices.Voice, seq.Rate, seq.Pitch, seq.Volume));
	}
}
