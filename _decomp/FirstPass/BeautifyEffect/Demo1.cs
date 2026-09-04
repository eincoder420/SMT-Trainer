using UnityEngine;
using UnityEngine.UI;

namespace BeautifyEffect;

public class Demo1 : MonoBehaviour
{
	private float deltaTime;

	private bool benchmarkEnabled;

	private GUIStyle style;

	private Rect rect;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.J))
		{
			Beautify.instance.bloomIntensity += 0.1f;
		}
		if (Input.GetKeyDown(KeyCode.T) || Input.GetMouseButtonDown(0))
		{
			Beautify.instance.enabled = !Beautify.instance.enabled;
			UpdateText();
		}
		if (Input.GetKeyDown(KeyCode.F))
		{
			benchmarkEnabled = !benchmarkEnabled;
		}
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
		if (Input.GetKeyDown(KeyCode.B))
		{
			Beautify.instance.Blink(0.1f);
		}
	}

	private void UpdateText()
	{
		if (Beautify.instance.enabled)
		{
			GameObject.Find("Beautify").GetComponent<Text>().text = "Beautify ON";
		}
		else
		{
			GameObject.Find("Beautify").GetComponent<Text>().text = "Beautify OFF";
		}
	}

	private void OnGUI()
	{
		if (benchmarkEnabled)
		{
			int width = Screen.width;
			int height = Screen.height;
			if (style == null)
			{
				style = new GUIStyle();
				rect = new Rect(0f, 0f, width, height * 4 / 100);
				style.alignment = TextAnchor.UpperLeft;
				style.fontSize = height * 4 / 100;
				style.normal.textColor = Color.white;
			}
			float num = deltaTime * 1000f;
			float num2 = 1f / deltaTime;
			string text = $"{num:0.0} ms ({num2:0.} fps)";
			GUI.Label(rect, text, style);
		}
	}
}
