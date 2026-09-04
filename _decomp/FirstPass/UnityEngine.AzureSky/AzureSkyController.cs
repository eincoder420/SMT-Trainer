using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.AzureSky;

[ExecuteInEditMode]
[AddComponentMenu("Azure[Sky]/Azure Sky Controller")]
public class AzureSkyController : MonoBehaviour
{
	public Transform sunTransform;

	public Transform moonTransform;

	public Light directionalLight;

	public Material skyMaterial;

	public Material fogMaterial;

	public Shader emptySkyShader;

	public Shader staticCloudShader;

	public Shader dynamicCloudShader;

	public AzureSkySettings settings = new AzureSkySettings();

	public float timeOfDay;

	public float sunElevation;

	public float moonElevation;

	public AzureScatteringMode scatteringMode;

	public AzureCloudMode cloudMode;

	public AzureShaderUpdateMode shaderUpdateMode;

	public Vector3 starFieldPosition = Vector3.zero;

	public Vector3 starFieldColor = Vector3.one;

	public float dayTransitionTime;

	public float mieDepth = 1f;

	public AzureSkyProfile defaultProfile;

	public AzureSkyProfile currentProfile;

	public AzureSkyProfile targetProfile;

	private AzureSkyProfile m_nextDayProfile;

	public List<AzureSkyProfile> defaultProfileList = new List<AzureSkyProfile>();

	public List<AzureGlobalWeather> globalWeatherList = new List<AzureGlobalWeather>();

	public List<AzureWeatherZone> weatherZoneList = new List<AzureWeatherZone>();

	public float globalWeatherTransitionProgress;

	public float globalWeatherTransitionTime;

	public float globalWeatherStartTransitionTime;

	public float defaultWeatherTransitionTime = 10f;

	public int globalWeatherIndex = -1;

	public bool isGlobalWeatherChanging;

	public Transform weatherZoneTrigger;

	private Vector3 m_weatherZoneTriggerPosition;

	private Vector3 m_weatherZoneClosestPoint;

	private float m_weatherZoneClosestDistanceSqr;

	private float m_weatherZoneDistance;

	private float m_weatherZoneBlendDistanceSqr;

	private float m_weatherZoneInterpolationFactor;

	private Collider m_weatherZoneCollider;

	public UnityEvent onMinuteChange = new UnityEvent();

	public UnityEvent onHourChange = new UnityEvent();

	public UnityEvent onDayChange = new UnityEvent();

	public Texture2D staticCloudSource;

	public Texture2D staticCloudTarget;

	private Vector2 m_dynamicCloudDirection;

	private float m_staticCloudLayer1Speed;

	private float m_staticCloudLayer2Speed;

	public AzureOutputProfile outputProfile;

	private AzureOutputType m_outputType;

	private void OnEnable()
	{
		if ((bool)skyMaterial)
		{
			RenderSettings.skybox = skyMaterial;
		}
	}

	private void Start()
	{
		m_dynamicCloudDirection = Vector2.zero;
		globalWeatherIndex = -1;
		defaultProfile = defaultProfileList[0];
		currentProfile = defaultProfile;
		targetProfile = defaultProfile;
		UpdateMaterialSettings();
		UpdateProfiles();
		if (shaderUpdateMode == AzureShaderUpdateMode.ByMaterial)
		{
			UpdateSkySettings(skyMaterial);
			UpdateSkySettings(fogMaterial);
		}
		else
		{
			UpdateSkySettings();
		}
	}

	private void Update()
	{
		m_dynamicCloudDirection = ComputeCloudPosition();
		m_staticCloudLayer1Speed += settings.StaticCloudLayer1Speed * Time.deltaTime;
		m_staticCloudLayer2Speed += settings.StaticCloudLayer2Speed * Time.deltaTime;
		if (m_staticCloudLayer1Speed >= 1f)
		{
			m_staticCloudLayer1Speed -= 1f;
		}
		if (m_staticCloudLayer2Speed >= 1f)
		{
			m_staticCloudLayer2Speed -= 1f;
		}
		skyMaterial.SetVector(AzureShaderUniforms.DynamicCloudDirection, m_dynamicCloudDirection);
		skyMaterial.SetFloat(AzureShaderUniforms.StaticCloudLayer1Speed, m_staticCloudLayer1Speed);
		skyMaterial.SetFloat(AzureShaderUniforms.StaticCloudLayer2Speed, m_staticCloudLayer2Speed);
		UpdateProfiles();
		if (shaderUpdateMode == AzureShaderUpdateMode.ByMaterial)
		{
			UpdateSkySettings(skyMaterial);
			UpdateSkySettings(fogMaterial);
		}
		else
		{
			UpdateSkySettings();
		}
		directionalLight.intensity = settings.DirectionalLightIntensity;
		directionalLight.color = settings.DirectionalLightColor;
		RenderSettings.ambientIntensity = settings.EnvironmentIntensity;
		RenderSettings.ambientLight = settings.EnvironmentAmbientColor;
		RenderSettings.ambientSkyColor = settings.EnvironmentAmbientColor;
		RenderSettings.ambientEquatorColor = settings.EnvironmentEquatorColor;
		RenderSettings.ambientGroundColor = settings.EnvironmentGroundColor;
	}

	public void SetNewWeatherProfile(int index)
	{
		if (index == -1)
		{
			if ((bool)defaultProfile)
			{
				targetProfile = defaultProfile;
				globalWeatherTransitionTime = defaultWeatherTransitionTime;
				globalWeatherIndex = index;
			}
		}
		else if ((bool)globalWeatherList[index].profile)
		{
			targetProfile = globalWeatherList[index].profile;
			globalWeatherTransitionTime = globalWeatherList[index].transitionTime;
			globalWeatherIndex = index;
		}
		globalWeatherTransitionProgress = 0f;
		globalWeatherStartTransitionTime = Time.time;
		isGlobalWeatherChanging = true;
	}

