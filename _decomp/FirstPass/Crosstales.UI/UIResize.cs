using UnityEngine;
using UnityEngine.EventSystems;

namespace Crosstales.UI;

[DisallowMultipleComponent]
public class UIResize : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IDragHandler
{
	[Tooltip("Minimum size of the UI element.")]
	public Vector2 MinSize = new Vector2(300f, 160f);

	[Tooltip("Maximum size of the UI element.")]
	public Vector2 MaxSize = new Vector2(800f, 600f);

	[Tooltip("Ignore maximum size of the UI element (default: false).")]
	public bool IgnoreMaxSize;

	[Tooltip("Resize speed (default: 2).")]
	public float SpeedFactor = 2f;

	private RectTransform _panelRectTransform;

	private Vector2 _originalLocalPointerPosition;

	private Vector2 _originalSizeDelta;

	private Vector2 _originalSize;

	private void Awake()
	{
		_panelRectTransform = base.transform.parent.GetComponent<RectTransform>();
		Rect rect = _panelRectTransform.rect;
		_originalSize = new Vector2(rect.width, rect.height);
	}

	public void OnPointerDown(PointerEventData data)
	{
		_originalSizeDelta = _panelRectTransform.sizeDelta;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_panelRectTransform, data.position, data.pressEventCamera, out _originalLocalPointerPosition);
	}

	public void OnDrag(PointerEventData data)
	{
		if (!(_panelRectTransform == null))
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_panelRectTransform, data.position, data.pressEventCamera, out var localPoint);
			Vector3 vector = localPoint - _originalLocalPointerPosition;
			Vector2 sizeDelta = _originalSizeDelta + new Vector2(vector.x * SpeedFactor, (0f - vector.y) * SpeedFactor);
			if (_originalSize.x + sizeDelta.x < MinSize.x)
			{
				sizeDelta.x = 0f - (_originalSize.x - MinSize.x);
			}
			else if (!IgnoreMaxSize && _originalSize.x + sizeDelta.x > MaxSize.x)
			{
				sizeDelta.x = MaxSize.x - _originalSize.x;
			}
			if (_originalSize.y + sizeDelta.y < MinSize.y)
			{
				sizeDelta.y = 0f - (_originalSize.y - MinSize.y);
			}
			else if (!IgnoreMaxSize && _originalSize.y + sizeDelta.y > MaxSize.y)
			{
				sizeDelta.y = MaxSize.y - _originalSize.y;
			}
			_panelRectTransform.sizeDelta = sizeDelta;
		}
	}
}
