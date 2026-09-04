using UnityEngine;
using UnityEngine.Events;

public class vEnableDisableEvent : MonoBehaviour
{
	public UnityEvent onEnable;

	public UnityEvent onDisable;

	private void OnEnable()
	{
		onEnable.Invoke();
	}

	private void OnDisable()
	{
		onDisable.Invoke();
	}
}