	public void PerformDayTransition()
	{
		m_nextDayProfile = defaultProfileList[Random.Range(0, defaultProfileList.Count)];
		defaultProfile = m_nextDayProfile;
		if (m_nextDayProfile != currentProfile && globalWeatherIndex < 0)
		{
			if (dayTransitionTime > 0f)
			{
				SetNewDayProfile(m_nextDayProfile, dayTransitionTime);
			}
			else
			{
				SetNewDayProfile(m_nextDayProfile);
			}
		}
	}

	public void OnDayChange()
	{
		onDayChange?.Invoke();
		PerformDayTransition();
	}

	public void SetNewDayProfile(AzureSkyProfile profile, float transitionTime)
	{
		targetProfile = profile;
		globalWeatherTransitionTime = transitionTime;
		globalWeatherTransitionProgress = 0f;
		globalWeatherStartTransitionTime = Time.time;
		isGlobalWeatherChanging = true;
	}

	public void SetNewDayProfile(AzureSkyProfile profile)
	{
		currentProfile = profile;
	}

	public float GetOutputFloatValue(int index)
	{
		if ((bool)outputProfile)
		{
			m_outputType = outputProfile.outputList[index].type;
			if (m_outputType == AzureOutputType.Slider || m_outputType == AzureOutputType.TimelineCurve || m_outputType == AzureOutputType.SunCurve || m_outputType == AzureOutputType.MoonCurve)
			{
				return outputProfile.outputList[index].floatOutput;
			}
			Debug.LogWarning("You are trying to get a float output, but the output type is set to " + m_outputType);
		}
		return 0f;
	}

	public Color GetOutputColorValue(int index)
	{
		if ((bool)outputProfile)
		{
			m_outputType = outputProfile.outputList[index].type;
			if (m_outputType == AzureOutputType.Color || m_outputType == AzureOutputType.TimelineGradient || m_outputType == AzureOutputType.SunGradient || m_outputType == AzureOutputType.MoonGradient)
			{
				return outputProfile.outputList[index].colorOutput;
			}
			Debug.LogWarning("You are trying to get a color output, but the output type is set to " + m_outputType);
		}
		return Color.black;
	}

	public void UpdateMaterialSettings()
	{
		switch (cloudMode)
		{
		case AzureCloudMode.EmptySky:
			skyMaterial.shader = emptySkyShader;
			break;
		case AzureCloudMode.StaticClouds:
			skyMaterial.shader = staticCloudShader;
			break;
		case AzureCloudMode.DynamicClouds:
			skyMaterial.shader = dynamicCloudShader;
			break;
		}
	}

	public void UpdateSkySettings(Material mat)
	{
		mat.SetInt(AzureShaderUniforms.ScatteringMode, (int)scatteringMode);
		mat.SetVector(AzureShaderUniforms.Rayleigh, ComputeRayleigh() * settings.Rayleigh);
		mat.SetVector(AzureShaderUniforms.Mie, ComputeMie() * settings.Mie);
		mat.SetFloat(AzureShaderUniforms.Scattering, settings.Scattering * 60f);
		mat.SetFloat(AzureShaderUniforms.Luminance, settings.Luminance);
		mat.SetFloat(AzureShaderUniforms.Exposure, settings.Exposure);
		mat.SetVector(AzureShaderUniforms.RayleighColor, settings.RayleighColor);
		mat.SetVector(AzureShaderUniforms.MieColor, settings.MieColor);
		mat.SetVector(AzureShaderUniforms.ScatteringColor, settings.ScatteringColor);
		mat.SetFloat(AzureShaderUniforms.SunTextureSize, settings.SunTextureSize);
		mat.SetFloat(AzureShaderUniforms.SunTextureIntensity, settings.SunTextureIntensity);
		mat.SetVector(AzureShaderUniforms.SunTextureColor, settings.SunTextureColor);
		mat.SetFloat(AzureShaderUniforms.MoonTextureSize, settings.MoonTextureSize);
		mat.SetFloat(AzureShaderUniforms.MoonTextureIntensity, settings.MoonTextureIntensity);
		mat.SetVector(AzureShaderUniforms.MoonTextureColor, settings.MoonTextureColor);
		mat.SetFloat(AzureShaderUniforms.StarsIntensity, settings.StarsIntensity);
		mat.SetFloat(AzureShaderUniforms.MilkyWayIntensity, settings.MilkyWayIntensity);
		mat.SetVector(AzureShaderUniforms.StarFieldColor, starFieldColor);
		mat.SetFloat(AzureShaderUniforms.FogScatteringScale, settings.FogScatteringScale);
		mat.SetFloat(AzureShaderUniforms.GlobalFogDistance, settings.GlobalFogDistance);
		mat.SetFloat(AzureShaderUniforms.GlobalFogSmooth, settings.GlobalFogSmooth);
		mat.SetFloat(AzureShaderUniforms.GlobalFogDensity, settings.GlobalFogDensity);
		mat.SetFloat(AzureShaderUniforms.HeightFogDistance, settings.HeightFogDistance);
		mat.SetFloat(AzureShaderUniforms.HeightFogSmooth, settings.HeightFogSmooth);
		mat.SetFloat(AzureShaderUniforms.HeightFogDensity, settings.HeightFogDensity);
		mat.SetFloat(AzureShaderUniforms.HeightFogStart, settings.HeightFogStart);
		mat.SetFloat(AzureShaderUniforms.HeightFogEnd, settings.HeightFogEnd);
		mat.SetFloat(AzureShaderUniforms.MieDepth, mieDepth);
		mat.SetTexture(AzureShaderUniforms.StaticCloudSourceTexture, staticCloudSource);
		mat.SetTexture(AzureShaderUniforms.StaticCloudTargetTexture, staticCloudTarget);
		mat.SetFloat(AzureShaderUniforms.StaticCloudInterpolator, settings.StaticCloudInterpolator);
		mat.SetFloat(AzureShaderUniforms.StaticCloudScattering, settings.StaticCloudScattering);
		mat.SetFloat(AzureShaderUniforms.StaticCloudExtinction, settings.StaticCloudExtinction);
		mat.SetFloat(AzureShaderUniforms.StaticCloudSaturation, settings.StaticCloudSaturation);
		mat.SetFloat(AzureShaderUniforms.StaticCloudOpacity, settings.StaticCloudOpacity);
		mat.SetVector(AzureShaderUniforms.StaticCloudColor, settings.StaticCloudColor);
		mat.SetFloat(AzureShaderUniforms.DynamicCloudAltitude, settings.DynamicCloudAltitude);
		mat.SetFloat(AzureShaderUniforms.DynamicCloudDensity, Mathf.Lerp(25f, 0f, settings.DynamicCloudDensity));
		mat.SetVector(AzureShaderUniforms.DynamicCloudColor1, settings.DynamicCloudColor1);
		mat.SetVector(AzureShaderUniforms.DynamicCloudColor2, settings.DynamicCloudColor2);
	}

