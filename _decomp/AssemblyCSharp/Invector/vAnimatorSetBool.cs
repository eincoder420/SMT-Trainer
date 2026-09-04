using UnityEngine;

namespace Invector;

public class vAnimatorSetBool : vAnimatorSetValue<bool>
{
	[vHelpBox("Random Value between True and False", vHelpBoxAttribute.MessageType.None)]
	public bool randomEnter;

	public bool randomExit;

	protected override bool GetEnterValue()
	{
		if (!randomEnter)
		{
			return base.GetEnterValue();
		}
		return Random.Range(0, 100) > 50;
	}

	protected override bool GetExitValue()
	{
		if (!randomExit)
		{
			return base.GetExitValue();
		}
		return Random.Range(0, 100) > 50;
	}
}
