using UnityEngine;

namespace Invector;

public class vDamageEffectInfo
{
	public Transform receiver;

	public Vector3 position;

	public Quaternion rotation;

	public string damageType;

	public vDamageEffectInfo(Vector3 position, Quaternion rotation, string damageType = "", Transform receiver = null)
	{
		this.receiver = receiver;
		this.position = position;
		this.rotation = rotation;
		this.damageType = damageType;
	}
}
