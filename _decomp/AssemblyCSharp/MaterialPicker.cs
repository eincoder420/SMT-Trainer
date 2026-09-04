using UnityEngine;

public class MaterialPicker : MonoBehaviour
{
	[Tooltip("Material Selection")]
	public Material[] MaterialList;

	private Material pick;

	private int n;

	private void Start()
	{
		n = 0;
	}

	private void Update()
	{
		pick = MaterialList[n];
		GetComponentInChildren<Renderer>().material = pick;
		if (Input.GetKeyDown(KeyCode.UpArrow) && n < MaterialList.Length - 1)
		{
			n++;
		}
		if (Input.GetKeyDown(KeyCode.DownArrow) && n > 0)
		{
			n--;
		}
	}
}
