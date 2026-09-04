using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.Utils;

[vClassHeader("Events With Delay", true, "icon_v2", false, "")]
public class vEventWithDelay : vMonoBehaviour
{
	[Serializable]
	public class vEventWithDelayObject
	{
		public string name = "EventName";

		public float delay;

		public UnityEvent onDoEvent;
	}

	public bool triggerOnStart;

	public bool triggerOnEnable;

	[vHideInInspector("triggerOnStart", false)]
	public bool all;

	[vHideInInspector("triggerOnStart", false)]
	public int eventIndex;

	[SerializeField]
	private vEventWithDelayObject[] events = new vEventWithDelayObject[0];

	private void OnEnable()
	{
		if (triggerOnEnable)
		{
			if (all)
			{
				DoEvents();
			}
			else
			{
				DoEvent(eventIndex);
			}
		}
	}

	private void Start()
	{
		if (triggerOnStart)
		{
			if (all)
			{
				DoEvents();
			}
			else
			{
				DoEvent(eventIndex);
			}
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	public void DoEvents()
	{
		for (int i = 0; i < events.Length; i++)
		{
			StartCoroutine(DoEventWithDelay(events[i]));
		}
	}

	public void DoEvent(int index)
	{
		if (index < events.Length && events.Length != 0)
		{
			StartCoroutine(DoEventWithDelay(events[index]));
		}
	}

	public void DoEvent(string name)
	{
		vEventWithDelayObject vEventWithDelayObject = Array.Find(events, (vEventWithDelayObject e) => e.name.Equals(name));
		if (vEventWithDelayObject != null)
		{
			StartCoroutine(DoEventWithDelay(vEventWithDelayObject));
		}
	}

	private IEnumerator DoEventWithDelay(vEventWithDelayObject _event)
	{
		yield return new WaitForSeconds(_event.delay);
		_event.onDoEvent.Invoke();
	}
}
