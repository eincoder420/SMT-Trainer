using Invector;
using UnityEngine;

public class vUIIndicatorPosition : MonoBehaviour
{
	public Transform referencePosition;

	public RectTransform container;

	public Canvas canvas;

	protected RectTransform rectTransform;

	protected Camera _camera;

	private void Start()
	{
		_camera = Camera.main;
		if (canvas == null)
		{
			canvas = GetComponentInParent<Canvas>();
		}
		if (container == null)
		{
			container = GetComponentInParent<RectTransform>();
		}
		rectTransform = GetComponent<RectTransform>();
	}

	public void Update()
	{
		if ((bool)canvas && (bool)referencePosition)
		{
			rectTransform.anchoredPosition = ClampToWindow();
		}
	}

	private Vector2 ClampToWindow()
	{
		Vector3 vector = referencePosition.position - _camera.transform.position;
		Vector3 vector2 = _camera.transform.forward.AngleFormOtherDirection(vector.normalized);
		float t = Mathf.Clamp(Mathf.Abs(vector2.y) - 60f, 0f, 20f) / 20f;
		Vector3 position = referencePosition.position;
		Vector3 b = position + Quaternion.AngleAxis(0f - vector2.y, Vector3.up) * vector;
		return canvas.WorldToCanvas(Vector3.Lerp(position, b, t), _camera).ClampInsideRectagle(container, rectTransform.rect.size);
	}
}
