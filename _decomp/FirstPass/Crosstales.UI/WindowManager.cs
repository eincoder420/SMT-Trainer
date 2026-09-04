using UnityEngine;

namespace Crosstales.UI;

public class WindowManager : MonoBehaviour
{
	[Tooltip("Window movement speed (default: 3).")]
	public float Speed = 3f;

	[Tooltip("Dependent GameObjects (active == open).")]
	public GameObject[] Dependencies;

	[Tooltip("Close the window at Start (default: true).")]
	public bool ClosedAtStart = true;

	private UIFocus _focus;

	private bool _open;

	private bool _close;

	private Vector3 _startPos;

	private Vector3 _centerPos;

	private Vector3 _lerpPos;

	private float _openProgress;

	private float _closeProgress;

	private GameObject _panel;

	private Transform _tf;

	private void Start()
	{
		_tf = base.transform;
		_panel = _tf.Find("Panel").gameObject;
		_startPos = _tf.position;
		if (ClosedAtStart)
		{
			ClosePanel();
			_panel.SetActive(value: false);
			if (Dependencies != null)
			{
				GameObject[] dependencies = Dependencies;
				for (int i = 0; i < dependencies.Length; i++)
				{
					dependencies[i].SetActive(value: false);
				}
			}
		}
		else
		{
			OpenPanel();
		}
	}

	private void Update()
	{
		_centerPos = new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f);
		if (_open && _openProgress < 1f)
		{
			_openProgress += Speed * Time.deltaTime;
			_tf.position = Vector3.Lerp(_lerpPos, _centerPos, _openProgress);
		}
		else
		{
			if (!_close)
			{
				return;
			}
			if (_closeProgress < 1f)
			{
				_closeProgress += Speed * Time.deltaTime;
				_tf.position = Vector3.Lerp(_lerpPos, _startPos, _closeProgress);
				return;
			}
			_panel.SetActive(value: false);
			if (Dependencies != null)
			{
				GameObject[] dependencies = Dependencies;
				for (int i = 0; i < dependencies.Length; i++)
				{
					dependencies[i].SetActive(value: false);
				}
			}
		}
	}

	public void SwitchPanel()
	{
		if (_open)
		{
			ClosePanel();
		}
		else
		{
			OpenPanel();
		}
	}

	public void OpenPanel()
	{
		_panel.SetActive(value: true);
		if (Dependencies != null)
		{
			GameObject[] dependencies = Dependencies;
			for (int i = 0; i < dependencies.Length; i++)
			{
				dependencies[i].SetActive(value: true);
			}
		}
		_focus = base.gameObject.GetComponent<UIFocus>();
		_focus.OnPanelEnter();
		_lerpPos = _tf.position;
		_open = true;
		_close = false;
		_openProgress = 0f;
	}

	public void ClosePanel()
	{
		_lerpPos = _tf.position;
		_open = false;
		_close = true;
		_closeProgress = 0f;
	}
}
