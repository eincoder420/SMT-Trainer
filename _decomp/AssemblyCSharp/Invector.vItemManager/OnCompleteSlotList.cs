using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Invector.vItemManager;

[Serializable]
public class OnCompleteSlotList : UnityEvent<List<vItemSlot>>
{
}
