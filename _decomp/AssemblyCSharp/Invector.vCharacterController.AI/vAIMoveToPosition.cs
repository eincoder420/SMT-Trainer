using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI;

[vClassHeader("AI Move To Position", "It's recommended to call the StopFSM function in the OnStartMove Event, to avoid conflicts with the FSM behaviour.")]
public class vAIMoveToPosition : vMonoBehaviour
{
	[Serializable]
	public class vAIPosition
	{
		public string Name;

		public Transform target;

		public vAIMovementSpeed speed = vAIMovementSpeed.Walking;

		public bool rotateToTargetForward;

		public UnityEvent onStartMove;

		public UnityEvent onFinishMove;

		public UnityEvent onCancelMove;

		internal bool canMove;

		private Vector3 lastTargetDirection;

		public IEnumerator MoveToPosition(vIControlAI controlAI)
		{
			lastTargetDirection = controlAI.transform.forward;
			onStartMove.Invoke();
			controlAI.MoveTo(target.position, speed);
			controlAI.ForceUpdatePath(2f);
			yield return new WaitForSeconds(1f);
			while (!controlAI.isInDestination && canMove)
			{
				if (controlAI.remainingDistance > 2f)
				{
					controlAI.MoveTo(target.position, speed);
					lastTargetDirection = target.position - controlAI.transform.position;
					lastTargetDirection.y = 0f;
				}
				else
				{
					controlAI.StrafeMoveTo(target.position, rotateToTargetForward ? target.forward : lastTargetDirection, speed);
				}
				yield return null;
			}
			if (canMove)
			{
				onFinishMove.Invoke();
			}
			else
			{
				onCancelMove.Invoke();
			}
			canMove = false;
		}
	}

	protected vIControlAI controlAI;

	public List<vAIPosition> positions;

	private vAIPosition currentPosition;

	public bool moveToOnStart;

	[vHideInInspector("moveToOnStart", false)]
	public string positionTarget;

	private IEnumerator Start()
	{
		controlAI = GetComponent<vIControlAI>();
		yield return new WaitForEndOfFrame();
		if (moveToOnStart)
		{
			MoveTo(positionTarget);
		}
	}

	public void MoveTo(string positionName)
	{
		if (controlAI == null || controlAI.isDead || (currentPosition != null && !(currentPosition.Name != positionName)))
		{
			return;
		}
		vAIPosition vAIPosition = positions.Find((vAIPosition p) => p.Name.Equals(positionName));
		if (vAIPosition != null)
		{
			if (currentPosition != null && currentPosition.canMove)
			{
				currentPosition.canMove = false;
			}
			currentPosition = vAIPosition;
			currentPosition.canMove = true;
			StartCoroutine(currentPosition.MoveToPosition(controlAI));
		}
	}
}
