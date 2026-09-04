using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vShooter;

[vClassHeader("Decal Manager", true, "icon_v2", false, "", openClose = false)]
public class vDecalManager : vMonoBehaviour
{
	[Serializable]
	public class DecalObject
	{
		public string tag;

		[SerializeField]
		protected vImpactEffectBase impactEffect;

		[SerializeField]
		protected List<vImpactEffectBase> additionalEffects;

		public void CreateEffect(Vector3 position, Quaternion rotation, GameObject impactSender, GameObject impactReceiver)
		{
			impactEffect.DoImpactEffect(position, rotation, impactSender, impactReceiver);
			for (int i = 0; i < additionalEffects.Count; i++)
			{
				additionalEffects[i].DoImpactEffect(position, rotation, impactSender, impactReceiver);
			}
		}
	}

	public LayerMask layermask;

	public List<DecalObject> decalObjects;

	public virtual void CreateDecal(RaycastHit hitInfo)
	{
		CreateDecal(hitInfo.collider.gameObject, hitInfo.point, hitInfo.normal);
	}

	public virtual void CreateDecal(GameObject target, Vector3 position, Vector3 normal)
	{
		if ((int)layermask == ((int)layermask | (1 << target.layer)))
		{
			DecalObject decal = GetDecal(target.tag);
			if (decal != null && Physics.SphereCast(new Ray(position + normal * 0.1f, -normal), 0.0001f, out var hitInfo, 1f, layermask) && hitInfo.collider.gameObject == target)
			{
				Quaternion rotation = Quaternion.LookRotation(hitInfo.normal, Vector3.up);
				Vector3 point = hitInfo.point;
				decal.CreateEffect(point, rotation, base.gameObject, target);
			}
		}
	}

	protected virtual DecalObject GetDecal(string tag)
	{
		return decalObjects.Find((DecalObject d) => d.tag.Equals(tag));
	}
}
