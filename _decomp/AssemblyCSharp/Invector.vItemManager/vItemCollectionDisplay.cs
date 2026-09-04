using UnityEngine;

namespace Invector.vItemManager;

[vClassHeader("Item Collected Display", true, "icon_v2", false, "", helpBoxText = "Use this to display the name of collected items", openClose = false)]
public class vItemCollectionDisplay : vMonoBehaviour
{
	private static vItemCollectionDisplay instance;

	public vItemCollectionTextHUD itemCollectedDiplayPrefab;

	public static vItemCollectionDisplay Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Object.FindObjectOfType<vItemCollectionDisplay>();
			}
			return instance;
		}
	}

	public void FadeText(string message, float timeToStay, float timeToFadeOut)
	{
		vItemCollectionTextHUD obj = Object.Instantiate(itemCollectedDiplayPrefab);
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		obj.transform.SetAsFirstSibling();
		obj.Show(message, timeToStay, timeToFadeOut);
	}
}
