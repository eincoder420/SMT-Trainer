using UnityEngine;
using UnityEngine.UI;

namespace BeautifyEffect;

public class Demo2 : MonoBehaviour
{
	private int demoMode;

	private void Start()
	{
		UpdateDemoMode();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			demoMode++;
			if (demoMode >= 11)
			{
				demoMode = 0;
			}
			UpdateDemoMode();
		}
		else if (Input.GetKeyDown(KeyCode.T))
		{
			if (demoMode > 0)
			{
				demoMode = 0;
			}
			else
			{
				demoMode = 1;
			}
			UpdateDemoMode();
		}
	}

	private void UpdateDemoMode()
	{
		string text = "";
		Beautify.instance.enabled = demoMode > 0;
		switch (demoMode)
		{
		case 0:
			text = "BEAUTIFY OFF (click to enable)";
			break;
		case 1:
			text = "BEAUTIFY ON";
			Beautify.instance.lut = false;
			Beautify.instance.outline = false;
			Beautify.instance.nightVision = false;
			Beautify.instance.bloom = false;
			Beautify.instance.anamorphicFlares = false;
			Beautify.instance.lensDirt = false;
			Beautify.instance.vignetting = false;
			Beautify.instance.frame = false;
			Beautify.instance.sunFlares = false;
			break;
		case 2:
			text = "BEAUTIFY ON + vignetting";
			Beautify.instance.vignetting = true;
			Beautify.instance.vignettingColor = new Color(0f, 0f, 0f, 0.05f);
			break;
		case 3:
			text = "BEAUTIFY ON + vignetting + bloom";
			Beautify.instance.bloom = true;
			break;
		case 4:
			text = "BEAUTIFY ON + vignetting + sun flares";
			Beautify.instance.sunFlares = true;
			break;
		case 5:
			text = "BEAUTIFY ON + vignetting + bloom + lens dirt";
			Beautify.instance.lensDirt = true;
			break;
		case 6:
			text = "BEAUTIFY ON + vignetting + lens dirt + anamorphic flares";
			Beautify.instance.bloom = false;
			Beautify.instance.anamorphicFlares = true;
			break;
		case 7:
			text = "BEAUTIFY ON + vignetting + lens dirt + vertical anamorphic flares";
			Beautify.instance.anamorphicFlaresVertical = true;
			break;
		case 8:
			text = "BEAUTIFY ON + vignetting + bloom + lens dirt + night vision";
			Beautify.instance.bloom = true;
			Beautify.instance.anamorphicFlares = false;
			Beautify.instance.nightVision = true;
			break;
		case 9:
			text = "BEAUTIFY ON + red vignetting + bloom + lens dirt + thermal vision";
			Beautify.instance.thermalVision = true;
			break;
		case 10:
			text = "BEAUTIFY ON + LUT sepia + outline + frame";
			Beautify.instance.thermalVision = false;
			Beautify.instance.vignetting = false;
			Beautify.instance.lut = true;
			Beautify.instance.outline = true;
			Beautify.instance.bloom = false;
			Beautify.instance.anamorphicFlares = false;
			Beautify.instance.anamorphicFlaresVertical = false;
			Beautify.instance.lensDirt = false;
			Beautify.instance.frame = true;
			break;
		}
		GameObject.Find("Beautify").GetComponent<Text>().text = text;
	}
}
