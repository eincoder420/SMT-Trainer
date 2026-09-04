using UnityEngine;
using UnityEngine.UI;

namespace Invector;

public class vSliderSizeControl : MonoBehaviour
{
	public Slider slider;

	public RectTransform rectTransform;

	public float multipScale = 0.1f;

	private float oldMaxValue;

	private void OnDrawGizmosSelected()
	{
		UpdateScale();
	}

	public void UpdateScale()
	{
		if ((bool)rectTransform && (bool)slider && slider.maxValue != oldMaxValue)
		{
			Vector2 sizeDelta = rectTransform.sizeDelta;
			sizeDelta.x = slider.maxValue * multipScale;
			rectTransform.sizeDelta = sizeDelta;
			oldMaxValue = slider.maxValue;
		}
	}
}
