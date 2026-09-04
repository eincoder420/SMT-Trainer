using System;
using System.Collections.Generic;

namespace Invector.vItemManager;

[Serializable]
public class ItemReference
{
	public int id;

	public string name;

	public int amount;

	public List<vItemAttribute> attributes;

	public bool changeAttributes;

	public bool autoEquip;

	public bool addToEquipArea = true;

	public int indexArea;

	public ItemReference(int id)
	{
		this.id = id;
		addToEquipArea = true;
		autoEquip = false;
	}

	public ItemReference Clone()
	{
		return new ItemReference(id)
		{
			name = name,
			amount = amount,
			autoEquip = autoEquip,
			addToEquipArea = addToEquipArea,
			indexArea = indexArea,
			changeAttributes = changeAttributes,
			attributes = attributes
		};
	}
}
