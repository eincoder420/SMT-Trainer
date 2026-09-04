using System.Collections.Generic;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAICheckDamage : vStateDecision
{
	public List<string> damageTypeToCheck;

	public override string categoryName => "Health/";

	public override string defaultName => "Check Damage Type";

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		return HasDamage(fsmBehaviour);
	}

	protected virtual bool HasDamage(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour.aiController == null)
		{
			return false;
		}
		bool result = fsmBehaviour.aiController.receivedDamage.isValid && (damageTypeToCheck.Count == 0 || damageTypeToCheck.Contains(fsmBehaviour.aiController.receivedDamage.lasType));
		if (fsmBehaviour.debugMode)
		{
			fsmBehaviour.SendDebug(base.Name + " " + fsmBehaviour.aiController.receivedDamage.isValid.ToString() + " " + fsmBehaviour.aiController.receivedDamage.lastSender, this);
		}
		return result;
	}
}
