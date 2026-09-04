using System;
using UnityEngine;

namespace Invector.vItemManager;

[Serializable]
public class vEquipProperties
{
	public vItem targetItem;

	public GameObject sender;

	public Transform targetEquipPoint;
}
