using System;
using UnityEngine.Events;

namespace UnityEngine.AzureSky;

[Serializable]
public sealed class AzureEventAction
{
	public UnityEvent eventAction;

	public int hour = 6;

	public int minute;

	public int year = 2020;

	public int month = 1;

	public int day = 1;
}
