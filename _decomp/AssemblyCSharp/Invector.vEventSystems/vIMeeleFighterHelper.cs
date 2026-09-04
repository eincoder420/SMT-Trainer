using UnityEngine;

namespace Invector.vEventSystems;

public static class vIMeeleFighterHelper
{
	public static bool IsMeleeFighter(this GameObject receiver)
	{
		return receiver.GetComponent<vIMeleeFighter>() != null;
	}

	public static void ApplyDamage(this GameObject receiver, vDamage damage, vIMeleeFighter attacker)
	{
		vIAttackReceiver component = receiver.GetComponent<vIAttackReceiver>();
		if (component != null)
		{
			component.OnReceiveAttack(damage, attacker);
		}
		else
		{
			receiver.ApplyDamage(damage);
		}
	}

	public static vIMeleeFighter GetMeleeFighter(this GameObject receiver)
	{
		return receiver.GetComponent<vIMeleeFighter>();
	}
}
