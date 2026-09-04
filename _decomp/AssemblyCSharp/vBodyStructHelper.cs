using System;

public static class vBodyStructHelper
{
	public static bool ToEnum<T>(this string value, ref T enumTarget)
	{
		object obj = Enum.Parse(typeof(T), value);
		if (obj != null)
		{
			enumTarget = (T)obj;
		}
		return obj != null;
	}
}
