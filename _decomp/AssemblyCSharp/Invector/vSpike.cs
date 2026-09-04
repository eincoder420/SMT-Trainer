using Invector.vCharacterController;
using UnityEngine;

namespace Invector;

public class vSpike : MonoBehaviour
{
	private HingeJoint joint;

	[HideInInspector]
	public vSpikeControl control;

	private bool inConect;

	private Transform impaled;

	private void Start()
	{
		joint = GetComponent<HingeJoint>();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!(collision.rigidbody != null) || !(collision.collider.GetComponent<vDamageReceiver>() != null) || inConect)
		{
			return;
		}
		bool num = control == null || !control.attachColliders.Contains(collision.collider.transform);
		if ((bool)control)
		{
			control.attachColliders.Add(collision.collider.transform);
		}
		if (num)
		{
			inConect = true;
			if ((bool)joint && (bool)collision.rigidbody)
			{
				joint.connectedBody = collision.rigidbody;
			}
			impaled = collision.transform;
			Rigidbody[] componentsInChildren = collision.transform.root.GetComponentsInChildren<Rigidbody>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].velocity = Vector3.zero;
			}
			vDamageReceiver component = collision.collider.GetComponent<vDamageReceiver>();
			if ((bool)component && (bool)component.ragdoll && component.ragdoll.iChar != null)
			{
				component.ragdoll.iChar.ChangeHealth((int)(0f - component.ragdoll.iChar.currentHealth));
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.transform != null && impaled != null && other.transform == impaled)
		{
			if ((bool)joint)
			{
				joint.connectedBody = null;
			}
			impaled = null;
			if (control != null && control.attachColliders.Contains(impaled))
			{
				control.attachColliders.Remove(impaled);
			}
			inConect = false;
		}
	}
}
