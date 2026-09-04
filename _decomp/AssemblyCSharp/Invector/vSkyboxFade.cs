using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector;

[vClassHeader("Skybox Fade", true, "icon_v2", false, "", helpBoxText = "A Skybox with Cubemap type is Required to use this Component", useHelpBox = true)]
public class vSkyboxFade : vMonoBehaviour
{
	[Serializable]
	public class SkyboxFadeSettings
	{
		public string name = "My SkySettings";

		public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public float fadeTime = 1f;

		public Color tint = new Color(0.5f, 0.5f, 0.5f, 0.5f);

		[Range(0f, 8f)]
		public float exposure = 1f;

		[Range(0f, 360f)]
		public float rotation;

		public SkyboxFadeSettings()
		{
		}

		public SkyboxFadeSettings(Material mat)
		{
			tint = mat.GetColor("_Tint");
			exposure = mat.GetFloat("_Exposure");
			rotation = mat.GetFloat("_Rotation");
		}

		public void CopyMaterial(Material mat)
		{
			tint = mat.GetColor("_Tint");
			exposure = mat.GetFloat("_Exposure");
			rotation = mat.GetFloat("_Rotation");
		}

		public SkyboxFadeSettings Copy()
		{
			return new SkyboxFadeSettings
			{
				curve = curve,
				fadeTime = fadeTime,
				tint = tint,
				exposure = exposure,
				rotation = rotation
			};
		}
	}

	public SkyboxFadeSettings defaultSkyboxSettings;

	public List<SkyboxFadeSettings> fadeSettings;

	private SkyboxFadeSettings currentFadeSettings;

	private SkyboxFadeSettings lastFadeSettings;

	private SkyboxFadeSettings targetFadeSettings;

	private Material skybox;

	private bool exitRoutine;

	private void Start()
	{
		skybox = RenderSettings.skybox;
		lastFadeSettings = new SkyboxFadeSettings();
		currentFadeSettings = defaultSkyboxSettings.Copy();
		currentFadeSettings = new SkyboxFadeSettings(skybox);
		skybox.SetColor("_Tint", defaultSkyboxSettings.tint);
		skybox.SetFloat("_Exposure", defaultSkyboxSettings.exposure);
		skybox.SetFloat("_Rotation", defaultSkyboxSettings.rotation);
	}

	private void OnApplicationQuit()
	{
		skybox.SetColor("_Tint", defaultSkyboxSettings.tint);
		skybox.SetFloat("_Exposure", defaultSkyboxSettings.exposure);
		skybox.SetFloat("_Rotation", defaultSkyboxSettings.rotation);
	}

	public void Fade(string _fadeName)
	{
		targetFadeSettings = fadeSettings.Find((SkyboxFadeSettings f) => f.name.Equals(_fadeName));
		if (targetFadeSettings != null)
		{
			exitRoutine = true;
			StartCoroutine(FadeRoutine());
		}
	}

	public void FadeToDefault()
	{
		targetFadeSettings = defaultSkyboxSettings.Copy();
		exitRoutine = true;
		StartCoroutine(FadeRoutine());
	}

	private IEnumerator FadeRoutine()
	{
		yield return new WaitForEndOfFrame();
		exitRoutine = false;
		lastFadeSettings.tint = currentFadeSettings.tint;
		lastFadeSettings.exposure = currentFadeSettings.exposure;
		lastFadeSettings.rotation = currentFadeSettings.rotation;
		float timer = 0f;
		if (lastFadeSettings.tint == targetFadeSettings.tint && lastFadeSettings.exposure == targetFadeSettings.exposure && targetFadeSettings.rotation == lastFadeSettings.rotation)
		{
			yield break;
		}
		do
		{
			currentFadeSettings.tint = Color.Lerp(lastFadeSettings.tint, targetFadeSettings.tint, targetFadeSettings.curve.Evaluate(timer));
			currentFadeSettings.exposure = Mathf.Lerp(lastFadeSettings.exposure, targetFadeSettings.exposure, targetFadeSettings.curve.Evaluate(timer));
			currentFadeSettings.rotation = Mathf.Lerp(lastFadeSettings.rotation, targetFadeSettings.rotation, targetFadeSettings.curve.Evaluate(timer));
			skybox.SetColor("_Tint", currentFadeSettings.tint);
			skybox.SetFloat("_Exposure", currentFadeSettings.exposure);
			skybox.SetFloat("_Rotation", currentFadeSettings.rotation);
			yield return null;
			if (!(timer >= 1f))
			{
				timer += Time.fixedDeltaTime / targetFadeSettings.fadeTime;
				continue;
			}
			break;
		}
		while (!exitRoutine);
	}
}
