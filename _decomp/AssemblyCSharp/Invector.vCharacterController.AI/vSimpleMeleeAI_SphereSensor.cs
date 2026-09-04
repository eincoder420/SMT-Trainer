using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI;

public class vSimpleMeleeAI_SphereSensor : MonoBehaviour
{
	public Transform root;

	public List<Transform> targetsInArea;

	protected bool getFromDistance;

	protected float lastDetectionDistance;

	protected virtual void Start()
	{
		targetsInArea = new List<Transform>();
	}

	public virtual void AddTarget(Transform _transform)
	{
		if (!targetsInArea.Contains(_transform))
		{
			targetsInArea.Add(_transform);
		}
	}

	public virtual void SetColliderRadius(float radius)
	{
		SphereCollider component = GetComponent<SphereCollider>();
		if ((bool)component)
		{
			component.radius = radius;
		}
	}

	public virtual Transform GetTargetTransform()
	{
		if (targetsInArea.Count > 0)
		{
			SortTargets();
			if (targetsInArea.Count > 0)
			{
				return targetsInArea[0].transform;
			}
		}
		return null;
	}

	public virtual vIHealthController GetTargetvCharacter()
	{
		if (targetsInArea.Count > 0)
		{
			SortCharacters();
			if (targetsInArea.Count > 0)
			{
				vIHealthController component = targetsInArea[0].GetComponent<vIHealthController>();
				if (component != null && component.currentHealth > 0f)
				{
					return component;
				}
			}
		}
		return null;
	}

	protected virtual void SortCharacters()
	{
		for (int num = targetsInArea.Count - 1; num >= 0; num--)
		{
			Transform transform = targetsInArea[num];
			float num2 = Vector3.Distance(base.transform.position, targetsInArea[num].transform.position);
			if (transform == null || num2 > lastDetectionDistance || transform.GetComponent<vIHealthController>() == null)
			{
				targetsInArea.RemoveAt(num);
			}
		}
		if (getFromDistance)
		{
			targetsInArea.Sort((Transform c1, Transform c2) => Vector3.Distance(base.transform.position, (c1 != null) ? c1.transform.position : (Vector3.one * float.PositiveInfinity)).CompareTo(Vector3.Distance(base.transform.position, (c2 != null) ? c2.transform.position : (Vector3.one * float.PositiveInfinity))));
		}
	}

	protected virtual void SortTargets()
	{
		for (int num = targetsInArea.Count - 1; num >= 0; num--)
		{
			Transform obj = targetsInArea[num];
			float num2 = Vector3.Distance(base.transform.position, targetsInArea[num].transform.position);
			if (obj == null || num2 > lastDetectionDistance)
			{
				targetsInArea.RemoveAt(num);
			}
		}
		if (getFromDistance)
		{
			targetsInArea.Sort((Transform c1, Transform c2) => Vector3.Distance(base.transform.position, (c1 != null) ? c1.transform.position : (Vector3.one * float.PositiveInfinity)).CompareTo(Vector3.Distance(base.transform.position, (c2 != null) ? c2.transform.position : (Vector3.one * float.PositiveInfinity))));
		}
	}

	public virtual void CheckTargetsAround(float FOV, float minDistance, float maxDistance, vTagMask detectTags, LayerMask detectMask, bool getTargetFromDistance = false)
	{
		getFromDistance = getTargetFromDistance;
		lastDetectionDistance = maxDistance;
		Collider[] array = Physics.OverlapSphere(base.transform.position, maxDistance, detectMask);
		array = Array.FindAll(array, (Collider t) => (bool)root && root != t.transform && detectTags != null && detectTags.Count > 0 && detectTags.Contains(t.gameObject.tag) && InFovAngle(t.transform, minDistance, FOV));
		targetsInArea = Array.ConvertAll(array, (Collider c) => c.transform).vToList();
	}

	protected virtual bool InFovAngle(Transform target, float minDistance, float FOV)
	{
		if (Vector3.Distance(base.transform.position, target.position) < minDistance)
		{
			return true;
		}
		Quaternion quaternion = Quaternion.LookRotation(target.position - base.transform.position, Vector3.up);
		Vector3 eulerAngles = base.transform.eulerAngles;
		Vector3 eulerAngle = quaternion.eulerAngles - eulerAngles;
		float y = eulerAngle.NormalizeAngle().y;
		float x = eulerAngle.NormalizeAngle().x;
		if (y <= FOV * 0.5f && y >= 0f - FOV * 0.5f && x <= FOV * 0.5f && x >= 0f - FOV * 0.5f)
		{
			return true;
		}
		return false;
	}
}
