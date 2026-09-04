using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Invector.vCharacterController.AI;

public class vSimpleMeleeAI_Companion : vSimpleMeleeAI_Controller
{
	public enum CompanionState
	{
		None,
		Follow,
		MoveTo,
		Stay
	}

	[vEditorToolbar("Companion", false, "", false, false)]
	public string companionTag = "Player";

	public float companionMaxDistance = 10f;

	[Range(0f, 1.5f)]
	public float followSpeed = 1f;

	public float followStopDistance = 2f;

	[Range(0f, 1.5f)]
	public float moveToStopDistance = 0.5f;

	public Transform moveToTarget;

	public CompanionState companionState = CompanionState.Follow;

	public Transform companion;

	public bool debug = true;

	public Text debugUIText;

	private float companionDistance
	{
		get
		{
			if (!(companion != null))
			{
				return 0f;
			}
			return Vector3.Distance(base.transform.position, companion.transform.position);
		}
	}

	private bool nearOfCompanion
	{
		get
		{
			if (!(companion != null) || !companion.gameObject.activeSelf || !(companionDistance < companionMaxDistance))
			{
				if (!(companion == null))
				{
					return !companion.gameObject.activeSelf;
				}
				return true;
			}
			return true;
		}
	}

	protected override float maxSpeed
	{
		get
		{
			if (companionState != 0)
			{
				if (companionState != CompanionState.Follow)
				{
					if (companionState != CompanionState.MoveTo)
					{
						return 0f;
					}
					return moveToSpeed;
				}
				return followSpeed;
			}
			return base.maxSpeed;
		}
	}

	protected void LateUpdate()
	{
		CompanionInputs();
	}

	private void CompanionInputs()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			companionState = CompanionState.Stay;
			agressiveAtFirstSight = false;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			companionState = CompanionState.Follow;
			agressiveAtFirstSight = false;
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			agressiveAtFirstSight = !agressiveAtFirstSight;
		}
		if (Input.GetKeyDown(KeyCode.Alpha4) && moveToTarget != null)
		{
			SetMoveTo(moveToTarget);
			companionState = CompanionState.MoveTo;
			agressiveAtFirstSight = false;
		}
	}

	public void SetMoveTo(Transform _target)
	{
		companionState = CompanionState.MoveTo;
		moveToTarget = _target;
	}

	protected override void Start()
	{
		try
		{
			GameObject gameObject = GameObject.FindGameObjectWithTag(companionTag);
			if (gameObject != null)
			{
				companion = gameObject.transform;
			}
			else
			{
				companionState = CompanionState.None;
				Debug.LogWarning("Cant find the " + companionTag);
			}
		}
		catch (UnityException ex)
		{
			companionState = CompanionState.None;
			Debug.LogWarning("AICompanion Cant find the " + companionTag);
			Debug.LogWarning("AICompanion " + ex.Message);
		}
		Init();
		agent.enabled = true;
		StartCoroutine(CompanionStateRoutine());
		StartCoroutine(FindTarget());
		StartCoroutine(DestinationBehaviour());
	}

	protected IEnumerator CompanionStateRoutine()
	{
		while (base.enabled)
		{
			yield return new WaitForEndOfFrame();
			StringBuilder debugString = new StringBuilder();
			debugString.AppendLine("----DEBUG----");
			debugString.AppendLine("Agressive : " + agressiveAtFirstSight);
			CheckIsOnNavMesh();
			CheckAutoCrouch();
			SetTarget();
			switch (companionState)
			{
			case CompanionState.Follow:
				if (canSeeTarget && nearOfCompanion)
				{
					yield return StartCoroutine(Chase());
				}
				else
				{
					yield return StartCoroutine(FollowCompanion());
				}
				debugString.AppendLine((canSeeTarget && nearOfCompanion) ? "Chase/Follow" : "Follow");
				break;
			case CompanionState.MoveTo:
				if (canSeeTarget)
				{
					yield return StartCoroutine(Chase());
				}
				else
				{
					yield return StartCoroutine(MoveTo());
				}
				debugString.AppendLine(canSeeTarget ? "Chase/MoveTo" : "MoveTo");
				break;
			case CompanionState.Stay:
				if (canSeeTarget)
				{
					yield return StartCoroutine(Chase());
				}
				else
				{
					yield return StartCoroutine(Stay());
				}
				debugString.AppendLine(canSeeTarget ? "Chase/Stay" : "Stay");
				break;
			case CompanionState.None:
				debugString.AppendLine("None : using normal AI routine");
				switch (currentState)
				{
				case AIStates.Idle:
					debugString.AppendLine("idle");
					yield return StartCoroutine(Idle());
					break;
				case AIStates.Chase:
					yield return StartCoroutine(Chase());
					break;
				case AIStates.Wander:
					yield return StartCoroutine(base.Wander());
					break;
				}
				break;
			}
			if (debugUIText != null && debug)
			{
				debugUIText.text = debugString.ToString();
			}
		}
	}

	protected IEnumerator Stay()
	{
		if (companion != null)
		{
			agent.speed = Mathf.Lerp(agent.speed, 0f, 2f * Time.deltaTime);
		}
		else
		{
			yield return StartCoroutine(Idle());
		}
	}

	protected override void SetAgressive(bool value)
	{
		if (companionState != CompanionState.Follow)
		{
			base.SetAgressive(value);
		}
	}

	private IEnumerator FollowCompanion()
	{
		while (!agent.enabled || base.currentHealth <= 0f)
		{
			yield return null;
		}
		if (companion != null && companion.gameObject.activeSelf)
		{
			agent.speed = Mathf.Lerp(agent.speed, followSpeed, 10f * Time.deltaTime);
			agent.stoppingDistance = followStopDistance;
			UpdateDestination(companion.position);
		}
		else
		{
			agent.speed = Mathf.Lerp(agent.speed, moveToSpeed, 10f * Time.deltaTime);
			agent.stoppingDistance = moveToStopDistance;
			UpdateDestination(startPosition);
		}
	}

	private IEnumerator MoveTo()
	{
		while (!agent.enabled || base.currentHealth <= 0f)
		{
			yield return null;
		}
		agent.speed = Mathf.Lerp(agent.speed, moveToSpeed, 2f * Time.deltaTime);
		agent.stoppingDistance = moveToStopDistance;
		UpdateDestination(moveToTarget.position);
		if (canSeeTarget && nearOfCompanion)
		{
			currentState = AIStates.Chase;
		}
	}
}
