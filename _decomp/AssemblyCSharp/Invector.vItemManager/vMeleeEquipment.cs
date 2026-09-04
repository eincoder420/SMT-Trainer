using Invector.vMelee;

namespace Invector.vItemManager;

[vClassHeader("Melee Equipment", true, "icon_v2", false, "", openClose = false, useHelpBox = true, helpBoxText = "Use this component if you also use the ItemManager in your Character")]
public class vMeleeEquipment : vEquipment
{
	private vMeleeWeapon _weapon;

	protected bool withoutMeleeWeapon;

	protected virtual vMeleeWeapon meleeWeapon
	{
		get
		{
			if (!_weapon && !withoutMeleeWeapon)
			{
				_weapon = GetComponent<vMeleeWeapon>();
				if (!_weapon)
				{
					withoutMeleeWeapon = true;
				}
			}
			return _weapon;
		}
	}

	public override void OnEquip(vItem item)
	{
		if ((bool)meleeWeapon)
		{
			vItemAttribute itemAttribute = item.GetItemAttribute(vItemAttributes.Damage);
			vItemAttribute itemAttribute2 = item.GetItemAttribute(vItemAttributes.StaminaCost);
			vItemAttribute itemAttribute3 = item.GetItemAttribute(vItemAttributes.DefenseRate);
			vItemAttribute itemAttribute4 = item.GetItemAttribute(vItemAttributes.DefenseRange);
			if (itemAttribute != null)
			{
				meleeWeapon.damage.damageValue = itemAttribute.value;
			}
			if (itemAttribute2 != null)
			{
				meleeWeapon.staminaCost = itemAttribute2.value;
			}
			if (itemAttribute3 != null)
			{
				meleeWeapon.defenseRate = itemAttribute3.value;
			}
			if (itemAttribute4 != null)
			{
				meleeWeapon.defenseRange = itemAttribute3.value;
			}
		}
		base.OnEquip(item);
	}

	public override void OnUnequip(vItem item)
	{
		base.OnUnequip(item);
	}
}
