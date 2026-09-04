using System;
using System.Collections.Generic;
using System.Linq;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public static class vFSMHelper
{
	public const float dragSnap = 5f;

	public static IEnumerable<Type> FindSubClasses(this Type type)
	{
		return from t in type.Assembly.GetTypes()
			where t.IsSubclassOf(type) && !t.IsAbstract
			select t;
	}

	public static float NearestRound(float x, float multiple)
	{
		if (multiple < 1f)
		{
			float num = (float)Math.Floor(x);
			while ((num += multiple) < x)
			{
			}
			float num2 = num - multiple;
			if (!(Math.Abs(x - num2) < Math.Abs(x - num)))
			{
				return num;
			}
			return num2;
		}
		return (float)Math.Round(x / multiple, MidpointRounding.AwayFromZero) * multiple;
	}
}
