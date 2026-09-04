using System.Collections.Generic;
using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vMelee;

[vClassHeader("Melee Object", true, "icon_v2", false, "", openClose = false)]
public class vMeleeAttackObject : vMonoBehaviour
{
	[vReadOnly(false)]
	public string attackObjectName;

	public vDamage damage;

	public Transform overrideDamageSender;

	public List<vHitBox> hitBoxes;

	public int damageModifier;

	[HideInInspector]
	public bool canApplyDamage;

	[vHelpBox("Event called when attack was successful", vHelpBoxAttribute.MessageType.None)]
	public OnHitEnter onDamageHit;

	[vHelpBox("Event called when the attack causes recoil", vHelpBoxAttribute.MessageType.None)]
	public OnHitEnter onRecoilHit;

	[vHelpBox("Event called when causes damage ", vHelpBoxAttribute.MessageType.None)]
	public OnReceiveDamage onPassDamage;

	[vHelpBox("Events called when  Damage applier (HitBoxes) is enabled or disabled ", vHelpBoxAttribute.MessageType.None)]
	public UnityEvent onEnableDamage;

	public UnityEvent onDisableDamage;

	private Dictionary<vHitBox, List<GameObject>> targetColliders;

	[HideInInspector]
	public vMeleeManager meleeManager;

	protected virtual void Start()
	{
		targetColliders = new Dictionary<vHitBox, List<GameObject>>();
		if (hitBoxes.Count > 0)
		{
			foreach (vHitBox hitBox in hitBoxes)
			{
				hitBox.attackObject = this;
				targetColliders.Add(hitBox, new List<GameObject>());
			}
			return;
		}
		base.enabled = false;
	}

	public virtual void SetActiveDamage(bool value)
	{
		canApplyDamage = value;
		for (int i = 0; i < hitBoxes.Count; i++)
		{
			vHitBox vHitBox2 = hitBoxes[i];
			vHitBox2.trigger.enabled = value;
			if (!value && targetColliders != null)
			{
				targetColliders[vHitBox2].Clear();
			}
		}
		if (value)
		{
			onEnableDamage.Invoke();
		}
		else
		{
			onDisableDamage.Invoke();
		}
	}

	public virtual void OnHit(vHitBox hitBox, Collider other)
	{
		if (!canApplyDamage || targetColliders[hitBox].Contains(other.gameObject) || !(meleeManager != null) || !(other.gameObject != meleeManager.gameObject))
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (meleeManager == null)
		{
			meleeManager = GetComponentInParent<vMeleeManager>();
		}
		HitProperties hitProperties = meleeManager.hitProperties;
		if (((hitBox.triggerType & vHitBoxType.Damage) != 0 && hitProperties.hitDamageTags == null) || hitProperties.hitDamageTags.Count == 0)
		{
			flag = true;
		}
		else if ((hitBox.triggerType & vHitBoxType.Damage) != 0 && hitProperties.hitDamageTags.Contains(other.tag))
		{
			flag = true;
		}
		else if ((hitBox.triggerType & vHitBoxType.Recoil) != 0 && (int)hitProperties.hitRecoilLayer == ((int)hitProperties.hitRecoilLayer | (1 << other.gameObject.layer)))
		{
			flag2 = true;
		}
		if (!(flag || flag2))
		{
			return;
		}
		targetColliders[hitBox].Add(other.gameObject);
		vHitInfo hitInfo = new vHitInfo(this, hitBox, other, hitBox.transform.position);
		if (flag)
		{
			if ((bool)meleeManager)
			{
				meleeManager.OnDamageHit(ref hitInfo);
			}
			else
			{
				damage.sender = (overrideDamageSender ? overrideDamageSender : base.transform);
			}
			if (!hitInfo.targetIsBlocking)
			{
				onDamageHit.Invoke(hitInfo);
			}
		}
		if (flag2)
		{
			if ((bool)meleeManager)
			{
				meleeManager.OnRecoilHit(hitInfo);
			}
			onRecoilHit.Invoke(hitInfo);
		}
	}

	public bool ApplyDamage(vHitBox hitBox, Collider other, vDamage damage)
	{
		vDamage vDamage = new vDamage(damage);
		vDamage.receiver = other.transform;
		vDamage.damageValue = Mathf.RoundToInt((float)(damage.damageValue + damageModifier) * ((float)hitBox.damagePercentage * 0.01f));
		vDamage.hitPosition = hitBox.transform.position;
		other.gameObject.ApplyDamage(vDamage, meleeManager.fighter);
		if (vDamage.hitReaction && vDamage.damageValue > 0)
		{
			onPassDamage.Invoke(vDamage);
		}
		return vDamage.hitReaction;
	}
}
