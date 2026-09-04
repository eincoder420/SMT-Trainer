using UnityEngine;

namespace Invector.vEventSystems;

public class vAnimatorTag : vAnimatorTagBase
{
	public string[] tags = new string[1] { "CustomAction" };

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (stateInfos != null)
		{
			for (int i = 0; i < tags.Length; i++)
			{
				for (int j = 0; j < stateInfos.Count; j++)
				{
					stateInfos[j].AddStateInfo(tags[i], layerIndex);
				}
			}
		}
		OnStateEnterEvent(tags.vToList());
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (stateInfos != null)
		{
			for (int i = 0; i < stateInfos.Count; i++)
			{
				stateInfos[i].UpdateStateInfo(layerIndex, stateInfo.normalizedTime, stateInfo.shortNameHash);
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfos != null)
		{
			for (int i = 0; i < tags.Length; i++)
			{
				for (int j = 0; j < stateInfos.Count; j++)
				{
					stateInfos[j].RemoveStateInfo(tags[i], layerIndex);
				}
			}
		}
		base.OnStateExit(animator, stateInfo, layerIndex);
		OnStateExitEvent(tags.vToList());
	}
}
