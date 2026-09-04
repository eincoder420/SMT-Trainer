using AmplifyOcclusion;
using UnityEngine;

[ExecuteInEditMode]
public class Controls : MonoBehaviour
{
	public AmplifyOcclusionEffect occlusion;

	private const AmplifyOcclusionEffect.ApplicationMethod POST = AmplifyOcclusionEffect.ApplicationMethod.PostEffect;

	private const AmplifyOcclusionEffect.ApplicationMethod DEFERRED = AmplifyOcclusionEffect.ApplicationMethod.Deferred;

	private const AmplifyOcclusionEffect.ApplicationMethod DEBUG = AmplifyOcclusionEffect.ApplicationMethod.Debug;

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
		GUILayout.BeginHorizontal();
		GUILayout.Space(5f);
		GUILayout.BeginVertical();
		occlusion.enabled = GUILayout.Toggle(occlusion.enabled, " Amplify Occlusion Enabled");
		GUILayout.Space(5f);
		occlusion.ApplyMethod = ((!GUILayout.Toggle(occlusion.ApplyMethod == AmplifyOcclusionEffect.ApplicationMethod.PostEffect, " Standard Post-effect")) ? occlusion.ApplyMethod : AmplifyOcclusionEffect.ApplicationMethod.PostEffect);
		occlusion.ApplyMethod = (GUILayout.Toggle(occlusion.ApplyMethod == AmplifyOcclusionEffect.ApplicationMethod.Deferred, " Deferred Injection") ? AmplifyOcclusionEffect.ApplicationMethod.Deferred : occlusion.ApplyMethod);
		occlusion.ApplyMethod = (GUILayout.Toggle(occlusion.ApplyMethod == AmplifyOcclusionEffect.ApplicationMethod.Debug, " Debug Mode") ? AmplifyOcclusionEffect.ApplicationMethod.Debug : occlusion.ApplyMethod);
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();
		GUILayout.Space(5f);
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		GUILayout.Space(-3f);
		GUILayout.Label("Intensity     ");
		GUILayout.EndVertical();
		occlusion.Intensity = GUILayout.HorizontalSlider(occlusion.Intensity, 0f, 1f, GUILayout.Width(100f));
		GUILayout.Space(5f);
		GUILayout.BeginVertical();
		GUILayout.Space(-3f);
		GUILayout.Label(" " + occlusion.Intensity.ToString("0.00"));
		GUILayout.EndVertical();
		GUILayout.Space(5f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		GUILayout.Space(-3f);
		GUILayout.Label("Power Exp. ");
		GUILayout.EndVertical();
		occlusion.PowerExponent = GUILayout.HorizontalSlider(occlusion.PowerExponent, 0.0001f, 6f, GUILayout.Width(100f));
		GUILayout.Space(5f);
		GUILayout.BeginVertical();
		GUILayout.Space(-3f);
		GUILayout.Label(" " + occlusion.PowerExponent.ToString("0.00"));
		GUILayout.EndVertical();
		GUILayout.Space(5f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		GUILayout.Space(-3f);
		GUILayout.Label("Radius        ");
		GUILayout.EndVertical();
		occlusion.Radius = GUILayout.HorizontalSlider(occlusion.Radius, 0.1f, 10f, GUILayout.Width(100f));
		GUILayout.Space(5f);
		GUILayout.BeginVertical();
		GUILayout.Space(-3f);
		GUILayout.Label(" " + occlusion.Radius.ToString("0.00"));
		GUILayout.EndVertical();
		GUILayout.Space(5f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		GUILayout.Space(-3f);
		GUILayout.Label("Quality        ");
		GUILayout.EndVertical();
		occlusion.SampleCount = (SampleCountLevel)GUILayout.HorizontalSlider((float)occlusion.SampleCount, 0f, 3f, GUILayout.Width(100f));
		GUILayout.Space(5f);
		GUILayout.BeginVertical();
		GUILayout.Space(-3f);
		GUILayout.Label("        ");
		GUILayout.EndVertical();
		GUILayout.Space(5f);
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
