using System;
using UnityEngine.Events;

namespace Invector.vMelee;

[Serializable]
public class OnHitEnter : UnityEvent<vHitInfo>
{
}
