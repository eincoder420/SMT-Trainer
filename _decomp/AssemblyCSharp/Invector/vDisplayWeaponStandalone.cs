using UnityEngine;
using UnityEngine.UI;

namespace Invector;

public class vDisplayWeaponStandalone : MonoBehaviour
{
	[Header("Weapon Display source")]
	public Image weaponIcon;

	public Text weaponText;

	[Header("Weapon unarmed sources")]
	public Sprite defaultIcon;

	public string defaultText;

	protected virtual void Start()
	{
		RemoveWeaponIcon();
		RemoveWeaponText();
	}

	public virtual void SetWeaponIcon(Sprite icon)
	{
		if ((bool)weaponIcon)
		{
			weaponIcon.sprite = icon;
			if (!weaponIcon.gameObject.activeSelf)
			{
				weaponIcon.gameObject.SetActive(value: true);
			}
		}
	}

	public virtual void SetWeaponText(string text)
	{
		if ((bool)weaponText)
		{
			weaponText.text = text;
			if (!weaponText.gameObject.activeSelf)
			{
				weaponText.gameObject.SetActive(value: true);
			}
		}
	}

	public virtual void RemoveWeaponIcon()
	{
		if ((bool)weaponIcon)
		{
			weaponIcon.sprite = defaultIcon;
			if (weaponIcon.gameObject.activeSelf && weaponIcon.sprite == null)
			{
				weaponIcon.gameObject.SetActive(value: false);
			}
		}
	}

	public virtual void RemoveWeaponText()
	{
		if ((bool)weaponText)
		{
			weaponText.text = defaultText;
		}
	}
}
