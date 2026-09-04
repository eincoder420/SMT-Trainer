using UnityEngine;

namespace Invector.vItemManager;

public class vItemCollectionDisplay_v2 : MonoBehaviour
{
	public vItemManager itemManager;

	public vItemDisplay displayPrefab;

	public RectTransform content;

	public float displayTime = 3f;

	private void Start()
	{
		itemManager.onCollectItem.AddListener(OnAddItem);
	}

	private void OnAddItem(vItemManager.CollectedItemInfo info)
	{
		vItemDisplay obj = Object.Instantiate(displayPrefab);
		obj.transform.SetParent(content, worldPositionStays: false);
		obj.DisplayItem(info);
		obj.transform.SetAsFirstSibling();
		Object.Destroy(obj.gameObject, displayTime);
	}
}
