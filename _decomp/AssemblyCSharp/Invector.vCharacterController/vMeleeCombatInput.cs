using System.Runtime.CompilerServices;
using Invector.vEventSystems;
using Invector.vMelee;
using UnityEngine;

namespace Invector.vCharacterController;

[vClassHeader("MELEE INPUT MANAGER", true, "icon_v2", false, "", iconName = "inputIcon")]
public class vMeleeCombatInput : vThirdPersonInput, vIMeleeFighter, vIAttackReceiver, vIAttackListener
{
	[vEditorToolbar("Inputs", false, "", false, false)]
	[Header("Melee Inputs")]
	public GenericInput weakAttackInput = new GenericInput("Mouse0", "RB", "RB");

	public GenericInput strongAttackInput = new GenericInput("Alpha1", keyboardAxis: false, "RT", joystickAxis: true, "RT", mobileAxis: false);

	public GenericInput blockInput = new GenericInput("Mouse1", "LB", "LB");

	internal vMeleeManager meleeManager;

	protected bool _isAttacking;

	[HideInInspector]
	public bool lockMeleeInput;

	public bool isAttacking
	{
		get
		{
			if (!_isAttacking)
			{
				return cc.IsAnimatorTag("Attack");
			}
			return true;
		}
		protected set
		{
			_isAttacking = value;
		}
	}

	public bool isBlocking { get; protected set; }

