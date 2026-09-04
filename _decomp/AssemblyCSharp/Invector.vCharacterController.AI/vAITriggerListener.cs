using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI;

[DisallowMultipleComponent]
public class vAITriggerListener : MonoBehaviour, vIAIComponent
{
	[Serializable]
	public class AITriggerEvent : UnityEvent<Collider>
	{
	}

	public bool test;

	public vTagMask tagsToDetect;

	public LayerMask layersToDetect;

	public AITriggerEvent onTriggerEnter;

	public AITriggerEvent onTriggerExit;

	public virtual Type ComponentType => typeof(vAITriggerListener);

	public virtual List<Collider> colliders { get; protected set; }

	private void Start()
	{
		colliders = new List<Collider>();
	}

	protected virtual void OnTriggerEnter(Collider other)
	{
		if (tagsToDetect.Contains(other.gameObject.tag) && layersToDetect.ContainsLayer(other.gameObject.layer) && !colliders.Contains(other))
		{
			onTriggerEnter.Invoke(other);
			colliders.Add(other);
		}
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		if (tagsToDetect.Contains(other.gameObject.tag) && layersToDetect.ContainsLayer(other.gameObject.layer) && colliders.Contains(other))
		{
			onTriggerExit.Invoke(other);
			colliders.Remove(other);
		}
	}
}
