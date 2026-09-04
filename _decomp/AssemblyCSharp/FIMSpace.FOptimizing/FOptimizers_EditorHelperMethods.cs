using UnityEngine;

namespace FIMSpace.FOptimizing;

public static class FOptimizers_EditorHelperMethods
{
	public static readonly Color[] lODColors = new Color[8]
	{
		new Color(0.2231376f, 0.8011768f, 0.1619608f, 1f),
		new Color(0.2070592f, 0.6333336f, 0.7556864f, 1f),
		new Color(0.159216f, 0.5578432f, 0.3435296f, 1f),
		new Color(0.1333336f, 0.4f, 0.7982352f, 1f),
		new Color(0.3827448f, 0.2886272f, 0.5239216f, 1f),
		new Color(0.8f, 0.4423528f, 0f, 1f),
		new Color(0.4886272f, 0.1078432f, 0.80196f, 1f),
		new Color(0.7749016f, 0.6368624f, 0.0250984f, 1f)
	};

	public static readonly Color culledLODColor = new Color(0.38f, 0.43f, 0.25f, 1f);

	public static readonly Color hiddenLODColor = new Color(0.38f, 0.43f, 0.25f, 0.8f);

	public static bool CanDrawErrorMessage()
	{
		return false;
	}

	public static void DisplayError(string title, string message, string buttonText)
	{
	}

	public static Color GetLODColor(int index, int count, float multiply = 1f, float saturation = 1f, float value = 1f, float multiplyHiddenCull = 1f)
	{
		Color color = ((index == count) ? culledLODColor : ((index != count + 1) ? lODColors[index] : hiddenLODColor));
		if (multiply != 1f)
		{
			color *= Color.white * multiply;
		}
		if (multiplyHiddenCull != 1f && (index == count || index == count + 1))
		{
			color *= Color.white * multiplyHiddenCull;
		}
		if (saturation != 1f || value != 1f)
		{
			Color.RGBToHSV(color, out var H, out var S, out var V);
			color = Color.HSVToRGB(H, S * saturation, V * value);
		}
		return color;
	}
}
