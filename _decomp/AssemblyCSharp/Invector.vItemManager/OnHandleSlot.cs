using System;
using UnityEngine.Events;

namespace Invector.vItemManager;

[Serializable]
public class OnHandleSlot : UnityEvent<vItemSlot>
{
}
