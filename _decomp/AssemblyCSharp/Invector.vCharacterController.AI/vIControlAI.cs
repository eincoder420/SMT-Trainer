using System.Collections.Generic;
using Invector.vEventSystems;
using UnityEngine;

namespace Invector.vCharacterController.AI;

public interface vIControlAI : vIHealthController, vIDamageReceiver
{
	GameObject customTarget { get; }

	bool _moveToTarget { get; }

	Vector3 selfStartPosition { get; set; }

	Vector3 targetDestination { get; }

	Collider selfCollider { get; }

	Animator animator { get; }

	vAnimatorStateInfos animatorStateInfos { get; }

	vWaypointArea waypointArea { get; set; }

	vAIReceivedDamegeInfo receivedDamage { get; }

	vWaypoint targetWaypoint { get; }

	List<vWaypoint> visitedWaypoints { get; set; }

	Vector3 lastTargetPosition { get; }

	bool ragdolled { get; }

	bool isInDestination { get; }

	bool isMoving { get; }

	bool isStrafing { get; }

	bool isRolling { get; }

	bool isCrouching { get; set; }

	bool targetInLineOfSight { get; }

	vAISightMethod SightMethod { get; set; }

	vAIUpdateQuality UpdatePathQuality { get; set; }

	vAIUpdateQuality FindTargetUpdateQuality { get; set; }

	vAIUpdateQuality CanseeTargetUpdateQuality { get; set; }

	vAIMovementSpeed movementSpeed { get; }

	float targetDistance { get; }

	float changeWaypointDistance { get; }

	Vector3 desiredVelocity { get; }

	float remainingDistance { get; }

	float stopingDistance { get; set; }

	float minDistanceToDetect { get; set; }

	float maxDistanceToDetect { get; set; }

	float fieldOfView { get; set; }

	bool selfStartingPoint { get; }

	bool customStartPoint { get; }

	Vector3 customStartPosition { get; }

	vAITarget currentTarget { get; }

	void MoveToTargetExample(vAIMovementSpeed speed = vAIMovementSpeed.Running);

	void CreatePrimaryComponents();

	void CreateSecondaryComponents();

	bool HasComponent<T>() where T : vIAIComponent;

	T GetAIComponent<T>() where T : vIAIComponent;

	void SetDetectionLayer(LayerMask mask);

	void SetDetectionTags(List<string> tags);

	void SetObstaclesLayer(LayerMask mask);

	void SetLineOfSight(float fov = -1f, float minDistToDetect = -1f, float maxDistToDetect = -1f, float lostTargetDistance = -1f);

	void NextWayPoint();

	void MoveTo(Vector3 destination, vAIMovementSpeed speed = vAIMovementSpeed.Walking);

	void StrafeMoveTo(Vector3 destination, Vector3 forwardDirection, vAIMovementSpeed speed = vAIMovementSpeed.Walking);

	void StrafeMoveTo(Vector3 destination, vAIMovementSpeed speed = vAIMovementSpeed.Walking);

	void RotateTo(Vector3 direction);

	void RollTo(Vector3 direction);

	void SetCurrentTarget(Transform target);

	void SetCurrentTarget(Transform target, bool overrideCanseeTarget);

	void RemoveCurrentTarget();

	Collider[] GetTargetsInRange();

	void FindTarget();

	void FindTarget(bool checkForObstacles);

	bool TryGetTarget(out vAITarget target);

	bool TryGetTarget(string tag, out vAITarget target);

	bool TryGetTarget(List<string> m_detectTags, out vAITarget target);

	void FindSpecificTarget(List<string> m_detectTags, LayerMask m_detectLayer, bool checkForObstables = true);

	void LookAround();

	void LookTo(Vector3 point, float stayLookTime = 1f, float offsetLookHeight = -1f);

	void LookToTarget(Transform target, float stayLookTime = 1f, float offsetLookHeight = -1f);

	void Stop();

	void ForceUpdatePath(float timeInUpdate = 1f);

	bool IsInTriggerWithTag(string tag);

	bool IsInTriggerWithName(string name);

	bool IsInTriggerWithTag(string tag, out Collider result);

	bool IsInTriggerWithName(string name, out Collider result);
}
