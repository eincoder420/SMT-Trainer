using UnityEngine;

namespace AmplifyOcclusion;

public static class PropertyID
{
	public static readonly int _MainTex = Shader.PropertyToID("_MainTex");

	public static readonly int _AO_Radius = Shader.PropertyToID("_AO_Radius");

	public static readonly int _AO_PowExponent = Shader.PropertyToID("_AO_PowExponent");

	public static readonly int _AO_Bias = Shader.PropertyToID("_AO_Bias");

	public static readonly int _AO_Levels = Shader.PropertyToID("_AO_Levels");

	public static readonly int _AO_ThicknessDecay = Shader.PropertyToID("_AO_ThicknessDecay");

	public static readonly int _AO_BlurSharpness = Shader.PropertyToID("_AO_BlurSharpness");

	public static readonly int _AO_BufDepthToLinearEye = Shader.PropertyToID("_AO_BufDepthToLinearEye");

	public static readonly int _AO_CameraViewLeft = Shader.PropertyToID("_AO_CameraViewLeft");

	public static readonly int _AO_CameraViewRight = Shader.PropertyToID("_AO_CameraViewRight");

	public static readonly int _AO_ProjMatrixLeft = Shader.PropertyToID("_AO_ProjMatrixLeft");

	public static readonly int _AO_ProjMatrixRight = Shader.PropertyToID("_AO_ProjMatrixRight");

	public static readonly int _AO_InvViewProjMatrixLeft = Shader.PropertyToID("_AO_InvViewProjMatrixLeft");

	public static readonly int _AO_PrevViewProjMatrixLeft = Shader.PropertyToID("_AO_PrevViewProjMatrixLeft");

	public static readonly int _AO_PrevInvViewProjMatrixLeft = Shader.PropertyToID("_AO_PrevInvViewProjMatrixLeft");

	public static readonly int _AO_InvViewProjMatrixRight = Shader.PropertyToID("_AO_InvViewProjMatrixRight");

	public static readonly int _AO_PrevViewProjMatrixRight = Shader.PropertyToID("_AO_PrevViewProjMatrixRight");

	public static readonly int _AO_PrevInvViewProjMatrixRight = Shader.PropertyToID("_AO_PrevInvViewProjMatrixRight");

	public static readonly int _AO_GBufferNormals = Shader.PropertyToID("_AO_GBufferNormals");

	public static readonly int _AO_Target_TexelSize = Shader.PropertyToID("_AO_Target_TexelSize");

	public static readonly int _AO_TemporalCurveAdj = Shader.PropertyToID("_AO_TemporalCurveAdj");

	public static readonly int _AO_TemporalMotionSensibility = Shader.PropertyToID("_AO_TemporalMotionSensibility");

	public static readonly int _AO_CurrOcclusionDepth = Shader.PropertyToID("_AO_CurrOcclusionDepth");

	public static readonly int _AO_CurrOcclusionDepth_TexelSize = Shader.PropertyToID("_AO_CurrOcclusionDepth_TexelSize");

	public static readonly int _AO_TemporalAccumm = Shader.PropertyToID("_AO_TemporalAccumm");

	public static readonly int _AO_TemporalDirections = Shader.PropertyToID("_AO_TemporalDirections");

	public static readonly int _AO_TemporalOffsets = Shader.PropertyToID("_AO_TemporalOffsets");

	public static readonly int _AO_GBufferAlbedo = Shader.PropertyToID("_AO_GBufferAlbedo");

	public static readonly int _AO_GBufferEmission = Shader.PropertyToID("_AO_GBufferEmission");

	public static readonly int _AO_UVToView = Shader.PropertyToID("_AO_UVToView");

	public static readonly int _AO_HalfProjScale = Shader.PropertyToID("_AO_HalfProjScale");

	public static readonly int _AO_FadeParams = Shader.PropertyToID("_AO_FadeParams");

	public static readonly int _AO_FadeValues = Shader.PropertyToID("_AO_FadeValues");

	public static readonly int _AO_FadeToTint = Shader.PropertyToID("_AO_FadeToTint");

	public static readonly int _AO_CurrMotionIntensity = Shader.PropertyToID("_AO_CurrMotionIntensity");

	public static readonly int _AO_CurrDepthSource_TexelSize = Shader.PropertyToID("_AO_CurrDepthSource_TexelSize");

	public static readonly int _AO_CurrDepthSource = Shader.PropertyToID("_AO_CurrDepthSource");

	public static readonly int _AO_CurrMotionIntensity_TexelSize = Shader.PropertyToID("_AO_CurrMotionIntensity_TexelSize");

	public static readonly int _AO_SourceDepthMipmap = Shader.PropertyToID("_AO_SourceDepthMipmap");

	public static readonly int _AO_Source_TexelSize = Shader.PropertyToID("_AO_Source_TexelSize");
}
