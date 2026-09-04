using System;
using UnityEngine;

namespace Invector.vItemManager;

[Serializable]
public class EquipPoint
{
	[SerializeField]
	public string equipPointName;

	public EquipmentReference equipmentReference = new EquipmentReference();

	[HideInInspector]
	public vEquipArea area;

	public vHandler handler = new vHandler();

	public OnInstantiateItemObjectEvent onInstantiateEquiment = new OnInstantiateItemObjectEvent();
}
