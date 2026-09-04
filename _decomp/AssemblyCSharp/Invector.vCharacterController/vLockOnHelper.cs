using UnityEngine;

namespace Invector.vCharacterController;

public static class vLockOnHelper
{
	public static Vector2 GetScreenPointOffBoundsCenter(this Transform target, Canvas canvas, Camera cam, float _heightOffset)
	{
		Bounds bounds = target.GetComponent<Collider>().bounds;
		Vector3 center = bounds.center;
		float num = Vector3.Distance(bounds.min, bounds.max);
		Vector3 position = center + new Vector3(0f, num * _heightOffset, 0f);
		RectTransform rectTransform = canvas.transform as RectTransform;
		Vector2 vector = cam.WorldToViewportPoint(position);
		return new Vector2(vector.x * rectTransform.sizeDelta.x - rectTransform.sizeDelta.x * 0.5f, vector.y * rectTransform.sizeDelta.y - rectTransform.sizeDelta.y * 0.5f);
	}

	public static Vector3 GetPointOffBoundsCenter(this Transform target, float _heightOffset)
	{
		Bounds bounds = target.GetComponent<Collider>().bounds;
		Vector3 center = bounds.center;
		float num = Vector3.Distance(bounds.min, bounds.max);
		return center + new Vector3(0f, num * _heightOffset, 0f);
	}
}
