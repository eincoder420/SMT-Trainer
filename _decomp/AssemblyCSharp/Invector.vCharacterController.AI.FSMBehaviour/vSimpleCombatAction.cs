using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vSimpleCombatAction : vStateAction
{
	public bool engageInStrafe;

	public vAIMovementSpeed engageSpeed = vAIMovementSpeed.Running;

	public vAIMovementSpeed combatSpeed = vAIMovementSpeed.Walking;

	public override string categoryName => "Combat/";

	public override string defaultName => "Melee Combat";

	public vSimpleCombatAction()
	{
		executionType = vFSMComponentExecutionType.OnStateUpdate | vFSMComponentExecutionType.OnStateEnter | vFSMComponentExecutionType.OnStateExit;
	}

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (fsmBehaviour.aiController is vIControlAICombat)
		{
			vIControlAICombat controller = fsmBehaviour.aiController as vIControlAICombat;
			switch (executionType)
			{
			case vFSMComponentExecutionType.OnStateEnter:
				OnEnterCombat(controller);
				break;
			case vFSMComponentExecutionType.OnStateExit:
				OnExitCombat(controller);
				break;
			case vFSMComponentExecutionType.OnStateUpdate:
				OnUpdateCombat(controller);
				break;
			case vFSMComponentExecutionType.OnStateUpdate | vFSMComponentExecutionType.OnStateEnter:
				break;
			}
		}
	}

	protected virtual void OnEnterCombat(vIControlAICombat controller)
	{
		controller.InitAttackTime();
		controller.isInCombat = true;
	}

	protected virtual void OnExitCombat(vIControlAICombat controller)
	{
		if (controller.currentTarget.transform == null || controller.currentTarget.isDead || !controller.targetInLineOfSight)
		{
			controller.ResetAttackTime();
		}
		controller.isInCombat = false;
	}

	protected virtual void OnUpdateCombat(vIControlAICombat controller)
	{
		if (!(controller.currentTarget.transform == null) && !controller.currentTarget.isLost && controller != null)
		{
			if (controller.canAttack)
			{
				EngageTarget(controller);
			}
			else
			{
				CombatMovement(controller);
			}
			ControlLookPoint(controller);
		}
	}

	protected virtual void EngageTarget(vIControlAICombat controller)
	{
		if (controller.currentTarget.transform == null)
		{
			return;
		}
		if (controller.targetDistance <= controller.attackDistance)
		{
			controller.Stop();
			controller.Attack();
		}
		else if (!controller.animatorStateInfos.HasAnyTag("Attack", "LockMovement", "CustomAction"))
		{
			if (engageInStrafe)
			{
				controller.StrafeMoveTo(controller.currentTarget.transform.position, (controller.currentTarget.transform.position - controller.transform.position).normalized, engageSpeed);
			}
			else
			{
				controller.MoveTo(controller.currentTarget.transform.position, engageSpeed);
			}
		}
		else
		{
			controller.Stop();
		}
	}

	protected virtual void CombatMovement(vIControlAICombat controller)
	{
		if (controller.strafeCombatMovement)
		{
			StrafeCombatMovement(controller);
		}
		else
		{
			SimpleCombatMovement(controller);
		}
		if (controller.canBlockInCombat)
		{
			controller.Blocking();
		}
	}

	protected virtual void ControlLookPoint(vIControlAICombat controller)
	{
		if (!(controller.currentTarget.transform == null) && controller.currentTarget.hasCollider)
		{
			Vector3 lastTargetPosition = controller.lastTargetPosition;
			controller.LookTo(lastTargetPosition);
		}
	}

	protected virtual void SimpleCombatMovement(vIControlAICombat controller)
	{
		bool flag = controller.targetDistance > controller.combatRange * 0.8f;
		bool flag2 = controller.targetDistance < controller.minDistanceOfTheTarget;
		Vector3 vector = (controller.currentTarget.transform.position - controller.transform.position).normalized * (flag ? (1f + controller.stopingDistance) : (flag2 ? (0f - (1f + controller.stopingDistance)) : 0f));
		controller.StrafeMoveTo(controller.transform.position + vector, (controller.currentTarget.transform.position - controller.transform.position).normalized, combatSpeed);
	}

	protected virtual void StrafeCombatMovement(vIControlAICombat controller)
	{
		bool flag = controller.targetDistance > controller.combatRange * 0.8f;
		bool flag2 = controller.targetDistance < controller.minDistanceOfTheTarget;
		Vector3 lastTargetPosition = controller.lastTargetPosition;
		Vector3 vector = (lastTargetPosition - controller.transform.position).normalized * (flag ? (1f + controller.stopingDistance) : (flag2 ? (0f - (1f + controller.stopingDistance)) : 0f));
		controller.StrafeMoveTo(controller.transform.position + controller.transform.right * (controller.stopingDistance + 1f) * controller.strafeCombatSide + vector, (lastTargetPosition - controller.transform.position).normalized, combatSpeed);
	}
}
