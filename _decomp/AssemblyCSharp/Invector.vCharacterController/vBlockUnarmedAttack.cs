using UnityEngine;

namespace Invector.vCharacterController;

public class vBlockUnarmedAttack : MonoBehaviour
{
	private vMeleeCombatInput meleeCombatInput;

	[SerializeField]
	protected bool useUnarmedAttack;

	public bool IsActiveUnarmedAttack
	{
		get
		{
			return useUnarmedAttack;
		}
		protected set
		{
			useUnarmedAttack = value;
		}
	}

	private void Start()
	{
		meleeCombatInput = GetComponent<vMeleeCombatInput>();
		meleeCombatInput.onUpdate += HandleAttackInput;
	}

	private void HandleAttackInput()
	{
		if (!IsActiveUnarmedAttack)
		{
			meleeCombatInput.weakAttackInput.useInput = meleeCombatInput.isArmed;
			meleeCombatInput.strongAttackInput.useInput = meleeCombatInput.isArmed;
		}
	}

	public void SetActiveUnarmedAttack(bool value)
	{
		if (value != IsActiveUnarmedAttack)
		{
			IsActiveUnarmedAttack = value;
			meleeCombatInput.weakAttackInput.useInput = value;
			meleeCombatInput.strongAttackInput.useInput = value;
		}
	}
}
