using System;
using UnityEngine;
using UnityEngine.UI;

public class Settings_Menu : MonoBehaviour
{
	[Serializable]
	public struct Dispay_Toggles
	{
		public Toggle Arrow_Tog;

		public Toggle Tasks_Tog;

		public Toggle Time_Tog;

		public Toggle Sliders_Tog;

		public Toggle Cloth_Floor_Tog;

		public Toggle Char_Names_Tog;

		public Toggle Mouse_Tog;

		public Toggle Freeze_Tog;
	}

	[Serializable]
	public struct Graphic_Settings
	{
		public Text graphics;

		public Text resolution;

		public Slider visibility;

		public Slider Music_Slider;

		public Slider Sound_Slider;

		public Slider Interface_Slider;

		public Toggle Fullscreen;

		public Toggle Motion;

		public Toggle Vsync;

		public Toggle Shadows;

		public Toggle Bloom;

		public Toggle Occlusion;

		public Toggle Small_Decor;
	}

	public Dispay_Toggles display;

	public Graphic_Settings settings;

	public Game_Data data;

	public Start_Menu menu;

	private PauseMenuScript interface_script;

	private void Start()
	{
		if (!menu)
		{
			menu = UnityEngine.Object.FindObjectOfType<Start_Menu>();
		}
		interface_script = UnityEngine.Object.FindObjectOfType<PauseMenuScript>();
		display.Arrow_Tog.SetIsOnWithoutNotify(data.Display.Show_Arrow);
		display.Tasks_Tog.SetIsOnWithoutNotify(data.Display.Show_Tasks);
		display.Time_Tog.SetIsOnWithoutNotify(data.Display.Show_Time);
		display.Sliders_Tog.SetIsOnWithoutNotify(data.Display.Show_Sliders);
		display.Cloth_Floor_Tog.SetIsOnWithoutNotify(data.Display.Show_Cloth_Names);
		display.Char_Names_Tog.SetIsOnWithoutNotify(data.Display.Show_Char_Names);
		display.Mouse_Tog.SetIsOnWithoutNotify(data.Display.Show_Mouse);
		display.Freeze_Tog.SetIsOnWithoutNotify(data.Display.Freeze_Mouse);
		settings.Music_Slider.value = data.Sounds.Music_Volume;
		settings.Sound_Slider.value = data.Sounds.Sound_Volume;
		settings.Interface_Slider.value = data.Sounds.Interface_Volume;
		settings.visibility.value = Mathf.Lerp(settings.visibility.minValue, settings.visibility.maxValue, data.Graphics.visibility);
		settings.resolution.text = data.Resolutions[data.Graphics.Resolution_level].x + "/" + data.Resolutions[data.Graphics.Resolution_level].y;
		settings.Fullscreen.SetIsOnWithoutNotify(data.Graphics.Full_Screen);
		settings.Motion.SetIsOnWithoutNotify(data.Graphics.Motion_blur);
		settings.Vsync.SetIsOnWithoutNotify(data.Graphics.Vsync);
		settings.Shadows.SetIsOnWithoutNotify(data.Graphics.Shadows);
		settings.Bloom.SetIsOnWithoutNotify(data.Graphics.Bloom);
		settings.Occlusion.SetIsOnWithoutNotify(data.Graphics.Occlusion);
		settings.Small_Decor.SetIsOnWithoutNotify(data.Graphics.Small_Decor);
		if ((bool)interface_script)
		{
			interface_script.Show_Display_Elements();
		}
	}

	public void Set_Quality()
	{
		if (data.Graphics.Graphics_Level < 2)
		{
			data.Graphics.Graphics_Level++;
		}
		else
		{
			data.Graphics.Graphics_Level = 0;
		}
		menu.Set_Graphics();
	}

	public void Show_Hide_Arrow()
	{
		data.Display.Show_Arrow = !data.Display.Show_Arrow;
		if ((bool)interface_script)
		{
			interface_script.Show_Display_Elements();
		}
	}

	public void Show_Hide_Tasks()
	{
		data.Display.Show_Tasks = !data.Display.Show_Tasks;
		if ((bool)interface_script)
		{
			interface_script.Show_Display_Elements();
		}
	}

	public void Show_Hide_Time()
	{
		data.Display.Show_Time = !data.Display.Show_Time;
		if ((bool)interface_script)
		{
			interface_script.Show_Display_Elements();
		}
	}

	public void Show_Hide_Sliders()
	{
		data.Display.Show_Sliders = !data.Display.Show_Sliders;
		if ((bool)interface_script)
		{
			interface_script.Show_Display_Elements();
		}
	}

	public void Show_Hide_Char_Names()
	{
		data.Display.Show_Char_Names = !data.Display.Show_Char_Names;
		if ((bool)interface_script)
		{
			interface_script.Show_Display_Elements();
		}
	}

	public void Show_Hide_Mouse()
	{
		data.Display.Show_Mouse = !data.Display.Show_Mouse;
		if ((bool)interface_script)
		{
			interface_script.Show_Display_Elements();
		}
	}

	public void Show_Hide_Freeze_Mouse()
	{
		data.Display.Freeze_Mouse = !data.Display.Freeze_Mouse;
		if ((bool)interface_script)
		{
			interface_script.Show_Display_Elements();
		}
	}

	public void Show_Hide_Floor_Names()
	{
		data.Display.Show_Cloth_Names = !data.Display.Show_Cloth_Names;
		if ((bool)interface_script)
		{
			interface_script.Show_Display_Elements();
		}
	}

	public void Set_Music_Volume()
	{
		data.Sounds.Music_Volume = settings.Music_Slider.value;
		menu.Set_All_Sounds();
	}

	public void Set_Sound_Volume()
	{
		data.Sounds.Sound_Volume = settings.Sound_Slider.value;
		menu.Set_All_Sounds();
	}

	public void Set_Interface_Volume()
	{
		data.Sounds.Interface_Volume = settings.Interface_Slider.value;
		menu.Set_All_Sounds();
	}

	public void Set_Visibility_Range()
	{
		data.Graphics.visibility = settings.visibility.value;
		menu.Set_Camera_And_Fog();
	}

	public void Set_Bloom()
	{
		data.Graphics.Bloom = !data.Graphics.Bloom;
		menu.Change_Bloom();
	}

	public void Set_Vsync()
	{
		data.Graphics.Vsync = !data.Graphics.Vsync;
		menu.Change_Vsync();
	}

	public void Set_Shadows()
	{
		data.Graphics.Shadows = !data.Graphics.Shadows;
		menu.Check_Shadows();
	}

	public void Set_Motion_Blur()
	{
		data.Graphics.Motion_blur = !data.Graphics.Motion_blur;
		menu.Change_Motion_Blur();
	}

	public void Set_Fullscreen()
	{
		data.Graphics.Full_Screen = !data.Graphics.Full_Screen;
		menu.Check_Fullscreen();
	}

	public void Set_Occlusion()
	{
		data.Graphics.Occlusion = !data.Graphics.Occlusion;
		menu.Check_Occlusion();
	}

	public void Set_Resolution()
	{
		if (data.Graphics.Resolution_level < 5)
		{
			data.Graphics.Resolution_level++;
		}
		else
		{
			data.Graphics.Resolution_level = 0;
		}
		menu.Change_Resolution();
	}
}
