using UnityEngine;
using UnityEngine.Events;

namespace FIMSpace.Basics;

public class FBasic_TriggerEvents : MonoBehaviour
{
	public string EnteringTag = "Player";

	public UnityEvent OnAwakeEvent;

	public UnityEvent OnStartEvent;

	public UnityEvent OnTriggerEnterEvents;

	public UnityEvent OnTriggerExitEvents;

	private void Awake()
	{
		if (OnAwakeEvent != null)
		{
			OnAwakeEvent.Invoke();
		}
	}

	private void Start()
	{
		if (OnStartEvent != null)
		{
			OnStartEvent.Invoke();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == EnteringTag && OnTriggerEnterEvents != null)
		{
			OnTriggerEnterEvents.Invoke();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == EnteringTag && OnTriggerExitEvents != null)
		{
			OnTriggerExitEvents.Invoke();
		}
	}
}
