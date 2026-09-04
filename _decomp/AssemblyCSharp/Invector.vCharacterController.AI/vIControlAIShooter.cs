using UnityEngine;

namespace Invector.vCharacterController.AI;

public interface vIControlAIShooter : vIControlAICombat, vIControlAI, vIHealthController, vIDamageReceiver
{
	vAIShooterManager shooterManager { get; set; }

	bool IsInShotAngle { get; }

	void SetShooterHitLayer(LayerMask mask);
}
