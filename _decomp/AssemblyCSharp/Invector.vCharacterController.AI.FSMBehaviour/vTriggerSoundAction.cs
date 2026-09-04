using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vTriggerSoundAction : vStateAction
{
	public AudioClip[] clips;

	public float minVolume = 0.5f;

	public float maxVolume = 1f;

	public override string categoryName => "Generic/";

	public override string defaultName => "Trigger Sound";

	public vTriggerSoundAction()
	{
		executionType = vFSMComponentExecutionType.OnStateEnter;
	}

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (executionType == vFSMComponentExecutionType.OnStateEnter)
		{
			AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length)], fsmBehaviour.transform.position, Random.Range(minVolume, maxVolume));
		}
	}
}
