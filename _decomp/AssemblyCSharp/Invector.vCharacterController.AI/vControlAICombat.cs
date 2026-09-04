using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Invector.vEventSystems;
using UnityEngine;

namespace Invector.vCharacterController.AI;

[vClassHeader("AI Combat Controller", true, "icon_v2", false, "", iconName = "AI-icon")]
public class vControlAICombat : vControlAI, vIControlAICombat, vIControlAI, vIHealthController, vIDamageReceiver, vIAttackListener
{
	[vEditorToolbar("Combat Settings", false, "", false, false, order = 9)]
	[Header("Attack Settings")]
	[SerializeField]
	protected float _minAttackTime = 0.5f;

	[SerializeField]
	protected float _maxAttackTime = 2f;

	[SerializeField]
	protected int _minAttackCount = 1;

	[SerializeField]
	protected int _maxAttackCount = 3;

	[SerializeField]
	protected float _attackDistance = 1f;

	[Header("Blocking Settings")]
	[Tooltip("The AI has a chance to enter the defence animation while in combat, easier to predict when to attack")]
	[Range(0f, 100f)]
	[SerializeField]
	protected float _combatBlockingChance = 50f;

	[Tooltip("The AI has a random chance to block the damage at the time he receive it, so you won't be able to predict")]
	[Range(0f, 100f)]
	[SerializeField]
	protected float _onDamageBlockingChance = 25f;

	[SerializeField]
	protected float _minStayBlockingTime = 4f;

	[SerializeField]
	protected float _maxStayBlockingTime = 6f;

	[SerializeField]
	protected float _minTimeToTryBlock = 4f;

	[SerializeField]
	protected float _maxTimeToTryBlock = 6f;

	[vHelpBox("Damage type that can block", vHelpBoxAttribute.MessageType.None)]
	[SerializeField]
	protected List<string> ignoreDefenseDamageTypes = new List<string> { "unarmed", "melee" };

	[Header("Combat Movement")]
	[SerializeField]
	protected float _minDistanceOfTheTarget = 2f;

	[SerializeField]
	protected float _combatDistance = 4f;

	[SerializeField]
	protected bool _strafeCombatMovement = true;

	[vHideInInspector("_strafeCombatMovement", false)]
	[SerializeField]
	[Tooltip("This control random Strafe Combate Movement side, if True the side is ever -1 or 1 else side can be set to zero (0)")]
	protected bool _alwaysStrafe;

	[vHideInInspector("_strafeCombatMovement", false)]
	[SerializeField]
	protected float _minTimeToChangeStrafeSide = 1f;

	[vHideInInspector("_strafeCombatMovement", false)]
	[SerializeField]
	protected float _maxTimeToChangeStrafeSide = 4f;

	[vEditorToolbar("Debug", false, "", false, false, order = 100)]
	[vHelpBox("Debug Combat", vHelpBoxAttribute.MessageType.None)]
	[SerializeField]
	[vReadOnly(false)]
	protected bool _isInCombat;

	[SerializeField]
	[vReadOnly(false)]
	protected bool _isAiming;

	[SerializeField]
	[vReadOnly(false)]
	protected bool _isBlocking;

	protected float _attackTime;

	protected float _blockingTime;

	protected float _tryBlockTime;

	protected float _timeToChangeStrafeSide;

	protected vAnimatorParameter isBlockingHash;

	protected vAnimatorParameter isAimingHash;

	private System.Random random = new System.Random();

	public virtual float combatRange => _combatDistance;

	public virtual int strafeCombatSide { get; set; }

	public virtual bool strafeCombatMovement => _strafeCombatMovement;

	public virtual bool isInCombat
	{
		get
		{
			return _isInCombat;
		}
		set
		{
			_isInCombat = value;
		}
	}

	public virtual float minDistanceOfTheTarget => _minDistanceOfTheTarget;

	public virtual float attackDistance => _attackDistance;

	public virtual int attackCount { get; set; }

	public virtual bool canAttack
	{
		get
		{
			if (_attackTime < Time.time && !base.ragdolled)
			{
				return attackCount > 0;
			}
			return false;
		}
	}

	public virtual bool isAttacking => base.animatorStateInfos.HasTag("Attack");

	public virtual bool isBlocking
	{
		get
		{
			return _isBlocking;
		}
		protected set
		{
			if ((int)isBlockingHash > -1)
			{
				base.animator.SetBool(isBlockingHash, value);
			}
			_isBlocking = value;
		}
	}

	public virtual bool isArmed => false;

	public virtual bool canBlockInCombat
	{
		get
		{
			if (_combatBlockingChance > 0f && Time.time > _tryBlockTime && Time.time > _blockingTime)
			{
				return !customAction;
			}
			return false;
		}
	}

