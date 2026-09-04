using FIMSpace.FOptimizing;
using UnityEngine;
using UnityEngine.UI;

public class OptDemo_LODViewer : MonoBehaviour
{
	public FOptimizer_Base optimizer;

	public Text TextToWriteOn;

	private void Update()
	{
		if ((bool)optimizer && (bool)TextToWriteOn)
		{
			TextToWriteOn.text = "Current LOD id: " + optimizer.CurrentLODLevel;
			Text textToWriteOn = TextToWriteOn;
			textToWriteOn.text = textToWriteOn.text + "  Distance: " + Mathf.Round(optimizer.GetReferenceDistance());
			if (optimizer.TransitionPercent >= 0f)
			{
				Text textToWriteOn2 = TextToWriteOn;
				textToWriteOn2.text = textToWriteOn2.text + "\nTransition: " + Mathf.Round(optimizer.TransitionPercent * 100f) + "%";
			}
		}
	}
}
