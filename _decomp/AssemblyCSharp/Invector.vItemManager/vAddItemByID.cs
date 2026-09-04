using UnityEngine;

namespace Invector.vItemManager;

[vClassHeader("Add Item By ID", "This is a simple example on how to add items using script", openClose = false)]
public class vAddItemByID : vMonoBehaviour
{
	public int id;

	public int amount;

	public bool addToEquipArea = true;

	[vHideInInspector("addToEquipArea", false)]
	public bool autoEquip;

	public bool destroyAfter;

	[vHideInInspector("addToEquipArea", false)]
	public int indexOfEquipArea;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.gameObject.CompareTag("Player"))
		{
			return;
		}
		vItemManager component = other.gameObject.GetComponent<vItemManager>();
		if ((bool)component)
		{
			ItemReference itemReference = new ItemReference(id);
			itemReference.amount = amount;
			itemReference.addToEquipArea = addToEquipArea;
			itemReference.autoEquip = autoEquip;
			itemReference.indexArea = indexOfEquipArea;
			component.CollectItem(itemReference, 0f, 2f, ignoreItemAnimation: false);
			if (destroyAfter)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
