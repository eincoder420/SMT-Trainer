using UnityEngine.Events;

namespace Invector;

[vClassHeader("FPS Attack Control", true, "icon_v2", false, "", openClose = false)]
public class vFPSAttackControl : vMonoBehaviour
{
	public float enableTime;

	public float disableTime = 1f;

	public UnityEvent onAttack;

	public UnityEvent onEnableAttack;

	public UnityEvent onDisableAttack;

	public UnityEvent onActiveWeapon;

	public UnityEvent onDisableWeapon;

	public void Attack()
	{
		onAttack.Invoke();
		Invoke("EnableAttack", enableTime);
		Invoke("DisableAttack", disableTime);
	}

	private void EnableAttack()
	{
		onEnableAttack.Invoke();
	}

	private void DisableAttack()
	{
		onDisableAttack.Invoke();
	}

	public void SetActiveWeapon(bool value)
	{
		if (value)
		{
			onActiveWeapon.Invoke();
		}
		else
		{
			onDisableWeapon.Invoke();
		}
	}
}
