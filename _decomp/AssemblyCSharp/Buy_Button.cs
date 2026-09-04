using UnityEngine;
using UnityEngine.UI;

public class Buy_Button : MonoBehaviour
{
	public int Price;

	public bool Cloth;

	public bool Haircut;

	public bool Stuff;

	public int id;

	public int variant;

	public Image Icon;

	private void Start()
	{
		Text component = GetComponentInChildren<Button>().transform.GetChild(0).GetChild(1).GetComponent<Text>();
		component.text = Price.ToString();
		component.fontSize = 35;
	}
}
