using System.Collections.Generic;
using Invector.vMelee;

namespace Invector.vCharacterController.AI;

public interface vIControlAIMelee : vIControlAICombat, vIControlAI, vIHealthController, vIDamageReceiver
{
	vMeleeManager MeleeManager { get; set; }

	void SetMeleeHitTags(List<string> tags);
}
