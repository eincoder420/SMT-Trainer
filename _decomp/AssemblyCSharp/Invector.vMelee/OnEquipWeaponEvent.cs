using System;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vMelee;

[Serializable]
public class OnEquipWeaponEvent : UnityEvent<GameObject, bool>
{
}
