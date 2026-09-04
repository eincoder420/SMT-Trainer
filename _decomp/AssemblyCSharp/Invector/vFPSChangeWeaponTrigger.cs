using UnityEngine;
using UnityEngine.Events;

namespace Invector;

public class vFPSChangeWeaponTrigger : MonoBehaviour
{
	public bool pressButtonToChange;

	public string targetWeaponName;

	public UnityEvent onChangeWeapon;

	private void OnTriggerStay(Collider other)
	{
		if (!pressButtonToChange)
		{
			vFPSWeaponManager componentInParent = other.GetComponentInParent<vFPSWeaponManager>();
			if ((bool)componentInParent)
			{
				componentInParent.EquipWeapon(targetWeaponName);
				onChangeWeapon.Invoke();
			}
		}
	}
}
