using System;
using UnityEngine;

namespace Invector.vItemManager;

[Serializable]
public class ApplyAttributeEvent
{
	[SerializeField]
	public vItemAttributes attribute;

	[SerializeField]
	public OnApplyAttribute onApplyAttribute;
}
