using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vShooterCombatAction : vSimpleCombatAction
{
	public override string categoryName => "Combat/";

	public override string defaultName => "Shooter Combat";

	protected override void OnUpdateCombat(vIControlAICombat controller)
	{
		if (!(controller.currentTarget.transform == null) && controller != null)
		{
			if (controller.targetDistance > controller.attackDistance)
			{
				EngageTarget(controller);
			}
			else
			{
				CombatMovement(controller);
			}
			ControlLookPoint(controller);
			HandleShotAttack(controller);
		}
	}

	protected virtual void HandleShotAttack(vIControlAICombat controller)
	{
		controller.AimToTarget();
		if (controller.canAttack)
		{
			controller.Attack();
		}
	}

	protected override void EngageTarget(vIControlAICombat controller)
	{
		if (!(controller.currentTarget.transform == null) && !controller.animatorStateInfos.HasAnyTag("Attack", "LockMovement", "CustomAction"))
		{
			Vector3 lastTargetPosition = controller.lastTargetPosition;
			controller.StrafeMoveTo(lastTargetPosition, lastTargetPosition - controller.transform.position, engageSpeed);
		}
	}

	protected override void CombatMovement(vIControlAICombat controller)
	{
		if (controller.strafeCombatMovement)
		{
			StrafeCombatMovement(controller);
		}
		else
		{
			SimpleCombatMovement(controller);
		}
	}

	protected override void SimpleCombatMovement(vIControlAICombat controller)
	{
		bool flag = controller.targetDistance > controller.combatRange * 0.8f;
		bool flag2 = controller.targetDistance < controller.minDistanceOfTheTarget;
		Vector3 lastTargetPosition = controller.lastTargetPosition;
		Vector3 vector = (lastTargetPosition - controller.transform.position).normalized * (flag ? (1f + controller.stopingDistance) : (flag2 ? (0f - (1f + controller.stopingDistance)) : 0f));
		controller.StrafeMoveTo(controller.transform.position + vector, (lastTargetPosition - controller.transform.position).normalized, combatSpeed);
	}
}
