using System.Collections;
using Invector.vCharacterController.vActions;
using UnityEngine;

namespace Invector.vItemManager;

[vClassHeader("vAmmoStandalone", true, "icon_v2", false, "")]
public class vAmmoStandalone : vTriggerGenericAction
{
	[Header("Ammo Standalone Options")]
	[Tooltip("Use the same name as in the AmmoManager")]
	public string weaponName;

	public int ammoID;

	public int ammoAmount;

	private vAmmoManager ammoManager;

	public override IEnumerator OnPressActionDelay(GameObject cc)
	{
		yield return StartCoroutine(base.OnPressActionDelay(cc));
		ammoManager = cc.gameObject.GetComponent<vAmmoManager>();
		if (ammoManager != null)
		{
			ammoManager.AddAmmo(weaponName, ammoID, ammoAmount);
		}
	}
}
