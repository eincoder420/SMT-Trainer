using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.vActions;

[vClassHeader("Generic Animation", "Use this script to trigger a simple animation.")]
public class vGenericAnimation : vMonoBehaviour
{
	[Tooltip("Input to trigger the custom animation")]
	public GenericInput actionInput = new GenericInput("L", "A", "A");

	[Tooltip("Name of the animation clip")]
	public string animationClip;

	[Tooltip("Where in the end of the animation will trigger the event OnEndAnimation")]
	public float animationEnd = 0.8f;

	public UnityEvent OnPlayAnimation;

	public UnityEvent OnEndAnimation;

	protected bool isPlaying;

	protected bool triggerOnce;

	protected vThirdPersonInput tpInput;

	protected virtual void Start()
	{
		tpInput = GetComponent<vThirdPersonInput>();
	}

	protected virtual void LateUpdate()
	{
		TriggerAnimation();
		AnimationBehaviour();
	}

	protected virtual void TriggerAnimation()
	{
		bool flag = !isPlaying && !tpInput.cc.customAction && !string.IsNullOrEmpty(animationClip);
		if (actionInput.GetButtonDown() && flag)
		{
			PlayAnimation();
		}
	}

	public virtual void PlayAnimation()
	{
		triggerOnce = true;
		OnPlayAnimation.Invoke();
		tpInput.cc.animator.CrossFadeInFixedTime(animationClip, 0.1f);
	}

	protected virtual void AnimationBehaviour()
	{
		isPlaying = tpInput.cc.baseLayerInfo.IsName(animationClip);
		if (isPlaying && tpInput.cc.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= animationEnd && triggerOnce)
		{
			triggerOnce = false;
			OnEndAnimation.Invoke();
		}
	}
}
