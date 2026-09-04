using System;
using UnityEngine.Events;

namespace Invector.vItemManager;

[Serializable]
public class OnChangeItemAmount : UnityEvent<vItem, int>
{
}
