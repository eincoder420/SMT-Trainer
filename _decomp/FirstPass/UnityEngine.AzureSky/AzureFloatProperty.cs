using System;

namespace UnityEngine.AzureSky;

[Serializable]
public sealed class AzureFloatProperty
{
	public enum PropertyType
	{
		Slider,
		TimelineCurve,
		SunCurve,
		MoonCurve
	}

	public PropertyType type;

	public float slider;

	public AnimationCurve timelineCurve;

	public AnimationCurve sunCurve;

	public AnimationCurve moonCurve;

	public AzureFloatProperty(float slider, AnimationCurve timelineCurve, AnimationCurve sunCurve, AnimationCurve moonCurve)
	{
		this.slider = slider;
		this.timelineCurve = timelineCurve;
		this.sunCurve = sunCurve;
		this.moonCurve = moonCurve;
	}

	public float GetValue(float time, float sunElevation, float moonElevation)
	{
		return type switch
		{
			PropertyType.Slider => slider, 
			PropertyType.TimelineCurve => timelineCurve.Evaluate(time), 
			PropertyType.SunCurve => sunCurve.Evaluate(sunElevation), 
			PropertyType.MoonCurve => moonCurve.Evaluate(moonElevation), 
			_ => slider, 
		};
	}
}
