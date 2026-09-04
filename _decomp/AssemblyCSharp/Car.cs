using UnityEngine;

public class Car : MonoBehaviour
{
	public GameObject col;

	public Material Main_Mat;

	public Transform out_target;

	private Roxanne_Control rox;

	public Transform Map_Sign;

	private void Start()
	{
		Object.FindObjectOfType<Street_Control>();
		float r = Random.Range(0f, 1f);
		float g = Random.Range(0f, 1f);
		float b = Random.Range(0f, 1f);
		Main_Mat.color = new Color(r, g, b);
	}
}
