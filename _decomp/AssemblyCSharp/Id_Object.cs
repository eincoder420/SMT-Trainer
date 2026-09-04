using UnityEngine;

public class Id_Object : MonoBehaviour
{
	public int id;

	public string[] Part_Name;

	public bool Tatoo;

	private Color base_color;

	private Renderer Tatoo_Render;

	private void Start()
	{
		if (Tatoo)
		{
			Tatoo_Render = GetComponent<Renderer>();
			base_color = Tatoo_Render.material.color;
		}
	}

	public void Highlight_Mat(bool Point_In)
	{
		if (Tatoo)
		{
			if (Point_In)
			{
				Tatoo_Render.material.color = new Color(2f, 2f, 0f, 1f);
			}
			else
			{
				Tatoo_Render.material.color = base_color;
			}
		}
	}
}
