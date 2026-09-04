namespace Invector.vCharacterController.AI.FSMBehaviour;

public class vAIAttack : vStateAction
{
	public bool overrideAttackDistance;

	[vHideInInspector("overrideAttackDistance", false)]
	public float attackDistance;

	public bool overrideAttackID;

	[vHideInInspector("overrideAttackID", false)]
	public int attackID;

	public bool overrideStrongAttack;

	[vHideInInspector("overrideStrongAttack", false)]
	public bool strongAttack;

	[vHelpBox("Force attack when attack distance reached", vHelpBoxAttribute.MessageType.None)]
	public bool forceFirstAttack;

	[vHelpBox("Speed Movement to Attack distance", vHelpBoxAttribute.MessageType.None)]
	public vAIMovementSpeed attackSpeed = vAIMovementSpeed.Walking;

	public override string categoryName => "Combat/";

	public override string defaultName => "Trigger MeleeAttack";

	public vAIAttack()
	{
		executionType = vFSMComponentExecutionType.OnStateUpdate | vFSMComponentExecutionType.OnStateEnter | vFSMComponentExecutionType.OnStateExit;
	}

	public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		Attack(fsmBehaviour.aiController as vIControlAICombat, executionType);
	}

	public virtual void Attack(vIControlAICombat aICombat, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
	{
		if (executionType == vFSMComponentExecutionType.OnStateEnter)
		{
			aICombat.InitAttackTime();
			if (forceFirstAttack)
			{
				aICombat.Attack(overrideStrongAttack && strongAttack, overrideAttackID ? attackID : (-1), forceCanAttack: true);
			}
		}
		if (aICombat != null && (bool)aICombat.currentTarget.transform)
		{
			if (aICombat.targetDistance <= (overrideAttackDistance ? attackDistance : aICombat.attackDistance))
			{
				aICombat.RotateTo(aICombat.currentTarget.transform.position - aICombat.transform.position);
				if (!aICombat.isAttacking && aICombat.canAttack)
				{
					aICombat.Attack(overrideStrongAttack && strongAttack, overrideAttackID ? attackID : (-1));
				}
			}
			else
			{
				aICombat.MoveTo(aICombat.currentTarget.transform.position, attackSpeed);
			}
		}
		if (executionType == vFSMComponentExecutionType.OnStateExit)
		{
			aICombat.ResetAttackTime();
		}
	}
}
