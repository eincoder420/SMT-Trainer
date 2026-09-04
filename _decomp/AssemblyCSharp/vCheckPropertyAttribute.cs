using System;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class vCheckPropertyAttribute : PropertyAttribute
{
	[Serializable]
	public struct CheckValue
	{
		public string property;

		public object value;

		public bool isValid => value != null;

		public CheckValue(string property, object value)
		{
			this.property = property;
			this.value = value;
		}
	}

	public List<CheckValue> checkValues = new List<CheckValue>();

	public bool hideInInspector;

	public vCheckPropertyAttribute(string propertyNames, params object[] values)
	{
		checkValues.Clear();
		string[] array = propertyNames.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			try
			{
				checkValues.Add(new CheckValue(array[i], values[i]));
			}
			catch
			{
				break;
			}
		}
	}
}
