using Invector.vShooter;

namespace Invector.vItemManager;

[vClassHeader("Shooter Equipment", true, "icon_v2", false, "", openClose = false, useHelpBox = true, helpBoxText = "Use this component if you also use the ItemManager in your Character")]
public class vShooterEquipment : vMeleeEquipment
{
	protected vShooterWeapon _shooter;

	protected vEquipment _secundaryEquipment;

	protected bool withoutShooterWeapon;

	public virtual vEquipment secundaryEquipment => _secundaryEquipment;

	public virtual vShooterWeapon shooterWeapon
	{
		get
		{
			if (!_shooter && !withoutShooterWeapon)
			{
				_shooter = GetComponent<vShooterWeapon>();
				if (!_shooter)
				{
					withoutShooterWeapon = true;
				}
			}
			return _shooter;
		}
	}

	public override void OnEquip(vItem item)
	{
		if ((bool)shooterWeapon)
		{
			shooterWeapon.changeAmmoHandle = ChangeAmmo;
			shooterWeapon.checkAmmoHandle = CheckAmmo;
			vItemAttribute itemAttribute = item.GetItemAttribute(vItemAttributes.Damage);
			if (itemAttribute != null)
			{
				shooterWeapon.maxDamage = itemAttribute.value;
			}
			if ((bool)secundaryEquipment)
			{
				secundaryEquipment.OnEquip(item);
			}
		}
		base.OnEquip(item);
	}

	public override void OnUnequip(vItem item)
	{
		if ((bool)shooterWeapon)
		{
			shooterWeapon.changeAmmoHandle = null;
			shooterWeapon.checkAmmoHandle = null;
			if ((bool)secundaryEquipment)
			{
				secundaryEquipment.OnUnequip(item);
			}
		}
		base.OnUnequip(item);
	}

	protected virtual bool CheckAmmo(ref bool isValid, ref int totalAmmo)
	{
		if (!referenceItem)
		{
			return false;
		}
		vItemAttribute itemAttribute = referenceItem.GetItemAttribute(vItemAttributes.AmmoCount);
		isValid = itemAttribute != null && !itemAttribute.isBool;
		if (isValid)
		{
			totalAmmo = itemAttribute.value;
		}
		if (isValid)
		{
			return itemAttribute.value > 0;
		}
		return false;
	}

	protected virtual void ChangeAmmo(int value)
	{
		if ((bool)referenceItem)
		{
			vItemAttribute itemAttribute = referenceItem.GetItemAttribute(vItemAttributes.AmmoCount);
			if (itemAttribute != null)
			{
				itemAttribute.value += value;
			}
		}
	}
}
