using System;
using UnityEngine.Events;

namespace Crosstales.RTVoice;

[Serializable]
public class ErrorEvent : UnityEvent<string, string>
{
}
