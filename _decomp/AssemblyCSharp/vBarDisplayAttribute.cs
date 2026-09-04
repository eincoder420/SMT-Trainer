using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class vBarDisplayAttribute : PropertyAttribute
{
	public readonly string maxValueProperty;

	public readonly bool showJuntInPlayMode;

	public vBarDisplayAttribute(string maxValueProperty, bool showJuntInPlayMode = false)
	{
		this.maxValueProperty = maxValueProperty;
		this.showJuntInPlayMode = showJuntInPlayMode;
	}
}
