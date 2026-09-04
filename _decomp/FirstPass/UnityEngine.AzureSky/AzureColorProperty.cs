using System;

namespace UnityEngine.AzureSky;

[Serializable]
public sealed class AzureColorProperty
{
	public enum PropertyType
	{
		Color,
		TimelineGradient,
		SunGradient,
		MoonGradient
	}

	public PropertyType type;

	public Color color;

	public Gradient timelineGradient;

	public Gradient sunGradient;

	public Gradient moonGradient;

	public AzureColorProperty(Color color, Gradient timelineGradient, Gradient sunGradient, Gradient moonGradient)
	{
		this.color = color;
		this.timelineGradient = timelineGradient;
		this.sunGradient = sunGradient;
		this.moonGradient = moonGradient;
	}

	public Color GetValue(float time, float sunElevation, float moonElevation)
	{
		return type switch
		{
			PropertyType.Color => color, 
			PropertyType.TimelineGradient => timelineGradient.Evaluate(time / 24f), 
			PropertyType.SunGradient => sunGradient.Evaluate(Mathf.InverseLerp(-1f, 1f, sunElevation)), 
			PropertyType.MoonGradient => moonGradient.Evaluate(Mathf.InverseLerp(-1f, 1f, moonElevation)), 
			_ => color, 
		};
	}
}
