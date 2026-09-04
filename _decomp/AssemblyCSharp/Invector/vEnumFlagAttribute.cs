using System;
using UnityEngine;

namespace Invector;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class vEnumFlagAttribute : PropertyAttribute
{
	public string enumName;

	public vEnumFlagAttribute()
	{
	}

	public vEnumFlagAttribute(string name)
	{
		enumName = name;
	}
}