	public void UpdateSkySettings()
	{
		Shader.SetGlobalInt(AzureShaderUniforms.ScatteringMode, (int)scatteringMode);
		Shader.SetGlobalVector(AzureShaderUniforms.Rayleigh, ComputeRayleigh() * settings.Rayleigh);
		Shader.SetGlobalVector(AzureShaderUniforms.Mie, ComputeMie() * settings.Mie);
		Shader.SetGlobalFloat(AzureShaderUniforms.Scattering, settings.Scattering * 60f);
		Shader.SetGlobalFloat(AzureShaderUniforms.Luminance, settings.Luminance);
		Shader.SetGlobalFloat(AzureShaderUniforms.Exposure, settings.Exposure);
		Shader.SetGlobalVector(AzureShaderUniforms.RayleighColor, settings.RayleighColor);
		Shader.SetGlobalVector(AzureShaderUniforms.MieColor, settings.MieColor);
		Shader.SetGlobalVector(AzureShaderUniforms.ScatteringColor, settings.ScatteringColor);
		Shader.SetGlobalFloat(AzureShaderUniforms.SunTextureSize, settings.SunTextureSize);
		Shader.SetGlobalFloat(AzureShaderUniforms.SunTextureIntensity, settings.SunTextureIntensity);
		Shader.SetGlobalVector(AzureShaderUniforms.SunTextureColor, settings.SunTextureColor);
		Shader.SetGlobalFloat(AzureShaderUniforms.MoonTextureSize, settings.MoonTextureSize);
		Shader.SetGlobalFloat(AzureShaderUniforms.MoonTextureIntensity, settings.MoonTextureIntensity);
		Shader.SetGlobalVector(AzureShaderUniforms.MoonTextureColor, settings.MoonTextureColor);
		Shader.SetGlobalFloat(AzureShaderUniforms.StarsIntensity, settings.StarsIntensity);
		Shader.SetGlobalFloat(AzureShaderUniforms.MilkyWayIntensity, settings.MilkyWayIntensity);
		Shader.SetGlobalVector(AzureShaderUniforms.StarFieldColor, starFieldColor);
		Shader.SetGlobalFloat(AzureShaderUniforms.FogScatteringScale, settings.FogScatteringScale);
		Shader.SetGlobalFloat(AzureShaderUniforms.GlobalFogDistance, settings.GlobalFogDistance);
		Shader.SetGlobalFloat(AzureShaderUniforms.GlobalFogSmooth, settings.GlobalFogSmooth);
		Shader.SetGlobalFloat(AzureShaderUniforms.GlobalFogDensity, settings.GlobalFogDensity);
		Shader.SetGlobalFloat(AzureShaderUniforms.HeightFogDistance, settings.HeightFogDistance);
		Shader.SetGlobalFloat(AzureShaderUniforms.HeightFogSmooth, settings.HeightFogSmooth);
		Shader.SetGlobalFloat(AzureShaderUniforms.HeightFogDensity, settings.HeightFogDensity);
		Shader.SetGlobalFloat(AzureShaderUniforms.HeightFogStart, settings.HeightFogStart);
		Shader.SetGlobalFloat(AzureShaderUniforms.HeightFogEnd, settings.HeightFogEnd);
		Shader.SetGlobalFloat(AzureShaderUniforms.MieDepth, mieDepth);
		Shader.SetGlobalTexture(AzureShaderUniforms.StaticCloudSourceTexture, staticCloudSource);
		Shader.SetGlobalTexture(AzureShaderUniforms.StaticCloudTargetTexture, staticCloudTarget);
		Shader.SetGlobalFloat(AzureShaderUniforms.StaticCloudInterpolator, settings.StaticCloudInterpolator);
		Shader.SetGlobalFloat(AzureShaderUniforms.StaticCloudScattering, settings.StaticCloudScattering);
		Shader.SetGlobalFloat(AzureShaderUniforms.StaticCloudExtinction, settings.StaticCloudExtinction);
		Shader.SetGlobalFloat(AzureShaderUniforms.StaticCloudSaturation, settings.StaticCloudSaturation);
		Shader.SetGlobalFloat(AzureShaderUniforms.StaticCloudOpacity, settings.StaticCloudOpacity);
		Shader.SetGlobalVector(AzureShaderUniforms.StaticCloudColor, settings.StaticCloudColor);
		Shader.SetGlobalFloat(AzureShaderUniforms.DynamicCloudAltitude, settings.DynamicCloudAltitude);
		Shader.SetGlobalFloat(AzureShaderUniforms.DynamicCloudDensity, Mathf.Lerp(25f, 0f, settings.DynamicCloudDensity));
		Shader.SetGlobalVector(AzureShaderUniforms.DynamicCloudColor1, settings.DynamicCloudColor1);
		Shader.SetGlobalVector(AzureShaderUniforms.DynamicCloudColor2, settings.DynamicCloudColor2);
	}

