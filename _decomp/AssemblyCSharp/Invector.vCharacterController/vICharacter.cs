using UnityEngine;

namespace Invector.vCharacterController;

public interface vICharacter : vIHealthController, vIDamageReceiver
{
	OnActiveRagdoll onActiveRagdoll { get; }

	Animator animator { get; }

	bool isCrouching { get; }

	bool ragdolled { get; set; }

	void EnableRagdoll();

	void ResetRagdoll();
}
