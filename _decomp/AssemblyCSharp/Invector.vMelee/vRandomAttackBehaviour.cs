using UnityEngine;

namespace Invector.vMelee;

public class vRandomAttackBehaviour : StateMachineBehaviour
{
	public int attackCount;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetInteger("RandomAttack", Random.Range(0, attackCount));
	}
}
