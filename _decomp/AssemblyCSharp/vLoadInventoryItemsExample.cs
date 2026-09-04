using System.Collections;
using Invector;
using Invector.vItemManager;
using UnityEngine;

public class vLoadInventoryItemsExample : MonoBehaviour
{
	private vGameController gm;

	private void Start()
	{
		gm = GetComponent<vGameController>();
	}

	public void LoadItemsToInventory()
	{
		if ((bool)gm)
		{
			StartCoroutine(LoadItems());
		}
	}

	private IEnumerator LoadItems()
	{
		yield return new WaitForSeconds(0.1f);
		gm.currentPlayer.GetComponent<vItemManager>().LoadItemsExample();
	}
}
