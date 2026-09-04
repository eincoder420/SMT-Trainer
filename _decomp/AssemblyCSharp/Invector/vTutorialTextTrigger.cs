using UnityEngine;
using UnityEngine.UI;

namespace Invector;

public class vTutorialTextTrigger : MonoBehaviour
{
	[TextArea(5, 3000)]
	[Multiline]
	public string text;

	public Text _textUI;

	public GameObject painel;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			EnableTutorialPanel();
		}
	}

	public void EnableTutorialPanel()
	{
		painel.SetActive(value: true);
		_textUI.gameObject.SetActive(value: true);
		_textUI.text = text;
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			DisableTutorialPanel();
		}
	}

	public void DisableTutorialPanel()
	{
		painel.SetActive(value: false);
		_textUI.gameObject.SetActive(value: false);
		_textUI.text = " ";
	}
}
