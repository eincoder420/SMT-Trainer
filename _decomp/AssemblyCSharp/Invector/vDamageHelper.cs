using UnityEngine;

namespace Invector;

public static class vDamageHelper
{
	public static void ApplyDamage(this GameObject receiver, vDamage damage)
	{
		vIDamageReceiver[] components = receiver.GetComponents<vIDamageReceiver>();
		if (components != null)
		{
			for (int i = 0; i < components.Length; i++)
			{
				components[i].TakeDamage(damage);
			}
		}
	}

	public static bool CanReceiveDamage(this GameObject receiver)
	{
		return receiver.GetComponent<vIDamageReceiver>() != null;
	}

	public static float HitAngle(this Transform transform, Vector3 hitpoint, bool normalized = true)
	{
		Vector3 vector = transform.InverseTransformPoint(hitpoint);
		int num = (int)(Mathf.Atan2(vector.x, vector.z) * 57.29578f);
		if (!normalized)
		{
			return num;
		}
		if (num <= 45 && num >= -45)
		{
			num = 0;
		}
		else if (num > 45 && num < 135)
		{
			num = 90;
		}
		else if (num >= 135 || num <= -135)
		{
			num = 180;
		}
		else if (num < -45 && num > -135)
		{
			num = -90;
		}
		return num;
	}
}
