using System;

namespace Invector.vItemManager;

[Serializable]
public class vItemAttribute
{
	public vItemAttributes name;

	public int value;

	public bool isOpen;

	public bool isBool;

	public string displayFormat => name.DisplayFormat();

	public string GetDisplayText(string format = null)
	{
		string text = (string.IsNullOrEmpty(format) ? displayFormat : format);
		if (string.IsNullOrEmpty(text))
		{
			text = name.ToString().InsertSpaceBeforeUpperCase().RemoveUnderline();
			text = text + " : " + value;
		}
		else
		{
			if (text.Contains("(NAME)"))
			{
				text = text.Replace("(NAME)", name.ToString().InsertSpaceBeforeUpperCase().RemoveUnderline());
			}
			if (text.Contains("(VALUE)"))
			{
				text = text.Replace("(VALUE)", value.ToString());
			}
		}
		return text;
	}

	public vItemAttribute(vItemAttributes name, int value)
	{
		this.name = name;
		this.value = value;
	}
}
