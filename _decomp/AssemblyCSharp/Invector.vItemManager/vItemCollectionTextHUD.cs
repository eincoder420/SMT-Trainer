using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Invector.vItemManager;

[vClassHeader("Item Collection HUD", true, "icon_v2", false, "", helpBoxText = "Contains all behaviour to show messages sended")]
public class vItemCollectionTextHUD : MonoBehaviour
{
	public Text Message;

	public void Show(string message, float timeToStay = 1f, float timeToFadeOut = 1f)
	{
		Message.text = message;
		StartCoroutine(Timer(timeToStay, timeToFadeOut));
	}

	private IEnumerator Timer(float timeToStay = 1f, float timeToFadeOut = 1f)
	{
		Message.CrossFadeAlpha(1f, 0.5f, ignoreTimeScale: false);
		yield return new WaitForSeconds(timeToStay);
		Message.CrossFadeAlpha(0f, timeToFadeOut, ignoreTimeScale: false);
		yield return new WaitForSeconds(timeToFadeOut + 0.1f);
		Object.Destroy(base.gameObject);
	}

	private void Awake()
	{
		Clear();
	}

	public void Clear()
	{
		Message.text = "";
		Message.CrossFadeAlpha(0f, 0f, ignoreTimeScale: false);
	}
}
