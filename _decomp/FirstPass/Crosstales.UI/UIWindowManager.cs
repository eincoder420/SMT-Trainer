using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.UI;

[DisallowMultipleComponent]
public class UIWindowManager : MonoBehaviour
{
	[Tooltip("All Windows of the scene.")]
	public GameObject[] Windows;

	private Image _image;

	private GameObject _dontTouch;

	private void Start()
	{
		GameObject[] windows = Windows;
		foreach (GameObject gameObject in windows)
		{
			_image = gameObject.transform.Find("Panel/Header").GetComponent<Image>();
			Color color = _image.color;
			color.a = 0.2f;
			_image.color = color;
		}
	}

	public void ChangeState(GameObject active)
	{
		GameObject[] windows = Windows;
		foreach (GameObject gameObject in windows)
		{
			if (gameObject != active)
			{
				_image = gameObject.transform.Find("Panel/Header").GetComponent<Image>();
				Color color = _image.color;
				color.a = 0.2f;
				_image.color = color;
			}
			_dontTouch = gameObject.transform.Find("Panel/DontTouch").gameObject;
			_dontTouch.SetActive(gameObject != active);
		}
	}
}
