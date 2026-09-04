using System;
using UnityEngine.Events;

namespace Crosstales.RTVoice;

[Serializable]
public class ProviderChangeEvent : UnityEvent<string>
{
}
