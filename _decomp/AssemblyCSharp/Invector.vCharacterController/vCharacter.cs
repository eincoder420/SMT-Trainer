using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController;

[Serializable]
[vClassHeader("vCharacter", true, "icon_v2", false, "")]
public class vCharacter : vHealthController, vICharacter, vIHealthController, vIDamageReceiver
{
	public enum DeathBy
	{
		Animation,
		AnimationWithRagdoll,
		Ragdoll
	}

	[vEditorToolbar("Health", false, "", false, false)]
	public DeathBy deathBy;

	public bool removeComponentsAfterDie;

	[vEditorToolbar("Debug", false, "", false, false, order = 9)]
	[HideInInspector]
	public bool debugActionListener;

	[vEditorToolbar("Events", false, "", false, false)]
	public UnityEvent OnCrouch;

	public UnityEvent OnStandUp;

	[SerializeField]
	protected OnActiveRagdoll _onActiveRagdoll = new OnActiveRagdoll();

	public UnityEvent onDisableRagdoll;

	[Header("Check if Character is in Trigger with tag Action")]
	[HideInInspector]
	public OnActionHandle onActionEnter = new OnActionHandle();

	[HideInInspector]
	public OnActionHandle onActionStay = new OnActionHandle();

	[HideInInspector]
	public OnActionHandle onActionExit = new OnActionHandle();

	protected vAnimatorParameter hitDirectionHash;

	protected vAnimatorParameter reactionIDHash;

	protected vAnimatorParameter triggerReactionHash;

	protected vAnimatorParameter triggerResetStateHash;

	protected vAnimatorParameter recoilIDHash;

	protected vAnimatorParameter triggerRecoilHash;

	protected bool isInit;

	protected bool _isCrouching;

	public Animator animator { get; protected set; }

	public bool ragdolled { get; set; }

	public OnActiveRagdoll onActiveRagdoll
	{
		get
		{
			return _onActiveRagdoll;
		}
		protected set
		{
			_onActiveRagdoll = value;
		}
	}

	public virtual bool isCrouching
	{
		get
		{
			return _isCrouching;
		}
		set
		{
			if (value != _isCrouching)
			{
				if (value)
				{
					OnCrouch.Invoke();
				}
				else
				{
					OnStandUp.Invoke();
				}
			}
			_isCrouching = value;
		}
	}

	public virtual void Init()
	{
		animator = GetComponent<Animator>();
		if ((bool)animator)
		{
			hitDirectionHash = new vAnimatorParameter(animator, "HitDirection");
			reactionIDHash = new vAnimatorParameter(animator, "ReactionID");
			triggerReactionHash = new vAnimatorParameter(animator, "TriggerReaction");
			triggerResetStateHash = new vAnimatorParameter(animator, "ResetState");
			recoilIDHash = new vAnimatorParameter(animator, "RecoilID");
			triggerRecoilHash = new vAnimatorParameter(animator, "TriggerRecoil");
		}
		this.LoadActionControllers(debugActionListener);
	}

	public virtual void ResetRagdoll()
	{
	}

	public virtual void EnableRagdoll()
	{
	}

	protected virtual void OnTriggerEnter(Collider other)
	{
		onActionEnter.Invoke(other);
	}

	protected virtual void OnTriggerStay(Collider other)
	{
		onActionStay.Invoke(other);
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		onActionExit.Invoke(other);
	}

	public override void TakeDamage(vDamage damage)
	{
		base.TakeDamage(damage);
		TriggerDamageReaction(damage);
	}

	protected virtual void TriggerDamageReaction(vDamage damage)
	{
		if (animator != null && animator.enabled && !damage.activeRagdoll && base.currentHealth > 0f)
		{
			if (hitDirectionHash.isValid && (bool)damage.sender)
			{
				animator.SetInteger(hitDirectionHash, (int)base.transform.HitAngle(damage.sender.position));
			}
			if (damage.hitReaction)
			{
				if (reactionIDHash.isValid)
				{
					animator.SetInteger(reactionIDHash, damage.reaction_id);
				}
				if (triggerReactionHash.isValid)
				{
					SetTrigger(triggerReactionHash);
				}
				if (triggerResetStateHash.isValid)
				{
					SetTrigger(triggerResetStateHash);
				}
			}
			else
			{
				if (recoilIDHash.isValid)
				{
					animator.SetInteger(recoilIDHash, damage.recoil_id);
				}
				if (triggerRecoilHash.isValid)
				{
					SetTrigger(triggerRecoilHash);
				}
				if (triggerResetStateHash.isValid)
				{
					SetTrigger(triggerResetStateHash);
				}
			}
		}
		if (damage.activeRagdoll)
		{
			onActiveRagdoll.Invoke(damage);
		}
	}

	private IEnumerator SetTriggerRoutine(int trigger)
	{
		animator.SetTrigger(trigger);
		yield return new WaitForSeconds(0.1f);
		animator.ResetTrigger(trigger);
	}

	public virtual void SetTrigger(int trigger)
	{
		StartCoroutine(SetTriggerRoutine(trigger));
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
