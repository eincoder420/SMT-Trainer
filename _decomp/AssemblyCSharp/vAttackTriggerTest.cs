using System.Collections;
using Invector;
using UnityEngine;

public class vAttackTriggerTest : MonoBehaviour
{
	[vButton("Attack", "Attack", typeof(vAttackTriggerTest), true)]
	public Animator animator;

	public AttackSequence[] attackSequences;

	public int minAttackCount;

	public int maxAttackCount;

	private int currentAttackCount;

	public void Start()
	{
	}

	public void Attack()
	{
		currentAttackCount = Random.Range(minAttackCount, maxAttackCount);
		StartCoroutine(Attack(attackSequences[0]));
	}

	public IEnumerator Attack(AttackSequence sequence, int index = 0)
	{
		float speed = animator.speed;
		float time = 0f;
		if (index >= sequence.sequence.Length || sequence.sequence.Length == 0)
		{
			yield break;
		}
		currentAttackCount--;
		bool triggerAttack = false;
		animator.CrossFade(sequence.sequence[index].AnimationPlay, sequence.sequence[index].crossFade);
		while (time < sequence.sequence[index].timeToFinish)
		{
			time += Time.deltaTime;
			if (!triggerAttack && time >= sequence.sequence[index].enableAttackTriggerTime && time < sequence.sequence[index].disableAttackTriggerTime)
			{
				sequence.sequence[index].onEnableAttackTrigger.Invoke();
				triggerAttack = true;
			}
			else if (triggerAttack && time >= sequence.sequence[index].disableAttackTriggerTime)
			{
				sequence.sequence[index].onDisableAttackTrigger.Invoke();
				triggerAttack = false;
			}
			animator.speed = sequence.sequence[index].animatorSpeed;
			yield return null;
		}
		animator.speed = speed;
		if (currentAttackCount > 0)
		{
			if (index + 1 < sequence.sequence.Length)
			{
				StartCoroutine(Attack(sequence, index + 1));
			}
			else
			{
				StartCoroutine(Attack(attackSequences[0]));
			}
		}
	}
}
