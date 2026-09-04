using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

public class vBarrel : vHealthController
{
	public Transform referenceTransformUP;

	public float maxAngleUp = 90f;

	protected bool isBarrelRoll;

	public UnityEvent onBarrelRoll;

	public List<string> acceptableAttacks = new List<string> { "explosion", "projectile" };

	private void OnCollisionEnter()
	{
		if ((bool)referenceTransformUP && Vector3.Angle(referenceTransformUP.up, Vector3.up) > maxAngleUp && !isBarrelRoll)
		{
			isBarrelRoll = true;
			onBarrelRoll.Invoke();
		}
	}

	public override void TakeDamage(vDamage damage)
	{
		if (acceptableAttacks.Contains(damage.damageType))
		{
			base.TakeDamage(damage);
		}
	}
}
