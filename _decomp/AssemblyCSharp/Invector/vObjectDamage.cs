using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

[vClassHeader("OBJECT DAMAGE", true, "icon_v2", false, "", iconName = "DamageIcon")]
public class vObjectDamage : vMonoBehaviour
{
	[Serializable]
	public class OnHitEvent : UnityEvent<Collider>
	{
	}

	public enum CollisionMethod
	{
		OnTriggerEnter,
		OnColliderEnter,
		OnParticleCollision
	}

	public vDamage damage;

	[Tooltip("Assign this to set other damage sender")]
	public Transform overrideDamageSender;

	[Tooltip("List of layers that can be hit, nothing will apply to all layers")]
	public LayerMask layerToCollide;

	[Tooltip("List of tags that can be hit, nothing will apply to all tags")]
	public vTagMask tags;

	[Tooltip("Check to use the damage Frequence")]
	public bool continuousDamage;

	[Tooltip("Apply damage to each end of the frequency in seconds ")]
	public float damageFrequency = 0.5f;

	private List<Collider> targets;

	private List<Collider> disabledTarget;

	private float currentTime;

	public OnHitEvent onHit;

	public CollisionMethod collisionMethod;

	public ParticleSystem part;

	public bool limitParticleCollisionEvent;

	public int maxParticleCollisionEvent = 1;

	public List<ParticleCollisionEvent> collisionEvents;

	protected virtual void Start()
	{
		targets = new List<Collider>();
		disabledTarget = new List<Collider>();
		if (collisionMethod == CollisionMethod.OnParticleCollision)
		{
			part = GetComponent<ParticleSystem>();
			collisionEvents = new List<ParticleCollisionEvent>();
		}
	}

	protected virtual void Update()
	{
		if (!base.enabled || !continuousDamage || targets == null || targets.Count <= 0)
		{
			return;
		}
		if (currentTime > 0f)
		{
			currentTime -= Time.deltaTime;
			return;
		}
		currentTime = damageFrequency;
		foreach (Collider target in targets)
		{
			if (target != null)
			{
				if (target.enabled)
				{
					ApplyDamage(target, base.transform.position);
				}
				else
				{
					disabledTarget.Add(target);
				}
			}
		}
		if (disabledTarget.Count > 0)
		{
			int num = disabledTarget.Count;
			while (num >= 0 && disabledTarget.Count != 0)
			{
				try
				{
					if (targets.Contains(disabledTarget[num]))
					{
						targets.Remove(disabledTarget[num]);
					}
				}
				catch
				{
					break;
				}
				num--;
			}
		}
		if (disabledTarget.Count > 0)
		{
			disabledTarget.Clear();
		}
	}

	protected virtual void OnCollisionEnter(Collision hit)
	{
		if (base.enabled && collisionMethod == CollisionMethod.OnColliderEnter && !continuousDamage && CanApplyDamage(hit.gameObject))
		{
			ApplyDamage(hit.collider, hit.contacts[0].point);
		}
	}

	protected virtual void OnTriggerEnter(Collider hit)
	{
		if (base.enabled && collisionMethod == CollisionMethod.OnTriggerEnter)
		{
			if (continuousDamage && CanApplyDamage(hit.gameObject) && !targets.Contains(hit))
			{
				targets.Add(hit);
			}
			else if (CanApplyDamage(hit.gameObject))
			{
				ApplyDamage(hit, base.transform.position);
			}
		}
	}

	private bool CanApplyDamage(GameObject hitObject)
	{
		if ((tags.Count != 0 && !tags.Contains(hitObject.tag)) || (int)layerToCollide != 0)
		{
			return layerToCollide.ContainsLayer(hitObject.layer);
		}
		return true;
	}

	protected virtual void OnTriggerExit(Collider hit)
	{
		if (base.enabled && (collisionMethod != CollisionMethod.OnColliderEnter || continuousDamage) && CanApplyDamage(hit.gameObject) && targets.Contains(hit))
		{
			targets.Remove(hit);
		}
	}

	protected virtual void OnParticleCollision(GameObject hit)
	{
		if (!base.enabled || !CanApplyDamage(hit) || collisionMethod != CollisionMethod.OnParticleCollision)
		{
			return;
		}
		int num = part.GetCollisionEvents(hit, collisionEvents);
		Collider component = hit.GetComponent<Collider>();
		for (int i = 0; (!limitParticleCollisionEvent && i < num) || (!limitParticleCollisionEvent && i < maxParticleCollisionEvent); i++)
		{
			if ((bool)component)
			{
				if (continuousDamage && !targets.Contains(component))
				{
					targets.Add(component);
				}
				else
				{
					ApplyDamage(component, base.transform.position);
				}
			}
		}
	}

	public virtual void ClearTargets()
	{
		targets.Clear();
	}

	protected virtual void ApplyDamage(Collider target, Vector3 hitPoint)
	{
		damage.hitReaction = true;
		damage.sender = (overrideDamageSender ? overrideDamageSender : base.transform);
		damage.hitPosition = hitPoint;
		damage.receiver = target.transform;
		target.gameObject.ApplyDamage(new vDamage(damage));
		onHit.Invoke(target);
	}
}
