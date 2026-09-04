using UnityEngine.Events;
using UnityEngine.UI;

namespace Invector.vItemManager;

public class vEquipmentDisplay : vItemSlot
{
	public Text slotIdentifier;

	public InputField.OnChangeEvent onChangeIdentifier;

	public UnityEvent onSelect;

	public UnityEvent onDeselect;

	public bool hasItem;

	public UnityEvent onSetLockToEquip;

	public UnityEvent onUnlockToEquip;

	public void SetLockToEquip(bool value)
	{
		if (value)
		{
			onSetLockToEquip.Invoke();
		}
		else
		{
			onUnlockToEquip.Invoke();
		}
	}

	public void ItemIdentifier(int identifier = 0, bool showIdentifier = false)
	{
		if (!slotIdentifier)
		{
			return;
		}
		if (showIdentifier)
		{
			if ((bool)slotIdentifier)
			{
				slotIdentifier.text = identifier.ToString();
			}
			onChangeIdentifier.Invoke(identifier.ToString());
		}
		else
		{
			if ((bool)slotIdentifier)
			{
				slotIdentifier.text = string.Empty;
			}
			onChangeIdentifier.Invoke(string.Empty);
		}
	}

	public override void AddItem(vItem item)
	{
		base.AddItem(item);
		hasItem = item != null;
	}
}
