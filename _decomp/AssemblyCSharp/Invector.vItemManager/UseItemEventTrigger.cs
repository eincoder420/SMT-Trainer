using System;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vItemManager;

[vClassHeader("Use Item Event Trigger", true, "icon_v2", false, "", useHelpBox = true, helpBoxText = "This script enable ItemUsage when TriggerEnter and disable onTriggerExit", openClose = false)]
public class UseItemEventTrigger : vMonoBehaviour
{
	[Serializable]
	public class OnUseItemEvent
	{
		internal vItem targetItem;

		public int id;

		[vHelpBox("Check this to enable the menu UI Button 'Use' on the Inventory Window", vHelpBoxAttribute.MessageType.None)]
		public bool canUseWithOpenInventory;

		[vHelpBox("Override the Delay to use this Item", vHelpBoxAttribute.MessageType.None)]
		public bool overrideItemUsageDelay;

		[vHideInInspector("overrideItemUsageDelay", false)]
		public float newDeleyTime;

		internal float defaultDelay;

		public UnityEvent onUse;

		public void OnOpenInventory(bool value)
		{
			if (!canUseWithOpenInventory && (bool)targetItem)
			{
				targetItem.canBeUsed = !value;
			}
		}

		public void ChangeItemUsageDelay()
		{
			if (overrideItemUsageDelay && !(targetItem == null))
			{
				defaultDelay = targetItem.enableDelayTime;
				targetItem.enableDelayTime = newDeleyTime;
			}
		}

		public void ResetItemUsageDelay()
		{
			if (overrideItemUsageDelay && !(targetItem == null))
			{
				targetItem.enableDelayTime = defaultDelay;
			}
		}
	}

	public OnUseItemEvent itemEvent;

	protected vItemManager itemManager;

	public virtual void OnTriggerEnter(Collider other)
	{
		if (!other.gameObject.CompareTag("Player"))
		{
			return;
		}
		itemManager = other.GetComponent<vItemManager>();
		if ((bool)itemManager)
		{
			itemEvent.targetItem = itemManager.GetItem(itemEvent.id);
			if ((bool)itemEvent.targetItem)
			{
				itemEvent.ChangeItemUsageDelay();
				itemManager.onUseItem.AddListener(OnUseItem);
				itemManager.onOpenCloseInventory.AddListener(itemEvent.OnOpenInventory);
				itemEvent.targetItem.canBeUsed = true;
			}
		}
	}

	protected virtual void OnUseItem(vItem item)
	{
		if ((bool)itemManager && itemEvent.id == item.id)
		{
			itemManager.inventory.CloseInventory();
			itemManager.onUseItem.RemoveListener(OnUseItem);
			itemManager.onOpenCloseInventory.RemoveListener(itemEvent.OnOpenInventory);
			itemEvent.onUse.Invoke();
			itemEvent.ResetItemUsageDelay();
			itemEvent.targetItem = null;
		}
	}

	public virtual void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Player") && (bool)itemManager)
		{
			itemManager.onUseItem.RemoveListener(OnUseItem);
			itemManager.onOpenCloseInventory.RemoveListener(itemEvent.OnOpenInventory);
			if ((bool)itemEvent.targetItem)
			{
				itemEvent.targetItem.canBeUsed = false;
				itemEvent.ResetItemUsageDelay();
				itemEvent.targetItem = null;
			}
		}
	}
}
