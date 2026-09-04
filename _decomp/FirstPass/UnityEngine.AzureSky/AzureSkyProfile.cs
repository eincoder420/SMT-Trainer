using System.Collections.Generic;

namespace UnityEngine.AzureSky;

[CreateAssetMenu(fileName = "Sky Profile", menuName = "Azure[Sky] Dynamic Skybox/New Sky Profile", order = 1)]
public sealed class AzureSkyProfile : ScriptableObject
{
	public AzureOutputProfile outputProfile;

	public List<AzureOutputProperty> outputPropertyList = new List<AzureOutputProperty>();

	public AzureFloatProperty molecularDensity = new AzureFloatProperty(2.545f, AnimationCurve.Linear(0f, 2.545f, 24f, 2.545f), AnimationCurve.Linear(-1f, 2.545f, 1f, 2.545f), AnimationCurve.Linear(-1f, 2.545f, 1f, 2.545f));

	public AzureFloatProperty wavelengthR = new AzureFloatProperty(680f, AnimationCurve.Linear(0f, 680f, 24f, 680f), AnimationCurve.Linear(-1f, 680f, 1f, 680f), AnimationCurve.Linear(-1f, 680f, 1f, 680f));

	public AzureFloatProperty wavelengthG = new AzureFloatProperty(550f, AnimationCurve.Linear(0f, 550f, 24f, 550f), AnimationCurve.Linear(-1f, 550f, 1f, 550f), AnimationCurve.Linear(-1f, 550f, 1f, 550f));

	public AzureFloatProperty wavelengthB = new AzureFloatProperty(450f, AnimationCurve.Linear(0f, 450f, 24f, 450f), AnimationCurve.Linear(-1f, 450f, 1f, 450f), AnimationCurve.Linear(-1f, 450f, 1f, 450f));

	public AzureFloatProperty rayleigh = new AzureFloatProperty(1.5f, AnimationCurve.Linear(0f, 1.5f, 24f, 1.5f), AnimationCurve.Linear(-1f, 1.5f, 1f, 1.5f), AnimationCurve.Linear(-1f, 1.5f, 1f, 1.5f));

	public AzureFloatProperty mie = new AzureFloatProperty(1f, AnimationCurve.Linear(0f, 1f, 24f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f));

