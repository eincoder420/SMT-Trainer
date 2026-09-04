using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Invector.vItemManager;

public class vAmmoDisplay : MonoBehaviour
{
	[Serializable]
	public class OnChangeAmmoEvent : UnityEvent<int>
	{
	}

	public int displayID = 1;

	[SerializeField]
	[vHelpBox("Ammo loaded in the Clip", vHelpBoxAttribute.MessageType.None)]
	protected Text display;

	[SerializeField]
	[vHelpBox("Ammo left in the Inventory", vHelpBoxAttribute.MessageType.None)]
	protected Text secundaryDisplay;

	public UnityEvent onShow;

	public UnityEvent onHide;

	[vHelpBox("Event based in the current AmmoID", vHelpBoxAttribute.MessageType.None)]
	public OnChangeAmmoEvent onChangeAmmo;

	private int currentAmmoId;

	private void Start()
	{
		if (display == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		display.text = "";
		if ((bool)secundaryDisplay)
		{
			secundaryDisplay.text = "";
		}
		currentAmmoId = -1;
	}

	public void Show()
	{
		if ((bool)display)
		{
			display.gameObject.SetActive(value: true);
		}
		if ((bool)secundaryDisplay)
		{
			secundaryDisplay.gameObject.SetActive(value: true);
		}
		onShow.Invoke();
	}

	public void Hide()
	{
		if ((bool)display)
		{
			display.gameObject.SetActive(value: false);
		}
		if ((bool)secundaryDisplay)
		{
			secundaryDisplay.gameObject.SetActive(value: true);
		}
		onHide.Invoke();
	}

	public void UpdateDisplay(string text1, string text2 = "", int id = 0)
	{
		if ((bool)display && !text1.Equals("") && !display.gameObject.activeSelf)
		{
			display.gameObject.SetActive(value: true);
		}
		if ((bool)secundaryDisplay && !text2.Equals("") && !secundaryDisplay.gameObject.activeSelf)
		{
			secundaryDisplay.gameObject.SetActive(value: true);
		}
		if (currentAmmoId != id)
		{
			onChangeAmmo.Invoke(id);
			currentAmmoId = id;
		}
		if ((bool)display)
		{
			display.text = text1;
		}
		if ((bool)secundaryDisplay)
		{
			secundaryDisplay.text = text2;
		}
	}
}
