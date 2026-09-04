using UnityEngine;

namespace Crosstales.Common.Audio;

[DisallowMultipleComponent]
public class FFTAnalyzer : MonoBehaviour
{
	[Tooltip("Array for the samples. More samples mean better accuracy but it also needs more performance (default: 256)")]
	public float[] Samples = new float[256];

	[Tooltip("Analyzed channel (0 = right, 1 = left, default: 0).")]
	[Range(0f, 1f)]
	public int Channel;

	[Tooltip("FFT-algorithm to analyze the audio (default: BlackmanHarris).")]
	public FFTWindow FFTMode = FFTWindow.BlackmanHarris;

	private void Update()
	{
		AudioListener.GetSpectrumData(Samples, Channel, FFTMode);
	}
}
