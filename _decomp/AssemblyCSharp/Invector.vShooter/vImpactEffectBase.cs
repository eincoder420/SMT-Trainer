using UnityEngine;

namespace Invector.vShooter;

public abstract class vImpactEffectBase : ScriptableObject
{
	public abstract void DoImpactEffect(Vector3 position, Quaternion rotation, GameObject sender, GameObject receiver);
}
