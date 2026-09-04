using System.ComponentModel;

namespace Invector.vItemManager;

public enum vItemAttributes
{
	[Description("")]
	Health,
	[Description("")]
	Stamina,
	[Description("<i>Damage</i> : <color=red>(VALUE)</color>")]
	Damage,
	[Description("")]
	StaminaCost,
	[Description("")]
	DefenseRate,
	[Description("")]
	DefenseRange,
	[Description("(VALUE)")]
	AmmoCount,
	[Description("")]
	MaxHealth,
	[Description("")]
	MaxStamina,
	[Description("(VALUE)")]
	SecundaryAmmoCount,
	[Description("")]
	SecundaryDamage
}
