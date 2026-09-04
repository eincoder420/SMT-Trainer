namespace UnityEngine.AzureSky;

public sealed class AzureSkySettings
{
	public float MolecularDensity = 2.545f;

	public Vector3 Wavelength = new Vector3(680f, 550f, 450f);

	public float Rayleigh = 1.5f;

	public float Mie = 1f;

	public float Scattering = 0.25f;

	public float Luminance = 1.5f;

	public float Exposure = 2f;

	public Color RayleighColor = Color.white;

	public Color MieColor = Color.white;

	public Color ScatteringColor = Color.white;

	public float SunTextureSize = 1.5f;

	public float SunTextureIntensity = 1f;

	public Color SunTextureColor = Color.white;

	public float MoonTextureSize = 1.5f;

	public float MoonTextureIntensity = 1f;

	public Color MoonTextureColor = Color.white;

	public float StarsIntensity = 0.5f;

	public float MilkyWayIntensity;

	public float FogScatteringScale = 1f;

	public float GlobalFogDistance = 1000f;

	public float GlobalFogSmooth = 0.25f;

	public float GlobalFogDensity = 1f;

	public float HeightFogDistance = 100f;

	public float HeightFogSmooth = 1f;

	public float HeightFogDensity;

	public float HeightFogStart;

	public float HeightFogEnd = 100f;

	public float StaticCloudInterpolator;

	public float StaticCloudLayer1Speed;

	public float StaticCloudLayer2Speed;

	public Color StaticCloudColor = Color.white;

	public float StaticCloudScattering = 1f;

	public float StaticCloudExtinction = 1.5f;

	public float StaticCloudSaturation = 2.5f;

	public float StaticCloudOpacity = 1.25f;

	public float DynamicCloudAltitude = 7.5f;

	public float DynamicCloudDirection = 1f;

	public float DynamicCloudSpeed = 0.1f;

	public float DynamicCloudDensity = 0.75f;

	public Color DynamicCloudColor1 = Color.white;

	public Color DynamicCloudColor2 = Color.white;

	public float DirectionalLightIntensity = 1f;

	public Color DirectionalLightColor = Color.white;

	public float EnvironmentIntensity = 1f;

	public Color EnvironmentAmbientColor = Color.white;

	public Color EnvironmentEquatorColor = Color.white;

	public Color EnvironmentGroundColor = Color.white;

	public float LightRainIntensity;

	public float MediumRainIntensity;

	public float HeavyRainIntensity;

	public float SnowIntensity;

	public Color RainColor = Color.white;

	public Color SnowColor = Color.white;

	public float LightRainSoundVolume;

	public float MediumRainSoundVolume;

	public float HeavyRainSoundVolume;

	public float LightWindSoundVolume;

	public float MediumWindSoundVolume;

	public float HeavyWindSoundVolume;

	public float WindSpeed;

	public float WindDirection;
}
