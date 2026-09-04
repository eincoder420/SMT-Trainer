using System.Collections.Generic;
using Invector.vCharacterController.vActions;
using UnityEngine;

namespace Invector.vItemManager;

public class vItemCollection : vTriggerGenericAction
{
	[vEditorToolbar("Item Collection", false, "", false, false)]
	[Header("--- Item Collection Options ---")]
	[Tooltip("List of items you want to use")]
	public vItemListData itemListData;

	[Tooltip("Delay to actually collect the items, you can use to match with animations of getting the item")]
	public float onCollectDelay;

	[Tooltip("Drag and drop the prefab ItemCollectionDisplay inside the UI gameObject to show a text of the items you've collected")]
	public float textDelay = 0.25f;

	[Tooltip("Ignore the Enable/Disable animation of your item, you can assign an animation to your item in the ItemListData")]
	public bool ignoreItemAnimation;

	[HideInInspector]
	public List<vItemType> itemsFilter = new List<vItemType> { vItemType.Consumable };

	[HideInInspector]
	public List<ItemReference> items = new List<ItemReference>();

	protected override void Start()
	{
		base.Start();
	}
}
