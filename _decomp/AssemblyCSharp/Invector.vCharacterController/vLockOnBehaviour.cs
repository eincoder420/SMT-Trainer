using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invector.vCamera;
using UnityEngine;

namespace Invector.vCharacterController;

[vClassHeader("Lock-On", true, "icon_v2", false, "")]
public abstract class vLockOnBehaviour : vMonoBehaviour
{
	private Transform watcher;

	[Tooltip("Tags of objects that can be found")]
	public string[] tagsToFind = new string[1] { "Enemy" };

	[Tooltip("Layer of Obscatacles to prevent the find for targets")]
	public LayerMask layerOfObstacles = 1;

	[Range(0f, 1f)]
	[Tooltip("Use this to set a margin of Aim point ")]
	public float screenMarginX = 0.8f;

	[Range(0f, 1f)]
	[Tooltip("Use this to set a margin of Aim point ")]
	public float screenMarginY = 0.1f;

	[Tooltip("Range of the search for targets")]
	public float range = 10f;

	[Tooltip("Show the Gizmos and helpers")]
	public bool showDebug;

	public float timeToChangeTarget = 0.25f;

	private int index;

	private List<Transform> visibles;

	protected Transform target;

	protected vThirdPersonCamera tpCamera;

	private Rect rect;

	private bool _inLockOn;

	protected bool changingTarget;

	public virtual Transform currentTarget => target;

	public virtual List<Transform> allTargets
	{
		get
		{
			if (visibles != null && visibles.Count > 0)
			{
				return visibles;
			}
			return null;
		}
	}

	public virtual void ChangeTarget(int value)
	{
		StartCoroutine(ChangeTargetRoutine(value));
	}

	public virtual bool isCharacterAlive()
	{
		if (currentTarget == null)
		{
			return false;
		}
		vIHealthController component = currentTarget.GetComponent<vIHealthController>();
		if (component == null)
		{
			return false;
		}
		if (component.currentHealth > 0f)
		{
			return true;
		}
		return false;
	}

	public virtual bool isCharacterAlive(Transform other)
	{
		vIHealthController component = other.GetComponent<vIHealthController>();
		if (component == null)
		{
			return false;
		}
		if (component.currentHealth > 0f)
		{
			return true;
		}
		return false;
	}

	public virtual void ResetLockOn()
	{
		target = null;
		_inLockOn = false;
	}

	protected virtual void UpdateLockOn(bool value)
	{
		if (value && value != _inLockOn)
		{
			_inLockOn = value;
			visibles = GetPossibleTargets();
			index = 0;
			if (visibles != null && visibles.Count > 0)
			{
				target = visibles[index];
			}
		}
		else if (!value && value != _inLockOn)
		{
			_inLockOn = value;
			index = 0;
			target = null;
			if (visibles != null)
			{
				visibles.Clear();
			}
		}
	}

	protected virtual IEnumerator ChangeTargetRoutine(int value)
	{
		if (changingTarget)
		{
			yield break;
		}
		changingTarget = true;
		visibles = GetPossibleTargets();
		if (_inLockOn && visibles != null && visibles.Count > 1)
		{
			if (index + value > visibles.Count - 1)
			{
				index = 0;
			}
			else if (index + value < 0)
			{
				index = visibles.Count - 1;
			}
			else
			{
				index += value;
			}
			target = visibles[index];
			SetTarget();
		}
		yield return new WaitForSeconds(timeToChangeTarget);
		changingTarget = false;
	}

	protected virtual void SetTarget()
	{
	}

	protected void OnGUI()
	{
		if (showDebug)
		{
			float num = (float)Screen.width - (float)Screen.width * screenMarginX;
			float num2 = (float)Screen.height - (float)Screen.height * screenMarginY;
			float x = (float)Screen.width * 0.5f - num * 0.5f;
			float y = (float)Screen.height * 0.5f - num2 * 0.5f;
			rect = new Rect(x, y, num, num2);
			GUI.Box(rect, "");
		}
	}

	protected void Init()
	{
		if (Camera.main == null)
		{
			base.enabled = false;
		}
		tpCamera = Camera.main.transform.root.GetComponent<vThirdPersonCamera>();
		float num = (float)Screen.width - (float)Screen.width * screenMarginX;
		float num2 = (float)Screen.height - (float)Screen.height * screenMarginY;
		float x = (float)Screen.width * 0.5f - num * 0.5f;
		float y = (float)Screen.height * 0.5f - num2 * 0.5f;
		rect = new Rect(x, y, num, num2);
	}

	protected void OnDrawGizmos()
	{
		if (!showDebug || !watcher)
		{
			return;
		}
		Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
		Gizmos.DrawSphere(watcher.position, range + 0.1f);
		if (visibles != null && visibles.Count > 0 && target != null)
		{
			visibles.ForEach(delegate(Transform _transform)
			{
				Gizmos.color = (_transform.Equals(currentTarget) ? Color.red : Color.yellow);
				Gizmos.DrawSphere(_transform.GetComponent<Collider>().bounds.center, 0.5f);
			});
		}
	}

