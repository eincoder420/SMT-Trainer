using System.Runtime.CompilerServices;
using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController;

[vClassHeader("DAMAGE RECEIVER", "You can add damage multiplier for example causing twice damage on Headshots", openClose = false)]
[vClassHeader("DAMAGE RECEIVER", "You can add damage multiplier for example causing twice damage on Headshots", openClose = false)]
public class vDamageReceiver : vMonoBehaviour, vIDamageReceiver, vIAttackReceiver
{
	[vEditorToolbar("Default", false, "", false, false)]
	public float damageMultiplier = 1f;

	[HideInInspector]
	public vRagdoll ragdoll;

	public bool overrideReactionID;

	[vHideInInspector("overrideReactionID", false)]
	public int reactionID;

	[vEditorToolbar("Random", false, "", false, false)]
	public bool useRandomValues;

	[vHideInInspector("useRandomValues", false)]
	public bool fixedValues;

	[vHideInInspector("useRandomValues", false)]
	public float minDamageMultiplier;

	[vHideInInspector("useRandomValues", false)]
	public float maxDamageMultiplier;

	[vHideInInspector("useRandomValues", false)]
	public int minReactionID;

	[vHideInInspector("useRandomValues", false)]
	public int maxReactionID;

	[vHideInInspector("useRandomValues;fixedValues", false)]
	[Tooltip("Change Between 0 and 100")]
	public float changeToMaxValue;

	public GameObject targetReceiver;

	public vIHealthController healthController;

	[SerializeField]
	protected OnReceiveDamage _onStartReceiveDamage = new OnReceiveDamage();

	[SerializeField]
	protected OnReceiveDamage _onReceiveDamage = new OnReceiveDamage();

	public UnityEvent OnGetMaxValue;

	public OnReceiveDamage onStartReceiveDamage
	{
		get
		{
			return _onStartReceiveDamage;
		}
		protected set
		{
			_onStartReceiveDamage = value;
		}
	}

	public OnReceiveDamage onReceiveDamage
	{
		get
		{
			return _onReceiveDamage;
		}
		protected set
		{
			_onReceiveDamage = value;
		}
	}

	protected virtual bool randomChange => Random.Range(0f, 100f) < changeToMaxValue;

	protected virtual void Start()
	{
		ragdoll = GetComponentInParent<vRagdoll>();
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		if (collision != null && (bool)ragdoll && ragdoll.isActive)
		{
			ragdoll.OnRagdollCollisionEnter(new vRagdollCollision(base.gameObject, collision));
		}
	}

	public virtual void TakeDamage(vDamage damage)
	{
		if (healthController == null && (bool)targetReceiver)
		{
			healthController = targetReceiver.GetComponent<vIHealthController>();
		}
		else if (healthController == null)
		{
			healthController = GetComponentInParent<vIHealthController>();
		}
		if (healthController != null)
		{
			onStartReceiveDamage.Invoke(damage);
			vDamage vDamage = ApplyDamageModifiers(damage);
			healthController.TakeDamage(vDamage);
			onReceiveDamage.Invoke(vDamage);
		}
	}

	public virtual vDamage ApplyDamageModifiers(vDamage damage)
	{
		float num = ((useRandomValues && !fixedValues) ? Random.Range(minDamageMultiplier, maxDamageMultiplier) : ((useRandomValues && fixedValues) ? (randomChange ? maxDamageMultiplier : minDamageMultiplier) : damageMultiplier));
		vDamage damage2 = new vDamage(damage);
		damage2.damageValue *= (int)num;
		if (num == maxDamageMultiplier)
		{
			OnGetMaxValue.Invoke();
		}
		OverrideReaction(ref damage2);
		return damage2;
	}

	protected virtual void OverrideReaction(ref vDamage damage)
	{
		if (overrideReactionID)
		{
			if (useRandomValues && !fixedValues)
			{
				damage.reaction_id = Random.Range(minReactionID, maxReactionID);
			}
			else if (useRandomValues && fixedValues)
			{
				damage.reaction_id = (randomChange ? maxReactionID : minReactionID);
			}
			else
			{
				damage.reaction_id = reactionID;
			}
		}
	}

	public void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
	{
		if ((bool)ragdoll && !ragdoll.iChar.isDead)
		{
			vDamage vDamage = ApplyDamageModifiers(damage);
			ragdoll.gameObject.ApplyDamage(vDamage, attacker);
			onReceiveDamage.Invoke(vDamage);
		}
		else if ((bool)targetReceiver)
		{
			vDamage vDamage2 = ApplyDamageModifiers(damage);
			targetReceiver.gameObject.ApplyDamage(vDamage2, attacker);
			onReceiveDamage.Invoke(vDamage2);
		}
		else
		{
			TakeDamage(damage);
		}
	}

	[SpecialName]
	Transform vIDamageReceiver.get_transform()
	{
		return base.transform;
	}

	[SpecialName]
	GameObject vIDamageReceiver.get_gameObject()
	{
		return base.gameObject;
	}
}
