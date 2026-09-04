using UnityEngine;

namespace Invector.vCharacterController.AI;

public class vAITester : MonoBehaviour
{
	public vControlAI ai;

	public Transform target;

	public vAIMovementSpeed speed = vAIMovementSpeed.Running;

	public bool testEnabled;

	public void MoveToTarget()
	{
		ai.MoveTo(target.position, speed);
	}

	public void Stop()
	{
		ai.Stop();
	}

	public void LookToTarget()
	{
		ai.LookToTarget(target, 2f, 0f);
	}

	public void RotateToTarget()
	{
		Vector3 vector = target.position - base.transform.position;
		vector.y = 0f;
		ai.RotateTo(vector.normalized);
	}

	public void Attack()
	{
		if (ai is vIControlAICombat)
		{
			(ai as vIControlAICombat).Attack(strongAttack: false, -1, forceCanAttack: true);
		}
	}

	public void Attack(bool strong = false)
	{
		if (ai is vIControlAICombat)
		{
			(ai as vIControlAICombat).Attack(strong, -1, forceCanAttack: true);
		}
	}

	public void GoToTarget(Transform target)
	{
	}
}
