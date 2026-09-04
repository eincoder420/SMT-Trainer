using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Invector.vShooter;

[vClassHeader("Shooter Weapon", true, "icon_v2", false, "", openClose = false)]
public class vShooterWeaponBase : vMonoBehaviour
{
	[Serializable]
	public class OnDestroyEvent : UnityEvent<GameObject>
	{
	}

	[Serializable]
	public class OnInstantiateProjectile : UnityEvent<vProjectileControl>
	{
	}

	[vEditorToolbar("Weapon Settings", false, "", false, false)]
	[Tooltip("The category of the weapon\n Used to the IK offset system. \nExample: HandGun, Pistol, Machine-Gun")]
	public string weaponCategory = "MyCategory";

	[Tooltip("Frequency of shots")]
	public float shootFrequency;

	[vEditorToolbar("Ammo", false, "", false, false)]
	public bool isInfinityAmmo;

	[Tooltip("Starting ammo")]
	[SerializeField]
	[vHideInInspector("isInfinityAmmo", true)]
	public int ammo;

	[vEditorToolbar("Layer & Tag", false, "", false, false)]
	public List<string> ignoreTags = new List<string>();

	public LayerMask hitLayer = 1;

	[vEditorToolbar("Projectile", false, "", false, false)]
	[Tooltip("Prefab of the projectile")]
	public GameObject projectile;

	[Tooltip("Assign the muzzle of your weapon")]
	public Transform muzzle;

	[Tooltip("How many projectiles will spawn per shot")]
	[Range(1f, 20f)]
	public int projectilesPerShot = 1;

	[Range(0f, 90f)]
	[Tooltip("how much dispersion the weapon have")]
	public float dispersion;

	[Range(0f, 1000f)]
	[Tooltip("Velocity of your projectile")]
	public float velocity = 380f;

	[Tooltip("Use the DropOffStart and DropOffEnd to calc damage by distance using min and max damage")]
	public bool damageByDistance = true;

	[Tooltip("Min distance to apply damage, used to evaluate the damage between minDamage and maxDamage")]
	[FormerlySerializedAs("DropOffStart")]
	public float minDamageDistance = 8f;

	[Tooltip("Max distance to apply damage, used to evaluate the damage between minDamage and maxDamage")]
	[FormerlySerializedAs("DropOffEnd")]
	public float maxDamageDistance = 50f;

	[Tooltip("Minimum damage caused by the shot, regardless the distance")]
	public int minDamage;

	[Tooltip("Maximum damage caused by the close shot")]
	public int maxDamage;

	[vEditorToolbar("Audio & VFX", false, "", false, false)]
	[Header("Audio")]
	public AudioSource source;

	public AudioClip fireClip;

	public AudioClip emptyClip;

	[Header("Effects")]
	public bool testShootEffect;

	public Light lightOnShot;

	[SerializeField]
	public ParticleSystem[] emittShurykenParticle;

	protected Transform sender;

	[HideInInspector]
	public OnDestroyEvent onDestroy;

	[vEditorToolbar("Events", false, "", false, false)]
	public UnityEvent onShot;

	[vEditorToolbar("Events", false, "", false, false)]
	public UnityEvent onEmptyClip;

	public OnInstantiateProjectile onInstantiateProjectile;

	protected float _nextShootTime;

	protected float _nextEmptyClipTime;

	public virtual float velocityMultiplierMod { get; set; }

	public virtual float damageMultiplierMod { get; set; }

	public virtual string weaponName => base.gameObject.name.Replace("(Clone)", string.Empty);

	public virtual bool CanDoShot => _nextShootTime < Time.time;

	public virtual bool CanDoEmptyClip => _nextEmptyClipTime < Time.time;

	protected virtual float damageMultiplier => 1f + damageMultiplierMod;

	protected virtual float velocityMultiplier => 1f + velocityMultiplierMod;

	public virtual void Shoot()
	{
		Shoot(muzzle.position + muzzle.forward * 100f);
	}

	public virtual void Shoot(Transform _sender = null, UnityAction<bool> successfulShot = null)
	{
		Shoot(muzzle.position + muzzle.forward * 100f, _sender, successfulShot);
	}

	public virtual void Shoot(Vector3 aimPosition, Transform _sender = null, UnityAction<bool> successfulShot = null)
	{
		if (HasAmmo())
		{
			if (CanDoShot)
			{
				UseAmmo();
				sender = ((_sender != null) ? _sender : base.transform);
				HandleShot(aimPosition);
				successfulShot?.Invoke(arg0: true);
				_nextShootTime = Time.time + shootFrequency;
				_nextEmptyClipTime = _nextShootTime;
			}
		}
		else if (CanDoEmptyClip)
		{
			EmptyClipEffect();
			successfulShot?.Invoke(arg0: false);
			_nextEmptyClipTime = Time.time + shootFrequency;
		}
	}

	public virtual void UseAmmo(int count = 1)
	{
		if (ammo > 0)
		{
			ammo -= count;
			if (ammo <= 0)
			{
				ammo = 0;
			}
		}
	}

	public virtual bool HasAmmo()
	{
		if (!isInfinityAmmo)
		{
			return ammo > 0;
		}
		return true;
	}

	protected virtual void OnDestroy()
	{
		onDestroy.Invoke(base.gameObject);
	}

