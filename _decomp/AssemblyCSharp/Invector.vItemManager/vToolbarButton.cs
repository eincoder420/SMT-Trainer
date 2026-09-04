using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Invector.vItemManager;

[RequireComponent(typeof(Image))]
public class vToolbarButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public GameObject targetWindow;

	public Image image;

	public Color selectedColor = Color.white;

	public Color unSelectedColor = Color.grey;

	public UnityEvent onSelect;

	public UnityEvent onDeselect;

	private bool isSelected;

	public void Reset()
	{
		image = GetComponent<Image>();
		if (!image)
		{
			image = base.gameObject.AddComponent<Image>();
		}
	}

	private void OnDisable()
	{
		image.color = unSelectedColor;
		image.SetAllDirty();
		onDeselect.Invoke();
		isSelected = false;
	}

	public void OnSelectTool(vToolbarButton toolbarButton)
	{
		if (toolbarButton.Equals(this))
		{
			image.color = selectedColor;
			if (!isSelected)
			{
				isSelected = true;
				onSelect.Invoke();
			}
			image.SetAllDirty();
		}
		else
		{
			image.color = unSelectedColor;
			image.SetAllDirty();
			onDeselect.Invoke();
			isSelected = false;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		onSelect.Invoke();
		isSelected = true;
	}
}
