using System;
using UnityEngine.Events;

namespace Invector.vCharacterController;

[Serializable]
public class OnActiveRagdoll : UnityEvent<vDamage>
{
}
