using UnityEngine;

public class vDebugUtils : MonoBehaviour
{
	public KeyCode timeScaleDown = KeyCode.KeypadMinus;

	public KeyCode timeScaleUp = KeyCode.KeypadPlus;

	public float timeScaleChangeValue = 0.1f;

	public bool affectFixedDeltaTime = true;

	private float currentFixedDeltaTime;

	private void Start()
	{
		currentFixedDeltaTime = Time.fixedDeltaTime;
	}

	private void Update()
	{
		if (Input.GetKeyDown(timeScaleDown))
		{
			Time.timeScale = Mathf.Clamp(Time.timeScale - timeScaleChangeValue, 0f, 1f);
			if (affectFixedDeltaTime)
			{
				Time.fixedDeltaTime = Time.timeScale * currentFixedDeltaTime;
			}
		}
		else if (Input.GetKeyDown(timeScaleUp))
		{
			Time.timeScale = Mathf.Clamp(Time.timeScale + timeScaleChangeValue, 0f, 1f);
			if (affectFixedDeltaTime)
			{
				Time.fixedDeltaTime = Time.timeScale * currentFixedDeltaTime;
			}
		}
	}

	private void OnGUI()
	{
		GUILayout.Label("TimeScale:" + Time.timeScale);
	}
}
