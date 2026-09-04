using UnityEngine;
using UnityEngine.Events;

namespace Invector;

public class vFootStepTrigger : MonoBehaviour
{
	protected Collider _trigger;

	protected vFootStepBase _fT;

	public UnityEvent OnStep;

	protected Collider lastCollider;

	internal FootStepObject footstepObj;

	public Collider trigger
	{
		get
		{
			if (_trigger == null)
			{
				_trigger = base.gameObject.GetComponent<Collider>();
			}
			return _trigger;
		}
	}

	private void OnDrawGizmos()
	{
		if ((bool)trigger)
		{
			Color green = Color.green;
			green.a = 0.5f;
			Gizmos.color = green;
			if (trigger is SphereCollider)
			{
				Gizmos.DrawSphere(trigger.bounds.center, (trigger as SphereCollider).radius);
			}
		}
	}

	private void Start()
	{
		_fT = GetComponentInParent<vFootStepBase>();
		Rigidbody component = base.gameObject.GetComponent<Rigidbody>();
		if (component == null)
		{
			base.gameObject.AddComponent<Rigidbody>().isKinematic = true;
		}
		else
		{
			component.isKinematic = true;
		}
		if (_fT == null)
		{
			Debug.Log(base.gameObject.name + " can't find the FootStepFromTexture");
			base.gameObject.SetActive(value: false);
			return;
		}
		Collider[] componentsInChildren = _fT.gameObject.GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider collider in componentsInChildren)
		{
			if (collider != null && collider.gameObject != trigger.gameObject)
			{
				Physics.IgnoreCollision(collider, trigger, ignore: true);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!(_fT == null))
		{
			if (lastCollider == null || lastCollider != other || footstepObj == null)
			{
				footstepObj = new FootStepObject(base.transform, other);
				lastCollider = other;
			}
			if (footstepObj.isTerrain)
			{
				_fT.StepOnTerrain(footstepObj);
				OnStep.Invoke();
			}
			else
			{
				_fT.StepOnMesh(footstepObj);
				OnStep.Invoke();
			}
		}
	}
}
