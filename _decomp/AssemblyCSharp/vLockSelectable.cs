using UnityEngine;
using UnityEngine.EventSystems;

public class vLockSelectable : MonoBehaviour
{
	public GameObject target;

	private void OnDisable()
	{
		target = null;
	}

	private void Update()
	{
		if (EventSystem.current.currentSelectedGameObject == null && (bool)target)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, eventData, ExecuteEvents.pointerExitHandler);
			EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
			ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerEnterHandler);
		}
		else if (EventSystem.current.currentSelectedGameObject != null)
		{
			target = EventSystem.current.currentSelectedGameObject;
		}
	}
}