	public AzureFloatProperty scattering = new AzureFloatProperty(0.25f, AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f), AnimationCurve.Linear(-1f, 0.25f, 1f, 0.25f), AnimationCurve.Linear(-1f, 0.25f, 1f, 0.25f));

	public AzureFloatProperty luminance = new AzureFloatProperty(1.5f, AnimationCurve.Linear(0f, 1.5f, 24f, 1.5f), AnimationCurve.Linear(-1f, 1.5f, 1f, 1.5f), AnimationCurve.Linear(-1f, 1.5f, 1f, 1.5f));

	public AzureFloatProperty exposure = new AzureFloatProperty(2f, AnimationCurve.Linear(0f, 2f, 24f, 2f), AnimationCurve.Linear(-1f, 2f, 1f, 2f), AnimationCurve.Linear(-1f, 2f, 1f, 2f));

	public AzureColorProperty rayleighColor = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureColorProperty mieColor = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureColorProperty scatteringColor = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureFloatProperty sunTextureSize = new AzureFloatProperty(2.5f, AnimationCurve.Linear(0f, 2.5f, 24f, 2.5f), AnimationCurve.Linear(-1f, 2.5f, 1f, 2.5f), AnimationCurve.Linear(-1f, 2.5f, 1f, 2.5f));

	public AzureFloatProperty sunTextureIntensity = new AzureFloatProperty(1f, AnimationCurve.Linear(0f, 1f, 24f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f));

	public AzureColorProperty sunTextureColor = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureFloatProperty moonTextureSize = new AzureFloatProperty(10f, AnimationCurve.Linear(0f, 10f, 24f, 10f), AnimationCurve.Linear(-1f, 10f, 1f, 10f), AnimationCurve.Linear(-1f, 10f, 1f, 10f));

	public AzureFloatProperty moonTextureIntensity = new AzureFloatProperty(1f, AnimationCurve.Linear(0f, 1f, 24f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f));

	public AzureColorProperty moonTextureColor = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureFloatProperty starsIntensity = new AzureFloatProperty(0.5f, AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f), AnimationCurve.Linear(-1f, 0.5f, 1f, 0.5f), AnimationCurve.Linear(-1f, 0.5f, 1f, 0.5f));

	public AzureFloatProperty milkyWayIntensity = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty fogScatteringScale = new AzureFloatProperty(1f, AnimationCurve.Linear(0f, 1f, 24f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f));

	public AzureFloatProperty globalFogDistance = new AzureFloatProperty(1000f, AnimationCurve.Linear(0f, 1000f, 24f, 1000f), AnimationCurve.Linear(-1f, 1000f, 1f, 1000f), AnimationCurve.Linear(-1f, 1000f, 1f, 1000f));

	public AzureFloatProperty globalFogSmooth = new AzureFloatProperty(0.25f, AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f), AnimationCurve.Linear(-1f, 0.25f, 1f, 0.25f), AnimationCurve.Linear(-1f, 0.25f, 1f, 0.25f));

	public AzureFloatProperty globalFogDensity = new AzureFloatProperty(1f, AnimationCurve.Linear(0f, 1f, 24f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f));

	public AzureFloatProperty heightFogDistance = new AzureFloatProperty(100f, AnimationCurve.Linear(0f, 100f, 24f, 100f), AnimationCurve.Linear(-1f, 100f, 1f, 100f), AnimationCurve.Linear(-1f, 100f, 1f, 100f));

	public AzureFloatProperty heightFogSmooth = new AzureFloatProperty(1f, AnimationCurve.Linear(0f, 1f, 24f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f));

	public AzureFloatProperty heightFogDensity = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty heightFogStart = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty heightFogEnd = new AzureFloatProperty(100f, AnimationCurve.Linear(0f, 100f, 24f, 100f), AnimationCurve.Linear(-1f, 100f, 1f, 100f), AnimationCurve.Linear(-1f, 100f, 1f, 100f));

	public Texture2D staticCloudTexture;

	public AzureFloatProperty staticCloudLayer1Speed = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty staticCloudLayer2Speed = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty staticCloudScattering = new AzureFloatProperty(1f, AnimationCurve.Linear(0f, 1f, 24f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f));

	public AzureFloatProperty staticCloudExtinction = new AzureFloatProperty(1.5f, AnimationCurve.Linear(0f, 1.5f, 24f, 1.5f), AnimationCurve.Linear(-1f, 1.5f, 1f, 1.5f), AnimationCurve.Linear(-1f, 1.5f, 1f, 1.5f));

	public AzureFloatProperty staticCloudSaturation = new AzureFloatProperty(2.5f, AnimationCurve.Linear(0f, 2.5f, 24f, 2.5f), AnimationCurve.Linear(-1f, 2.5f, 1f, 2.5f), AnimationCurve.Linear(-1f, 2.5f, 1f, 2.5f));

	public AzureFloatProperty staticCloudOpacity = new AzureFloatProperty(1.25f, AnimationCurve.Linear(0f, 1.25f, 24f, 1.25f), AnimationCurve.Linear(-1f, 1.25f, 1f, 1.25f), AnimationCurve.Linear(-1f, 1.25f, 1f, 1.25f));

	public AzureColorProperty staticCloudColor = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureFloatProperty dynamicCloudAltitude = new AzureFloatProperty(7.5f, AnimationCurve.Linear(0f, 7.5f, 24f, 7.5f), AnimationCurve.Linear(-1f, 7.5f, 1f, 7.5f), AnimationCurve.Linear(-1f, 7.5f, 1f, 7.5f));

	public AzureFloatProperty dynamicCloudDirection = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty dynamicCloudSpeed = new AzureFloatProperty(0.1f, AnimationCurve.Linear(0f, 0.1f, 24f, 0.1f), AnimationCurve.Linear(-1f, 0.1f, 1f, 0.1f), AnimationCurve.Linear(-1f, 0.1f, 1f, 0.1f));

	public AzureFloatProperty dynamicCloudDensity = new AzureFloatProperty(0.75f, AnimationCurve.Linear(0f, 0.75f, 24f, 0.75f), AnimationCurve.Linear(-1f, 0.75f, 1f, 0.75f), AnimationCurve.Linear(-1f, 0.75f, 1f, 0.75f));

	public AzureColorProperty dynamicCloudColor1 = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureColorProperty dynamicCloudColor2 = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureFloatProperty directionalLightIntensity = new AzureFloatProperty(1f, AnimationCurve.Linear(0f, 1f, 24f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f));

	public AzureColorProperty directionalLightColor = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureFloatProperty environmentIntensity = new AzureFloatProperty(1f, AnimationCurve.Linear(0f, 1f, 24f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f), AnimationCurve.Linear(-1f, 1f, 1f, 1f));

	public AzureColorProperty environmentAmbientColor = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureColorProperty environmentEquatorColor = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureColorProperty environmentGroundColor = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureFloatProperty lightRainIntensity = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty mediumRainIntensity = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty heavyRainIntensity = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty snowIntensity = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureColorProperty rainColor = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureColorProperty snowColor = new AzureColorProperty(Color.white, new Gradient(), new Gradient(), new Gradient());

	public AzureFloatProperty lightRainSoundVolume = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty mediumRainSoundVolume = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty heavyRainSoundVolume = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty lightWindSoundVolume = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty mediumWindSoundVolume = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty heavyWindSoundVolume = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty windSpeed = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));

	public AzureFloatProperty windDirection = new AzureFloatProperty(0f, AnimationCurve.Linear(0f, 0f, 24f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f), AnimationCurve.Linear(-1f, 0f, 1f, 0f));
}
