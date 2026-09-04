using UnityEngine;

public class DragObject : MonoBehaviour
{
	private Vector3 MouseOffset;

	private float mouseZCoord;

	private void Update()
	{
		if (Input.GetMouseButtonDown(1))
		{
			mouseZCoord = Camera.main.WorldToScreenPoint(base.gameObject.transform.position).z;
			MouseOffset = base.gameObject.transform.position - GetMouseWorldPos();
		}
		if (Input.GetMouseButton(1))
		{
			base.transform.position = GetMouseWorldPos() + MouseOffset;
		}
	}

	private Vector3 GetMouseWorldPos()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = mouseZCoord;
		return Camera.main.ScreenToWorldPoint(mousePosition);
	}
}
