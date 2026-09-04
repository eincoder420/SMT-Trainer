using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Invector.vCharacterController.AI;

[SelectionBase]
[RequireComponent(typeof(BoxCollider))]
[vClassHeader("AI Cover Point", true, "icon_v2", false, "", openClose = false)]
public class vAICoverPoint : vMonoBehaviour
{
	[Serializable]
	public class CoverEvent : UnityEvent<vAICoverPoint, GameObject>
	{
	}

	[Flags]
	public enum Corner
	{
		Left = 1,
		Right = 2
	}

	[Tooltip("Auto assign the properties : Corner enum, LeftCorner and RightCorner ")]
	public bool autoDetectCorner = true;

	public LayerMask mask = 1;

	[vEnumFlag]
	public Corner corner;

	public float offsetPosePositionX;

	public float rayCastNeighborOffsetX = 0.15f;

	public vAICoverPoint leftCorner;

	public vAICoverPoint rightCorner;

	public vAICoverPoint left;

	public vAICoverPoint right;

	public CoverEvent onEnterCover;

	public bool showRays;

	private RaycastHit hit;

	private PhysicsScene physics;

	public bool isValid;

	protected BoxCollider _boxCollider;

	public bool isOccuped;

	private float space => boxCollider.size.z;

	public float posePositionZ
	{
		get
		{
			if (!boxCollider)
			{
				return 0f;
			}
			return boxCollider.size.z;
		}
	}

	public BoxCollider boxCollider
	{
		get
		{
			if (_boxCollider == null)
			{
				_boxCollider = GetComponent<BoxCollider>();
			}
			if (_boxCollider == null)
			{
				_boxCollider = base.gameObject.AddComponent<BoxCollider>();
			}
			return _boxCollider;
		}
		set
		{
			_boxCollider = value;
		}
	}

	public Vector3 posePosition => base.transform.position + base.transform.forward * posePositionZ + base.transform.right * offsetPosePositionX;

	public Vector3 rightCornerP
	{
		get
		{
			Vector3[] array = new Vector3[2];
			Vector3 vector = posePosition;
			array[0] = vector - base.transform.right * (boxCollider.size.z * 1.5f);
			array[1] = (rightCorner ? (rightCorner.posePosition + rightCorner.transform.right * (rightCorner.boxCollider.size.z * 1.5f)) : array[0]);
			return (array[0] + array[1]) / 2f;
		}
	}

	public Vector3 leftCornerP
	{
		get
		{
			Vector3[] array = new Vector3[2];
			Vector3 vector = posePosition;
			array[0] = vector + base.transform.right * (boxCollider.size.z * 1.5f);
			array[1] = (leftCorner ? (leftCorner.posePosition - leftCorner.transform.right * (leftCorner.boxCollider.size.z * 1.5f)) : array[0]);
			return (array[0] + array[1]) / 2f;
		}
	}

	private void Awake()
	{
		if ((bool)boxCollider)
		{
			boxCollider.isTrigger = true;
		}
	}

	private IEnumerator Start()
	{
		base.gameObject.SetActive(isValid = CheckPosePositionInNavMesh());
		yield return new WaitForEndOfFrame();
		CheckConnections();
	}

	public virtual bool CheckPosePositionInNavMesh()
	{
		NavMeshHit navMeshHit;
		return NavMesh.SamplePosition(posePosition, out navMeshHit, 0.1f, -1);
	}

	public void CheckConnections()
	{
		if (autoDetectCorner)
		{
			corner = (Corner)0;
		}
		mask = 1 | (1 << base.gameObject.layer);
		_ = base.gameObject.scene;
		physics = base.gameObject.scene.GetPhysicsScene();
		CheckLeftConnection();
		CheckRightConnection();
	}

	private void CheckRightConnection()
	{
		bool flag = false;
		vAICoverPoint vAICoverPoint2 = null;
		if (RayCastConnections(base.transform.position + base.transform.TransformDirection(boxCollider.center) + base.transform.right * boxCollider.size.x * rayCastNeighborOffsetX, -base.transform.right, out hit, boxCollider.size.x * (1.1f + rayCastNeighborOffsetX), mask))
		{
			vAICoverPoint2 = hit.transform.gameObject.GetComponent<vAICoverPoint>();
		}
		else
		{
			for (int i = 0; i < 6; i++)
			{
				Vector3 vector = Quaternion.AngleAxis(i * 15, Vector3.up) * -base.transform.right;
				Vector3 vector2 = boxCollider.bounds.center - base.transform.right * boxCollider.size.x * 0.45f;
				if (RayCastConnections(vector2, vector, out hit, space, mask))
				{
					vAICoverPoint2 = hit.transform.gameObject.GetComponent<vAICoverPoint>();
					if (showRays)
					{
						Debug.DrawLine(vector2, hit.point, Color.red);
					}
					if ((bool)vAICoverPoint2)
					{
						break;
					}
				}
				else if (showRays)
				{
					Debug.DrawRay(vector2, vector * space, Color.green);
				}
			}
			if (!vAICoverPoint2)
			{
				for (int j = 0; j < 6; j++)
				{
					Vector3 vector3 = Quaternion.AngleAxis(-(j * 15), Vector3.up) * -base.transform.right;
					Vector3 vector4 = boxCollider.bounds.center - base.transform.right * boxCollider.size.x * 0.45f;
					if (RayCastConnections(vector4, vector3, out hit, space, mask))
					{
						vAICoverPoint2 = hit.transform.gameObject.GetComponent<vAICoverPoint>();
						if (showRays)
						{
							Debug.DrawLine(vector4, hit.point, Color.red);
						}
						if ((bool)vAICoverPoint2)
						{
							break;
						}
					}
					else if (showRays)
					{
						Debug.DrawRay(vector4, vector3 * space, Color.yellow);
					}
				}
			}
		}
		if ((bool)vAICoverPoint2)
		{
			right = (((vAICoverPoint2.transform.eulerAngles - base.transform.eulerAngles).NormalizeAngle().y > -75f) ? vAICoverPoint2 : null);
			flag = right == null;
			rightCorner = ((right == null) ? vAICoverPoint2 : null);
		}
		else
		{
			right = null;
			rightCorner = null;
			flag = true;
		}
		if (flag && autoDetectCorner)
		{
			corner |= Corner.Right;
		}
	}

