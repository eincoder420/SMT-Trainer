using System.ComponentModel;

namespace Invector.vItemManager;

public enum vItemType
{
	[Description("")]
	Consumable,
	[Description("Melee")]
	MeleeWeapon,
	[Description("Shooter")]
	ShooterWeapon,
	[Description("(VALUE)")]
	Ammo,
	[Description("")]
	Archery,
	[Description("")]
	Builder,
	[Description("")]
	Defense,
	[Description("")]
	CraftingMaterials
}
