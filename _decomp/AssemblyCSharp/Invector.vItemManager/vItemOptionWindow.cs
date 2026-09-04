using UnityEngine.UI;

namespace Invector.vItemManager;

[vClassHeader("Item Options Window", true, "icon_v2", false, "")]
public class vItemOptionWindow : vMonoBehaviour
{
	public Button useItemButton;

	public Button equipItemButton;

	public Button dropItemButton;

	public Button destroyItemButton;

	public virtual void EnableOptions(vItemSlot slot)
	{
	}

	protected virtual void ValidateButtons(vItem item, out bool result)
	{
		if ((bool)item.originalObject && item.originalObject.GetComponent<vEquipment>() != null)
		{
			if ((bool)equipItemButton)
			{
				equipItemButton.gameObject.SetActive(value: true);
			}
			if ((bool)useItemButton)
			{
				useItemButton.gameObject.SetActive(value: false);
			}
		}
		else
		{
			if ((bool)equipItemButton)
			{
				equipItemButton.gameObject.SetActive(value: false);
			}
			if ((bool)useItemButton)
			{
				useItemButton.gameObject.SetActive(value: true);
			}
		}
		if ((bool)useItemButton)
		{
			useItemButton.interactable = item.canBeUsed;
		}
		if ((bool)dropItemButton)
		{
			dropItemButton.interactable = item.canBeDroped;
		}
		if ((bool)destroyItemButton)
		{
			destroyItemButton.interactable = item.canBeDestroyed;
		}
		result = ((bool)equipItemButton && equipItemButton.gameObject.activeSelf) || ((bool)useItemButton && useItemButton.interactable) || ((bool)dropItemButton && dropItemButton.interactable) || ((bool)destroyItemButton && destroyItemButton.interactable);
	}

	public virtual bool CanOpenOptions(vItem item)
	{
		if (item == null)
		{
			return false;
		}
		bool result = false;
		ValidateButtons(item, out result);
		return result;
	}
}
