using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Invector.Utils;

public class vTimerCounter : MonoBehaviour
{
	public float targetTime;

	public bool normalizeResult;

	[SerializeField]
	[vReadOnly(true)]
	protected float timerResult;

	public UnityEvent onStart;

	public UnityEvent onPause;

	public UnityEvent onStop;

	public UnityEvent onFinish;

	public Slider.SliderEvent onTimerUpdated;

	protected float currentTime;

	protected Coroutine timerRoutine;

	public virtual void StartTimer()
	{
		if (timerRoutine != null)
		{
			StopCoroutine(timerRoutine);
		}
		timerRoutine = StartCoroutine(TimerRoutiner());
	}

	public void StopTimer()
	{
		PauseTimer();
		currentTime = 0f;
		onStop.Invoke();
		timerResult = 0f;
		onTimerUpdated.Invoke(0f);
	}

	public void PauseTimer()
	{
		if (timerRoutine != null)
		{
			StopCoroutine(timerRoutine);
		}
		timerRoutine = null;
		onPause.Invoke();
	}

	private IEnumerator TimerRoutiner()
	{
		onStart.Invoke();
		while (currentTime < targetTime)
		{
			currentTime += Time.deltaTime;
			timerResult = (normalizeResult ? (currentTime / targetTime) : currentTime);
			onTimerUpdated.Invoke(timerResult);
			yield return null;
		}
		timerRoutine = null;
		timerResult = (normalizeResult ? 1f : targetTime);
		onTimerUpdated.Invoke(timerResult);
		onFinish.Invoke();
	}
}
