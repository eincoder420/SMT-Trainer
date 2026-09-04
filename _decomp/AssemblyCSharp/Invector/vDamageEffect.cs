using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

[Serializable]
public class vDamageEffect
{
	public string damageType = "";

	public List<GameObject> customDamageEffect;

	public bool rotateToHitDirection = true;

	[Tooltip("Attach prefab in Damage Receiver transform")]
	public bool attachInReceiver;

	public UnityEvent onTriggerEffect;
}
