using UnityEngine;

namespace Invector.vMelee;

public class vHitInfo
{
	public vMeleeAttackObject attackObject;

	public vHitBox hitBox;

	public Vector3 hitPoint;

	public Collider targetCollider;

	public bool targetIsBlocking;

	public vHitInfo(vMeleeAttackObject attackObject, vHitBox hitBox, Collider targetCollider, Vector3 hitPoint)
	{
		this.attackObject = attackObject;
		this.hitBox = hitBox;
		this.targetCollider = targetCollider;
		this.hitPoint = hitPoint;
	}
}
