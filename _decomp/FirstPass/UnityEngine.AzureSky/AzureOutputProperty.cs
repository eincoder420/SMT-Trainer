using System;

namespace UnityEngine.AzureSky;

[Serializable]
public sealed class AzureOutputProperty
{
	public AzureOutputType type;

	public float slider;

	public AnimationCurve timelineCurve = AnimationCurve.Linear(0f, 0f, 24f, 0f);

	public AnimationCurve sunCurve = AnimationCurve.Linear(-1f, 0f, 1f, 0f);

	public AnimationCurve moonCurve = AnimationCurve.Linear(-1f, 0f, 1f, 0f);

	public Color color = Color.white;

	public Gradient timelineGradient = new Gradient();

	public Gradient sunGradient = new Gradient();

	public Gradient moonGradient = new Gradient();

	public float GetFloatValue(float time, float sunElevation, float moonElevation)
	{
		return type switch
		{
			AzureOutputType.Slider => slider, 
			AzureOutputType.TimelineCurve => timelineCurve.Evaluate(time), 
			AzureOutputType.SunCurve => sunCurve.Evaluate(sunElevation), 
			AzureOutputType.MoonCurve => moonCurve.Evaluate(moonElevation), 
			_ => slider, 
		};
	}

	public Color GetColorValue(float time, float sunElevation, float moonElevation)
	{
		return type switch
		{
			AzureOutputType.Color => color, 
			AzureOutputType.TimelineGradient => timelineGradient.Evaluate(time / 24f), 
			AzureOutputType.SunGradient => sunGradient.Evaluate(Mathf.InverseLerp(-1f, 1f, sunElevation)), 
			AzureOutputType.MoonGradient => moonGradient.Evaluate(Mathf.InverseLerp(-1f, 1f, moonElevation)), 
			_ => color, 
		};
	}
}
