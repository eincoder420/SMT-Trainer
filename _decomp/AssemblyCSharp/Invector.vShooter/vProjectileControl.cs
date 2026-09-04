using System;
using System.Collections.Generic;
using Invector.vEventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vShooter;

[vClassHeader("Projectile Control", "The damage value is changed from minDamage, maxDamage, DropOffStart, DropOffEnd of the ShooterWeapon", openClose = false)]
public class vProjectileControl : vMonoBehaviour
{
	[Serializable]
	public class ProjectileCastColliderEvent : UnityEvent<RaycastHit>
	{
	}

	[Serializable]
	public class ProjectilePassDamage : UnityEvent<vDamage>
	{
	}

	public vBulletLifeSettings bulletLifeSettings;

	public int bulletLife = 100;

	public bool debugTrajetory;

	public bool debugHittedObject;

	public vDamage damage;

	public float forceMultiplier = 1f;

	public bool destroyOnCast = true;

	[Tooltip("Control Trail renderer")]
	public TrailRenderer trail;

	public ProjectilePassDamage onPassDamage;

	public ProjectileCastColliderEvent onCastCollider;

	public ProjectileCastColliderEvent onDestroyProjectile;

	public vProjectileInstantiateData instantiateData;

	internal bool damageByDistance;

	internal float velocity = 580f;

	internal int minDamage;

	internal int maxDamage;

	internal float minDamageDistance = 8f;

	internal float maxDamageDistance = 50f;

	internal Vector3 startPosition;

	internal LayerMask hitLayer = -1;

	internal List<string> ignoreTags = new List<string>();

	internal Transform shooterTransform;

	protected Vector3 previousPosition;

	protected Rigidbody _rigidBody;

	protected Color debugColor = Color.green;

	protected int debugLife;

	protected float castDist;

	protected List<Vector3> trajectoryPositions = new List<Vector3>();

	protected virtual void Start()
	{
		base.transform.SetParent(vObjectContainer.root, worldPositionStays: true);
		debugLife = bulletLife;
		_rigidBody = GetComponent<Rigidbody>();
		startPosition = base.transform.position;
		previousPosition = base.transform.position - base.transform.forward * 0.1f;
		if ((bool)trail)
		{
			AddTrailPosition();
		}
	}

