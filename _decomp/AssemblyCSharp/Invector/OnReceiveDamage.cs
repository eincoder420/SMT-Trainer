using System;
using UnityEngine.Events;

namespace Invector;

[Serializable]
public class OnReceiveDamage : UnityEvent<vDamage>
{
}
