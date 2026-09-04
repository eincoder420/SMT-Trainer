using System;
using UnityEngine.Events;

namespace Invector.vItemManager;

[Serializable]
public class OnHandleItemEvent : UnityEvent<vItem>
{
}
