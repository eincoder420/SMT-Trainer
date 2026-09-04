using UnityEngine;

public static class CanvasExtensions
{
	public static Vector2 WorldToCanvas(this Canvas canvas, Vector3 world_position, Camera camera = null)
	{
		if (camera == null)
		{
			camera = Camera.main;
		}
		Vector3 vector = camera.WorldToViewportPoint(world_position);
		RectTransform component = canvas.GetComponent<RectTransform>();
		return new Vector2(vector.x * component.sizeDelta.x - component.sizeDelta.x * 0.5f, vector.y * component.sizeDelta.y - component.sizeDelta.y * 0.5f);
	}

	public static Vector2 ClampInsideRectagle(this Vector2 pos, RectTransform container, Vector2 margin)
	{
		Vector2 result = pos;
		result.x = Mathf.Clamp(result.x, (0f - container.rect.width) / 2f + margin.x, container.rect.width / 2f - margin.x);
		result.y = Mathf.Clamp(result.y, (0f - container.rect.height) / 2f + margin.y, container.rect.height / 2f - margin.y);
		return result;
	}
}
