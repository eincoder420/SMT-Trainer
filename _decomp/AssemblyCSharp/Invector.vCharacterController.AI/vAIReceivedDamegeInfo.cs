using System;
using UnityEngine;

namespace Invector.vCharacterController.AI;

[Serializable]
public class vAIReceivedDamegeInfo
{
	[vReadOnly(false)]
	public bool isValid;

	[vReadOnly(false)]
	public int lastValue;

	[vReadOnly(false)]
	public string lasType = "unnamed";

	[vReadOnly(false)]
	public Transform lastSender;

	[vReadOnly(false)]
	public int massiveCount;

	[vReadOnly(false)]
	public int massiveValue;

	protected float lastValidDamage;

	private float _massiveTime;

	public vAIReceivedDamegeInfo()
	{
		lasType = "unnamed";
	}

	public void Update()
	{
		_massiveTime -= Time.deltaTime;
		if (_massiveTime <= 0f)
		{
			_massiveTime = 0f;
			if (massiveValue > 0)
			{
				massiveValue--;
			}
			if (massiveCount > 0)
			{
				massiveCount--;
			}
		}
		isValid = lastValidDamage > Time.time;
	}

	public void UpdateDamage(vDamage damage, float validDamageTime = 2f)
	{
		if (damage != null)
		{
			lastValidDamage = Time.time + validDamageTime;
			_massiveTime += Time.deltaTime;
			massiveCount++;
			lastValue = damage.damageValue;
			massiveValue += lastValue;
			lastSender = damage.sender;
			lasType = (string.IsNullOrEmpty(damage.damageType) ? "unnamed" : damage.damageType);
		}
	}
}
