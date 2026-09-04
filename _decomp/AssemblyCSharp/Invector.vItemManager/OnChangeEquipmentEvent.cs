using System;
using UnityEngine.Events;

namespace Invector.vItemManager;

[Serializable]
public class OnChangeEquipmentEvent : UnityEvent<vEquipArea, vItem>
{
}
