using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Invector.vItemManager;

public static class vInventoryDisplayFormat
{
	private static readonly List<string> ItemTypeFormats = new List<string>();

	private static readonly List<string> ItemAttributeFormats = new List<string>();

	public static string DisplayFormat(this vItemType value)
	{
		if (ItemTypeFormats.Count == 0)
		{
			vItemType[] array = Enum.GetValues(typeof(vItemType)).OfType<vItemType>().ToArray();
			foreach (vItemType value2 in array)
			{
				ItemTypeFormats.Add(value2.GetDisplayFormat());
			}
		}
		return ItemTypeFormats[(int)value];
	}

	public static string DisplayFormat(this vItemAttributes value)
	{
		if (ItemAttributeFormats.Count == 0)
		{
			vItemAttributes[] array = Enum.GetValues(typeof(vItemAttributes)).OfType<vItemAttributes>().ToArray();
			foreach (vItemAttributes value2 in array)
			{
				ItemAttributeFormats.Add(value2.GetDisplayFormat());
			}
		}
		return ItemAttributeFormats[(int)value];
	}

	private static string GetDisplayFormat<T>(this T value) where T : Enum
	{
		return value.GetType().GetMember(value.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
	}
}
