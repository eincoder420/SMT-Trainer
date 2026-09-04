using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

[RequireComponent(typeof(BoxCollider))]
[vClassHeader("SimpleTrigger", true, "icon_v2", false, "", openClose = false, useHelpBox = true, helpBoxText = "Tags and Layer To Detect : Use this to filter tags and layer that can interact with trigger, Select Nothing  to ignore filter")]
public class vSimpleTrigger : vMonoBehaviour
{
	[Serializable]
	public class vTriggerEvent : UnityEvent<Collider>
	{
	}

	public static bool drawGizmos = true;

	[vButton("ToggleGizmos", "ToggleGizmos", typeof(vSimpleTrigger), false)]
	public bool useFilter = true;

	public bool debugMode;

	public vTagMask tagsToDetect = new List<string> { "Player" };

	public LayerMask layersToDetect = 0;

	public vTriggerEvent onTriggerEnter;

	public vTriggerEvent onTriggerExit;

	public vTriggerEvent onTriggerStay;

	protected bool inCollision;

	protected bool triggerStay;

	protected Collider other;

	protected BoxCollider _selfCollider;

	public virtual BoxCollider selfCollider
	{
		get
		{
			if (!_selfCollider && base.transform.GetComponent<BoxCollider>() == null)
			{
				_selfCollider = base.gameObject.AddComponent<BoxCollider>();
			}
			else if (!_selfCollider)
			{
				_selfCollider = base.transform.GetComponent<BoxCollider>();
			}
			return _selfCollider;
		}
		protected set
		{
			_selfCollider = value;
		}
	}

	public void ToggleGizmos()
	{
		drawGizmos = !drawGizmos;
	}

	protected virtual void OnDrawGizmos()
	{
		if (drawGizmos)
		{
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
			Vector3 center = selfCollider.center;
			Vector3 one = Vector3.one;
			one.x *= selfCollider.size.x;
			one.y *= selfCollider.size.y;
			one.z *= selfCollider.size.z;
			Gizmos.color = Color.green * 0.8f;
			Gizmos.DrawWireCube(center, one);
			Color color = new Color(1f, 0f, 0f, 0.2f);
			Color color2 = new Color(0f, 1f, 0f, 0.2f);
			Gizmos.color = ((inCollision && Application.isPlaying) ? color : color2);
			Gizmos.DrawCube(center, one);
		}
	}

	protected virtual void Start()
	{
		inCollision = false;
		selfCollider.isTrigger = true;
	}

	protected virtual void OnTriggerEnter(Collider other)
	{
		if (this.other == null && IsInTagMask(other.gameObject.tag) && IsInLayerMask(other.gameObject.layer))
		{
			inCollision = true;
			this.other = other;
			onTriggerEnter.Invoke(other);
			if (debugMode)
			{
				Debug.Log(other.gameObject.name + "TriggerEnter");
			}
			if (base.enabled && base.gameObject.activeInHierarchy)
			{
				StartCoroutine(TriggerStayRoutine());
			}
		}
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		if (this.other != null && this.other.gameObject == other.gameObject)
		{
			inCollision = false;
			onTriggerExit.Invoke(other);
			if (debugMode)
			{
				Debug.Log(other.gameObject.name + "TriggerExit");
			}
			this.other = null;
		}
	}

	protected virtual bool IsInTagMask(string tag)
	{
		if (tagsToDetect.Count == 0)
		{
			return true;
		}
		return tagsToDetect.Contains(tag);
	}

	protected virtual bool IsInLayerMask(int layer)
	{
		if (layersToDetect.value != 0)
		{
			return (layersToDetect.value & (1 << layer)) > 0;
		}
		return true;
	}

	protected IEnumerator TriggerStayRoutine()
	{
		while (other != null)
		{
			if (other == null || !other.gameObject.activeInHierarchy)
			{
				OnTriggerExit(other);
				break;
			}
			onTriggerStay.Invoke(other);
			if (debugMode)
			{
				Debug.Log(other.gameObject.name + "TriggerStay");
			}
			yield return null;
		}
	}
}
