using UnityEngine;

namespace Invector.vItemManager;

public interface IWeaponEquipmentListener
{
	void SetLeftWeapon(GameObject equipment);

	void SetRightWeapon(GameObject equipment);
}
