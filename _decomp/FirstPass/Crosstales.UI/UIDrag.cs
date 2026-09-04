using UnityEngine;

namespace Crosstales.UI;

[DisallowMultipleComponent]
public class UIDrag : MonoBehaviour
{
	private float _offsetX;

	private float _offsetY;

	private Transform _tf;

	private void Start()
	{
		_tf = base.transform;
	}

	public void BeginDrag()
	{
		Vector3 position = _tf.position;
		_offsetX = position.x - Input.mousePosition.x;
		_offsetY = position.y - Input.mousePosition.y;
	}

	public void OnDrag()
	{
		_tf.position = new Vector3(_offsetX + Input.mousePosition.x, _offsetY + Input.mousePosition.y);
	}
}