	public bool isArmed
	{
		get
		{
			if (meleeManager != null)
			{
				if (!(meleeManager.rightWeapon != null))
				{
					if (meleeManager.leftWeapon != null)
					{
						return meleeManager.leftWeapon.meleeType != vMeleeType.OnlyDefense;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public bool isEquipping { get; protected set; }

	public virtual bool lockInventory
	{
		get
		{
			if (!isAttacking && !cc.isDead && !cc.customAction)
			{
				return cc.isRolling;
			}
			return true;
		}
	}

	public virtual int defaultMoveSetID { get; set; }

	public virtual bool overrideWeaponMoveSetID { get; set; }

	public virtual int meleeMoveSetID
	{
		get
		{
			int moveSetID = meleeManager.GetMoveSetID();
			if (moveSetID == 0 || overrideWeaponMoveSetID)
			{
				moveSetID = defaultMoveSetID;
			}
			return moveSetID;
		}
	}

	public virtual int AttackID
	{
		get
		{
			if (!meleeManager)
			{
				return 0;
			}
			return meleeManager.GetAttackID();
		}
	}

	public virtual int DefenseID
	{
		get
		{
			if (!meleeManager)
			{
				return 0;
			}
			return meleeManager.GetDefenseID();
		}
	}

	public virtual vICharacter character => cc;

	public void SetLockMeleeInput(bool value)
	{
		lockMeleeInput = value;
		if (value)
		{
			isAttacking = false;
			isBlocking = false;
		}
	}

	public override void SetLockAllInput(bool value)
	{
		base.SetLockAllInput(value);
		SetLockMeleeInput(value);
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void LateUpdate()
	{
		UpdateMeleeAnimations();
		base.LateUpdate();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	protected override void InputHandle()
	{
		if (!(cc == null) && !cc.isDead)
		{
			base.InputHandle();
			if (MeleeAttackConditions() && !lockMeleeInput)
			{
				MeleeWeakAttackInput();
				MeleeStrongAttackInput();
				BlockingInput();
			}
			else
			{
				ResetAttackTriggers();
				isBlocking = false;
			}
		}
	}

	public virtual void MeleeWeakAttackInput()
	{
		if (!(cc.animator == null) && weakAttackInput.GetButtonDown() && MeleeAttackStaminaConditions())
		{
			TriggerWeakAttack();
		}
	}

	public virtual void TriggerWeakAttack()
	{
		cc.animator.SetInteger(vAnimatorParameters.AttackID, AttackID);
		cc.animator.SetTrigger(vAnimatorParameters.WeakAttack);
	}

	public virtual void MeleeStrongAttackInput()
	{
		if (!(cc.animator == null) && strongAttackInput.GetButtonDown() && (!meleeManager.CurrentActiveAttackWeapon || meleeManager.CurrentActiveAttackWeapon.useStrongAttack) && MeleeAttackStaminaConditions())
		{
			TriggerStrongAttack();
		}
	}

	public virtual void TriggerStrongAttack()
	{
		cc.animator.SetInteger(vAnimatorParameters.AttackID, AttackID);
		cc.animator.SetTrigger(vAnimatorParameters.StrongAttack);
	}

	public virtual void BlockingInput()
	{
		if (!(cc.animator == null))
		{
			isBlocking = blockInput.GetButton() && cc.currentStamina > 0f && !cc.customAction && !isAttacking;
		}
	}

	protected override void SprintInput()
	{
		if (sprintInput.useInput)
		{
			bool flag = (cc.useContinuousSprint ? sprintInput.GetButtonDown() : sprintInput.GetButton());
			cc.Sprint(flag && !isAttacking);
		}
	}

	protected virtual bool MeleeAttackStaminaConditions()
	{
		return cc.currentStamina - meleeManager.GetAttackStaminaCost() >= 0f;
	}

	protected virtual bool MeleeAttackConditions()
	{
		if (meleeManager == null)
		{
			meleeManager = GetComponent<vMeleeManager>();
		}
		if (meleeManager != null && cc.isGrounded && !cc.customAction && !cc.isJumping && !cc.isCrouching && !cc.isRolling && !isEquipping)
		{
			return !cc.animator.IsInTransition(cc.baseLayer);
		}
		return false;
	}

	protected override bool JumpConditions()
	{
		if (!isAttacking)
		{
			return base.JumpConditions();
		}
		return false;
	}

	protected override bool RollConditions()
	{
		if (base.RollConditions() && !isAttacking && !cc.animator.IsInTransition(cc.upperBodyLayer))
		{
			return !cc.animator.IsInTransition(cc.fullbodyLayer);
		}
		return false;
	}

	protected virtual void UpdateMeleeAnimations()
	{
		if (!(cc.animator == null) && !(meleeManager == null))
		{
			cc.animator.SetInteger(vAnimatorParameters.AttackID, AttackID);
			cc.animator.SetInteger(vAnimatorParameters.DefenseID, DefenseID);
			cc.animator.SetBool(vAnimatorParameters.IsBlocking, isBlocking);
			cc.animator.SetFloat(vAnimatorParameters.MoveSet_ID, meleeMoveSetID, 0.2f, vTime.deltaTime);
			isEquipping = cc.IsAnimatorTag("IsEquipping");
		}
	}

	public virtual void ResetMeleeAnimations()
	{
		if (!(meleeManager == null) && (bool)base.animator)
		{
			cc.animator.SetBool(vAnimatorParameters.IsBlocking, value: false);
		}
	}

	public virtual void OnEnableAttack()
	{
		if (meleeManager == null)
		{
			meleeManager = GetComponent<vMeleeManager>();
		}
		if (!(meleeManager == null))
		{
			cc.currentStaminaRecoveryDelay = meleeManager.GetAttackStaminaRecoveryDelay();
			cc.currentStamina -= meleeManager.GetAttackStaminaCost();
			isAttacking = true;
			cc.isSprinting = false;
		}
	}

	public virtual void OnDisableAttack()
	{
		isAttacking = false;
	}

	public virtual void ResetAttackTriggers()
	{
		cc.animator.ResetTrigger(vAnimatorParameters.WeakAttack);
		cc.animator.ResetTrigger(vAnimatorParameters.StrongAttack);
	}

	public virtual void BreakAttack(int breakAtkID)
	{
		ResetAttackTriggers();
		OnRecoil(breakAtkID);
	}

	public virtual void OnRecoil(int recoilID)
	{
		cc.animator.SetInteger(vAnimatorParameters.RecoilID, recoilID);
		cc.animator.SetTrigger(vAnimatorParameters.TriggerRecoil);
		cc.animator.SetTrigger(vAnimatorParameters.ResetState);
		cc.animator.ResetTrigger(vAnimatorParameters.WeakAttack);
		cc.animator.ResetTrigger(vAnimatorParameters.StrongAttack);
	}

	public virtual void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
	{
		if (!damage.ignoreDefense && isBlocking && meleeManager != null && meleeManager.CanBlockAttack(damage.sender.position))
		{
			int defenseRate = meleeManager.GetDefenseRate();
			if (defenseRate > 0)
			{
				damage.ReduceDamage(defenseRate);
			}
			if (attacker != null && meleeManager != null && meleeManager.CanBreakAttack())
			{
				attacker.BreakAttack(meleeManager.GetDefenseRecoilID());
			}
			meleeManager.OnDefense();
			cc.currentStaminaRecoveryDelay = damage.staminaRecoveryDelay;
			cc.currentStamina -= damage.staminaBlockCost;
		}
		damage.hitReaction = !isBlocking || damage.ignoreDefense;
		cc.TakeDamage(damage);
	}

	[SpecialName]
	Transform vIMeleeFighter.get_transform()
	{
		return base.transform;
	}

	[SpecialName]
	GameObject vIMeleeFighter.get_gameObject()
	{
		return base.gameObject;
	}
}
