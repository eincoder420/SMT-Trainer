using UnityEngine;

namespace Invector.vCharacterController.AI;

public interface vIControlAICombat : vIControlAI, vIHealthController, vIDamageReceiver
{
	int strafeCombatSide { get; }

	float minDistanceOfTheTarget { get; }

	float combatRange { get; }

	bool isInCombat { get; set; }

	bool strafeCombatMovement { get; }

	int attackCount { get; set; }

	float attackDistance { get; }

	bool isAttacking { get; }

	bool canAttack { get; }

	bool isBlocking { get; }

	bool canBlockInCombat { get; }

	bool isAiming { get; }

	bool isArmed { get; }

	void InitAttackTime();

	void ResetAttackTime();

	void Attack(bool strongAttack = false, int attackID = -1, bool forceCanAttack = false);

	void ResetBlockTime();

	void Blocking();

	void AimTo(Vector3 point, float stayLookTime = 1f, object sender = null);

	void AimToTarget(float stayLookTime = 1f, object sender = null);
}
