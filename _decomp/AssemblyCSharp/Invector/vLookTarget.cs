using System;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;

namespace Invector;

public class vLookTarget : MonoBehaviour
{
	[Serializable]
	public class OnLookEvent : UnityEvent<vHeadTrack>
	{
	}

	public enum VisibleCheckType
	{
		None,
		SingleCast,
		BoxCast
	}

	public bool ignoreHeadTrackAngle;

	[Header("Set this to assign a different point to look")]
	public Transform lookPointTarget;

	[Header("Area to check if is visible")]
	public Vector3 centerArea = Vector3.zero;

	public Vector3 sizeArea = Vector3.one;

	public bool useLimitToDetect = true;

	public float minDistanceToDetect = 2f;

	public VisibleCheckType visibleCheckType;

	[Tooltip("use this to turn the object undetectable")]
	public bool HideObject;

	public OnLookEvent onEnterLook;

	public OnLookEvent onExitLook;

	public Vector3 lookPoint
	{
		get
		{
			if ((bool)lookPointTarget)
			{
				return lookPointTarget.position;
			}
			return base.transform.TransformPoint(centerArea);
		}
	}

	private void OnDrawGizmosSelected()
	{
		DrawBox();
	}

	private void Start()
	{
		int layer = LayerMask.NameToLayer("HeadTrack");
		base.gameObject.layer = layer;
	}

	private void DrawBox()
	{
		Gizmos.color = new Color(1f, 0f, 0f, 1f);
		Gizmos.DrawSphere(lookPoint, 0.05f);
		if (visibleCheckType == VisibleCheckType.BoxCast)
		{
			float x = base.transform.lossyScale.x * sizeArea.x;
			float y = base.transform.lossyScale.y * sizeArea.y;
			float z = base.transform.lossyScale.z * sizeArea.z;
			float x2 = base.transform.lossyScale.x * centerArea.x;
			float y2 = base.transform.lossyScale.y * centerArea.y;
			float z2 = base.transform.lossyScale.z * centerArea.z;
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position + new Vector3(x2, y2, z2), base.transform.rotation, new Vector3(x, y, z) * 2f);
			Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
			Gizmos.color = new Color(0f, 1f, 0f, 1f);
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
		else if (visibleCheckType == VisibleCheckType.SingleCast)
		{
			Vector3 center = base.transform.TransformPoint(centerArea);
			Gizmos.color = new Color(0f, 1f, 0f, 1f);
			Gizmos.DrawSphere(center, 0.05f);
		}
	}

	internal void EnterLook(vHeadTrack vHeadTrack)
	{
		onEnterLook.Invoke(vHeadTrack);
	}

	internal void ExitLook(vHeadTrack vHeadTrack)
	{
		onExitLook.Invoke(vHeadTrack);
	}
}
