using System;
using UnityEngine;

namespace Invector;

public class vAnimatorSetFloat : vAnimatorSetValue<float>
{
	[vHelpBox("Random Value between Default Value and Max Value", vHelpBoxAttribute.MessageType.None)]
	public bool randomEnter;

	[vHideInInspector("randomEnter", false)]
	public float maxEnterValue;

	public bool randomExit;

	[vHideInInspector("randomExit", false)]
	public float maxExitValue;

	[vHelpBox("Use this in <b>Random mode</b> to generat a rounded value", vHelpBoxAttribute.MessageType.None)]
	public bool roundValue;

	[Tooltip("Digits after the comma")]
	[vHideInInspector("roundValue", false)]
	public int roundDigits = 1;

	[Tooltip("Invert number randomly")]
	public bool randomInvert;

	protected override float GetEnterValue()
	{
		float num = 0f;
		if (randomEnter)
		{
			num = UnityEngine.Random.Range(base.GetEnterValue(), maxEnterValue);
			if (roundValue)
			{
				num = (float)Math.Round(num, roundDigits);
			}
		}
		else
		{
			num = base.GetEnterValue();
		}
		if (randomInvert && UnityEngine.Random.Range(0, 100) > 50)
		{
			num *= -1f;
		}
		return num;
	}

	protected override float GetExitValue()
	{
		float num = 0f;
		if (randomEnter)
		{
			num = UnityEngine.Random.Range(base.GetExitValue(), maxEnterValue);
			if (roundValue)
			{
				num = (float)Math.Round(num, roundDigits);
			}
		}
		else
		{
			num = base.GetExitValue();
		}
		if (randomInvert && UnityEngine.Random.Range(0, 100) > 50)
		{
			num *= -1f;
		}
		return num;
	}
}
