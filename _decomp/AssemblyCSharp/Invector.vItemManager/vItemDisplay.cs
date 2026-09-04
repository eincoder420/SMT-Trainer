using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Invector.vItemManager;

[vClassHeader("vItemDisplay", true, "icon_v2", false, "")]
public class vItemDisplay : vMonoBehaviour
{
	[Serializable]
	public class ColorByType
	{
		public List<vItemType> types;

		public Color color = Color.white;

		public ColorByType()
		{
			_ = Color.white;
		}
	}

	[vEditorToolbar("Settings", false, "", false, false)]
	public Image icon;

	public Text itemName;

	public Text type;

	public Text amount;

	[vEditorToolbar("ColorByType", false, "", false, false)]
	public bool useColorByType;

	public MaskableGraphic bgColor;

	public ColorByType[] colorByTypes;

	[vHelpBox("Use {0} inside string format to inset the value", vHelpBoxAttribute.MessageType.None)]
	public string nameFormat = "Name: {0}";

	public string typeFormat = "Type: {0}";

	public string amountFormat = "Amount: {0}";

	public bool displayAmountOnlyGreaterOne = true;

	public void DisplayItem(vItemManager.CollectedItemInfo info)
	{
		if (useColorByType)
		{
			ColorByType colorByType = Array.Find(colorByTypes, (ColorByType c) => c.types.Contains(info.item.type));
			if (colorByType != null)
			{
				bgColor.color = colorByType.color;
			}
		}
		icon.sprite = info.item.icon;
		itemName.text = FormatText(nameFormat, info.item.name);
		type.text = FormatText(typeFormat, info.item.type.ToString());
		if (info.amount > 1 || !displayAmountOnlyGreaterOne)
		{
			amount.text = FormatText(amountFormat, info.amount.ToString());
		}
		else
		{
			amount.text = "";
		}
	}

	public string FormatText(string format, string value)
	{
		if (string.IsNullOrEmpty(format))
		{
			return value;
		}
		return string.Format(format, value);
	}
}
