using UnityEngine;
using UnityEngine.UI;

public class ColorPreview : MonoBehaviour
{
	public int id;

	public int mat_id;

	private Game_Data data;

	public Graphic previewGraphic;

	public Material material;

	public ColorPicker colorPicker;

	public bool Cloth;

	public bool Finger;

	public bool Eye;

	public bool Pussy;

	public bool Hair;

	public bool Lips;

	public bool Eyeshadow;

	public bool Tatoo;

	private Edit_Base edit;

	private void Start()
	{
		if (!edit)
		{
			edit = Object.FindObjectOfType<Edit_Base>();
		}
		data = edit.data;
		if ((bool)material)
		{
			previewGraphic.color = material.color;
		}
		colorPicker.onColorChanged += OnColorChanged;
	}

	public void OnColorChanged(Color c)
	{
		if (!(colorPicker.Current_Preview == this))
		{
			return;
		}
		previewGraphic.color = c;
		if ((bool)material)
		{
			material.color = c;
		}
		if (Hair)
		{
			edit.Set_Hair_Materials();
		}
		if (Cloth)
		{
			material.SetColor("_Color", c);
		}
		if ((bool)data)
		{
			if (Cloth && mat_id == 0)
			{
				data.Clothes[id].main_color = c;
			}
			if (Finger)
			{
				data.Character.finger_color = c;
			}
			if (Eye)
			{
				data.Character.eye_color = c;
			}
			if (Pussy)
			{
				data.Character.pussy_color = c;
			}
			if (Hair)
			{
				data.Character.hair_color = c;
			}
			if (Lips)
			{
				data.Character.lips_color = c;
			}
			if (Eyeshadow)
			{
				data.Character.eyeshadow_color = c;
			}
			if (Tatoo)
			{
				data.Tatoo[id].color = c;
			}
		}
	}

	private void OnDestroy()
	{
		if (colorPicker != null)
		{
			colorPicker.onColorChanged -= OnColorChanged;
		}
	}
}
