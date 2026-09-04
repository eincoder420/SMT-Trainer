using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vShooter;

[vClassHeader("v Bow Control", true, "icon_v2", false, "")]
[RequireComponent(typeof(vShooterWeapon))]
public class vBowControl : vMonoBehaviour
{
	private vShooterWeapon weapon;

	private Animator animator;

	public float delayToSpringAfterShot;

	public float minPenetration;

	public float maxPenetration;

	public UnityEvent OnFinishShot;

	public UnityEvent OnEnableArrow;

	public UnityEvent OnDisableArrow;

	private void Start()
	{
		weapon = GetComponent<vShooterWeapon>();
		animator = GetComponent<Animator>();
	}

	public void EnableArrow()
	{
		if ((bool)weapon)
		{
			if ((bool)animator)
			{
				animator.SetFloat("PowerCharger", 0f);
			}
			if ((bool)animator)
			{
				animator.ResetTrigger("Shot");
			}
			if (weapon.ammoCount > 0)
			{
				OnEnableArrow.Invoke();
			}
		}
	}

	public void DisableArrow()
	{
		OnDisableArrow.Invoke();
		if ((bool)animator)
		{
			animator.ResetTrigger("Shot");
		}
		if ((bool)animator)
		{
			animator.SetFloat("PowerCharger", 0f);
		}
	}

	public void OnChangePowerCharger(float charger)
	{
		if ((bool)animator)
		{
			animator.SetFloat("PowerCharger", charger);
		}
	}

	public void Shot()
	{
		if ((bool)weapon)
		{
			if ((bool)animator)
			{
				animator.SetFloat("PowerCharger", 0f);
			}
			if ((bool)animator)
			{
				animator.SetTrigger("Shot");
			}
			StartCoroutine(ShootEffect());
		}
	}

	public void OnInstantiateProjectile(vProjectileControl pCtrl)
	{
		if ((bool)weapon)
		{
			vArrow component = pCtrl.GetComponent<vArrow>();
			if ((bool)component)
			{
				component.penetration = Mathf.Lerp(minPenetration, maxPenetration, weapon.powerCharge);
			}
		}
	}

	private IEnumerator ShootEffect()
	{
		yield return new WaitForSeconds(delayToSpringAfterShot);
		if (weapon.isAiming && weapon.ammoCount > 0)
		{
			OnFinishShot.Invoke();
		}
	}
}
