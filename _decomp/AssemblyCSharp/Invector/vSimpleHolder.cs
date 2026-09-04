using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

[vClassHeader("AI SIMPLE HOLDER", true, "icon_v2", false, "")]
public class vSimpleHolder : vMonoBehaviour
{
	[vHelpBox("These GameObjects are just the weapon mesh to display in the character \n Use events to manipulate the Original Weapon Prefab", vHelpBoxAttribute.MessageType.Info)]
	[Tooltip("The Holder object is just the holder mesh without any colliders or components")]
	public GameObject holderObject;

	[Tooltip("The Weapon object is just the weapon mesh without any colliders or components")]
	public GameObject weaponObject;

	[vHelpBox("Trigger a Equip animation - Check the UpperBody Layer, EquipWeapon State too see how it works.", vHelpBoxAttribute.MessageType.Info)]
	public Animator animator;

	public string equipAnim = "LowBack";

	public string unequipAnim = "LowBack";

	public float equipDelayTime = 0.5f;

	public float unequipDelayTime = 0.5f;

	[vReadOnly(true)]
	[SerializeField]
	protected bool isEquiped;

	private bool inEquip;

	[vHelpBox("Use to enable or disable the Original Weapon Prefab", vHelpBoxAttribute.MessageType.Info)]
	[vHelpBox("Is called when fake weapon (Weapon Mesh) is enabled", vHelpBoxAttribute.MessageType.None)]
	public UnityEvent onEnableWeapon;

	[vHelpBox("Is called when fake weapon (Weapon Mesh) is disabled", vHelpBoxAttribute.MessageType.None)]
	public UnityEvent onDisableWeapon;

	[vHelpBox("Is called when fake holder (Holder Mesh) is enabled", vHelpBoxAttribute.MessageType.None)]
	public UnityEvent onEnableHolder;

	[vHelpBox("Is called when fake holder (Holder Mesh) is disabled", vHelpBoxAttribute.MessageType.None)]
	public UnityEvent onDisableHolder;

	private void SetActiveHolder(bool active)
	{
		if ((bool)holderObject)
		{
			holderObject.SetActive(active);
		}
		if (active)
		{
			onEnableHolder.Invoke();
		}
		else
		{
			onDisableHolder.Invoke();
		}
	}

	private void SetActiveWeapon(bool active)
	{
		if ((bool)weaponObject)
		{
			weaponObject.SetActive(active);
		}
		if (active)
		{
			onEnableWeapon.Invoke();
		}
		else
		{
			onDisableWeapon.Invoke();
		}
	}

	public void EquipWeapon()
	{
		if (!isEquiped)
		{
			if ((bool)animator)
			{
				animator.CrossFadeInFixedTime(equipAnim, 0.25f);
			}
			StartCoroutine(EquipRoutine());
		}
	}

	public void UnequipWeapon()
	{
		if (isEquiped)
		{
			if ((bool)animator)
			{
				animator.CrossFadeInFixedTime(unequipAnim, 0.25f);
			}
			StartCoroutine(UnequipRoutine());
		}
	}

	private IEnumerator UnequipRoutine()
	{
		SetActiveHolder(active: true);
		if (!inEquip)
		{
			float equipTime = unequipDelayTime;
			while (equipTime > 0f && !inEquip)
			{
				yield return null;
				equipTime -= Time.deltaTime;
			}
		}
		SetActiveWeapon(active: true);
		isEquiped = false;
	}

	private IEnumerator EquipRoutine()
	{
		inEquip = true;
		SetActiveHolder(active: true);
		for (float equipTime = equipDelayTime; equipTime > 0f; equipTime -= Time.deltaTime)
		{
			yield return null;
		}
		SetActiveWeapon(active: false);
		inEquip = false;
		isEquiped = true;
	}
}