	protected List<Transform> GetPossibleTargets()
	{
		if (tpCamera != null && tpCamera.mainTarget != null)
		{
			watcher = tpCamera.mainTarget;
		}
		else
		{
			watcher = base.transform;
		}
		List<Transform> list = new List<Transform>();
		RaycastHit[] array = Physics.SphereCastAll(watcher.position, range, watcher.forward, 0.01f);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			if (!tagsToFind.Contains(raycastHit.transform.tag) || !isCharacterAlive(raycastHit.transform.GetComponent<Transform>()))
			{
				continue;
			}
			Vector3[] array2 = BoundPoints(raycastHit.collider);
			foreach (Vector3 end in array2)
			{
				if (Physics.Linecast(base.transform.position, end, out var hitInfo, layerOfObstacles))
				{
					if (hitInfo.transform == raycastHit.transform)
					{
						list.Add(raycastHit.transform);
						if (showDebug)
						{
							Debug.DrawLine(base.transform.position, end, Color.green, 2f);
						}
						break;
					}
					if (showDebug)
					{
						Debug.DrawLine(base.transform.position, end, Color.red, 2f);
					}
					continue;
				}
				list.Add(raycastHit.transform);
				if (showDebug)
				{
					Debug.DrawLine(base.transform.position, end, Color.green, 2f);
				}
				break;
			}
		}
		SortTargets(ref list);
		return list;
	}

	protected void SortTargets(ref List<Transform> list)
	{
		if (tpCamera == null || (bool)tpCamera.targetCamera)
		{
			Init();
		}
		List<Transform> list2 = new List<Transform>();
		List<Transform> list3 = new List<Transform>();
		List<Transform> list4 = new List<Transform>();
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(tpCamera.targetCamera);
		for (int i = 0; i < list.Count; i++)
		{
			Transform transform = list[i];
			Vector2 point = tpCamera.targetCamera.WorldToScreenPoint(transform.transform.position);
			if (GeometryUtility.TestPlanesAABB(planes, transform.GetComponent<Collider>().bounds) && rect.Contains(point))
			{
				list2.Add(transform);
			}
			else if (GeometryUtility.TestPlanesAABB(planes, transform.GetComponent<Collider>().bounds))
			{
				list3.Add(transform);
			}
			else
			{
				list4.Add(transform);
			}
		}
		list2.Sort(delegate(Transform t1, Transform t2)
		{
			Vector2 a3 = tpCamera.targetCamera.WorldToScreenPoint(t1.transform.position);
			Vector2 a4 = tpCamera.targetCamera.WorldToScreenPoint(t2.transform.position);
			return Vector2.Distance(a3, rect.center).CompareTo(Vector2.Distance(a4, rect.center));
		});
		list3.Sort(delegate(Transform t1, Transform t2)
		{
			Vector2 a = tpCamera.targetCamera.WorldToScreenPoint(t1.transform.position);
			Vector2 a2 = tpCamera.targetCamera.WorldToScreenPoint(t2.transform.position);
			return Vector2.Distance(a, rect.center).CompareTo(Vector2.Distance(a2, rect.center));
		});
		list4.Sort((Transform t1, Transform t2) => Vector3.Distance(t1.transform.position, base.transform.position).CompareTo(Vector3.Distance(t2.transform.position, base.transform.position)));
		list = list2.Union(list3).Union(list4).ToList();
	}

	protected Vector3[] BoundPoints(Collider collider)
	{
		Vector3 min = collider.bounds.min;
		Vector3 max = collider.bounds.max;
		Vector3 vector = new Vector3(min.x, min.y, max.z);
		Vector3 vector2 = new Vector3(min.x, max.y, min.z);
		Vector3 vector3 = new Vector3(max.x, min.y, min.z);
		Vector3 vector4 = new Vector3(min.x, max.y, max.z);
		Vector3 vector5 = new Vector3(max.x, min.y, max.z);
		Vector3 vector6 = new Vector3(max.x, max.y, min.z);
		Color white = Color.white;
		if (showDebug)
		{
			Debug.DrawLine(vector4, max, white, 1f);
			Debug.DrawLine(max, vector6, white, 1f);
			Debug.DrawLine(vector6, vector2, white, 1f);
			Debug.DrawLine(vector2, vector4, white, 1f);
			Debug.DrawLine(vector, vector5, white, 1f);
			Debug.DrawLine(vector5, vector3, white, 1f);
			Debug.DrawLine(vector3, min, white, 1f);
			Debug.DrawLine(min, vector, white, 1f);
			Debug.DrawLine(vector4, vector, white, 1f);
			Debug.DrawLine(max, vector5, white, 1f);
			Debug.DrawLine(vector6, vector3, white, 1f);
			Debug.DrawLine(vector2, min, white, 1f);
		}
		return new Vector3[8] { min, max, vector, vector2, vector3, vector4, vector5, vector6 };
	}
}
