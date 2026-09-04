using System;
using System.Collections;
using System.Collections.Generic;
using Invector.vItemManager;
using Invector.vMelee;
using UnityEngine;

namespace Invector.vCharacterController.AI;

public class vSimpleMeleeAI_WeaponsControl : MonoBehaviour
{
	[Header("---- Right Weapon Settings----")]
	public bool useRightWeapon = true;

	public int rightWeaponID;

	public bool randomRightWeapon = true;

	[Header("Right Equip Point")]
	public Transform defaultEquipPointR;

	public List<Transform> customEquipPointR;

	[Header("---- Left Weapon Settings----")]
	public bool useLeftWeapon = true;

	public int leftWeaponID;

	public bool randomLeftWeapon = true;

	[Header("Left Equip Point")]
	public Transform defaultEquipPointL;

	public List<Transform> customEquipPointL;

	public vItemCollection itemCollection;

	protected vSimpleMeleeAI_Controller ai;

	protected vMeleeManager manager;

	protected vItem leftWeaponItem;

	protected vItem rightWeaponItem;

	protected GameObject leftWeapon;

	protected GameObject rightWeapon;

	protected List<vItem> weaponItems = new List<vItem>();

	protected Transform leftArm;

	protected Transform rightArm;

	protected bool inEquip;

	protected bool inUnequip;

	protected float equipTimer;

	protected float unequipTimer;

	protected float timeToStart;

	protected int equipCalls;

	protected bool changeLeft;

	protected bool changeRight;

	private IEnumerator Start()
	{
		itemCollection = GetComponentInChildren<vItemCollection>(includeInactive: true);
		ai = GetComponent<vSimpleMeleeAI_Controller>();
		manager = GetComponent<vMeleeManager>();
		yield return new WaitForEndOfFrame();
		if (!itemCollection || !ai || !manager)
		{
			yield break;
		}
		ai.onSetAgressive.AddListener(OnSetAgressive);
		leftArm = ai.animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
		rightArm = ai.animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
		int i;
		for (i = 0; i < itemCollection.items.Count; i++)
		{
			if (itemCollection.items[i].amount > 0 && itemCollection.itemListData.items.Find((vItem _item) => _item.id == itemCollection.items[i].id && _item.type == vItemType.MeleeWeapon) != null)
			{
				AddItem(itemCollection.items[i].id, itemCollection.items[i].amount);
			}
		}
		if (useRightWeapon)
		{
			if (randomRightWeapon)
			{
				GetRandomWeapon(ref rightWeaponItem, vMeleeType.OnlyAttack);
			}
			else
			{
				GetItemWeapon(rightWeaponID, ref rightWeaponItem, vMeleeType.OnlyAttack);
			}
		}
		if (useLeftWeapon)
		{
			if (randomLeftWeapon)
			{
				GetRandomWeapon(ref leftWeaponItem, vMeleeType.OnlyDefense);
			}
			else
			{
				GetItemWeapon(leftWeaponID, ref leftWeaponItem, vMeleeType.OnlyDefense);
			}
		}
		if ((bool)rightArm && (bool)rightWeaponItem)
		{
			Transform transform = null;
			if (customEquipPointR.Count > 0)
			{
				transform = customEquipPointR.Find((Transform t) => t.name == rightWeaponItem.customHandler);
			}
			if (transform == null)
			{
				transform = defaultEquipPointR;
			}
			if ((bool)transform)
			{
				rightWeapon = UnityEngine.Object.Instantiate(rightWeaponItem.originalObject);
				rightWeapon.transform.parent = transform;
				rightWeapon.transform.localPosition = Vector3.zero;
				rightWeapon.transform.localEulerAngles = Vector3.zero;
				manager.SetRightWeapon(rightWeapon);
				rightWeapon.SetActive(value: false);
				if (ai.agressiveAtFirstSight)
				{
					StartCoroutine(EquipItemRoutine(flipEquip: false, rightWeaponItem, rightWeapon));
				}
			}
		}
		if (!leftArm || !leftWeaponItem)
		{
			yield break;
		}
		Transform transform2 = null;
		if (customEquipPointL.Count > 0)
		{
			transform2 = customEquipPointL.Find((Transform t) => t.name == leftWeaponItem.customHandler);
		}
		if (transform2 == null)
		{
			transform2 = defaultEquipPointL;
		}
		if ((bool)transform2)
		{
			leftWeapon = UnityEngine.Object.Instantiate(leftWeaponItem.originalObject);
			leftWeapon.transform.parent = transform2;
			leftWeapon.transform.localPosition = Vector3.zero;
			leftWeapon.transform.localEulerAngles = Vector3.zero;
			Vector3 localScale = leftWeapon.transform.localScale;
			localScale.x *= -1f;
			leftWeapon.transform.localScale = localScale;
			manager.SetLeftWeapon(leftWeapon);
			leftWeapon.SetActive(value: false);
			if (ai.agressiveAtFirstSight)
			{
				StartCoroutine(EquipItemRoutine(flipEquip: true, leftWeaponItem, leftWeapon));
			}
		}
	}

