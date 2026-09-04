using System;
using System.Collections;
using System.Collections.Generic;
using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

[vClassHeader("Explosive", true, "icon_v2", false, "", openClose = false)]
public class vExplosive : vMonoBehaviour
{
	[Serializable]
	protected class OnUpdateTime : UnityEvent<float>
	{
	}

	public enum ExplosiveMethod
	{
		collisionEnter,
		collisionEnterTimer,
		remote,
		timer,
		remoteTimer
	}

	public vDamage damage;

	public float explosionForce;

	public float minExplosionRadius;

	public float maxExplosionRadius;

	public float upwardsModifier = 1f;

	public ForceMode forceMode;

	public ExplosiveMethod method;

	public LayerMask applyDamageLayer;

	public LayerMask applyForceLayer;

	public float timeToExplode = 10f;

	[Tooltip("conver to progress 0 to 1")]
	public bool normalizeTime;

	public bool showGizmos;

	public UnityEvent onInitTimer;

	[SerializeField]
	protected OnUpdateTime onUpdateTimer;

	public UnityEvent onExplode;

	private bool inTimer;

	private List<GameObject> collidersReached;

	private void OnDrawGizmosSelected()
	{
		if (showGizmos)
		{
			Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
			Gizmos.DrawSphere(base.transform.position, minExplosionRadius);
			Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
			Gizmos.DrawSphere(base.transform.position, maxExplosionRadius);
		}
	}

	public void SetDamage(vDamage damage)
	{
		this.damage = damage;
	}

	protected virtual void Start()
	{
		collidersReached = new List<GameObject>();
		if (method == ExplosiveMethod.timer)
		{
			StartCoroutine(StartTimer());
		}
	}

	protected virtual IEnumerator StartTimer()
	{
		if (!inTimer)
		{
			onInitTimer.Invoke();
			inTimer = true;
			float startTime = Time.time;
			float num = 0f;
			while (num < timeToExplode)
			{
				yield return new WaitForEndOfFrame();
				num = Time.time - startTime;
				onUpdateTimer.Invoke(normalizeTime ? (num / timeToExplode) : num);
			}
			if ((bool)base.gameObject)
			{
				onUpdateTimer.Invoke(normalizeTime ? 1f : timeToExplode);
				Explode();
			}
		}
	}

	protected virtual IEnumerator DestroyBomb()
	{
		yield return new WaitForSeconds(0.1f);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		if (method == ExplosiveMethod.collisionEnter)
		{
			Explode();
		}
		else if (method == ExplosiveMethod.collisionEnterTimer)
		{
			StartCoroutine(StartTimer());
		}
	}

	protected virtual void Explode()
	{
		onExplode.Invoke();
		Collider[] array = Physics.OverlapSphere(base.transform.position, maxExplosionRadius, applyDamageLayer);
		if (collidersReached == null)
		{
			collidersReached = new List<GameObject>();
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null && array[i].gameObject != null && !collidersReached.Contains(array[i].gameObject))
			{
				collidersReached.Add(array[i].gameObject);
				vDamage vDamage2 = new vDamage(damage);
				if (!vDamage2.sender)
				{
					vDamage2.sender = base.transform;
				}
				vDamage2.hitPosition = array[i].ClosestPointOnBounds(base.transform.position);
				vDamage2.receiver = array[i].transform;
				float num = Vector3.Distance(base.transform.position, vDamage2.hitPosition);
				float num2 = ((num <= minExplosionRadius) ? ((float)damage.damageValue) : GetPercentageForce(num, damage.damageValue));
				vDamage2.activeRagdoll = !(num > maxExplosionRadius * 0.5f) && vDamage2.activeRagdoll;
				vDamage2.damageValue = (int)num2;
				array[i].gameObject.ApplyDamage(vDamage2, null);
			}
		}
		StartCoroutine(ApplyExplosionForce());
		StartCoroutine(DestroyBomb());
	}

	protected virtual IEnumerator ApplyExplosionForce()
	{
		yield return new WaitForSeconds(0.1f);
		Collider[] array = Physics.OverlapSphere(base.transform.position, maxExplosionRadius, applyForceLayer);
		for (int i = 0; i < array.Length; i++)
		{
			Rigidbody component = array[i].GetComponent<Rigidbody>();
			float num = Vector3.Distance(base.transform.position, array[i].ClosestPointOnBounds(base.transform.position));
			float num2 = ((num <= minExplosionRadius) ? explosionForce : GetPercentageForce(num, explosionForce));
			if ((bool)component)
			{
				component.AddExplosionForce(num2, base.transform.position, maxExplosionRadius, upwardsModifier, forceMode);
			}
		}
	}

	private float GetPercentageForce(float distance, float value)
	{
		if (distance > maxExplosionRadius)
		{
			distance = maxExplosionRadius;
		}
		float num = maxExplosionRadius - minExplosionRadius;
		float num2 = Mathf.Clamp(distance - minExplosionRadius, 0f, num);
		float num3 = Mathf.Clamp(num - num2, 0f, num) / num * 100f * 0.01f;
		return value * num3;
	}

	public virtual void SetCollisionEnterMethod()
	{
		method = ExplosiveMethod.collisionEnter;
	}

	public virtual void SetCollisionEnterTimerMethod(int timer)
	{
		method = ExplosiveMethod.collisionEnterTimer;
		timeToExplode = timer;
	}

	public virtual void SetRemoveMethod()
	{
		method = ExplosiveMethod.remote;
	}

	public virtual void SetRemoveTimerMethod(int timer)
	{
		method = ExplosiveMethod.remoteTimer;
		timeToExplode = timer;
	}

	public virtual void SetTimerMethod(int timer)
	{
		method = ExplosiveMethod.timer;
		timeToExplode = timer;
	}

	public virtual void ActiveExplosion()
	{
		if (method == ExplosiveMethod.remote)
		{
			Explode();
		}
		else if (method == ExplosiveMethod.remoteTimer)
		{
			StartCoroutine(StartTimer());
		}
	}

	public void RemoveParent()
	{
		base.transform.parent = null;
	}

	public void RemoveParentOfOther(Transform other)
	{
		other.parent = null;
	}
}
