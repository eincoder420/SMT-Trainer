using System.Collections.Generic;

namespace UnityEngine.AzureSky;

[ExecuteInEditMode]
[AddComponentMenu("Azure[Sky]/Azure Effects Controller")]
public class AzureEffectsController : MonoBehaviour
{
	private AzureSkyController m_skyController;

	public WindZone windZone;

	public float windMultiplier = 1f;

	private Vector3 m_windDirection = Vector3.forward;

	public AudioSource lightRainSoundFx;

	public AudioSource mediumRainSoundFx;

	public AudioSource heavyRainSoundFx;

	public AudioSource lightWindSoundFx;

	public AudioSource mediumWindSoundFx;

	public AudioSource heavyWindSoundFx;

	public Transform particleSystemTransform;

	public Material rainMaterial;

	public Material heavyRainMaterial;

	public Material snowMaterial;

	public Material rippleMaterial;

	public ParticleSystem lightRainParticle;

	public ParticleSystem mediumRainParticle;

	public ParticleSystem heavyRainParticle;

	public ParticleSystem snowParticle;

	public Transform followTarget;

	public List<AzureThunderSettings> thunderSettingsList = new List<AzureThunderSettings>();

	private void OnEnable()
	{
		m_skyController = GetComponent<AzureSkyController>();
	}

	private void Start()
	{
		m_skyController = GetComponent<AzureSkyController>();
		UpdateParticlesMaterials();
		UpdateParticlesPosition();
	}

	private void Update()
	{
		UpdateParticlesMaterials();
		UpdateParticlesPosition();
		if (Application.isPlaying)
		{
			SoundEffectController(m_skyController.settings.LightRainSoundVolume, lightRainSoundFx);
			SoundEffectController(m_skyController.settings.MediumRainSoundVolume, mediumRainSoundFx);
			SoundEffectController(m_skyController.settings.HeavyRainSoundVolume, heavyRainSoundFx);
			SoundEffectController(m_skyController.settings.LightWindSoundVolume, lightWindSoundFx);
			SoundEffectController(m_skyController.settings.MediumWindSoundVolume, mediumWindSoundFx);
			SoundEffectController(m_skyController.settings.HeavyWindSoundVolume, heavyWindSoundFx);
			ParticleEffectController(m_skyController.settings.LightRainIntensity * 4000f, lightRainParticle);
			ParticleEffectController(m_skyController.settings.MediumRainIntensity * 4000f, mediumRainParticle);
			ParticleEffectController(m_skyController.settings.HeavyRainIntensity * 2000f, heavyRainParticle);
			ParticleEffectController(m_skyController.settings.SnowIntensity * 2000f, snowParticle);
			windZone.windMain = m_skyController.settings.WindSpeed * windMultiplier;
			m_windDirection = new Vector3(0f, m_skyController.settings.WindDirection + 180f, 0f);
			windZone.transform.rotation = Quaternion.Euler(m_windDirection);
		}
	}

	private void SoundEffectController(float volume, AudioSource sound)
	{
		sound.volume = volume;
		if (volume > 0f)
		{
			if (!sound.isPlaying)
			{
				sound.Play();
			}
		}
		else if (sound.isPlaying)
		{
			sound.Stop();
		}
	}

	private void ParticleEffectController(float intensity, ParticleSystem particle)
	{
		ParticleSystem.EmissionModule emission = particle.emission;
		emission.rateOverTimeMultiplier = intensity;
		if (intensity > 0f)
		{
			if (!particle.isPlaying)
			{
				particle.Play();
			}
		}
		else if (particle.isPlaying)
		{
			particle.Stop();
		}
	}

	private void UpdateParticlesPosition()
	{
		if ((bool)followTarget)
		{
			particleSystemTransform.position = followTarget.position;
		}
	}

	private void UpdateParticlesMaterials()
	{
		rainMaterial.SetColor("_TintColor", m_skyController.settings.RainColor);
		heavyRainMaterial.SetColor("_TintColor", m_skyController.settings.RainColor);
		snowMaterial.SetColor("_TintColor", m_skyController.settings.SnowColor);
		rippleMaterial.SetColor("_TintColor", m_skyController.settings.RainColor);
	}

	public void InstantiateThunderEffect(int index)
	{
		AzureThunderEffect component = Object.Instantiate(thunderSettingsList[index].thunderPrefab, thunderSettingsList[index].position, thunderSettingsList[index].thunderPrefab.rotation).GetComponent<AzureThunderEffect>();
		component.audioClip = thunderSettingsList[index].audioClip;
		component.audioDelay = thunderSettingsList[index].audioDelay;
		component.lightFrequency = thunderSettingsList[index].lightFrequency;
	}

	public void InstantiateThunderEffect(int index, Vector3 worldPos)
	{
		AzureThunderEffect component = Object.Instantiate(thunderSettingsList[index].thunderPrefab, worldPos, thunderSettingsList[index].thunderPrefab.rotation).GetComponent<AzureThunderEffect>();
		component.audioClip = thunderSettingsList[index].audioClip;
		component.audioDelay = thunderSettingsList[index].audioDelay;
		component.lightFrequency = thunderSettingsList[index].lightFrequency;
	}
}
