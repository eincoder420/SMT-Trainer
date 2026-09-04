using System;
using UnityEngine.Events;

namespace Invector.vItemManager;

[Serializable]
public class OnCollectItemEvent : UnityEvent<vItemManager.CollectedItemInfo>
{
}
