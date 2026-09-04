using System.Collections.Generic;
using Invector.vCharacterController;

namespace Invector.vItemManager;

[vClassHeader("Check if can Add Health", "Simple Example to verify if the health item can be used based on the character's health is full or not.", openClose = false)]
public class vCheckCanAddHealth : vMonoBehaviour
{
	public vItemManager itemManager;

	public vThirdPersonController tpController;

	public bool getInParent = true;

	internal bool canUse;

	internal bool firstRun;

	private void Start()
	{
		if (itemManager == null)
		{
			if (getInParent)
			{
				itemManager = GetComponentInParent<vItemManager>();
			}
			else
			{
				itemManager = GetComponent<vItemManager>();
			}
		}
		if (tpController == null)
		{
			if (getInParent)
			{
				tpController = GetComponentInParent<vThirdPersonController>();
			}
			else
			{
				tpController = GetComponent<vThirdPersonController>();
			}
		}
		if ((bool)itemManager)
		{
			itemManager.canUseItemDelegate -= CanUseItem;
			itemManager.canUseItemDelegate += CanUseItem;
		}
	}

	private void OnDestroy()
	{
		vItemManager component = GetComponent<vItemManager>();
		if ((bool)component)
		{
			component.canUseItemDelegate -= CanUseItem;
		}
	}

	private void CanUseItem(vItem item, ref List<bool> validateResult)
	{
		if (item.GetItemAttribute(vItemAttributes.Health) != null)
		{
			bool flag = tpController.currentHealth < (float)tpController.maxHealth;
			if (flag != canUse || !firstRun)
			{
				canUse = flag;
				firstRun = true;
				vHUDController.instance.ShowText(flag ? "Increase health" : ("Can't use " + item.name + " because your health is full"), 4f);
			}
			if (!flag)
			{
				validateResult.Add(flag);
			}
		}
	}
}