	public virtual bool isAiming
	{
		get
		{
			return _isAiming;
		}
		protected set
		{
			_isAiming = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		if ((bool)base.animator)
		{
			isBlockingHash = new vAnimatorParameter(base.animator, "IsBlocking");
			isAimingHash = new vAnimatorParameter(base.animator, "IsAiming");
		}
		strafeCombatSide = 1;
	}

	protected override void UpdateLockMovement()
	{
		if (isAttacking && lockRotation)
		{
			lockMovement = true;
		}
		else
		{
			base.UpdateLockMovement();
		}
	}

	protected override void HandleTarget()
	{
		base.HandleTarget();
		UpdateStrafeCombateMovementSide();
	}

	protected override void UpdateAnimator()
	{
		base.UpdateAnimator();
		UpdateCombatAnimator();
	}

	protected virtual void UpdateCombatAnimator()
	{
		if ((_isBlocking && Time.time > _blockingTime) || customAction)
		{
			_tryBlockTime = UnityEngine.Random.Range(_minTimeToTryBlock, _maxTimeToTryBlock) + Time.time;
			_isBlocking = false;
		}
		if (isBlockingHash.isValid)
		{
			base.animator.SetBool(isBlockingHash, _isBlocking);
		}
		if (isAimingHash.isValid)
		{
			base.animator.SetBool(isAimingHash, _isAiming);
		}
	}

	public override void FindTarget(bool checkForObstacles = true)
	{
		if (base.ragdolled)
		{
			return;
		}
		if (((bool)currentTarget.transform && targetDistance <= combatRange && _hasPositionOfTheTarget) || isAttacking)
		{
			if (!(updateFindTargetTime > Time.time))
			{
				updateFindTargetTime = Time.time + GetUpdateTimeFromQuality(findTargetUpdateQuality);
			}
		}
		else
		{
			base.FindTarget(checkForObstacles);
		}
	}

	public virtual void UpdateStrafeCombateMovementSide()
	{
		if (!strafeCombatMovement)
		{
			return;
		}
		if (_timeToChangeStrafeSide <= 0f)
		{
			int num = UnityEngine.Random.Range(0, 100);
			if (_alwaysStrafe)
			{
				if (num > 50)
				{
					strafeCombatSide = 1;
				}
				else
				{
					strafeCombatSide = -1;
				}
			}
			else if (num >= 70)
			{
				strafeCombatSide = 1;
			}
			else if (num <= 30)
			{
				strafeCombatSide = -1;
			}
			else
			{
				strafeCombatSide = 0;
			}
			_timeToChangeStrafeSide = UnityEngine.Random.Range(_minTimeToChangeStrafeSide, _maxTimeToChangeStrafeSide);
		}
		else
		{
			_timeToChangeStrafeSide -= Time.deltaTime;
		}
	}

	public virtual void Attack(bool strongAttack = false, int attackID = -1, bool forceCanAttack = false)
	{
		if (canAttack || forceCanAttack)
		{
			base.animator.SetTrigger(strongAttack ? "StrongAttack" : "WeakAttack");
		}
	}

	public virtual float BetterRandomThenUnity(float minimum, float maximum)
	{
		return (float)(random.NextDouble() * (double)(maximum - minimum) + (double)minimum);
	}

	public virtual void InitAttackTime()
	{
		_tryBlockTime = BetterRandomThenUnity(_minTimeToTryBlock, _maxTimeToTryBlock) + Time.time;
		_attackTime = BetterRandomThenUnity(_minAttackTime, _maxAttackTime) + Time.time;
		attackCount = (int)BetterRandomThenUnity(_minAttackCount, _maxAttackCount);
	}

	public virtual void ResetAttackTime()
	{
		attackCount = 0;
		_attackTime = UnityEngine.Random.Range(_minAttackTime, _maxAttackTime) + Time.time;
	}

	public virtual void ResetBlockTime()
	{
		_blockingTime = 0f;
	}

	public virtual void Blocking()
	{
		if (!isBlocking && canBlockInCombat && CheckChanceToBlock(_combatBlockingChance))
		{
			isBlocking = true;
			_blockingTime = UnityEngine.Random.Range(_minStayBlockingTime, _maxStayBlockingTime) + Time.time;
		}
	}

	protected virtual void ImmediateBlocking()
	{
		if (CheckChanceToBlock(_onDamageBlockingChance))
		{
			_blockingTime = UnityEngine.Random.Range(_minStayBlockingTime, _maxStayBlockingTime) + Time.time;
			isBlocking = true;
		}
	}

	protected virtual bool CheckChanceToBlock(float chance)
	{
		return UnityEngine.Random.Range(0f, 100f) <= chance;
	}

	public virtual void ResetAttackTriggers()
	{
		base.animator.ResetTrigger("WeakAttack");
		base.animator.ResetTrigger("StrongAttack");
	}

	public virtual void OnEnableAttack()
	{
		attackCount--;
		lockRotation = true;
	}

	public virtual void OnDisableAttack()
	{
		if (attackCount <= 0)
		{
			InitAttackTime();
		}
		lockRotation = false;
	}

	public virtual void AimTo(Vector3 point, float stayLookTime = 1f, object sender = null)
	{
	}

	public virtual void AimToTarget(float stayLookTime = 1f, object sender = null)
	{
	}

	protected virtual void TryBlockAttack(vDamage damage)
	{
		bool flag = !ignoreDefenseDamageTypes.Contains(damage.damageType) && !damage.ignoreDefense;
		if (string.IsNullOrEmpty(damage.damageType) && flag)
		{
			ImmediateBlocking();
		}
		damage.hitReaction = !isBlocking || !flag;
	}

	public override void TakeDamage(vDamage damage)
	{
		TryBlockAttack(damage);
		base.TakeDamage(damage);
	}

	[SpecialName]
	Transform vIDamageReceiver.get_transform()
	{
		return base.transform;
	}

	[SpecialName]
	GameObject vIDamageReceiver.get_gameObject()
	{
		return base.gameObject;
	}
}
