using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace AmplifyOcclusion;

public static class AmplifyOcclusionCommon
{
	public static readonly int PerPixelNormalSourceCount = 4;

	public static readonly float[] m_temporalRotations = new float[6] { 60f, 300f, 180f, 240f, 120f, 0f };

	public static readonly float[] m_spatialOffsets = new float[4] { 0f, 0.5f, 0.25f, 0.75f };

	public static void CommandBuffer_TemporalFilterDirectionsOffsets(CommandBuffer cb, uint aSampleStep)
	{
		float num = m_temporalRotations[aSampleStep % 6];
		float value = m_spatialOffsets[aSampleStep / 6 % 4];
		cb.SetGlobalFloat(PropertyID._AO_TemporalDirections, num / 360f);
		cb.SetGlobalFloat(PropertyID._AO_TemporalOffsets, value);
	}

	public static Material CreateMaterialWithShaderName(string aShaderName, bool aThroughErrorMsg)
	{
		Shader shader = Shader.Find(aShaderName);
		if (shader == null)
		{
			if (aThroughErrorMsg)
			{
				Debug.LogErrorFormat("[AmplifyOcclusion] Cannot find shader: \"{0}\" Please contact support@amplify.pt", aShaderName);
			}
			return null;
		}
		return new Material(shader)
		{
			hideFlags = HideFlags.DontSave
		};
	}

	public static int SafeAllocateTemporaryRT(CommandBuffer cb, string propertyName, int width, int height, RenderTextureFormat format = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default, FilterMode filterMode = FilterMode.Point)
	{
		int num = Shader.PropertyToID(propertyName);
		cb.GetTemporaryRT(num, width, height, 0, filterMode, format, readWrite);
		return num;
	}

	public static void SafeReleaseTemporaryRT(CommandBuffer cb, int id)
	{
		cb.ReleaseTemporaryRT(id);
	}

	public static RenderTexture SafeAllocateRT(string name, int width, int height, RenderTextureFormat format, RenderTextureReadWrite readWrite, FilterMode filterMode = FilterMode.Point, int antiAliasing = 1, bool aUseMipMap = false)
	{
		width = Mathf.Clamp(width, 1, 65536);
		height = Mathf.Clamp(height, 1, 65536);
		RenderTexture renderTexture = new RenderTexture(width, height, 0, format, readWrite);
		renderTexture.hideFlags = HideFlags.DontSave;
		renderTexture.name = name;
		renderTexture.filterMode = filterMode;
		renderTexture.wrapMode = TextureWrapMode.Clamp;
		renderTexture.antiAliasing = Mathf.Max(antiAliasing, 1);
		renderTexture.useMipMap = aUseMipMap;
		renderTexture.Create();
		return renderTexture;
	}

	public static void SafeReleaseRT(ref RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.active = null;
			rt.Release();
			UnityEngine.Object.DestroyImmediate(rt);
			rt = null;
		}
	}

	public static bool IsStereoSinglePassEnabled(Camera aCamera)
	{
		if (aCamera.stereoEnabled)
		{
			return XRSettings.eyeTextureDesc.vrUsage == VRTextureUsage.TwoEyes;
		}
		return false;
	}

	public static bool IsStereoMultiPassEnabled(Camera aCamera)
	{
		if (aCamera.stereoEnabled)
		{
			return XRSettings.eyeTextureDesc.vrUsage == VRTextureUsage.OneEye;
		}
		return false;
	}

	public static void UpdateGlobalShaderConstants(CommandBuffer cb, ref TargetDesc aTarget, Camera aCamera, bool isDownsample, bool isFilterDownsample)
	{
		if (XRSettings.enabled)
		{
			aTarget.fullWidth = (int)((float)XRSettings.eyeTextureDesc.width * XRSettings.eyeTextureResolutionScale);
			aTarget.fullHeight = (int)((float)XRSettings.eyeTextureDesc.height * XRSettings.eyeTextureResolutionScale);
		}
		else
		{
			aTarget.fullWidth = aCamera.pixelWidth;
			aTarget.fullHeight = aCamera.pixelHeight;
		}
		if (isFilterDownsample)
		{
			aTarget.width = aTarget.fullWidth / 2;
			aTarget.height = aTarget.fullHeight / 2;
		}
		else
		{
			aTarget.width = aTarget.fullWidth;
			aTarget.height = aTarget.fullHeight;
		}
		aTarget.oneOverWidth = 1f / (float)aTarget.width;
		aTarget.oneOverHeight = 1f / (float)aTarget.height;
		float num = aCamera.fieldOfView * ((float)Math.PI / 180f);
		float num2 = 1f / Mathf.Tan(num * 0.5f);
		Vector2 vector = new Vector2(num2 * ((float)aTarget.height / (float)aTarget.width), num2);
		Vector2 vector2 = new Vector2(1f / vector.x, 1f / vector.y);
		cb.SetGlobalVector(PropertyID._AO_UVToView, new Vector4(2f * vector2.x, 2f * vector2.y, -1f * vector2.x, -1f * vector2.y));
		float num3 = ((!aCamera.orthographic) ? ((float)aTarget.fullHeight / (Mathf.Tan(num * 0.5f) * 2f)) : ((float)aTarget.fullHeight / aCamera.orthographicSize));
		num3 = ((!isDownsample && !isFilterDownsample) ? (num3 * 0.5f) : (num3 * 0.5f * 0.5f));
		cb.SetGlobalFloat(PropertyID._AO_HalfProjScale, num3);
	}
}
