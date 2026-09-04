using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Invector.Utils;

[vClassHeader("Fade Canvas", true, "icon_v2", false, "")]
public class vFadeCanvas : vMonoBehaviour
{
	public CanvasGroup group;

	public float fadeSpeed = 2f;

	public UnityEvent onStartFadeIn;

	public UnityEvent onFinishFadeIn;

	public UnityEvent onStartFadeOut;

	public UnityEvent onFinishFadeOut;

	public Slider.SliderEvent OnChangeValue;

	public bool autoControlCanvasGroup;

	public bool fadeInStart;

	public bool fadeOutStart;

	public bool startWithAlphaZero = true;

	public bool startWithAlphaFull;

	private float currentValue;

	private bool inFade;

	private void Awake()
	{
		if (!group)
		{
			group = GetComponent<CanvasGroup>();
		}
	}

	private void Start()
	{
		InitilizeFadeEffect();
	}

	private void OnEnable()
	{
		InitilizeFadeEffect();
	}

	private void InitilizeFadeEffect()
	{
		if (fadeInStart)
		{
			FadeIn();
		}
		if (fadeOutStart)
		{
			FadeOut();
		}
		if (startWithAlphaZero)
		{
			AlphaZero();
		}
		if (startWithAlphaFull)
		{
			AlphaFull();
		}
	}

	public void AlphaZero()
	{
		if ((bool)group)
		{
			group.alpha = 0f;
		}
	}

	public void AlphaFull()
	{
		if ((bool)group)
		{
			group.alpha = 1f;
		}
	}

	public void FadeIn()
	{
		StartCoroutine(Fade(1f));
	}

	public void FadeOut()
	{
		StartCoroutine(Fade(0f));
	}

	private IEnumerator Fade(float targetValue)
	{
		if (targetValue == 1f)
		{
			onStartFadeIn.Invoke();
			if (autoControlCanvasGroup && (bool)group)
			{
				group.interactable = false;
				group.blocksRaycasts = true;
			}
		}
		else
		{
			if (autoControlCanvasGroup && (bool)group)
			{
				group.interactable = false;
				group.blocksRaycasts = true;
			}
			onStartFadeOut.Invoke();
		}
		inFade = false;
		yield return new WaitForEndOfFrame();
		inFade = true;
		if ((bool)group)
		{
			currentValue = group.alpha;
		}
		while (((targetValue == 1f) ? (currentValue < 1f) : (currentValue > 0f)) && inFade)
		{
			yield return null;
			currentValue = ((targetValue == 1f) ? (currentValue + Time.unscaledDeltaTime * fadeSpeed) : (currentValue - Time.unscaledDeltaTime * fadeSpeed));
			if ((bool)group)
			{
				group.alpha = currentValue;
			}
			OnChangeValue.Invoke(currentValue);
		}
		if (targetValue == 1f)
		{
			onFinishFadeIn.Invoke();
			if (autoControlCanvasGroup && (bool)group)
			{
				group.interactable = true;
				group.blocksRaycasts = true;
			}
		}
		else
		{
			if (autoControlCanvasGroup && (bool)group)
			{
				group.interactable = false;
				group.blocksRaycasts = false;
			}
			onFinishFadeOut.Invoke();
		}
	}
}
