using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class vScorePointDisplay : MonoBehaviour
{
	[SerializeField]
	protected Text _display;

	public string stringFormat;

	private const string StringFormatDefault = "{0}";

	public Text display
	{
		get
		{
			if (_display == null)
			{
				_display = GetComponent<Text>();
			}
			return _display;
		}
	}

	public void ShowValue(float value)
	{
		if (string.IsNullOrEmpty(stringFormat))
		{
			stringFormat = "{0}";
		}
		display.text = string.Format(stringFormat, value.ToString());
	}

	public void ShowValue(int value)
	{
		ShowValue((float)value);
	}
}
