using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI;

[vClassHeader("AI Cover", true, "icon_v2", false, "")]
public class vAICover : vMonoBehaviour, vIAIComponent
{
	[vHelpBox("If you own the Shooter Cover Add-on you can also use the cover prefabs from the add-on, open this script and follow the instructions at the top", vHelpBoxAttribute.MessageType.Warning)]
	[vEditorToolbar("Settings", false, "", false, false)]
	public float getCoverRange = 10f;

	public float minDistanceOfThreat = 5f;

	public float maxDistanceOfThreat = 50f;

	public float minAngleOfThreat = 100f;

	[vMinMax(minLimit = 2f, maxLimit = 100f)]
	public Vector2 timeToChangeCover = new Vector2(2f, 10f);

	public string coverTag = "CoverPoint";

	public LayerMask coverLayer;

	[vHelpBox("<i>\n Check AI Controller receivedDamage in Debug Toolbar to debug massive damage Values</i>", vHelpBoxAttribute.MessageType.Info)]
	public bool changeCoverByDamage = true;

	[vToggleOption("Compare Damage", "Less", "GreaterEqual")]
	[SerializeField]
	protected bool greaterValue = true;

	[vToggleOption("Compare Type", "Massive Count", "Massive Value")]
	[SerializeField]
	protected bool massiveValue = true;

	[SerializeField]
	protected int valueToCompare = 100;

	[vMinMax(minLimit = 0f, maxLimit = 100f)]
	[Tooltip("Use this time to control when the valid damage checks are true after cover is changed to prevent that change cover every time")]
	[SerializeField]
	protected Vector2 timeToChangeByDamge = new Vector2(0f, 2f);

	[vEditorToolbar("Debug", false, "", false, false)]
	public bool debugMode;

	[vReadOnly(true)]
	public bool isGointToCoverPoint;

	[vButton("Get Cover Test", "GetCoverTest", typeof(vAICover), true)]
	[SerializeField]
	private Transform targetTest;

	public vAICoverPoint coverPoint;

	internal vIControlAICombat controller;

	internal Vector3 threatdir;

	internal Vector3 threatPos;

	private float _timeInCover;

	private float _timeToChangeByDamage;

	private bool inGetCover;

	public List<vAICoverPoint> _coverPoints = new List<vAICoverPoint>();

	protected vAIMovementSpeed getCoverSpeed = vAIMovementSpeed.Running;

	public Type ComponentType => typeof(vAICover);

	protected virtual void Start()
	{
		controller = GetComponent<vIControlAICombat>();
		controller.onDead.AddListener(RemoveCoverOnDead);
	}

