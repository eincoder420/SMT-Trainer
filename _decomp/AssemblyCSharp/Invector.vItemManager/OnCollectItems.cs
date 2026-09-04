using System;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager;

[Serializable]
public class OnCollectItems : UnityEvent<GameObject>
{
}