	public void OnSetAgressive(bool value)
	{
		timeToStart = 2f;
		if (value)
		{
			if ((bool)rightWeapon && !rightWeapon.activeSelf && !changeRight)
			{
				changeRight = true;
				StartCoroutine(EquipItemRoutine(flipEquip: false, rightWeaponItem, rightWeapon));
			}
			if ((bool)leftWeapon && !leftWeapon.activeSelf && !changeLeft)
			{
				changeLeft = true;
				StartCoroutine(EquipItemRoutine(flipEquip: true, leftWeaponItem, leftWeapon));
			}
		}
		else
		{
			if ((bool)rightWeapon && rightWeapon.activeSelf)
			{
				StartCoroutine(UnequipItemRoutine(flipEquip: false, rightWeaponItem, rightWeapon));
			}
			if ((bool)leftWeapon && leftWeapon.activeSelf)
			{
				StartCoroutine(UnequipItemRoutine(flipEquip: true, leftWeaponItem, leftWeapon));
			}
		}
	}

	private void GetItemWeapon(int id, ref vItem weaponItem, vMeleeType type)
	{
		if (weaponItems.Count > 0)
		{
			weaponItem = weaponItems.Find((vItem _item) => _item.id == id && _item.originalObject != null && _item.originalObject.GetComponent<vMeleeWeapon>() != null && (_item.originalObject.GetComponent<vMeleeWeapon>().meleeType == vMeleeType.AttackAndDefense || _item.originalObject.GetComponent<vMeleeWeapon>().meleeType == type));
			weaponItems.Remove(weaponItem);
		}
	}

	private void GetRandomWeapon(ref vItem weaponItem, vMeleeType type)
	{
		if (weaponItems.Count > 0)
		{
			List<vItem> list = weaponItems.FindAll((vItem _item) => _item.originalObject != null && _item.originalObject.GetComponent<vMeleeWeapon>() != null && (_item.originalObject.GetComponent<vMeleeWeapon>().meleeType == vMeleeType.AttackAndDefense || _item.originalObject.GetComponent<vMeleeWeapon>().meleeType == type));
			int num = 0;
			UnityEngine.Random.InitState(UnityEngine.Random.Range(0, DateTime.Now.Millisecond));
			num = UnityEngine.Random.Range(0, list.Count - 1);
			weaponItem = list[num];
			weaponItems.Remove(weaponItem);
		}
	}

	private IEnumerator EquipItemRoutine(bool flipEquip, vItem item, GameObject weapon)
	{
		equipCalls++;
		while (inEquip || timeToStart > 0f || ai.ragdolled)
		{
			timeToStart -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		if (inUnequip)
		{
			yield break;
		}
		inEquip = true;
		if (weapon != null && item != null)
		{
			equipTimer = item.enableDelayTime;
			if (item.type != 0 && !ai.isDead && !string.IsNullOrEmpty(item.EnableAnim))
			{
				ai.animator.SetBool("FlipEquip", flipEquip);
				ai.animator.CrossFade(item.EnableAnim, 0.25f);
			}
			if (weapon != null)
			{
				while (equipTimer > 0f)
				{
					equipTimer -= Time.deltaTime;
					yield return new WaitForEndOfFrame();
				}
				if (!ai.isDead)
				{
					weapon.SetActive(value: true);
				}
			}
		}
		inEquip = false;
		equipCalls--;
		if (equipCalls == 0)
		{
			ai.lockMovement = false;
		}
		if (flipEquip)
		{
			changeLeft = false;
		}
		else
		{
			changeRight = false;
		}
	}

	private IEnumerator UnequipItemRoutine(bool flipEquip, vItem item, GameObject weapon)
	{
		ai.lockMovement = true;
		while (inUnequip || ai.actions || ai.isAttacking || ai.ragdolled)
		{
			yield return new WaitForEndOfFrame();
		}
		if (!inEquip)
		{
			yield return new WaitForSeconds(1f);
			inUnequip = true;
			if (weapon != null && item != null)
			{
				unequipTimer = item.enableDelayTime;
				if (item.type != 0 && !string.IsNullOrEmpty(item.DisableAnim))
				{
					ai.animator.SetBool("FlipEquip", flipEquip);
					ai.animator.CrossFade(item.DisableAnim, 0.25f);
				}
				if (weapon != null)
				{
					while (unequipTimer > 0f)
					{
						unequipTimer -= Time.deltaTime;
						yield return new WaitForEndOfFrame();
					}
					weapon.SetActive(value: false);
				}
			}
			inUnequip = false;
		}
		ai.lockMovement = true;
	}

	public void AddItem(int itemID, int amount)
	{
		if (!(itemCollection.itemListData != null) || itemCollection.itemListData.items.Count <= 0)
		{
			return;
		}
		vItem item = itemCollection.itemListData.items.Find((vItem t) => t.id.Equals(itemID));
		if (!item)
		{
			return;
		}
		List<vItem> list = weaponItems.FindAll((vItem i) => i.stackable && i.id == item.id && i.amount < i.maxStack);
		if (list.Count == 0)
		{
			vItem vItem = UnityEngine.Object.Instantiate(item);
			vItem.name = vItem.name.Replace("(Clone)", string.Empty);
			vItem.amount = 0;
			for (int j = 0; j < item.maxStack; j++)
			{
				if (vItem.amount >= vItem.maxStack)
				{
					break;
				}
				if (amount <= 0)
				{
					break;
				}
				vItem.amount++;
				amount--;
			}
			weaponItems.Add(vItem);
			if (amount > 0)
			{
				AddItem(item.id, amount);
			}
			return;
		}
		int index = weaponItems.IndexOf(list[0]);
		for (int k = 0; k < weaponItems[index].maxStack; k++)
		{
			if (weaponItems[index].amount >= weaponItems[index].maxStack)
			{
				break;
			}
			if (amount <= 0)
			{
				break;
			}
			weaponItems[index].amount++;
			amount--;
		}
		if (amount > 0)
		{
			AddItem(item.id, amount);
		}
	}
}
