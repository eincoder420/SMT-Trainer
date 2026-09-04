using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.UI;

[DisallowMultipleComponent]
public class UIFocus : MonoBehaviour
{
	[Tooltip("Name of the gameobject containing the UIWindowManager.")]
	public string ManagerName = "Canvas";

	private UIWindowManager _manager;

	private Image _image;

	private Transform _tf;

	private void Start()
	{
	}

	private void Awake()
	{
		_tf = base.transform;
		_manager = GameObject.Find(ManagerName).GetComponent<UIWindowManager>();
		_image = _tf.Find("Panel/Header").GetComponent<Image>();
	}

	public void OnPanelEnter()
	{
		if (_manager != null)
		{
			_manager.ChangeState(base.gameObject);
		}
		Color color = _image.color;
		color.a = 255f;
		_image.color = color;
		_tf.SetAsLastSibling();
		_tf.SetAsFirstSibling();
		_tf.SetSiblingIndex(-1);
		_tf.GetSiblingIndex();
	}
}
