using System;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

[Serializable]
public class OnDead : UnityEvent<GameObject>
{
}
