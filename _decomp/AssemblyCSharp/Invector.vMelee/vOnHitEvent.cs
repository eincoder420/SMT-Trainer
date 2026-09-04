using System;
using UnityEngine.Events;

namespace Invector.vMelee;

[Serializable]
public class vOnHitEvent : UnityEvent<vHitInfo>
{
}
