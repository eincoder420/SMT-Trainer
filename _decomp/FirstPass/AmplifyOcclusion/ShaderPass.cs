namespace AmplifyOcclusion;

public static class ShaderPass
{
	public const int OcclusionLow_None_UseDynamicDepthMips = 16;

	public const int CombineDownsampledOcclusionDepth = 32;

	public const int NeighborMotionIntensity = 33;

	public const int ClearTemporal = 34;

	public const int ScaleDownCloserDepthEven = 35;

	public const int ScaleDownCloserDepthEven_CameraDepthTexture = 36;

	public const int Temporal = 37;

	public const int BlurHorizontal1 = 0;

	public const int BlurVertical1 = 1;

	public const int BlurHorizontal2 = 2;

	public const int BlurVertical2 = 3;

	public const int BlurHorizontal3 = 4;

	public const int BlurVertical3 = 5;

	public const int BlurHorizontal4 = 6;

	public const int BlurVertical4 = 7;

	public const int BlurHorizontalIntensity = 8;

	public const int BlurVerticalIntensity = 9;

	public const int ApplyDebug = 0;

	public const int ApplyDebugTemporal = 1;

	public const int ApplyDeferred = 3;

	public const int ApplyDeferredTemporal = 4;

	public const int ApplyDeferredLog = 6;

	public const int ApplyDeferredLogTemporal = 7;

	public const int ApplyPostEffect = 9;

	public const int ApplyPostEffectTemporal = 10;

	public const int ApplyPostEffectTemporalMultiply = 12;

	public const int ApplyDeferredTemporalMultiply = 13;

	public const int ApplyDebugCombineFromTemporal = 14;

	public const int ApplyCombineFromTemporal = 15;

	public const int ApplyDeferredCombineFromTemporal = 16;

	public const int ApplyDeferredLogCombineFromTemporal = 17;

	public const int OcclusionLow_None = 0;

	public const int OcclusionLow_Camera = 1;

	public const int OcclusionLow_GBuffer = 2;

	public const int OcclusionLow_GBufferOctaEncoded = 3;
}
