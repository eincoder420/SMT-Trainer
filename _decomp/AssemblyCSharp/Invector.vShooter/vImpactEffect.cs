using System.Collections.Generic;
using UnityEngine;

namespace Invector.vShooter;

[CreateAssetMenu(menuName = "Invector/Effects/New ImpactEffect", fileName = "ImpactEffect@")]
public class vImpactEffect : vImpactEffectBase
{
	public List<GameObject> decals;

	public List<GameObject> hitEffects;

	protected virtual GameObject GetRandomObject(List<GameObject> referenceList)
	{
		if (referenceList.Count > 1)
		{
			int index = Random.Range(0, referenceList.Count - 1);
			return referenceList[index];
		}
		if (referenceList.Count == 1)
		{
			return referenceList[0];
		}
		return null;
	}

	protected virtual GameObject CreateDecal(Vector3 position, Quaternion rotation)
	{
		return CreateInstance(GetRandomObject(decals), position, rotation);
	}

	protected virtual GameObject CreateHitEffect(Vector3 position, Quaternion rotation)
	{
		return CreateInstance(GetRandomObject(hitEffects), position, rotation);
	}

	protected GameObject CreateInstance(GameObject target, Vector3 position, Quaternion rotation)
	{
		if (target == null)
		{
			return null;
		}
		return Object.Instantiate(target, position, rotation);
	}

	public override void DoImpactEffect(Vector3 position, Quaternion rotation, GameObject sender, GameObject receiver)
	{
		GameObject gameObject = CreateInstance(GetRandomObject(decals), position, rotation);
		gameObject.transform.Rotate(Vector3.forward, Random.Range(0, 360), Space.Self);
		GameObject gameObject2 = CreateInstance(GetRandomObject(hitEffects), position, rotation);
		if ((bool)gameObject && (bool)receiver)
		{
			gameObject.transform.SetParent(receiver.transform, worldPositionStays: true);
		}
		if ((bool)gameObject2)
		{
			gameObject2.transform.SetParent(vObjectContainer.root, worldPositionStays: true);
		}
	}
}
