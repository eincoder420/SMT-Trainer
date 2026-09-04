using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Invector;

[Serializable]
public class vDamageModifier
{
	public enum FilterMethod
	{
		ApplyToAll,
		ApplyToAllInList,
		ApplyToAllOutList
	}

	[Serializable]
	public class DamageModifierEvent : UnityEvent<vDamageModifier>
	{
	}

	public string name = "MyModifier";

	public FilterMethod filterMethod;

	[Tooltip("List of Damage type that this can modify, keep empty if the filter will be applied to all types of damage")]
	public List<string> damageTypes = new List<string>();

	[Tooltip("Modifier value")]
	public int value;

	[Tooltip("true: Reduce a percentage of damage value\nfalse: Reduce da damage value directly")]
	public bool percentage;

	[Tooltip("The Filter will receive all damage and decrease your self resistance")]
	public bool destructible = true;

	public int resistance = 100;

	public int maxResistance = 100;

	public Slider.SliderEvent onChangeResistance;

	public DamageModifierEvent onBroken;

	public bool isBroken
	{
		get
		{
			if (destructible)
			{
				return resistance <= 0;
			}
			return false;
		}
	}

	public virtual void ApplyModifier(vDamage damage)
	{
		if (damage.damageValue <= 0 || !CanFilterDamage(damage.damageType) || (destructible && resistance <= 0))
		{
			return;
		}
		int num = 0;
		num = ((!percentage) ? value : (damage.damageValue - damage.damageValue / 100 * value));
		if (destructible)
		{
			resistance -= damage.damageValue;
			onChangeResistance.Invoke(Mathf.Max(resistance, 0f));
			if (resistance <= 0)
			{
				onBroken.Invoke(this);
			}
		}
		if (!destructible || resistance > 0)
		{
			damage.damageValue -= num;
		}
	}

	protected virtual bool CanFilterDamage(string damageType)
	{
		return filterMethod switch
		{
			FilterMethod.ApplyToAll => true, 
			FilterMethod.ApplyToAllInList => damageType.Contains(damageType), 
			FilterMethod.ApplyToAllOutList => !damageType.Contains(damageType), 
			_ => true, 
		};
	}

	public virtual void ResetModifier()
	{
		if (destructible)
		{
			resistance = maxResistance;
			onChangeResistance.Invoke(Mathf.Max(resistance, 0));
		}
	}
}
