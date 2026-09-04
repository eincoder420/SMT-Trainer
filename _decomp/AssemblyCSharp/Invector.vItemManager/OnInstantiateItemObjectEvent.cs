using System;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager;

[Serializable]
public class OnInstantiateItemObjectEvent : UnityEvent<GameObject>
{
}
