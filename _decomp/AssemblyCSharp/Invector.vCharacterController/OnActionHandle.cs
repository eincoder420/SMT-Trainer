using System;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController;

[Serializable]
public class OnActionHandle : UnityEvent<Collider>
{
}