	private void OnApplicationQuit()
	{
		onDestroy.RemoveAllListeners();
	}

	protected virtual void HandleShot(Vector3 aimPosition)
	{
		ShootBullet(aimPosition);
		ShotEffect();
	}

	protected virtual Vector3 Dispersion(Vector3 aim, float distance, float variance)
	{
		aim.Normalize();
		Vector3 zero = Vector3.zero;
		do
		{
			zero = UnityEngine.Random.insideUnitSphere;
		}
		while (zero == aim || zero == -aim);
		zero = Vector3.Cross(aim, zero);
		zero *= UnityEngine.Random.Range(0f, variance);
		return aim * distance + zero;
	}

	protected virtual void ShootBullet(Vector3 aimPosition)
	{
		Vector3 vector = aimPosition - muzzle.position;
		Quaternion rotation = Quaternion.LookRotation(vector);
		GameObject gameObject = null;
		float num = 0f;
		if (dispersion > 0f && (bool)projectile)
		{
			for (int i = 0; i < projectilesPerShot; i++)
			{
				Vector3 vector2 = Dispersion(vector.normalized, maxDamageDistance, dispersion);
				Quaternion rotation2 = Quaternion.LookRotation(vector2);
				gameObject = UnityEngine.Object.Instantiate(projectile, muzzle.transform.position, rotation2);
				vProjectileControl component = gameObject.GetComponent<vProjectileControl>();
				component.shooterTransform = sender;
				component.ignoreTags = ignoreTags;
				component.hitLayer = hitLayer;
				component.damage.sender = sender;
				component.startPosition = gameObject.transform.position;
				component.damageByDistance = damageByDistance;
				component.maxDamage = (int)((float)(maxDamage / projectilesPerShot) * damageMultiplier);
				component.minDamage = (int)((float)(minDamage / projectilesPerShot) * damageMultiplier);
				component.minDamageDistance = minDamageDistance;
				component.maxDamageDistance = maxDamageDistance;
				onInstantiateProjectile.Invoke(component);
				num = velocity * velocityMultiplier;
				StartCoroutine(ApplyForceToBullet(gameObject, vector2, num));
				component = CreateProjectileData(aimPosition, num, vector2, component);
			}
		}
		else if (projectilesPerShot > 0 && (bool)projectile)
		{
			gameObject = UnityEngine.Object.Instantiate(projectile, muzzle.transform.position, rotation);
			vProjectileControl component2 = gameObject.GetComponent<vProjectileControl>();
			component2.shooterTransform = sender;
			component2.ignoreTags = ignoreTags;
			component2.hitLayer = hitLayer;
			component2.damage.sender = sender;
			component2.startPosition = gameObject.transform.position;
			component2.damageByDistance = damageByDistance;
			component2.maxDamage = (int)((float)(maxDamage / projectilesPerShot) * damageMultiplier);
			component2.minDamage = (int)((float)(minDamage / projectilesPerShot) * damageMultiplier);
			component2.minDamageDistance = minDamageDistance;
			component2.maxDamageDistance = maxDamageDistance;
			onInstantiateProjectile.Invoke(component2);
			num = velocity * velocityMultiplier;
			StartCoroutine(ApplyForceToBullet(gameObject, vector, num));
		}
	}

	protected virtual vProjectileControl CreateProjectileData(Vector3 aimPosition, float velocityChanged, Vector3 dispersionDir, vProjectileControl pCtrl)
	{
		pCtrl.instantiateData = new vProjectileInstantiateData
		{
			aimPos = aimPosition,
			dir = dispersionDir,
			vel = velocityChanged
		};
		return pCtrl;
	}

	protected virtual IEnumerator ApplyForceToBullet(GameObject bulletObject, Vector3 direction, float velocityChanged)
	{
		yield return new WaitForSeconds(0.01f);
		try
		{
			Rigidbody component = bulletObject.GetComponent<Rigidbody>();
			component.mass /= projectilesPerShot;
			component.AddForce(direction.normalized * velocityChanged, ForceMode.VelocityChange);
		}
		catch
		{
		}
	}

	protected virtual void ShotEffect()
	{
		onShot.Invoke();
		StopCoroutine(LightOnShoot());
		if ((bool)source && (bool)fireClip)
		{
			source.PlayOneShot(fireClip);
		}
		StartCoroutine(LightOnShoot(0.037f));
		StartEmitters();
	}

	protected virtual void StopSound()
	{
		if ((bool)source)
		{
			source.Stop();
		}
	}

	protected virtual IEnumerator LightOnShoot(float time = 0f)
	{
		if ((bool)lightOnShot)
		{
			lightOnShot.enabled = true;
			yield return new WaitForSeconds(time);
			lightOnShot.enabled = false;
		}
	}

	protected virtual void StartEmitters()
	{
		if (emittShurykenParticle != null)
		{
			ParticleSystem[] array = emittShurykenParticle;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Emit(1);
			}
		}
	}

	protected virtual void StopEmitters()
	{
		if (emittShurykenParticle != null)
		{
			ParticleSystem[] array = emittShurykenParticle;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Stop();
			}
		}
	}

	protected virtual void EmptyClipEffect()
	{
		if ((bool)source && (bool)emptyClip)
		{
			source.PlayOneShot(emptyClip);
		}
		onEmptyClip.Invoke();
	}
}
