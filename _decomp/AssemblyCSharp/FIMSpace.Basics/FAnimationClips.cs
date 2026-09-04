using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Basics;

public class FAnimationClips : Dictionary<string, int>
{
	public readonly Animator Animator;

	public string CurrentAnimation { get; private set; }

	public string PreviousAnimation { get; private set; }

	public FAnimationClips(Animator animator)
	{
		Animator = animator;
		CurrentAnimation = "";
		PreviousAnimation = "";
	}

	public void Add(string clipName, bool exactClipName = false)
	{
		AddClip(clipName, exactClipName);
	}

	public void AddClip(string clipName, bool exactClipName = false)
	{
		AddClip(Animator, clipName, exactClipName);
	}

	public void AddClip(Animator animator, string clipName, bool exactClipName = false)
	{
		if (!animator)
		{
			Debug.LogError("No animator!");
			return;
		}
		string text = "";
		if (!exactClipName)
		{
			if (FAnimatorMethods.StateExists(animator, clipName))
			{
				text = clipName;
			}
			else if (FAnimatorMethods.StateExists(animator, FStringMethods.CapitalizeFirstLetter(clipName)))
			{
				text = FStringMethods.CapitalizeFirstLetter(clipName);
			}
			else if (FAnimatorMethods.StateExists(animator, clipName.ToLower()))
			{
				text = clipName.ToLower();
			}
			else if (FAnimatorMethods.StateExists(animator, clipName.ToUpper()))
			{
				text = clipName.ToUpper();
			}
		}
		else if (FAnimatorMethods.StateExists(animator, clipName))
		{
			text = clipName;
		}
		if (text == "")
		{
			Debug.LogWarning("Clip with name " + clipName + " not exists in animator from game object " + animator.gameObject.name);
		}
		else if (!ContainsKey(clipName))
		{
			Add(clipName, Animator.StringToHash(text));
		}
	}

	public void CrossFadeInFixedTime(string clip, float transitionTime = 0.25f, int layer = 0, float timeOffset = 0f, bool startOver = false)
	{
		if (ContainsKey(clip))
		{
			RefreshClipMemory(clip);
			if (startOver)
			{
				Animator.CrossFadeInFixedTime(base[clip], transitionTime, layer, timeOffset);
			}
			else if (!IsPlaying(clip))
			{
				Animator.CrossFadeInFixedTime(base[clip], transitionTime, layer, timeOffset);
			}
		}
	}

	public void CrossFade(string clip, float transitionTime = 0.25f, int layer = 0, float timeOffset = 0f, bool startOver = false)
	{
		if (ContainsKey(clip))
		{
			RefreshClipMemory(clip);
			if (startOver)
			{
				Animator.CrossFade(base[clip], transitionTime, layer, timeOffset);
			}
			else if (!IsPlaying(clip))
			{
				Animator.CrossFade(base[clip], transitionTime, layer, timeOffset);
			}
		}
	}

	private void RefreshClipMemory(string name)
	{
		if (name != CurrentAnimation)
		{
			PreviousAnimation = CurrentAnimation;
			CurrentAnimation = name;
		}
	}

	public void SetFloat(string parameter, float value = 0f, float deltaSpeed = 60f)
	{
		float @float = Animator.GetFloat(parameter);
		@float = ((!(deltaSpeed >= 60f)) ? FLogicMethods.FLerp(@float, value, Time.deltaTime * deltaSpeed) : value);
		Animator.SetFloat(parameter, @float);
	}

	public void SetFloatUnscaledDelta(string parameter, float value = 0f, float deltaSpeed = 60f)
	{
		float @float = Animator.GetFloat(parameter);
		@float = ((!(deltaSpeed >= 60f)) ? FLogicMethods.FLerp(@float, value, Time.unscaledDeltaTime * deltaSpeed) : value);
		Animator.SetFloat(parameter, @float);
	}

	internal bool IsPlaying(string clip)
	{
		if (Animator.IsInTransition(0))
		{
			if (Animator.GetNextAnimatorStateInfo(0).shortNameHash == base[clip])
			{
				return true;
			}
		}
		else if (Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == base[clip])
		{
			return true;
		}
		return false;
	}
}
