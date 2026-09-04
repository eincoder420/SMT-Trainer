using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Invector.vItemManager;

[Serializable]
public class vItem : ScriptableObject
{
	[HideInInspector]
	public int id;

	[HideInInspector]
	public string description = "Item Description";

	[HideInInspector]
	public vItemType type;

	[HideInInspector]
	public Sprite icon;

	[HideInInspector]
	public bool stackable = true;

	[HideInInspector]
	public int maxStack;

	[HideInInspector]
	public int amount;

	[HideInInspector]
	public GameObject originalObject;

	[HideInInspector]
	public GameObject dropObject;

	[HideInInspector]
	public List<vItemAttribute> attributes = new List<vItemAttribute>();

	[HideInInspector]
	public bool isInEquipArea;

	[HideInInspector]
	public bool isEquiped;

	public bool destroyAfterUse = true;

	public bool canBeUsed = true;

	public bool canBeDroped = true;

	public bool canBeDestroyed = true;

	[Header("Animation Settings")]
	[vHelpBox("Triggers a animation when Equipping a Weapon or enabling item.\nYou can also trigger an animation if the ItemType is a Consumable", vHelpBoxAttribute.MessageType.None)]
	public string EnableAnim = "LowBack";

	[vHelpBox("Triggers a animation when Unequipping a Weapon or disable item", vHelpBoxAttribute.MessageType.None)]
	public string DisableAnim = "LowBack";

	[vHelpBox("Delay to enable the Weapon/Item object when Equipping\n If ItemType is a Consumable use this to delay the item usage.", vHelpBoxAttribute.MessageType.None)]
	public float enableDelayTime = 0.5f;

	[vHelpBox("Delay to hide the Weapon/Item object when Unequipping", vHelpBoxAttribute.MessageType.None)]
	public float disableDelayTime = 0.5f;

	[vHelpBox("If the item is equippable use this to set a custom handler to instantiate the SpawnObject", vHelpBoxAttribute.MessageType.None)]
	public string customHandler;

	[vHelpBox("If the item is equippable and need to use two hand\n<color=yellow><b>This option makes it impossible to equip two items</b></color>", vHelpBoxAttribute.MessageType.None)]
	public bool twoHandWeapon;

	[HideInInspector]
	public OnHandleItemEvent onDestroy;

	public Texture2D iconTexture
	{
		get
		{
			if (!icon)
			{
				return null;
			}
			try
			{
				if (icon.rect.width != (float)icon.texture.width || icon.rect.height != (float)icon.texture.height)
				{
					Texture2D texture2D = new Texture2D((int)icon.textureRect.width, (int)icon.textureRect.height);
					texture2D.name = icon.name;
					Color[] pixels = icon.texture.GetPixels((int)icon.textureRect.x, (int)icon.textureRect.y, (int)icon.textureRect.width, (int)icon.textureRect.height);
					texture2D.SetPixels(pixels);
					texture2D.Apply();
					return texture2D;
				}
				return icon.texture;
			}
			catch
			{
				Debug.LogWarning("Icon texture of the " + base.name + " is not Readable", icon.texture);
				return icon.texture;
			}
		}
	}

	public void OnDestroy()
	{
		onDestroy.Invoke(this);
	}

	public vItemAttribute GetItemAttribute(vItemAttributes attribute)
	{
		if (attributes != null)
		{
			return attributes.Find((vItemAttribute _attribute) => _attribute.name == attribute);
		}
		return null;
	}

	public vItemAttribute GetItemAttribute(string name)
	{
		if (attributes != null)
		{
			return attributes.Find((vItemAttribute attribute) => attribute.name.ToString().Equals(name));
		}
		return null;
	}

	public string GetItemAttributesText(List<vItemAttributes> ignore = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < attributes.Count; i++)
		{
			if (ignore == null || !ignore.Contains(attributes[i].name))
			{
				stringBuilder.AppendLine(GetItemAttributeText(i));
			}
		}
		return stringBuilder.ToString();
	}

	protected string GetItemAttributeText(int i)
	{
		if (attributes.Count > 0 && i < attributes.Count && attributes.Count > 0 && i < attributes.Count)
		{
			return attributes[i].GetDisplayText();
		}
		return string.Empty;
	}

	protected string GetItemAttributeText(int i, string customFormat)
	{
		if (attributes.Count > 0 && i < attributes.Count)
		{
			return attributes[i].GetDisplayText(customFormat);
		}
		return string.Empty;
	}

	public string ItemTypeText()
	{
		return ItemTypeText(type.DisplayFormat());
	}

	public string ItemTypeText(string format)
	{
		string text = type.ToString().InsertSpaceBeforeUpperCase().RemoveUnderline();
		if (string.IsNullOrEmpty(format))
		{
			return text;
		}
		if (format.Contains("(NAME)"))
		{
			format.Replace("(NAME)", text);
		}
		return format;
	}

	public string GetFullItemDescription(string format = null, List<vItemAttributes> ignoreAttributes = null)
	{
		string text = "";
		if (string.IsNullOrEmpty(format))
		{
			text += base.name;
			text = text + "\n" + ItemTypeText();
			text = text + "\n" + description;
			text = text + "\n" + GetItemAttributesText();
		}
		else
		{
			text = format;
			if (text.Contains("(NAME)"))
			{
				text = text.Replace("(NAME)", base.name);
			}
			if (text.Contains("(TYPE)"))
			{
				text = text.Replace("(TYPE)", ItemTypeText());
			}
			if (text.Contains("(DESC)"))
			{
				text = text.Replace("(DESC)", description);
			}
			if (text.Contains("(ATTR)"))
			{
				text = text.Replace("(ATTR)", GetItemAttributesText(ignoreAttributes));
			}
		}
		return text;
	}
}
