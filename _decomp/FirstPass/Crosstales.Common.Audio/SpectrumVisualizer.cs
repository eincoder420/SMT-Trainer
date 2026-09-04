using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.Common.Audio;

public class SpectrumVisualizer : MonoBehaviour
{
	[Tooltip("FFT-analyzer with the spectrum data.")]
	public FFTAnalyzer Analyzer;

	[Tooltip("Prefab for the frequency representation.")]
	public GameObject VisualPrefab;

	[Tooltip("Width per prefab.")]
	public float Width = 0.075f;

	[Tooltip("Gain-power for the frequency.")]
	public float Gain = 70f;

	[Tooltip("Frequency band from left-to-right (default: true).")]
	public bool LeftToRight = true;

	[Tooltip("Opacity of the material of the prefab (default: 1).")]
	[Range(0f, 1f)]
	public float Opacity = 1f;

	private Transform _tf;

	private Transform[] _visualTransforms;

	private Vector3 _visualPos = Vector3.zero;

	private int _samplesPerChannel;

	private void Start()
	{
		_tf = base.transform;
		_samplesPerChannel = Analyzer.Samples.Length / 2;
		_visualTransforms = new Transform[_samplesPerChannel];
		for (int i = 0; i < _samplesPerChannel; i++)
		{
			GameObject gameObject;
			if (LeftToRight)
			{
				Vector3 position = _tf.position;
				gameObject = Object.Instantiate(VisualPrefab, new Vector3(position.x + (float)i * Width, position.y, position.z), Quaternion.identity);
			}
			else
			{
				Vector3 position2 = _tf.position;
				gameObject = Object.Instantiate(VisualPrefab, new Vector3(position2.x - (float)i * Width, position2.y, position2.z), Quaternion.identity);
			}
			gameObject.GetComponent<Renderer>().material.color = BaseHelper.HSVToRGB(360f / (float)_samplesPerChannel * (float)i, 1f, 1f, Opacity);
			_visualTransforms[i] = gameObject.GetComponent<Transform>();
			_visualTransforms[i].parent = _tf;
		}
	}

	private void Update()
	{
		for (int i = 0; i < _visualTransforms.Length; i++)
		{
			_visualPos.Set(Width, Analyzer.Samples[i] * Gain, Width);
			_visualTransforms[i].localScale = _visualPos;
		}
	}
}
