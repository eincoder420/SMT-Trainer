using System;
using UnityEngine;

namespace Invector;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class vReadOnlyAttribute : PropertyAttribute
{
	public readonly bool justInPlayMode;

	public vReadOnlyAttribute(bool justInPlayMode = true)
	{
		this.justInPlayMode = justInPlayMode;
	}
}
