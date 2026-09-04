using UnityEngine;

namespace Invector;

public class vControlDisplayWeaponStandalone : MonoBehaviour
{
	[SerializeField]
	protected vDisplayWeaponStandalone leftDisplay;

	[SerializeField]
	protected vDisplayWeaponStandalone rightDisplay;

	public virtual void SetLeftWeaponIcon(Sprite icon)
	{
		if ((bool)leftDisplay)
		{
			leftDisplay.SetWeaponIcon(icon);
		}
	}

	public virtual void SetLeftWeaponText(string text)
	{
		if ((bool)leftDisplay)
		{
			leftDisplay.SetWeaponText(text);
		}
	}

	public virtual void RemoveLeftWeaponIcon()
	{
		if ((bool)leftDisplay)
		{
			leftDisplay.RemoveWeaponIcon();
		}
	}

	public virtual void RemoveLeftWeaponText()
	{
		if ((bool)leftDisplay)
		{
			leftDisplay.RemoveWeaponText();
		}
	}

	public virtual void SetRightWeaponIcon(Sprite icon)
	{
		if ((bool)rightDisplay)
		{
			rightDisplay.SetWeaponIcon(icon);
		}
	}

	public virtual void SetRightWeaponText(string text)
	{
		if ((bool)rightDisplay)
		{
			rightDisplay.SetWeaponText(text);
		}
	}

	public virtual void RemoveRightWeaponIcon()
	{
		if ((bool)rightDisplay)
		{
			rightDisplay.RemoveWeaponIcon();
		}
	}

	public virtual void RemoveRightWeaponText()
	{
		if ((bool)rightDisplay)
		{
			rightDisplay.RemoveWeaponText();
		}
	}
}
