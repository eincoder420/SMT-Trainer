using UnityEngine;

namespace Invector.vCharacterController;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class vHeadTrackSensor : MonoBehaviour
{
	[HideInInspector]
	public vHeadTrack headTrack;

	public SphereCollider sphere;

	private void OnDrawGizmos()
	{
		if (Application.isPlaying && (bool)sphere && (bool)headTrack)
		{
			sphere.radius = headTrack.distanceToDetect;
		}
	}

	private void Start()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		sphere = GetComponent<SphereCollider>();
		sphere.isTrigger = true;
		component.useGravity = false;
		component.isKinematic = true;
		component.constraints = RigidbodyConstraints.FreezeAll;
		if ((bool)headTrack)
		{
			sphere.radius = headTrack.distanceToDetect;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (headTrack != null)
		{
			headTrack.OnDetect(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (headTrack != null)
		{
			headTrack.OnLost(other);
		}
	}
}
