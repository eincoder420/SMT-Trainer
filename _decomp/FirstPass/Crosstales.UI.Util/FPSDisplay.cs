using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.UI.Util;

[DisallowMultipleComponent]
public class FPSDisplay : MonoBehaviour
{
	[Tooltip("Text component to display the FPS.")]
	public Text FPS;

	[Tooltip("Update every set frame (default: 5).")]
	[Range(1f, 300f)]
	public int FrameUpdate = 5;

	[Tooltip("Key to activate the FPS counter (default: none).")]
	public KeyCode Key;

	private float _deltaTime;

	private float _elapsedTime;

	private float _msec;

	private float _fps;

	private const string WAIT = "<i>...calculating <b>FPS</b>...</i>";

	private const string RED = "<color=#E57373><b>FPS: {0:0.}</b> ({1:0.0} ms)</color>";

	private const string ORANGE = "<color=#FFB74D><b>FPS: {0:0.}</b> ({1:0.0} ms)</color>";

	private const string GREEN = "<color=#81C784><b>FPS: {0:0.}</b> ({1:0.0} ms)</color>";

	private void Update()
	{
		if (Key == KeyCode.None || Input.GetKey(Key))
		{
			_deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
			_elapsedTime += Time.unscaledDeltaTime;
			if (_elapsedTime > 1f)
			{
				if (Time.frameCount % FrameUpdate == 0)
				{
					FPS.enabled = true;
					_msec = _deltaTime * 1000f;
					_fps = 1f / _deltaTime;
					if (_fps < 15f)
					{
						FPS.text = $"<color=#E57373><b>FPS: {_fps:0.}</b> ({_msec:0.0} ms)</color>";
					}
					else if (_fps < 29f)
					{
						FPS.text = $"<color=#FFB74D><b>FPS: {_fps:0.}</b> ({_msec:0.0} ms)</color>";
					}
					else
					{
						FPS.text = $"<color=#81C784><b>FPS: {_fps:0.}</b> ({_msec:0.0} ms)</color>";
					}
				}
			}
			else
			{
				FPS.text = "<i>...calculating <b>FPS</b>...</i>";
			}
		}
		else
		{
			FPS.enabled = false;
		}
	}
}