	private void UpdateProfiles()
	{
		if (!isGlobalWeatherChanging)
		{
			GetDefaultSettings();
		}
		else
		{
			globalWeatherTransitionProgress = Mathf.Clamp01((Time.time - globalWeatherStartTransitionTime) / globalWeatherTransitionTime);
			ApplyGlobalWeatherTransition(currentProfile, targetProfile, globalWeatherTransitionProgress);
			if (Math.Abs(globalWeatherTransitionProgress - 1f) <= 0f)
			{
				isGlobalWeatherChanging = false;
				globalWeatherTransitionProgress = 0f;
				globalWeatherStartTransitionTime = 0f;
				currentProfile = targetProfile;
			}
		}
		if (!weatherZoneTrigger)
		{
			return;
		}
		m_weatherZoneTriggerPosition = weatherZoneTrigger.position;
		foreach (AzureWeatherZone weatherZone in weatherZoneList)
		{
			if (weatherZone == null)
			{
				continue;
			}
			m_weatherZoneCollider = weatherZone.GetComponent<Collider>();
			if (!m_weatherZoneCollider || !m_weatherZoneCollider.enabled)
			{
				continue;
			}
			m_weatherZoneClosestDistanceSqr = float.PositiveInfinity;
			m_weatherZoneClosestPoint = m_weatherZoneCollider.ClosestPoint(m_weatherZoneTriggerPosition);
			m_weatherZoneDistance = ((m_weatherZoneClosestPoint - m_weatherZoneTriggerPosition) / 2f).sqrMagnitude;
			if (m_weatherZoneDistance < m_weatherZoneClosestDistanceSqr)
			{
				m_weatherZoneClosestDistanceSqr = m_weatherZoneDistance;
			}
			m_weatherZoneCollider = null;
			m_weatherZoneBlendDistanceSqr = weatherZone.blendDistance * weatherZone.blendDistance;
			if (!(m_weatherZoneClosestDistanceSqr > m_weatherZoneBlendDistanceSqr))
			{
				m_weatherZoneInterpolationFactor = 1f;
				if (m_weatherZoneBlendDistanceSqr > 0f)
				{
					m_weatherZoneInterpolationFactor = 1f - m_weatherZoneClosestDistanceSqr / m_weatherZoneBlendDistanceSqr;
				}
				ApplyWeatherZonesInfluence(weatherZone.profile, m_weatherZoneInterpolationFactor);
			}
		}
	}

	private void GetDefaultSettings()
	{
		settings.MolecularDensity = currentProfile.molecularDensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.Wavelength.x = currentProfile.wavelengthR.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.Wavelength.y = currentProfile.wavelengthG.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.Wavelength.z = currentProfile.wavelengthB.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.Rayleigh = currentProfile.rayleigh.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.Mie = currentProfile.mie.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.Scattering = currentProfile.scattering.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.Luminance = currentProfile.luminance.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.Exposure = currentProfile.exposure.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.RayleighColor = currentProfile.rayleighColor.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.MieColor = currentProfile.mieColor.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.ScatteringColor = currentProfile.scatteringColor.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.SunTextureSize = currentProfile.sunTextureSize.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.SunTextureIntensity = currentProfile.sunTextureIntensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.SunTextureColor = currentProfile.sunTextureColor.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.MoonTextureSize = currentProfile.moonTextureSize.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.MoonTextureIntensity = currentProfile.moonTextureIntensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.MoonTextureColor = currentProfile.moonTextureColor.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.StarsIntensity = currentProfile.starsIntensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.MilkyWayIntensity = currentProfile.milkyWayIntensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.FogScatteringScale = currentProfile.fogScatteringScale.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.GlobalFogDistance = currentProfile.globalFogDistance.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.GlobalFogSmooth = currentProfile.globalFogSmooth.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.GlobalFogDensity = currentProfile.globalFogDensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.HeightFogDistance = currentProfile.heightFogDistance.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.HeightFogSmooth = currentProfile.heightFogSmooth.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.HeightFogDensity = currentProfile.heightFogDensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.HeightFogStart = currentProfile.heightFogStart.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.HeightFogEnd = currentProfile.heightFogEnd.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.StaticCloudInterpolator = 0f;
		staticCloudSource = currentProfile.staticCloudTexture;
		staticCloudTarget = currentProfile.staticCloudTexture;
		settings.StaticCloudLayer1Speed = currentProfile.staticCloudLayer1Speed.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.StaticCloudLayer2Speed = currentProfile.staticCloudLayer2Speed.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.StaticCloudColor = currentProfile.staticCloudColor.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.StaticCloudScattering = currentProfile.staticCloudScattering.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.StaticCloudExtinction = currentProfile.staticCloudExtinction.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.StaticCloudSaturation = currentProfile.staticCloudSaturation.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.StaticCloudOpacity = currentProfile.staticCloudOpacity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.DynamicCloudAltitude = currentProfile.dynamicCloudAltitude.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.DynamicCloudDirection = currentProfile.dynamicCloudDirection.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.DynamicCloudSpeed = currentProfile.dynamicCloudSpeed.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.DynamicCloudDensity = currentProfile.dynamicCloudDensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.DynamicCloudColor1 = currentProfile.dynamicCloudColor1.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.DynamicCloudColor2 = currentProfile.dynamicCloudColor2.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.DirectionalLightIntensity = currentProfile.directionalLightIntensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.DirectionalLightColor = currentProfile.directionalLightColor.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.EnvironmentIntensity = currentProfile.environmentIntensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.EnvironmentAmbientColor = currentProfile.environmentAmbientColor.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.EnvironmentEquatorColor = currentProfile.environmentEquatorColor.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.EnvironmentGroundColor = currentProfile.environmentGroundColor.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.LightRainIntensity = currentProfile.lightRainIntensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.MediumRainIntensity = currentProfile.mediumRainIntensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.HeavyRainIntensity = currentProfile.heavyRainIntensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.SnowIntensity = currentProfile.snowIntensity.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.RainColor = currentProfile.rainColor.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.SnowColor = currentProfile.snowColor.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.LightRainSoundVolume = currentProfile.lightRainSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.MediumRainSoundVolume = currentProfile.mediumRainSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.HeavyRainSoundVolume = currentProfile.heavyRainSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.LightWindSoundVolume = currentProfile.lightWindSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.MediumWindSoundVolume = currentProfile.mediumWindSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.HeavyWindSoundVolume = currentProfile.heavyWindSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.WindSpeed = currentProfile.windSpeed.GetValue(timeOfDay, sunElevation, moonElevation);
		settings.WindDirection = currentProfile.windDirection.GetValue(timeOfDay, sunElevation, moonElevation);
		if (!outputProfile || !currentProfile.outputProfile || outputProfile != currentProfile.outputProfile || outputProfile.outputList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < outputProfile.outputList.Count; i++)
		{
			m_outputType = outputProfile.outputList[i].type;
			if (m_outputType == AzureOutputType.Slider || m_outputType == AzureOutputType.TimelineCurve || m_outputType == AzureOutputType.SunCurve || m_outputType == AzureOutputType.MoonCurve)
			{
				outputProfile.outputList[i].floatOutput = currentProfile.outputPropertyList[i].GetFloatValue(timeOfDay, sunElevation, moonElevation);
			}
			else
			{
				outputProfile.outputList[i].colorOutput = currentProfile.outputPropertyList[i].GetColorValue(timeOfDay, sunElevation, moonElevation);
			}
		}
	}

