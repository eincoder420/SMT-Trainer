using System;
using UnityEngine;

namespace Invector.vCamera;

public static class vThirdPersonCameraExtensions
{
	public static void Slerp(this vThirdPersonCameraState to, vThirdPersonCameraState from, float time)
	{
		to.Name = from.Name;
		to.forward = Mathf.Lerp(to.forward, from.forward, time);
		to.right = Mathf.Lerp(to.right, from.right, time);
		to.defaultDistance = Mathf.Lerp(to.defaultDistance, from.defaultDistance, time);
		to.maxDistance = Mathf.Lerp(to.maxDistance, from.maxDistance, time);
		to.minDistance = Mathf.Lerp(to.minDistance, from.minDistance, time);
		to.height = Mathf.Lerp(to.height, from.height, time);
		to.fixedAngle = Vector2.Lerp(to.fixedAngle, from.fixedAngle, time);
		to.smooth = Mathf.Lerp(to.smooth, from.smooth, time);
		to.xMouseSensitivity = Mathf.Lerp(to.xMouseSensitivity, from.xMouseSensitivity, time);
		to.yMouseSensitivity = Mathf.Lerp(to.yMouseSensitivity, from.yMouseSensitivity, time);
		to.yMinLimit = Mathf.Lerp(to.yMinLimit, from.yMinLimit, time);
		to.yMaxLimit = Mathf.Lerp(to.yMaxLimit, from.yMaxLimit, time);
		to.xMinLimit = Mathf.Lerp(to.xMinLimit, from.xMinLimit, time);
		to.xMaxLimit = Mathf.Lerp(to.xMaxLimit, from.xMaxLimit, time);
		to.rotationOffSet = Vector3.Lerp(to.rotationOffSet, from.rotationOffSet, time);
		to.cullingHeight = Mathf.Lerp(to.cullingHeight, from.cullingHeight, time);
		to.cullingMinDist = Mathf.Lerp(to.cullingMinDist, from.cullingMinDist, time);
		to.cameraMode = from.cameraMode;
		to.useZoom = from.useZoom;
		to.lookPoints = from.lookPoints;
		to.fov = Mathf.Lerp(to.fov, from.fov, time);
		if (to.fov <= 0f)
		{
			to.fov = 1f;
		}
	}

	public static void CopyState(this vThirdPersonCameraState to, vThirdPersonCameraState from)
	{
		to.Name = from.Name;
		to.forward = from.forward;
		to.right = from.right;
		to.defaultDistance = from.defaultDistance;
		to.maxDistance = from.maxDistance;
		to.minDistance = from.minDistance;
		to.height = from.height;
		to.fixedAngle = from.fixedAngle;
		to.lookPoints = from.lookPoints;
		to.smooth = from.smooth;
		to.xMouseSensitivity = from.xMouseSensitivity;
		to.yMouseSensitivity = from.yMouseSensitivity;
		to.yMinLimit = from.yMinLimit;
		to.yMaxLimit = from.yMaxLimit;
		to.xMinLimit = from.xMinLimit;
		to.xMaxLimit = from.xMaxLimit;
		to.rotationOffSet = from.rotationOffSet;
		to.cullingHeight = from.cullingHeight;
		to.cullingMinDist = from.cullingMinDist;
		to.cameraMode = from.cameraMode;
		to.useZoom = from.useZoom;
		to.fov = from.fov;
		if (to.fov <= 0f)
		{
			to.fov = 1f;
		}
	}

	public static ClipPlanePoints NearClipPlanePoints(this Camera camera, Vector3 pos, float clipPlaneMargin)
	{
		ClipPlanePoints result = default(ClipPlanePoints);
		Transform transform = camera.transform;
		float f = camera.fieldOfView / 2f * ((float)Math.PI / 180f);
		float aspect = camera.aspect;
		float nearClipPlane = camera.nearClipPlane;
		float num = nearClipPlane * Mathf.Tan(f);
		float num2 = num * aspect;
		num *= 1f + clipPlaneMargin;
		num2 *= 1f + clipPlaneMargin;
		result.LowerRight = pos + transform.right * num2;
		result.LowerRight -= transform.up * num;
		result.LowerRight += transform.forward * nearClipPlane;
		result.LowerLeft = pos - transform.right * num2;
		result.LowerLeft -= transform.up * num;
		result.LowerLeft += transform.forward * nearClipPlane;
		result.UpperRight = pos + transform.right * num2;
		result.UpperRight += transform.up * num;
		result.UpperRight += transform.forward * nearClipPlane;
		result.UpperLeft = pos - transform.right * num2;
		result.UpperLeft += transform.up * num;
		result.UpperLeft += transform.forward * nearClipPlane;
		return result;
	}
}
