using System;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class IsInShotAngleDecision : vStateDecision
{
	public override string categoryName => "Combat/";

	public override string defaultName => "IsInShotAngleDecision";

	public override Type requiredType => typeof(vIControlAIShooter);

	public override bool Decide(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour.aiController is vIControlAIShooter)
		{
			return (fsmBehaviour.aiController as vIControlAIShooter).IsInShotAngle;
		}
		return false;
	}
}
