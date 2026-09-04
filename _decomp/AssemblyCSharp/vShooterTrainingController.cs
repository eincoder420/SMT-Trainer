using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class vShooterTrainingController : MonoBehaviour
{
	public vShooterScore shooterScore;

	public float timeToStartTraining = 3f;

	public float timeToFinishTraining = 60f;

	public Text timeDisplay;

	public UnityEvent onInit;

	public UnityEvent onStartCounter;

	public UnityEvent onFinishCounter;

	public UnityEvent onCancelTraining;

	private Coroutine currentRoutine;

	private float currentTime;

	public bool initOnStart;

	private void Start()
	{
		if ((bool)shooterScore && initOnStart)
		{
			StartTraining();
		}
	}

	public void StartTraining()
	{
		if ((bool)shooterScore)
		{
			shooterScore.StartScore();
			FinishTraining();
			currentRoutine = StartCoroutine(RunTraining());
		}
	}

	public void CancelTraining()
	{
		if ((bool)shooterScore && currentRoutine != null)
		{
			StopCoroutine(currentRoutine);
			currentRoutine = null;
			onCancelTraining.Invoke();
		}
	}

	public void FinishTraining()
	{
		if ((bool)shooterScore)
		{
			if (currentRoutine != null)
			{
				StopCoroutine(currentRoutine);
				currentRoutine = null;
				shooterScore.FinishScore();
			}
			timeDisplay.text = "";
		}
	}

	private IEnumerator RunTraining()
	{
		onInit.Invoke();
		yield return new WaitForSeconds(1f);
		timeDisplay.text = "";
		float timeToEnd = Time.time + timeToStartTraining;
		while (timeToEnd > Time.time)
		{
			timeDisplay.text = (timeToEnd - Time.time).ToString("00");
			yield return null;
		}
		yield return new WaitForSeconds(0.2f);
		timeDisplay.text = "";
		yield return new WaitForSeconds(0.2f);
		timeDisplay.text = "GO!";
		yield return new WaitForSeconds(1f);
		timeDisplay.text = "";
		onStartCounter.Invoke();
		yield return new WaitForSeconds(0.2f);
		timeToEnd = Time.time + timeToFinishTraining;
		while (timeToEnd > Time.time)
		{
			timeDisplay.text = (timeToEnd - Time.time).ToString("00");
			yield return null;
		}
		onFinishCounter.Invoke();
		shooterScore.FinishScore();
		timeDisplay.text = "";
	}
}