	private void ApplyGlobalWeatherTransition(AzureSkyProfile from, AzureSkyProfile to, float t)
	{
		settings.MolecularDensity = FloatInterpolation(from.molecularDensity.GetValue(timeOfDay, sunElevation, moonElevation), to.molecularDensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Wavelength.x = FloatInterpolation(from.wavelengthR.GetValue(timeOfDay, sunElevation, moonElevation), to.wavelengthR.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Wavelength.y = FloatInterpolation(from.wavelengthG.GetValue(timeOfDay, sunElevation, moonElevation), to.wavelengthG.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Wavelength.z = FloatInterpolation(from.wavelengthB.GetValue(timeOfDay, sunElevation, moonElevation), to.wavelengthB.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Rayleigh = FloatInterpolation(from.rayleigh.GetValue(timeOfDay, sunElevation, moonElevation), to.rayleigh.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Mie = FloatInterpolation(from.mie.GetValue(timeOfDay, sunElevation, moonElevation), to.mie.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Scattering = FloatInterpolation(from.scattering.GetValue(timeOfDay, sunElevation, moonElevation), to.scattering.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Luminance = FloatInterpolation(from.luminance.GetValue(timeOfDay, sunElevation, moonElevation), to.luminance.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Exposure = FloatInterpolation(from.exposure.GetValue(timeOfDay, sunElevation, moonElevation), to.exposure.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.RayleighColor = ColorInterpolation(from.rayleighColor.GetValue(timeOfDay, sunElevation, moonElevation), to.rayleighColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MieColor = ColorInterpolation(from.mieColor.GetValue(timeOfDay, sunElevation, moonElevation), to.mieColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.ScatteringColor = ColorInterpolation(from.scatteringColor.GetValue(timeOfDay, sunElevation, moonElevation), to.scatteringColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.SunTextureSize = FloatInterpolation(from.sunTextureSize.GetValue(timeOfDay, sunElevation, moonElevation), to.sunTextureSize.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.SunTextureIntensity = FloatInterpolation(from.sunTextureIntensity.GetValue(timeOfDay, sunElevation, moonElevation), to.sunTextureIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.SunTextureColor = ColorInterpolation(from.sunTextureColor.GetValue(timeOfDay, sunElevation, moonElevation), to.sunTextureColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MoonTextureSize = FloatInterpolation(from.moonTextureSize.GetValue(timeOfDay, sunElevation, moonElevation), to.moonTextureSize.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MoonTextureIntensity = FloatInterpolation(from.moonTextureIntensity.GetValue(timeOfDay, sunElevation, moonElevation), to.moonTextureIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MoonTextureColor = ColorInterpolation(from.moonTextureColor.GetValue(timeOfDay, sunElevation, moonElevation), to.moonTextureColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StarsIntensity = FloatInterpolation(from.starsIntensity.GetValue(timeOfDay, sunElevation, moonElevation), to.starsIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MilkyWayIntensity = FloatInterpolation(from.milkyWayIntensity.GetValue(timeOfDay, sunElevation, moonElevation), to.milkyWayIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.FogScatteringScale = FloatInterpolation(from.fogScatteringScale.GetValue(timeOfDay, sunElevation, moonElevation), to.fogScatteringScale.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.GlobalFogDistance = FloatInterpolation(from.globalFogDistance.GetValue(timeOfDay, sunElevation, moonElevation), to.globalFogDistance.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.GlobalFogSmooth = FloatInterpolation(from.globalFogSmooth.GetValue(timeOfDay, sunElevation, moonElevation), to.globalFogSmooth.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.GlobalFogDensity = FloatInterpolation(from.globalFogDensity.GetValue(timeOfDay, sunElevation, moonElevation), to.globalFogDensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeightFogDistance = FloatInterpolation(from.heightFogDistance.GetValue(timeOfDay, sunElevation, moonElevation), to.heightFogDistance.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeightFogSmooth = FloatInterpolation(from.heightFogSmooth.GetValue(timeOfDay, sunElevation, moonElevation), to.heightFogSmooth.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeightFogDensity = FloatInterpolation(from.heightFogDensity.GetValue(timeOfDay, sunElevation, moonElevation), to.heightFogDensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeightFogStart = FloatInterpolation(from.heightFogStart.GetValue(timeOfDay, sunElevation, moonElevation), to.heightFogStart.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeightFogEnd = FloatInterpolation(from.heightFogEnd.GetValue(timeOfDay, sunElevation, moonElevation), to.heightFogEnd.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudInterpolator = globalWeatherTransitionProgress;
		staticCloudSource = currentProfile.staticCloudTexture;
		staticCloudTarget = targetProfile.staticCloudTexture;
		settings.StaticCloudLayer1Speed = FloatInterpolation(from.staticCloudLayer1Speed.GetValue(timeOfDay, sunElevation, moonElevation), to.staticCloudLayer1Speed.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudLayer2Speed = FloatInterpolation(from.staticCloudLayer2Speed.GetValue(timeOfDay, sunElevation, moonElevation), to.staticCloudLayer2Speed.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudColor = ColorInterpolation(from.staticCloudColor.GetValue(timeOfDay, sunElevation, moonElevation), to.staticCloudColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudScattering = FloatInterpolation(from.staticCloudScattering.GetValue(timeOfDay, sunElevation, moonElevation), to.staticCloudScattering.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudExtinction = FloatInterpolation(from.staticCloudExtinction.GetValue(timeOfDay, sunElevation, moonElevation), to.staticCloudExtinction.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudSaturation = FloatInterpolation(from.staticCloudSaturation.GetValue(timeOfDay, sunElevation, moonElevation), to.staticCloudSaturation.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudOpacity = FloatInterpolation(from.staticCloudOpacity.GetValue(timeOfDay, sunElevation, moonElevation), to.staticCloudOpacity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DynamicCloudAltitude = FloatInterpolation(from.dynamicCloudAltitude.GetValue(timeOfDay, sunElevation, moonElevation), to.dynamicCloudAltitude.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DynamicCloudDirection = FloatInterpolation(from.dynamicCloudDirection.GetValue(timeOfDay, sunElevation, moonElevation), to.dynamicCloudDirection.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DynamicCloudSpeed = FloatInterpolation(from.dynamicCloudSpeed.GetValue(timeOfDay, sunElevation, moonElevation), to.dynamicCloudSpeed.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DynamicCloudDensity = FloatInterpolation(from.dynamicCloudDensity.GetValue(timeOfDay, sunElevation, moonElevation), to.dynamicCloudDensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DynamicCloudColor1 = ColorInterpolation(from.dynamicCloudColor1.GetValue(timeOfDay, sunElevation, moonElevation), to.dynamicCloudColor1.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DynamicCloudColor2 = ColorInterpolation(from.dynamicCloudColor2.GetValue(timeOfDay, sunElevation, moonElevation), to.dynamicCloudColor2.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DirectionalLightIntensity = FloatInterpolation(from.directionalLightIntensity.GetValue(timeOfDay, sunElevation, moonElevation), to.directionalLightIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DirectionalLightColor = ColorInterpolation(from.directionalLightColor.GetValue(timeOfDay, sunElevation, moonElevation), to.directionalLightColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.EnvironmentIntensity = FloatInterpolation(from.environmentIntensity.GetValue(timeOfDay, sunElevation, moonElevation), to.environmentIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.EnvironmentAmbientColor = ColorInterpolation(from.environmentAmbientColor.GetValue(timeOfDay, sunElevation, moonElevation), to.environmentAmbientColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.EnvironmentEquatorColor = ColorInterpolation(from.environmentEquatorColor.GetValue(timeOfDay, sunElevation, moonElevation), to.environmentEquatorColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.EnvironmentGroundColor = ColorInterpolation(from.environmentGroundColor.GetValue(timeOfDay, sunElevation, moonElevation), to.environmentGroundColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.LightRainIntensity = FloatInterpolation(from.lightRainIntensity.GetValue(timeOfDay, sunElevation, moonElevation), to.lightRainIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MediumRainIntensity = FloatInterpolation(from.mediumRainIntensity.GetValue(timeOfDay, sunElevation, moonElevation), to.mediumRainIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeavyRainIntensity = FloatInterpolation(from.heavyRainIntensity.GetValue(timeOfDay, sunElevation, moonElevation), to.heavyRainIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.SnowIntensity = FloatInterpolation(from.snowIntensity.GetValue(timeOfDay, sunElevation, moonElevation), to.snowIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.RainColor = ColorInterpolation(from.rainColor.GetValue(timeOfDay, sunElevation, moonElevation), to.rainColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.SnowColor = ColorInterpolation(from.snowColor.GetValue(timeOfDay, sunElevation, moonElevation), to.snowColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.LightRainSoundVolume = FloatInterpolation(from.lightRainSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), to.lightRainSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MediumRainSoundVolume = FloatInterpolation(from.mediumRainSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), to.mediumRainSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeavyRainSoundVolume = FloatInterpolation(from.heavyRainSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), to.heavyRainSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.LightWindSoundVolume = FloatInterpolation(from.lightWindSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), to.lightWindSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MediumWindSoundVolume = FloatInterpolation(from.mediumWindSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), to.mediumWindSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeavyWindSoundVolume = FloatInterpolation(from.heavyWindSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), to.heavyWindSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.WindSpeed = FloatInterpolation(from.windSpeed.GetValue(timeOfDay, sunElevation, moonElevation), to.windSpeed.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.WindDirection = FloatInterpolation(from.windDirection.GetValue(timeOfDay, sunElevation, moonElevation), to.windDirection.GetValue(timeOfDay, sunElevation, moonElevation), t);
		if (!outputProfile || !from.outputProfile || !to.outputProfile || outputProfile != from.outputProfile || outputProfile != to.outputProfile || outputProfile.outputList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < outputProfile.outputList.Count; i++)
		{
			m_outputType = outputProfile.outputList[i].type;
			if (m_outputType == AzureOutputType.Slider || m_outputType == AzureOutputType.TimelineCurve || m_outputType == AzureOutputType.SunCurve || m_outputType == AzureOutputType.MoonCurve)
			{
				outputProfile.outputList[i].floatOutput = FloatInterpolation(from.outputPropertyList[i].GetFloatValue(timeOfDay, sunElevation, moonElevation), to.outputPropertyList[i].GetFloatValue(timeOfDay, sunElevation, moonElevation), t);
			}
			else
			{
				outputProfile.outputList[i].colorOutput = ColorInterpolation(from.outputPropertyList[i].GetColorValue(timeOfDay, sunElevation, moonElevation), to.outputPropertyList[i].GetColorValue(timeOfDay, sunElevation, moonElevation), t);
			}
		}
	}

	private void ApplyWeatherZonesInfluence(AzureSkyProfile climateZoneProfile, float t)
	{
		settings.MolecularDensity = FloatInterpolation(settings.MolecularDensity, climateZoneProfile.molecularDensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Wavelength.x = FloatInterpolation(settings.Wavelength.x, climateZoneProfile.wavelengthR.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Wavelength.y = FloatInterpolation(settings.Wavelength.y, climateZoneProfile.wavelengthG.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Wavelength.z = FloatInterpolation(settings.Wavelength.z, climateZoneProfile.wavelengthB.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Rayleigh = FloatInterpolation(settings.Rayleigh, climateZoneProfile.rayleigh.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Mie = FloatInterpolation(settings.Mie, climateZoneProfile.mie.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Scattering = FloatInterpolation(settings.Scattering, climateZoneProfile.scattering.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Luminance = FloatInterpolation(settings.Luminance, climateZoneProfile.luminance.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.Exposure = FloatInterpolation(settings.Exposure, climateZoneProfile.exposure.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.RayleighColor = ColorInterpolation(settings.RayleighColor, climateZoneProfile.rayleighColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MieColor = ColorInterpolation(settings.MieColor, climateZoneProfile.mieColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.ScatteringColor = ColorInterpolation(settings.ScatteringColor, climateZoneProfile.scatteringColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.SunTextureSize = FloatInterpolation(settings.SunTextureSize, climateZoneProfile.sunTextureSize.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.SunTextureIntensity = FloatInterpolation(settings.SunTextureIntensity, climateZoneProfile.sunTextureIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.SunTextureColor = ColorInterpolation(settings.SunTextureColor, climateZoneProfile.sunTextureColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MoonTextureSize = FloatInterpolation(settings.MoonTextureSize, climateZoneProfile.moonTextureSize.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MoonTextureIntensity = FloatInterpolation(settings.MoonTextureIntensity, climateZoneProfile.moonTextureIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MoonTextureColor = ColorInterpolation(settings.MoonTextureColor, climateZoneProfile.moonTextureColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StarsIntensity = FloatInterpolation(settings.StarsIntensity, climateZoneProfile.starsIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MilkyWayIntensity = FloatInterpolation(settings.MilkyWayIntensity, climateZoneProfile.milkyWayIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.FogScatteringScale = FloatInterpolation(settings.FogScatteringScale, climateZoneProfile.fogScatteringScale.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.GlobalFogDistance = FloatInterpolation(settings.GlobalFogDistance, climateZoneProfile.globalFogDistance.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.GlobalFogSmooth = FloatInterpolation(settings.GlobalFogSmooth, climateZoneProfile.globalFogSmooth.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.GlobalFogDensity = FloatInterpolation(settings.GlobalFogDensity, climateZoneProfile.globalFogDensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeightFogDistance = FloatInterpolation(settings.HeightFogDistance, climateZoneProfile.heightFogDistance.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeightFogSmooth = FloatInterpolation(settings.HeightFogSmooth, climateZoneProfile.heightFogSmooth.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeightFogDensity = FloatInterpolation(settings.HeightFogDensity, climateZoneProfile.heightFogDensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeightFogStart = FloatInterpolation(settings.HeightFogStart, climateZoneProfile.heightFogStart.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeightFogEnd = FloatInterpolation(settings.HeightFogEnd, climateZoneProfile.heightFogEnd.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudInterpolator = t;
		staticCloudSource = currentProfile.staticCloudTexture;
		staticCloudTarget = climateZoneProfile.staticCloudTexture;
		settings.StaticCloudLayer1Speed = FloatInterpolation(settings.StaticCloudLayer1Speed, climateZoneProfile.staticCloudLayer1Speed.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudLayer2Speed = FloatInterpolation(settings.StaticCloudLayer2Speed, climateZoneProfile.staticCloudLayer2Speed.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudColor = ColorInterpolation(settings.StaticCloudColor, climateZoneProfile.staticCloudColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudScattering = FloatInterpolation(settings.StaticCloudScattering, climateZoneProfile.staticCloudScattering.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudExtinction = FloatInterpolation(settings.StaticCloudExtinction, climateZoneProfile.staticCloudExtinction.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudSaturation = FloatInterpolation(settings.StaticCloudSaturation, climateZoneProfile.staticCloudSaturation.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.StaticCloudOpacity = FloatInterpolation(settings.StaticCloudOpacity, climateZoneProfile.staticCloudOpacity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DynamicCloudAltitude = FloatInterpolation(settings.DynamicCloudAltitude, climateZoneProfile.dynamicCloudAltitude.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DynamicCloudDirection = FloatInterpolation(settings.DynamicCloudDirection, climateZoneProfile.dynamicCloudDirection.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DynamicCloudSpeed = FloatInterpolation(settings.DynamicCloudSpeed, climateZoneProfile.dynamicCloudSpeed.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DynamicCloudDensity = FloatInterpolation(settings.DynamicCloudDensity, climateZoneProfile.dynamicCloudDensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DynamicCloudColor1 = ColorInterpolation(settings.DynamicCloudColor1, climateZoneProfile.dynamicCloudColor1.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DynamicCloudColor2 = ColorInterpolation(settings.DynamicCloudColor2, climateZoneProfile.dynamicCloudColor2.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DirectionalLightIntensity = FloatInterpolation(settings.DirectionalLightIntensity, climateZoneProfile.directionalLightIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.DirectionalLightColor = ColorInterpolation(settings.DirectionalLightColor, climateZoneProfile.directionalLightColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.EnvironmentIntensity = FloatInterpolation(settings.EnvironmentIntensity, climateZoneProfile.environmentIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.EnvironmentAmbientColor = ColorInterpolation(settings.EnvironmentAmbientColor, climateZoneProfile.environmentAmbientColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.EnvironmentEquatorColor = ColorInterpolation(settings.EnvironmentEquatorColor, climateZoneProfile.environmentEquatorColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.EnvironmentGroundColor = ColorInterpolation(settings.EnvironmentGroundColor, climateZoneProfile.environmentGroundColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.LightRainIntensity = FloatInterpolation(settings.LightRainIntensity, climateZoneProfile.lightRainIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MediumRainIntensity = FloatInterpolation(settings.MediumRainIntensity, climateZoneProfile.mediumRainIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeavyRainIntensity = FloatInterpolation(settings.HeavyRainIntensity, climateZoneProfile.heavyRainIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.SnowIntensity = FloatInterpolation(settings.SnowIntensity, climateZoneProfile.snowIntensity.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.RainColor = ColorInterpolation(settings.RainColor, climateZoneProfile.rainColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.SnowColor = ColorInterpolation(settings.SnowColor, climateZoneProfile.snowColor.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.LightRainSoundVolume = FloatInterpolation(settings.LightRainSoundVolume, climateZoneProfile.lightRainSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MediumRainSoundVolume = FloatInterpolation(settings.MediumRainSoundVolume, climateZoneProfile.mediumRainSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeavyRainSoundVolume = FloatInterpolation(settings.HeavyRainSoundVolume, climateZoneProfile.heavyRainSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.LightWindSoundVolume = FloatInterpolation(settings.LightWindSoundVolume, climateZoneProfile.lightWindSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.MediumWindSoundVolume = FloatInterpolation(settings.MediumWindSoundVolume, climateZoneProfile.mediumWindSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.HeavyWindSoundVolume = FloatInterpolation(settings.HeavyWindSoundVolume, climateZoneProfile.heavyWindSoundVolume.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.WindSpeed = FloatInterpolation(settings.WindSpeed, climateZoneProfile.windSpeed.GetValue(timeOfDay, sunElevation, moonElevation), t);
		settings.WindDirection = FloatInterpolation(settings.WindDirection, climateZoneProfile.windDirection.GetValue(timeOfDay, sunElevation, moonElevation), t);
		if (!outputProfile || !climateZoneProfile.outputProfile || outputProfile != climateZoneProfile.outputProfile || outputProfile.outputList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < outputProfile.outputList.Count; i++)
		{
			m_outputType = outputProfile.outputList[i].type;
			if (m_outputType == AzureOutputType.Slider || m_outputType == AzureOutputType.TimelineCurve || m_outputType == AzureOutputType.SunCurve || m_outputType == AzureOutputType.MoonCurve)
			{
				outputProfile.outputList[i].floatOutput = FloatInterpolation(outputProfile.outputList[i].floatOutput, climateZoneProfile.outputPropertyList[i].GetFloatValue(timeOfDay, sunElevation, moonElevation), t);
			}
			else
			{
				outputProfile.outputList[i].colorOutput = ColorInterpolation(outputProfile.outputList[i].colorOutput, climateZoneProfile.outputPropertyList[i].GetColorValue(timeOfDay, sunElevation, moonElevation), t);
			}
		}
	}

	private float FloatInterpolation(float from, float to, float t)
	{
		return from + (to - from) * t;
	}

	private Vector2 Vector2Interpolation(Vector2 from, Vector2 to, float t)
	{
		Vector2 result = default(Vector2);
		result.x = from.x + (to.x - from.x) * t;
		result.y = from.y + (to.y - from.y) * t;
		return result;
	}

	private Vector3 Vector3Interpolation(Vector3 from, Vector3 to, float t)
	{
		Vector3 result = default(Vector3);
		result.x = from.x + (to.x - from.x) * t;
		result.y = from.y + (to.y - from.y) * t;
		result.z = from.z + (to.z - from.z) * t;
		return result;
	}

	private Color ColorInterpolation(Color from, Color to, float t)
	{
		Color result = default(Color);
		result.r = from.r + (to.r - from.r) * t;
		result.g = from.g + (to.g - from.g) * t;
		result.b = from.b + (to.b - from.b) * t;
		result.a = from.a + (to.a - from.a) * t;
		return result;
	}

	private Vector2 ComputeCloudPosition()
	{
		float x = m_dynamicCloudDirection.x;
		float y = m_dynamicCloudDirection.y;
		float num = settings.DynamicCloudSpeed * 0.05f * Time.deltaTime;
		x += num * Mathf.Sin(0.01745329f * settings.DynamicCloudDirection);
		y += num * Mathf.Cos(0.01745329f * settings.DynamicCloudDirection);
		if (x >= 1f)
		{
			x -= 1f;
		}
		if (y >= 1f)
		{
			y -= 1f;
		}
		return new Vector2(x, y);
	}

	private Vector3 ComputeRayleigh()
	{
		Vector3 one = Vector3.one;
		Vector3 vector = settings.Wavelength * 1E-09f;
		float num = 0.035f;
		float num2 = 1.0003f * 1.0003f;
		float molecularDensity = settings.MolecularDensity;
		float num3 = 248.05023f * ((num2 - 1f) * (num2 - 1f)) / (3f * molecularDensity * 1E+25f) * ((6f + 3f * num) / (6f - 7f * num));
		one.x = num3 / Mathf.Pow(vector.x, 4f);
		one.y = num3 / Mathf.Pow(vector.y, 4f);
		one.z = num3 / Mathf.Pow(vector.z, 4f);
		return one;
	}

	private Vector3 ComputeMie()
	{
		float num = 2.6209998E-08f;
		Vector3 vector = new Vector3(686f, 678f, 682f);
		Vector3 result = default(Vector3);
		result.x = 434f * num * (float)Math.PI * Mathf.Pow((float)Math.PI * 4f / settings.Wavelength.x, 2f) * vector.x;
		result.y = 434f * num * (float)Math.PI * Mathf.Pow((float)Math.PI * 4f / settings.Wavelength.y, 2f) * vector.y;
		result.z = 434f * num * (float)Math.PI * Mathf.Pow((float)Math.PI * 4f / settings.Wavelength.z, 2f) * vector.z;
		return result;
	}
}
