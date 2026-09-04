using UnityEngine;

namespace Invector;

public static class vLookTargetHelper
{
	private struct LookPoints
	{
		public Vector3 frontTopLeft;

		public Vector3 frontTopRight;

		public Vector3 frontBottomLeft;

		public Vector3 frontBottomRight;

		public Vector3 backTopLeft;

		public Vector3 backTopRight;

		public Vector3 backBottomLeft;

		public Vector3 backBottomRight;
	}

	private static LookPoints GetLookPoints(vLookTarget lookTarget)
	{
		LookPoints result = default(LookPoints);
		Vector3 centerArea = lookTarget.centerArea;
		Vector3 sizeArea = lookTarget.sizeArea;
		Transform transform = lookTarget.transform;
		result.frontTopLeft = new Vector3(centerArea.x - sizeArea.x, centerArea.y + sizeArea.y, centerArea.z - sizeArea.z);
		result.frontTopRight = new Vector3(centerArea.x + sizeArea.x, centerArea.y + sizeArea.y, centerArea.z - sizeArea.z);
		result.frontBottomLeft = new Vector3(centerArea.x - sizeArea.x, centerArea.y - sizeArea.y, centerArea.z - sizeArea.z);
		result.frontBottomRight = new Vector3(centerArea.x + sizeArea.x, centerArea.y - sizeArea.y, centerArea.z - sizeArea.z);
		result.backTopLeft = new Vector3(centerArea.x - sizeArea.x, centerArea.y + sizeArea.y, centerArea.z + sizeArea.z);
		result.backTopRight = new Vector3(centerArea.x + sizeArea.x, centerArea.y + sizeArea.y, centerArea.z + sizeArea.z);
		result.backBottomLeft = new Vector3(centerArea.x - sizeArea.x, centerArea.y - sizeArea.y, centerArea.z + sizeArea.z);
		result.backBottomRight = new Vector3(centerArea.x + sizeArea.x, centerArea.y - sizeArea.y, centerArea.z + sizeArea.z);
		result.frontTopLeft = transform.TransformPoint(result.frontTopLeft);
		result.frontTopRight = transform.TransformPoint(result.frontTopRight);
		result.frontBottomLeft = transform.TransformPoint(result.frontBottomLeft);
		result.frontBottomRight = transform.TransformPoint(result.frontBottomRight);
		result.backTopLeft = transform.TransformPoint(result.backTopLeft);
		result.backTopRight = transform.TransformPoint(result.backTopRight);
		result.backBottomLeft = transform.TransformPoint(result.backBottomLeft);
		result.backBottomRight = transform.TransformPoint(result.backBottomRight);
		return result;
	}

	public static bool IsVisible(this vLookTarget lookTarget, Vector3 from, LayerMask layerMask, bool debug = false)
	{
		if (lookTarget.HideObject)
		{
			return false;
		}
		if (lookTarget.visibleCheckType == vLookTarget.VisibleCheckType.None)
		{
			if (lookTarget.useLimitToDetect && Vector3.Distance(from, lookTarget.transform.position) > lookTarget.minDistanceToDetect)
			{
				return false;
			}
			return true;
		}
		if (lookTarget.visibleCheckType == vLookTarget.VisibleCheckType.SingleCast)
		{
			if (lookTarget.useLimitToDetect && Vector3.Distance(from, lookTarget.centerArea) > lookTarget.minDistanceToDetect)
			{
				return false;
			}
			if (CastPoint(from, lookTarget.transform.TransformPoint(lookTarget.centerArea), lookTarget.transform, layerMask, debug))
			{
				return true;
			}
			return false;
		}
		if (lookTarget.visibleCheckType == vLookTarget.VisibleCheckType.BoxCast)
		{
			if (lookTarget.useLimitToDetect && Vector3.Distance(from, lookTarget.transform.position) > lookTarget.minDistanceToDetect)
			{
				return false;
			}
			LookPoints lookPoints = GetLookPoints(lookTarget);
			if (CastPoint(from, lookPoints.frontTopLeft, lookTarget.transform, layerMask, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.frontTopRight, lookTarget.transform, layerMask, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.frontBottomLeft, lookTarget.transform, layerMask, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.frontBottomRight, lookTarget.transform, layerMask, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.backTopLeft, lookTarget.transform, layerMask, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.backTopRight, lookTarget.transform, layerMask, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.backBottomLeft, lookTarget.transform, layerMask, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.backBottomRight, lookTarget.transform, layerMask, debug))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsVisible(this vLookTarget lookTarget, Vector3 from, bool debug = false)
	{
		if (lookTarget.HideObject)
		{
			return false;
		}
		LookPoints lookPoints = GetLookPoints(lookTarget);
		if (lookTarget.visibleCheckType == vLookTarget.VisibleCheckType.None)
		{
			return true;
		}
		if (lookTarget.visibleCheckType == vLookTarget.VisibleCheckType.SingleCast)
		{
			if (CastPoint(from, lookTarget.transform.TransformPoint(lookTarget.centerArea), lookTarget.transform, debug))
			{
				return true;
			}
			return false;
		}
		if (lookTarget.visibleCheckType == vLookTarget.VisibleCheckType.BoxCast)
		{
			if (CastPoint(from, lookPoints.frontTopLeft, lookTarget.transform, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.frontTopRight, lookTarget.transform, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.frontBottomLeft, lookTarget.transform, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.frontBottomRight, lookTarget.transform, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.backTopLeft, lookTarget.transform, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.backTopRight, lookTarget.transform, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.backBottomLeft, lookTarget.transform, debug))
			{
				return true;
			}
			if (CastPoint(from, lookPoints.backBottomRight, lookTarget.transform, debug))
			{
				return true;
			}
		}
		return false;
	}

	private static bool CastPoint(Vector3 from, Vector3 point, Transform lookTarget, LayerMask layerMask, bool debug = false)
	{
		if (Physics.Linecast(from, point, out var hitInfo, layerMask))
		{
			if (hitInfo.transform != lookTarget.transform)
			{
				if (debug)
				{
					Debug.DrawLine(from, hitInfo.point, Color.red);
				}
				return false;
			}
			if (debug)
			{
				Debug.DrawLine(from, hitInfo.point, Color.green);
			}
			return true;
		}
		if (debug)
		{
			Debug.DrawLine(from, point, Color.green);
		}
		return true;
	}

	private static bool CastPoint(Vector3 from, Vector3 point, Transform lookTarget, bool debug = false)
	{
		if (Physics.Linecast(from, point, out var hitInfo))
		{
			if (hitInfo.transform != lookTarget.transform)
			{
				if (debug)
				{
					Debug.DrawLine(from, hitInfo.point, Color.red);
				}
				return false;
			}
			if (debug)
			{
				Debug.DrawLine(from, hitInfo.point, Color.green);
			}
			return true;
		}
		if (debug)
		{
			Debug.DrawLine(from, point, Color.green);
		}
		return true;
	}
}
