using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAddHealth : vStateAction
{
	public bool recoverWhenIsDead;

	[Header("This action won't work with the DecisionTimer")]
	public float timeToAdd = 1f;

	public int healthToRecovery = 1;

	public override string categoryName => "Controller/";

	public override string defaultName => "Add Health";

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (!fsmBehaviour.aiController.isDead || recoverWhenIsDead)
		{
			AddHealth(fsmBehaviour);
		}
	}

	protected virtual void AddHealth(vIFSMBehaviourController fsmBehaviour)
	{
		if (fsmBehaviour != null && InTimer(fsmBehaviour, timeToAdd))
		{
			fsmBehaviour.aiController.ChangeHealth(healthToRecovery);
		}
	}
}