	private void CheckLeftConnection()
	{
		bool flag = false;
		vAICoverPoint vAICoverPoint2 = null;
		if (RayCastConnections(base.transform.position + base.transform.TransformDirection(boxCollider.center) - base.transform.right * boxCollider.size.x * rayCastNeighborOffsetX, base.transform.right, out hit, boxCollider.size.x * (1.1f + rayCastNeighborOffsetX), mask))
		{
			vAICoverPoint2 = hit.transform.gameObject.GetComponent<vAICoverPoint>();
		}
		else
		{
			for (int i = 0; i < 6; i++)
			{
				Vector3 vector = Quaternion.AngleAxis(-(i * 15), Vector3.up) * base.transform.right;
				Vector3 vector2 = boxCollider.bounds.center + base.transform.right * boxCollider.size.x * 0.45f;
				if (RayCastConnections(vector2, vector, out hit, space, mask))
				{
					vAICoverPoint2 = hit.transform.gameObject.GetComponent<vAICoverPoint>();
					if (showRays)
					{
						Debug.DrawLine(vector2, hit.point, Color.red);
					}
					if ((bool)vAICoverPoint2)
					{
						break;
					}
				}
				else if (showRays)
				{
					Debug.DrawRay(vector2, vector * space, Color.green);
				}
			}
			if (!vAICoverPoint2)
			{
				for (int j = 0; j < 6; j++)
				{
					Vector3 vector3 = Quaternion.AngleAxis(j * 15, Vector3.up) * base.transform.right;
					Vector3 vector4 = boxCollider.bounds.center + base.transform.right * boxCollider.size.x * 0.45f;
					if (RayCastConnections(vector4, vector3, out hit, space, mask))
					{
						vAICoverPoint2 = hit.transform.gameObject.GetComponent<vAICoverPoint>();
						if (showRays)
						{
							Debug.DrawLine(vector4, hit.point, Color.red);
						}
						if ((bool)vAICoverPoint2)
						{
							break;
						}
					}
					else if (showRays)
					{
						Debug.DrawRay(vector4, vector3 * space, Color.yellow);
					}
				}
			}
		}
		if ((bool)vAICoverPoint2)
		{
			left = (((vAICoverPoint2.transform.eulerAngles - base.transform.eulerAngles).NormalizeAngle().y < 75f) ? vAICoverPoint2 : null);
			flag = left == null;
			leftCorner = ((left == null) ? vAICoverPoint2 : null);
		}
		else
		{
			left = null;
			leftCorner = null;
			flag = true;
		}
		if (flag && autoDetectCorner)
		{
			corner |= Corner.Left;
		}
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			isValid = true;
		}
		CheckConnections();
		Matrix4x4 matrix = Gizmos.matrix;
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position + base.transform.TransformDirection(boxCollider.center) + base.transform.forward * posePositionZ * 0.45f, base.transform.rotation, new Vector3(boxCollider.BoxSize().x, boxCollider.BoxSize().y, 0.01f));
		Gizmos.color = (isValid ? (Color.white * 0.8f) : (Color.red * 0.8f));
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(1f, 1f, 0f));
		Gizmos.color = (isValid ? (Color.white * 0.25f) : (Color.red * 0.25f));
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		Gizmos.matrix = matrix;
	}

	private void OnDrawGizmosSelected()
	{
	}

	protected virtual bool RayCastConnections(Vector3 rayOrigin, Vector3 dir, out RaycastHit hit, float distance, LayerMask mask)
	{
		if (Application.isPlaying)
		{
			return Physics.Raycast(rayOrigin, dir, out hit, distance, mask, QueryTriggerInteraction.Collide);
		}
		return physics.Raycast(rayOrigin, dir, out hit, distance, mask, QueryTriggerInteraction.Collide);
	}

	public void EnterCover(GameObject visitor)
	{
		onEnterCover.Invoke(this, visitor);
	}
}
