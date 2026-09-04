using System;
using UnityEngine;

namespace Invector.vMelee;

[Serializable]
public class vBodyMember
{
	public Transform transform;

	public string bodyPart;

	public vMeleeAttackObject attackObject;

	public bool isHuman;

	public void SetActiveDamage(bool active)
	{
		attackObject.SetActiveDamage(active);
	}
}