	protected virtual void Update()
	{
		if (_rigidBody.velocity.magnitude > 1f)
		{
			base.transform.rotation = Quaternion.LookRotation(_rigidBody.velocity.normalized, base.transform.up);
		}
		if (Physics.Linecast(previousPosition, base.transform.position + base.transform.forward * 0.5f, out var hitInfo, hitLayer))
		{
			if (!hitInfo.collider)
			{
				return;
			}
			float num = Vector3.Distance(startPosition, base.transform.position) + castDist;
			if (!ignoreTags.Contains(hitInfo.collider.gameObject.tag) && (!(shooterTransform != null) || !hitInfo.collider.transform.IsChildOf(shooterTransform)))
			{
				if (debugHittedObject)
				{
					Debug.Log(hitInfo.collider.gameObject.name, hitInfo.collider);
				}
				onCastCollider.Invoke(hitInfo);
				damage.damageValue = maxDamage;
				if (damageByDistance)
				{
					float num2 = 0f;
					int num3 = maxDamage - minDamage;
					if (num - minDamageDistance >= 0f)
					{
						num2 = Mathf.Clamp((float)(int)Math.Round((double)(100f * (num - minDamageDistance)) / (double)(maxDamageDistance - minDamageDistance)) * 0.01f, 0f, 1f);
						damage.damageValue = maxDamage - (int)((float)num3 * num2);
					}
					else
					{
						damage.damageValue = maxDamage;
					}
				}
				damage.hitPosition = hitInfo.point;
				damage.receiver = hitInfo.collider.transform;
				damage.force = base.transform.forward * damage.damageValue * forceMultiplier;
				if (damage.damageValue > 0)
				{
					onPassDamage.Invoke(damage);
					hitInfo.collider.gameObject.ApplyDamage(damage, damage.sender ? damage.sender.GetComponent<vIMeleeFighter>() : null);
				}
				Rigidbody component = hitInfo.collider.gameObject.GetComponent<Rigidbody>();
				if ((bool)component)
				{
					component.AddForce(base.transform.forward * damage.damageValue * forceMultiplier, ForceMode.Impulse);
				}
				startPosition = base.transform.position;
				castDist = num;
				if (destroyOnCast)
				{
					if ((bool)bulletLifeSettings)
					{
						vBulletLifeSettings.vBulletLifeInfo reduceLife = bulletLifeSettings.GetReduceLife(hitInfo.collider.gameObject.tag, hitInfo.collider.gameObject.layer);
						bulletLife -= reduceLife.lostLife;
						if (debugTrajetory)
						{
							DrawHitPoint(hitInfo.point);
						}
						bool flag = false;
						if (bulletLife > 0 && !reduceLife.ricochet)
						{
							Vector3 vector2 = (base.transform.position = hitInfo.point + base.transform.forward * 0.001f);
							Vector3 vector3 = vector2;
							if ((bool)trail)
							{
								trail.AddPosition(base.transform.position);
							}
							if (debugTrajetory)
							{
								Debug.DrawLine(base.transform.position, previousPosition, debugColor, 10f);
							}
							for (float num4 = 0f; num4 <= reduceLife.maxThicknessToCross; num4 += 0.01f)
							{
								Vector3 vector4 = vector3 + base.transform.forward * num4;
								if (Physics.Linecast(vector4, vector3))
								{
									hitInfo.point = vector4;
									hitInfo.normal = base.transform.forward;
									onCastCollider.Invoke(hitInfo);
									flag = true;
									break;
								}
							}
							if (flag && (bool)trail)
							{
								AddTrailPosition();
							}
						}
						if (!flag && !reduceLife.ricochet)
						{
							bulletLife = 0;
							base.transform.position = hitInfo.point;
							if (debugTrajetory)
							{
								Debug.DrawLine(base.transform.position, previousPosition, debugColor, 10f);
							}
							onDestroyProjectile.Invoke(hitInfo);
							if ((bool)trail && trail.gameObject != base.gameObject)
							{
								if ((bool)trail)
								{
									AddTrailPosition();
								}
								trail.transform.SetParent(vObjectContainer.root);
							}
							UnityEngine.Object.Destroy(base.gameObject);
							return;
						}
						maxDamage -= maxDamage - maxDamage * reduceLife.lostDamage / 100;
						minDamage -= minDamage - minDamage * reduceLife.lostDamage / 100;
						if (maxDamage < 0)
						{
							maxDamage = 0;
						}
						if (minDamage < 0)
						{
							minDamage = 0;
						}
						float num5 = UnityEngine.Random.Range(reduceLife.minChangeTrajectory, reduceLife.maxChangeTrajectory) * (float)((UnityEngine.Random.Range(-1, 1) >= 0) ? 1 : (-1));
						float num6 = UnityEngine.Random.Range(reduceLife.minChangeTrajectory, reduceLife.maxChangeTrajectory) * (float)((UnityEngine.Random.Range(-1, 1) >= 0) ? 1 : (-1));
						if (num6 > 60f || num6 < -60f)
						{
							num5 = Mathf.Clamp(num5, -15f, 15f);
						}
						if (num5 != 0f || num6 != 0f)
						{
							Vector3 vector5 = Quaternion.Euler(num5, num6, 0f) * _rigidBody.velocity;
							if (vector5 != Vector3.zero)
							{
								_rigidBody.velocity = vector5 * ((!reduceLife.ricochet) ? 1 : (-1));
								base.transform.forward = vector5 * ((!reduceLife.ricochet) ? 1 : (-1));
							}
						}
						if (debugTrajetory)
						{
							float num7 = (float)bulletLife / (float)debugLife * 100f;
							debugColor = ((num7 > 76f) ? Color.green : ((num7 > 51f) ? Color.yellow : ((num7 > 26f) ? new Color(1f, 0.5f, 0f) : Color.red)));
							debugColor.a = 0.5f;
						}
					}
					else
					{
						bulletLife = 0;
					}
					if (bulletLife <= 0 || bulletLifeSettings == null)
					{
						base.transform.position = hitInfo.point;
						if (debugTrajetory)
						{
							Debug.DrawLine(base.transform.position, previousPosition, debugColor, 10f);
						}
						onDestroyProjectile.Invoke(hitInfo);
						if ((bool)trail && trail.gameObject != base.gameObject)
						{
							if ((bool)trail)
							{
								AddTrailPosition();
							}
							trail.transform.SetParent(vObjectContainer.root);
						}
						UnityEngine.Object.Destroy(base.gameObject);
						return;
					}
				}
			}
			else
			{
				base.transform.position = hitInfo.point + base.transform.forward * 0.001f;
				if ((bool)trail && trail.gameObject != base.gameObject && (bool)trail)
				{
					AddTrailPosition();
				}
				if (debugTrajetory)
				{
					Debug.DrawLine(base.transform.position, previousPosition, debugColor, 10f);
				}
			}
		}
		else if (debugTrajetory)
		{
			Debug.DrawLine(base.transform.position, previousPosition, debugColor, 10f);
		}
		previousPosition = base.transform.position;
	}

	private void AddTrailPosition()
	{
		if (trajectoryPositions.Count > 0)
		{
			Vector3 vector = trajectoryPositions[trajectoryPositions.Count - 1];
			float num = Vector3.Distance(vector, base.transform.position);
			Vector3 vector2 = base.transform.position - vector;
			int num2 = (int)(num / 0.5f);
			for (int i = 0; i < num2; i++)
			{
				trajectoryPositions.Add(vector + vector2.normalized * 0.5f);
				if (debugTrajetory)
				{
					Debug.DrawRay(vector, Vector3.up * 0.1f, Color.red, 10f);
				}
				vector += vector2.normalized * 0.5f;
			}
		}
		else
		{
			trajectoryPositions.Add(base.transform.position);
		}
		trail.Clear();
		Vector3[] positions = trajectoryPositions.ToArray();
		trail.AddPositions(positions);
	}

	private void DrawHitPoint(Vector3 point)
	{
		Debug.DrawRay(point, -base.transform.forward * 0.1f, Color.red, 10f);
		Debug.DrawRay(point, base.transform.right * 0.1f, Color.red, 10f);
		Debug.DrawRay(point, -base.transform.right * 0.1f, Color.red, 10f);
		Debug.DrawRay(point, base.transform.up * 0.1f, Color.red, 10f);
		Debug.DrawRay(point, -base.transform.up * 0.1f, Color.red, 10f);
	}

	public void RemoveParentOfOther(Transform other)
	{
		other.SetParent(vObjectContainer.root, worldPositionStays: true);
	}
}