	protected virtual void OnDrawGizmosSelected()
	{
		if (!debugMode)
		{
			return;
		}
		Gizmos.DrawWireSphere(base.transform.position, getCoverRange);
		if (_coverPoints.Count != 0)
		{
			for (int i = 0; i < _coverPoints.Count; i++)
			{
				float num = Vector3.Angle(_coverPoints[i].transform.forward, threatdir);
				vAICoverPoint vAICoverPoint2 = _coverPoints[i];
				Gizmos.color = ((num > minAngleOfThreat && !vAICoverPoint2.isOccuped) ? Color.blue : Color.red);
				Gizmos.DrawSphere(vAICoverPoint2.posePosition, 0.25f);
				Gizmos.DrawRay(vAICoverPoint2.posePosition, Vector3.up);
			}
			if ((bool)coverPoint)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawSphere(coverPoint.posePosition, 0.25f);
				Gizmos.DrawRay(coverPoint.posePosition, Vector3.up);
				Gizmos.DrawSphere(threatPos, 0.2f);
				Gizmos.DrawRay(threatPos, -threatdir.normalized);
				Gizmos.DrawRay(threatPos, Vector3.up);
				Gizmos.color = Color.red * 0.8f;
				Gizmos.DrawLine(base.transform.position, threatPos);
			}
		}
	}

	internal void OnExitCover()
	{
		if ((bool)coverPoint)
		{
			coverPoint.isOccuped = false;
		}
		coverPoint = null;
		controller.isCrouching = false;
	}

	public void GetCoverTest()
	{
		if ((bool)targetTest)
		{
			threatPos = targetTest.position;
			threatdir = threatPos - base.transform.position;
		}
		else
		{
			threatPos = base.transform.position + UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(minDistanceOfThreat, maxDistanceOfThreat);
			threatPos.y = base.transform.position.y;
			threatdir = threatPos - base.transform.position;
		}
		GetCover(forceGet: true);
	}

	public virtual void GetCoverFromRandomThreat(vAIMovementSpeed speed = vAIMovementSpeed.Running)
	{
		if (controller != null)
		{
			getCoverSpeed = speed;
			CheckController();
			if (!coverPoint)
			{
				threatPos = base.transform.position + base.transform.forward * UnityEngine.Random.Range(minDistanceOfThreat, maxDistanceOfThreat) + UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(0f, minDistanceOfThreat);
				threatPos.y = base.transform.position.y;
			}
			threatdir = threatPos - base.transform.position;
			GetCover();
		}
	}

	public virtual void GetCoverFromTargetThreat(vAIMovementSpeed speed = vAIMovementSpeed.Running)
	{
		if (controller != null)
		{
			getCoverSpeed = speed;
			if ((bool)controller.currentTarget.transform && !controller.currentTarget.isLost)
			{
				CheckController();
				threatPos = controller.currentTarget.transform.position;
				threatdir = threatPos - base.transform.position;
				GetCoverOfTarget();
			}
			else
			{
				GetCoverFromRandomThreat();
			}
		}
	}

	protected virtual void CheckController()
	{
		if (!isGointToCoverPoint && ChangeCoverByDamage() && _timeToChangeByDamage < Time.time)
		{
			_timeInCover = 0f;
			_timeToChangeByDamage = Time.time + UnityEngine.Random.Range(timeToChangeByDamge.x, timeToChangeByDamge.y);
		}
		else if (isGointToCoverPoint && ChangeCoverByDamage())
		{
			_timeToChangeByDamage = Time.time + UnityEngine.Random.Range(timeToChangeByDamge.x, timeToChangeByDamge.y);
		}
		if (controller.ragdolled)
		{
			controller.isCrouching = false;
			if ((bool)coverPoint)
			{
				coverPoint.isOccuped = false;
			}
			coverPoint = null;
		}
	}

	public virtual void UpdateCoverPoints(Collider[] coverColliders)
	{
		_coverPoints.Clear();
		for (int i = 0; i < coverColliders.Length; i++)
		{
			vAICoverPoint component = coverColliders[i].GetComponent<vAICoverPoint>();
			if ((bool)component && component.gameObject.CompareTag(coverTag))
			{
				_coverPoints.Add(component);
			}
		}
	}

	public virtual bool HasValidCoversFromPosition(Vector3 threatdir, Vector3 threatPosition)
	{
		return _coverPoints.Exists((vAICoverPoint c) => Vector3.Angle(c.transform.forward, threatdir) > minAngleOfThreat && (c.transform.position - threatPosition).magnitude > minDistanceOfThreat && c != coverPoint);
	}

	public virtual void RemoveCoverPoint()
	{
		if ((bool)coverPoint)
		{
			coverPoint.isOccuped = false;
			coverPoint = null;
		}
	}

	protected virtual void GetCover(bool forceGet = false)
	{
		if (inGetCover || ((bool)coverPoint && !forceGet && _timeInCover > Time.time))
		{
			return;
		}
		inGetCover = true;
		if (_coverPoints.Count == 0 || !HasValidCoversFromPosition(threatdir, threatPos))
		{
			Collider[] coverColliders = Physics.OverlapSphere(base.transform.position, getCoverRange, coverLayer);
			UpdateCoverPoints(coverColliders);
		}
		float num = maxDistanceOfThreat;
		float num2 = minAngleOfThreat;
		vAICoverPoint vAICoverPoint2 = null;
		for (int i = 0; i < _coverPoints.Count; i++)
		{
			vAICoverPoint vAICoverPoint3 = _coverPoints[i];
			if (!vAICoverPoint3.isOccuped && (bool)vAICoverPoint3)
			{
				float magnitude = (threatPos - vAICoverPoint3.posePosition).magnitude;
				float num3 = Vector3.Angle(vAICoverPoint3.transform.forward, threatdir);
				if (magnitude < num && num3 > num2 && vAICoverPoint3 != coverPoint)
				{
					num = magnitude;
					vAICoverPoint2 = vAICoverPoint3;
				}
			}
		}
		if (vAICoverPoint2 != coverPoint && !isGointToCoverPoint && (bool)vAICoverPoint2)
		{
			if ((bool)coverPoint)
			{
				coverPoint.isOccuped = false;
			}
			coverPoint = vAICoverPoint2;
			coverPoint.isOccuped = true;
			_coverPoints.Remove(coverPoint);
			StartCoroutine(GoToCoverPoint());
		}
		inGetCover = false;
	}

	protected virtual void GetCoverOfTarget()
	{
		if (inGetCover)
		{
			return;
		}
		if (_timeInCover > Time.time)
		{
			if ((bool)coverPoint && Vector3.Angle(coverPoint.transform.forward, threatdir) > minAngleOfThreat && controller.targetDistance > minDistanceOfThreat && !controller.currentTarget.isLost)
			{
				return;
			}
			if ((bool)controller.currentTarget.transform && controller.currentTarget.isLost)
			{
				if ((bool)coverPoint)
				{
					coverPoint.isOccuped = false;
				}
				coverPoint = null;
				return;
			}
			if (isGointToCoverPoint)
			{
				return;
			}
		}
		else if (controller.currentTarget.isLost)
		{
			_timeInCover = Time.time + UnityEngine.Random.Range(timeToChangeCover.x, timeToChangeCover.y);
			if (_coverPoints.Count > 0)
			{
				_coverPoints.Clear();
			}
			return;
		}
		inGetCover = true;
		float num = minAngleOfThreat;
		if (_coverPoints.Count == 0 || !HasValidCoversFromPosition(threatdir, threatPos))
		{
			Collider[] coverColliders = Physics.OverlapSphere(base.transform.position, getCoverRange, coverLayer);
			UpdateCoverPoints(coverColliders);
		}
		float num2 = maxDistanceOfThreat;
		vAICoverPoint vAICoverPoint2 = null;
		for (int i = 0; i < _coverPoints.Count; i++)
		{
			vAICoverPoint vAICoverPoint3 = _coverPoints[i];
			float magnitude = (threatPos - vAICoverPoint3.posePosition).magnitude;
			float num3 = Vector3.Angle(vAICoverPoint3.transform.forward, threatdir);
			if (!vAICoverPoint3.isOccuped && magnitude < num2 && magnitude > minDistanceOfThreat && num3 > num && vAICoverPoint3 != coverPoint)
			{
				num2 = magnitude;
				vAICoverPoint2 = vAICoverPoint3;
			}
		}
		if (vAICoverPoint2 != null && vAICoverPoint2 != coverPoint && !isGointToCoverPoint)
		{
			if ((bool)coverPoint)
			{
				coverPoint.isOccuped = false;
			}
			coverPoint = vAICoverPoint2;
			coverPoint.isOccuped = true;
			_coverPoints.Remove(coverPoint);
			StartCoroutine(GoToCoverPointFromTarget());
		}
		inGetCover = false;
	}

	protected virtual bool ChangeCoverByDamage()
	{
		if (!changeCoverByDamage || controller.receivedDamage == null)
		{
			return false;
		}
		int num = (massiveValue ? controller.receivedDamage.massiveValue : controller.receivedDamage.massiveCount);
		if (!greaterValue)
		{
			return num < valueToCompare;
		}
		return num >= valueToCompare;
	}

	protected virtual IEnumerator GoToCoverPointFromTarget()
	{
		if (Vector3.Distance(base.transform.position, coverPoint.posePosition) > controller.stopingDistance)
		{
			controller.isCrouching = Vector3.Distance(base.transform.position, coverPoint.posePosition) < 2f;
			isGointToCoverPoint = true;
			if (controller.isAiming || controller.isStrafing)
			{
				controller.StrafeMoveTo(coverPoint.posePosition, controller.currentTarget.transform.position - base.transform.position, getCoverSpeed);
			}
			else
			{
				controller.MoveTo(coverPoint.posePosition, getCoverSpeed);
			}
			yield return new WaitForSeconds(1f);
			while (!controller.isInDestination)
			{
				if (controller.remainingDistance < 1f + controller.stopingDistance && !controller.isCrouching)
				{
					controller.isCrouching = true;
				}
				if (coverPoint != null)
				{
					if (controller.isAiming || controller.isStrafing)
					{
						controller.StrafeMoveTo(coverPoint.posePosition, controller.currentTarget.transform.position - base.transform.position, getCoverSpeed);
					}
					else
					{
						controller.MoveTo(coverPoint.posePosition, getCoverSpeed);
					}
				}
				if (controller.targetDistance < minDistanceOfThreat || coverPoint == null)
				{
					_timeInCover = 0f;
					break;
				}
				yield return null;
			}
		}
		if ((bool)coverPoint && controller.targetDistance > minDistanceOfThreat)
		{
			controller.isCrouching = true;
		}
		_timeInCover = Time.time + UnityEngine.Random.Range(timeToChangeCover.x, timeToChangeCover.y);
		isGointToCoverPoint = false;
	}

	protected virtual IEnumerator GoToCoverPoint()
	{
		if (Vector3.Distance(base.transform.position, coverPoint.posePosition) > controller.stopingDistance)
		{
			controller.isCrouching = Vector3.Distance(base.transform.position, coverPoint.posePosition) < 2f;
			isGointToCoverPoint = true;
			controller.ForceUpdatePath(2f);
			if (controller.isAiming || controller.isStrafing)
			{
				controller.StrafeMoveTo(coverPoint.posePosition, base.transform.forward, getCoverSpeed);
			}
			else
			{
				controller.MoveTo(coverPoint.posePosition, getCoverSpeed);
			}
			controller.MoveTo(coverPoint.posePosition, getCoverSpeed);
			yield return new WaitForSeconds(1f);
			while (!controller.isInDestination)
			{
				if (controller.remainingDistance < 1f + controller.stopingDistance && !controller.isCrouching)
				{
					controller.isCrouching = true;
				}
				if (!coverPoint)
				{
					break;
				}
				yield return null;
			}
		}
		if ((bool)coverPoint)
		{
			controller.isCrouching = true;
			_timeInCover = Time.time + UnityEngine.Random.Range(timeToChangeCover.x, timeToChangeCover.y);
		}
		isGointToCoverPoint = false;
	}

	protected virtual void RemoveCoverOnDead(GameObject g)
	{
		RemoveCoverPoint();
	}
}
