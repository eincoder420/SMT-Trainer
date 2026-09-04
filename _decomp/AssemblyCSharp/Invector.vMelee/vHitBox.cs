using UnityEngine;

namespace Invector.vMelee;

[vClassHeader("HitBox", true, "icon_v2", false, "", openClose = false)]
public class vHitBox : vMonoBehaviour
{
	[HideInInspector]
	public vMeleeAttackObject attackObject;

	public int damagePercentage = 100;

	[vEnumFlag]
	public vHitBoxType triggerType = vHitBoxType.Damage | vHitBoxType.Recoil;

	protected bool canHit;

	protected Collider _trigger;

	public Collider trigger
	{
		get
		{
			_trigger = base.gameObject.GetComponent<Collider>();
			if (!_trigger)
			{
				_trigger = base.gameObject.AddComponent<BoxCollider>();
			}
			return _trigger;
		}
	}

	private void OnDrawGizmos()
	{
		Color color = (((triggerType & vHitBoxType.Damage) != 0 && (triggerType & vHitBoxType.Recoil) == 0) ? Color.green : (((triggerType & vHitBoxType.Damage) != 0 && (triggerType & vHitBoxType.Recoil) != 0) ? Color.yellow : (((triggerType & vHitBoxType.Recoil) != 0 && (triggerType & vHitBoxType.Damage) == 0) ? Color.red : Color.black)));
		color.a = 0.6f;
		Gizmos.color = color;
		if (!Application.isPlaying && (bool)trigger && !trigger.enabled)
		{
			trigger.enabled = true;
		}
		if ((bool)trigger && trigger.enabled && (bool)(trigger as BoxCollider))
		{
			BoxCollider boxCollider = trigger as BoxCollider;
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
			Gizmos.DrawCube(boxCollider.center, Vector3.Scale(Vector3.one, boxCollider.size));
		}
	}

	private void Start()
	{
		if ((bool)trigger)
		{
			trigger.isTrigger = true;
			trigger.enabled = false;
		}
		int layer = LayerMask.NameToLayer("Ignore Raycast");
		base.transform.gameObject.layer = layer;
		canHit = (triggerType & vHitBoxType.Damage) != 0 || (triggerType & vHitBoxType.Recoil) != 0;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (TriggerCondictions(other) && attackObject != null)
		{
			attackObject.OnHit(this, other);
		}
	}

	private bool TriggerCondictions(Collider other)
	{
		if (canHit)
		{
			if (attackObject != null)
			{
				if (!(attackObject.meleeManager == null))
				{
					return other.gameObject != attackObject.meleeManager.gameObject;
				}
				return true;
			}
			return false;
		}
		return false;
	}
}
