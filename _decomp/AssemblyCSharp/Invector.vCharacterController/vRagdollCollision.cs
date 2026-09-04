using UnityEngine;

namespace Invector.vCharacterController;

public class vRagdollCollision
{
	private GameObject sender;

	private Collision collision;

	private float impactForce;

	public GameObject Sender => sender;

	public Collision Collision => collision;

	public float ImpactForce => impactForce;

	public vRagdollCollision(GameObject sender, Collision collision)
	{
		this.sender = sender;
		this.collision = collision;
		impactForce = collision.relativeVelocity.magnitude;
	}
}
