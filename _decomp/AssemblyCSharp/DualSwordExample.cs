using Invector;
using Invector.vMelee;
using UnityEngine;

public class DualSwordExample : vMonoBehaviour
{
	public vMeleeWeapon secundaryWeaponPrefab;

	public string otherSideHandlerName;

	[vReadOnly(true)]
	[SerializeField]
	protected vMeleeWeapon secundaryWeapon;

	[vReadOnly(true)]
	[SerializeField]
	protected Transform otherSideTransform;

	[vReadOnly(true)]
	[SerializeField]
	protected vMeleeManager manager;

	private void Start()
	{
		OnEnable();
	}

	private void OnDestroy()
	{
		OnDisable();
		if ((bool)secundaryWeapon)
		{
			Object.Destroy(secundaryWeapon.gameObject);
		}
	}

	private void OnEnable()
	{
		if (!otherSideTransform)
		{
			Animator componentInParent = GetComponentInParent<Animator>();
			if ((bool)componentInParent)
			{
				Transform[] componentsInChildren = componentInParent.GetBoneTransform(HumanBodyBones.LeftHand).GetComponentsInChildren<Transform>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (componentsInChildren[i].gameObject.name.Equals(otherSideHandlerName))
					{
						otherSideTransform = componentsInChildren[i];
						break;
					}
				}
			}
		}
		if ((bool)otherSideTransform)
		{
			ActiveSecundaryWeapon();
		}
	}

	private void ActiveSecundaryWeapon()
	{
		if ((bool)secundaryWeapon)
		{
			secundaryWeapon.gameObject.SetActive(value: true);
		}
		else
		{
			secundaryWeapon = Object.Instantiate(secundaryWeaponPrefab);
			secundaryWeapon.transform.parent = otherSideTransform;
			secundaryWeapon.transform.localPosition = Vector3.zero;
			secundaryWeapon.transform.localEulerAngles = Vector3.zero;
		}
		if (!manager)
		{
			manager = GetComponentInParent<vMeleeManager>();
		}
		if ((bool)manager)
		{
			manager.SetLeftWeapon(secundaryWeapon);
		}
	}

	private void OnDisable()
	{
		if ((bool)secundaryWeapon)
		{
			secundaryWeapon.gameObject.SetActive(value: false);
			manager.leftWeapon = null;
		}
	}
}
