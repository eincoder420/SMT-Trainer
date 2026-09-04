using System.Collections.Generic;
using UnityEngine;

namespace Invector;

public class vTriggerSoundByState : StateMachineBehaviour
{
	public GameObject audioSource;

	public List<AudioClip> sounds;

	public float triggerTime;

	private vFisherYatesRandom _random;

	private bool isTrigger;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		isTrigger = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime % 1f >= triggerTime && !isTrigger)
		{
			TriggerSound(animator, stateInfo, layerIndex);
		}
		else if (stateInfo.normalizedTime % 1f < triggerTime && isTrigger)
		{
			isTrigger = false;
		}
	}

	private void TriggerSound(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_random == null)
		{
			_random = new vFisherYatesRandom();
		}
		isTrigger = true;
		GameObject gameObject = null;
		if (audioSource != null)
		{
			gameObject = Object.Instantiate(audioSource.gameObject, animator.transform.position, Quaternion.identity);
		}
		else
		{
			gameObject = new GameObject("audioObject");
			gameObject.transform.position = animator.transform.position;
		}
		if (gameObject != null)
		{
			AudioSource component = gameObject.gameObject.GetComponent<AudioSource>();
			AudioClip clip = sounds[_random.Next(sounds.Count)];
			component.PlayOneShot(clip);
		}
	}
}
