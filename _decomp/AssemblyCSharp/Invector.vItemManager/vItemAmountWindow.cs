using UnityEngine;
using UnityEngine.UI;

namespace Invector.vItemManager;

public class vItemAmountWindow : vMasterWindow
{
	public vItemWindowDisplay itemWindowDisplay;

	public GameObject singleAmountControl;

	public GameObject multAmountControl;

	public Text amountDisplay;

	protected override void OnEnable()
	{
		base.OnEnable();
		if ((bool)itemWindowDisplay && (bool)itemWindowDisplay.currentSelectedSlot.item)
		{
			singleAmountControl.SetActive(itemWindowDisplay.currentSelectedSlot.item.amount <= 1);
			multAmountControl.SetActive(itemWindowDisplay.currentSelectedSlot.item.amount > 1);
			if ((bool)amountDisplay)
			{
				amountDisplay.text = 1.ToString("00");
			}
			itemWindowDisplay.amount = 1;
		}
	}

	public virtual void ChangeAmount(int value)
	{
		if ((bool)itemWindowDisplay && (bool)itemWindowDisplay.currentSelectedSlot.item)
		{
			itemWindowDisplay.amount += value;
			itemWindowDisplay.amount = Mathf.Clamp(itemWindowDisplay.amount, 1, itemWindowDisplay.currentSelectedSlot.item.amount);
			if ((bool)amountDisplay)
			{
				amountDisplay.text = itemWindowDisplay.amount.ToString("00");
			}
		}
	}
}
