using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAIShooterAttack : vStateAction
{
	[vHelpBox("Use this to ignore attack time", vHelpBoxAttribute.MessageType.None)]
	public bool forceCanAttack;

	[Tooltip("This action will check if aim is enabled using vAnimatorTag with  (Upperbody Pose) tag on layer setted")]
	public int aimLayer = 4;

	[Tooltip("The shot rountine just will run when Aim angle is in Max Angle To Shot (Inspector of vControlAIShooter>ShooterSettings")]
	public bool onlyShotWhenInAngle;

	public bool debug;

	public override string categoryName => "Combat/";

	public override string defaultName => "Trigger ShooterAttack";

	public vAIShooterAttack()
	{
		executionType = vFSMComponentExecutionType.OnStateUpdate | vFSMComponentExecutionType.OnStateEnter | vFSMComponentExecutionType.OnStateExit;
	}

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (fsmBehaviour.aiController is vIControlAIShooter)
		{
			ControlAttack(fsmBehaviour, fsmBehaviour.aiController as vIControlAIShooter, executionType);
		}
	}

	protected virtual void ControlAttack(vIFSMBehaviourController fsmBehaviour, vIControlAIShooter combat, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		switch (executionType)
		{
		case vFSMComponentExecutionType.OnStateEnter:
			InitAttack(combat);
			break;
		case vFSMComponentExecutionType.OnStateUpdate:
			HandleAttack(fsmBehaviour, combat);
			break;
		case vFSMComponentExecutionType.OnStateExit:
			FinishAttack(combat);
			break;
		case vFSMComponentExecutionType.OnStateUpdate | vFSMComponentExecutionType.OnStateEnter:
			break;
		}
	}

	protected virtual void InitAttack(vIControlAIShooter combat)
	{
		combat.isInCombat = true;
		combat.InitAttackTime();
	}

	protected virtual void HandleAttack(vIFSMBehaviourController fsmBehaviour, vIControlAIShooter combat)
	{
		combat.AimToTarget(0.2f);
		if ((!onlyShotWhenInAngle || combat.IsInShotAngle) && combat.isAiming && combat.animatorStateInfos.HasTag("Upperbody Pose") && !combat.animator.IsInTransition(aimLayer) && !(combat.animator.GetCurrentAnimatorStateInfo(aimLayer).normalizedTime < 0.9f))
		{
			if (debug)
			{
				Debug.Log("Trigger Shooter Attack");
				fsmBehaviour.SendDebug("Trigger Shooter Attack", this);
			}
			combat.Attack(strongAttack: false, -1, forceCanAttack);
		}
	}

	protected virtual void FinishAttack(vIControlAIShooter combat)
	{
		combat.isInCombat = false;
		combat.ResetAttackTime();
	}
}
